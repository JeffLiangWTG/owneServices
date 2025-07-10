using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusGoodsCatalogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGC_AuthorityIdentifier()
		{
			GoodsCatalog.CGC_AuthorityIdentifier = "1";
			AssertNoErrorContaining(GoodsCatalog.CGC_AuthorityIdentifierInfo, "Authority Identifier should be only numbers.");

			GoodsCatalog.CGC_AuthorityIdentifier = "XXX";
			AssertHasErrorContaining(GoodsCatalog.CGC_AuthorityIdentifierInfo, "Authority Identifier should be only numbers.");
		}

		public void TestCheckCGC_AuthorityVersion()
		{
			GoodsCatalog.CGC_AuthorityVersion = "1";
			AssertNoError(GoodsCatalog.CGC_AuthorityVersionInfo, "Authority Version should be only numbers.");

			GoodsCatalog.CGC_AuthorityVersion = "-1";
			AssertHasError(GoodsCatalog.CGC_AuthorityVersionInfo, "Authority Version should be only numbers.");

			GoodsCatalog.CGC_AuthorityVersion = "B";
			AssertHasError(GoodsCatalog.CGC_AuthorityVersionInfo, "Authority Version should be only numbers.");

			GoodsCatalog.CGC_AuthorityVersion = string.Empty;
			AssertNoError(GoodsCatalog.CGC_AuthorityVersionInfo, "Authority Version should be only numbers.");
		}

		public void TestCheckCGC_AuthorityStatus()
		{
			GoodsCatalog.CGC_AuthorityStatus = "X";
			AssertHasErrorContaining(GoodsCatalog.CGC_AuthorityStatusInfo, ListValidation.InvalidCodeError);

			GoodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Draft;
			AssertNoErrorContaining(GoodsCatalog.CGC_AuthorityStatusInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCGC_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			ValidationTestHelper.AssertErrorIfNotEntered(GoodsCatalog.CGC_TariffInfo);

			GoodsCatalog.CGC_Tariff = "56049001";
			AssertHasMessageError(GoodsCatalog.CGC_TariffInfo, "The Tariff Code entered is not valid for the current context.");

			GoodsCatalog.CGC_Tariff = "56049000";
			AssertNoMessageError(GoodsCatalog.CGC_TariffInfo, "The Tariff Code entered is not valid for the current context.");
		}

		public void TestCheckCGC_OH_Owner()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_Code = "TST";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN";
			owner.OH_FullName = "TEST COMPANY1";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);
			GoodsCatalog.CGC_OH_Owner = owner.PK;
			AssertHasMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "Please enter a Root CNPJ in this organization to continue.");

			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);
			GoodsCatalog.CGC_OH_Owner = owner.PK;
			AssertNoMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "Please enter a Root CNPJ in this organization to continue.");
			AssertHasMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "No Catalog Manager has been assigned for this Owner. Please verify it on Details > Brazil > Catalog Manager.");

			var orgImpAddInfo = BROrgImpAddInfo.Get(owner);
			orgImpAddInfo.ZO_BrokerCode = "TST";
			GoodsCatalog.CGC_OH_Owner = owner.PK;
			AssertNoMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "No Catalog Manager has been assigned for this Owner. Please verify it on Details > Brazil > Catalog Manager.");
			AssertHasMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "The digital certificate for the Catalog Manager is missing, expired, or invalid. Please verify it on Details > Brazil > Catalog Manager.");

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			GoodsCatalog.CGC_OH_Owner = owner.PK;
			AssertNoMessageErrorContaining(GoodsCatalog.CGC_OH_OwnerInfo, "The digital certificate for the Catalog Manager is missing, expired, or invalid. Please verify it on Details > Brazil > Catalog Manager.");
		}

		BaseCusGoodsCatalog GoodsCatalog => goodsCatalog ??= Factory.New<BaseCusGoodsCatalog>();
		BaseCusGoodsCatalog goodsCatalog;
	}
}
