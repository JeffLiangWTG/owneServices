using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	static class CW1022
	{
		//CW1022:Thread Static Set In Static Initializer Rule
		[ThreadStatic]
		public static string field = "string value";
	}
}
