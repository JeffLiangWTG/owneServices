using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	class ARTransactionModuleNonTransactionalTest : TransactionModuleStripNonTransactionalTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARTransaction;
		}
	}
}
