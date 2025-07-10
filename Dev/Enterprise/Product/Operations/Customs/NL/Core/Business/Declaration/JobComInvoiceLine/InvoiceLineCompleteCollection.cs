using CargoWise.EntityFramework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		using (child.SuspendSettingHasChanges())
		{
			if (child is JobComInvoiceLine jci)
			{
				jci.PopulateFromSupplierLink();

				if (jci.Declaration is JobDeclaration jobDeclaration && jobDeclaration.IsExport && jci.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.CEI_Procedure.Equals(ProcedureCodes._10))
				{
					jci.JI_CEI = entryInstruction.PK;
					jci.JI_FormattedProcedure = FormattedProcedureCodes._1000;
					jci.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Netherlands;
				}
			}
		}
	}
}
