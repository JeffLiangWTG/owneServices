using System.IO;
using NUnit.Framework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	public sealed class CW1051 : TestCase
	{
		public CW1051()
		{
			//CW1051:Do not use BaseSourcePath for resources
			_ = Path.Combine(BaseSourcePath, "subfolder");
		}
	}
}
