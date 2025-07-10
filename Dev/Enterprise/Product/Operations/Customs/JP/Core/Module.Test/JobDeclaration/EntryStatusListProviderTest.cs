using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Module.Test
{
	sealed class EntryStatusListProviderTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			var provider = new EntryStatusListProvider();
			AssertSame(Factory.GetCachedValue<CustomsStatusList>(), provider.EntryStatusList(Factory, Core.Constants.CountryCodes.Japan, string.Empty));
		}
	}
}
