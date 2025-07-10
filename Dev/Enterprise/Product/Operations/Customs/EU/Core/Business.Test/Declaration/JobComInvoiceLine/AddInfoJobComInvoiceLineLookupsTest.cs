using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValuationAdjustmentCodeList()
		{
			var valuationAdjustmentCodeList = lookups.ValuationAdjustmentCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "L, K, D, I, B, G, M, E, J, C, H, A, F", valuationAdjustmentCodeList.CodesAsString);
				AssertSame("Cached", valuationAdjustmentCodeList, Factory.GetCachedValue<ValuationAdjustmentCodeList>());
			});
		}

		[TestDate(2021, 03, 29)]
		public void TestCountriesOfDestination_Imp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "European Countries Of Destination");
			helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CN", "CN", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
			helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "AU", "AU", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
			helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "IE", "IE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 02, 01));
			Factory.Save();

			declaration.JE_MessageType = "IMP";
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CountriesOfDestination;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "CN", "AU" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CountriesOfDestination);
			});
		}

		public void TestCountriesOfDestination_Exp()
		{
			declaration.JE_MessageType = "EXP";
			AssertType<RefCountryCollection>(lookups.CountriesOfDestination);
		}

		public void TestCountriesOfDispatch()
		{
			AssertType<RefCountryCollection>(lookups.CountriesOfDispatch);
		}

		[TestDate(2021, 03, 29)]
		public void TestCusNumberList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(latviaCountryCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-1", "DESC1", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-2", "DESC2", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-3", "DESC3", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 02, 01));
			Factory.Save();

			var list = lookups.CusNumberList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "0018137-1", "0018137-2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CusNumberList);
			});
		}

		[TestDate(2023, 09, 25)]
		public void TestCusNumberList_InvalidTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
			var ieCountryCode = Core.Constants.CountryCodes.Ireland;
			helper.CreateNewOrGetExistingDataGrouping(ieCountryCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(ieCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-1", "DESC1", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 12, 31));
			helper.CreateCusCodeList(ieCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-2", "DESC2", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 12, 31));
			helper.CreateCusCodeList(ieCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-3", "DESC3", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 02, 01));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "XXXXXXX";
			lookups = invoiceLine.AddInfoLookups;
			var list = lookups.CusNumberList;
			list.Load();
			AssertEquals("Invalid tariff - should not have any CusCodes", 0, list.Count);
		}

		public void TestTransactionNatureList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var grouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
				var netherlandsCountryCode = Core.Constants.CountryCodes.Netherlands;
				helper.CreateNewOrGetExistingDataGrouping(netherlandsCountryCode, parent: grouping);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Transaction Nature");
				helper.CreateNewOrGetExistingCusCodeList(netherlandsCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "11", "Outright purchase/sale", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01));
				helper.CreateNewOrGetExistingCusCodeList(netherlandsCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "22", "Replacement of returned goods", new ZDateTime(2021, 01, 01), new ZDateTime(2079, 01, 01));
				Factory.Save();

				var transactionNatureList = lookups.TransactionNatureList;

				CombineAssertions(() =>
				{
					AssertNotNull(transactionNatureList);
					AssertType<CodeDescriptionPairList>(transactionNatureList);
					AssertEquals("Number of elements", 2, transactionNatureList.Count);
					Assert("Contains code 11", transactionNatureList.ContainsCode("11"));
					Assert("Contains code 22", transactionNatureList.ContainsCode("22"));
				});
			}
		}

		public void TestRegionOfDestinationList()
		{
			AssertEquals(string.Empty, ((CodeDescriptionPairList)lookups.RegionOfDestinationList).CodesAsString);
		}

		public void TestRelatedIndicatorList()
		{
			AssertSame(Factory.GetCachedValue<ValuationIndicatorCodeList>(), lookups.RelatedIndicatorList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			lookups = declaration.Invoices.AddNew().InvoiceLines.AddNew().AddInfoLookups;
		}
		JobDeclaration declaration;
		AddInfoJobComInvoiceLineLookups lookups;
	}
}
