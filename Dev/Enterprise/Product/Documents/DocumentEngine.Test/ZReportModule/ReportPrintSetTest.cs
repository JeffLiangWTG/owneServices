using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportPrintSetTest : TestCaseWithFactory
	{
		public void TestReportPrintSet()
		{
			ReportCommand reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report";

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (ReportPrintSet printSet = new ReportPrintSet(reportCommand))
			{
				AssertEquals("Underlying PrintTask has default delivery instructions PK", reportCommand.PK, printSet.DeliveryInstructionsDefaultPK);
				AssertEquals("DocumentPacks count", 1, printSet.Count);
			}
		}
	}
}
