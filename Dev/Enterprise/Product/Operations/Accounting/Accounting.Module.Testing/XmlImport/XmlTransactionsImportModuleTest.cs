using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing.XmlImport
{
	[TestedType(typeof(XmlTransactionsImportModule))]
	class XmlTransactionsImportModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.XmlTransactionsImport;
		}

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			throw new ModuleGuiNotSupportedException("Controller uses PromptUserAndImport, so N/A for this test.");
		}
	}
}
