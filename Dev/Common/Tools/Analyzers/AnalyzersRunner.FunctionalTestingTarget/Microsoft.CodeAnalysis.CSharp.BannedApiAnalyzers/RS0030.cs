using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers
{
	class RS0030
	{
		public void Method()
		{
			//Banned in BannedSymbols.txt

			//RS0030:Do not used banned APIs
			_ = Math.Ceiling(4.2);

			//RS0030:Do not used banned APIs
			_ = Math.PI;

			//RS0030:Do not used banned APIs
			_ = Very.Extremely.Super.Bad.Thing.Name;

			//RS0030:Do not used banned APIs
			Very.Extremely.Super.Bad.Thing.Run();
		}
	}
}

namespace Very.Extremely.Super.Bad
{
	public static class Thing
	{
		public const string Name = "Bad Thing";

		public static void Run()
		{
		}
	}
}
