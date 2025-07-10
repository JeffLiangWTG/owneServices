using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache.Testing
{
	class ResourceStringCacheBuilderTest : TestCase
	{
		public void TestReleaseModeResourceStringCacheBuilder()
		{
			var builder = new ResourceStringCacheBuilder(null);
			var engCache = builder.GetResourceStringCache("EN-US");
			AssertType(typeof(EmptyResourceStringCache), engCache);

			var frnCache = builder.GetResourceStringCache("FR-FR");
			AssertType(typeof(ZrsResourceStringCache), frnCache);
			AssertEquals("FR-FR", frnCache.Language);
		}
	}
}
