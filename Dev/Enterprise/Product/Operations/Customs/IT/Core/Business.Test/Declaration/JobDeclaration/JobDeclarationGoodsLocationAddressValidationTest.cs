using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationGoodsLocationAddressValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationGoodsLocationAddressValidation(jobDeclaration: null));
		AssertNoExceptionThrown("When declaration is not null", () => new JobDeclarationGoodsLocationAddressValidation(declaration));
	}

	public void TestCheckGoodsLocationAddress_WithAuthorisationNumber_LocationQualifier()
	{
		CombineAssertions("When JE_LocationQualifier is F and ZG_AuthorisationNumber is empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;

			declaration.GoodsLocationAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(declaration.GoodsLocationAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrorContaining(declaration.GoodsLocationAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When JE_LocationQualifier is F and ZG_AuthorisationNumber is not empty", () =>
		{
			declaration.ZG_AuthorisationNumber = "1234";

			declaration.GoodsLocationAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.GoodsLocationAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckGoodsLocationAddress_Address1And2Length()
	{
		var expectedMessage = "Goods Location Address is longer than 70 characters, it will be truncated in the message";
		CombineAssertions("When LocationQualifier = F", () =>
		{
			address.OA_Address1 = new ZString('1', 50);
			address.OA_Address2 = new ZString('2', 50);
			declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining("When Address1 and Address 2 length more than 70", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);

			address.OA_Address2 = new ZString('2', 10);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When Address1 and Address 2 length less than 70", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});

		CombineAssertions("When LocationQualifier = FC", () =>
		{
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			address.OA_Address2 = new ZString('2', 50);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When Address1 and Address 2 length more than 70", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});
	}

	public void TestCheckGoodsLocationAddress_PostcodeLength()
	{
		var expectedMessage = "Goods Location Address Postcode is longer than 9 characters, it will be truncated in the message";
		CombineAssertions("When LocationQualifier = F", () =>
		{
			address.OA_PostCode = new ZString('P', 10);
			declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining("When Postcode length more than 9", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);

			address.OA_PostCode = new ZString('P', 7);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When Postcode length less than 9", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});

		CombineAssertions("When LocationQualifier = FC", () =>
		{
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			address.OA_PostCode = new ZString('P', 10);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When Postcode length more than 9", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});
	}

	public void TestCheckGoodsLocationAddress_CityLength()
	{
		var expectedMessage = "Goods Location Address City is longer than 35 characters, it will be truncated in the message";
		CombineAssertions("When LocationQualifier = F", () =>
		{
			address.OA_City = new ZString('C', 50);
			declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining("When City length more than 35", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);

			address.OA_City = new ZString('C', 20);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When City length less than 35", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});

		CombineAssertions("When LocationQualifier = FC", () =>
		{
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			address.OA_City = new ZString('C', 50);
			declaration.GoodsLocationAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarningContaining("When City length more than 35", declaration.GoodsLocationAddress.E2_OA_AddressInfo, expectedMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
		orgHeader = Factory.New<OrgHeader>();
		address = orgHeader.MainAddress;
	}

	JobDeclaration declaration;
	OrgHeader orgHeader;
	OrgAddress address;
}
