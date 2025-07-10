using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class LPCODeclarationMessageSendingObject : DeclarationMessageSendingObject
	{
		public LPCODeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
			RequestObject = new LPCORequestObject(MessageTypeInfo as ZPropertyInfoString);
		}

		public LPCORequestObject RequestObject { get; private set; }

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = Header.Declaration.ActiveEntryHeaders.Count == 1;
		}

		#region MessageType

		protected override bool MessageType_ReadOnly => false;

		public override CodeDescriptionPairList MessageTypesList => Factory.GetCachedValue<LPCOEntryActionCodeList>();

		protected override ZString GetDefaultMessageType()
		{
			return Header.CanSendRectification ? LPCOEntryActionCodeList.Codes.RCA : LPCOEntryActionCodeList.Codes.ORI;
		}

		#endregion

		#region RequestObject

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|AmendmentReason", ShortCaption = "Reason")]
		public ZString Reason { get => RequestObject.Reason; set => RequestObject.Reason = value; }
		public ZPropertyInfo ReasonInfo => GetWrappedZPropertyInfo(nameof(Reason), x => RequestObject.ReasonInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|NewEffectiveDate", Caption = "New Effective Date")]
		public ZDate NewEffectiveDate { get => RequestObject.NewEffectiveDate; set => RequestObject.NewEffectiveDate = value; }
		public ZPropertyInfo NewEffectiveDateInfo => GetWrappedZPropertyInfo(nameof(NewEffectiveDate), x => RequestObject.NewEffectiveDateInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Requirement", Caption = "Requirement")]
		public ZInt Requirement { get => RequestObject.Requirement; set => RequestObject.Requirement = value; }
		public ZPropertyInfo RequirementInfo => GetWrappedZPropertyInfo(nameof(Requirement), x => RequestObject.RequirementInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber { get => RequestObject.DocumentNumber; set => RequestObject.DocumentNumber = value; }
		public ZPropertyInfo EntryNumberInfo => GetWrappedZPropertyInfo(nameof(EntryNumber), x => RequestObject.DocumentNumberInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|EntryLineNumber", Caption = "Entry Line Number")]
		public ZInt EntryLineNumber { get => RequestObject.DocumentItemNumber; set => RequestObject.DocumentItemNumber = value; }
		public ZPropertyInfo EntryLineNumberInfo => GetWrappedZPropertyInfo(nameof(EntryLineNumber), x => RequestObject.DocumentItemNumberInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Version", Caption = "Version")]
		public ZString Version { get => RequestObject.Version; set => RequestObject.Version = value; }
		public ZPropertyInfo VersionInfo => GetWrappedZPropertyInfo(nameof(Version), x => RequestObject.VersionInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Message", ShortCaption = "Message")]
		public ZString Message { get => RequestObject.Message; set => RequestObject.Message = value; }
		public ZPropertyInfo MessageInfo => GetWrappedZPropertyInfo(nameof(Message), x => RequestObject.MessageInfo);

		#endregion

		#region IMessageSendingObject

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.LPC;

		public override ZString GetMessageOwner() => ZString.Empty;

		public override ZGuid GetGlbExternalPasswordPK() => Header.Declaration.BrokerCertificate?.PK ?? ZGuid.Empty;

		public override ZString GetApplicationReference() => RequestObject.IsREQ ? new ZString(base.GetApplicationReference() + "|" + RequestObject.Requirement) : base.GetApplicationReference();

		public override ZString GetMessageText() => RequestObject.NewLPCOMessageBuilder()?.GenerateJsonMessage().GetSerializedString();

		#endregion
	}
}
