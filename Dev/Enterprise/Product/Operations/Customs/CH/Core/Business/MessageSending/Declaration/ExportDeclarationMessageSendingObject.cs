using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class ExportDeclarationMessageSendingObject : DeclarationMessageSendingObject
{
	public ExportDeclarationMessageSendingObject(ExportDeclarationMessageSendingObjectParent sendingObjectParent, CusEntryHeader header)
		: base(header)
	{
		SendingObjectParent = sendingObjectParent;
	}

	public ExportDeclarationMessageSendingObjectParent SendingObjectParent { get; }

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new ExportDeclarationMessageSendingObjectValidation Validation => (ExportDeclarationMessageSendingObjectValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new ExportDeclarationMessageSendingObjectValidation(this);

	public new ExportDeclarationMessageSendingObjectLookups Lookups => (ExportDeclarationMessageSendingObjectLookups)base.Lookups;

	protected override DeclarationMessageSendingObjectLookups GetNewLookups() => new ExportDeclarationMessageSendingObjectLookups(this);

	public override ZString FriendlyNameForMessageManager => MessageTypeCodeList.Descriptions.Export;

	public override ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.Export;

	public override ZString MessageSubTypeForEDIMessage => PassarMessageTypeList.GetMessageSubType(MessageTypeCodeList.Codes.Export, MessageType);

	public override ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.TokenCredentials?.PK ?? ZGuid.Empty;

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObject|VOCReason", Caption = "Reason Code", FullDescription = "The correction/cancellation reason code to be sent to customs.", ShortCaption = "Reason")]
	public override ZString VOCReason { get => base.VOCReason; set => base.VOCReason = value; }

	protected override bool VOCReason_ReadOnly
	{
		get
		{
			switch (MessageType)
			{
				case PassarMessageTypeList.Codes.NE015:
				case PassarMessageTypeList.Codes.NE130:
				case PassarMessageTypeList.Codes.NC016:
				case PassarMessageTypeList.Codes.NC123:
					return true;
				default:
					return false;
			}
		}
	}

	public override ZString ApplicationCode => EDIMessage.ApplicationCodes.CHCustomsPassar;

	public ZString MessageIdentifier { get; } = ZGuid.NewZGuid().ToString();

	protected override ZString GetApplicationReferenceCore() => MessageIdentifier;

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObject|NextProcedure", Caption = "Next Procedure")]
	[List(nameof(Lookups) + "." + nameof(ExportDeclarationMessageSendingObjectLookups.NextProcedureList))]
	public ZString NextProcedure
	{
		get => nextProcedure;
		set
		{
			SetNonPersistentPropertyValue(NextProcedureInfo, ref nextProcedure, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateNextProcedure();
			}
		}
	}
	ZString nextProcedure;

	public ZPropertyInfo NextProcedureInfo => GetZPropertyInfo(nameof(NextProcedure));

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObject|DeclarationNumber", Caption = "Entry Number (GDRN)", ShortCaption = "GDRN")]
	public ZString DeclarationNumber => Header.EntryNumber;

	public bool IsNC123 => MessageType == PassarMessageTypeList.Codes.NC123;

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		NextProcedure = Header.EntryInstruction?.CEI_NextProcedure ?? ZString.Empty;
	}
}
