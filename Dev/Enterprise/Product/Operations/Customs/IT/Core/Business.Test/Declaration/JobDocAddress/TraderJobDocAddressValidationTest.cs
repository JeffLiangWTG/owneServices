using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class TraderJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestSelectedAddressIsValidatedRegardlessMainOne()
	{
		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		const string expectedWarningError = "Mock Postcode is longer than 9 characters, it will be truncated in the message.";
		traderDocAddress.OrganisationPK = genericOrganisation.PK;

		var organization = Factory.New<OrgHeader>();
		organization.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123");
		var addresses = organization.Addresses;

		organization.MainAddress.Postcode = "1234567890";

		var address2 = AddAddressToCollection(OrgConstants.AddressType.Office);
		address2.Postcode = "20148";

		traderDocAddress.E2_OA_Address = address2.PK;
		traderDocAddress.Validation.ValidateAll();
		AssertNoWarningContaining("Address 2 is taken for validation, because it is the selected one. No warning expected as it has postcode shorter than 9 characters", traderDocAddress.OrganisationPKInfo, expectedWarningError);

		traderDocAddress.E2_OA_Address = organization.MainAddress.PK;
		traderDocAddress.Validation.ValidateAll();
		AssertHasWarningContaining("Address 1 is taken for validation, because it is the selected one. Warning expected as it has postcode longer than 9 characters", traderDocAddress.OrganisationPKInfo, expectedWarningError);

		OrgAddress AddAddressToCollection(ZString addressType)
		{
			var address = addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType);
			return address;
		}
	}

	public void TestCheckOrganisationPK_MandatoryCodesValidation_NaturalPerson()
	{
		var euNaturalPersonSupplierWithoutAnyCustomsCode = Factory.NewWithValidTestData<OrgHeader>();
		euNaturalPersonSupplierWithoutAnyCustomsCode.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		euNaturalPersonSupplierWithoutAnyCustomsCode.MainAddress.OA_RN_NKCountryCode = "IT";

		var euNaturalPersonSupplierWithEori = Factory.NewWithValidTestData<OrgHeader>();
		euNaturalPersonSupplierWithEori.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		euNaturalPersonSupplierWithEori.CustomsCodes.Add(CreateOrgCusCode("EOR", "EOR CODE", "IT"));
		euNaturalPersonSupplierWithEori.MainAddress.OA_RN_NKCountryCode = "IT";

		var euNaturalPersonSupplierWithFiscalCode = Factory.NewWithValidTestData<OrgHeader>();
		euNaturalPersonSupplierWithFiscalCode.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		euNaturalPersonSupplierWithFiscalCode.CustomsCodes.Add(CreateOrgCusCode("COD", "FISCAL CODE", "IT"));
		euNaturalPersonSupplierWithFiscalCode.MainAddress.OA_RN_NKCountryCode = "IT";

		var extraEuSupplierWithoutAnyCustomsCode = Factory.NewWithValidTestData<OrgHeader>();
		extraEuSupplierWithoutAnyCustomsCode.OH_Category = OrgConstants.Category.Business;
		extraEuSupplierWithoutAnyCustomsCode.MainAddress.OA_RN_NKCountryCode = "ZA";

		var traderJobDocAddress = Factory.New<TraderJobDocAddress>();

		var expectedMessageError = ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrFiscalCodeIsRequired;
		traderJobDocAddress.OrganisationPK = ZGuid.Empty;
		AssertNoMessageErrorContaining(traderJobDocAddress.OrganisationPKInfo, expectedMessageError);

		traderJobDocAddress.OrganisationPK = euNaturalPersonSupplierWithoutAnyCustomsCode.PK;
		AssertHasMessageErrorContaining(traderJobDocAddress.OrganisationPKInfo, expectedMessageError);

		traderJobDocAddress.OrganisationPK = euNaturalPersonSupplierWithFiscalCode.PK;
		AssertNoMessageErrorContaining(traderJobDocAddress.OrganisationPKInfo, expectedMessageError);

		traderJobDocAddress.OrganisationPK = euNaturalPersonSupplierWithEori.PK;
		AssertNoMessageErrorContaining(traderJobDocAddress.OrganisationPKInfo, expectedMessageError);

		traderJobDocAddress.OrganisationPK = extraEuSupplierWithoutAnyCustomsCode.PK;
		AssertNoMessageErrorContaining(traderJobDocAddress.OrganisationPKInfo, expectedMessageError);
	}

	public void TestCheckOrganisationPK_MandatoryCodesValidation_Business()
	{
		var euBusinessSupplierWithoutAnyCustomsCode = Factory.NewWithValidTestData<OrgHeader>();
		euBusinessSupplierWithoutAnyCustomsCode.OH_Category = OrgConstants.Category.Business;
		euBusinessSupplierWithoutAnyCustomsCode.MainAddress.OA_RN_NKCountryCode = "IT";

		var euBusinessSupplierWithEori = Factory.NewWithValidTestData<OrgHeader>();
		euBusinessSupplierWithEori.OH_Category = OrgConstants.Category.Business;
		euBusinessSupplierWithEori.CustomsCodes.Add(CreateOrgCusCode("EOR", "EOR CODE", "IT"));
		euBusinessSupplierWithEori.MainAddress.OA_RN_NKCountryCode = "IT";

		var euBusinessSupplierWithVat = Factory.NewWithValidTestData<OrgHeader>();
		euBusinessSupplierWithVat.OH_Category = OrgConstants.Category.Business;
		euBusinessSupplierWithVat.CustomsCodes.Add(CreateOrgCusCode("IVA", "VAT (IVA) CODE", "IT"));
		euBusinessSupplierWithVat.MainAddress.OA_RN_NKCountryCode = "IT";

		var extraEuSupplierWithoutAnyCustomsCode = Factory.NewWithValidTestData<OrgHeader>();
		extraEuSupplierWithoutAnyCustomsCode.OH_Category = OrgConstants.Category.Business;
		extraEuSupplierWithoutAnyCustomsCode.MainAddress.OA_RN_NKCountryCode = "ZA";

		var expectedMessageError = ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrVatCodeIsRequired;
		var traderDocAddress = Factory.New<TraderJobDocAddress>();

		traderDocAddress.OrganisationPK = ZGuid.Empty;
		AssertNoMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);

		traderDocAddress.OrganisationPK = euBusinessSupplierWithoutAnyCustomsCode.PK;
		AssertHasMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);

		traderDocAddress.OrganisationPK = euBusinessSupplierWithEori.PK;
		AssertNoMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);

		traderDocAddress.OrganisationPK = euBusinessSupplierWithVat.PK;
		AssertNoMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);
	}

	public void TestCheckOrganisationPK_CompanyNameExceedsCustomsMaxLength()
	{
		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		const string expectedWarningError = "Mock Company Name is longer than 35 characters, it will be truncated in the message.";
		traderDocAddress.OrganisationPK = genericOrganisation.PK;

		genericOrganisation.OH_FullName = "";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		genericOrganisation.OH_FullName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		genericOrganisation.OH_FullName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);
	}

	public void TestCheckOrganisationPK_AddressExceedsCustomsMaxLength()
	{
		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		const string expectedWarningError = "Mock Address is longer than 35 characters, it will be truncated in the message.";
		traderDocAddress.OrganisationPK = genericOrganisation.PK;
		var address = genericOrganisation.MainAddress;

		address.Address1 = "";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Address1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Address1 = "ABCDE";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Address2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Address1 = "MAINADDRESSABCDEFGHI";
		address.Address2 = "JKLMNOPQRSTUVW";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);
	}

	public void TestCheckOrganisationPK_PostCodeExceedsCustomsMaxLength()
	{
		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		const string expectedWarningError = "Mock Postcode is longer than 9 characters, it will be truncated in the message.";
		traderDocAddress.OrganisationPK = genericOrganisation.PK;
		var address = genericOrganisation.MainAddress;

		address.Postcode = "";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Postcode = "0123456789";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.Postcode = "012345";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);
	}

	public void TestCheckOrganisationPK_CityExceedsCustomsMaxLength()
	{
		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		var expectedWarningError = "Mock City is longer than 35 characters, it will be truncated in the message.";
		traderDocAddress.OrganisationPK = genericOrganisation.PK;
		var address = genericOrganisation.MainAddress;

		address.City = "";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.City = "ABCDE";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);

		address.City = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarningContaining(traderDocAddress.OrganisationPKInfo, expectedWarningError);
	}

	public void TestCheckOrganisationPK_MandatoryAddressValidation()
	{
		var expectedMessageError = "You have not entered a Mock";

		var traderDocAddress = Factory.New<TraderJobDocAddress>();
		traderDocAddress.OrganisationPK = ZGuid.Empty;
		traderDocAddress.Validation.ValidateOrganisationPK();
		AssertHasMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);

		traderDocAddress.OrganisationPK = genericOrganisation.PK;
		AssertNoMessageErrorContaining(traderDocAddress.OrganisationPKInfo, expectedMessageError);
	}

	public void TestCheckOrganizationPK_WhenAddressNull()
	{
		var organization = Factory.New<OrgHeader>();
		var traderWithEmptyAddress = Factory.New<TraderJobDocAddress>();
		traderWithEmptyAddress.OrganisationPK = organization.PK;
		traderWithEmptyAddress.E2_OA_Address = ZGuid.Empty;
		traderWithEmptyAddress.E2_RN_NKCountryCode = "IT";

		const string expectedMessageError = "EORI Code or VAT (IVA) Code is required for the EU Organization";

		traderWithEmptyAddress.Validation.ValidateOrganisationPK();
		AssertHasMessageErrorContaining(traderWithEmptyAddress.OrganisationPKInfo, expectedMessageError);

		organization.CustomsCodes.AddNew("EOR", "123");
		traderWithEmptyAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining(traderWithEmptyAddress.OrganisationPKInfo, expectedMessageError);
	}

	protected override void SetUp()
	{
		base.SetUp();
		genericOrganisation = Factory.NewWithValidTestData<OrgHeader>();
	}

	OrgCusCode CreateOrgCusCode(string codeType, string regNo, string countryCode)
	{
		var cusCode = Factory.New<OrgCusCode>();
		cusCode.OK_CodeType = codeType;
		cusCode.OK_CustomsRegNo = regNo;
		cusCode.OK_RN_NKCodeCountry = countryCode;
		return cusCode;
	}

	OrgHeader genericOrganisation;

	class TraderJobDocAddress : JobDocAddress
	{
		public TraderJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new TraderJobDocAddressValidation(this, "Mock", new MockDeclaration(isUCC6AndIsExport: false, isTransitionPeriodAES30: false, isImport: false));
		}
	}

	class MockDeclaration : IUCC6AndTransitionPeriodProvider
	{
		public MockDeclaration(bool isUCC6AndIsExport, bool isTransitionPeriodAES30, bool isImport)
		{
			this.isImport = isImport;
			this.isUCC6AndIsExport = isUCC6AndIsExport;
			this.isTransitionPeriodAES30 = isTransitionPeriodAES30;
		}

		readonly bool isUCC6AndIsExport;
		readonly bool isTransitionPeriodAES30;
		readonly bool isImport;

		bool IUCC6AndTransitionPeriodProvider.IsUCC6AndIsExport => isUCC6AndIsExport;

		bool IUCC6AndTransitionPeriodProvider.IsTransitionPeriodAES30 => isTransitionPeriodAES30;

		bool IUCC6AndTransitionPeriodProvider.IsImport => isImport;
	}
}
