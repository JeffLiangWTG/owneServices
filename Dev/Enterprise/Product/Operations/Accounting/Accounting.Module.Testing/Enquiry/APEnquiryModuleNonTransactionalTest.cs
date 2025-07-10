using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	class APEnquiryModuleNonTransactionalTest : APTransactionModuleNonTransactionalTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APEnquiry;
		}
	}
}
