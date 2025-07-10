using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CHGTSTCusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIdentificationIndicatorList()
		{
			var storageDec = Factory.New<CHGTSTCusTempStorageDec>();
			var indicatorList = storageDec.Lookups.IdentificationIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", storageDec.GetIdentificationIndicatorListExcludingSIN(), indicatorList);
				AssertEquals("Codes", "AWB, REG", indicatorList.CodesAsString);
			});
		}

		public void TestNewCustodianBranchList()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "CUSBRNTST";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;

			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.OA_Code = "TEST1";
			orgAddress1.OA_Address1 = "ADDRESS1";

			var orgAddress2 = orgHeader.Addresses.AddNew();
			orgAddress2.OA_Code = "TEST2";
			orgAddress2.OA_Address1 = "ADDRESS2";

			var orgAddress3 = orgHeader.Addresses.AddNew();
			orgAddress3.OA_Code = "TEST3";
			orgAddress3.OA_Address1 = "ADDRESS3";

			var customCode1 = orgHeader.CustomsCodes.AddNew();
			customCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			customCode1.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			customCode1.OK_CustomsRegNo = "00012";
			customCode1.OK_OA_PremisesAddress = orgAddress1.PK;

			var customCode2 = orgHeader.CustomsCodes.AddNew();
			customCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			customCode2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			customCode2.OK_CustomsRegNo = "00013";
			customCode2.OK_OA_PremisesAddress = orgAddress2.PK;

			var customCode3 = orgHeader.CustomsCodes.AddNew();
			customCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			customCode3.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			customCode3.OK_CustomsRegNo = "0004";
			customCode3.OK_OA_PremisesAddress = orgAddress3.PK;

			var customCode4 = orgHeader.CustomsCodes.AddNew();
			customCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			customCode4.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.AccreditedExporter;
			customCode4.OK_CustomsRegNo = "789";

			var customCode5 = orgHeader.CustomsCodes.AddNew();
			customCode5.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			customCode5.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			customCode5.OK_CustomsRegNo = "ABCUK";
			Factory.Save();

			var storageDec = Factory.New<CusTempStorageJobHeader>().CHGTSTCusTempStorageDecs.AddNew();
			var list = storageDec.Lookups.NewCustodianBranchList;
			AssertEquals("0001, 0001, 0004", list.CodesAsString);
			AssertEquals("ADDRESS3", list.GetDescriptionFromCode("0004"));
		}
	}
}
