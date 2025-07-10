using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1103
	{
		public void Method(DateTime input)
		{
			//CW1103:Do Not Use String Literals For Date Formats
			input.ToString("dd-MMM-yy");
		}
	}
}
