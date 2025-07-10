using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.GB.Business.NCTS.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.GB.Business
{
	public sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
	{
		public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header)
		{
			var codeList = base.MessageTypeList(header);
			if (header.IsArrivalMovement)
			{
				codeList = header.Factory.GetCachedValue<GB_NCTS5ArrivalPhaseList>();
			}
			else if (header.IsDepartureMovement)
			{
				codeList = header.Factory.GetCachedValue<GB_NCTS5DeparturePhaseList>();
			}
			return codeList;
		}

		protected override void SetDefaultMessageTypeCore(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			nctsHeaderMessageSendingObject.MessageType = ZString.Empty;
			var header = nctsHeaderMessageSendingObject.NctsHeader;
			var subApplicationCode = header.IsDepartureMovement ? header.MovementHeader?.BM_SubApplicationCode : header.IsArrivalMovement ? header.ArrivalMovementHeader?.BM_SubApplicationCode : ZString.Empty;
			if (subApplicationCode.Equals(Enterprise.Customs.Common.EU.NctsMoveHeaderType.Codes.Departure))
			{
				var customsStatus = nctsHeaderMessageSendingObject.NctsHeader.MovementHeader.BM_CustomsStatus;
				var phase = nctsHeaderMessageSendingObject.NctsHeader.MovementHeader.BM_Phase;
				if (customsStatus.IsEmpty || phase.IsEmpty || phase.Equals(NctsMovementHeaderTransactionStatusList.Codes.Declaration))
				{
					nctsHeaderMessageSendingObject.MessageType = GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration;
				}
				else if (customsStatus.Equals(NCTS5DepartureCustomsStatusList.Codes.PreLodged))
				{
					nctsHeaderMessageSendingObject.MessageType = GB_NCTS5DeparturePhaseList.Codes.PresentationOfAPreLodgedDeclaration;
				}
				else if (customsStatus.Equals(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated))
				{
					nctsHeaderMessageSendingObject.MessageType = GB_NCTS5DeparturePhaseList.Codes.Amendment;
				}
			}
			else if (subApplicationCode.Equals(Enterprise.Customs.Common.EU.NctsMoveHeaderType.Codes.Arrival))
			{
				var customsStatus = nctsHeaderMessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_CustomsStatus;
				var phase = nctsHeaderMessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_Phase;
				if (customsStatus.IsEmpty && (phase.IsEmpty || phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival))
				{
					nctsHeaderMessageSendingObject.MessageType = GB_NCTS5ArrivalPhaseList.Codes.ArrivalNotification;
				}
				else if (customsStatus.Equals(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted) || customsStatus.Equals(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks)
					&& phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival || phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingPermission)
				{
					nctsHeaderMessageSendingObject.MessageType = GB_NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;
				}
			}
		}

		protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header) => new NctsHeaderMessageSendingObjectParent(header);
	}
}
