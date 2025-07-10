using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(RefCusPackListProvider))]
sealed class RefCusPackListProviderTest : TestCaseWithFactory
{
	public void TestGetCustomsPackList()
	{
		var providers = ObjectFactory.Get<Hashtable>("RefCusPackListProviders");
		var objectHandle = (ObjectHandle)providers[Core.Constants.CountryCodes.Japan];
		var jpProvider = objectHandle?.GetObject() as RefCusPackListProvider;
		AssertNotNull(jpProvider);

		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BA");
		var collection = jpProvider.GetCustomsPackList(Factory, "GMB", "JP");
		AssertEquals(1, collection.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "BA" }, collection.GetAllCodes());
	}
}
