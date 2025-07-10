using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ReportsGridFieldsLayoutBuilder<CusExitReport>))]
	class ReportsGridFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ReportsGridFieldsLayoutBuilder<CusExitReport>, CusExitReport, ReportsGridFieldsControlBag>
	{
		protected override ReportsGridFieldsLayoutBuilder<CusExitReport> GetColumnLayoutBuilderForTesting() => new ReportsGridFieldsLayoutBuilder<CusExitReport>();

		protected override int ExpectedMaxColumns => 3;
	}
}
