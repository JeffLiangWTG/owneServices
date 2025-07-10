using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	public class PreMatchingExportModuleTest : TestCase
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.ExportPrematchingTransactions, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(JASSecurityCheckpoints.ExportPrematchingTransactions, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(PreMatchingExportController), Module.ExposedGetNewController().GetType());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new ExposedPreMatchingExportModule();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		protected ExposedPreMatchingExportModule Module;
		#region ExposedPreMatchingExportModule class
		protected class ExposedPreMatchingExportModule : PreMatchingExportModule
		{
			public ZPopupController ExposedGetNewController()
			{
				return GetNewController();
			}
		}
		#endregion
		#endregion
	}
}
