using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransfer))]
	public class ARTransferIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override ITransaction GetNewObject()
		{
			return Transfer.New(GetExpectedObjectType(), Factory);
		}
	}
}
