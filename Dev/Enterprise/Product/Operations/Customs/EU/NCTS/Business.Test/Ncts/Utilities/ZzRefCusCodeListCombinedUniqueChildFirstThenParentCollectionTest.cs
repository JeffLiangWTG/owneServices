using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection))]
	sealed class ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadWithFilter()
		{
			const string testCodeType = "CODE1";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCode, "European Union");
			var itCode = Core.Constants.CountryCodes.Italy;
			var it = helper.CreateNewOrGetExistingDataGrouping(itCode, "Italy", eun);
			helper.CreateCusCodeType(testCodeType, "Ref Code");

			var euCode1 = helper.CreateCusCodeList(eunCode, testCodeType, "CODE_1", "CODE_1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(euCode1.PK, "LEVEL", "ITEM");

			var euCode2 = helper.CreateCusCodeList(eunCode, testCodeType, "CODE_N113", "CODE_N113 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(euCode2.PK, "LEVEL", "ITEM");

			var euCode3 = helper.CreateCusCodeList(eunCode, testCodeType, "CODE_N114", "CODE_N114 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(euCode2.PK, "LEVEL", "HEADER");

			helper.CreateCusCodeList(itCode, testCodeType, "IT_CODE_2", "IT_CODE_2 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(itCode, testCodeType, "CODE_1", "IT CODE_1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(itCode, testCodeType, "Code_N112", "IT Code_N112 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				var collection = new ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection(Factory, itCode, testCodeType, ZDateTime.Today, null, new[] { new RefCusCodeListAttributeFilter("LEVEL", SQLComparisonOperator.Equal, "ITEM") });
				collection.Load();
				AssertEquals("Item Count", 4, collection.Count);
				AssertContainsExactElementsInAnyOrder("No duplicate codes", new[] { "CODE_1", "IT_CODE_2", "CODE_N113", "Code_N112" }, collection.Select(c => c.ZZD_Code));

				var filter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.Contains, "N11");
				collection.Load(filter);
				AssertEquals("Item Count on applying filter", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder("Code from IT on applying filter", new[] { "Code_N112", "CODE_N113" }, collection.Select(c => c.ZZD_Code));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
			=> new ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection(Factory, Core.Constants.CountryCodes.Italy, "ABC", ZDateTime.Today.AddMonths(-1), null, null);
	}
}
