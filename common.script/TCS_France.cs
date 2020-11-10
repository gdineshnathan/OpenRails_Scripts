// COPYRIGHT 2020 by the Open Rails project.
// 
// This file is part of Open Rails.
// 
// Open Rails is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Open Rails is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Open Rails.  If not, see <http://www.gnu.org/licenses/>.

using Orts.Common;
using Orts.Simulation;
using ORTS.Common;
using ORTS.Scripting.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ORTS.Scripting.Script
{
    public class TCS_France : TrainControlSystem
    {
        // Helper functions
        public static T Min<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        public static T Max<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        // Cabview control number
        const int BP_AC_SF = 0;
        const int BP_A_LS_SF = 2;
        const int Z_ES_VA = 3;
        const int KVB_BP_VAL = 5;
        const int KVB_BP_MV = 6;
        const int KVB_BP_FC = 7;
        const int KVB_BP_TEST = 8;
        const int BP_AM_V1 = 9;
        const int BP_AM_V2 = 10;
        const int BP_DM = 11;
        const int TVM_Mask = 20;
        const int VY_CO_URG = 21;
        const int VY_CO_Z = 22;
        const int VY_CV = 23;
        const int VY_SECT = 24;
        const int VY_SECT_AU = 25;
        const int VY_BPT = 26;
        const int TVM_VL = 27;
        const int TVM_Ex1 = 28;
        const int TVM_Ex2 = 29;
        const int TVM_An1 = 30;
        const int TVM_An2 = 31;
        const int LS_SF = 32;
        const int VY_SOS_RSO = 33;
        const int VY_SOS_VAC = 34;
        const int VY_ES_FU = 35;
        const int VY_SOS_KVB = 36;
        const int VY_VTE = 37;
        const int VY_FU = 38;
        const int VY_PE = 39;
        const int VY_PS = 40;
        const int KVB_Principal1 = 41;
        const int KVB_Principal2 = 42;
        const int KVB_Auxiliary1 = 43;
        const int KVB_Auxiliary2 = 44;
        const int KVB_VY_BP_VAL = 45;
        const int KVB_VY_BP_MV = 46;
        const int KVB_VY_BP_FC = 47;

        public enum SignalType
        {
            Normal,
            Distant,
            Info,
            Repeater,
            Speed,
        }

        public enum ETCSLevel
        {
            L0,         // Unfitted (national system active)
            NTC,        // Specific Transmission Module (national system information transmitted to ETCS)
            L1,         // Level 1 : Beacon transmission, loop transmission and radio in-fill
            L2,         // Level 2 : Radio transmission, beacon positionning
            L3          // Level 3 : Same as level 2 + moving block
        }

        public enum TrainCategoryType
        {
            AU,
            VO,
            ME,
            MA
        }

        public enum RSOStateType
        {
            Init,
            Off,
            TriggeredPressed,
            TriggeredBlinking,
            TriggeredFixed
        }

        public enum KVBVersionType
        {
            V5,
            V6,
            V7
        }

        public enum KVBSignalFieldType
        {
            Unknown,
            C_BAL,
            S_BAL,
            S_BM,
            A,
            ACLI,
            VL_INF,
            VLCLI,
            VL_SUP,
        }

        public enum KVBSignalExecutionSpeedType
        {
            None,
            A,
            B,
            C,
            VM,
            VMb
        }

        public enum KVBSignalTargetSpeedType
        {
            None,
            V0,
            V160,
            VM,
            VMb
        }

        public enum KVBSpeedPostSpeedCategory
        {
            LTV,
            G,
            GS1,
            GS3,
            GS123
        }

        public enum KVBSpeedPostSpeedType
        {
            None,
            V0,
            V5,
            V10,
            V15,
            V20,
            V25,
            V30,
            V35,
            V40,
            V45,
            V50,
            V55,
            V60,
            V65,
            V70,
            V75,
            V80,
            V85,
            V90,
            V95,
            V100,
            V105,
            V110,
            V115,
            V120,
            V125,
            V130,
            V135,
            V140,
            V150,
            V160,
            V170,
            V180,
            V190,
            V200,
            V210,
            V220,
            V230,
            P,
            AA
        }

        public enum KVBStateType
        {
            Normal,
            Alert,
            Emergency
        }

        public enum KVBPreAnnounceType
        {
            Deactivated,
            Triggered,
            Armed
        }

        public enum KVBModeType
        {
            Off,
            Init,
            ConventionalLine,
            HighSpeedLine,
            Shunting
        }

        public enum KVBReleaseSpeed
        {
            V30,
            V10
        }

        public enum KVBPrincipalDisplayStateType
        {
            Empty,
            FU,
            V000,
            V00,
            L,
            b,
            p,
            Dashes3,
            Dashes9,
            Test,
            VersionPA,
            VersionUC
        }

        public enum KVBAuxiliaryDisplayStateType
        {
            Empty,
            V000,
            V00,
            L,
            p,
            Dashes3,
            Test,
            PA,
            UC
        }

        public enum QBalType
        {
            LC,
            LGV,
        }

        public enum TVMModelType
        {
            None,
            TVM300,
            TVM430_V300,
            TVM430_V320
        }

        public enum TVMModeType
        {
            None,
            TVM300,
            TVM430,
        }

        public enum TVMSpeedType
        {
            _RRR,
            _000,
            _30E,
            _30,
            _60E,
            _60,
            _80E,
            _80,
            _100E,
            _100,
            _130E,
            _130,
            _160E,
            _160,
            _170E,
            _170,
            _200V,
            _200,
            _220E,
            _220V,
            _220,
            _230E,
            _230V,
            _230,
            _270V,
            _270,
            _300V,
            _300,
            _320V,
            _320,
            Any
        }

        public enum TVMAspectType
        {
            None,
            _RRR,
            _000,
            _30E,
            _30A,
            _60E,
            _60A,
            _80E,
            _80A,
            _100E,
            _100A,
            _130E,
            _130A,
            _160E,
            _160A,
            _170E,
            _170A,
            _200V,
            _200A,
            _220E,
            _220V,
            _220A,
            _230E,
            _230V,
            _230A,
            _270V,
            _270A,
            _300V,
            _300A,
            _320V
        }

        ETCSLevel CurrentETCSLevel = ETCSLevel.L0;

    // Properties
        bool RearmingButton { get; set; } = false;

    // Train parameters
        bool VACMAPresent;                                  // VACMA
        bool RSOPresent;                                    // RSO
        bool DAATPresent;                                   // DAAT
        bool KVBPresent;                                    // KVB
        bool TVM300Present;                                 // TVM300
        bool TVM430Present;                                 // TVM430
        bool ETCSPresent;                                   // ETCS (Not implemented)
        ETCSLevel ETCSMaxLevel = ETCSLevel.L0;              // ETCS maximum level (Not implemented)
        bool ElectroPneumaticBrake;                         // EP
        bool HeavyFreightTrain;                             // MA train only
        float SafeDecelerationMpS2;                         // Gamma

    // RSO (Répétition Optique des Signaux / Optical Signal Repetition)
        // Parameters
        float RSODelayBeforeEmergencyBrakingS;
        float RSOBlinkerFrequencyHz;

        // Variables
        RSOStateType RSOState = RSOStateType.Init;
        bool RSOEmergencyBraking = true;
        bool RSOPressed = false;
        bool RSOPreviousPressed = false;
        bool RSOCancelPressed = false;
        bool RSOType1Inhibition = false;                    // Inhibition 1 : Reverse
        bool RSOType2Inhibition = false;                    // Inhibition 2 : KVB not inhibited and train on HSL
        bool RSOType3Inhibition = false;                    // Inhibition 3 : TVM COVIT not inhibited
        bool RSOClosedSignal = false;
        bool RSOPreviousClosedSignal = false;
        bool RSOOpenedSignal = false;
        Blinker RSOBlinker;
        Timer RSOEmergencyTimer;

    // DAAT (Dispositif d'Arrêt Automatique des Trains / Automatic Train Stop System)
        // Not implemented

    // KVB (Contrôle de Vitesse par Balises / Beacon speed control)
        // Parameters
        bool KVBInhibited;
        KVBVersionType KVBVersion = KVBVersionType.V5;
        TrainCategoryType KVBTrainCategory = TrainCategoryType.AU;
        bool KVBDataEntryPanel = false;
        const float KVBDelayBeforeEmergencyBrakingS = 5f;   // Tx
        float KVBTrainSpeedLimitMpS;                        // VT
        float KVBTrainLengthM;                              // L
        float KVBDelayBeforeBrakingEstablishedS;            // Tbo
        float KVBSafeDecelerationMpS2 => KVBMode == KVBModeType.Shunting ? 0.45f : SafeDecelerationMpS2;

        // Variables
        bool KVBCLTV = false;
        bool KVBCTABP = false;
        bool KVBGroundFailure = false;
        bool KVBEngineFailure = false;
        bool KVBParametersValidation = false;
        bool KVBOverrideNf = false; // Also used for TVM

        Timer KVBGroundFailureTimer;
        Blinker KVBGroundFailureBlinker;
        Timer KVBInitTimer;
        Timer KVBTestTimer;
        Timer KVBTutTimer;
        Timer KVBBipTimer;

        bool KVBSpadEmergency = false;
        bool KVBOverspeedEmergency = false;
        KVBStateType KVBState = KVBStateType.Emergency;
        bool KVBEmergencyBraking = true;
        KVBModeType KVBMode = KVBModeType.ConventionalLine;
        KVBPrincipalDisplayStateType KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
        bool KVBPrincipalDisplayBlinking = false;
        KVBAuxiliaryDisplayStateType KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;

        Blinker KVBPrincipalDisplayBlinker;
        Blinker KVBVALButtonLightBlinker;
        Blinker KVBFCButtonLightBlinker;
        OdoMeter KVBCLTVOdometer;
        OdoMeter KVBCTABPOdometer;
        OdoMeter KVBTrainLengthOdometer;
        OdoMeter KVBOverrideNfOdometer;
        OdoMeter KVBShuntingOdometer;

        KVBSignalFieldType KVBSignalField = KVBSignalFieldType.Unknown;
        KVBSignalExecutionSpeedType KVBSignalExecutionSpeed = KVBSignalExecutionSpeedType.None;
        KVBSignalTargetSpeedType KVBSignalTargetSpeed = KVBSignalTargetSpeedType.None;
        bool KVBSignalTargetIsBufferStop = false;
        float KVBSignalTargetDistanceM => (KVBSignalTargetOdometer.Started ? KVBSignalTargetOdometer.RemainingValue : float.PositiveInfinity);

        KVBPreAnnounceType KVBPreAnnounce => Min(KVBPreAnnounceVLCLI, KVBPreAnnounceTABP);
        KVBPreAnnounceType KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
        KVBPreAnnounceType KVBPreAnnounceTABP = KVBPreAnnounceType.Deactivated;
        OdoMeter KVBPreAnnounceOdometerVLCLI;
        OdoMeter KVBTrainLengthOdometerVLCLI;

        KVBReleaseSpeed KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
        bool KVBOnSight = false;
        float KVBSignalTargetAlertSpeedMpS = MpS.FromKpH(5f);
        float KVBSignalTargetEBSpeedMpS = MpS.FromKpH(10f);
        float KVBSignalTargetReleaseSpeedMpS = MpS.FromKpH(30f);
        OdoMeter KVBSignalTargetOdometer;

        Dictionary<KVBSpeedPostSpeedCategory, KVBSpeedPostSpeedType> KVBSpeedPostExecutionList = new Dictionary<KVBSpeedPostSpeedCategory, KVBSpeedPostSpeedType>();
        Dictionary<KVBSpeedPostSpeedCategory, (KVBSpeedPostSpeedType Speed, OdoMeter Odometer)> KVBSpeedPostPendingList = new Dictionary<KVBSpeedPostSpeedCategory, (KVBSpeedPostSpeedType Speed, OdoMeter Odometer)>();
        Dictionary<KVBSpeedPostSpeedCategory, (KVBSpeedPostSpeedType Speed, OdoMeter Odometer)> KVBSpeedPostAnnounceList = new Dictionary<KVBSpeedPostSpeedCategory, (KVBSpeedPostSpeedType Speed, OdoMeter Odometer)>();
        OdoMeter KVBTrainLengthOdometerFVL;

        float KVBDeclivity = 0f;                            // i

        bool KVBTrainSpeedLimitAlert = false;
        bool KVBTrainSpeedLimitEmergency = false;
        bool KVBOnSightAlert = false;
        bool KVBOnSightEmergency = false;
        bool KVBSignalTargetSpeedAlert = false;
        bool KVBSignalTargetSpeedEmergency = false;
        bool KVBSignalExecutionSpeedAlert = false;
        bool KVBSignalExecutionSpeedEmergency = false;
        bool KVBSpeedPostExecutionSpeedAlert = false;
        bool KVBSpeedPostExecutionSpeedEmergency = false;
        bool KVBSpeedPostAnnounceSpeedAlert = false;
        bool KVBSpeedPostAnnounceSpeedEmergency = false;
        bool KVBSpeedPostPreAnnounceAlert = false;
        bool KVBSpeedPostPreAnnounceEmergency = false;

        bool KVBSpeedTooHighLight = false;
        bool KVBEmergencyBrakeLight = false;

        bool KVBVALButtonLight = false;
        bool KVBVALButtonLightBlinking = false;
        bool KVBMVButtonLight = false;
        bool KVBFCButtonLight = false;
        bool KVBFCButtonLightBlinking = false;

        // TVM inputs/outputs
        bool ARMCAB = false;
        bool DECAB = true;

        // TVM arming check
        OdoMeter KarmStartOdometer;
        bool KarmEmergencyBraking = false;
        QBalType QBal = QBalType.LC;    // Q-BAL

    // TVM COVIT common
        // Constants
        const int TVMNumberOfBlockSections = 10;

        // Parameters
        TVMModelType TVMModel = TVMModelType.None;
        TVMModeType TVMMode = TVMModeType.None;
        bool TVMCOVITInhibited = false;

        // Variables
        bool TVMArmed = false;
        bool TVMManualArming = false;
        bool TVMManualDearming = false;
        bool TVMCOVITSpadEmergency = false;
        bool TVMCOVITEmergencyBraking = false;

        bool TVMOpenCircuitBreaker = false;
        bool TVMOpenCircuitBreakerAutomatic = false;
        bool TVMLowerPantograph = false;

        int[] SpeedSequence = new int[TVMNumberOfBlockSections];
        Aspect[] AspectSequence = new Aspect[TVMNumberOfBlockSections];
        int PreviousSectionSpeed = 0;
        Aspect PreviousSectionAspect = Aspect.None;
        TVMSpeedType PreviousVcond = TVMSpeedType.Any;

        TVMSpeedType[] Vcond = new TVMSpeedType[TVMNumberOfBlockSections];
        TVMSpeedType[] Ve = new TVMSpeedType[TVMNumberOfBlockSections];
        TVMSpeedType[] Vc = new TVMSpeedType[TVMNumberOfBlockSections];
        TVMSpeedType[] Va = new TVMSpeedType[TVMNumberOfBlockSections];

        TVMAspectType TVMAspectCommand = TVMAspectType.None;
        TVMAspectType TVMAspectCurrent = TVMAspectType.None;
        TVMAspectType TVMAspectPreviousCycle = TVMAspectType.None;
        bool TVMBlinkingCommand = false;
        bool TVMBlinkingCurrent = false;
        bool TVMBlinkingPreviousCycle = false;
        Blinker TVMBlinker;

        float TVMStartControlSpeedMpS = 0f;
        float TVMEndControlSpeedMpS = 0f;
        float TVMDecelerationMpS2 = 0f;

        bool TVMClosedSignal;
        bool TVMPreviousClosedSignal;
        bool TVMOpenedSignal;
        bool TVMPreviousOpenedSignal;

    // TVM300 COVIT (Transmission Voie Machine 300 COntrôle de VITesse / Track Machine Transmission 300 Speed control)
        // Constants
        string TVM300DecodingFileName;
        Dictionary<Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>, Tuple<TVMAspectType, bool, float>> TVM300DecodingTable = new Dictionary<Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>, Tuple<TVMAspectType, bool, float>>();

        Dictionary<TVMAspectType, Aspect> TVM300MstsTranslation = new Dictionary<TVMAspectType, Aspect>
        {
            { TVMAspectType.None, Aspect.None  },
            { TVMAspectType._300V, Aspect.Clear_2 },
            { TVMAspectType._270A, Aspect.Clear_1  },
            { TVMAspectType._270V, Aspect.Approach_3 },
            { TVMAspectType._220A, Aspect.Approach_2 },
            { TVMAspectType._220E, Aspect.Approach_1 },
            { TVMAspectType._160A, Aspect.Restricted },
            { TVMAspectType._160E, Aspect.StopAndProceed },
            { TVMAspectType._80A, Aspect.Restricted },
            { TVMAspectType._80E, Aspect.StopAndProceed },
            { TVMAspectType._000, Aspect.Stop },
            { TVMAspectType._RRR, Aspect.Permission }
        };

        Dictionary<TVMSpeedType, TVMSpeedType> TVM300Tab1 = new Dictionary<TVMSpeedType, TVMSpeedType>
        {
            { TVMSpeedType._RRR,   TVMSpeedType._000 },
            { TVMSpeedType._000,  TVMSpeedType._160 },
            { TVMSpeedType._80E,  TVMSpeedType._80 },
            { TVMSpeedType._80,   TVMSpeedType._160 },
            { TVMSpeedType._160E, TVMSpeedType._160 },
            { TVMSpeedType._160,  TVMSpeedType._220 },
            { TVMSpeedType._220E, TVMSpeedType._220 },
            { TVMSpeedType._220,  TVMSpeedType._270 },
            { TVMSpeedType._270V, TVMSpeedType._270 },
            { TVMSpeedType._270,  TVMSpeedType._300 },
            { TVMSpeedType._300V, TVMSpeedType._300 },
            { TVMSpeedType._300,  TVMSpeedType._000 }
        };
        Dictionary<TVMSpeedType, TVMSpeedType> TVM300Tab2 = new Dictionary<TVMSpeedType, TVMSpeedType>
        {
            { TVMSpeedType._RRR,   TVMSpeedType._000 },
            { TVMSpeedType._000,  TVMSpeedType._000 },
            { TVMSpeedType._80E,  TVMSpeedType._80 },
            { TVMSpeedType._80,   TVMSpeedType._80 },
            { TVMSpeedType._160E, TVMSpeedType._160 },
            { TVMSpeedType._160,  TVMSpeedType._160 },
            { TVMSpeedType._220E, TVMSpeedType._220 },
            { TVMSpeedType._220,  TVMSpeedType._220 },
            { TVMSpeedType._270V, TVMSpeedType._270 },
            { TVMSpeedType._270,  TVMSpeedType._270 },
            { TVMSpeedType._300V, TVMSpeedType._300 },
            { TVMSpeedType._300,  TVMSpeedType._000 }
        };

    // TVM430 COVIT (Transmission Voie Machine 430 COntrôle de VITesse / Track Machine Transmission 430 Speed control)
        // Constants
        // TVM430 300 km/h
        string TVM430DecodingFileName;
        Dictionary<Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>, Tuple<TVMAspectType, bool, float, float, float>> TVM430DecodingTable = new Dictionary<Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>, Tuple<TVMAspectType, bool, float, float, float>>();

        Dictionary<TVMAspectType, Aspect> TVM430S300MstsTranslation = new Dictionary<TVMAspectType, Aspect>
        {
            { TVMAspectType.None, Aspect.None  },
            { TVMAspectType._300V, Aspect.Clear_2 },
            { TVMAspectType._270A, Aspect.Clear_1 },
            { TVMAspectType._270V, Aspect.Clear_1  },
            { TVMAspectType._230A, Aspect.Approach_3 },
            { TVMAspectType._230V, Aspect.Approach_3 },
            { TVMAspectType._230E, Aspect.Approach_3 },
            { TVMAspectType._220A, Aspect.Approach_3 },
            { TVMAspectType._220V, Aspect.Approach_3 },
            { TVMAspectType._220E, Aspect.Approach_3 },
            { TVMAspectType._200A, Aspect.Approach_2 },
            { TVMAspectType._200V, Aspect.Approach_2 },
            { TVMAspectType._170A, Aspect.Approach_2 },
            { TVMAspectType._170E, Aspect.Approach_2 },
            { TVMAspectType._160A, Aspect.Approach_1 },
            { TVMAspectType._160E, Aspect.Approach_1 },
            { TVMAspectType._130A, Aspect.Restricted },
            { TVMAspectType._130E, Aspect.Restricted },
            { TVMAspectType._80A, Aspect.Restricted },
            { TVMAspectType._80E, Aspect.Restricted },
            { TVMAspectType._60A, Aspect.Restricted },
            { TVMAspectType._60E, Aspect.Restricted },
            { TVMAspectType._000, Aspect.Stop },
            { TVMAspectType._RRR, Aspect.Permission }
        };
        Dictionary<TVMAspectType, Aspect> TVM430S320MstsTranslation = new Dictionary<TVMAspectType, Aspect>
        {
            { TVMAspectType.None, Aspect.None  },
            { TVMAspectType._320V, Aspect.Clear_2 },
            { TVMAspectType._300A, Aspect.Clear_1  },
            { TVMAspectType._300V, Aspect.Clear_1  },
            { TVMAspectType._270A, Aspect.Approach_3 },
            { TVMAspectType._270V, Aspect.Approach_3 },
            { TVMAspectType._230A, Aspect.Approach_3 },
            { TVMAspectType._230V, Aspect.Approach_3 },
            { TVMAspectType._230E, Aspect.Approach_3 },
            { TVMAspectType._220A, Aspect.Approach_3 },
            { TVMAspectType._220E, Aspect.Approach_3 },
            { TVMAspectType._200A, Aspect.Approach_2 },
            { TVMAspectType._200V, Aspect.Approach_2 },
            { TVMAspectType._170A, Aspect.Approach_2 },
            { TVMAspectType._170E, Aspect.Approach_2 },
            { TVMAspectType._160A, Aspect.Approach_1 },
            { TVMAspectType._160E, Aspect.Approach_1 },
            { TVMAspectType._130A, Aspect.Restricted },
            { TVMAspectType._130E, Aspect.Restricted },
            { TVMAspectType._80A, Aspect.Restricted },
            { TVMAspectType._80E, Aspect.Restricted },
            { TVMAspectType._60A, Aspect.Restricted },
            { TVMAspectType._60E, Aspect.Restricted },
            { TVMAspectType._000, Aspect.Stop },
            { TVMAspectType._RRR, Aspect.Permission }
        };

        Dictionary<TVMSpeedType, TVMSpeedType> TVM430SncfTab1 = new Dictionary<TVMSpeedType, TVMSpeedType>
        {
            { TVMSpeedType._RRR,   TVMSpeedType._000 },
            { TVMSpeedType._000,  TVMSpeedType._170 },
            { TVMSpeedType._60E,  TVMSpeedType._60 },
            { TVMSpeedType._60,   TVMSpeedType._170 },
            { TVMSpeedType._80E,  TVMSpeedType._80 },
            { TVMSpeedType._80,   TVMSpeedType._170 },
            { TVMSpeedType._130E, TVMSpeedType._130 },
            { TVMSpeedType._130,  TVMSpeedType._200 },
            { TVMSpeedType._160E, TVMSpeedType._160 },
            { TVMSpeedType._160,  TVMSpeedType._230 },
            { TVMSpeedType._170E, TVMSpeedType._170 },
            { TVMSpeedType._170,  TVMSpeedType._230 },
            { TVMSpeedType._200V, TVMSpeedType._200 },
            { TVMSpeedType._200,  TVMSpeedType._230 },
            { TVMSpeedType._220E, TVMSpeedType._220 },
            { TVMSpeedType._220V, TVMSpeedType._220 },
            { TVMSpeedType._220,  TVMSpeedType._270 },
            { TVMSpeedType._230E, TVMSpeedType._230 },
            { TVMSpeedType._230V, TVMSpeedType._230 },
            { TVMSpeedType._230,  TVMSpeedType._270 },
            { TVMSpeedType._270V, TVMSpeedType._270 },
            { TVMSpeedType._270,  TVMSpeedType._300 },
            { TVMSpeedType._300V, TVMSpeedType._300 },
            { TVMSpeedType._300,  TVMSpeedType._320 },
            { TVMSpeedType._320V, TVMSpeedType._320 },
            { TVMSpeedType._320,  TVMSpeedType._000 }
        };
        Dictionary<TVMSpeedType, TVMSpeedType> TVM430SncfTab2 = new Dictionary<TVMSpeedType, TVMSpeedType>
        {
            { TVMSpeedType._RRR,   TVMSpeedType._000 },
            { TVMSpeedType._000,  TVMSpeedType._000 },
            { TVMSpeedType._60E,  TVMSpeedType._60 },
            { TVMSpeedType._60,   TVMSpeedType._60 },
            { TVMSpeedType._80E,  TVMSpeedType._80 },
            { TVMSpeedType._80,   TVMSpeedType._80 },
            { TVMSpeedType._130E, TVMSpeedType._130 },
            { TVMSpeedType._130,  TVMSpeedType._130 },
            { TVMSpeedType._160E, TVMSpeedType._160 },
            { TVMSpeedType._160,  TVMSpeedType._160 },
            { TVMSpeedType._170E, TVMSpeedType._170 },
            { TVMSpeedType._170,  TVMSpeedType._170 },
            { TVMSpeedType._200V, TVMSpeedType._200 },
            { TVMSpeedType._200,  TVMSpeedType._200 },
            { TVMSpeedType._220E, TVMSpeedType._220 },
            { TVMSpeedType._220V, TVMSpeedType._220 },
            { TVMSpeedType._220,  TVMSpeedType._220 },
            { TVMSpeedType._230E, TVMSpeedType._230 },
            { TVMSpeedType._230V, TVMSpeedType._230 },
            { TVMSpeedType._230,  TVMSpeedType._230 },
            { TVMSpeedType._270V, TVMSpeedType._270 },
            { TVMSpeedType._270,  TVMSpeedType._270 },
            { TVMSpeedType._300V, TVMSpeedType._300 },
            { TVMSpeedType._300,  TVMSpeedType._300 },
            { TVMSpeedType._320V, TVMSpeedType._320 },
            { TVMSpeedType._320,  TVMSpeedType._000 }
        };

        // Parameters
        float TVM430TrainSpeedLimitMpS;

        // Variables
        Timer TVM430AspectChangeTimer;

    // Vigilance monitoring (VACMA)
        // Parameters
        float VACMAActivationSpeedMpS;
        float VACMAReleasedAlertDelayS;
        float VACMAReleasedEmergencyDelayS;
        float VACMAPressedAlertDelayS;
        float VACMAPressedEmergencyDelayS;

        // Variables
        bool VACMAEmergencyBraking = true;
        bool VACMATest = false;
        bool VACMAPressed = false;
        Timer VACMAPressedAlertTimer;
        Timer VACMAPressedEmergencyTimer;
        Timer VACMAReleasedAlertTimer;
        Timer VACMAReleasedEmergencyTimer;

    // Other variables
        float InitCount = 0;

        bool EmergencyBraking = false;

        float PreviousNormalSignalDistanceM = 0f;
        bool NormalSignalPassed = false;
        SignalFeatures LastNormalSignalPassed;
        SignalFeatures NextNormalSignal;

        float PreviousDistantSignalDistanceM = 0f;
        bool DistantSignalPassed = false;
        SignalFeatures LastDistantSignalPassed;
        SignalFeatures NextDistantSignal;

        float PreviousInfoSignalDistanceM = 0f;
        bool InfoSignalPassed = false;
        SignalFeatures LastInfoSignalPassed;
        SignalFeatures NextInfoSignal;

        float PreviousRepeaterSignalDistanceM = 0f;
        bool RepeaterSignalPassed = false;
        SignalFeatures LastRepeaterSignalPassed;
        SignalFeatures NextRepeaterSignal;

        float PreviousSpeedPostDistanceM = 0f;
        bool SpeedPostPassed = false;
        SpeedPostFeatures LastSpeedPostPassed;
        SpeedPostFeatures NextSpeedPost;

        float PreviousLineSpeed = 0f;

        public TCS_France() { }

        public override void Initialize()
        {
            // General section
            VACMAPresent = GetBoolParameter("General", "VACMAPresent", true);
            RSOPresent = GetBoolParameter("General", "RSOPresent", true);
            DAATPresent = GetBoolParameter("General", "DAATPresent", false);
            KVBPresent = GetBoolParameter("General", "KVBPresent", false);
            TVM300Present = GetBoolParameter("General", "TVM300Present", false);
            TVM430Present = GetBoolParameter("General", "TVM430Present", false);
            ETCSPresent = GetBoolParameter("General", "ETCSPresent", false);
            ElectroPneumaticBrake = GetBoolParameter("General", "ElectroPneumaticBrake", false);
            HeavyFreightTrain = GetBoolParameter("General", "HeavyFreightTrain", false);
            SafeDecelerationMpS2 = GetFloatParameter("General", "SafeDecelerationMpS2", 0.7f);

            // RSO section
            RSODelayBeforeEmergencyBrakingS = GetFloatParameter("RSO", "DelayBeforeEmergencyBrakingS", 4f);
            RSOBlinkerFrequencyHz = GetFloatParameter("RSO", "BlinkerFrequencyHz", 6f);

            RSOBlinker = new Blinker(this);
            RSOEmergencyTimer = new Timer(this);

            RSOBlinker.Setup(RSOBlinkerFrequencyHz);
            RSOBlinker.Start();
            RSOEmergencyTimer.Setup(RSODelayBeforeEmergencyBrakingS);

            // KVB section
            KVBInhibited = GetBoolParameter("KVB", "Inhibited", false);
            KVBVersion = (KVBVersionType) Enum.Parse(typeof(KVBVersionType), GetStringParameter("KVB", "Version", "V5"));
            KVBTrainCategory = (TrainCategoryType)Enum.Parse(typeof(TrainCategoryType), GetStringParameter("KVB", "TrainCategory", "AU"));
            KVBDataEntryPanel = GetBoolParameter("KVB", "DataEntryPanel", false);
            KVBTrainSpeedLimitMpS = MpS.FromKpH(GetFloatParameter("KVB", "TrainSpeedLimitKpH", 160f));

            KVBGroundFailureTimer = new Timer(this);
            KVBGroundFailureBlinker = new Blinker(this);
            KVBInitTimer = new Timer(this);
            KVBTestTimer = new Timer(this);
            KVBTutTimer = new Timer(this);
            KVBBipTimer = new Timer(this);
            KVBPrincipalDisplayBlinker = new Blinker(this);
            KVBVALButtonLightBlinker = new Blinker(this);
            KVBFCButtonLightBlinker = new Blinker(this);
            KVBCLTVOdometer = new OdoMeter(this);
            KVBCTABPOdometer = new OdoMeter(this);
            KVBTrainLengthOdometer = new OdoMeter(this);
            KVBOverrideNfOdometer = new OdoMeter(this);
            KVBShuntingOdometer = new OdoMeter(this);
            KVBSignalTargetOdometer = new OdoMeter(this);
            KVBPreAnnounceOdometerVLCLI = new OdoMeter(this);
            KVBTrainLengthOdometerVLCLI = new OdoMeter(this);
            KVBTrainLengthOdometerFVL = new OdoMeter(this);
            KarmStartOdometer = new OdoMeter(this);

            KVBGroundFailureTimer.Setup(10f);
            KVBGroundFailureBlinker.Setup(2f);
            KVBInitTimer.Setup(10f);
            KVBTestTimer.Setup(3f);
            KVBTutTimer.Setup(2f);
            KVBBipTimer.Setup(0.5f);
            KVBPrincipalDisplayBlinker.Setup(2f);
            KVBVALButtonLightBlinker.Setup(2f);
            KVBFCButtonLightBlinker.Setup(2f);
            KVBCLTVOdometer.Setup(4400f);
            KVBCTABPOdometer.Setup(2000f);
            KVBOverrideNfOdometer.Setup(100f);
            KVBShuntingOdometer.Setup(3500f);
            KarmStartOdometer.Setup(450f);

            // TVM common section
            TVMCOVITInhibited = GetBoolParameter("TVM", "CovitInhibited", false);

            TVMBlinker = new Blinker(this);

            TVMBlinker.Setup(1f);

            // TVM300 section
            TVM300DecodingFileName = GetStringParameter("TVM300", "DecodingFileName", "..\\..\\common.script\\TGVR_TVM300.csv");

            {
                string path = Path.Combine(Path.GetDirectoryName(Locomotive().WagFilePath), "Script", TVM300DecodingFileName);

                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] parts = line.Split(';');
                            
                            Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType> triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>
                                (
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), "_" + parts[0]),
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), "_" + parts[1]),
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), (parts[2] == "---" ? "Any" : "_" + parts[2]))
                                );
                            Tuple<TVMAspectType, bool, float> onBoardValues = new Tuple<TVMAspectType, bool, float>
                                (
                                    (TVMAspectType)Enum.Parse(typeof(TVMAspectType), "_" + parts[3]),
                                    bool.Parse(parts[4]),
                                    float.Parse(parts[5], CultureInfo.InvariantCulture)
                                );
                            TVM300DecodingTable.Add(triplet, onBoardValues);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(string.Format("File {0} has not been found", path));
                }
            }

            // TVM430 section
            TVM430TrainSpeedLimitMpS = MpS.FromKpH(GetFloatParameter("TVM430", "TrainSpeedLimitKpH", 320f));
            TVM430DecodingFileName = GetStringParameter("TVM430", "DecodingFileName", "..\\..\\common.script\\TGVR_TVM430.csv");

            TVM430AspectChangeTimer = new Timer(this);

            TVM430AspectChangeTimer.Setup(4.7f);

            {
                string path = Path.Combine(Path.GetDirectoryName(Locomotive().WagFilePath), "Script", TVM430DecodingFileName);

                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] parts = line.Split(';');

                            Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType> triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>
                                (
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), "_" + parts[0]),
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), "_" + parts[1]),
                                    (TVMSpeedType)Enum.Parse(typeof(TVMSpeedType), (parts[2] == "---" ? "Any" : "_" + parts[2]))
                                );
                            Tuple<TVMAspectType, bool, float, float, float> onBoardValues = new Tuple<TVMAspectType, bool, float, float, float>
                                (
                                    (TVMAspectType)Enum.Parse(typeof(TVMAspectType), "_" + parts[3]),
                                    bool.Parse(parts[4]),
                                    float.Parse(parts[5], CultureInfo.InvariantCulture),
                                    float.Parse(parts[6], CultureInfo.InvariantCulture),
                                    float.Parse(parts[7], CultureInfo.InvariantCulture)
                                );
                            TVM430DecodingTable.Add(triplet, onBoardValues);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(string.Format("File {0} has not been found", path));
                }
            }

            if (TVM300Present)
            {
                TVMModel = TVMModelType.TVM300;
            }
            else if (TVM430Present)
            {
                if (TVM430TrainSpeedLimitMpS == MpS.FromKpH(300f))
                {
                    TVMModel = TVMModelType.TVM430_V300;
                }
                else
                {
                    TVMModel = TVMModelType.TVM430_V320;
                }
            }

            // VACMA section
            VACMAActivationSpeedMpS = MpS.FromKpH(GetFloatParameter("VACMA", "ActivationSpeedKpH", 3f));
            VACMAReleasedAlertDelayS = GetFloatParameter("VACMA", "ReleasedAlertDelayS", 2.5f);
            VACMAReleasedEmergencyDelayS = GetFloatParameter("VACMA", "ReleasedEmergencyDelayS", 5f);
            VACMAPressedAlertDelayS = GetFloatParameter("VACMA", "PressedAlertDelayS", 55f);
            VACMAPressedEmergencyDelayS = GetFloatParameter("VACMA", "PressedEmergencyDelayS", 60f);

            VACMAPressedAlertTimer = new Timer(this);
            VACMAPressedEmergencyTimer = new Timer(this);
            VACMAReleasedAlertTimer = new Timer(this);
            VACMAReleasedEmergencyTimer = new Timer(this);

            VACMAPressedAlertTimer.Setup(VACMAPressedAlertDelayS);
            VACMAPressedEmergencyTimer.Setup(VACMAPressedEmergencyDelayS);
            VACMAReleasedAlertTimer.Setup(VACMAReleasedAlertDelayS);
            VACMAReleasedEmergencyTimer.Setup(VACMAReleasedEmergencyDelayS);

            // Cabview control names initialization
            SetCustomizedCabviewControlName(BP_AC_SF, "BP (AC) SF : Acquittement / Acknowledge");
            SetCustomizedCabviewControlName(BP_A_LS_SF, "BP (A) LS (SF) : Annulation LS (SF) / Cancel LS (SF)");
            SetCustomizedCabviewControlName(Z_ES_VA, "Z (ES) VA : Essai VACMA / Alerter test");
            SetCustomizedCabviewControlName(KVB_BP_VAL, "BP VAL : Validation des paramètres KVB / KVB parameters validation");
            SetCustomizedCabviewControlName(KVB_BP_MV, "BP MV : Mode manoeuvre KVB / KVB shunting mode");
            SetCustomizedCabviewControlName(KVB_BP_FC, "BP FC : Franchissement carré / Signal at danger override");
            SetCustomizedCabviewControlName(KVB_BP_TEST, "BP TEST : Test KVB / KVB test");
            SetCustomizedCabviewControlName(BP_AM_V1, "BP AM V1 : Armement manuel TVM voie 1 / TVM manual arming track 1");
            SetCustomizedCabviewControlName(BP_AM_V2, "BP AM V2 : Armement manuel TVM voie 2 / TVM manual arming track 2");
            SetCustomizedCabviewControlName(BP_DM, "BP DM : Désarmement manuel TVM / TVM manual dearming");
            SetCustomizedCabviewControlName(TVM_Mask, "Masque TVM / TVM mask");
            SetCustomizedCabviewControlName(VY_CO_URG, "VY CO URG : Contrôle freinage d'urgence / Emergency braking check");
            SetCustomizedCabviewControlName(VY_CO_Z, "VY CO Z : Contrôle interrupteurs d'isolement / Isolation switch check");
            SetCustomizedCabviewControlName(VY_CV, "VY CV : COVIT (freinage d'urgence TVM) / TVM emergency braking");
            SetCustomizedCabviewControlName(VY_SECT, "VY SECT : Sectionnement / Open circuit breaker");
            SetCustomizedCabviewControlName(VY_SECT_AU, "VY SECT AU : Sectionnement automatique / Automatic circuit breaker opening");
            SetCustomizedCabviewControlName(VY_BPT, "VY BPT : Baissez Panto / Lower pantograph");
            SetCustomizedCabviewControlName(TVM_VL, "Visualisateur TVM / TVM display");
            SetCustomizedCabviewControlName(TVM_An1, "Visualisateur TVM / TVM display");
            SetCustomizedCabviewControlName(TVM_An2, "Visualisateur TVM / TVM display");
            SetCustomizedCabviewControlName(TVM_Ex1, "Visualisateur TVM / TVM display");
            SetCustomizedCabviewControlName(TVM_Ex2, "Visualisateur TVM / TVM display");
            SetCustomizedCabviewControlName(LS_SF, "LS (SF) : Signal Fermé / Closed Signal");
            SetCustomizedCabviewControlName(VY_SOS_RSO, "VY SOS RSO : FU RSO / RSO EB");
            SetCustomizedCabviewControlName(VY_SOS_VAC, "VY SOS VAC : FU VACMA / Alerter EB");
            SetCustomizedCabviewControlName(VY_ES_FU, "VY ES FU : Essai FU / EB test");
            SetCustomizedCabviewControlName(VY_SOS_KVB, "VY SOS KVB : FU KVB / KVB EB");
            SetCustomizedCabviewControlName(VY_VTE, "VY VTE : Vitesse Trop Elevée / Speed too high");
            SetCustomizedCabviewControlName(VY_FU, "VY FU : FU KVB / KVB EB");
            SetCustomizedCabviewControlName(KVB_Principal1, "Visualisateur principal KVB");
            SetCustomizedCabviewControlName(KVB_Principal2, "Visualisateur principal KVB");
            SetCustomizedCabviewControlName(KVB_Auxiliary1, "Visualisateur auxiliaire KVB");
            SetCustomizedCabviewControlName(KVB_Auxiliary2, "Visualisateur auxiliaire KVB");

            Activated = true;

            SetNextSignalAspect(Aspect.Clear_1);
        }

        public override void InitializeMoving()
        {
            RSOState = RSOStateType.Off;
            RSOEmergencyBraking = false;
            KVBCLTV = true;
            KVBCTABP = true;
            KVBState = KVBStateType.Normal;
            KVBEmergencyBraking = false;
            VACMAEmergencyBraking = false;

            if (CurrentPostSpeedLimitMpS() > MpS.FromKpH(221f))
            {
                KVBMode = KVBModeType.HighSpeedLine;
                TVMArmed = true;
            }
        }

        protected SignalFeatures MstsSignalToNewSignal(SignalFeatures signal, SignalType signalType)
        {
            switch (signalType)
            {
                case SignalType.Normal:
                case SignalType.Distant:
                    SignalFeatures firstNextSignal = NextGenericSignalFeatures("NORMAL", 0, 3000f);
                    SignalFeatures secondNextSignal = NextGenericSignalFeatures("NORMAL", 0, 6000f);

                    int passedSignalSpeedLimitKpH = Convert.ToInt32(MpS.ToKpH(signal.SpeedLimitMpS));
                    int firstNextSignalSpeedLimitKpH = Convert.ToInt32(MpS.ToKpH(signal.SpeedLimitMpS));
                    int secondNextSignalSpeedLimitKpH = Convert.ToInt32(MpS.ToKpH(signal.SpeedLimitMpS));

                    if (!signal.MainHeadSignalTypeName.ToLower().Contains("tvm")
                        || signal.MainHeadSignalTypeName.ToLower().Contains("cab"))
                    {
                        string textAspect = string.Empty;

                        switch (signal.Aspect)
                        {
                            case Aspect.Stop:
                                if (signal.MainHeadSignalTypeName.Contains("BM"))
                                {
                                    textAspect = "FR_C_BM";
                                }
                                else if (signal.MainHeadSignalTypeName.Contains("BAPR"))
                                {
                                    textAspect = "FR_C_BAPR";
                                }
                                else
                                {
                                    textAspect = "FR_C_BAL";
                                }
                                break;

                            case Aspect.StopAndProceed:
                                if (signal.MainHeadSignalTypeName.Contains("BAPR"))
                                {
                                    textAspect = "FR_S_BAPR";
                                }
                                else
                                {
                                    textAspect = "FR_S_BAL";
                                }
                                break;

                            case Aspect.Restricted:
                                if (signal.MainHeadSignalTypeName.Contains("D"))
                                {
                                    textAspect = "FR_D";
                                }
                                else if (signal.MainHeadSignalTypeName.Contains("RR")
                                    && !signal.MainHeadSignalTypeName.Contains("RR60")
                                    && passedSignalSpeedLimitKpH == 30)
                                {
                                    textAspect = "FR_RR_A";
                                }
                                else if (signal.MainHeadSignalTypeName.Contains("RR60")
                                    && passedSignalSpeedLimitKpH == 60)
                                {
                                    textAspect = "FR_RRCLI_A";
                                }
                                else if (firstNextSignalSpeedLimitKpH == 30)
                                {
                                    textAspect = "FR_R";
                                }
                                else
                                {
                                    textAspect = "FR_SCLI";
                                }
                                break;

                            case Aspect.Approach_1:
                                if (passedSignalSpeedLimitKpH == 30)
                                {
                                    if (firstNextSignal.Aspect == Aspect.Stop
                                        || firstNextSignal.Aspect == Aspect.StopAndProceed)
                                    {
                                        textAspect = "FR_RR_A";
                                    }
                                    else
                                    {
                                        textAspect = "FR_RR";
                                    }
                                }
                                else
                                {
                                    textAspect = "FR_A";
                                }
                                break;

                            case Aspect.Approach_2:
                                if (firstNextSignalSpeedLimitKpH == 30)
                                {
                                    textAspect = "FR_R";
                                }
                                else if (firstNextSignalSpeedLimitKpH == 60)
                                {
                                    if (signal.MainHeadSignalTypeName.Contains("AA")
                                        && (secondNextSignal.Aspect == Aspect.Stop
                                            || secondNextSignal.Aspect == Aspect.StopAndProceed))
                                    {
                                        textAspect = "FR_RCLI_ACLI";
                                    }
                                    else
                                    {
                                        textAspect = "FR_RCLI";
                                    }
                                }
                                else if (firstNextSignalSpeedLimitKpH == 160)
                                {
                                    textAspect = "FR_VLCLI_ANN";
                                }
                                else if (secondNextSignal.Aspect == Aspect.Stop
                                    || secondNextSignal.Aspect == Aspect.StopAndProceed)
                                {
                                    textAspect = "FR_ACLI";
                                }
                                break;

                            case Aspect.Approach_3:
                                if (passedSignalSpeedLimitKpH == 30)
                                {
                                    textAspect = "FR_RR_A";
                                }
                                else if (passedSignalSpeedLimitKpH == 60)
                                {
                                    textAspect = "FR_RRCLI_A";
                                }
                                else if (firstNextSignalSpeedLimitKpH == 30
                                    && firstNextSignal.Aspect > Aspect.StopAndProceed)
                                {
                                    textAspect = "FR_R";
                                }
                                else if (firstNextSignal.SpeedLimitMpS.AlmostEqual(MpS.FromKpH(60f), 0.1f))
                                {
                                    if (secondNextSignal.Aspect == Aspect.Stop
                                        || secondNextSignal.Aspect == Aspect.StopAndProceed)
                                    {
                                        textAspect = "FR_RCLI_ACLI";
                                    }
                                    else
                                    {
                                        textAspect = "FR_RCLI";
                                    }
                                }
                                else if (firstNextSignal.SpeedLimitMpS.AlmostEqual(MpS.FromKpH(160f), 0.1f))
                                {
                                    textAspect = "FR_VLCLI_ANN";
                                }
                                else if (secondNextSignal.Aspect == Aspect.Stop
                                    || secondNextSignal.Aspect == Aspect.StopAndProceed)
                                {
                                    textAspect = "FR_ACLI";
                                }
                                break;

                            case Aspect.Clear_1:
                                if (signal.MainHeadSignalTypeName.Contains("VLVL"))
                                {
                                    textAspect = "FR_VLCLI_ANN";
                                }
                                else if ()
                                {

                                }
                                break;

                            case Aspect.Clear_2:
                                if (signal.MainHeadSignalTypeName.Contains("VLVL"))
                                {
                                    textAspect = "FR_VL_SUP";
                                }
                                else
                                {
                                    textAspect = "FR_VL_INF";
                                }
                                break;
                        }

                        if (signal.MainHeadSignalTypeName.ToLower().Contains("tvm430")
                            && signal.MainHeadSignalTypeName.ToLower().Contains("fincab"))
                        {
                            textAspect += " BSP_ESL";
                        }
                        else if (signal.MainHeadSignalTypeName.ToLower().Contains("tvm430")
                            && signal.MainHeadSignalTypeName.ToLower().Contains("cab"))
                        {
                            textAspect += " BSP_ECS";
                        }

                        return new SignalFeatures(
                            mainHeadSignalTypeName: signal.MainHeadSignalTypeName,
                            aspect: signal.Aspect,
                            distanceM: signal.DistanceM,
                            speedLimitMpS: signal.SpeedLimitMpS,
                            altitudeM: signal.AltitudeM,
                            textAspect: textAspect
                            );
                    }
                    else if (signal.MainHeadSignalTypeName.ToLower().Contains("tvm430"))
                    {
                        return new SignalFeatures(
                            mainHeadSignalTypeName: signal.MainHeadSignalTypeName,
                            aspect: signal.Aspect,
                            distanceM: signal.DistanceM,
                            speedLimitMpS: signal.SpeedLimitMpS,
                            altitudeM: signal.AltitudeM,
                            textAspect: "FR_TVM430"
                            );
                    }
                    else if (signal.MainHeadSignalTypeName.ToLower().Contains("tvm300"))
                    {
                        return new SignalFeatures(
                            mainHeadSignalTypeName: signal.MainHeadSignalTypeName,
                            aspect: signal.Aspect,
                            distanceM: signal.DistanceM,
                            speedLimitMpS: signal.SpeedLimitMpS,
                            altitudeM: signal.AltitudeM,
                            textAspect: "FR_TVM300"
                            );
                    }
                    else
                    {
                        return signal;
                    }

                default:
                    return signal;
            }
        }

        protected void OnSignalPassed(SignalFeatures signal, SignalType signalType)
        {
            signal = MstsSignalToNewSignal(signal, signalType);

            OnSignalPassedRso(signal, signalType);
            OnSignalPassedKvb(signal, signalType);
            OnSignalPassedTvm(signal, signalType);
        }

        protected void OnSpeedPostPassed(SpeedPostFeatures speedPost)
        {
            OnSpeedPostPassedRso(speedPost);
            OnSpeedPostPassedKvb(speedPost);
        }

        public override void Update()
        {
            if (IsTrainControlEnabled())
            {
                if (InitCount < 5)
                {
                    InitCount++;

                    if (InitCount == 5 && CurrentPostSpeedLimitMpS() > MpS.FromKpH(221f))
                    {
                        QBal = QBalType.LGV;
                    }

                    return;
                }

                UpdateSignalPassed();

                UpdateVACMA();
                UpdateRso();
                UpdateTvm();
                UpdateKvb();

                if (RSOEmergencyBraking
                    || KVBEmergencyBraking
                    || KarmEmergencyBraking
                    || TVMCOVITEmergencyBraking
                    || VACMAEmergencyBraking)
                {
                    EmergencyBraking = true;
                }
                else if (RearmingButton)
                {
                    EmergencyBraking = false;
                }

                SetEmergencyBrake(EmergencyBraking);

                SetPenaltyApplicationDisplay(IsBrakeEmergency());

                SetPowerAuthorization(!EmergencyBraking);

                RSOType1Inhibition = IsDirectionReverse();
                RSOType2Inhibition = !KVBInhibited && ((TVM300Present || TVM430Present) && TVMArmed);
                RSOType3Inhibition = (!TVM300Present && !TVM430Present) || !TVMCOVITInhibited;

                PreviousLineSpeed = CurrentPostSpeedLimitMpS();
            }
        }

        protected void UpdateRso()
        {
            if (RSOPresent && IsSpeedControlEnabled())
            {
                if ((RSOClosedSignal && !RSOType2Inhibition) || (TVMClosedSignal && !RSOType3Inhibition))
                {
                    RSOEmergencyTimer.Start();

                    if (RSOPressed)
                    {
                        RSOState = RSOStateType.TriggeredPressed;
                    }
                    else
                    {
                        RSOState = RSOStateType.TriggeredBlinking;
                    }
                }

                if (RSOOpenedSignal || TVMOpenedSignal || RSOCancelPressed)
                {
                    RSOEmergencyTimer.Stop();
                    RSOState = RSOStateType.Off;
                }

                switch (RSOState)
                {
                    case RSOStateType.Init:
                        if (!RSOBlinker.Started)
                        {
                            RSOBlinker.Start();
                        }
                        SetCabDisplayControl(LS_SF, RSOBlinker.On || RSOPressed ? 1 : 0);
                        break;

                    case RSOStateType.Off:
                        if (RSOBlinker.Started)
                        {
                            RSOBlinker.Stop();
                        }
                        SetCabDisplayControl(LS_SF, RSOPressed ? 1 : 0);
                        break;

                    case RSOStateType.TriggeredPressed:
                        SetCabDisplayControl(LS_SF, 0);

                        if (!RSOPressed)
                        {
                            RSOState = RSOStateType.TriggeredFixed;
                            RSOEmergencyTimer.Stop();
                        }
                        break;

                    case RSOStateType.TriggeredBlinking:
                        if (!RSOBlinker.Started)
                        {
                            RSOBlinker.Start();
                        }
                        SetCabDisplayControl(LS_SF, RSOBlinker.On ? 1 : 0);

                        if (!RSOPressed && RSOPreviousPressed)
                        {
                            RSOState = RSOStateType.TriggeredFixed;
                            RSOEmergencyTimer.Stop();
                        }
                        break;

                    case RSOStateType.TriggeredFixed:
                        SetCabDisplayControl(LS_SF, 1);
                        break;
                }

                if (RSOEmergencyTimer.Triggered)
                {
                    RSOEmergencyBraking = true;
                }
                else if (RearmingButton)
                {
                    RSOEmergencyBraking = false;
                }

                SetCabDisplayControl(VY_SOS_RSO, RSOEmergencyBraking ? 1 : 0);

                if (RSOClosedSignal && !RSOPreviousClosedSignal && !RSOType1Inhibition)
                {
                    TriggerKvbBipSound(0.3f);
                }

                RSOPreviousClosedSignal = RSOClosedSignal;

                if (TVM300Present || TVM430Present)
                {
                    if (TVMClosedSignal && !TVMPreviousClosedSignal)
                    {
                        TriggerKvbBipSound(0.3f);
                    }

                    if (TVMOpenedSignal && !TVMPreviousOpenedSignal)
                    {
                        TriggerKvbBipSound(0.3f);
                    }

                    TVMPreviousClosedSignal = TVMClosedSignal;
                    TVMPreviousOpenedSignal = TVMOpenedSignal;
                }

                RSOPreviousPressed = RSOPressed;

                RSOClosedSignal = RSOOpenedSignal = false;
            }
        }

        protected void OnSignalPassedRso(SignalFeatures signal, SignalType signalType)
        {
            if (RSOPresent && IsSpeedControlEnabled())
            {
                List<string> signalAspect = signal.TextAspect?.Split(' ').ToList() ?? new List<string>();

                switch (signalType)
                {
                    case SignalType.Normal:
                    case SignalType.Distant:
                        if (signalAspect.Exists(x => x == "FR_C_BAL"
                            || x == "FR_C_BAPR"
                            || x == "FR_C_BM"
                            || x == "FR_CV"
                            || x == "FR_S_BAL"
                            || x == "FR_S_BAPR"
                            || x == "FR_S_BM"
                            || x == "FR_SCLI"
                            || x == "FR_MCLI"
                            || x == "FR_M"
                            || x == "FR_D"
                            || x == "FR_RR_A"
                            || x == "FR_RR_ACLI"
                            || x == "FR_RRCLI_A"
                            || x == "FR_RRCLI_ACLI"
                            || x == "FR_A"
                            || x == "FR_R"
                            || x == "FR_ACLI"
                            || x == "FR_RCLI"
                            || x == "FR_RCLI_ACLI"
                            || x == "LU_SFP1"
                            || x == "LU_SFAv1"
                            || x == "LU_SFAv3"
                            || x == "CH_CROCODILE_SF"))
                        {
                            RSOClosedSignal = true;
                            RSOOpenedSignal = false;
                        }
                        else
                        {
                            RSOClosedSignal = false;
                            RSOOpenedSignal = true;
                        }
                        break;

                    case SignalType.Repeater:
                        if (signalAspect.Contains("FR_TIVD_PRESENTE"))
                        {
                            RSOClosedSignal = true;
                            RSOOpenedSignal = false;
                        }
                        else if (signalAspect.Contains("FR_TIVD_EFFACE"))
                        {
                            RSOClosedSignal = false;
                            RSOOpenedSignal = true;
                        }
                        break;
                }
            }
        }

        protected void OnSpeedPostPassedRso(SpeedPostFeatures speedPost)
        {
            if (RSOPresent && IsSpeedControlEnabled())
            {
                if (speedPost.SpeedPostTypeName?.StartsWith("TIV_L") ?? false)
                {
                    RSOClosedSignal = true;
                    RSOOpenedSignal = false;
                }
            }
        }

        protected void UpdateKvb()
        {
            if (KVBPresent && IsSpeedControlEnabled())
            {
                if (IsCabPowerSupplyOn())
                {
                    if (CurrentPostSpeedLimitMpS() > MpS.FromKpH(221f) && PreviousLineSpeed <= MpS.FromKpH(221f) && SpeedMpS() > 0f)
                    {
                        KarmStartOdometer.Start();

                        KVBSpadEmergency = false;
                        KVBOverspeedEmergency = false;
                        KVBSpeedTooHighLight = false;
                        KVBMode = KVBModeType.HighSpeedLine;
                    }
                    else if (NextPostSpeedLimitMpS(0) <= MpS.FromKpH(221f) && NextPostDistanceM(0) < 5f && PreviousLineSpeed > MpS.FromKpH(221f) && SpeedMpS() > 0f)
                    {
                        KVBMode = KVBModeType.ConventionalLine;
                    }

                    if (KarmStartOdometer.Triggered)
                    {
                        KarmStartOdometer.Stop();
                        QBal = QBalType.LGV;
                    }
                    else if (NextPostSpeedLimitMpS(0) <= MpS.FromKpH(221f) && NextPostDistanceM(0) < 60f && PreviousLineSpeed > MpS.FromKpH(221f) && SpeedMpS() > 0f)
                    {
                        QBal = QBalType.LC;
                    }

                    if (QBal == QBalType.LGV && (TVM300Present || TVM430Present))
                    {
                        if (!KVBInhibited && !TVMArmed)
                        {
                            KarmEmergencyBraking = true;
                        }
                        else if (RearmingButton)
                        {
                            KarmEmergencyBraking = false;
                        }
                    }
                    else
                    {
                        KarmEmergencyBraking = false;
                    }

                    switch (KVBMode)
                    {
                        case KVBModeType.Off:
                            KVBMode = KVBModeType.Init;
                            KVBInitTimer.Start();
                            break;

                        case KVBModeType.Init:
                            if (KVBInitTimer.Triggered)
                            {
                                KVBInitTimer.Stop();

                                KVBMode = KVBModeType.ConventionalLine;
                                DECAB = true;

                                if (!KVBDataEntryPanel)
                                {
                                    KVBParametersValidation = true;
                                }
                            }
                            else
                            {
                                if (KVBInitTimer.RemainingValue <= 8f && KVBInitTimer.RemainingValue > 6f)
                                {
                                    TriggerKvbTutSound(2f);
                                }
                                else if (KVBInitTimer.RemainingValue <= 5.8f && KVBInitTimer.RemainingValue > 5.3f)
                                {
                                    TriggerKvbTutSound(0.5f);
                                    TriggerKvbBipSound(0.5f);
                                    KVBEngineFailure = true;
                                }
                                else
                                {
                                    KVBEngineFailure = false;
                                }

                                UpdateKvbDisplay();

                                UpdateKvbSound();
                            }
                            break;

                        case KVBModeType.HighSpeedLine:
                            if (!ARMCAB)
                            {
                                KVBMode = KVBModeType.ConventionalLine;
                                DECAB = true;
                            }
                            else
                            {
                                ResetKvbTargets();

                                UpdateKvbInit();

                                UpdateKvbTest();

                                UpdateKvbOverrideNf();

                                UpdateKvbEmergencyBraking();

                                UpdateKvbDisplay();

                                UpdateKvbSound();
                            }
                            break;

                        case KVBModeType.ConventionalLine:
                            if (KVBGroundFailure)
                            {
                                if (!KVBGroundFailureTimer.Started)
                                {
                                    KVBGroundFailureTimer.Start();
                                    KVBGroundFailureBlinker.Start();
                                    TriggerKvbBipSound(0.5f);
                                    ResetKvbTargets(true);
                                }

                                UpdateKvbTest();

                                UpdateKvbDisplay();

                                UpdateKvbSound();
                            }
                            else
                            {
                                if (ARMCAB)
                                {
                                    KVBSpadEmergency = false;
                                    KVBOverspeedEmergency = false;
                                    KVBSpeedTooHighLight = false;
                                    KVBMode = KVBModeType.HighSpeedLine;
                                    DECAB = false;
                                }
                                else
                                {
                                    UpdateKvbParameters();

                                    UpdateKvbInit();

                                    UpdateKvbTest();

                                    UpdateKvbOverrideNf();

                                    UpdateKvbTargets();

                                    UpdateKvbProx();

                                    UpdateKvbPreAnnounce();

                                    UpdateKvbSpeedControl();

                                    UpdateKvbEmergencyBraking();

                                    UpdateKvbDisplay();

                                    UpdateKvbSound();

                                    // Send data to the simulator
                                    if (KVBSignalTargetSpeed == KVBSignalTargetSpeedType.V0)
                                    {
                                        SetNextSpeedLimitMpS(0f);
                                    }
                                    else if (KVBSignalTargetSpeed == KVBSignalTargetSpeedType.V160)
                                    {
                                        SetNextSpeedLimitMpS(Math.Min(KVBTrainSpeedLimitMpS, 160f));
                                    }
                                    else
                                    {
                                        SetNextSpeedLimitMpS(Math.Min(
                                            KVBTrainSpeedLimitMpS,
                                            KVBSpeedPostAnnounceList.Values.Select(x => (float?)KvbSpeedToSpeed(x.Speed)).Min() ?? KVBTrainSpeedLimitMpS
                                            ));
                                    }

                                    SetCurrentSpeedLimitMpS(Math.Min(
                                        KVBTrainSpeedLimitMpS,
                                        Math.Min(
                                            KVBSpeedPostExecutionList.Values.Select(x => (float?)KvbSpeedToSpeed(x)).Min() ?? KVBTrainSpeedLimitMpS,
                                            KVBPreAnnounce != KVBPreAnnounceType.Deactivated ? MpS.FromKpH(220f) : MpS.FromKpH(160f)
                                        )));
                                }
                            }
                            break;

                        case KVBModeType.Shunting:
                            ResetKvbTargets(true);

                            UpdateKvbParameters();

                            UpdateKvbInit();

                            UpdateKvbTest();

                            UpdateKvbShunting();

                            UpdateKvbSpeedControl();

                            UpdateKvbEmergencyBraking();

                            UpdateKvbDisplay();

                            UpdateKvbSound();
                            break;
                    }
                }
                else
                {
                    KVBMode = KVBModeType.Off;
                    KVBSpadEmergency = false;
                    KVBEmergencyBraking = true;
                    ResetKvbTargets(true);
                    UpdateKvbDisplay();

                    if (KVBTutTimer.Started)
                    {
                        KVBTutTimer.Stop();
                        TriggerGenericSound(Event.TrainControlSystemPenalty2);
                    }

                    if (KVBBipTimer.Started)
                    {
                        KVBBipTimer.Stop();
                        TriggerGenericSound(Event.TrainControlSystemInfo2);
                    }
                }
            }
            else
            {
                KVBEmergencyBraking = false;
            }
        }

        protected void UpdateKvbParameters()
        {
            KVBTrainLengthM = KVBMode == KVBModeType.Shunting || !KVBParametersValidation ? 800f : (float)Math.Ceiling((double)(TrainLengthM() / 100f)) * 100f;
            KVBTrainLengthOdometer.Setup(KVBTrainLengthM);
            KVBTrainLengthOdometerVLCLI.Setup(KVBTrainLengthM);
            KVBTrainLengthOdometerFVL.Setup(KVBTrainLengthM);

            if (ElectroPneumaticBrake)
                KVBDelayBeforeBrakingEstablishedS = 2f;
            else if (HeavyFreightTrain)
                KVBDelayBeforeBrakingEstablishedS = 12f + KVBTrainLengthM / 200f;
            else
                KVBDelayBeforeBrakingEstablishedS = 2f + 2f * KVBTrainLengthM * KVBTrainLengthM * 0.00001f;
        }

        protected void UpdateKvbInit()
        {
            if (!KVBCLTV)
            {
                if (!KVBCLTVOdometer.Started)
                {
                    KVBCLTVOdometer.Start();
                }

                if (KVBCLTVOdometer.Triggered)
                {
                    KVBCLTV = true;
                }
            }
            else
            {
                if (KVBCLTVOdometer.Started)
                {
                    KVBCLTVOdometer.Stop();
                }
            }

            if (!KVBCTABP)
            {
                if (!KVBCTABPOdometer.Started)
                {
                    KVBCTABPOdometer.Start();
                }

                if (KVBCTABPOdometer.Triggered || ARMCAB)
                {
                    KVBCTABP = true;
                }
            }
            else
            {
                if (KVBCTABPOdometer.Started)
                {
                    KVBCTABPOdometer.Stop();
                }
            }
        }

        protected void OnSignalPassedKvb(SignalFeatures signal, SignalType signalType)
        {
            if (KVBPresent && IsSpeedControlEnabled() && KVBMode != KVBModeType.Shunting)
            {
                List<string> lastSignalAspect = signal.TextAspect?.Split(' ').ToList() ?? new List<string>();

                if (lastSignalAspect.Contains("FR_REPRISE_VL"))
                {
                    return;
                }

                switch (signalType)
                {
                    case SignalType.Normal:
                    case SignalType.Distant:
                        KVBSignalFieldType signalField = TextAspectToKvbSignalField(lastSignalAspect);

                        switch (signalField)
                        {
                            case KVBSignalFieldType.C_BAL:
                                if (!KVBOverrideNf)
                                {
                                    KVBSpadEmergency = true;
                                    TriggerKvbTutSound(2.0f);

                                    Message(ConfirmLevel.Warning, "SOS KVB");
                                    Message(ConfirmLevel.Warning, "KVB : Franchissement carré / Signal passed at danger");
                                }

                                KVBOnSight = true;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.S_BM:
                                if (!KVBOverrideNf)
                                {
                                    KVBSpadEmergency = true;
                                    TriggerKvbTutSound(2.0f);

                                    Message(ConfirmLevel.Warning, "SOS KVB");
                                    Message(ConfirmLevel.Warning, "KVB : Franchissement carré / Signal passed at danger");
                                }

                                KVBOnSight = false;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.S_BAL:
                                KVBOnSight = true;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.A:
                                KVBOnSight = false;

                                if (NextNormalSignal.TextAspect.Length > 0)
                                {
                                    KVBSignalTargetIsBufferStop = false;
                                    KVBSignalTargetOdometer.Setup(NextNormalSignal.DistanceM);
                                    KVBSignalTargetOdometer.Start();
                                }
                                else
                                {
                                    KVBSignalTargetIsBufferStop = true;
                                    KVBSignalTargetOdometer.Setup(EOADistanceM(0));
                                    KVBSignalTargetOdometer.Start();
                                }

                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.ACLI:
                                KVBOnSight = false;

                                SignalFeatures secondNextSignal = NextGenericSignalFeatures("NORMAL", 1, 5000f);

                                if (secondNextSignal.TextAspect.Length > 0)
                                {
                                    KVBSignalTargetIsBufferStop = false;
                                    KVBSignalTargetOdometer.Setup(secondNextSignal.DistanceM);
                                    KVBSignalTargetOdometer.Start();
                                }
                                else
                                {
                                    KVBSignalTargetIsBufferStop = true;
                                    KVBSignalTargetOdometer.Setup(EOADistanceM(0));
                                    KVBSignalTargetOdometer.Start();
                                }

                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.VLCLI:
                                KVBOnSight = false;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBSignalTargetOdometer.Stop();

                                switch (KVBPreAnnounceVLCLI)
                                {
                                    case KVBPreAnnounceType.Armed:
                                        KVBPreAnnounceVLCLI = KVBPreAnnounceType.Triggered;
                                        if (KVBPrincipalDisplayState == KVBPrincipalDisplayStateType.b)
                                        {
                                            TriggerKvbBipSound(0.5f);
                                        }
                                        KVBPreAnnounceOdometerVLCLI.Setup(NextNormalSignal.DistanceM);
                                        KVBPreAnnounceOdometerVLCLI.Start();
                                        break;

                                    case KVBPreAnnounceType.Triggered:
                                        KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                        KVBPreAnnounceOdometerVLCLI.Stop();
                                        break;
                                }
                                break;

                            case KVBSignalFieldType.VL_INF:
                                KVBOnSight = false;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBSignalTargetOdometer.Stop();
                                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                                break;

                            case KVBSignalFieldType.VL_SUP:
                                KVBOnSight = false;
                                KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                                KVBSignalTargetOdometer.Stop();

                                if (KVBSignalField == KVBSignalFieldType.VLCLI)
                                {
                                    KVBTrainLengthOdometerVLCLI.Start();
                                }
                                else
                                {
                                    KVBPreAnnounceVLCLI = KVBPreAnnounceType.Armed;
                                }
                                break;
                        }

                        KVBSignalField = signalField;
                        KVBSignalExecutionSpeed = KvbSignalFieldToKvbSignalExecutionSpeed(signalField);
                        KVBSignalTargetSpeed = KvbSignalFieldToKvbSignalTargetSpeed(signalField);

                        // Speed announce field
                        if (lastSignalAspect.Contains("FR_R"))
                        {
                            SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 3000f);

                            // If DVL found
                            if (infoSignal.TextAspect.Contains("KVB_DVL"))
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(infoSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V30, odometer);
                            }
                            else
                            {
                                SignalFeatures normalSignal = FindNextSignalWithSpeedLimit(30f, "NORMAL", 5, 3000f);

                                // If signal found
                                if (normalSignal.DistanceM <= 3000f)
                                {
                                    OdoMeter odometer = new OdoMeter(this);
                                    odometer.Setup(normalSignal.DistanceM);
                                    odometer.Start();

                                    KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V30, odometer);
                                }
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_RCLI") || lastSignalAspect.Contains("FR_RCLI_ACLI"))
                        {
                            SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 3000f);

                            // If DVL found
                            if (infoSignal.TextAspect.Contains("KVB_DVL"))
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(infoSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V60, odometer);
                            }
                            else
                            {
                                SignalFeatures normalSignal = FindNextSignalWithSpeedLimit(60f, "NORMAL", 5, 3000f);

                                // If signal found
                                if (normalSignal.DistanceM <= 3000f)
                                {
                                    OdoMeter odometer = new OdoMeter(this);
                                    odometer.Setup(normalSignal.DistanceM);
                                    odometer.Start();

                                    KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V60, odometer);
                                }
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VAN_V40"))
                        {
                            SignalFeatures normalSignal = FindNextSignalWithTextAspect("CH_KVB_CHAMP_VRA_V40", "NORMAL", 5, 3000f);

                            // If signal found
                            if (normalSignal.DistanceM <= 3000f)
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(normalSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V40, odometer);
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VAN_V60"))
                        {
                            SignalFeatures normalSignal = FindNextSignalWithTextAspect("CH_KVB_CHAMP_VRA_V60", "NORMAL", 5, 3000f);

                            // If signal found
                            if (normalSignal.DistanceM <= 3000f)
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(normalSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V60, odometer);
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VAN_V90"))
                        {
                            SignalFeatures normalSignal = FindNextSignalWithTextAspect("CH_KVB_CHAMP_VRA_V90", "NORMAL", 5, 3000f);

                            // If signal found
                            if (normalSignal.DistanceM <= 3000f)
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(normalSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V90, odometer);
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_RR")
                            || lastSignalAspect.Contains("FR_RR_A")
                            || lastSignalAspect.Contains("FR_RR_ACLI"))
                        {
                            SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 1000f);

                            // If DVL found
                            if (infoSignal.TextAspect.Contains("KVB_DVL"))
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(infoSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V30, odometer);
                            }
                            // If DVL not found, start speed limit now
                            else
                            {
                                KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostSpeedType.V30;

                                if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                                {
                                    KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                                }
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_RRCLI")
                            || lastSignalAspect.Contains("FR_RRCLI_A")
                            || lastSignalAspect.Contains("FR_RRCLI_ACLI"))
                        {
                            SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 1000f);

                            // If DVL found
                            if (infoSignal.TextAspect.Contains("KVB_DVL"))
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(infoSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (KVBSpeedPostSpeedType.V60, odometer);
                            }
                            // If DVL not found, start speed limit now
                            else
                            {
                                KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostSpeedType.V60;

                                if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                                {
                                    KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                                }
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VRA_V40"))
                        {
                            KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostSpeedType.V40;

                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                            {
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VRA_V60"))
                        {
                            KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostSpeedType.V60;

                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                            {
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                            }
                        }
                        else if (lastSignalAspect.Contains("CH_KVB_CHAMP_VRA_V90"))
                        {
                            KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostSpeedType.V90;

                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                            {
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                            }
                        }
                        break;

                    case SignalType.Repeater:
                        if (lastSignalAspect.Contains("FR_TIVR_PRESENTE"))
                        {
                            int i;
                            float speed = 0f;

                            // Search for the next speed limit
                            for (i = 0; i < 5; i++)
                            {
                                speed = NextPostSpeedLimitMpS(i);

                                if (speed > 0f && speed <= MpS.FromKpH(160f))
                                {
                                    break;
                                }
                            }
                            KVBSpeedPostSpeedType kvbSpeed = SpeedToKvbSpeed(speed);

                            SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 1000f);

                            // If DVL found
                            if (infoSignal.TextAspect.Contains("KVB_DVL"))
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(infoSignal.DistanceM);
                                odometer.Start();

                                KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (kvbSpeed, odometer);
                            }
                            // If DVL not found, start speed limit now
                            else
                            {
                                KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = kvbSpeed;

                                if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                                {
                                    KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                                }
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_TIVR_EFFACE")
                            && KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G)
                            && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G].Speed != KVBSpeedPostSpeedType.V30
                            && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G].Speed != KVBSpeedPostSpeedType.V60)
                        {
                            KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                        }
                        else if (lastSignalAspect.Contains("FR_TIVD_PRESENTE"))
                        {
                            float speed = -1f;

                            // Search for the next speed limit
                            for (int i = 0; i < 5; i++)
                            {
                                speed = NextPostSpeedLimitMpS(i);

                                if (speed > 0f)
                                {
                                    break;
                                }
                            }
                            KVBSpeedPostSpeedType kvbSpeed = SpeedToKvbSpeed(speed);

                            // If signal found
                            if (speed > 0)
                            {
                                SignalFeatures infoSignal = FindNextSignalWithTextAspect("KVB_DVL", "INFO", 5, 3000f);

                                // If DVL found
                                if (infoSignal.TextAspect.Contains("KVB_DVL"))
                                {
                                    OdoMeter odometer = new OdoMeter(this);
                                    odometer.Setup(infoSignal.DistanceM);
                                    odometer.Start();

                                    KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (kvbSpeed, odometer);
                                }
                                else
                                {
                                    SignalFeatures normalSignal = FindNextSignalWithSpeedLimit(60f, "NORMAL", 5, 3000f);

                                    // If signal found
                                    if (normalSignal.DistanceM <= 3000f)
                                    {
                                        OdoMeter odometer = new OdoMeter(this);
                                        odometer.Setup(normalSignal.DistanceM);
                                        odometer.Start();

                                        KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G] = (kvbSpeed, odometer);
                                    }
                                }
                            }
                            else
                            {
                                KVBGroundFailure = true;
                            }

                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.GS1)
                                && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Speed == KVBSpeedPostSpeedType.P)
                            {
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.GS1);
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_TIVD_EFFACE"))
                        {
                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.GS1)
                                && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Speed == KVBSpeedPostSpeedType.P
                                && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Odometer.RemainingValue <= 50f)
                            {
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.GS1);
                            }
                        }
                        else if (lastSignalAspect.Contains("FR_TABP_PRESENTE"))
                        {
                            KVBCTABP = true;

                            if (KVBTrainSpeedLimitMpS > MpS.FromKpH(160f))
                            {
                                int i;
                                SignalFeatures tivdMobile = NextGenericSignalFeatures("REPEATER", 0, 3000f);

                                // Search for the next repeater signal with TIVD information
                                for (i = 0; i < 5; i++)
                                {
                                    tivdMobile = NextGenericSignalFeatures("REPEATER", i, 3000f);

                                    if (tivdMobile.TextAspect.Contains("FR_TIVD_PRESENTE"))
                                    {
                                        break;
                                    }
                                }

                                // If signal found
                                if (i < 5)
                                {
                                    if (KVBPrincipalDisplayState == KVBPrincipalDisplayStateType.b)
                                    {
                                        TriggerKvbBipSound(0.5f);
                                    }

                                    OdoMeter odometer = new OdoMeter(this);
                                    odometer.Setup(tivdMobile.DistanceM);
                                    odometer.Start();

                                    KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1] = (KVBSpeedPostSpeedType.P, odometer);
                                }
                                else
                                {
                                    SpeedPostFeatures tivFixe = NextSpeedPostFeatures(0, 3000f);

                                    // Search for the speed post with a speed of 160 kph
                                    for (i = 0; i < 5; i++)
                                    {
                                        tivFixe = NextSpeedPostFeatures(i, 3000f);

                                        if (!tivFixe.IsWarning && SpeedToKvbSpeed(tivFixe.SpeedLimitMpS) == KVBSpeedPostSpeedType.V160)
                                        {
                                            break;
                                        }
                                    }

                                    // If speed post found
                                    if (i < 5)
                                    {
                                        if (KVBPrincipalDisplayState == KVBPrincipalDisplayStateType.b)
                                        {
                                            TriggerKvbBipSound(0.5f);
                                        }

                                        OdoMeter odometer = new OdoMeter(this);
                                        odometer.Setup(tivFixe.DistanceM);
                                        odometer.Start();

                                        KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1] = (KVBSpeedPostSpeedType.P, odometer);
                                    }
                                }
                            }
                        }
                        break;

                    case SignalType.Info:
                        if (lastSignalAspect.Contains("KVB_DGV"))
                        {
                            QBal = QBalType.LGV;
                        }
                        else if (lastSignalAspect.Contains("KVB_FGV"))
                        {
                            QBal = QBalType.LC;
                        }
                        else if (lastSignalAspect.Contains("KVB_DVL"))
                        {
                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                            {
                                KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.G] = KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.G].Speed;
                                KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.G);
                            }
                        }
                        else if (lastSignalAspect.Contains("KVB_FVL"))
                        {
                            if (KVBSpeedPostExecutionList.ContainsKey(KVBSpeedPostSpeedCategory.G))
                            {
                                KVBTrainLengthOdometerFVL.Start();
                            }
                        }
                        else if (lastSignalAspect.Contains("KVB_FZ"))
                        {
                            ResetKvbTargets();
                        }
                        break;
                }
            }
        }

        protected void OnSpeedPostPassedKvb(SpeedPostFeatures speedPost)
        {
            if (KVBPresent && IsSpeedControlEnabled())
            {
                if (speedPost.SpeedLimitMpS > 0f)
                {
                    string speedPostType = speedPost.SpeedPostTypeName ?? string.Empty;

                    if (speedPostType.Contains("TIVP"))
                    {
                        KVBCTABP = true;

                        if (KVBTrainSpeedLimitMpS > MpS.FromKpH(160f))
                        {
                            if (KVBPrincipalDisplayState == KVBPrincipalDisplayStateType.b)
                            {
                                TriggerKvbBipSound(0.5f);
                            }

                            OdoMeter odometer = new OdoMeter(this);
                            odometer.Setup(NextSpeedPost.DistanceM);
                            odometer.Start();

                            KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1] = (KVBSpeedPostSpeedType.P, odometer);
                        }
                    }
                    else
                    {
                        if (speedPost.IsWarning)
                        {
                            KVBSpeedPostSpeedType speed = SpeedToKvbSpeed(speedPost.SpeedLimitMpS);

                            OdoMeter odometer = new OdoMeter(this);
                            odometer.Setup(NextSpeedPost.DistanceM);
                            odometer.Start();

                            KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1] = (speed, odometer);
                        }
                        else
                        {
                            KVBSpeedPostSpeedType speed = SpeedToKvbSpeed(speedPost.SpeedLimitMpS);

                            if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.GS1))
                            {
                                if (KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Speed == speed
                                    || (KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Speed == KVBSpeedPostSpeedType.P
                                        && speed == KVBSpeedPostSpeedType.V160))
                                {
                                    KVBSpeedPostAnnounceList.Remove(KVBSpeedPostSpeedCategory.GS1);
                                }
                            }

                            if (KVBSpeedPostExecutionList.ContainsKey(KVBSpeedPostSpeedCategory.GS1)
                                && KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.GS1] < speed)
                            {
                                OdoMeter odometer = new OdoMeter(this);
                                odometer.Setup(KVBTrainLengthM);
                                odometer.Start();

                                KVBSpeedPostPendingList[KVBSpeedPostSpeedCategory.GS1] = (speed, odometer);
                            }
                            else
                            {
                                KVBSpeedPostExecutionList[KVBSpeedPostSpeedCategory.GS1] = speed;
                            }
                        }
                    }
                }
            }
        }

        protected void UpdateKvbTest()
        {
            if (KVBTestTimer.Triggered)
            {
                KVBTestTimer.Stop();
                KVBEmergencyBraking = true;
            }
        }

        protected void UpdateKvbOverrideNf()
        {
            if (KVBOverrideNfOdometer.Triggered)
            {
                KVBOverrideNfOdometer.Stop();
                KVBOverrideNf = false;
            }
        }

        protected void UpdateKvbShunting()
        {
            if (KVBShuntingOdometer.Triggered)
            {
                KVBShuntingOdometer.Stop();

                if (ARMCAB)
                {
                    KVBMode = KVBModeType.HighSpeedLine;
                }
                else
                {
                    KVBMode = KVBModeType.ConventionalLine;
                }
            }
        }

        protected void UpdateKvbTargets()
        {
            foreach (var pending in KVBSpeedPostPendingList)
            {
                if (pending.Value.Odometer.Triggered)
                {
                    KVBSpeedPostExecutionList[pending.Key] = pending.Value.Speed;
                }
            }
            KVBSpeedPostPendingList = KVBSpeedPostPendingList.Where(x => !x.Value.Odometer.Triggered).ToDictionary(x => x.Key, x => x.Value);

            if (KVBSpeedPostExecutionList.ContainsKey(KVBSpeedPostSpeedCategory.G))
            {
                if (KVBTrainLengthOdometerFVL.Started)
                {
                    if (KVBTrainLengthOdometerFVL.Triggered)
                    {
                        KVBSpeedPostExecutionList.Remove(KVBSpeedPostSpeedCategory.G);
                        KVBTrainLengthOdometerFVL.Stop();
                    }
                }
                else
                {
                    if (!NextInfoSignal.TextAspect?.Contains("KVB_FVL") ?? false)
                    {
                        float nextDivergingSwitchDistanceM = NextDivergingSwitchDistanceM(500f);
                        float nextTrailingDivergingSwitchDistanceM = NextTrailingDivergingSwitchDistanceM(500f);
                        if (nextDivergingSwitchDistanceM > 500f
                            && nextTrailingDivergingSwitchDistanceM > 500f)
                        {
                            KVBTrainLengthOdometerFVL.Start();
                        }
                    }
                }
            }
        }

        protected void UpdateKvbProx()
        {
            // Update release speed
            if (KVBSignalTargetSpeed == KVBSignalTargetSpeedType.V0)
            {
                if (!KVBSignalTargetIsBufferStop) {
                    SignalFeatures stopTargetSignal = NextGenericSignalFeatures("NORMAL", 0, 250f);
                    List<string> stopTargetSignalAspect = stopTargetSignal.TextAspect.Split(' ').ToList();

                    // Proximity to a C aspect
                    if (KVBSignalTargetDistanceM <= 150f
                        && KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V30
                        && (stopTargetSignalAspect.Contains("FR_C_BAL")
                            || stopTargetSignalAspect.Contains("FR_CV"))
                        )
                    {
                        KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V10;
                    }
                }
                else
                {
                    // Proximity to a buffer stop
                    if (KVBSignalTargetDistanceM <= 200f && KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V30)
                    {
                        KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V10;
                    }
                }
            }
        }

        protected void UpdateKvbPreAnnounce()
        {
            if (KVBTrainSpeedLimitMpS <= MpS.FromKpH(160f))
            {
                KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
                KVBPreAnnounceTABP = KVBPreAnnounceType.Deactivated;
            }
            else
            {
                if (KVBTrainLengthOdometerVLCLI.Triggered)
                {
                    KVBPreAnnounceVLCLI = KVBPreAnnounceType.Armed;
                    KVBTrainLengthOdometerVLCLI.Stop();
                }

                switch (KVBPreAnnounceTABP)
                {
                    case KVBPreAnnounceType.Deactivated:
                        if (KVBCLTV
                            && KVBCTABP
                            && (KVBSpeedPostExecutionList.Values.Select(x => (float?)KvbSpeedToSpeed(x)).Min() ?? 0f) > MpS.FromKpH(160f)
                            && (KVBSpeedPostAnnounceList.Values.Select(x => (float?)KvbSpeedToSpeed(x.Speed)).Min() ?? MpS.FromKpH(220f)) > MpS.FromKpH(160f))
                        {
                            KVBPreAnnounceTABP = KVBPreAnnounceType.Armed;
                        }
                        break;

                    case KVBPreAnnounceType.Armed:
                        if (KVBSpeedPostAnnounceList.ContainsKey(KVBSpeedPostSpeedCategory.GS1)
                            && KVBSpeedPostAnnounceList[KVBSpeedPostSpeedCategory.GS1].Speed == KVBSpeedPostSpeedType.P)
                        {
                            KVBPreAnnounceTABP = KVBPreAnnounceType.Triggered;
                            if (KVBPrincipalDisplayState == KVBPrincipalDisplayStateType.b)
                            {
                                TriggerKvbBipSound(0.5f);
                            }
                        }
                        break;

                    case KVBPreAnnounceType.Triggered:
                        if ((KVBSpeedPostExecutionList.Values.Select(x => (float?)KvbSpeedToSpeed(x)).Min() ?? 0f) <= MpS.FromKpH(160f)
                            || (KVBSpeedPostAnnounceList.Values
                            .Where(x => x.Speed != KVBSpeedPostSpeedType.P)
                            .Select(x => (float?)KvbSpeedToSpeed(x.Speed)).Min() ?? MpS.FromKpH(220f)) <= MpS.FromKpH(160f))
                        {
                            KVBPreAnnounceTABP = KVBPreAnnounceType.Deactivated;
                        }
                        else if ((KVBSpeedPostExecutionList.Values.Select(x => (float?)KvbSpeedToSpeed(x)).Min() ?? 0f) > MpS.FromKpH(160f)
                            && (KVBSpeedPostAnnounceList.Values.Select(x => (float?)KvbSpeedToSpeed(x.Speed)).Min() ?? MpS.FromKpH(220f)) > MpS.FromKpH(160f))
                        {
                            KVBPreAnnounceTABP = KVBPreAnnounceType.Armed;
                        }
                        break;
                }
            }
        }

        protected void UpdateKvbSpeedControl()
        {
            if (KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V10)
            {
                KVBSignalTargetAlertSpeedMpS = MpS.FromKpH(2.5f);
                KVBSignalTargetEBSpeedMpS = MpS.FromKpH(5f);
                KVBSignalTargetReleaseSpeedMpS = MpS.FromKpH(10f);
            }
            else
            {
                KVBSignalTargetAlertSpeedMpS = MpS.FromKpH(5f);
                KVBSignalTargetEBSpeedMpS = MpS.FromKpH(10f);
                KVBSignalTargetReleaseSpeedMpS = MpS.FromKpH(30f);
            }

            bool alert = false;
            bool emergency = false;
            KVBSpeedTooHighLight = false;

            // Train speed limit
            if (KVBMode == KVBModeType.Shunting || !KVBParametersValidation)
            {
                KVBTrainSpeedLimitAlert = SpeedMpS() > MpS.FromKpH(35f);
                KVBTrainSpeedLimitEmergency = SpeedMpS() > MpS.FromKpH(40f);
            }
            else
            {
                KVBTrainSpeedLimitAlert = SpeedMpS() > KVBTrainSpeedLimitMpS + MpS.FromKpH(5f);
                KVBTrainSpeedLimitEmergency = SpeedMpS() > KVBTrainSpeedLimitMpS + MpS.FromKpH(10f);
            }
            alert |= KVBTrainSpeedLimitAlert;
            emergency |= KVBTrainSpeedLimitEmergency;
            KVBSpeedTooHighLight |= KVBTrainSpeedLimitAlert;

            // On sight
            if (KVBOnSight)
            {
                KVBOnSightAlert = SpeedMpS() > MpS.FromKpH(30f) + MpS.FromKpH(5f);
                KVBOnSightEmergency = SpeedMpS() > MpS.FromKpH(30f) + MpS.FromKpH(10f);
                alert |= KVBOnSightAlert;
                emergency |= KVBOnSightEmergency;
            }
            else
            {
                KVBOnSightAlert = false;
                KVBOnSightEmergency = false;
            }

            // Signal target speed
            switch (KVBSignalTargetSpeed)
            {
                case KVBSignalTargetSpeedType.V0:
                    KVBSignalTargetSpeedAlert = CheckKvbSpeedCurve(
                        KVBSignalTargetDistanceM,
                        0f,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS + KVBDelayBeforeEmergencyBrakingS,
                        KVBSignalTargetAlertSpeedMpS,
                        KVBSignalTargetReleaseSpeedMpS);
                    KVBSignalTargetSpeedEmergency = CheckKvbSpeedCurve(
                        KVBSignalTargetDistanceM,
                        0f,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS,
                        KVBSignalTargetEBSpeedMpS,
                        KVBSignalTargetReleaseSpeedMpS);
                    KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(5f);
                    KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(10f);
                    break;

                case KVBSignalTargetSpeedType.V160:
                    if (KVBPreAnnounceVLCLI == KVBPreAnnounceType.Triggered)
                    {
                        KVBSignalTargetSpeedAlert = CheckKvbSpeedCurve(
                            KVBPreAnnounceOdometerVLCLI.RemainingValue,
                            160f,
                            KVBDeclivity,
                            KVBDelayBeforeBrakingEstablishedS + KVBDelayBeforeEmergencyBrakingS,
                            KVBSignalTargetAlertSpeedMpS,
                            160f);
                        KVBSignalTargetSpeedEmergency = CheckKvbSpeedCurve(
                            KVBPreAnnounceOdometerVLCLI.RemainingValue,
                            160f,
                            KVBDeclivity,
                            KVBDelayBeforeBrakingEstablishedS,
                            KVBSignalTargetEBSpeedMpS,
                            160f);
                        KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(220f) + MpS.FromKpH(5f);
                        KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(220f) + MpS.FromKpH(10f);
                    }
                    else
                    {
                        KVBSignalTargetSpeedAlert = false;
                        KVBSignalTargetSpeedEmergency = false;
                        KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(5f);
                        KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(10f);
                    }
                    break;

                case KVBSignalTargetSpeedType.VM:
                    KVBSignalTargetSpeedAlert = false;
                    KVBSignalTargetSpeedEmergency = false;
                    KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(5f);
                    KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(10f);
                    break;

                case KVBSignalTargetSpeedType.VMb:
                    if (KVBPreAnnounceVLCLI == KVBPreAnnounceType.Armed)
                    {
                        KVBSignalTargetSpeedAlert = false;
                        KVBSignalTargetSpeedEmergency = false;
                        KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(220f) + MpS.FromKpH(5f);
                        KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(220f) + MpS.FromKpH(10f);
                    }
                    else
                    {
                        KVBSignalTargetSpeedAlert = false;
                        KVBSignalTargetSpeedEmergency = false;
                        KVBSignalExecutionSpeedAlert = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(5f);
                        KVBSignalExecutionSpeedEmergency = SpeedMpS() > MpS.FromKpH(160f) + MpS.FromKpH(10f);
                    }
                    break;
            }

            alert |= KVBSignalTargetSpeedAlert || KVBSignalExecutionSpeedAlert;
            emergency |= KVBSignalTargetSpeedEmergency || KVBSignalExecutionSpeedEmergency;

            // Speed post execution speed
            float speedPostExecutionSpeedMpS = KVBSpeedPostExecutionList.Values.Select(x => (float?) KvbSpeedToSpeed(x)).Min() ?? float.PositiveInfinity;
            KVBSpeedPostExecutionSpeedAlert = SpeedMpS() > speedPostExecutionSpeedMpS + MpS.FromKpH(5f);
            KVBSpeedPostExecutionSpeedEmergency = SpeedMpS() > speedPostExecutionSpeedMpS + MpS.FromKpH(10f);

            alert |= KVBSpeedPostExecutionSpeedAlert;
            emergency |= KVBSpeedPostExecutionSpeedEmergency;
            KVBSpeedTooHighLight |= KVBSpeedPostExecutionSpeedAlert;

            // Speed post target speed
            KVBSpeedPostAnnounceSpeedAlert = false;
            KVBSpeedPostAnnounceSpeedEmergency = false;
            KVBSpeedPostPreAnnounceAlert = false;
            KVBSpeedPostPreAnnounceEmergency = false;
            foreach (var announce in KVBSpeedPostAnnounceList)
            {
                float speedMpS = KVBTrainSpeedLimitMpS;
                float distanceM = announce.Value.Odometer.RemainingValue;

                if (announce.Value.Speed.ToString().StartsWith("V"))
                {
                    speedMpS = KvbSpeedToSpeed(announce.Value.Speed);

                    KVBSpeedPostAnnounceSpeedAlert |= CheckKvbSpeedCurve(
                        distanceM,
                        speedMpS,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS + KVBDelayBeforeEmergencyBrakingS,
                        MpS.FromKpH(5f),
                        speedMpS);

                    KVBSpeedPostAnnounceSpeedEmergency |= CheckKvbSpeedCurve(
                        distanceM,
                        speedMpS,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS,
                        MpS.FromKpH(10f),
                        speedMpS);
                }
                else if (announce.Value.Speed == KVBSpeedPostSpeedType.P)
                {
                    KVBSpeedPostPreAnnounceAlert |= CheckKvbSpeedCurve(
                        distanceM,
                        160f,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS + KVBDelayBeforeEmergencyBrakingS,
                        MpS.FromKpH(5f),
                        160f);

                    KVBSpeedPostPreAnnounceEmergency |= CheckKvbSpeedCurve(
                        distanceM,
                        160f,
                        KVBDeclivity,
                        KVBDelayBeforeBrakingEstablishedS,
                        MpS.FromKpH(10f),
                        160f);
                }
            }

            alert |= KVBSpeedPostAnnounceSpeedAlert || KVBSpeedPostPreAnnounceAlert;
            emergency |= KVBSpeedPostAnnounceSpeedEmergency || KVBSpeedPostPreAnnounceEmergency;

            switch (KVBState)
            {
                case KVBStateType.Normal:
                    if (alert)
                    {
                        TriggerKvbTutSound(2.5f);
                        KVBState = KVBStateType.Alert;
                        Message(ConfirmLevel.Warning, "KVB : Survitesse / Overspeed");
                    }
                    break;

                case KVBStateType.Alert:
                    if (!alert)
                    {
                        KVBState = KVBStateType.Normal;
                    }
                    else if (emergency)
                    {
                        TriggerKvbTutSound(2.0f);
                        KVBState = KVBStateType.Emergency;
                        Message(ConfirmLevel.Warning, "KVB : Survitesse / Overspeed");
                        Message(ConfirmLevel.Warning, "SOS KVB");
                    }
                    break;

                case KVBStateType.Emergency:
                    if (SpeedMpS() < 0.1f)
                    {
                        KVBState = KVBStateType.Normal;
                    }
                    break;
            }

            KVBOverspeedEmergency = KVBState == KVBStateType.Emergency;
        }

        protected void UpdateKvbEmergencyBraking()
        {
            if (KVBOverspeedEmergency && SpeedMpS() < 0.1f)
            {
                KVBOverspeedEmergency = false;
            }


            if (!KVBEmergencyBraking)
            {
                if (KVBSpadEmergency || KVBOverspeedEmergency)
                {
                    KVBEmergencyBraking = true;

                    KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
                }
            }
            else
            {
                if (!KVBSpadEmergency && !KVBOverspeedEmergency && RearmingButton)
                {
                    KVBEmergencyBraking = false;
                }
            }
        }

        protected void UpdateKvbDisplay()
        {
            SetOverspeedWarningDisplay(KVBState >= KVBStateType.Alert);

            // Legacy display
            if (KVBMode != KVBModeType.HighSpeedLine)
            {
                if (KVBPreAnnounce == KVBPreAnnounceType.Armed)
                {
                    SetNextSignalAspect(Aspect.Clear_2);
                }
                else if (KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V10)
                {
                    SetNextSignalAspect(Aspect.Stop);
                }
                else
                {
                    SetNextSignalAspect(Aspect.Clear_1);
                }
            }

            // New display
            switch (KVBMode)
            {
                case KVBModeType.Off:
                    KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                    KVBPrincipalDisplayBlinking = false;
                    KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                    KVBVALButtonLight = false;
                    KVBVALButtonLightBlinking = false;
                    KVBMVButtonLight = false;
                    KVBFCButtonLight = false;
                    KVBFCButtonLightBlinking = false;
                    break;

                case KVBModeType.Init:
                    if (KVBInitTimer.RemainingValue <= 8f && KVBInitTimer.RemainingValue > 6f)
                    {
                        KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.VersionPA;
                        KVBPrincipalDisplayBlinking = false;
                        KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.PA;
                    }
                    else if (KVBInitTimer.RemainingValue <= 5.3f && KVBInitTimer.RemainingValue > 3.3f)
                    {
                        KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.VersionUC;
                        KVBPrincipalDisplayBlinking = false;
                        KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.UC;
                    }
                    else if (KVBInitTimer.RemainingValue <= 3.3f && KVBInitTimer.RemainingValue > 0.3f)
                    {
                        KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Test;
                        KVBPrincipalDisplayBlinking = false;
                        KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Test;
                        KVBVALButtonLight = true;
                        KVBVALButtonLightBlinking = true;
                        KVBMVButtonLight = true;
                        KVBFCButtonLight = true;
                        KVBFCButtonLightBlinking = false;
                    }
                    else
                    {
                        KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                        KVBPrincipalDisplayBlinking = false;
                        KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                        KVBVALButtonLight = false;
                        KVBVALButtonLightBlinking = false;
                        KVBMVButtonLight = false;
                        KVBFCButtonLight = false;
                        KVBFCButtonLightBlinking = false;
                    }
                    break;

                case KVBModeType.HighSpeedLine:
                    KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                    KVBPrincipalDisplayBlinking = false;
                    KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                    KVBVALButtonLight = false;
                    KVBVALButtonLightBlinking = false;
                    KVBMVButtonLight = false;
                    KVBFCButtonLight = TVMCOVITSpadEmergency || KVBOverrideNf;
                    KVBFCButtonLightBlinking = TVMCOVITSpadEmergency && !KVBOverrideNf;
                    break;

                case KVBModeType.ConventionalLine:
                    if (KVBTestTimer.Started)
                    {
                        KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Test;
                        KVBPrincipalDisplayBlinking = false;
                        KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Test;
                        KVBVALButtonLight = true;
                        KVBVALButtonLightBlinking = false;
                        KVBMVButtonLight = true;
                        KVBFCButtonLight = true;
                        KVBFCButtonLightBlinking = false;
                    }
                    else
                    {
                        KVBVALButtonLight = !KVBParametersValidation;
                        KVBVALButtonLightBlinking = !KVBParametersValidation;
                        KVBMVButtonLight = false;
                        KVBFCButtonLight = KVBSpadEmergency || KVBOverrideNf;
                        KVBFCButtonLightBlinking = KVBSpadEmergency && !KVBOverrideNf;

                        if (KVBVersion != KVBVersionType.V5 && KVBEmergencyBrakeLight)
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.FU;
                            KVBPrincipalDisplayBlinking = false;
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                        }
                        else if (ARMCAB)
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                            KVBPrincipalDisplayBlinking = false;
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                        }
                        else if (KVBOnSight)
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.V00;
                            if (KVBOnSightAlert)
                            {
                                KVBPrincipalDisplayBlinking = true;
                            }
                            else
                            {
                                KVBPrincipalDisplayBlinking = false;
                            }

                            if (KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V10)
                            {
                                KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.V000;
                            }
                            else
                            {
                                KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.V00;
                            }
                        }
                        else if (KVBSignalTargetSpeed == KVBSignalTargetSpeedType.V0
                            || KVBSignalTargetIsBufferStop)
                        {
                            if (KVBSignalTargetSpeedAlert || KVBSpeedPostAnnounceSpeedAlert)
                            {
                                KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.V00;
                                KVBPrincipalDisplayBlinking = true;
                            }
                            else if (CheckKvbSpeedCurveBelowReleaseSpeed(
                                KVBSignalTargetDistanceM,
                                0f,
                                KVBDeclivity,
                                KVBDelayBeforeBrakingEstablishedS + KVBDelayBeforeEmergencyBrakingS,
                                KVBSignalTargetAlertSpeedMpS,
                                KVBSignalTargetReleaseSpeedMpS))
                            {
                                KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.V00;
                                KVBPrincipalDisplayBlinking = false;
                            }
                            else
                            {
                                KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                                KVBPrincipalDisplayBlinking = false;
                            }

                            if (KVBSignalTargetReleaseSpeed == KVBReleaseSpeed.V10)
                            {
                                KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.V000;
                            }
                            else
                            {
                                KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.V00;
                            }
                        }
                        else if (!KVBCLTV
                            || (KVBSpeedPostAnnounceList.Count() == 0 && KVBSpeedPostExecutionList.Count() == 0)
                            || KVBSignalField == KVBSignalFieldType.Unknown)
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                            KVBPrincipalDisplayBlinking = false;
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                        }
                        else if (KVBPreAnnounce == KVBPreAnnounceType.Triggered)
                        {
                            if (KVBSignalTargetSpeedAlert || KVBSpeedPostAnnounceSpeedAlert)
                            {
                                KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.p;
                                KVBPrincipalDisplayBlinking = true;
                            }
                            else
                            {
                                KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                                KVBPrincipalDisplayBlinking = false;
                            }
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.p;
                        }
                        else if (KVBPreAnnounce == KVBPreAnnounceType.Armed)
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.b;
                            KVBPrincipalDisplayBlinking = KVBSpeedPostAnnounceSpeedAlert;
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                        }
                        else
                        {
                            KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Dashes3;
                            KVBPrincipalDisplayBlinking = KVBSpeedPostAnnounceSpeedAlert;
                            KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Dashes3;
                        }
                    }
                    break;

                case KVBModeType.Shunting:
                    KVBPrincipalDisplayState = KVBPrincipalDisplayStateType.Empty;
                    KVBPrincipalDisplayBlinking = false;
                    KVBAuxiliaryDisplayState = KVBAuxiliaryDisplayStateType.Empty;
                    KVBVALButtonLight = true;
                    KVBVALButtonLightBlinking = true;
                    KVBMVButtonLight = true;
                    KVBFCButtonLight = false;
                    KVBFCButtonLightBlinking = false;
                    break;
            }

            if (KVBPrincipalDisplayBlinking)
            {
                if (!KVBPrincipalDisplayBlinker.Started)
                {
                    KVBPrincipalDisplayBlinker.Start();
                }
            }
            else
            {
                if (KVBPrincipalDisplayBlinker.Started)
                {
                    KVBPrincipalDisplayBlinker.Stop();
                }
            }

            if (KVBVALButtonLight && KVBVALButtonLightBlinking)
            {
                if (!KVBVALButtonLightBlinker.Started)
                {
                    KVBVALButtonLightBlinker.Start();
                }
            }
            else
            {
                if (KVBVALButtonLightBlinker.Started)
                {
                    KVBVALButtonLightBlinker.Stop();
                }
            }

            if (KVBFCButtonLight && KVBFCButtonLightBlinking)
            {
                if (!KVBFCButtonLightBlinker.Started)
                {
                    KVBFCButtonLightBlinker.Start();
                }
            }
            else
            {
                if (KVBFCButtonLightBlinker.Started)
                {
                    KVBFCButtonLightBlinker.Stop();
                }
            }

            switch (KVBPrincipalDisplayState)
            {
                case KVBPrincipalDisplayStateType.Empty:
                    SetCabDisplayControl(KVB_Principal1, 0);
                    SetCabDisplayControl(KVB_Principal2, 0);
                    break;

                case KVBPrincipalDisplayStateType.FU:
                    SetCabDisplayControl(KVB_Principal1, 0);
                    SetCabDisplayControl(KVB_Principal2, 1);
                    break;

                case KVBPrincipalDisplayStateType.V000:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, KVBPrincipalDisplayBlinker.On ? 1 : 0);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 1);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    break;

                case KVBPrincipalDisplayStateType.V00:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, KVBPrincipalDisplayBlinker.On ? 2 : 0);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 2);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    break;

                case KVBPrincipalDisplayStateType.L:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, 0);
                        SetCabDisplayControl(KVB_Principal2, KVBPrincipalDisplayBlinker.On ? 4 : 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 0);
                        SetCabDisplayControl(KVB_Principal2, 4);
                    }
                    break;

                case KVBPrincipalDisplayStateType.b:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, KVBPrincipalDisplayBlinker.On ? 4 : 0);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 4);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    break;

                case KVBPrincipalDisplayStateType.p:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, KVBPrincipalDisplayBlinker.On ? 5 : 0);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 5);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    break;

                case KVBPrincipalDisplayStateType.Dashes3:
                    if (KVBPrincipalDisplayBlinking)
                    {
                        SetCabDisplayControl(KVB_Principal1, KVBPrincipalDisplayBlinker.On ? 6 : 0);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    else
                    {
                        SetCabDisplayControl(KVB_Principal1, 6);
                        SetCabDisplayControl(KVB_Principal2, 0);
                    }
                    break;

                case KVBPrincipalDisplayStateType.Dashes9:
                    SetCabDisplayControl(KVB_Principal1, 7);
                    SetCabDisplayControl(KVB_Principal2, 0);
                    break;

                case KVBPrincipalDisplayStateType.Test:
                    SetCabDisplayControl(KVB_Principal1, 0);
                    SetCabDisplayControl(KVB_Principal2, 7);
                    break;

                case KVBPrincipalDisplayStateType.VersionPA:
                    SetCabDisplayControl(KVB_Principal1, 0);
                    SetCabDisplayControl(KVB_Principal2, 2);
                    break;

                case KVBPrincipalDisplayStateType.VersionUC:
                    SetCabDisplayControl(KVB_Principal1, 0);
                    SetCabDisplayControl(KVB_Principal2, 3);
                    break;
            }


            switch (KVBAuxiliaryDisplayState)
            {
                case KVBAuxiliaryDisplayStateType.Empty:
                    SetCabDisplayControl(KVB_Auxiliary1, 0);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.V000:
                    SetCabDisplayControl(KVB_Auxiliary1, 1);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.V00:
                    SetCabDisplayControl(KVB_Auxiliary1, 2);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.L:
                    SetCabDisplayControl(KVB_Auxiliary1, 4);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.p:
                    SetCabDisplayControl(KVB_Auxiliary1, 5);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.Dashes3:
                    SetCabDisplayControl(KVB_Auxiliary1, 6);
                    SetCabDisplayControl(KVB_Auxiliary2, 0);
                    break;

                case KVBAuxiliaryDisplayStateType.Test:
                    SetCabDisplayControl(KVB_Auxiliary1, 0);
                    SetCabDisplayControl(KVB_Auxiliary2, 7);
                    break;

                case KVBAuxiliaryDisplayStateType.PA:
                    SetCabDisplayControl(KVB_Auxiliary1, 0);
                    SetCabDisplayControl(KVB_Auxiliary2, 2);
                    break;

                case KVBAuxiliaryDisplayStateType.UC:
                    SetCabDisplayControl(KVB_Auxiliary1, 0);
                    SetCabDisplayControl(KVB_Auxiliary2, 3);
                    break;
            }

            // VY SOS KVB
            SetCabDisplayControl(VY_SOS_KVB, KVBEmergencyBraking ? 1 : 0);

            // VY VTE
            SetCabDisplayControl(VY_VTE, KVBSpeedTooHighLight || KVBTestTimer.Started ? 1 : 0);

            // VY FU
            KVBEmergencyBrakeLight = KVBSpadEmergency || KVBOverspeedEmergency || KVBTestTimer.Started;
            SetCabDisplayControl(VY_FU, KVBEmergencyBrakeLight ? 1 : 0);

            // VY PE
            SetCabDisplayControl(VY_PE, KVBEngineFailure || KVBTestTimer.Started ? 1 : 0);

            // VY PS
            SetCabDisplayControl(VY_PS, KVBGroundFailureBlinker.On || KVBTestTimer.Started ? 1 : 0);

            // VY BP VAL
            SetCabDisplayControl(KVB_VY_BP_VAL, (KVBVALButtonLightBlinking ? KVBVALButtonLightBlinker.On : KVBVALButtonLight) ? 1 : 0);

            // VY BP MV
            SetCabDisplayControl(KVB_VY_BP_MV, KVBMVButtonLight ? 1 : 0);

            // VY BP FC
            SetCabDisplayControl(KVB_VY_BP_FC, (KVBFCButtonLightBlinking ? KVBFCButtonLightBlinker.On : KVBFCButtonLight) ? 1 : 0);
        }

        protected void TriggerKvbTutSound(float durationS)
        {
            if (!KVBTutTimer.Started)
            {
                KVBTutTimer.Setup(durationS);
                KVBTutTimer.Start();
                TriggerGenericSound(Event.TrainControlSystemPenalty1);
            }
        }

        protected void TriggerKvbBipSound(float durationS)
        {
            if (!KVBBipTimer.Started)
            {
                KVBBipTimer.Setup(durationS);
                KVBBipTimer.Start();
                TriggerGenericSound(Event.TrainControlSystemInfo1);
            }
        }

        protected void UpdateKvbSound()
        {
            if (KVBTutTimer.Triggered)
            {
                KVBTutTimer.Stop();
                TriggerGenericSound(Event.TrainControlSystemPenalty2);
            }

            if (KVBBipTimer.Triggered)
            {
                KVBBipTimer.Stop();
                TriggerGenericSound(Event.TrainControlSystemInfo2);
            }
        }

        protected bool CheckKvbSpeedCurve(float targetDistanceM, float targetSpeedMpS, float slope, float delayS, float marginMpS, float releaseSpeedMpS)
        {
            float speedCurveMpS =
                Math.Max(
                    SpeedCurve(
                        targetDistanceM,
                        targetSpeedMpS,
                        slope,
                        delayS,
                        KVBSafeDecelerationMpS2
                    ),
                    releaseSpeedMpS + marginMpS
                );

            return SpeedMpS() > speedCurveMpS;
        }

        protected bool CheckKvbSpeedCurveBelowReleaseSpeed(float targetDistanceM, float targetSpeedMpS, float slope, float delayS, float marginMpS, float releaseSpeedMpS)
        {
            float speedCurveMpS =
                SpeedCurve(
                    targetDistanceM,
                    targetSpeedMpS,
                    slope,
                    delayS,
                    KVBSafeDecelerationMpS2
                );

            return speedCurveMpS <= releaseSpeedMpS + marginMpS;
        }

        protected KVBSignalFieldType TextAspectToKvbSignalField(List<string> textAspect)
        {
            KVBSignalFieldType signalField = KVBSignalFieldType.Unknown;

            foreach (string part in textAspect)
            {
                switch (part)
                {
                    case "FR_C_BAL":
                    case "FR_C_BAPR":
                    case "FR_CV":
                        signalField = KVBSignalFieldType.C_BAL;
                        break;

                    case "FR_C_BM":
                    case "FR_S_BM":
                    case "CH_KVB_CHAMP_S_C_BM":
                        signalField = KVBSignalFieldType.S_BM;
                        break;

                    case "FR_S_BAL":
                    case "FR_S_BAPR":
                    case "FR_SCLI":
                    case "FR_MCLI":
                    case "FR_M":
                        signalField = KVBSignalFieldType.S_BAL;
                        break;

                    case "FR_A":
                    case "FR_RR_A":
                    case "FR_RRCLI_A":
                    case "CH_KVB_CHAMP_S_A":
                        signalField = KVBSignalFieldType.A;
                        break;

                    case "FR_ACLI":
                    case "FR_RR_ACLI":
                    case "FR_RRCLI_ACLI":
                    case "FR_RCLI_ACLI":
                        signalField = KVBSignalFieldType.ACLI;
                        break;

                    case "FR_VLCLI_ANN":
                    case "FR_VLCLI_EXE":
                        signalField = KVBSignalFieldType.VLCLI;
                        break;

                    case "FR_VL_INF":
                    case "CH_KVB_CHAMP_S_VL_INF":
                        signalField = KVBSignalFieldType.VL_INF;
                        break;

                    case "FR_VL_SUP":
                        signalField = KVBSignalFieldType.VL_SUP;
                        break;
                }

                if (signalField != KVBSignalFieldType.Unknown)
                {
                    break;
                }
            }

            return signalField;
        }

        protected KVBSignalExecutionSpeedType KvbSignalFieldToKvbSignalExecutionSpeed(KVBSignalFieldType signalField)
        {
            KVBSignalExecutionSpeedType executionSpeed = KVBSignalExecutionSpeedType.None;

            switch (signalField)
            {
                case KVBSignalFieldType.C_BAL:
                    executionSpeed = KVBSignalExecutionSpeedType.A;
                    break;

                case KVBSignalFieldType.S_BM:
                    executionSpeed = KVBSignalExecutionSpeedType.B;
                    break;

                case KVBSignalFieldType.S_BAL:
                    executionSpeed = KVBSignalExecutionSpeedType.C;
                    break;

                case KVBSignalFieldType.A:
                case KVBSignalFieldType.ACLI:
                case KVBSignalFieldType.VLCLI:
                case KVBSignalFieldType.VL_INF:
                    executionSpeed = KVBSignalExecutionSpeedType.VM;
                    break;

                case KVBSignalFieldType.VL_SUP:
                    executionSpeed = KVBSignalExecutionSpeedType.VMb;
                    break;
            }

            return executionSpeed;
        }

        protected KVBSignalTargetSpeedType KvbSignalFieldToKvbSignalTargetSpeed(KVBSignalFieldType signalField)
        {
            KVBSignalTargetSpeedType executionSpeed = KVBSignalTargetSpeedType.None;

            switch (signalField)
            {
                case KVBSignalFieldType.C_BAL:
                case KVBSignalFieldType.S_BAL:
                case KVBSignalFieldType.A:
                case KVBSignalFieldType.ACLI:
                    executionSpeed = KVBSignalTargetSpeedType.V0;
                    break;

                case KVBSignalFieldType.VLCLI:
                    executionSpeed = KVBSignalTargetSpeedType.V160;
                    break;

                case KVBSignalFieldType.S_BM:
                case KVBSignalFieldType.VL_INF:
                    executionSpeed = KVBSignalTargetSpeedType.VM;
                    break;

                case KVBSignalFieldType.VL_SUP:
                    executionSpeed = KVBSignalTargetSpeedType.VMb;
                    break;
            }

            return executionSpeed;
        }

        protected KVBSpeedPostSpeedType SpeedToKvbSpeed(float speedMpS)
        {
            int speedKpH = (int) Math.Round(MpS.ToKpH(speedMpS));

            if (speedKpH <= 140 && speedKpH % 5 > 0)
            {
                speedKpH = ((speedKpH / 5) + 1) * 5;
            }
            else if (speedKpH > 140 && speedKpH % 10 > 0)
            {
                speedKpH = ((speedKpH / 10) + 1) * 10;
            }

            string speedText = "V" + speedKpH.ToString();
            KVBSpeedPostSpeedType speed;
            Enum.TryParse(speedText, false, out speed);
            return speed;
        }

        protected float KvbSpeedToSpeed(KVBSpeedPostSpeedType speed)
        {
            float speedMpS = KVBTrainSpeedLimitMpS;

            if (speed.ToString().StartsWith("V"))
            {
                speedMpS = MpS.FromKpH(float.Parse(speed.ToString().Substring(1)));
            }
            else if (speed == KVBSpeedPostSpeedType.P)
            {
                speedMpS = MpS.FromKpH(160f);
            }

            return speedMpS;
        }

        protected int FindNextSignalNumberWithAspect(Aspect aspect, string signalType, int maxSignals, float maxDistanceM)
        {
            int i;

            for (i = 0; i < maxSignals; i++)
            {
                SignalFeatures foundSignal = NextGenericSignalFeatures(signalType, i, maxDistanceM);

                if (foundSignal.Aspect == aspect)
                {
                    return i;
                }
                // Signal not found
                else if (foundSignal.DistanceM > maxDistanceM)
                {
                    break;
                }
            }

            return -1;
        }

        protected SignalFeatures FindNextSignalWithAspect(Aspect aspect, string signalType, int maxSignals, float maxDistanceM)
        {
            SignalFeatures signal = new SignalFeatures("", Aspect.None, float.MaxValue, -1f, float.MinValue, "");

            int i = FindNextSignalNumberWithAspect(aspect, signalType, maxSignals, maxDistanceM);

            if (i < maxSignals)
            {
                signal = NextGenericSignalFeatures(signalType, i, maxDistanceM);
            }

            return signal;
        }

        protected SignalFeatures FindNextSignalWithSpeedLimit(float speedKpH, string signalType, int maxSignals, float maxDistanceM)
        {
            SignalFeatures signal = new SignalFeatures("", Aspect.None, float.MaxValue, -1f, float.MinValue, "");

            for (int i = 0; i < maxSignals; i++)
            {
                SignalFeatures foundSignal = NextGenericSignalFeatures(signalType, i, maxDistanceM);

                if (Math.Round(MpS.ToKpH(foundSignal.SpeedLimitMpS)) == speedKpH)
                {
                    signal = foundSignal;
                    break;
                }
                // Signal not found
                else if (foundSignal.DistanceM > maxDistanceM)
                {
                    break;
                }
            }

            return signal;
        }

        protected void ResetKvbTargets(bool fullReset = false)
        {
            if (fullReset)
            {
                KVBCLTV = false;
                KVBCTABP = false;
            }

            KVBTrainLengthOdometerVLCLI.Stop();
            KVBTrainLengthOdometerFVL.Stop();

            KVBSignalField = KVBSignalFieldType.Unknown;
            KVBSignalExecutionSpeed = KVBSignalExecutionSpeedType.None;
            KVBSignalTargetSpeed = KVBSignalTargetSpeedType.None;
            KVBSignalTargetIsBufferStop = false;
            KVBSignalTargetReleaseSpeed = KVBReleaseSpeed.V30;
            KVBSignalTargetOdometer.Stop();
            KVBOnSight = false;
            KVBPreAnnounceVLCLI = KVBPreAnnounceType.Deactivated;
            KVBPreAnnounceTABP = KVBPreAnnounceType.Deactivated;

            KVBSpeedPostAnnounceList.Clear();
            KVBSpeedPostPendingList.Clear();
            KVBSpeedPostExecutionList.Clear();

            KVBTrainSpeedLimitAlert = false;
            KVBTrainSpeedLimitEmergency = false;
            KVBOnSightAlert = false;
            KVBOnSightEmergency = false;
            KVBSignalTargetSpeedAlert = false;
            KVBSignalTargetSpeedEmergency = false;
            KVBSignalExecutionSpeedAlert = false;
            KVBSignalExecutionSpeedEmergency = false;
            KVBSpeedPostExecutionSpeedAlert = false;
            KVBSpeedPostExecutionSpeedEmergency = false;
            KVBSpeedPostAnnounceSpeedAlert = false;
            KVBSpeedPostAnnounceSpeedEmergency = false;
            KVBSpeedPostPreAnnounceAlert = false;
            KVBSpeedPostPreAnnounceEmergency = false;

            KVBState = KVBStateType.Normal;
        }

        protected void UpdateTvm()
        {
            if ((TVM300Present || TVM430Present) && IsSpeedControlEnabled())
            {
                if (!ARMCAB && TVMArmed && DECAB)
                {
                    TVMArmed = false;
                    TVMMode = TVMModeType.None;
                    TVMCOVITEmergencyBraking = false;
                    TVM430AspectChangeTimer.Stop();
                }

                if (!TVMArmed && TVMManualArming)
                {
                    TVMArmed = true;
                    if (NextNormalSignal.TextAspect.Contains("FR_TVM430"))
                    {
                        TVMMode = TVMModeType.TVM430;
                    }
                    else if (NextNormalSignal.TextAspect.Contains("FR_TVM300"))
                    {
                        TVMMode = TVMModeType.TVM300;
                    }
                    ARMCAB = true;
                }
                else if (TVMArmed && TVMManualDearming)
                {
                    TVMArmed = false;
                    TVMMode = TVMModeType.None;
                    TVMCOVITEmergencyBraking = false;
                    TVM430AspectChangeTimer.Stop();
                    ARMCAB = false;
                }

                if (TVMArmed)
                {
                    CalculateTvmSequence();
                    DetermineTvmAspect();
                    UpdateTvmCovit();
                    UpdateTvmDisplay();
                    UpdateTvmSounds();

                    TVMAspectPreviousCycle = TVMAspectCurrent;
                    TVMBlinkingPreviousCycle = TVMBlinkingCurrent;
                }
                else
                {
                    TVMCOVITEmergencyBraking = false;

                    TVMAspectCommand = TVMAspectType.None;
                    TVMAspectCurrent = TVMAspectType.None;
                    TVMAspectPreviousCycle = TVMAspectType.None;
                    TVMBlinkingCommand = false;
                    TVMBlinkingCurrent = false;
                    TVMBlinkingPreviousCycle = false;

                    TVMStartControlSpeedMpS = 0f;
                    TVMEndControlSpeedMpS = 0f;
                    TVMDecelerationMpS2 = 0f;

                    UpdateTvmDisplay();
                }
            }
        }

        protected void OnSignalPassedTvm(SignalFeatures signal, SignalType signalType)
        {
            if ((TVM300Present || TVM430Present) && IsSpeedControlEnabled())
            {
                List<string> lastSignalAspect = signal.TextAspect?.Split(' ').ToList() ?? new List<string>();

                switch (signalType)
                {
                    case SignalType.Normal:
                        if (lastSignalAspect.Contains("EPI_ECS"))
                        {
                            TVMArmed = true;
                            TVMMode = TVMModeType.TVM300;
                            ARMCAB = true;
                        }
                        else if (lastSignalAspect.Contains("EPI_ECR")
                            || lastSignalAspect.Contains("EPI_EB"))
                        {
                            ARMCAB = false;
                        }
                        else if (lastSignalAspect.Contains("EPI_Nf"))
                        {
                            TVMCOVITSpadEmergency = true;
                        }
                        break;
                }
            }

            if (TVM430Present && IsSpeedControlEnabled())
            {
                List<string> lastSignalAspect = signal.TextAspect?.Split(' ').ToList() ?? new List<string>();

                switch (signalType)
                {
                    case SignalType.Normal:
                        if (lastSignalAspect.Contains("BSP_ECS"))
                        {
                            TVMArmed = true;
                            TVMMode = TVMModeType.TVM430;
                            ARMCAB = true;
                        }
                        else if (lastSignalAspect.Contains("BSP_C430"))
                        {
                            TVMArmed = true;
                            TVMMode = TVMModeType.TVM430;
                            ARMCAB = true;
                        }
                        else if (lastSignalAspect.Contains("BSP_C300"))
                        {
                            TVMArmed = true;
                            TVMMode = TVMModeType.TVM300;
                            ARMCAB = true;
                        }
                        else if (lastSignalAspect.Contains("BSP_ESL"))
                        {
                            ARMCAB = false;
                        }
                        else if (lastSignalAspect.Contains("BSP_CNf"))
                        {
                            TVMCOVITSpadEmergency = true;
                        }
                        break;
                }
            }
        }

        protected void CalculateTvmSequence()
        {
            int i;

            if (NormalSignalPassed)
            {
                PreviousSectionSpeed = SpeedSequence[0];
                PreviousSectionAspect = AspectSequence[0];
                PreviousVcond = Vcond[0];
            }

            int ignoreCount = 0;

            // Get the 10 next signals (from 10th to 1st in order to be optimal)
            for (i = TVMNumberOfBlockSections - 1; i >= 0; i--)
            {
                SpeedSequence[i] = Convert.ToInt32(MpS.ToKpH(NextSignalSpeedLimitMpS(i)));
                AspectSequence[i] = NextSignalAspect(i);

                if (SpeedSequence[i] <= 0f && AspectSequence[i] > Aspect.Stop)
                {
                    // Ignore
                    ignoreCount++;
                    i++;
                }
            }

            // Calculate execution speeds (Vcond)
            for (i = TVMNumberOfBlockSections - 1; i >= 0; i--)
            {
                Aspect currentAspect;
                int currentSpeed;

                if (i == 0)
                {
                    currentAspect = PreviousSectionAspect;
                    currentSpeed = PreviousSectionSpeed;
                }
                else
                {
                    currentAspect = AspectSequence[i - 1];
                    currentSpeed = SpeedSequence[i - 1];
                }

                Aspect nextAspect = AspectSequence[i];
                int nextSpeed = SpeedSequence[i];

                if (currentAspect == Aspect.Stop || (currentAspect == Aspect.StopAndProceed && TVMModel > TVMModelType.TVM300))
                {
                    Vcond[i] = TVMSpeedType._RRR;
                }
                else if (nextSpeed == 30 && currentSpeed == 30)
                {
                    Vcond[i] = TVMSpeedType._RRR;
                }
                else if (nextSpeed == 60 && currentSpeed == 60 && TVMModel > TVMModelType.TVM300)
                {
                    Vcond[i] = TVMSpeedType._60E;
                }
                else if (nextAspect == Aspect.Stop && (TVMModel > TVMModelType.TVM300 || currentSpeed == 80))
                {
                    Vcond[i] = TVMSpeedType._80E;
                }
                else if (nextSpeed == 80 && currentSpeed == 80)
                {
                    Vcond[i] = TVMSpeedType._80E;
                }
                else if (nextSpeed == 130 && currentSpeed == 130 && TVMModel > TVMModelType.TVM300)
                {
                    Vcond[i] = TVMSpeedType._130E;
                }
                else if (nextSpeed == 130 && currentSpeed == 170) // 130 kph HSL exit
                {
                    Vcond[i] = TVMSpeedType._130E;
                }
                else if (nextSpeed == 160 && currentSpeed == 160)
                {
                    Vcond[i] = TVMSpeedType._160E;
                }
                else if (nextSpeed == 160 && currentSpeed == 170) // 160 kph HSL exit
                {
                    Vcond[i] = TVMSpeedType._160E;
                }
                else if (TVMModel == TVMModelType.TVM300 && nextAspect == Aspect.StopAndProceed) // MSTS TVM300 160E
                {
                    Vcond[i] = TVMSpeedType._160E;
                }
                else if (nextSpeed == 170 && currentSpeed == 170f && TVMModel > TVMModelType.TVM300)
                {
                    Vcond[i] = TVMSpeedType._170E;
                }
                else if (nextSpeed == 200 && currentSpeed == 200 && TVMModel > TVMModelType.TVM300)
                {
                    Vcond[i] = TVMSpeedType._200V;
                }
                else if (nextSpeed == 220 && currentSpeed == 220)
                {
                    // TODO : 220V not available currently
                    Vcond[i] = TVMSpeedType._220E;
                }
                else if (nextSpeed == 220 && currentSpeed == 230) // 220 kph HSL exit
                {
                    // TODO : 220V not available currently
                    Vcond[i] = TVMSpeedType._220E;
                }
                else if (TVMModel == TVMModelType.TVM300 && nextAspect == Aspect.Approach_1) // MSTS TVM300 220E
                {
                    Vcond[i] = TVMSpeedType._220E;
                }
                else if (nextSpeed == 230 && currentSpeed == 230 && TVMModel > TVMModelType.TVM300)
                {
                    // TODO : 230V not available currently
                    Vcond[i] = TVMSpeedType._230E;
                }
                else if (nextSpeed == 270 && currentSpeed == 270)
                {
                    Vcond[i] = TVMSpeedType._270V;
                }
                else if (nextSpeed == 300 && currentSpeed == 300 || TVMModel == TVMModelType.TVM300)
                {
                    Vcond[i] = TVMSpeedType._300V;
                }
                else
                {
                    Vcond[i] = TVMSpeedType._320V;
                }
            }

            for (i = 0; i < TVMNumberOfBlockSections - 1; i++)
            {
                TVMSpeedType previousVcond;

                if (i == 0)
                {
                    previousVcond = PreviousVcond;
                }
                else
                {
                    previousVcond = Vcond[i - 1];
                }

                TVMSpeedType currentVcond = Vcond[i];
                TVMSpeedType nextVcond = Vcond[i + 1];

                if (currentVcond == TVMSpeedType._300V && TVMModel == TVMModelType.TVM300 || currentVcond == TVMSpeedType._320V)
                {
                    if (previousVcond == TVMSpeedType._230E || previousVcond == TVMSpeedType._270V)
                    {
                        if (nextVcond == TVMSpeedType._270V)
                        {
                            Vcond[i] = TVMSpeedType._270V;
                        }
                        else if (nextVcond == TVMSpeedType._300V)
                        {
                            Vcond[i] = TVMSpeedType._300V;
                        }
                    }
                }
            }

            Dictionary<TVMSpeedType, TVMSpeedType> TAB1 = TVMModel == TVMModelType.TVM300 ? TVM300Tab1 : TVM430SncfTab1;
            Dictionary<TVMSpeedType, TVMSpeedType> TAB2 = TVMModel == TVMModelType.TVM300 ? TVM300Tab2 : TVM430SncfTab2;

            // Calculate Vc, Ve, Va

            // Initialize last block section
            i = TVMNumberOfBlockSections - 1;
            Vc[i] = Vcond[i];
            Ve[i] = Min(TAB2[Vcond[i]], TAB1[Vc[i]]);
            Va[i] = TAB2[Vcond[i]];

            // Calculate the sequence
            for (i = TVMNumberOfBlockSections - 1; i > 0; i--)
            {
                Vc[i-1] = Min(Vcond[i-1], Ve[i]);
                Ve[i-1] = Min(TAB2[Vcond[i-1]], TAB1[Vc[i-1]]);
                Va[i-1] = TAB2[Vc[i]];
            }
        }

        protected void DetermineTvmAspect()
        {
            switch (TVMModel)
            {
                case TVMModelType.TVM300:
                    DetermineTvm300Aspect();
                    break;

                case TVMModelType.TVM430_V300:
                case TVMModelType.TVM430_V320:
                    DetermineTvm430Aspect();
                    break;
            }
        }

        protected void DetermineTvm300Aspect()
        {
            Tuple<TVMAspectType, bool, float> onBoardValues;

            Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType> triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>(Ve[0], Vc[0], Va[0]);

            if (TVM300DecodingTable.ContainsKey(triplet))
            {
                onBoardValues = TVM300DecodingTable[triplet];
            }
            else
            {
                triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>(Ve[0], Vc[0], TVMSpeedType.Any);

                if (TVM300DecodingTable.ContainsKey(triplet))
                {
                    onBoardValues = TVM300DecodingTable[triplet];
                }
                else
                {
                    onBoardValues = new Tuple<TVMAspectType, bool, float>(TVMAspectType._RRR, false, 35f);
                }
            }

            TVMAspectCommand = onBoardValues.Item1;
            TVMBlinkingCommand = onBoardValues.Item2;
            TVMStartControlSpeedMpS = TVMEndControlSpeedMpS = MpS.FromKpH(onBoardValues.Item3);
            TVMDecelerationMpS2 = 0f;
        }

        protected void DetermineTvm430Aspect()
        {
            Tuple<TVMAspectType, bool, float, float, float> onBoardValues;

            Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType> triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>(Ve[0], Vc[0], Va[0]);

            if (TVM430DecodingTable.ContainsKey(triplet))
            {
                onBoardValues = TVM430DecodingTable[triplet];
            }
            else
            {
                triplet = new Tuple<TVMSpeedType, TVMSpeedType, TVMSpeedType>(Ve[0], Vc[0], TVMSpeedType.Any);

                if (TVM430DecodingTable.ContainsKey(triplet))
                {
                    onBoardValues = TVM430DecodingTable[triplet];
                }
                else
                {
                    onBoardValues = new Tuple<TVMAspectType, bool, float, float, float>(TVMAspectType._RRR, false, 35f, 35f, 0f);
                }
            }
            
            if ((TVMAspectCommand != onBoardValues.Item1 || TVMBlinkingCommand != onBoardValues.Item2) && !TVM430AspectChangeTimer.Started)
            {
                TVMAspectCommand = onBoardValues.Item1;
                TVMBlinkingCommand = onBoardValues.Item2;
                TVMStartControlSpeedMpS = MpS.FromKpH(onBoardValues.Item3);
                TVMEndControlSpeedMpS = MpS.FromKpH(onBoardValues.Item4);
                TVMDecelerationMpS2 = onBoardValues.Item5;
            }
        }

        protected void UpdateTvmCovit()
        {
            switch (TVMMode)
            {
                case TVMModeType.TVM300:
                    UpdateTvm300Covit();
                    break;

                case TVMModeType.TVM430:
                    UpdateTvm430Covit();
                    break;

                default:
                    // Conventional line
                    // Since there is no TVM signal, use the simpler function.
                    UpdateTvm300Covit();
                    break;
            }
        }

        protected void UpdateTvm300Covit()
        {
            if (TVMCOVITInhibited)
            {
                TVMCOVITEmergencyBraking = false;
            }
            else
            {
                SetCurrentSpeedLimitMpS(TVMStartControlSpeedMpS);
                SetNextSpeedLimitMpS(TVMEndControlSpeedMpS);

                TVMCOVITEmergencyBraking = TVMCOVITSpadEmergency || SpeedMpS() > TVMStartControlSpeedMpS;
            }
        }

        protected void UpdateTvm430Covit()
        {
            if (TVMCOVITInhibited)
            {
                TVMCOVITEmergencyBraking = false;
            }
            else
            {
                SetCurrentSpeedLimitMpS(TVMStartControlSpeedMpS);
                SetNextSpeedLimitMpS(TVMEndControlSpeedMpS);

                float SpeedCurveMpS = Math.Min(
                    SpeedCurve(
                        NextSignalDistanceM(0),
                        TVMEndControlSpeedMpS,
                        0,
                        0,
                        TVMDecelerationMpS2
                    ),
                    TVMStartControlSpeedMpS
                );

                TVMCOVITEmergencyBraking = TVMCOVITSpadEmergency || SpeedMpS() > SpeedCurveMpS;
            }
        }

        protected void UpdateTvmDisplay()
        {
            switch (TVMModel)
            {
                case TVMModelType.TVM300:
                    UpdateTvm300Display();
                    break;

                case TVMModelType.TVM430_V300:
                case TVMModelType.TVM430_V320:
                    UpdateTvm430Display();
                    break;
            }
        }

        protected void UpdateTvm300Display()
        {
            UpdateTvmCabSignal(TVMAspectCommand, TVMBlinkingCommand, TVMAspectCommand != TVMAspectPreviousCycle);

            SetCabDisplayControl(TVM_Mask, TVMAspectCommand == TVMAspectType.None ? 0 : 1);

            SetCabDisplayControl(VY_CV, TVMCOVITEmergencyBraking || KarmEmergencyBraking ? 1 : 0);
            SetCabDisplayControl(VY_SECT, TVMOpenCircuitBreaker ? 1 : 0);
            SetCabDisplayControl(VY_SECT_AU, 0);
            SetCabDisplayControl(VY_BPT, TVMLowerPantograph ? 1 : 0);

            // Legacy
            Aspect aspect = TVM300MstsTranslation[TVMAspectCommand];
            SetNextSignalAspect(aspect);
        }

        protected void UpdateTvm430Display()
        {
            UpdateTvmCabSignal(TVMAspectCurrent, TVMBlinkingCurrent, false);

            if (TVMAspectCommand != TVMAspectCurrent || TVMBlinkingCommand != TVMBlinkingCurrent)
            {
                if (!TVM430AspectChangeTimer.Started)
                {
                    TVM430AspectChangeTimer.Start();
                }
                else
                {
                    if (TVM430AspectChangeTimer.Triggered)
                    {
                        UpdateTvmCabSignal(TVMAspectCommand, TVMBlinkingCommand, TVMAspectCommand != TVMAspectCurrent);

                        TVM430AspectChangeTimer.Stop();
                    }
                }
            }

            SetCabDisplayControl(TVM_Mask, TVMAspectCommand == TVMAspectType.None ? 0 : 1);

            SetCabDisplayControl(VY_CV, TVMCOVITEmergencyBraking || KarmEmergencyBraking ? 1 : 0);
            SetCabDisplayControl(VY_SECT, TVMOpenCircuitBreaker ? 1 : 0);
            SetCabDisplayControl(VY_SECT_AU, TVMOpenCircuitBreakerAutomatic ? 1 : 0);
            SetCabDisplayControl(VY_BPT, TVMLowerPantograph ? 1 : 0);

            // Legacy
            Aspect aspect;
            if (TVM430TrainSpeedLimitMpS <= MpS.FromKpH(300f))
            {
                aspect = TVM430S300MstsTranslation[TVMAspectCommand];
            }
            else
            {
                aspect = TVM430S320MstsTranslation[TVMAspectCommand];
            }
            SetNextSignalAspect(aspect);
        }

        protected void UpdateTvmCabSignal(TVMAspectType aspect, bool blinking, bool resetBlinking)
        {
            TVMAspectCurrent = aspect;
            TVMBlinkingCurrent = blinking;

            bool on = true;

            if (blinking)
            {
                if (!TVMBlinker.Started)
                {
                    TVMBlinker.Start();
                }

                if (resetBlinking)
                {
                    TVMBlinker.Stop();
                    TVMBlinker.Start();
                }

                on = TVMBlinker.On;
            }
            else
            {
                TVMBlinker.Stop();
            }

            switch (aspect)
            {
                case TVMAspectType.None:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._RRR:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 1);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._000:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 1);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._30E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 2 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._30A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 2 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._60E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 3 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._60A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 3 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._80E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 4 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._80A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 4 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._100E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 5 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._100A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 5 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._130E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 6 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._130A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 6 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._160E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, on ? 7 : 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._160A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, on ? 7 : 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._170E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, on ? 1 : 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._170A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 1 : 0);
                    break;

                case TVMAspectType._200V:
                    SetCabDisplayControl(TVM_VL, on ? 2 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._200A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 2 : 0);
                    break;

                case TVMAspectType._220E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, on ? 3 : 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._220V:
                    SetCabDisplayControl(TVM_VL, on ? 3 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._220A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 3 : 0);
                    break;

                case TVMAspectType._230E:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, on ? 4 : 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._230V:
                    SetCabDisplayControl(TVM_VL, on ? 4 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._230A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 4 : 0);
                    break;

                case TVMAspectType._270V:
                    SetCabDisplayControl(TVM_VL, on ? 5 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._270A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 5 : 0);
                    break;

                case TVMAspectType._300V:
                    SetCabDisplayControl(TVM_VL, on ? 6 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;

                case TVMAspectType._300A:
                    SetCabDisplayControl(TVM_VL, 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, on ? 6 : 0);
                    break;

                case TVMAspectType._320V:
                    SetCabDisplayControl(TVM_VL, on ? 7 : 0);
                    SetCabDisplayControl(TVM_Ex1, 0);
                    SetCabDisplayControl(TVM_Ex2, 0);
                    SetCabDisplayControl(TVM_An1, 0);
                    SetCabDisplayControl(TVM_An2, 0);
                    break;
            }
        }

        protected void UpdateTvmSounds()
        {
            if (TVMAspectCurrent != TVMAspectType.None && TVMAspectPreviousCycle != TVMAspectType.None)
            {
                TVMClosedSignal = (TVMAspectPreviousCycle > TVMAspectCurrent) || (TVMBlinkingCurrent && !TVMBlinkingPreviousCycle);
                TVMOpenedSignal = (TVMAspectPreviousCycle < TVMAspectCurrent) || (!TVMBlinkingCurrent && TVMBlinkingPreviousCycle);
            }
        }

        public override void HandleEvent(TCSEvent evt, string message)
        {
            switch (evt)
            {
                case TCSEvent.AlerterPressed:
                    VACMAPressed = true;
                    break;

                case TCSEvent.AlerterReleased:
                    VACMAPressed = false;
                    break;

                case TCSEvent.ThrottleChanged:
                case TCSEvent.DynamicBrakeChanged:
                case TCSEvent.HornActivated:
                    if (VACMAPressedAlertTimer.Started || VACMAPressedEmergencyTimer.Started)
                    {
                        VACMAPressedAlertTimer.Start();
                        VACMAPressedEmergencyTimer.Start();
                    }
                    break;

                case TCSEvent.GenericTCSButtonPressed:
                    {
                        int tcsButton = -1;
                        if (Int32.TryParse(message, out tcsButton))
                        {
                            SetCabDisplayControl(tcsButton, 1);

                            switch (tcsButton)
                            {
                                // BP (AC) SF
                                case BP_AC_SF:
                                    RSOPressed = true;
                                    break;

                                // BP (A) LS (SF)
                                case BP_A_LS_SF:
                                    RSOCancelPressed = true;
                                    break;

                                // BP VAL
                                case KVB_BP_VAL:
                                    if (SpeedMpS() < 0.1f)
                                    {
                                        if (KVBMode == KVBModeType.Shunting)
                                        {
                                            KVBShuntingOdometer.Stop();
                                            KVBMode = KVBModeType.ConventionalLine;
                                        }

                                        KVBParametersValidation = true;
                                    }
                                    break;

                                // BP MV
                                case KVB_BP_MV:
                                    switch (KVBMode)
                                    {
                                        case KVBModeType.ConventionalLine:
                                            if ((KVBParametersValidation && SpeedMpS() < 0.1f)
                                                || (!KVBParametersValidation && SpeedMpS() <= MpS.FromKpH(30f)))
                                            {
                                                KVBMode = KVBModeType.Shunting;
                                                KVBParametersValidation = false;
                                                KVBShuntingOdometer.Start();
                                            }
                                            break;

                                        case KVBModeType.Shunting:
                                            if (SpeedMpS() <= MpS.FromKpH(30f))
                                            {
                                                KVBShuntingOdometer.Start();
                                            }
                                            break;
                                    }
                                    break;

                                // BP FC
                                case KVB_BP_FC:
                                    switch (KVBMode)
                                    {
                                        case KVBModeType.ConventionalLine:
                                        case KVBModeType.HighSpeedLine:
                                            if (SpeedMpS() < 0.1f)
                                            {
                                                if (KVBSpadEmergency)
                                                {
                                                    KVBSpadEmergency = false;
                                                    TVMCOVITSpadEmergency = false;
                                                }
                                                else
                                                {
                                                    KVBOverrideNf = true;
                                                }
                                            }
                                            break;
                                    }
                                    break;

                                // BP TEST
                                case KVB_BP_TEST:
                                    if (!KVBTestTimer.Started)
                                    {
                                        KVBTestTimer.Start();
                                        TriggerKvbTutSound(3f);
                                        TriggerKvbBipSound(3f);
                                    }
                                    break;

                                // BP AM V1 and BP AM V2
                                case BP_AM_V1:
                                case BP_AM_V2:
                                    TVMManualArming = true;
                                    break;

                                // BP DM
                                case BP_DM:
                                    TVMManualDearming = true;
                                    break;
                            }
                        }
                    }
                    break;

                case TCSEvent.GenericTCSButtonReleased:
                    {
                        int tcsButton = -1;
                        if (Int32.TryParse(message, out tcsButton))
                        {
                            SetCabDisplayControl(tcsButton, 0);

                            switch (tcsButton)
                            {
                                // BP (AC) SF
                                case BP_AC_SF:
                                    RSOPressed = false;
                                    break;

                                // BP (A) LS (SF)
                                case BP_A_LS_SF:
                                    RSOCancelPressed = false;
                                    break;

                                // BP AM V1 and BP AM V2
                                case BP_AM_V1:
                                case BP_AM_V2:
                                    TVMManualArming = false;
                                    break;

                                // BP DM
                                case BP_DM:
                                    TVMManualDearming = false;
                                    break;
                            }
                        }
                    }
                    break;

                case TCSEvent.GenericTCSSwitchOn:
                    {
                        int tcsButton = -1;
                        if (Int32.TryParse(message, out tcsButton))
                        {
                            SetCabDisplayControl(tcsButton, 1);

                            switch (tcsButton)
                            {
                                // Z (ES) VA
                                case Z_ES_VA:
                                    VACMATest = true;
                                    break;
                            }
                        }
                    }
                    break;

                case TCSEvent.GenericTCSSwitchOff:
                    {
                        int tcsButton = -1;
                        if (Int32.TryParse(message, out tcsButton))
                        {
                            SetCabDisplayControl(tcsButton, 0);

                            switch (tcsButton)
                            {
                                // Z (ES) VA
                                case Z_ES_VA:
                                    VACMATest = false;
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        public override void HandleEvent(PowerSupplyEvent evt, string message)
        {
            switch (evt)
            {
                case PowerSupplyEvent.CloseCircuitBreakerButtonPressed:
                case PowerSupplyEvent.CloseTractionCutOffRelayButtonPressed:
                    RearmingButton = true;
                    break;

                case PowerSupplyEvent.CloseCircuitBreakerButtonReleased:
                case PowerSupplyEvent.CloseTractionCutOffRelayButtonReleased:
                    RearmingButton = false;
                    break;
            }
        }

        protected void UpdateVACMA()
        {
            if (VACMAPresent && Activated && IsAlerterEnabled())
            {
                if (SpeedMpS() >= VACMAActivationSpeedMpS || VACMATest)
                {
                    if (VACMAPressed && (!VACMAPressedAlertTimer.Started || !VACMAPressedEmergencyTimer.Started))
                    {
                        VACMAReleasedAlertTimer.Stop();
                        VACMAReleasedEmergencyTimer.Stop();
                        VACMAPressedAlertTimer.Start();
                        VACMAPressedEmergencyTimer.Start();
                    }
                    if (!VACMAPressed && (!VACMAReleasedAlertTimer.Started || !VACMAReleasedEmergencyTimer.Started))
                    {
                        VACMAReleasedAlertTimer.Start();
                        VACMAReleasedEmergencyTimer.Start();
                        VACMAPressedAlertTimer.Stop();
                        VACMAPressedEmergencyTimer.Stop();
                    }
                }
                else
                {
                    VACMAReleasedAlertTimer.Stop();
                    VACMAReleasedEmergencyTimer.Stop();
                    VACMAPressedAlertTimer.Stop();
                    VACMAPressedEmergencyTimer.Stop();
                }

                if (VACMAReleasedAlertTimer.Started && VACMAReleasedAlertTimer.Triggered)
                    TriggerSoundWarning1();
                else
                    TriggerSoundWarning2();

                if (VACMAPressedAlertTimer.Started && VACMAPressedAlertTimer.Triggered)
                    TriggerSoundAlert1();
                else
                    TriggerSoundAlert2();

                if (!VACMAEmergencyBraking && (VACMAPressedEmergencyTimer.Triggered || VACMAReleasedEmergencyTimer.Triggered))
                {
                    VACMAEmergencyBraking = true;
                    SetVigilanceEmergencyDisplay(true);
                }

                if (VACMAEmergencyBraking && SpeedMpS() < VACMAActivationSpeedMpS && RearmingButton)
                {
                    VACMAEmergencyBraking = false;
                    SetVigilanceEmergencyDisplay(false);
                }
            }
            else
            {
                // Reset everything
                VACMAReleasedAlertTimer.Stop();
                VACMAReleasedEmergencyTimer.Stop();
                VACMAPressedAlertTimer.Stop();
                VACMAPressedEmergencyTimer.Stop();
                VACMAEmergencyBraking = false;
                SetVigilanceEmergencyDisplay(false);

                TriggerSoundWarning2();
                TriggerSoundAlert2();
                return;
            }

            // VY SOS VAC
            SetCabDisplayControl(VY_SOS_VAC, VACMAEmergencyBraking ? 1 : 0);

            // VY (ES) FU
            SetCabDisplayControl(VY_ES_FU, VACMATest && IsBrakeEmergency() && !TractionAuthorization() ? 1 : 0);
        }

        protected void UpdateSignalPassed()
        {
            SignalFeatures nextNormalSignal = NextGenericSignalFeatures("NORMAL", 0, 5000f);
            NormalSignalPassed = nextNormalSignal.DistanceM > PreviousNormalSignalDistanceM && SpeedMpS() > 0.1f;
            if (NormalSignalPassed)
            {
                LastNormalSignalPassed = NextNormalSignal;
                NextNormalSignal = nextNormalSignal;
                OnSignalPassed(LastNormalSignalPassed, SignalType.Normal);
            }
            else
            {
                NextNormalSignal = nextNormalSignal;
            }
            PreviousNormalSignalDistanceM = nextNormalSignal.DistanceM;

            SignalFeatures nextDistantSignal = NextGenericSignalFeatures("DISTANCE", 0, 5000f);
            DistantSignalPassed = nextDistantSignal.DistanceM > PreviousDistantSignalDistanceM && SpeedMpS() > 0.1f;
            if (DistantSignalPassed)
            {
                LastDistantSignalPassed = NextDistantSignal;
                NextDistantSignal = nextDistantSignal;
                OnSignalPassed(LastDistantSignalPassed, SignalType.Distant);
            }
            else
            {
                NextDistantSignal = nextDistantSignal;
            }
            PreviousDistantSignalDistanceM = nextDistantSignal.DistanceM;

            SignalFeatures nextInfoSignal = NextGenericSignalFeatures("INFO", 0, 5000f);
            InfoSignalPassed = nextInfoSignal.DistanceM > PreviousInfoSignalDistanceM && SpeedMpS() > 0.1f;
            if (InfoSignalPassed)
            {
                LastInfoSignalPassed = NextInfoSignal;
                NextInfoSignal = nextInfoSignal;
                OnSignalPassed(LastInfoSignalPassed, SignalType.Info);
            }
            else
            {
                NextInfoSignal = nextInfoSignal;
            }
            PreviousInfoSignalDistanceM = nextInfoSignal.DistanceM;

            SignalFeatures nextRepeaterSignal = NextGenericSignalFeatures("REPEATER", 0, 5000f);
            RepeaterSignalPassed = nextRepeaterSignal.DistanceM > PreviousRepeaterSignalDistanceM && SpeedMpS() > 0.1f;
            if (RepeaterSignalPassed)
            {
                LastRepeaterSignalPassed = NextRepeaterSignal;
                NextRepeaterSignal = nextRepeaterSignal;
                OnSignalPassed(LastRepeaterSignalPassed, SignalType.Repeater);
            }
            else
            {
                NextRepeaterSignal = nextRepeaterSignal;
            }
            PreviousRepeaterSignalDistanceM = nextRepeaterSignal.DistanceM;

            SpeedPostFeatures nextSpeedPost = NextSpeedPostFeatures(0, 5000f);
            SpeedPostPassed = nextSpeedPost.DistanceM > PreviousSpeedPostDistanceM && SpeedMpS() > 0.1f;
            if (SpeedPostPassed)
            {
                LastSpeedPostPassed = NextSpeedPost;
                NextSpeedPost = nextSpeedPost;
                OnSpeedPostPassed(LastSpeedPostPassed);
            }
            else
            {
                NextSpeedPost = nextSpeedPost;
            }
            PreviousSpeedPostDistanceM = nextSpeedPost.DistanceM;
        }
    }
}