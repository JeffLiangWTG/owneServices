using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobCartageBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (JobManagementModule)ZModuleFactory.Instance.Create(ModuleIDs.JobManagement);
		}
	}
}
