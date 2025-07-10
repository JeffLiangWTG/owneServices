using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsReceiveLine))]
	sealed class DocWhsReceiveLineTest : DocWhsDocketLineTest<WhsReceive, WhsReceiveLine, DocWhsReceiveLine>
	{
		protected override DocWhsReceiveLine CreateDocketLineWrapper(WhsReceiveLine docketLine)
		{
			return DocWhsReceiveLine.New(docketLine, Factory);
		}
	}
}
