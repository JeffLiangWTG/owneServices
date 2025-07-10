using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	public class MatchedTransactionImportModuleTest : TestCase
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.ImportMatchedTransactions, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(JASSecurityCheckpoints.ImportMatchedTransactions, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(MatchedTransactionImportController), Module.ExposedGetNewController().GetType());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new ExposedMatchedTransactionImportModule();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		protected ExposedMatchedTransactionImportModule Module;
		#region ExposedPreMatchingExportModule class
		protected class ExposedMatchedTransactionImportModule : MatchedTransactionImportModule
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
