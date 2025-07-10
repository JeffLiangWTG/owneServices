using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	public abstract class ZWebModule_Test : TestCaseWithFactory
	{
		#region setup

		protected override void SetUp()
		{
			base.SetUp();
			TestZWebModule = GetNewZWebModule();
		}

		protected override void TearDown()
		{
			TestZWebModule.Dispose();
			base.TearDown();
		}

		protected ZWebModule TestZWebModule;
		protected abstract WebModuleID TestID { get; }
		protected abstract ZWebModule GetNewZWebModule();

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
		}

		#endregion

		public void TestLicenseCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, TestZWebModule.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, TestZWebModule.SecurityCheckpoint);
		}

		public void TestFactory()
		{
			AssertNotNull(TestZWebModule.FactoryInternal);
			AssertEquals(typeof(BusinessObjectFactory), TestZWebModule.FactoryInternal.GetType());
		}
	}
}
