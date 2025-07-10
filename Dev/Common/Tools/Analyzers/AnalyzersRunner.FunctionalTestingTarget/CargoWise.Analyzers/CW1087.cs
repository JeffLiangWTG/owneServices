using System.Drawing.Printing;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1087
	{
		public void Method()
		{
			//CW1087:Do Not Use PrinterSettings.InstalledPrinters
			_ = PrinterSettings.InstalledPrinters;
		}
	}
}
