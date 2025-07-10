using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public static class ResStringTestHelper
{
	public static void PutString(this IMockResourceStringCache resCache, string key, string text)
	{
		resCache.Put(key, new ResourceStringData(key, text));
	}
}
