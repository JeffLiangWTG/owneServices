using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJobBillingTaxBranchDefaultingManager
	{
		void SetDefaultValue(IJobInvoicingPlugIn jobPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment);
		IZType DefaultValue { get; }
	}
}
