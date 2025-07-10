using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.Module.Testing;

sealed class EntryStatusListProviderTest : TestCaseWithFactory
{
	public void TestEntryStatusList()
	{
		var provider = new EntryStatusListProvider();
		var list = provider.EntryStatusList(Factory, Core.Constants.CountryCodes.Switzerland, string.Empty);
		AssertSame(CommonLookups.CustomsStatusList(Factory), list);
	}
}

