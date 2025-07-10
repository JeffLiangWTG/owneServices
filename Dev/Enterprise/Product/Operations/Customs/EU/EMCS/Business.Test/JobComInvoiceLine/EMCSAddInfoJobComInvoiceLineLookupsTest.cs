using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoJobComInvoiceLineLookupsTest : EUEMCSAddInfoLookupsTest
	{
		public void TestExciseProductCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "B000", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "C000", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "C000", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			Factory.Save();

			var list = lookups.ExciseProductCodes;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "B000, C000", list.CodesAsString);
				AssertEquals("Cached", list, lookups.ExciseProductCodes);
			});
		}

		public void TestCountryOfOrigins()
		{
			AssertType<RefCountryCollection>(lookups.CountryOfOrigins);
		}

		public void TestGrowingZoneList()
		{
			var growingZoneList = lookups.GrowingZoneList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "1, 2, 3, 4, 5, 6", growingZoneList.CodesAsString);
				AssertSame("Cached", growingZoneList, lookups.GrowingZoneList);
			});
		}

		public void TestWineCategoryList()
		{
			var wineCategoryList = lookups.WineCategoryList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "1, 2, 3, 4, 5", wineCategoryList.CodesAsString);
				AssertSame("Cached", wineCategoryList, lookups.WineCategoryList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<EMCSJobDeclaration>().InvoiceHeader.InvoiceLines.AddNew();
			lookups = invoiceLine.AddInfoLookups;
		}
		EMCSJobComInvoiceLine invoiceLine;
		EMCSAddInfoJobComInvoiceLineLookups lookups;
	}
}
