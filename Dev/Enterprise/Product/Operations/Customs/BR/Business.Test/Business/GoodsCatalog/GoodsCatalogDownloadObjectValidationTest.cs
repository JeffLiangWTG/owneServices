using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class GoodsCatalogDownloadObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOwnerCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";

			var goodsCatalog = new GoodsCatalogDownloadObject(Factory);

			ValidationTestHelper.AssertErrorIfNotEntered(goodsCatalog.OwnerCodeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(goodsCatalog.OwnerCodeInfo, "XXX", orgHeader.OH_Code, "Owner");

			AssertHasError(goodsCatalog.OwnerCodeInfo, "Organization is not set as Consignee or Consignor");

			orgHeader.OH_IsConsignor = true;
			goodsCatalog.Validation.ValidateOwnerCode();
			AssertNoError(goodsCatalog.OwnerCodeInfo, "Organization is not set as Consignee or Consignor");

			orgHeader.OH_IsConsignor = false;
			orgHeader.OH_IsConsignee = true;
			goodsCatalog.Validation.ValidateOwnerCode();
			AssertNoError(goodsCatalog.OwnerCodeInfo, "Organization is not set as Consignee or Consignor");

			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "27.094.734/0001-33", Core.Constants.CountryCodes.Brazil);
			AssertHasError(goodsCatalog.OwnerCodeInfo, "Organization is not set as Root CNPJ");

			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "27094734", Core.Constants.CountryCodes.Brazil);
			goodsCatalog.Validation.ValidateOwnerCode();
			AssertNoError(goodsCatalog.OwnerCodeInfo, "Organization is not set as Root CNPJ");
		}

		public void TestCheckBrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DOW";

			var goodsCatalog = new GoodsCatalogDownloadObject(Factory);

			ValidationTestHelper.AssertErrorIfNotEntered(goodsCatalog.BrokerCodeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(goodsCatalog.BrokerCodeInfo, "XXX", staff.GS_Code);

			goodsCatalog.BrokerCode = staff.GS_Code;
			goodsCatalog.Validation.ValidateBrokerCode();
			AssertHasMessageError(goodsCatalog.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(-10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			goodsCatalog.Validation.ValidateBrokerCode();
			AssertHasMessageError(goodsCatalog.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
			goodsCatalog.Validation.ValidateBrokerCode();
			AssertHasMessageError(goodsCatalog.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			goodsCatalog.Validation.ValidateBrokerCode();
			AssertNoMessageError(goodsCatalog.BrokerCodeInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");
		}

		public void TestCheckDownloadCatalog()
		{
			var goodsCatalog = new GoodsCatalogDownloadObject(Factory);
			goodsCatalog.DownloadForeignOperator = false;
			goodsCatalog.DownloadCatalog = false;
			AssertHasError(goodsCatalog.DownloadCatalogInfo, "Please choose one of the options to Download Catalog");

			goodsCatalog.DownloadCatalog = true;
			AssertNoError(goodsCatalog.DownloadCatalogInfo, "Please choose one of the options to Download Catalog");

			goodsCatalog.DownloadForeignOperator = true;
			goodsCatalog.DownloadCatalog = false;
			AssertNoError(goodsCatalog.DownloadCatalogInfo, "Please choose one of the options to Download Catalog");
		}

		public void TestCheckDownloadForeignOperator()
		{
			var goodsCatalog = new GoodsCatalogDownloadObject(Factory);
			goodsCatalog.DownloadCatalog = false;
			goodsCatalog.DownloadForeignOperator = false;
			AssertHasError(goodsCatalog.DownloadForeignOperatorInfo, "Please choose one of the options to Download Catalog");

			goodsCatalog.DownloadForeignOperator = true;
			AssertNoError(goodsCatalog.DownloadForeignOperatorInfo, "Please choose one of the options to Download Catalog");

			goodsCatalog.DownloadCatalog = true;
			goodsCatalog.DownloadForeignOperator = false;
			AssertNoError(goodsCatalog.DownloadForeignOperatorInfo, "Please choose one of the options to Download Catalog");
		}
	}
}
