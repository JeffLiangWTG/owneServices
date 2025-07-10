using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferFromRow))]
	public abstract class ARTransferRowTest : TransferRowTest
	{
		protected override TransferRow TestTransferRow
		{
			get { return Header as ARTransferFromRow; }
		}
	}
}
