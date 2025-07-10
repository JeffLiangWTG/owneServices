using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APAccQueryClaimModule))]
	public class APAccQueryClaimModuleTest : AccQueryClaimModuleTest
	{
		public void TestCheckpoints()
		{
			using (AccQueryClaimModule module = GetModuleInstance())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.PayablesClaimsAndQueries, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (IQueryClaimForTest module = GetModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is APAccQueryClaimFilterControl);
				filterControl.Dispose();
			}
		}

		protected override IQueryClaimForTest GetModuleForTest()
		{
			return new APAccQueryClaimModuleForTest();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APAccQueryClaim;
		}

		protected override AccQueryClaimModule GetModuleInstance()
		{
			return new APAccQueryClaimModule();
		}

		class APAccQueryClaimModuleForTest : APAccQueryClaimModule, IQueryClaimForTest
		{
			#region IQueryClaimForTest Members

			IFilterControl IQueryClaimForTest.NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			IBusinessObjectCollection IQueryClaimForTest.NewGridCollection
			{
				get { return GridCollection; }
			}

			FilterBusinessObject IQueryClaimForTest.NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}

			#endregion
		}
	}
}
