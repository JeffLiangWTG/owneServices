using CargoWise.BuildTools;

namespace Enterprise.Builder.Generator.Cache
{
	public class SolutionCache : GenericConcurrentCache<CSharpSolution>
	{
		SolutionCache() { }

		static SolutionCache()
		{
			Instance = new SolutionCache();
		}

		public static SolutionCache Instance { get; private set; }

		public CSharpSolution GetSolution(string solutionFileName)
		{
			return Get(solutionFileName);
		}

		protected override CSharpSolution GeneratorFunc(string solutionName)
		{
			return new CSharpSolution(solutionName);
		}
	}
}
