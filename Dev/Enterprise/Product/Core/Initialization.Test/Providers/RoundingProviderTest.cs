using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Initialisation.Testing
{
	sealed class RoundingProviderTest : TestCase
	{
		public void TestRound()
		{
			var provider = new RoundingProvider();
			AssertEquals(Utilities.Round(1.234m, 2), provider.Round(1.234m, 2));
			AssertEquals(Utilities.Round(1.239m, 2), provider.Round(1.239m, 2));
			AssertEquals(Utilities.Round(1.299m, 2), provider.Round(1.299m, 2));
		}
	}
}
