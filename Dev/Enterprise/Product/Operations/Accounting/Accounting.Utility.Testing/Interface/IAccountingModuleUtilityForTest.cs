using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Utility.Testing
{
	public interface IAccountingModuleUtilityForTest
	{
		ZQuery GetAdditionalFilterForBulkJobClosure(ZQuery query);

		ModuleDateFilter GetModuleDateFilter(string description);
	}
}
