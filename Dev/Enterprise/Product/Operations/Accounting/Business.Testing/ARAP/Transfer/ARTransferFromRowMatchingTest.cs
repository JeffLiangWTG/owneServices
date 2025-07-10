using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferFromRow))]
	public class ARTransferFromRowMatchingTest : TransferRowMatchingTest
	{
		protected override TransferRow GetNewTransferRow()
		{
			return Factory.New<ARTransferFromRow>();
		}
	}
}
