using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.Configurations
{
	public sealed class TP5MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
	{
		protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header) => new TP5MessageSendingObjectParent((NctsHeader)header);

		protected override void SetDefaultMessageTypeCore(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			var messageTypeList = nctsHeaderMessageSendingObject.Lookups.MessageTypeList.GetAllCodes();
			nctsHeaderMessageSendingObject.MessageType = messageTypeList.Length == 1 ? messageTypeList.First() : ZString.Empty;
		}

		public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header)
		{
			var list = new CodeDescriptionPairList();

			if (header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
			{
				list = GetMessageTypeListForDeparture(departureMovementHeader.BM_CustomsStatus);
			}
			else if (header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				list = GetMessageTypeListForArrival(arrivalMovementHeader.BM_CustomsStatus);
			}

			return list;
		}

		CodeDescriptionPairList GetMessageTypeListForDeparture(ZString customsStatus)
		{
			var list = new CodeDescriptionPairList();

			switch (customsStatus)
			{
				case "":
				case NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest:
				case NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice:
				case NCTS5DepartureCustomsStatusList.Codes.ReleaseRequestHasBeenRequested:
				case NCTS5DepartureCustomsStatusList.Codes.RejectedAtOrigin:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC015C, TP5MessageTypeList.Descriptions.CC015C);
					break;
				case NCTS5DepartureCustomsStatusList.Codes.MrnAllocated:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Descriptions.CC013C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Descriptions.CC014C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC034C, TP5MessageTypeList.Descriptions.CC034C);
					break;
				case NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC141C, TP5MessageTypeList.Descriptions.CC141C);
					break;
				case NCTS5DepartureCustomsStatusList.Codes.PreLodged:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Descriptions.CC013C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Descriptions.CC014C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC034C, TP5MessageTypeList.Descriptions.CC034C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC170C, TP5MessageTypeList.Descriptions.CC170C);
					break;
				case NCTS5DepartureCustomsStatusList.Codes.Acknowledged:
				case NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested:
				case NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Descriptions.CC013C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Descriptions.CC014C);
					break;
				case NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Descriptions.CC014C);
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC034C, TP5MessageTypeList.Descriptions.CC034C);
					break;
				default:
					break;
			}

			return list;
		}

		CodeDescriptionPairList GetMessageTypeListForArrival(ZString customsStatus)
		{
			var list = new CodeDescriptionPairList();

			switch (customsStatus)
			{
				case NCTS5ArrivalCustomsStatusList.Codes.Canceled:
				case NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease:
				case NCTS5ArrivalCustomsStatusList.Codes.UnderRecoveryProcedure:
					break;
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted:
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC044C, TP5MessageTypeList.Descriptions.CC044C);
					break;
				default:
					list.AddPairIfNotExist(TP5MessageTypeList.Codes.CC007C, TP5MessageTypeList.Descriptions.CC007C);
					break;
			}

			return list;
		}

		protected override bool ShowJustificationCore(EU.NCTS.Business.NctsHeader header)
		{
			return false;
		}

		protected override EU.NCTS.Business.INctsHeaderMessageSendingObjectValidationDecider GetValidationDeciderCore() => new TP5MessageSendingObjectValidationDecider();
	}
}
