using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BR.Business.Export;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public partial class ExportDeclarationMessageSendingObject : DeclarationMessageSendingObject
	{
		public ExportDeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public override ZString GetMessageOwner() => ZString.Empty;

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CDE;

		protected override ZString GetDefaultMessageType()
		{
			return Header.CanSendRectification ? ExportEntryActionCodeList.Codes.RET : ExportEntryActionCodeList.Codes.ORI;
		}

		protected override bool MessageType_ReadOnly => false;

		public override CodeDescriptionPairList MessageTypesList => Factory.GetCachedValue<ExportEntryActionCodeList>();

		public override ZGuid GetGlbExternalPasswordPK() => Header.Declaration.BrokerCertificate?.PK ?? ZGuid.Empty;

		public override ZString GetMessageText()
		{
			IXmlMessageBuilder messageBuilder = null;

			if (MessageTypesList.ContainsCode(MessageType))
			{
				var legalDocument = Header?.EntryInstruction?.CEI_LegalDocument;
				switch (legalDocument)
				{
					case LegalDocumentList.Codes.ElectronicLogisticInvoice:
						messageBuilder = new DeclarationNFeMessageBuilder(new DeclarationNFnoNFProvider(this));
						break;

					case LegalDocumentList.Codes.NoInvoice:
						messageBuilder = new DeclarationNoNFMessageBuilder(new DeclarationNFnoNFProvider(this));
						break;
				}
			}

			return messageBuilder?.GenerateXmlMessage().GetSerializedString();
		}

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new ExportDeclarationMessageSendingObjectValidation(this);
		}

		public new ExportDeclarationMessageSendingObjectValidation Validation => (ExportDeclarationMessageSendingObjectValidation)base.Validation;

		#region Rectification Reason

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ExportDeclarationMessageSendingObject|AmendmentReason", ShortCaption = "Reason", Caption = "Rectification Reason")]
		public override ZString VOCReason
		{
			get => base.VOCReason;
			set => base.VOCReason = value;
		}

		internal bool IsRectification => MessageType == ExportEntryActionCodeList.Codes.RET;

		internal bool IsOriginal => MessageType == ExportEntryActionCodeList.Codes.ORI;

		protected override bool VOCReason_ReadOnly => !IsRectification;

		#endregion

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = (Header.Declaration.ActiveEntryHeaders.Count == 1 && !Header.IsWaitingForResponse);
		}

		#endregion
	}
}
