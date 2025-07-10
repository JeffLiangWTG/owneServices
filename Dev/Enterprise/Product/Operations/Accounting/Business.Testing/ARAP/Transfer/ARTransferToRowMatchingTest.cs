using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferToRow))]
	public class ARTransferToRowMatchingTest : TransferRowMatchingTest
	{
		protected override TransferRow GetNewTransferRow()
		{
			return Factory.New<ARTransferToRow>();
		}
	}
}
