using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
	{
		public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header)
		{
			CodeDescriptionPairList result;
			if (header.IsDepartureMovement)
			{
				result = header.Factory.GetCachedValue("IENCTSOutgoingDepartureMessageTypeList|Movement", GetMovementNCTSOutgoingDepartureMessageTypeList);
			}
			else
			{
				result = header.Factory.GetCachedValue<NCTSOutgoingArrivalMessageTypeList>();
			}
			return result;
		}

		CodeDescriptionPairList GetMovementNCTSOutgoingDepartureMessageTypeList()
		{
			var fullList = new NCTSOutgoingDepartureMessageTypeList();
			fullList.RemoveCode(NCTSOutgoingDepartureMessageTypeList.Codes.QueryOnGuarantees);
			fullList.Sort();
			return fullList;
		}

		protected override void SetDefaultMessageTypeCore(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			if (nctsHeaderMessageSendingObject.NctsHeader.IsDepartureMovement)
			{
				switch (nctsHeaderMessageSendingObject.NctsHeader.MovementHeader?.BM_CustomsStatus)
				{
					case "":
						nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
						break;
					case NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry:
						nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement;
						break;
					case NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice:
					case NCTS5DepartureCustomsStatusList.Codes.DecisionToControl:
					case NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest:
					case NCTS5DepartureCustomsStatusList.Codes.IntentionToControl:
						nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.RequestOfRelease;
						break;
					case NCTS5DepartureCustomsStatusList.Codes.PreLodged:
						nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.PresentationNotification;
						break;
					default:
						if (!nctsHeaderMessageSendingObject.MRN.IsEmpty)
						{
							nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment;
						}
						break;
				}
			}
			else if (nctsHeaderMessageSendingObject.NctsHeader.IsArrivalMovement)
			{
				switch (nctsHeaderMessageSendingObject.NctsHeader.ArrivalMovementHeader?.BM_CustomsStatus)
				{
					case "":
						nctsHeaderMessageSendingObject.MessageType =
							NCTSOutgoingMessageTypeList.Codes.ArrivalNotification;
						break;
					default:
						nctsHeaderMessageSendingObject.MessageType = NCTSOutgoingMessageTypeList.Codes.UnloadingRemarks;
						break;
				}
			}
		}

		protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header) => new NctsHeaderMessageSendingObjectParent((NctsHeader)header);
	}
}
