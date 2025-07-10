using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing.XmlExport
{
	[TestedType(typeof(TransactionsExportModule))]
	class TransactionsExportModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TransactionsExport;
		}
	}
}
