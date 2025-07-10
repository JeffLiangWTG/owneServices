using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Duimp;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public partial class DuimpMessageSendingObject : DeclarationMessageSendingObject
	{
		public DuimpMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public override ZString GetMessageOwner() => ZString.Empty;

		public override ZString GetMessageTypeForEDIMessage()
		{
			switch (MessageType)
			{
				case EDIMessageSubTypeList.Codes.Original:
				case EDIMessageSubTypeList.Codes.Update:
				case EDIMessageSubTypeList.Codes.CompleteConsult:
				case ImportEntryActionCodeList.Codes.REG:
				case ImportEntryActionCodeList.Codes.DIA:
					return MessageTypeList.Codes.CIH;
				case ImportEntryActionCodeList.Codes.DEL:
					return MessageTypeList.Codes.DOR;
				default:
					return MessageTypeList.Codes.CDD;
			}
		}

		protected override ZString GetDefaultMessageType() => ImportEntryActionCodeList.Codes.ORI;

		protected override bool MessageType_ReadOnly => false;

		public override CodeDescriptionPairList MessageTypesList
		{
			get
			{
				return Factory.GetCachedValue("DUIMPMessageTypesList", () =>
				{
					var result = new ImportEntryActionCodeList();
					result.RemoveCode(ImportEntryActionCodeList.Codes.CVH);
					result.RemoveCode(ImportEntryActionCodeList.Codes.DEL);
					result.RemoveCode(ImportEntryActionCodeList.Codes.RET);
					return result;
				});
			}
		}

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new DuimpMessageSendingObjectValidation(this);
		}

		public new DuimpMessageSendingObjectValidation Validation => (DuimpMessageSendingObjectValidation)base.Validation;

		public override ZString GetApplicationReference() => MessageType == ImportEntryActionCodeList.Codes.ORI ? base.GetApplicationReference() : new ZString(base.GetApplicationReference() + "|" + Header.CH_AuthorityVersion);

		public override ZGuid GetGlbExternalPasswordPK() => Header.Declaration.BrokerCertificate?.PK ?? ZGuid.Empty;

		public override ZString GetMessageText()
		{
			IJsonMessageBuilder messageBuilder = null;

			switch (MessageType)
			{
				case ImportEntryActionCodeList.Codes.DIA:
					messageBuilder = new DuimpDiagnosisMessageBuilder(new DuimpDiagnosisProvider(this));
					break;
				case ImportEntryActionCodeList.Codes.REG:
					messageBuilder = new DuimpRegisterMessageBuilder(new DuimpRegisterProvider(this));
					break;
				case EDIMessageSubTypeList.Codes.Original:
				case EDIMessageSubTypeList.Codes.Update:
				case ImportEntryActionCodeList.Codes.RET:
					messageBuilder = new DuimpHeaderMessageBuilder(new DuimpHeaderProvider(this));
					break;
			}

			return messageBuilder?.GenerateJsonMessage().GetSerializedString();
		}

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = (Header.Declaration.ActiveEntryHeaders.Count == 1 && !Header.IsWaitingForResponse);
		}

		#endregion
	}
}
