using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Module.Testing
{
	[TestedType(typeof(IncidentApprovalModule))]
	sealed class IncidentApprovalModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ServiceRequest;
		}

		public void TestAllowNew()
		{
			using (IncidentApprovalModule module = (IncidentApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.ServiceRequest))
			{
				AssertEquals("AllowNew false", false, module.AllowNew);
			}
		}

		public void TestLicense()
		{
			using (IncidentApprovalModule module = (IncidentApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.ServiceRequest))
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurity()
		{
			using (IncidentApprovalModule module = (IncidentApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.ServiceRequest))
			{
				AssertEquals(Env.Security.IncidentApproval, module.SecurityCheckpoint);
			}
		}
	}
}
