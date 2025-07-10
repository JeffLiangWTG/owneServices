using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public AdditionalInfoValidation(AdditionalInfo parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckForExistanceOfNRV();
	}

	protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		if (Parent.Parent is CusEntryInstruction instruction)
		{
			if (Parent.IsAnAdditionalReference && Parent.HasDuplicateInAdditionalInfoCollectionOnEntryInstructionParent)
			{
				Parent.CSI_ReferenceNumberInfo.AddError(Res.GetString("E804B944-8D4F-4643-B013-5B5A7966DC58", "[R9006] Combination of Type and Reference must be unique for Kind REF"));
			}

			else if (Parent.IsATransportDocument && Parent.HasDuplicateInAdditionalInfoCollectionOnEntryInstructionParent)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("5297149F-52F9-4D20-8408-BFE04334EEAC", "[R9009] Combination of Type and Reference must be unique for Kind TRA"));
			}
		}

		if (Parent.ParentIsJobComInvoiceLine)
		{
			if (Parent.IsAnAdditionalReference && Parent.HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("20CCAFB1-B310-4543-894C-F3BFAF0D3A88", "[R9007] Combination of Type and Reference must be unique for Kind REF"));
			}
			else if (Parent.IsATransportDocument && Parent.HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("21A7206F-5933-48A8-B5F2-AD0582BFCC90", "[R9008] Combination of Type and Reference must be unique for Kind TRA"));
			}
		}
	}

	protected override void CheckCSI_Code()
	{
		Parent.ClearRowNotifications();
		base.CheckCSI_Code();
		CheckForExistanceOfNRV();
		ValidateRuleNR9024_AdditionalInfoType00100NA();
	}

	void CheckForExistanceOfNRV()
	{
		if (Parent.Parent is CusEntryInstruction entryInstruction && (entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.IncompleteDeclaration
			|| entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB
			|| entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
			|| entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.SimplifiedDeclaration)
			&& !entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Any(y => y.IsAnAdditionalInformation && y.CSI_Code.StartsWith(UniversalReferenceConstants.RefCusCodeList.Codes.NRV)))
		{
			Parent.AddRowMessageError(Res.GetString("D767A134-9E8E-4989-BBC9-F3F47E95E522", "[C9003] If Sub style is B, C, E or F an additional document kind INF is required of type NRV"));
		}
	}

	void ValidateRuleNR9024_AdditionalInfoType00100NA()
	{
		if (Parent.Parent is CusEntryInstruction entryInstruction && Parent.IsAnAdditionalInformation && Parent.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100)
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("75d0d9dd-5fa3-422f-bb61-0f71dfd93989", "[NR9024] Additional information type 00100 is only allowed on line item level"));
		}
	}
}
