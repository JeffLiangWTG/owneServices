using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	class AREnquiryModuleNonTransactionalTest : ARTransactionModuleNonTransactionalTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AREnquiry;
		}
	}
}
