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
	[TestedType(typeof(GlobalChargeCodeOrganizationModule))]
	public class GlobalChargeCodeOrganizationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlobalChargeCodeOrganization;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (GlobalChargeCodeOrganizationModule module = new GlobalChargeCodeOrganizationModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlobalChargeCodeOrganization, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (GlobalChargeCodeOrganizationModuleForTest module = new GlobalChargeCodeOrganizationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is GlobalChargeCodeOrganizationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (GlobalChargeCodeOrganizationModuleForTest module = new GlobalChargeCodeOrganizationModuleForTest())
			{
				IBusinessObjectCollection zonesCollection = module.NewGridCollection;
				Assert("Invalid type", zonesCollection is GlobalChargeCodeMapOrganizationCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (GlobalChargeCodeOrganizationModuleForTest module = new GlobalChargeCodeOrganizationModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is GlobalChargeCodeOrganizationFilterBusinessObject);
			}
		}

		#endregion
	}
}
