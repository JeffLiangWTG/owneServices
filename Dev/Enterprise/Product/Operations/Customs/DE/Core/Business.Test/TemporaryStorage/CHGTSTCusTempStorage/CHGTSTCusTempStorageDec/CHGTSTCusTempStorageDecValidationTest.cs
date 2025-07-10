using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageDec))]
	class CHGTSTCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<CHGTSTCusTempStorageDec>
	{
		public void TestCheckSTH_OwnerReferenceNumber()
		{
			CombineAssertions(() =>
			{
				var storageDec = GetCusTempStorageDecToTest();
				storageDec.Validation.ValidateSTH_OwnerReferenceNumber();
				AssertHasMessageErrorContaining("Message error for Mandatory", storageDec.STH_OwnerReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
				storageDec.STH_OwnerReferenceNumber = "AT/B/15/000001/03/2000/3001";
				AssertNoMessageErrorContaining("No Message Error for Mandatory", storageDec.STH_OwnerReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckNewCustodianBranch()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "CUSBRNTST";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;

			var customCode = orgHeader.CustomsCodes.AddNew();
			customCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			customCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			customCode.OK_CustomsRegNo = "0001";
			Factory.Save();

			var header = Factory.New<CusTempStorageJobHeader>();
			var storageDec = header.CHGTSTCusTempStorageDecs.AddNew();

			storageDec.Validation.ValidateNewCustodianBranch();
			AssertHasMessageErrorContaining(storageDec.NewCustodianBranchInfo, MandatoryValidation.YouHaveNotEntered);

			storageDec.NewCustodianBranch = "0004";
			AssertHasMessageError(storageDec.NewCustodianBranchInfo, ListValidation.InvalidCodeMessageError);

			storageDec.NewCustodianBranch = "0001";
			AssertNoMessageErrors(storageDec.NewCustodianBranchInfo);
		}

		protected override CHGTSTCusTempStorageDec GetCusTempStorageDecToTest() => Factory.New<CHGTSTCusTempStorageDec>();
	}
}
