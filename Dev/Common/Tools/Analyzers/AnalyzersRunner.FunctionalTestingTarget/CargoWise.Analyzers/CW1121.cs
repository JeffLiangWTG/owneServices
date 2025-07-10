//CW1121:Do Not Include Column Values Or Names In Error Reporter Key Analyzer

using System;
using CargoWise.Common;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1121
	{
		public void SomeReport(string message, Exception ex)
		{
			var pk_TABLENAME = "PK_TABLENAME";
			ErrorReporter.ReportOnce(pk_TABLENAME, message, ex);
		}
	}
}
