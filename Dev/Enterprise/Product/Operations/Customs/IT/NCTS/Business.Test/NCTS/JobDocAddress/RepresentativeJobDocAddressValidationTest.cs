using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class RepresentativeJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When the JobDocAddress is null", () => new RepresentativeJobDocAddressValidation(parent: null, departureMovement));
		AssertExceptionThrown<ArgumentNullException>("When the Parent DepartureMovement is null", () => new RepresentativeJobDocAddressValidation(representativeDocAddress, departureMovement: null));
	}

	public void TestCheckRepresentative_MandatoryValidation()
	{
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		representativeDocAddress.OrganisationPK = ZGuid.Empty;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertHasMessageErrorContaining("When is not TIR and Representative is empty", representativeDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		representativeDocAddress.OrganisationPK = orgHeader1.PK;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertNoMessageErrorContaining("When is not TIR and Representative is not empty", representativeDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		representativeDocAddress.OrganisationPK = ZGuid.Empty;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertNoMessageErrorContaining("When is TIR and Representative is empty", representativeDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckRepresentative_RequiredCustomsCodeValidation()
	{
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		representativeDocAddress.OrganisationPK = ZGuid.Empty;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertNoMessageErrorContaining("When not TIR and Representative is empty", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);

		var representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		representativeOrgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

		CombineAssertions("When Representative is a business", () =>
		{
			representativeOrgHeader.OH_Category = OrgConstants.Category.Business;

			representativeDocAddress.OrganisationPK = representativeOrgHeader.PK;
			representativeDocAddress.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining("When Representative does not have a VAT CustomsCode", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);

			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "123");
			representativeDocAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining("When Representative has a VAT CustomsCode", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);
		});

		CombineAssertions("When Representative is a natural person", () =>
		{
			representativeOrgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			representativeDocAddress.OrganisationPK = representativeOrgHeader.PK;
			representativeDocAddress.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining("When Representative does not have a COD CustomsCode", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);

			representativeOrgHeader.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, "456");
			representativeDocAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining("When Representative has a COD CustomsCode", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);
		});

		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		representativeOrgHeader.CustomsCodes.RemoveAndDeleteAll();
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertNoMessageErrorContaining("When is TIR and Representative has no customs codes", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);
	}

	public void TestCheckRepresentative_RequiredCustomsCodeValidation_NCTS5Departure()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		representativeDocAddress.OrganisationPK = ZGuid.Empty;

		var representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		representativeOrgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

		representativeOrgHeader.OH_Category = OrgConstants.Category.Business;

		representativeDocAddress.OrganisationPK = representativeOrgHeader.PK;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertNoNotifications(representativeDocAddress.OrganisationPKInfo);

		nctsHeader.BH_ApplicationCode = "NCT";
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		representativeDocAddress.OrganisationPK = ZGuid.Empty;

		representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		representativeOrgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

		representativeOrgHeader.OH_Category = OrgConstants.Category.Business;

		representativeDocAddress.OrganisationPK = representativeOrgHeader.PK;
		representativeDocAddress.AdditionalValidation.ValidateAll();
		AssertHasMessageErrorContaining("When Representative does not have a VAT CustomsCode", representativeDocAddress.OrganisationPKInfo, VatCodeOrFiscalCodeIsRequired);
	}

	public void TestCheckRepresentative_CustomsLengthValidation()
	{
		const string messageWarning = "Representative Company Name is longer than 35 characters, it will be truncated in the message.";

		var organisation = Factory.New<OrgHeader>();
		organisation.OH_FullName = "".PadRight(40, 'A');
		representativeDocAddress.OrganisationPK = organisation.PK;
		AssertHasWarningContaining("When is not TIR and Representative Company Name is > than 35 is empty", representativeDocAddress.OrganisationPKInfo, messageWarning);

		organisation.OH_FullName = "".PadRight(20, 'A');
		representativeDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarningContaining("When is not TIR and Representative Company Name is < than 35 is empty", representativeDocAddress.OrganisationPKInfo, messageWarning);
	}

	public void TestCheckRepresentative_CustomsLengthValidation_NCTS5Departure()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

		var organisation = Factory.New<OrgHeader>();

		organisation.OH_FullName = "".PadRight(40, 'A');
		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234");
		representativeDocAddress.OrganisationPK = organisation.PK;
		AssertNoNotifications(representativeDocAddress.OrganisationPKInfo);

		organisation.OH_FullName = "".PadRight(20, 'A');
		representativeDocAddress.Validation.ValidateOrganisationPK();
		AssertNoNotifications(representativeDocAddress.OrganisationPKInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		departureMovement = nctsHeader.MovementHeader;
		representativeDocAddress = departureMovement.Representative;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;
	JobDocAddress representativeDocAddress;

	const string VatCodeOrFiscalCodeIsRequired = "VAT (IVA) Code or Fiscal Code (Italian Registration Number) is required for the EU Organization. Please press F3, go to Detail -> Config -> Registration Numbers/Codes and enter a valid one.";
}
