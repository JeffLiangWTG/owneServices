using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	abstract class AreaAbstractTest : TestCaseWithFactory
	{
		public virtual void TestVisualisationManagerHasAppropriateVisualisationRenderer()
		{
			AssertEquals(typeof(RendererGeneral), AreaVisualisationManagerFactory.New(GetNewAreaToTest()).VisualisationRenderer.GetType());
		}

		public void TestVisualisationManagerComponentsList()
		{
			var areaVisualisationManager = AreaVisualisationManagerFactory.New(GetNewAreaToTest());
			if (areaVisualisationManager == null)
			{
				Assert("We return null when the area does not have any visualisation enabled.", true);
			}
			else
			{
				AssertEquals("Preconditon: area.Components.Count", 0, areaVisualisationManager.Components.Count);

				var components = new List<VisualiserComponent>();
				components.Add(new VisualiserComponentLabel(new Point(5, 10), new Size(100, 20), "Caption the First", new CellFormat()));
				components.Add(new VisualiserComponentLabel(new Point(25, 70), new Size(150, 40), "Caption the Second", new CellFormat()));
				components.Add(new VisualiserComponentLabel(new Point(55, 110), new Size(200, 60), "Caption the Third", new CellFormat()));

				areaVisualisationManager.Components.AddRange(components);

				AssertEquals("area.Components.Count", 3, areaVisualisationManager.Components.Count);

				AssertEquals("area.Components[0].Location (5, 10)", new Point(5, 10), areaVisualisationManager.Components[0].Location);
				AssertEquals("area.Components[1].Size (150, 40)", new Size(150, 40), areaVisualisationManager.Components[1].Size);
				AssertEquals("Caption the Third", ((VisualiserComponentLabel)areaVisualisationManager.Components[2]).Caption);
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
			embeddedResourceRetriever?.Dispose();
			testReport?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		protected abstract Area GetNewAreaToTest();

		IDisposable temporarilyUseMainConnection;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		Report testReport;
		protected Report TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
					testReport = new Report(new DocumentPack(), excelTemplate);
					testReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					testReport.PrepareForRender();
					testReport.Renderer.CurrentPass = Passes.SecondPass;
				}
				return testReport;
			}
		}
	}
}
