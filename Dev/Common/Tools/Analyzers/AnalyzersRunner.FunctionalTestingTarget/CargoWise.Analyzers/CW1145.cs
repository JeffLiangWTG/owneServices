using WTG.StaticAnalysis.Annotation;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1145
	{
		static readonly CW1145ForTest instance = new CW1145ForTest();

		// CW1145 Singletons should be accesed by a static property named Instance
		public CW1145ForTest Instance => instance;

		// CW1145 Singletons should be accesed by a static property
		public static CW1145ForTest GetInstance()
		{
			return instance;
		}
	}

	[Immutable]
	class CW1145ForTest
	{
	}
}
