using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	sealed class CusIntrastatLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMassInKilogramsUnits()
		{
			AssertType<CodeDescriptionPairList>(lookups.MassInKilogramsUnits);
			AssertEquals("Used to satisfy [List] requirement when binding readonly unit", 0, lookups.MassInKilogramsUnits.Count);
		}

		public void TestRegions()
		{
			AssertType<CodeDescriptionPairList>(lookups.Regions);
			AssertEquals("Not defined for EU yet", 0, lookups.Regions.Count);
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "lv", eun);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "2", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "3", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "4", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var list = lookups.CustomsUQList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 2", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsUQList);
			});
		}

		protected override void SetUp()
		{
			lookups = IntrastatTestDataHelper.New(Factory).NewCusIntrastatLineWithValidData().Lookups;
		}

		CusIntrastatLineLookups lookups;
	}
}
