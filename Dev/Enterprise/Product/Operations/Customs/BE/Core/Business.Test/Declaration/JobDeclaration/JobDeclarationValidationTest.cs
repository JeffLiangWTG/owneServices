using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
{
	public void TestCheckJE_OA_Representative_NotEmpty() => CombineAssertions(() =>
	{
		var message = "A Declarant or Representative must be specified.";

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertHasMessageError("both empty", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = ZGuid.BrettsGuid;
		AssertNoMessageError("representative filled", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_DeclarantAddress = ZGuid.BrettsGuid;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertNoMessageError("declarant filled", declaration.JE_OA_RepresentativeInfo, message);
	});

	public void TestCheckJE_OA_Representative_EORI() => CombineAssertions(() =>
	{
		var message = "Representative must have an EORI number.";

		var representative = Factory.NewWithValidTestData<OrgAddress>();
		declaration.JE_OA_Representative = representative.PK;
		AssertHasMessageError("has no eori", declaration.JE_OA_RepresentativeInfo, message);

		var eori = declaration.Representative.Header.CustomsCodes.AddNew();
		eori.OK_CodeType = "EOR";
		eori.OK_RN_NKCodeCountry = "DE";
		eori.OK_CustomsRegNo = "regno";
		declaration.Validation.ValidateJE_OA_Representative();
		AssertNoMessageError("has eori", declaration.JE_OA_RepresentativeInfo, message);
	});

	public void TestTestCheckJE_OA_Representative_LogonSameValidation() => CombineAssertions(() =>
	{
		var message = "Representative EORI number must be same as the logon company.";

		var logonOrgHeader = GlbCompany.CurrentCompany.OrgProxy;
		var customCode = logonOrgHeader.CustomsCodes.AddNew();
		customCode.OK_CodeType = "EOR";
		customCode.OK_RN_NKCodeCountry = "DE";
		customCode.OK_CustomsRegNo = "same";

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertNoMessageError("both empty", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("different").PK;
		AssertHasMessageError("representative filled, different eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		AssertNoMessageError("representative filled, same eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("different").PK;
		AssertHasMessageError("both filled, representative different eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		AssertNoMessageError("representative filled, same eori", declaration.JE_OA_RepresentativeInfo, message);
	});

	public void TestCheckJE_OA_Representative_ImporterDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Importer, Declarant and Representative cannot all have the same EORI number.";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("representative has different eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("representative has no eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = ZGuid.Missing;
		AssertNoMessageError("representative is not filled", declaration.JE_OA_RepresentativeInfo, message);
	});

	public void TestCheckJE_OA_Representative_SupplierDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Supplier, Declarant and Representative cannot all have the same EORI number.";

		declaration.SupplierDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("representative has different eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("representative has no eori", declaration.JE_OA_RepresentativeInfo, message);

		declaration.JE_OA_Representative = ZGuid.Missing;
		AssertNoMessageError("representative is not filled", declaration.JE_OA_RepresentativeInfo, message);
	});

	public void TestCheckJE_OA_DeclarantAddress_NotEmpty() => CombineAssertions(() =>
	{
		var message = "A Declarant or Representative must be specified.";

		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertHasMessageError("both empty", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = ZGuid.BrettsGuid;
		AssertNoMessageError("declarant filled", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_Representative = ZGuid.BrettsGuid;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageError("representative filled", declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestCheckJE_OA_DeclarantAddress_EORI() => CombineAssertions(() =>
	{
		var message = "Declarant must have an EORI number.";

		declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertHasMessageError("has no eori", declaration.JE_OA_DeclarantAddressInfo, message);

		var eori = declaration.DeclarantAddress.Header.CustomsCodes.AddNew();
		eori.OK_CodeType = "EOR";
		eori.OK_RN_NKCodeCountry = "DE";
		eori.OK_CustomsRegNo = "regno";
		declaration.Validation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageError("has eori", declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestTestCheckJE_OA_DeclarantAddress_LogonSameValidation() => CombineAssertions(() =>
	{
		var message = "Declarant EORI number must be same as the logon company.";

		var logonOrgHeader = GlbCompany.CurrentCompany.OrgProxy;
		var customCode = logonOrgHeader.CustomsCodes.AddNew();
		customCode.OK_CodeType = "EOR";
		customCode.OK_RN_NKCodeCountry = "DE";
		customCode.OK_CustomsRegNo = "same";

		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageError("both empty", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		AssertHasMessageError("declarant filled, different eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		AssertNoMessageError("declarant filled, same eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		AssertNoMessageError("both filled, representative different eori", declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestCheckJE_OA_DeclarantAddress_ImporterDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Importer, Declarant and Representative cannot all have the same EORI number.";

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("declarant has different eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("declarant has no eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = ZGuid.Missing;
		AssertNoMessageError("declarant is not filled", declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestCheckJE_OA_DeclarantAddress_SupplierDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Supplier, Declarant and Representative cannot all have the same EORI number.";

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.SupplierDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("declarant has different eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("declarant has no eori", declaration.JE_OA_DeclarantAddressInfo, message);

		declaration.JE_OA_DeclarantAddress = ZGuid.Missing;
		AssertNoMessageError("declarant is not filled", declaration.JE_OA_DeclarantAddressInfo, message);
	});

	public void TestValidateImporterDocumentaryAddress_ImporterDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Importer, Declarant and Representative cannot all have the same EORI number.";

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.ImporterDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("representative has different eori", declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.ImporterDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("representative has no eori", declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Missing;
		AssertNoMessageError("representative is not filled", declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, message);
	});

	public void TestValidateSupplierDocumentaryAddress_SupplierDeclarantRepresentativeCantHaveSameEORI() => CombineAssertions(() =>
	{
		var message = "Supplier, Declarant and Representative cannot all have the same EORI number.";

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.SupplierDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("same").PK;
		AssertHasMessageError("all have same eori", declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.SupplierDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("different").PK;
		AssertNoMessageError("supplier has different eori", declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.SupplierDocumentaryAddress.E2_OA_Address = CreateOrgAddressWithEORI("").PK;
		AssertNoMessageError("supplier has no eori", declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, message);

		declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Missing;
		AssertNoMessageError("supplier is not filled", declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, message);
	});

	public void TestCheckJE_DeclarantType_ShouldBeIND() => CombineAssertions(() =>
	{
		var message = "The Representative Type must be IND since Declarant and Representative have the same EORI number.";
		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_DeclarantType = "IND";
		AssertNoMessageError("both are same, is IND", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_DeclarantType = "XXX";
		AssertHasMessageError("both are same, is not IND", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageError("both are different, is not IND", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_DeclarantType = "IND";
		AssertNoMessageError("both are different, is IND", declaration.JE_DeclarantTypeInfo, message);
	});

	public void TestCheckJE_DeclarantType_ShouldBeDIR() => CombineAssertions(() =>
	{
		var message = "The Representative Type must be DIR as the Declarant and Representative do not have the same EORI number.";

		declaration.JE_OA_Representative = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("same").PK;
		declaration.JE_DeclarantType = "IND";
		AssertNoMessageError("same not DIR", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_DeclarantType = "DIR";
		AssertNoMessageError("same DIR", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("different").PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageError("different DIR", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_DeclarantType = "XXX";
		AssertHasMessageError("different not DIR", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_OA_DeclarantAddress = ZGuid.Missing;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageError("different (missing address) not DIR", declaration.JE_DeclarantTypeInfo, message);

		declaration.JE_OA_DeclarantAddress = CreateOrgAddressWithEORI("").PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageError("different (missing eori) not DIR", declaration.JE_DeclarantTypeInfo, message);
	});

	public void TestCheckJE_DefermentAccountNumber()
	{
		var dec = Factory.New<JobDeclaration>();

		dec.JE_DefermentAccountNumber = "";
		dec.Validation.ValidateJE_DefermentAccountNumber();
		AssertNoErrors(dec.JE_DefermentAccountNumberInfo);

		dec.JE_DefermentAccountNumber = "abcdef78901234567";
		dec.Validation.ValidateJE_DefermentAccountNumber();
		AssertNoErrors(dec.JE_DefermentAccountNumberInfo);

		dec.JE_DefermentAccountNumber = "12345678";
		dec.Validation.ValidateJE_DefermentAccountNumber();
		AssertHasErrors(dec.JE_DefermentAccountNumberInfo);

		dec.JE_DefermentAccountNumber = "@bcdef78901234567";
		dec.Validation.ValidateJE_DefermentAccountNumber();
		AssertHasErrors(dec.JE_DefermentAccountNumberInfo);

		AssertEquals(17, dec.JE_DefermentAccountNumberInfo.MaxLength);
	}

	public void TestCheckJE_PaymentMethod()
	{
		var dec = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			foreach (var paymentMethod in new PaymentMethodList().GetAllCodes())
			{
				dec.JE_PaymentMethod = paymentMethod;
				dec.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageErrors(dec.JE_PaymentMethodInfo);
			}

			dec.JE_PaymentMethod = "Z";
			dec.Validation.ValidateJE_PaymentMethod();
			AssertHasMessageErrorContaining(dec.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCheckJE_OH_Buyer()
	{
		var jobDec = Factory.New<JobDeclaration>();
		jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

		OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
		testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testImporter.MainAddress.OA_Address1 = "Changi Airport";
		testImporter.MainAddress.OA_Address2 = "Building 3C";
		testImporter.OH_Code = "TEST";
		testImporter.PrimaryRegistrationNumber.Number = string.Empty;

		var importerAddress = Factory.New<OrgAddress>();
		importerAddress.OA_OH = testImporter.PK;
		importerAddress.OA_Address1 = "Eugene Leroy Street ";
		importerAddress.CompanyName = "test Declarant";

		OrgHeader testBuyer = Factory.NewWithValidTestData<OrgHeader>();
		testBuyer.OH_FullName = "London Test Supplier DHL Ltd.";
		testBuyer.MainAddress.OA_Address1 = "Heathrow Airport";
		testBuyer.MainAddress.OA_Address2 = "Building 1B";
		testBuyer.OH_Code = "TEST";
		testBuyer.PrimaryRegistrationNumber.Number = string.Empty;

		var buyerAddress = Factory.New<OrgAddress>();
		buyerAddress.OA_OH = testBuyer.PK;
		buyerAddress.OA_Address1 = "Eugene Leroy Street ";
		buyerAddress.CompanyName = "test Declarant";

		var declarantAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "London test Declarant Corp";
		declarantAddress.OA_OH = orgHeader.PK;
		declarantAddress.OA_Address1 = "Eugene Leroy Street ";
		declarantAddress.CompanyName = "test Declarant";

		string eoriMustExist = "Buyer must have an EORI Number entered in Organizations Registration Numbers / Codes.";

		jobDec.JE_OA_DeclarantAddress = declarantAddress.PK;
		jobDec.JE_OA_ImporterAddress = importerAddress.PK;
		jobDec.ZG_IsHighValueOvrd = true;
		jobDec.Validation.ValidateJE_OA_ConsigneeAddress();

		AssertHasMessageErrorContaining(jobDec.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

		jobDec.JE_OA_ConsigneeAddress = buyerAddress.PK;

		AssertHasMessageErrorContaining(jobDec.JE_OA_ConsigneeAddressInfo, eoriMustExist);

		jobDec.ConsigneeOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Belgium);

		jobDec.Validation.ValidateJE_OA_ConsigneeAddress();
		AssertNoErrors(jobDec.JE_OA_ConsigneeAddressInfo);
		AssertNoMessageErrors(jobDec.JE_OA_ConsigneeAddressInfo);
	}

	public void TestCheckJE_OH_Seller()
	{
		var jobDec = Factory.New<JobDeclaration>();
		jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

		OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
		testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testImporter.MainAddress.OA_Address1 = "Changi Airport";
		testImporter.MainAddress.OA_Address2 = "Building 3C";
		testImporter.OH_Code = "TEST";
		testImporter.PrimaryRegistrationNumber.Number = string.Empty;

		var importerAddress = Factory.New<OrgAddress>();
		importerAddress.OA_OH = testImporter.PK;
		importerAddress.OA_Address1 = "Eugene Leroy Street ";
		importerAddress.CompanyName = "test Declarant";

		OrgHeader testSeller = Factory.NewWithValidTestData<OrgHeader>();
		testSeller.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testSeller.MainAddress.OA_Address1 = "Changi Airport";
		testSeller.MainAddress.OA_Address2 = "Building 3C";
		testSeller.OH_Code = "TEST";

		var sellerAddress = Factory.New<OrgAddress>();
		sellerAddress.OA_OH = testSeller.PK;
		sellerAddress.OA_Address1 = "Eugene Leroy Street ";
		sellerAddress.CompanyName = "test Declarant";

		var declarantAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "London test Declarant Corp";
		declarantAddress.OA_OH = orgHeader.PK;
		declarantAddress.OA_Address1 = "Eugene Leroy Street ";
		declarantAddress.CompanyName = "test Declarant";

		string eoriMustExist = "Seller must have an EORI Number entered in Organizations Registration Numbers / Codes.";

		jobDec.JE_OA_DeclarantAddress = declarantAddress.PK;
		jobDec.JE_OA_ImporterAddress = importerAddress.PK;
		jobDec.ZG_IsHighValueOvrd = true;
		jobDec.Validation.ValidateJE_OA_SellerAddress();

		AssertHasMessageErrorContaining(jobDec.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);

		jobDec.JE_OA_SellerAddress = sellerAddress.PK;

		AssertHasMessageErrorContaining(jobDec.JE_OA_SellerAddressInfo, eoriMustExist);

		jobDec.Seller.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Belgium);

		jobDec.Validation.ValidateJE_OA_SellerAddress();
		AssertNoErrors(jobDec.JE_OA_SellerAddressInfo);
		AssertNoMessageErrors(jobDec.JE_OA_SellerAddressInfo);
	}

	public void TestCheckJE_IATALoadPort()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_TransportMode = "AIR";
		dec.JE_MessageType = "IMP";
		dec.JE_IATALoadPort = ZString.Empty;
		dec.Validation.ValidateJE_IATALoadPort();
		AssertHasMessageError(dec.JE_IATALoadPortInfo, "IATA should be filled in for Import Shipments when [25. Transport] is \"AIR\".");

		dec.JE_IATALoadPort = "X";
		dec.Validation.ValidateJE_IATALoadPort();
		AssertNoMessageError(dec.JE_IATALoadPortInfo, "IATA should be filled in for Import Shipments when [25. Transport] is \"AIR\".");

		dec.JE_MessageType = "EXP";
		dec.JE_IATALoadPort = ZString.Empty;
		dec.Validation.ValidateJE_IATALoadPort();
		AssertNoMessageError(dec.JE_IATALoadPortInfo, "IATA should be filled in for Import Shipments when [25. Transport] is \"AIR\".");
	}

	public void TestCheckJE_LocationOfGoodsAttributeTypes()
	{
		TestCheckJE_LocationOfGoodsAttributeTypes("AIR", new string[] { "D&A Locatie", "Luchthaven", "Pakhuis" });
		TestCheckJE_LocationOfGoodsAttributeTypes("SEA", new string[] { "D&A Locatie", "Zeehaven", "Pakhuis" });
		TestCheckJE_LocationOfGoodsAttributeTypes("RAI", new string[] { "D&A Locatie", "Luchthaven", "Zeehaven", "Pakhuis" });
		TestCheckJE_LocationOfGoodsAttributeTypes("ROA", new string[] { "D&A Locatie", "Luchthaven", "Zeehaven", "Pakhuis" });
	}

	OrgAddress CreateOrgAddressWithEORI(string eori)
	{
		var address = Factory.NewWithValidTestData<OrgAddress>();
		address.OA_RN_NKCountryCode = "BE";
		var customCode = address.Header.CustomsCodes.AddNew();
		customCode.OK_CodeType = "EOR";
		customCode.OK_RN_NKCodeCountry = "DE";
		customCode.OK_CustomsRegNo = eori;
		return address;
	}

	void TestCheckJE_LocationOfGoodsAttributeTypes(string transportMode, string[] validAttributeTypes)
	{
		var allAttributeTypes = new string[] { "D&A Locatie", "Luchthaven", "Zeehaven", "Pakhuis", "failforall" };
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("FAC", "FAC");
		allAttributeTypes.ToList().ForEach(li => helper.CreateCusCodeListWithAttribute("BE", "FAC", li + "_code", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", li));
		Factory.Save();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_TransportMode = transportMode;

		foreach (var attributeType in allAttributeTypes)
		{
			dec.JE_LocationOfGoods = attributeType + "_code";

			if (new List<string>(validAttributeTypes).Contains(attributeType))
			{
				AssertNoMessageErrors(dec.JE_LocationOfGoodsInfo);
			}
			else
			{
				AssertHasMessageErrors(dec.JE_LocationOfGoodsInfo);
			}
		}
	}

	public void TestCheckJE_LocationOfGoodsQuery()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("FAC", "FAC");
		helper.CreateNewOrGetExistingCusCodeType("DIF", "DIF");
		helper.CreateCusCodeListWithAttribute("BE", "FAC", "valid", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", "D&A Locatie");
		helper.CreateCusCodeListWithAttribute("BE", "DIF", "invalid1", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", "D&A Locatie");
		helper.CreateCusCodeListWithAttribute("FR", "FAC", "invalid2", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Type", "D&A Locatie");
		helper.CreateCusCodeListWithAttribute("BE", "FAC", "invalid3", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "NotType", "D&A Locatie");
		Factory.Save();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_TransportModeInland = "AIR";

		dec.JE_LocationOfGoods = "valid";
		AssertNoMessageError(dec.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		dec.JE_LocationOfGoods = "invalid1";
		AssertHasMessageError(dec.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		dec.JE_LocationOfGoods = "invalid2";
		AssertHasMessageError(dec.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		dec.JE_LocationOfGoods = "invalid3";
		AssertHasMessageError(dec.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJE_CustomsOffice()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
		helper.CreateCusCodeListWithAttribute("BE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VALID", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		CombineAssertions(() =>
		{
			declaration.JE_CustomsOffice = "VALID";
			AssertNoErrorContaining("No error", declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeError);
			declaration.JE_CustomsOffice = "INVALID";
			AssertHasErrorContaining("Error", declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckJE_DeclarantType_Export_SELShouldBeUsed()
	{
		var messageError = "When Exporter is same organization as the 'Organization Proxy', then there is no representation and option SEL should be used";
		var exportDeclaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			exportDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			exportDeclaration.ExporterDocAddress.OrganisationPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			exportDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertHasMessageError("Export: Message error when exporter = logged in organization", exportDeclaration.JE_DeclarantTypeInfo, messageError);
			exportDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNoMessageError("Export no message error when SEL is selected", exportDeclaration.JE_DeclarantTypeInfo, messageError);
			AssertNoWarnings("There should be no validation on Power of attorney", exportDeclaration.JE_DeclarantTypeInfo);
		});
	}

	public void TestCheckJE_DeclarantType_Import_SELShouldBeUsed()
	{
		var messageError = "When Importer is same organization as the 'Organization Proxy', then there is no representation and option SEL should be used";
		var importDeclaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			importDeclaration.JE_OH_Importer = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			importDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			importDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertHasMessageError("Import: Message error when importer = logged in organization", importDeclaration.JE_DeclarantTypeInfo, messageError);
			importDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNoMessageError("Import no message error when SEL is selected", importDeclaration.JE_DeclarantTypeInfo, messageError);
			AssertNoWarnings("There should be no validation on Power of attorney", importDeclaration.JE_DeclarantTypeInfo);
		});
	}

	public void TestValidatePowerOfAttorney()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		var exporter = Factory.NewWithValidTestData<OrgHeader>();

		var exportDeclaration = Factory.New<JobDeclaration>();
		exportDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		exportDeclaration.JE_OH_Supplier = supplier.PK;
		exportDeclaration.JE_OH_Exporter = exporter.PK;
		exportDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

		AssertHasWarnings("There should be a validation on Power of attorney", exportDeclaration.JE_DeclarantTypeInfo);

		var pocDocument = exportDeclaration.Exporter.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
		pocDocument.EQ_DocDescription = "Power of Attorney Customs";
		pocDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
		pocDocument.EQ_DateReceived = ZDateTimeOffset.Today;
		pocDocument.EQ_ValidToDate = ZDateTime.Today.AddMonths(1);

		exportDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		AssertNoWarnings("There should be no validation on Power of attorney", exportDeclaration.JE_DeclarantTypeInfo);

		pocDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
		exportDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		AssertHasWarnings("There should be a validation on Power of attorney because the document is expired", exportDeclaration.JE_DeclarantTypeInfo);
	}
}
