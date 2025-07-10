using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Common.EU.LogicalStatusList.Codes;
using static Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList.Codes;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
	{
		public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header)
		{
			if (header.IsArrivalMovement)
			{
				return NctsMessageTypeList.NctsArrivalMessageTypeList;
			}
			else if (header.IsDepartureMovement)
			{
				return NctsMessageTypeList.NctsDepartureMessageTypeList;
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		public override bool GetShouldSendDefault(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			var header = nctsHeaderMessageSendingObject.NctsHeader;
			var messageStatus = nctsHeaderMessageSendingObject.MessageStatus.ToString();

			return nctsHeaderMessageSendingObject.MessageType.ToString() switch
			{
				NctsMessageTypeList.Codes.DEPDAT => ShouldSendDepDat(),
				_ => messageStatus is not Sent || !header.IsDepartureMovement,
			};

			bool ShouldSendDepDat() => messageStatus is Error or Failed or DepartureDeclarationNotSent or Rejected or ""
										|| (messageStatus is Invalid && header.MovementReferenceNumber.IsEmpty);
		}

		protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header) => new NctsHeaderMessageSendingObjectParent(header);

		protected override bool ShouldFillAdditionalWarningsOnSendScreenCore => true;

		protected override void SetDefaultMessageTypeCore(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			if (nctsHeaderMessageSendingObject.NctsHeader.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				if (GetDefaultMessageType(arrivalMovementHeader) is { } defaultMessageType)
				{
					nctsHeaderMessageSendingObject.MessageType = defaultMessageType;
				}
			}
			else
			{
				base.SetDefaultMessageTypeCore(nctsHeaderMessageSendingObject);
			}
		}

		protected override bool ShouldHideSendWithAdditionalWarningCheckBoxCore => true;

		protected override bool ShowJustificationCore(EU.NCTS.Business.NctsHeader header) => false;

		public static string GetDefaultMessageType(NctsArrivalMovementHeader arrivalMovementHeader)
		{
			var messageStatus = arrivalMovementHeader.BM_MessageStatus.ToString();
			var customsStatus = arrivalMovementHeader.BM_CustomsStatus.ToString();
			const string rejectedMessageStatus = nameof(CUSINFReferencedMessageStatus.REJ);

			return (messageStatus, customsStatus) switch
			{
				("" or ArrivalNotificationNotSent, _) => NctsMessageTypeList.Codes.DESNOT,
				(Error or Failed or rejectedMessageStatus, "") => NctsMessageTypeList.Codes.DESNOT,
				(Accepted or Error or Failed or rejectedMessageStatus, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted) => NctsMessageTypeList.Codes.DESREM,
				(Error or Failed or rejectedMessageStatus, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks) => NctsMessageTypeList.Codes.DESREM,
				_ => null
			};
		}
	}
}
