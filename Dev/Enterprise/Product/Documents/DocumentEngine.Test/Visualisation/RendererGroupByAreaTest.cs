using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class RendererGroupByAreaTest : RendererGeneralAbstractTest
	{
		// Complete end to end concrete testing for resulting functionality present in:
		// TemplateToVisualiserComponentsConverterTest.TestVisualiserDealsWithFieldsInGroupHeaders()

		public override void TestGetHeight()
		{
			var bodyArea = new SectionBodyArea(1, 5, TestReport, "#SectionBody:Data=Test");
			var groupByArea = new GroupByArea(6, 10, TestReport, "#GroupBy:Test.AccountingGroupCode");
			groupByArea.SectionBody = bodyArea;
			var groupByRenderer = new RendererGroupByArea(groupByArea);
			AssertEquals("groupByRenderer.GetHeight()", 0, groupByRenderer.GetHeightInXL());
		}

		public void TestGroupInsertsFieldFromHeaderAndBody()
		{
			var bodyArea = new SectionBodyArea(1, 5, TestReport, "#SectionBody:Data=Test");
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<Test.Description>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Test.GSTRate> <Test.AmountExTax>";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";

			var tablesAdded = new List<string>();
			var dS = new VisualiserDataSet();
			var bodyRenderer = new RendererSectionBody(bodyArea);
			bodyRenderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, tablesAdded);

			var groupByArea = new GroupByArea(6, 10, TestReport, "#GroupBy:Test.AccountingGroupCode");
			TestReport.WorkSheetCurrentlyBeingProcessed[7, 2] = "<Test.AccountingGroupName>";
			groupByArea.SectionBody = bodyArea;

			var groupByRenderer = new RendererGroupByArea(groupByArea);
			groupByRenderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, tablesAdded);

			AssertEquals(2, dS.Tables.Count);
			AssertEquals(5, dS.Tables[1].Columns.Count);
			AssertEquals("Description", dS.Tables[1].Columns[0].ColumnName);
			AssertEquals("GSTRate", dS.Tables[1].Columns[1].ColumnName);
			AssertEquals("AmountExTax", dS.Tables[1].Columns[2].ColumnName);
			AssertEquals("AccountingGroupCode", dS.Tables[1].Columns[3].ColumnName);
			AssertEquals("AccountingGroupName", dS.Tables[1].Columns[4].ColumnName);
		}
	}
}
