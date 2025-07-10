using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business
{
	public class AESOutboundEDIMessage : OutboundEDIMessage
	{
		public AESOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new AESOutboundEDIMessageLookups Lookups => (AESOutboundEDIMessageLookups)base.Lookups;
		protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new AESOutboundEDIMessageLookups(this);

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIMessage.ApplicationCodes.IECustomsExport).GetNextFormatted(Factory) : (string)EM_MessageNum;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
		}

		protected override string GetSendersReference()
		{
			if (EM_MessageType == AESOutgoingMessageTypeList.Codes.ExportOriginal || EM_MessageType == AESOutgoingMessageTypeList.Codes.ExitOriginal || EM_MessageType == AESOutgoingMessageTypeList.Codes.ReExport)
			{
				return (EM_LinkedObject as CusEntryHeader)?.GetLRNAndSetIfNeeded() ?? string.Empty;
			}
			else
			{
				return string.Empty;
			}
		}

		protected override string SendersReferencePlaceHolderOverride => LRNPlaceHolder;

		#region Constants For Number Fountain Place Holders
		public const string LRNPlaceHolder = "AES__LRN_Holder";
		#endregion

		protected override string MessageNumberPlaceHolderOverride => CargoWise.Customs.IE.MessageContracts.Constants.AESMessageRecipientHolder;
		protected override string UniqueBatchNumberPlaceHolderOverride => CargoWise.Customs.IE.MessageContracts.Constants.AESMessageSenderHolder;
		protected override string GetBatchNumber() => GlbCompanyWrapper.GetMessageSenderEORI(EM_LinkedObject as IMessageAttachee);
	}
}
