using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(TransactionMatchLinkGroup))]
	class TransactionMatchLinkGroupSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return new TransactionMatchLinkGroup(Factory);
		}
	}
}
