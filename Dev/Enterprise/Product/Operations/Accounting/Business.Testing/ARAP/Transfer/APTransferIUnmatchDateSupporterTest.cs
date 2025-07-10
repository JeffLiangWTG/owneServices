using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransfer))]
	public class APTransferIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override ITransaction GetNewObject()
		{
			return Transfer.New(GetExpectedObjectType(), Factory);
		}
	}
}
