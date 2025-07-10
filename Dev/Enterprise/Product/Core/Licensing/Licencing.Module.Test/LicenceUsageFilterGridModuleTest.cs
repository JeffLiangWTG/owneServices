using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Licencing.Module.Testing
{
	[TestedType(typeof(LicenceUsageFilterGridModule))]
	sealed class LicenceUsageFilterGridModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.LicenceUsage;
		}

		public void TestCheckpoints()
		{
			using (LicenceUsageFilterGridModule module = new LicenceUsageFilterGridModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.LicenceUsage, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestAllowActions()
		{
			using (LicenceUsageFilterGridModule module = new LicenceUsageFilterGridModule())
			{
				AssertEquals(false, module.AllowDelete);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowNew);
			}
		}

		protected override bool HasController()
		{
			return false;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			LicenceUsageLog log = Factory.New<LicenceUsageLog>();
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_ParentID = GlbCompany.CurrentCompany.PK;
			log.S7_OpenDateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
	}
}
