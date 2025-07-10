using System.Linq;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class CurrencyCodesTest : TestCase
	{
		public void TestAll()
		{
			var all = Constants.CurrencyCodes.All.ToArray();

			AssertCollectionContains(Constants.CurrencyCodes.Australia, all);
			AssertCollectionContains(Constants.CurrencyCodes.Canada, all);
			AssertCollectionContains(Constants.CurrencyCodes.China, all);
			AssertCollectionContains(Constants.CurrencyCodes.UnitedKingdom, all);
			AssertCollectionContains(Constants.CurrencyCodes.UnitedStates, all);
			AssertCollectionContains(Constants.CurrencyCodes.Singapore, all);
			Assert("All should return the list of currencies", all.Length > 150 && all.Length < 300);
		}
	}
}
