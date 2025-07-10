using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CustomsConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageLineLookupsBaseOnlyTest : BusinessObjectLookupsTestCase
	{
		public void TestCustodianIdentifierBranchNoList()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				TestHelper.CreateCL010CoutryList(Factory);
				AssertEquals("CustodianIdentifier empty", 0, storageLine.Lookups.CustodianIdentifierBranchNoList.Count);
				Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("Test", "12345", Core.Constants.CountryCodes.Greece, new ZString[] { "0001", "00002" });
				storageLine.TSL_CustodianIdentifier = "GR12345";
				var custodianIdentifierBranchNoList = storageLine.Lookups.CustodianIdentifierBranchNoList;
				AssertEquals("CustodianIdentifier populated", "0001", custodianIdentifierBranchNoList.CodesAsString);
				AssertSame("Cached", custodianIdentifierBranchNoList, Factory.GetCachedValue("DE|BranchNoList|GR12345", () => new CodeDescriptionPairList()));
			});
		}

		public void TestGoodsOwnerIdentifierBranchNoList()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				TestHelper.CreateCL010CoutryList(Factory);
				AssertEquals("GoodsOwnerIdentifier", 0, storageLine.Lookups.GoodsOwnerIdentifierBranchNoList.Count);
				Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("Test", "12345", Core.Constants.CountryCodes.Greece, new ZString[] { "0001", "00002" });
				storageLine.TSL_GoodsOwnerIdentifier = "GR12345";
				var goodsOwnerIdentifierBranchNoList = storageLine.Lookups.GoodsOwnerIdentifierBranchNoList;
				AssertEquals("GoodsOwnerIdentifier populated", "0001", goodsOwnerIdentifierBranchNoList.CodesAsString);
				AssertSame("Cached", goodsOwnerIdentifierBranchNoList, Factory.GetCachedValue("DE|BranchNoList|GR12345", () => new CodeDescriptionPairList()));
			});
		}

		public void TestGoodsTypeList()
		{
			CombineAssertions(() =>
			{
				var goodsTypeList = storageLine.Lookups.GoodsTypeList;
				AssertEquals("Codes", "A, B, C, D, E, F, G, H, I, J, K", goodsTypeList.CodesAsString);
				AssertSame("Cached", goodsTypeList, Factory.GetCachedValue<DEGoodsTypeList>());
			});
		}

		public void TestPackageTypeList()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(CustomsConstants.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package code List");
			helper.CreateNewOrGetExistingCusCodeList(CustomsConstants.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, CustomsConstants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VQ", "VOLUME PACKAGE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(CustomsConstants.RefDataGrouping.Codes.UnitedNationsRecommendations, CustomsConstants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VQ", "VOLUME PACKAGE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var packageTypeList = storageLine.Lookups.PackageTypeList;
				AssertEquals("Codes", "VQ", packageTypeList.CodesAsString);
				AssertEquals("Code and Description", "VOLUME PACKAGE", packageTypeList["VQ"].Description);
			});
		}

		public void TestLocationOfGoodsList()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "01", "01 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "02", "02 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var custodianOrg = Factory.NewWithValidTestData<OrgHeader>();
			var authorisation = custodianOrg.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER1");
			var authorisationRule = authorisation.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "01");
			authorisationRule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");
			var authorisation2 = custodianOrg.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER2");
			var authorisationRule2 = authorisation2.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "02");
			authorisationRule2.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000002");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Custodian unset, no customs office: Count", 0, storageLine.Lookups.LocationOfGoodsList.Count);
				storageLine.Dec.StorageHeader.SJH_CustomsOffice = "DE000001";
				AssertEquals("CUstodian unset, has customs office: Count", 0, storageLine.Lookups.LocationOfGoodsList.Count);

				storageLine.TSL_OA_Custodian = custodianOrg.Addresses[0].PK;
				AssertEquals("Custodian set, Matched CustomsOffice", "01", storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				storageLine.Dec.StorageHeader.SJH_CustomsOffice = "DE000002";
				AssertEquals("Custodian set, Different CustomsOffice", "02", storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				storageLine.Dec.StorageHeader.SJH_CustomsOffice = "NOMATCH";
				AssertEquals("Custodian set, CustomsOffice mismatch", ZString.Empty, storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				storageLine.TSL_OA_Custodian = ZGuid.Empty;
				AssertEquals("No Custodian", ZString.Empty, storageLine.Lookups.LocationOfGoodsList.CodesAsString);
			});
		}

		public void TestCountries()
		{
			CombineAssertions(() =>
			{
				var countries = storageLine.Lookups.Countries;
				AssertType<RefCountryCollection>("Type", countries);
				AssertEquals("Not cached", false, ReferenceEquals(countries, storageLine.Lookups.Countries));
			});
		}

		public void TestCustodiansAndTraders()
		{
			CombineAssertions(() =>
			{
				var custodiansAndTraders = storageLine.Lookups.CustodiansAndTraders;
				AssertType<OrganisationsFindBoxCollection>("Type", custodiansAndTraders);
				AssertEquals("Not loaded", 0, custodiansAndTraders.Count);
				AssertEquals("Not cached", false, ReferenceEquals(custodiansAndTraders, storageLine.Lookups.CustodiansAndTraders));
			});
		}

		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AWB, REG, SIN, ULD, ZZZ", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue<OwnerReferenceTypeList>());
			});
		}

		public void TestUnionStatusList()
		{
			var unionStatusList = storageLine.Lookups.UnionStatusList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", unionStatusList, Factory.GetCachedValue<DEUnionStatusList>());
				AssertEquals("Values", "C, D, F, N, X", unionStatusList.CodesAsString);
				AssertEquals("Description D", "Goods with transit document (TD)", unionStatusList[DEUnionStatusList.Codes.D].Description);
				AssertEquals("Description X", "Community goods to be exported", unionStatusList[DEUnionStatusList.Codes.X].Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CusTempStorageLineBaseForTest>();
		}
		CusTempStorageLineBaseForTest storageLine;
	}
}
