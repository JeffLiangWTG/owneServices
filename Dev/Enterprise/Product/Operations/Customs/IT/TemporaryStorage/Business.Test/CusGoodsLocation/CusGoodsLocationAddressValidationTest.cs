using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestRuleC0065()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();

		var goodslocation = temporaryStorageHeader.GoodsLocation;
		var propertyInfoAuthorisation = goodslocation.Address.E2_GovRegNumInfo;
		var propertyInfoOrganisation = goodslocation.Address.E2_AdditionalAddressInformationInfo;

		goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertNoNotifications("Neither authorisation number nor organisation", propertyInfoAuthorisation);
		AssertNoNotifications("Neither EORI nor organisation, organisation", propertyInfoOrganisation);

		goodslocation.Address.AuthorisationNumber = "auth";
		AssertNoNotifications("There is only an authorisation number", propertyInfoAuthorisation);
		AssertNoNotifications("There is only an EORI, organisation", propertyInfoOrganisation);

		goodslocation.Address.IdentificationHolderPK = orgHeader.PK;
		goodslocation.Address.AuthorisationNumber = "auth";
		AssertNoNotifications("There is authorisation number and organization", propertyInfoAuthorisation);
		AssertNoNotifications("There is EORI and organisation, organisation", propertyInfoOrganisation);

		goodslocation.Address.AuthorisationNumber = ZString.Empty;
		AssertNoNotifications("There is only an organization", propertyInfoAuthorisation);
		AssertNoNotifications("There is only an organization, organisation", propertyInfoOrganisation);
	}

	public void Test_CheckE2_AdditionalAddressInformation()
	{
		var orgHeader = Factory.New<OrgHeader>();
		const string expectedErrorMessage = "Enter a valid";
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();

		var goodslocation = temporaryStorageHeader.GoodsLocation;
		var address = goodslocation.Address;
		var propertyInfoIdentificationHolder = address.IdentificationHolderPKInfo;

		CombineAssertions(() =>
		{
			address.IdentificationHolderPK = orgHeader.PK;
			AssertNoError("Valid PK: no error expected", propertyInfoIdentificationHolder, expectedErrorMessage);

			address.IdentificationHolderPK = ZGuid.Empty;
			AssertNoError("Empty PK: no error expected", propertyInfoIdentificationHolder, expectedErrorMessage);

			address.IdentificationHolderPK = ZGuid.BrettsGuid;
			AssertHasErrorContaining("Invalid PK: error expected", propertyInfoIdentificationHolder, expectedErrorMessage);
		});
	}
}
