using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	public class JXCImportModuleTest : TestCase
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.ImportJXCFile, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(JASSecurityCheckpoints.ImportJXCFile, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(JXCImportController), Module.GetNewController().GetType());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new JXCImportModuleForTest();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		protected JXCImportModuleForTest Module;
		#region JXCImportModuleForTest class
		protected class JXCImportModuleForTest : JXCImportModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}
		#endregion
		#endregion
	}
}
