using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CsvTransactionsImportModule))]
	class CsvTransactionsImportModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CsvTransactionsImport;
		}

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			var csvModule = (CsvTransactionsImportModule)module;
			var controller = csvModule.GetNewController_ForTest();

			return controller.GetNewForm_ForTest();
		}
	}
}
