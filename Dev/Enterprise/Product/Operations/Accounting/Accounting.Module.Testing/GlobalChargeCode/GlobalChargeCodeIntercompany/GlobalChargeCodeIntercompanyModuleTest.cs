using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GlobalChargeCodeIntercompanyModule))]
	public class GlobalChargeCodeIntercompanyModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlobalChargeCodeIntercompany;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (GlobalChargeCodeIntercompanyModule module = new GlobalChargeCodeIntercompanyModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlobalChargeCodeIntercompany, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (GlobalChargeCodeIntercompanyModuleForTest module = new GlobalChargeCodeIntercompanyModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is GlobalChargeCodeIntercompanyFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (GlobalChargeCodeIntercompanyModuleForTest module = new GlobalChargeCodeIntercompanyModuleForTest())
			{
				IBusinessObjectCollection zonesCollection = module.NewGridCollection;
				Assert("Invalid type", zonesCollection is GlobalChargeCodeMapIntercompanyCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (GlobalChargeCodeIntercompanyModuleForTest module = new GlobalChargeCodeIntercompanyModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is GlobalChargeCodeIntercompanyFilterBusinessObject);
			}
		}

		#endregion
	}
}
