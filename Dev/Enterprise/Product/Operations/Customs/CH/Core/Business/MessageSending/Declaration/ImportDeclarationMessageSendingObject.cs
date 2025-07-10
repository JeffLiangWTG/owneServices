using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ImportDeclarationMessageSendingObject : DeclarationMessageSendingObject
{
	public ImportDeclarationMessageSendingObject(CusEntryHeader header)
		: base(header)
	{
	}

	public new ImportDeclarationMessageSendingObjectValidation Validation => (ImportDeclarationMessageSendingObjectValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new ImportDeclarationMessageSendingObjectValidation(this);

	public new ImportDeclarationMessageSendingObjectLookups Lookups => (ImportDeclarationMessageSendingObjectLookups)base.Lookups;

	protected override DeclarationMessageSendingObjectLookups GetNewLookups() => new ImportDeclarationMessageSendingObjectLookups(this);

	public override ZString FriendlyNameForMessageManager => MessageTypeCodeList.Descriptions.Import;

	[ResourceStringData("CH.Business.ImportDeclarationMessageSendingObject|VOCReason", Caption = "Reason", FullDescription = "The correction/cancellation reason code to be sent to customs.", ShortCaption = "Reason")]
	public override ZString VOCReason { get => base.VOCReason; set => base.VOCReason = value; }

	protected override bool VOCReason_ReadOnly => MessageType == PassarMessageTypeList.Codes.NI015 || MessageType == PassarMessageTypeList.Codes.NI016;

	public override ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;
}
