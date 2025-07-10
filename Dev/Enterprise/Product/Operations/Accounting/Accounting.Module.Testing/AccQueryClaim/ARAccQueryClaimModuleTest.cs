using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARAccQueryClaimModule))]
	public class ARAccQueryClaimModuleTest : AccQueryClaimModuleTest
	{
		public void TestCheckpoints()
		{
			using (AccQueryClaimModule module = GetModuleInstance())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ReceivablesClaimsAndQueries, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (IQueryClaimForTest module = GetModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is ARAccQueryClaimFilterControl);
				filterControl.Dispose();
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARAccQueryClaim;
		}

		protected override AccQueryClaimModule GetModuleInstance()
		{
			return new ARAccQueryClaimModule();
		}

		protected override IQueryClaimForTest GetModuleForTest()
		{
			return new ARAccQueryClaimModuleForTest();
		}

		class ARAccQueryClaimModuleForTest : ARAccQueryClaimModule, IQueryClaimForTest
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

			ZArchitecture.Business.FilterBusinessObject IQueryClaimForTest.NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}

			#endregion
		}
	}
}
