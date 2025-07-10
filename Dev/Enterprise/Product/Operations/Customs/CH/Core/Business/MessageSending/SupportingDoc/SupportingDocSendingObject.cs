using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocSendingObject : JobDeclarationSupportingDocSendingObject, IMessageSendingObject
{
	public SupportingDocSendingObject(JobDeclaration declaration) : base(declaration)
	{
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.SupportingDocSendingObject|LocalReferenceNumber", Caption = "Entry (MRN)")]
	public override ZString LocalReferenceNumber { get => base.LocalReferenceNumber; set => base.LocalReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.SupportingDocSendingObject|CaseNumber", Caption = "Reference")]
	public override ZString CaseNumber { get => base.CaseNumber; set => base.CaseNumber = value; }

	public override CodeDescriptionPairList Entries
	{
		get
		{
			if (fEntries == null)
			{
				fEntries = new CodeDescriptionPairList();
				foreach (CusEntryHeader validEntryHeader in Declaraction.ActiveEntryHeaders)
				{
					var code = validEntryHeader.MovementReferenceNumber;
					if (!code.IsEmpty)
					{
						fEntries.AddPair(code, validEntryHeader.EntryInstruction?.CEI_Description ?? ZString.Empty);
					}
				}
			}
			return fEntries;
		}
	}
	CodeDescriptionPairList fEntries;

	protected override ZString DocumentTypeCode => UniversalReferenceConstants.RefCusCodeList.EdecTypes.DocumentType;

	protected override Customs.Business.SupportingDocSendingObjectValidation GetNewValidation() => new SupportingDocSendingObjectValidation(this);

	#region IMessageSendingObject

	public ZString ApplicationCode => EDIMessage.ApplicationCodes.CHCustomsEdec;

	public ZString GetApplicationReference() => ZString.Empty;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.EBD;

	public ZString MessageSubTypeForEDIMessage => ZString.Empty;

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;

	#endregion
}
