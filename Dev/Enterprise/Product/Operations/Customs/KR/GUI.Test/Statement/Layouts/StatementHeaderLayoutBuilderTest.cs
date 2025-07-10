using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(StatementHeaderLayoutBuilder<CusStatementHeader>))]
	sealed class StatementHeaderLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<StatementHeaderLayoutBuilder<CusStatementHeader>, CusStatementHeader, StatementHeaderControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override StatementHeaderLayoutBuilder<CusStatementHeader> GetColumnLayoutBuilderForTesting() => new StatementHeaderLayoutBuilder<CusStatementHeader>();
	}
}
