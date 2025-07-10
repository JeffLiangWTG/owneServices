using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceNumbers_Departure()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";

			var guaranteeHeader1 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = "TRA";
			guaranteeHeader1.CPH_Number = "EU1";
			guaranteeHeader1.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader1.CPH_StartDate = ZDate.Today;
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader2 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Type = "COD";
			guaranteeHeader2.CPH_Number = "EU2";
			guaranteeHeader2.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader2.CPH_StartDate = ZDate.Today;
			guaranteeHeader2.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader3 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader3.CPH_Type = "COD";
			guaranteeHeader3.AdditionalGuaranteeReferences.AddNew("D1", "test");
			guaranteeHeader3.CPH_Number = "EU3";
			guaranteeHeader3.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader3.CPH_StartDate = ZDate.Today;
			guaranteeHeader3.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader4 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader4.CPH_Type = "COD";
			guaranteeHeader4.AdditionalGuaranteeReferences.AddNew("B1", "test");
			guaranteeHeader4.CPH_Number = "EU4";
			guaranteeHeader4.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader4.CPH_StartDate = ZDate.Today;
			guaranteeHeader4.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader5 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader5.CPH_Type = "IMP";
			guaranteeHeader5.CPH_Number = "EU5";
			guaranteeHeader5.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader5.CPH_StartDate = ZDate.Today;
			guaranteeHeader5.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader6 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader6.CPH_Type = "TRA";
			guaranteeHeader6.CPH_Number = "NONEU";
			guaranteeHeader6.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			guaranteeHeader6.CPH_StartDate = ZDate.Today;
			guaranteeHeader6.CPH_OH_PermitHolder = orgHeader.PK;
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.Guarantees.AddNew();

			var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Collection elements for Phase 4", new[] { "EU1", "EU2", "EU3", "EU4", "EU5" }, referenceNumbers.Select(x => x.CPH_Number));

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				guarantee = header.MovementHeader.Guarantees.AddNew();
				referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				AssertContainsExactElementsInAnyOrder("Collection elements for Phase 5", new[] { "EU1", "EU2", "EU3" }, referenceNumbers.Select(x => x.CPH_Number));
				AssertEquals("TRA Should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader1.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("COD should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader2.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("COD with reference starting with 'D' should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader3.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("COD with reference starting with 'B' should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader4.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("IMP should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader5.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("A NON EU country record should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader6.PK).MatchesFilter(referenceNumbers.CompleteFilter));
			});
		}

		public void TestReferenceNumbers_NoC0009Code()
		{
			var guaranteeHeader1 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "G1";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
			AssertEquals(true, referenceNumbers.CompleteFilter.IsNoResultQuery);
		}

		public void TestReferenceNumbers_Arrival()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";

			var guaranteeHeader1 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = "TST";
			guaranteeHeader1.CPH_Number = "EU1";
			guaranteeHeader1.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader1.CPH_StartDate = ZDate.Today;
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader2 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Type = "TST";
			guaranteeHeader2.CPH_Number = "EU2";
			guaranteeHeader2.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader2.CPH_StartDate = ZDate.Today;
			guaranteeHeader2.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader3 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader3.CPH_Type = "TST";
			guaranteeHeader3.AdditionalGuaranteeReferences.AddNew("D1", "test");
			guaranteeHeader3.CPH_Number = "EU3";
			guaranteeHeader3.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader3.CPH_StartDate = ZDate.Today;
			guaranteeHeader3.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader4 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader4.CPH_Type = "TST";
			guaranteeHeader4.AdditionalGuaranteeReferences.AddNew("B1", "test");
			guaranteeHeader4.CPH_Number = "EU4";
			guaranteeHeader4.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader4.CPH_StartDate = ZDate.Today;
			guaranteeHeader4.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader5 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader5.CPH_Type = "IMP";
			guaranteeHeader5.CPH_Number = "EU5";
			guaranteeHeader5.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader5.CPH_StartDate = ZDate.Today;
			guaranteeHeader5.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader6 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader6.CPH_Type = "TRA";
			guaranteeHeader6.CPH_Number = "EU6";
			guaranteeHeader6.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader6.CPH_StartDate = ZDate.Today;
			guaranteeHeader6.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader7 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader7.CPH_Type = "COD";
			guaranteeHeader7.AdditionalGuaranteeReferences.AddNew("D2", "test");
			guaranteeHeader7.CPH_Number = "EU7";
			guaranteeHeader7.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader7.CPH_StartDate = ZDate.Today;
			guaranteeHeader7.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader8 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader8.CPH_Type = "TST";
			guaranteeHeader8.CPH_Number = "NONEU";
			guaranteeHeader8.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			guaranteeHeader8.CPH_StartDate = ZDate.Today;
			guaranteeHeader8.CPH_OH_PermitHolder = orgHeader.PK;
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var guarantee = header.Guarantees.AddNew();

			var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Collection elements for Phase 4", new[] { "EU1", "EU2", "EU3", "EU4", "EU5", "EU6", "EU7" }, referenceNumbers.Select(x => x.CPH_Number));

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				guarantee = header.Guarantees.AddNew();
				referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				AssertContainsExactElementsInAnyOrder("Collection elements for Phase 5", new[] { "EU1", "EU2", "EU3", "EU4" }, referenceNumbers.Select(x => x.CPH_Number));
				AssertEquals("Only TST Should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader1.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("TST with reference starting with 'D' should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader3.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("TST with reference starting with 'B' should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader4.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("IMP should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader5.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("TRA should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader6.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("COD should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader7.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("A NON EU country record should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader6.PK).MatchesFilter(referenceNumbers.CompleteFilter));
			});
		}

		public void TestReferenceNumbers_FilterBusinessObjectDefaults()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "ABC123";
			CombineAssertions("NCTS Departure", () =>
			{
				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				var filterBusinessObjectDefaults = referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Where(x =>
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolder ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber)
					.GroupBy(x => x.FilterName).ToDictionary(x => x.Key, x => x.ToList());
				AssertEquals(2, filterBusinessObjectDefaults.Count);
				AssertEquals("Guarantee Number is defaulted", "ABC123", filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber].Single().Value);

				var guaranteeHoldersDefaults = filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders];
				AssertEquals("PrimaryGuaranteeHolderAddress is Empty", ZGuid.Empty, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property1").Value);
				AssertEquals("SecondaryGuaranteeHolderAddress is null", null, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property2").Value);

				header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
				referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				filterBusinessObjectDefaults = referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Where(x =>
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolder ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber)
					.GroupBy(x => x.FilterName).ToDictionary(x => x.Key, x => x.ToList());
				AssertEquals(2, filterBusinessObjectDefaults.Count);
				AssertEquals("Guarantee Number is defaulted", "ABC123", filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber].Single().Value);

				guaranteeHoldersDefaults = filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders];
				AssertEquals("Principal is set, PrimaryGuaranteeHolderAddress is not Empty", orgHeader.PK, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property1").Value);
				AssertEquals("SecondaryGuaranteeHolderAddress is null", null, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property2").Value);
			});

			CombineAssertions("NCTS Departure Phase 5", () =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var guarantee = header.MovementHeader.Guarantees.AddNew();
				guarantee.PW_BondType = "1";
				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				AssertEquals("Guarantee Subtype", "1", referenceNumbers.FilterBusinessObjectDefaults["Guarantee Subtype:Property"].Value);
				AssertEquals("Guarantee Type not found", false, referenceNumbers.FilterBusinessObjectDefaults.ContainsDefaultFor("Guarantee Type:Property"));
			});

			CombineAssertions("NCTS Arrival", () =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.ArrivalMovementHeader.GoodsLocation.AdditionalIdentifier = "ES009999AAAAAA";
				var guarantee = header.Guarantees.AddNew();
				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				AssertEquals("Guarantee Type", "TST", referenceNumbers.FilterBusinessObjectDefaults["Guarantee Type:Property"].Value);
				AssertEquals("Guarantee Rule Code", "TSP", referenceNumbers.FilterBusinessObjectDefaults["Guarantee Rule:Property1"].Value);
				AssertEquals("Guarantee Rule Value From", "ES009999AAAAAA", referenceNumbers.FilterBusinessObjectDefaults["Guarantee Rule:Property2"].Value);
			});
		}

		public void TestReferenceNumbers_Standalone()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			AssertNoExceptionThrown("When NctsHeader is null", () =>
			{
				_ = guarantee.Lookups.ReferenceNumbers;
			});
		}

		public void TestLiabilityApplicablePercentageCodeList()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			var liabilityTypeList = guarantee.Lookups.LiabilityApplicablePercentageCodeList;
			CombineAssertions(() =>
			{
				AssertType<LiabilityApplicablePercentageCodeList>("Type", liabilityTypeList);
				AssertSame("Cache", liabilityTypeList, guarantee.Lookups.LiabilityApplicablePercentageCodeList);
			});
		}

		public void TestBondTypeList_Phase4()
		{
			CreateCreateCusCodeList("EUN", "CL251", new List<ZString>() { "A", "B", "C" });
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var guarantee = header.Guarantees.AddNew();
			var bondTypeList = guarantee.Lookups.BondTypeList;
			AssertEquals("BondTypeList when not IsPhase5Departure", "3, 1, 4, 8, 7, 6, 0, A, 5, 2, 9, B", bondTypeList.CodesAsString);
		}

		public void TestBondTypeList_Phase5()
		{
			CreateCreateCusCodeList("EUN", "CL251", new List<ZString>() { "A", "B", "C" });
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var guarantee = header.MovementHeader.Guarantees.AddNew();
			var bondTypeList = guarantee.Lookups.BondTypeList;
			bondTypeList.Sort();
			AssertEquals("BondTypeList when IsPhase5Departure", "A, B, C", bondTypeList.CodesAsString);
		}

		void CreateCreateCusCodeList(ZString dataGroupingCode, ZString codeType, IEnumerable<ZString> codes)
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: dataGrouping);
			codes.ForEach(code =>
			{
				helper.CreateCusCodeList(dataGroupingCode, codeType, code, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			});
		}
	}
}
