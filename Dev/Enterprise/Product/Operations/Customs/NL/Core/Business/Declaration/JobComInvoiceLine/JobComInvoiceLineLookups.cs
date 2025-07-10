using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override RefCusProcedureCollection CPCList
	{
		get
		{
			var result = base.CPCList;
			var procedureCode = Parent.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
			if (!procedureCode.IsEmpty)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.CPC, "Property", procedureCode, false));
			}
			return result;
		}
	}
}
