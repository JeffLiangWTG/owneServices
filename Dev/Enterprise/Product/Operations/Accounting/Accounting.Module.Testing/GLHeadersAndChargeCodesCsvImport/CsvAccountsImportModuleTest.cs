using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CsvAccountsImportModule))]
	public class CsvAccountsImportModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CsvAccountsImport, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Accountant, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ImportCsvChartOfAccounts, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CsvAccountsChartImportController), Module.GetNewController_ForTestOnly().GetType());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new CsvAccountsImportModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CsvAccountsImport;
		}

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			var csvModule = (CsvAccountsImportModule)module;
			var controller = (CsvAccountsChartImportController)csvModule.GetNewController_ForTestOnly();
			return controller.GetNewForm_ForTest();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected CsvAccountsImportModule Module;

		#endregion
	}
}
