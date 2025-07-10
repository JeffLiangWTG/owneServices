using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class MessageSendingConfiguration
	{
		public virtual CodeDescriptionPairList MessageTypeList(NctsHeader header)
		{
			if (header.IsPhase5Arrival)
			{
				return header.Factory.GetCachedValue<NCTSArrivalOutgoingMessageTypeList>();
			}
			else if (header.IsPhase5Departure)
			{
				return header.Factory.GetCachedValue<NCTSDepartureOutgoingMessageTypeList>();
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		public void SetDefaultMessageType(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject) => SetDefaultMessageTypeCore(nctsHeaderMessageSendingObject);

		protected virtual void SetDefaultMessageTypeCore(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			var messageTypeList = nctsHeaderMessageSendingObject.Lookups.MessageTypeList;
			if (messageTypeList.Count == 1)
			{
				nctsHeaderMessageSendingObject.MessageType = messageTypeList[0].Code;
			}
		}

		public NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParent(NctsHeader header) => GetNewNctsHeaderMessageSendingObjectParentCore(header);

		protected virtual NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(NctsHeader header) => new NctsHeaderMessageSendingObjectParent(header);

		public virtual bool GetShouldSendDefault(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject) => true;

		public bool ShouldFillAdditionalWarningsOnSendScreen => ShouldFillAdditionalWarningsOnSendScreenCore;

		protected virtual bool ShouldFillAdditionalWarningsOnSendScreenCore => false;

		public virtual string ReleaseRequestCode => NCTS5DeparturePhaseList.Codes.ReleaseRequest;

		public bool ShouldHideSendWithAdditionalWarningCheckBox => ShouldHideSendWithAdditionalWarningCheckBoxCore;

		protected virtual bool ShouldHideSendWithAdditionalWarningCheckBoxCore => false;

		public bool ShowJustification(NctsHeader header) => ShowJustificationCore(header);

		protected virtual bool ShowJustificationCore(NctsHeader header) => true;

		public INctsHeaderMessageSendingObjectValidationDecider GetValidationDecider() => GetValidationDeciderCore();

		protected virtual INctsHeaderMessageSendingObjectValidationDecider GetValidationDeciderCore()
		{
			return new NctsHeaderMessageSendingObjectValidationDecider();
		}
	}
}
