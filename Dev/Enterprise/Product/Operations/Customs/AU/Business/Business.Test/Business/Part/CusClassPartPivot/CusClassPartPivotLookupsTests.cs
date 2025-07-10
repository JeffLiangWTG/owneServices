using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassPartPivotLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestClassificationTypes()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(2, pivot.Lookups.ClassificationTypes.Count);
			AssertEquals(true, pivot.Lookups.ClassificationTypes.ContainsCode("HTI"));
			AssertEquals(true, pivot.Lookups.ClassificationTypes.ContainsCode("HTE"));
		}

		public void TestClassificationList()
		{
			var newFactory = new BusinessObjectFactory();
			var class1 = newFactory.New<Classification>();
			class1.CC_LookupCode = "EXPLookup";
			class1.CC_ClassificationType = Classification.ClassificationType.EXP;
			class1.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var class2 = newFactory.New<Classification>();
			class2.CC_LookupCode = "IMPLookup";
			class2.CC_ClassificationType = Classification.ClassificationType.IMP;
			class2.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			newFactory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			CombineAssertions(() =>
			{
				{
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
					var collection = pivot.Lookups.ClassificationList;
					collection.Load();
					AssertEquals("1", 1, collection.Count);
					AssertEquals("2", true, collection is ExportClassificationCollection);
				}
				{
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					var collection = pivot.Lookups.ClassificationList;
					collection.Load();
					AssertEquals("3", 1, collection.Count);
					AssertEquals("4", true, collection is ImportClassificationCollection);
				}
				{
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
					var collection = pivot.Lookups.ClassificationList;
					collection.Load();
					AssertEquals("5", 2, collection.Count);
					AssertEquals("6", false, collection is ExportClassificationCollection);
					AssertEquals("7", false, collection is ImportClassificationCollection);
					AssertEquals("8", true, collection is BaseClassificationCollection<Classification>);
				}
				{
					pivot.CI_ChildType = "XXX";
					var collection = pivot.Lookups.ClassificationList;
					collection.Load();
					AssertEquals("9", 2, collection.Count);
					AssertEquals("10", false, collection is ExportClassificationCollection);
					AssertEquals("11", false, collection is ImportClassificationCollection);
					AssertEquals("12", true, collection is BaseClassificationCollection<Classification>);
				}
				{
					pivot.CI_ChildType = "";
					var collection = pivot.Lookups.ClassificationList;
					collection.Load();
					AssertEquals("13", 2, collection.Count);
					AssertEquals("14", false, collection is ExportClassificationCollection);
					AssertEquals("15", false, collection is ImportClassificationCollection);
					AssertEquals("16", true, collection is BaseClassificationCollection<Classification>);
				}
			});
		}

		public void TestTariffs()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			Assert("Lookups is CusClassPartPivotLookups", pivot.Lookups is CusClassPartPivotLookups);
			AssertEquals("Lookups.Tariffs is ClassificationList", pivot.Lookups.Tariffs, pivot.Lookups.ClassificationList);
		}
	}
}
