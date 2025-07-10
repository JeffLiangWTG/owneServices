using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionLineDetailsLayoutBuilder<CusIntrastatLine>))]
	sealed class TransactionLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransactionLineDetailsLayoutBuilder<CusIntrastatLine>, CusIntrastatLine, TransactionLineDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override TransactionLineDetailsLayoutBuilder<CusIntrastatLine> GetColumnLayoutBuilderForTesting() => new TransactionLineDetailsLayoutBuilder<CusIntrastatLine>();
	}
}
