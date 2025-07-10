using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
{
	public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		var targetInfo = Parent.JI_ProcedureInfo;
		var isRequestedProcedureValid = IsRequestedProcedureValid(Parent.JI_Calc_RequestedProcedure);
		var fiscalReference = Parent.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer);
		if (isRequestedProcedureValid && !fiscalReference)
		{
			targetInfo.AddMessageError(Res.GetString("02CC8CBE-CFBF-4D55-B379-0F24B22B3C5D", "Fiscal reference with code FR2 is mandatory if procedure is 42 or 63"));
		}

		if (fiscalReference && !isRequestedProcedureValid)
		{
			targetInfo.AddMessageError(Res.GetString("6C828D91-B953-418C-9CF8-C78EAAA9AF90", "The CPC must start with 42 of 63 for this invoice line"));
		}
	}

	static bool IsRequestedProcedureValid(string requestedProcedure) => requestedProcedure == "42" || requestedProcedure == "63";

	protected override void CheckJI_CountryOfOriginMandatoryValidation()
	{
		if (Parent.JI_CountryOfOrigin.IsEmpty && (Parent.EntryInstruction?.IsImportDeclarationType() ?? false))
		{
			if (Parent.JI_PrimaryPreference.IsEmpty)
			{
				Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("2FDAEC3F-D6E9-49A1-A2C7-7FF426087AE6", "Country of Origin is required when no Preference is used for Declaration Type {0}", Parent.EntryInstruction.CEI_Style));
			}
			else if (IsPreferenceUsed())
			{
				if (Parent.JI_PrimaryPreference.StartsWith("1"))
				{
					Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("1D12C8CB-7941-49B2-A4FA-1B20B7487B4D", "Country of Origin is required when Preference is {0} for Declaration Type {1}", Parent.JI_PrimaryPreference, Parent.EntryInstruction.CEI_Style));
				}
				else
				{
					Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("391CD4AE-0BB4-4DD4-9BD7-858CF6477B13", "Country of Preferential Origin is required when Preference is {0} for Declaration Type {1}", Parent.JI_PrimaryPreference, Parent.EntryInstruction.CEI_Style));
				}
			}
		}

		bool IsPreferenceUsed()
		{
			var firstCharOfPrimaryReference = Parent.JI_PrimaryPreference.SubstringSafe(0, 1);
			return firstCharOfPrimaryReference == "1"
					|| firstCharOfPrimaryReference == "2"
					|| firstCharOfPrimaryReference == "3"
					|| firstCharOfPrimaryReference == "4";
		}
	}
}
