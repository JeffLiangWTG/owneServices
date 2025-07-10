using CargoWise.EntityFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccountingModuleUtilityForTest : IAccountingModuleUtilityForTest
	{
		public ZQuery GetAdditionalFilterForBulkJobClosure(ZQuery query)
		{
			return JobManagementFilterBusinessObject.GetAdditionalFilterForBulkJobClosure(query);
		}

		public ModuleDateFilter GetModuleDateFilter(string description)
		{
			return JobManagementFilterBusinessObject[description] as ModuleDateFilter;
		}

		JobManagementFilterBusinessObject JobManagementFilterBusinessObject
		{
			get
			{
				if (fJobManagementFilterBusinessObject == null)
				{
					fJobManagementFilterBusinessObject = new JobManagementFilterBusinessObject();
				}
				return fJobManagementFilterBusinessObject;
			}
		}
		JobManagementFilterBusinessObject fJobManagementFilterBusinessObject;
	}
}
