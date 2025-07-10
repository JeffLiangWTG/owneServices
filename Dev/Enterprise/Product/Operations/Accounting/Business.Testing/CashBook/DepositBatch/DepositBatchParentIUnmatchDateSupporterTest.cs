using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchParent))]
	public class DepositBatchParentIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override Base.Interfaces.ITransaction GetNewObject()
		{
			return new DepositBatchParent(Factory);
		}
	}
}
