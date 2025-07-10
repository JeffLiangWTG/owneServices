using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateRuleNR9024();
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		ValidateRuleNR9025();
	}

	protected override void CheckJI_RN_NKCountryOfExport()
	{
		base.CheckJI_RN_NKCountryOfExport();
		if (Parent.JI_RN_NKCountryOfExport.IsEmpty && Parent.IsExport && (Parent.Declaration?.JE_RL_NKOrigin.IsEmpty ?? false))
		{
			Parent.JI_RN_NKCountryOfExportInfo.AddMessageError(Res.GetString("778EBF29-B2B9-4969-9B6A-3BB7C96413FD", "Export country is required on either Invoice header OR Invoice Item level"));
		}
	}

	protected override void CheckJI_CEI()
	{
		if (Parent.JI_CEI.IsEmpty)
		{
			Parent.JI_CEIInfo.AddMessageError(Res.GetString("C0374B9B-D4E5-4E65-8A0A-8309FEB7B6A7", "Invoice Line is not linked to an Entry Instruction."));
		}
	}

	protected override void CheckJI_CountryOfOriginMandatoryValidation()
	{
		base.CheckJI_CountryOfOriginMandatoryValidation();
		CheckRuleC0871(Parent.AdditionalProcedureCodesAsString, Parent.JI_CountryOfOriginInfo, Parent.JI_CountryOfOrigin);
	}

	void CheckRuleC0871(ZString additionalProcedureCode, ZPropertyInfo info, ZString countryOfOrigin)
	{
		if (!additionalProcedureCode.IsEmpty
			&& additionalProcedureCode.StartsWith("E")
			&& countryOfOrigin.IsEmpty)
		{
			info.AddMessageError(Res.GetString("FEF0CEDA-4BB5-4733-8863-550459B77795", "[C0871] If additional procedure starts with E, then ‘Country/Region of Origin’ is required."));
		}
	}

	void ValidateRuleNR9024()
	{
		if (Parent.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.AdditionalInfos.Cast<AdditionalInfo>().Any(y => y.IsAnAdditionalInformation && y.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100)) && !Parent.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.IsAnAdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100))
		{
			Parent.AddRowMessageError(Res.GetString("84939628-a648-4c0f-8887-c3adbefa2222", "[NR9024]  If additional information type 00100 is completed it must be present for all line items."));
		}
	}

	void ValidateRuleNR9025()
	{
		if (Parent.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.AdditionalInfos.Cast<AdditionalInfo>().Any(y => y.IsAnAdditionalInformation && y.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100) && x.JI_Procedure.Length >= 2 && Parent.JI_Procedure.Length >= 2 && !Parent.HasProcedureStartingWithAny([x.JI_Procedure.Substring(0, 2)])))
		{
			Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("c00c6b38-8aac-4a44-8036-d36df6002178", "[NR9025]  If additional information type 00100 is completed the first 2 digits of the procedure must be equal for all line items."));
		}
	}
}
