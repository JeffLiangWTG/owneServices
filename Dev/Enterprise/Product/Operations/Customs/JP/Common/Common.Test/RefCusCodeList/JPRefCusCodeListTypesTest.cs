using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPRefCusCodeListTypes))]
sealed class JPRefCusCodeListTypesTest : TestCaseWithFactory
{
	public void TestGetJapanBondedAreaCodes()
	{
		var list = JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory, Core.Constants.TransportModes.Sea, "12345");
		AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

		var collection = list as ZZRefCusCodeListCombinedCollection;

		CombineAssertions(() =>
		{
			AssertEquals("List Type:Property", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
			Assert("List Type:Property is not removable.", !collection.FilterBusinessObjectDefaults["List Type:Property"].IsRemovable);
			AssertEquals("Code:Property", "12345", collection.FilterBusinessObjectDefaults["Code:Property"].Value);
			Assert("Code:Property is removable", collection.FilterBusinessObjectDefaults["Code:Property"].IsRemovable);
			AssertEquals("Country/Region or Grouping:Property", Core.Constants.CountryCodes.Japan, collection.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value);
			Assert("Country/Region or Grouping is not removable.", !collection.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].IsRemovable);
			AssertEquals("Transport Mode:Property", Core.Constants.TransportModes.Sea, collection.FilterBusinessObjectDefaults["Transport Mode:Property"].Value);
			Assert("Transport Mode:Property is removable.", collection.FilterBusinessObjectDefaults["Transport Mode:Property"].IsRemovable);
			AssertEquals("Effective Date:Property1", ZDateTime.Today, collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
			Assert("Effective Date:Property1 is removable.", collection.FilterBusinessObjectDefaults["Effective Date:Property1"].IsRemovable);
		});
	}

	public void TestGetSpecialCargoCodes()
	{
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG", "EPG");

		var boCollection = JPRefCusCodeListTypes.GetSpecialCargoCodes(Factory);
		var testCollection = (BusinessObjectCollection)boCollection;
		var filters = testCollection.FilterBusinessObjectDefaults;
		var listTypeFilter = filters["List Type:Property"];
		var countryOrGroupingFilter = filters["Country/Region or Grouping:Property"];
		var descriptionFilter = filters["Description:Property"];
		CombineAssertions(() =>
		{
			AssertEquals("List Type", "SPC", listTypeFilter.Value);
			AssertEquals("Country/Region or Grouping", "JP", countryOrGroupingFilter.Value);
			AssertEquals("Description", ZString.Empty, descriptionFilter.Value);

			var codeListCollection = (ZZRefCusCodeListCombinedCollection)boCollection;
			codeListCollection.Load();
			AssertEquals("Count", 2, codeListCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AOG", "EPG" }, codeListCollection.Select(x => x.ZZD_Code));
		});
	}

	public void TestGetSpecialCargoCode()
	{
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG", "EPG");
		CombineAssertions(() =>
		{
			var specialCargoCode = JPRefCusCodeListTypes.GetSpecialCargoCode(Factory, "AOG");
			AssertEquals("AOG ZZD_CodeType", "SPC", specialCargoCode.ZZD_CodeType);
			AssertEquals("AOG ZZD_Description", "AOG Description", specialCargoCode.ZZD_Description);

			specialCargoCode = JPRefCusCodeListTypes.GetSpecialCargoCode(Factory, "EPG");
			AssertEquals("EPG ZZD_CodeType", "SPC", specialCargoCode.ZZD_CodeType);
			AssertEquals("EPG ZZD_Description", "EPG Description", specialCargoCode.ZZD_Description);
		});
	}
}
