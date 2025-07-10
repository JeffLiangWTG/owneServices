// CW1169 Do not use OxyPlot. It is not supported in Blazor and will be removed in web version.
using OxyPlot;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1169
	{
		public void BadCode()
		{
			_ = new PlotModel();
		}
	}
}
