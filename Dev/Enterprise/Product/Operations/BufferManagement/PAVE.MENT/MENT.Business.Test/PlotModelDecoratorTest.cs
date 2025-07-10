using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class PlotModelDecoratorTest : TestCaseWithFactory
	{
		public void TestShowLegend()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TROY");

			var modelToDecorate = new PlotModel();

			var decorator = new PlotModelDecorator(visualisation, modelToDecorate, new LinearAxis(), new LinearAxis());
			decorator.Decorate();

			AssertEquals(true, modelToDecorate.IsLegendVisible);

			visualisation.ShowLegend = false;

			decorator.Decorate();

			AssertEquals(false, modelToDecorate.IsLegendVisible);
		}

		public void TestShowAcceptabilityBands()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ELMSTREET");
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			band.BAB_CautionLowerBound = 0;
			band.BAB_GoodLowerBound = 1;
			band.BAB_ExcellentLowerBound = 2;
			band.BAB_ExcellentUpperBound = 3;
			band.BAB_GoodUpperBound = 4;
			band.BAB_CautionUpperBound = 5;

			visualisation.Extraction.RelatedQuery.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			var modelToDecorate = new PlotModel();

			var decorator = new PlotModelDecorator(visualisation, modelToDecorate, new LinearAxis(), new LinearAxis());
			decorator.Decorate();

			var lineAnnotations = modelToDecorate.Annotations.OfType<LineAnnotation>();

			AssertEquals(0, lineAnnotations.Count());

			visualisation.ShowAcceptabilityBands = true;
			decorator.Decorate();

			lineAnnotations = modelToDecorate.Annotations.OfType<LineAnnotation>();

			AssertEquals(6, lineAnnotations.Count());
			AssertContainsExactElementsInAnyOrder(new double[] { 0d, 1d, 2d, 3d, 4d, 5d }, lineAnnotations.Select(a => a.X));
		}
	}
}
