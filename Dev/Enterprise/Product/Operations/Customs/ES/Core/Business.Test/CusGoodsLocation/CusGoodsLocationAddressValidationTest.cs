using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.Business.Testing;

sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestRule065() => CombineAssertions(() =>
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		var goodslocation = temporaryStorageHeader.GoodsLocation;
		var propertyInfoAuthorisation = goodslocation.Address.E2_GovRegNumInfo;
		var propertyInfoOrganisation = goodslocation.Address.E2_AdditionalAddressInformationInfo;
		var messageAuthorisation = "[C0065] Location: Authorization No. required when qualifier is 'Y'.";
		var messageOrganisation = "[C0065] Location: Organization required when qualifier is 'Y'.";

		goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertNoNotifications("Neither authorisation number nor organisation: " + messageAuthorisation, propertyInfoAuthorisation);
		AssertNoNotifications("Neither EORI nor organisation, organisation: " + messageOrganisation, propertyInfoOrganisation);

		goodslocation.Address.AuthorisationNumber = "auth";
		AssertNoNotifications("There is only an authorisation number: " + messageAuthorisation, propertyInfoAuthorisation);
		AssertNoNotifications("There is only an EORI, organisation: " + messageOrganisation, propertyInfoOrganisation);

		goodslocation.Address.E2_AdditionalAddressInformation = ZGuid.BrettsGuid.ToString();
		goodslocation.Address.AuthorisationNumber = "auth";
		AssertNoNotifications("There is authorisation number and organization: " + messageAuthorisation, propertyInfoAuthorisation);
		AssertNoNotifications("There is EORI and organisation, organisation: " + messageOrganisation, propertyInfoOrganisation);

		goodslocation.Address.AuthorisationNumber = ZString.Empty;
		AssertNoNotifications("There is only an organization: " + messageAuthorisation, propertyInfoAuthorisation);
		AssertNoNotifications("There is only an organization, organisation: " + messageOrganisation, propertyInfoOrganisation);
	});
}
