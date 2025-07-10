// CW1106:Do Not Leave In Debug Messages Analyzer

using System;
using System.Diagnostics;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1106
	{
		public void WriteDebugMessages(bool test)
		{
			Console.Write("bad debug message 1");
			Console.WriteLine("bad debug message 2");
			Debug.Write("bad debug message 2");
			Debug.WriteIf(test, "bad debug message 3");
			Debug.WriteLineIf(test, "bad debug message 4");
		}
	}
}
