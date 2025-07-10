using FlexCel.Render;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1016
	{
		public void Method()
		{
			//CW1016:Don't use FlexCelPdfExport
			var obj = new FlexCelPdfExport();
		}
	}
}
