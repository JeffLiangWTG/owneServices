using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(VisualisationViewModel))]
	class VisualisationViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "tangine");
			return new VisualisationViewModel(visualisation.Extraction, visualisation, new MENTTestHelper.TestExtractorFactoryProvider());
		}

		public void TestPlotModelRefresh()
		{
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			{
				var visualisation = MENTTestHelper.CreateVisualisation(Factory, "tangine");
				visualisation.GraphTitle = "You wont see me";
				var extraction = visualisation.Extraction;

				foreach (SQLColumnSpecification column in extraction.SeriesColumns)
				{
					column.Selected = true;
				}

				var viewModel = new VisualisationViewModel(extraction, visualisation, new MENTTestHelper.TestExtractorFactoryProvider());
				var plotModel = viewModel.PlotModel;

				AssertEquals("You wont see me", plotModel.Title);
				visualisation.GraphTitle = "See meeee!";
				AssertEquals("Just changing a detail on a visualisation does not cause a change. You need to refresh the Plot model for the update to occur", "You wont see me", plotModel.Title);

				viewModel.RefreshModel();

				AssertEquals("See meeee!", viewModel.PlotModel.Title);
			}
		}
	}
}
