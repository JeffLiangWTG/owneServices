using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRHeaderCarrierValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCarrierCodeInbDocAddress()
		{
			var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			testOrgAddress.OA_OH = testOrganization.PK;
			var testCarrier = Factory.New<JobDocAddress>();
			testCarrier.AdditionalValidation = new JPAFRHeaderCarrierValidation(testCarrier, null);

			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQBA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testCarrier.DocAddressType = DocAddressType.Carrier;

			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQBA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testCarrier.E2_OA_Address = testOrgAddress.PK;

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, string.Empty, Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertHasWarning(testCarrier.OrganisationPKInfo, ValidationConstants.Header.CarrierOrgHasNoJPCarrierCode);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQBA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertHasWarningContaining(testCarrier.OrganisationPKInfo, ValidationConstants.Header.CarrierOrgHaveJPCarrierCodeReachedMaxAllowed);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			testCarrier.AdditionalValidation.ValidateAll();
			AssertNoNotifications(testCarrier.OrganisationPKInfo);
		}

		public void TestCarrierCodeInTransports()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "McLaren";
			carrier1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_FullName = "Button";
			var carrier2Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier2Address2.OA_OH = carrier2.PK;
			carrier2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingLine = true;
			carrier3.OH_FullName = "Perez";
			var carrier3Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier3Address2.OA_OH = carrier3.PK;
			carrier3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var invalidAddress = Factory.New<OrgAddress>();
			invalidAddress.OA_OH = ZGuid.Empty;

			var consol = Factory.New<CommonConsol>();
			var leg1 = consol.Transports.AddNew();
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;
			var leg2 = consol.Transports.AddNew();
			leg2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;

			Header.JPH_ParentId = consol.PK;
			Header.JPH_ParentTableCode = consol.TablePrefix;
			Header.Carrier.E2_OA_Address = carrier3.MainAddress.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertHasMessageError("Expected a message error as carrier3 is unrelated to the carriers on both transport legs", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);

			Header.Carrier.E2_OA_Address = carrier2.MainAddress.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertNoMessageError("Expected no message error as the carrier matches a carrier from a rounting leg", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);
		}

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;
	}
}
