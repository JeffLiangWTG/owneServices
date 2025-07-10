using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusUnderbond
		{
			#region C4_ArrivalDate

			ZDateTime C4_ArrivalDate
			{
				get;
				set;
			}

			ZPropertyInfo C4_ArrivalDateInfo
			{
				get;
			}

			#endregion

			#region C4_C6

			ZGuid C4_C6
			{
				get;
				set;
			}

			ZPropertyInfo C4_C6Info
			{
				get;
			}

			#endregion

			#region C4_DateOfArrivalIntoDestinationPremise

			ZDateTime C4_DateOfArrivalIntoDestinationPremise
			{
				get;
				set;
			}

			ZPropertyInfo C4_DateOfArrivalIntoDestinationPremiseInfo
			{
				get;
			}

			#endregion

			#region C4_DestinationPremiseID

			ZString C4_DestinationPremiseID
			{
				get;
				set;
			}

			ZPropertyInfo C4_DestinationPremiseIDInfo
			{
				get;
			}

			#endregion

			#region C4_DischargePremiseID

			ZString C4_DischargePremiseID
			{
				get;
				set;
			}

			ZPropertyInfo C4_DischargePremiseIDInfo
			{
				get;
			}

			#endregion

			#region C4_FlightNo

			ZString C4_FlightNo
			{
				get;
				set;
			}

			ZPropertyInfo C4_FlightNoInfo
			{
				get;
			}

			#endregion

			#region C4_IsBureau

			ZBool C4_IsBureau
			{
				get;
				set;
			}

			ZPropertyInfo C4_IsBureauInfo
			{
				get;
			}

			#endregion

			#region C4_IsMoveFromDischarge

			ZBool C4_IsMoveFromDischarge
			{
				get;
				set;
			}

			ZPropertyInfo C4_IsMoveFromDischargeInfo
			{
				get;
			}

			#endregion

			#region C4_UnderbondBySeaLloydsIMONum

			ZString C4_UnderbondBySeaLloydsIMONum
			{
				get;
				set;
			}

			ZPropertyInfo C4_UnderbondBySeaLloydsIMONumInfo
			{
				get;
			}

			#endregion

			#region C4_TranshipBySeaLloydsIMONum

			ZString C4_TranshipBySeaLloydsIMONum
			{
				get;
				set;
			}

			ZPropertyInfo C4_TranshipBySeaLloydsIMONumInfo
			{
				get;
			}

			#endregion

			#region C4_MAWB

			ZString C4_MAWB
			{
				get;
				set;
			}

			ZPropertyInfo C4_MAWBInfo
			{
				get;
			}

			#endregion

			#region C4_MessageStatus

			ZString C4_MessageStatus
			{
				get;
				set;
			}

			ZPropertyInfo C4_MessageStatusInfo
			{
				get;
			}

			#endregion

			#region C4_ModeOfMovement

			ZString C4_ModeOfMovement
			{
				get;
				set;
			}

			ZPropertyInfo C4_ModeOfMovementInfo
			{
				get;
			}

			#endregion

			#region C4_MovementReason

			ZString C4_MovementReason
			{
				get;
				set;
			}

			ZPropertyInfo C4_MovementReasonInfo
			{
				get;
			}

			#endregion

			#region C4_OriginPremiseID

			ZString C4_OriginPremiseID
			{
				get;
				set;
			}

			ZPropertyInfo C4_OriginPremiseIDInfo
			{
				get;
			}

			#endregion

			#region C4_Outurned

			ZDateTime C4_Outurned
			{
				get;
				set;
			}

			ZPropertyInfo C4_OuturnedInfo
			{
				get;
			}

			#endregion

			#region C4_PackageType

			ZString C4_PackageType
			{
				get;
				set;
			}

			ZPropertyInfo C4_PackageTypeInfo
			{
				get;
			}

			#endregion

			#region C4_ParentID

			ZGuid C4_ParentID
			{
				get;
				set;
			}

			ZPropertyInfo C4_ParentIDInfo
			{
				get;
			}

			#endregion

			#region C4_ParentTableCode

			ZString C4_ParentTableCode
			{
				get;
				set;
			}

			ZPropertyInfo C4_ParentTableCodeInfo
			{
				get;
			}

			#endregion

			#region C4_PiecesManifested

			ZInt C4_PiecesManifested
			{
				get;
				set;
			}

			ZPropertyInfo C4_PiecesManifestedInfo
			{
				get;
			}

			#endregion

			#region C4_ResponsiblePartyID

			ZString C4_ResponsiblePartyID
			{
				get;
				set;
			}

			ZPropertyInfo C4_ResponsiblePartyIDInfo
			{
				get;
			}

			#endregion

			#region C4_RL_NKDischargePort

			ZString C4_RL_NKDischargePort
			{
				get;
				set;
			}

			ZPropertyInfo C4_RL_NKDischargePortInfo
			{
				get;
			}

			#endregion

			#region C4_RL_NKLoadPort

			ZString C4_RL_NKLoadPort
			{
				get;
				set;
			}

			ZPropertyInfo C4_RL_NKLoadPortInfo
			{
				get;
			}

			#endregion

			#region C4_RL_NKTranshipDestPort

			ZString C4_RL_NKTranshipDestPort
			{
				get;
				set;
			}

			ZPropertyInfo C4_RL_NKTranshipDestPortInfo
			{
				get;
			}

			#endregion

			#region C4_UnderbondBySeaVessel

			ZString C4_UnderbondBySeaVessel
			{
				get;
				set;
			}

			ZPropertyInfo C4_UnderbondBySeaVesselInfo
			{
				get;
			}

			#endregion

			#region C4_TranshipBySeaVessel

			ZString C4_TranshipBySeaVessel
			{
				get;
				set;
			}

			ZPropertyInfo C4_TranshipBySeaVesselInfo
			{
				get;
			}

			#endregion

			#region C4_SendersMessageReference

			ZString C4_SendersMessageReference
			{
				get;
				set;
			}

			ZPropertyInfo C4_SendersMessageReferenceInfo
			{
				get;
			}

			#endregion

			#region C4_Status

			ZString C4_Status
			{
				get;
				set;
			}

			ZPropertyInfo C4_StatusInfo
			{
				get;
			}

			#endregion

			#region C4_UnderbondBySeaVoyage

			ZString C4_UnderbondBySeaVoyage
			{
				get;
				set;
			}

			ZPropertyInfo C4_UnderbondBySeaVoyageInfo
			{
				get;
			}

			#endregion

			BusinessObject LinkedObject
			{
				get;
				set;
			}
		}
	}
}