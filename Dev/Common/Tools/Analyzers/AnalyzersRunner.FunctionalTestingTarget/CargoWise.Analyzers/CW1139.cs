//CW1139:Do Not Use Null Literal As Return In Coalesce Expression Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1139
	{
		public string Foo(string inputString)
		{
			return inputString ?? null;
		}
	}
}
