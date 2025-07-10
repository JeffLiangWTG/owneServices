using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransferToRow))]
	public class APTransferToRowMatchingTest : TransferRowMatchingTest
	{
		protected override TransferRow GetNewTransferRow()
		{
			return Factory.New<APTransferToRow>();
		}
	}
}
