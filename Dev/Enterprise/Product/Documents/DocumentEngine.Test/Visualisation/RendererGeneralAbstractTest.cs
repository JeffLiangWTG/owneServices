using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	abstract class RendererGeneralAbstractTest : TestCaseWithFactory
	{
		protected Report testReport;
		protected Report TestReport
		{
			get
			{
				if (testReport == null)
				{
					using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
					{
						var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
						var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
						testReport = new Report(new DocumentPack(), excelTemplate);
						testReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
						testReport.PrepareForRender();
						testReport.Renderer.CurrentPass = Passes.SecondPass;
					}
				}
				return testReport;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateLinesTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			testReport?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;

		public abstract void TestGetHeight();
	}
}
