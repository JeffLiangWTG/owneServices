using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransferFromRow))]
	public class APTransferFromRowMatchingTest : TransferRowMatchingTest
	{
		protected override TransferRow GetNewTransferRow()
		{
			return Factory.New<APTransferFromRow>();
		}
	}
}
