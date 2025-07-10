using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CHGTSTCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocationOfGoodsList()
		{
			CombineAssertions(() =>
			{
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "01", "01 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "02", "02 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				var companyOrgProxy = Factory.New<OrgHeader>();
				companyOrgProxy.OH_Code = "COMPORGPROX";
				var authorisation = companyOrgProxy.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER1");
				var authorisationRule = authorisation.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "01");
				authorisationRule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

				var branchOrgProxy = Factory.New<OrgHeader>();
				branchOrgProxy.OH_Code = "BRANORGPROX";
				var authorisation2 = branchOrgProxy.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER2");
				var authorisationRule2 = authorisation2.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "02");
				authorisationRule2.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000002");
				Factory.Save();

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
				storageHeader.SJH_CustomsOffice = "DE000002";
				AssertEquals("LOC-codes of Branch", "02", storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
				storageHeader.SJH_CustomsOffice = "DE000001";
				AssertEquals("LOC-codes of Company due to EORI-fallback", "01", storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				storageHeader.SJH_CustomsOffice = "NOMATCH";
				AssertEquals("No LOC-codes due to CustomsOffice mismatch", ZString.Empty, storageLine.Lookups.LocationOfGoodsList.CodesAsString);

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				AssertEquals("No Organisation", ZString.Empty, storageLine.Lookups.LocationOfGoodsList.CodesAsString);
			});
		}

		public void TestOwnerReferenceTypeList()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertEquals("AWB, ULD", storageLine.Lookups.OwnerReferenceTypeList.CodesAsString);
			Assert(ReferenceEquals(Factory.GetCachedValue("DE|CHGTSTCusTempStorageLineLookups|OwnerReferenceTypeList|AWB", () => new CodeDescriptionPairList()), storageLine.Lookups.OwnerReferenceTypeList));
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertEquals(0, storageLine.Lookups.OwnerReferenceTypeList.Count);
			storageDec.STH_IdentificationIndicator = ZString.Empty;
			AssertEquals(0, storageLine.Lookups.OwnerReferenceTypeList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTOMER";

			storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = customer.PK;
			storageDec = storageHeader.CHGTSTCusTempStorageDecs.AddNew();
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CusTempStorageJobHeader storageHeader;
		CHGTSTCusTempStorageDec storageDec;
		CHGTSTCusTempStorageLine storageLine;
	}
}
