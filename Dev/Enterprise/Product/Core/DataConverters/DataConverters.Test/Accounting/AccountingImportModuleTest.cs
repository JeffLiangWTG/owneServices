using Enterprise.DataConverters.Accounting;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	[TestedType(typeof(AccountingImportModule))]
	sealed internal class AccountingImportModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ImportAccountingData;
		}
	}
}
