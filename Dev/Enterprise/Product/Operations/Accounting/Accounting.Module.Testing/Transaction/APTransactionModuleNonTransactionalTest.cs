using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	class APTransactionModuleNonTransactionalTest : TransactionModuleStripNonTransactionalTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APTransaction;
		}
	}
}
