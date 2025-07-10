using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	public class CognosCsvExportModuleTest : TestCase
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.ExportCognosCsv, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(JASSecurityCheckpoints.ExportCognosFile, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CognosCsvExportController), Module.GetNewController().GetType());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new CognosCsvExportModuleForTest();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		protected CognosCsvExportModuleForTest Module;
		#region CognosCsvExportModuleForTest class
		protected class CognosCsvExportModuleForTest : CognosCsvExportModule
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
