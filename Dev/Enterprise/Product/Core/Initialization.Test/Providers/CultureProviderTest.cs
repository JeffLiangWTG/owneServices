using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Initialisation.Testing
{
	sealed class CultureProviderTest : TestCase
	{
		public void TestCulture()
		{
			ICultureProvider provider = new CultureProvider();
			AssertEquals(Enterprise.ZArchitecture.Core.Culture.Current, provider.Culture);
		}
	}
}
