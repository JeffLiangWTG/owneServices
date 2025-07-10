using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APTransferFromRow))]
	public abstract class APTransferRowTest : TransferRowTest
	{
		protected override TransferRow TestTransferRow
		{
			get { return Header as APTransferFromRow; }
		}
	}
}
