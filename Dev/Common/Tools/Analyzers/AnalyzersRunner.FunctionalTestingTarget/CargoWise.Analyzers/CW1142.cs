//CW1142:Await Expressions In Assert That Argument Analyzer

using System.Threading.Tasks;
using NUnit.Framework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1142
	{
		async Task<bool> ResolveTrue()
		{
			await Task.Delay(1000);
			return true;
		}

		[Test]
		public async Task TestResolveTrue()
		{
			Assert.That(await ResolveTrue(), Is.True.After(2000, 200));
		}
	}
}
