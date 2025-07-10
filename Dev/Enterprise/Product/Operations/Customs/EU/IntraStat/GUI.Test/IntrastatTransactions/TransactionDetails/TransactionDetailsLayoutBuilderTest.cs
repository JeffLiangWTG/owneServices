using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionDetailsLayoutBuilder<CusIntrastatHeader>))]
	sealed class TransactionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransactionDetailsLayoutBuilder<CusIntrastatHeader>, CusIntrastatHeader, TransactionDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override TransactionDetailsLayoutBuilder<CusIntrastatHeader> GetColumnLayoutBuilderForTesting() => new TransactionDetailsLayoutBuilder<CusIntrastatHeader>();
	}
}
