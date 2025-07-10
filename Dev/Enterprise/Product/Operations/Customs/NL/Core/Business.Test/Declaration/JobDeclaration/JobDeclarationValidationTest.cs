using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class JobDeclarationValidationTest : EU.Business.Declaration.Testing.JobDeclarationValidationTest
{
	public void Test_JE_TransportMeans_ListValidation()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;

		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_TransportMeansInfo, "AA", ExportInlandTransportTypeList.Codes._30);
	}

	protected override EU.Business.Declaration.JobDeclaration SetUpDeclarationForTestValidateJE_PaymentMethod()
	{
		var declaration = Factory.New<JobDeclaration>();
		var importer = Factory.New<OrgHeader>();
		importer.FillWithValidTestData();
		declaration.JE_OH_Importer = importer.PK;

		var controllingAgent = Factory.New<OrgHeader>();
		controllingAgent.FillWithValidTestData(); //Make sure this doesnt already have any unwanted info.
		declaration.JE_OH_ControllingAgent = controllingAgent.PK;

		return declaration;
	}

	protected override void AssertParty_TestValidateJE_PaymentMethod(EU.Business.Declaration.JobDeclaration declaration)
	{
		CombineAssertions(() =>
		{
			var errorMessage = "The Importer requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)";

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertHasMessageError("When JE_PaymentMethod = A and there is no EORI number of the importer", declaration.JE_PaymentMethodInfo, errorMessage);

			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertHasMessageError("When JE_PaymentMethod = A and the EORI number of the importer is empty", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.Importer.CustomsCodes.DeleteAll();

			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertNoMessageError("When JE_PaymentMethod = A and there is an EORI number of the importer from NL", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.Importer.CustomsCodes.DeleteAll();

			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE123", CountryCodes.Belgium);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertNoMessageError("When JE_PaymentMethod = A and there is an EORI number of the importer from other countries", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.Importer.CustomsCodes.DeleteAll();

			errorMessage = "The Controlling agent requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)";
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertHasMessageError("When JE_PaymentMethod = B and there is no EORI number of the ControllingAgent", declaration.JE_PaymentMethodInfo, errorMessage);

			declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertHasMessageError("When JE_PaymentMethod = B and the EORI number of the ControllingAgent is empty", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.ControllingAgent.CustomsCodes.DeleteAll();

			declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertNoMessageError("When JE_PaymentMethod = B and there is an EORI number of the ControllingAgent from NL", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.ControllingAgent.CustomsCodes.DeleteAll();

			declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE123", CountryCodes.Belgium);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertNoMessageError("When JE_PaymentMethod = B and there is an EORI number of the ControllingAgent from other countries", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.ControllingAgent.CustomsCodes.DeleteAll();

			errorMessage = "EORI number from representative is required.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var representative = Factory.New<OrgHeader>();
			representative.FillWithValidTestData();
			var orgA = representative.Addresses[0];
			orgA.OA_RN_NKCountryCode = "NL";
			declaration.JE_OA_Representative = orgA.PK;

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertHasMessageError("When JE_PaymentMethod = B and there is no EORI number of the Representative", declaration.JE_PaymentMethodInfo, errorMessage);

			declaration.Representative.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertHasMessageError("When JE_PaymentMethod = B and the EORI number of the Representative is empty", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.Representative.Header.CustomsCodes.DeleteAll();

			declaration.Representative.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", CountryCodes.Netherlands);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertNoMessageError("When JE_PaymentMethod = B and there is an EORI number of the Representative from NL", declaration.JE_PaymentMethodInfo, errorMessage);
			declaration.Representative.Header.CustomsCodes.DeleteAll();

			declaration.Representative.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE123", CountryCodes.Belgium);
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertNoMessageError("When JE_PaymentMethod = B and there is an EORI number of the Representative from other countries", declaration.JE_PaymentMethodInfo, errorMessage);
		});
	}

	public void TestCheckJE_PaymentMethod()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_FormattedProcedure = "XXXXF48";
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertHasWarning(declaration.JE_PaymentMethodInfo, "No deferment party found in the declaration or the deferment party does not have a VAT number configured (Edit Organization > Details > Details > Config > Registration Numbers / Codes).");

		invoiceLine.JI_FormattedProcedure = "XXXXXXX";
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;
		AssertNoWarning(declaration.JE_PaymentMethodInfo, "No deferment party found in the declaration or the deferment party does not have a VAT number configured (Edit Organization > Details > Details > Config > Registration Numbers / Codes).");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_FormattedProcedure = "XXXXF48";
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertNoWarning(declaration.JE_PaymentMethodInfo, "No deferment party found in the declaration or the deferment party does not have a VAT number configured (Edit Organization > Details > Details > Config > Registration Numbers / Codes).");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var orgHeaderDeferment = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderDeferment, Core.Constants.CountryCodes.Netherlands, "1234567", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands));
		orgHeaderDeferment.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressDeferment = Factory.New<OrgAddress>();
		addressDeferment.OA_Code = "AAA";
		addressDeferment.OA_OH = orgHeaderDeferment.PK;
		addressDeferment.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment.PK;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertNoWarning(declaration.JE_PaymentMethodInfo, "No deferment party found in the declaration or the deferment party does not have a VAT number configured (Edit Organization > Details > Details > Config > Registration Numbers / Codes).");

		var expectedMessage = "EORI number from Controlling Agent or EORI from Deferment party is required here.";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var nl = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.Netherlands);

		var orgDeclarant = Factory.NewWithValidTestData<OrgHeader>();
		var orgDeclarantAddress = Factory.New<OrgAddress>();
		orgDeclarantAddress.OA_Address1 = "AD1";
		orgDeclarantAddress.OA_OH = orgDeclarant.PK;
		declaration.JE_OA_DeclarantAddress = orgDeclarantAddress.PK;

		var orgControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;

		AssertHasMessageError(declaration.JE_PaymentMethodInfo, expectedMessage);

		declaration.JE_DeclarantType = Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._1Self;
		declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", nl);
		declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL456", nl);
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;

		AssertNoMessageError(declaration.JE_PaymentMethodInfo, expectedMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_DeclarantType = ZString.Empty;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertNoMessageError(declaration.JE_PaymentMethodInfo, expectedMessage);
	}

	OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
	{
		OrgCusCode taxCode = organisation.CustomsCodes.AddNew();
		taxCode.OK_RN_NKCodeCountry = country;
		taxCode.OK_CodeType = codeType ?? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		taxCode.OK_CustomsRegNo = customsRegNo;
		return taxCode;
	}

	protected override void AssertPartyForDefermentMethod_TestValidateJE_PaymentMethod(EU.Business.Declaration.JobDeclaration declaration, string letter)
	{
		var nl = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.Netherlands);

		declaration.JE_PaymentMethod = letter;
		AssertHasMessageError(letter, declaration.JE_PaymentMethodInfo, "The Controlling agent requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)");

		declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", nl);
		declaration.JE_PaymentMethod = letter;
		AssertHasMessageError(letter, declaration.JE_PaymentMethodInfo, "The Controlling agent requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)");
		declaration.ControllingAgent.CustomsCodes.RemoveAll();

		declaration.ControllingAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", nl);
		declaration.JE_PaymentMethod = letter;
		AssertNoMessageError(letter, declaration.JE_PaymentMethodInfo, "The Controlling agent requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)");
		declaration.ControllingAgent.CustomsCodes.RemoveAll();
	}

	public void TestCheckJE_LocationQualifier()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();

		declaration.JE_LocationQualifier = "1";
		AssertHasMessageErrorContaining(declaration.JE_LocationQualifierInfo, "The code you have selected is not in the list.");

		declaration.JE_LocationQualifier = ZString.Empty;
		AssertNoMessageErrors(declaration.JE_LocationQualifierInfo);

		declaration.JE_LocationQualifier = LocationQualifierList.Codes.DesignatedPlace;
		AssertNoMessageErrors(declaration.JE_LocationQualifierInfo);
	}

	public void TestJE_LocationQualifierMaxLength()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		AssertEquals(1, declaration.JE_LocationQualifierInfo.MaxLength);
	}

	public void TestCheckJE_OH_ControllingCustomer()
	{
		var message = "The Guarantee Provider Party requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)";

		CombineAssertions(() =>
		{
			var nl = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.Netherlands);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_ControllingCustomer = ZGuid.Empty;
			AssertNoMessageError("No Controlling Customer", declaration.JE_OH_ControllingCustomerInfo, message);

			var surety = Factory.New<OrgHeader>();
			surety.FillWithValidTestData();
			declaration.JE_OH_ControllingCustomer = surety.PK;
			AssertHasMessageError("Controlling Customer exists but has no Eori", declaration.JE_OH_ControllingCustomerInfo, message);

			declaration.ControllingCustomer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", nl);
			declaration.JE_OH_ControllingCustomer = surety.PK;
			AssertHasMessageError("Controlling Customer exists but its Eori is empty", declaration.JE_OH_ControllingCustomerInfo, message);
			declaration.ControllingCustomer.CustomsCodes.DeleteAll();

			declaration.ControllingCustomer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "NL123", nl);
			declaration.JE_OH_ControllingCustomer = surety.PK;
			AssertNoMessageError("Controlling Customer exists but its Eori is not empty", declaration.JE_OH_ControllingCustomerInfo, message);
		});
	}

	public void TestCheckJE_OH_ControllingAgent()
	{
		var messageError = "The Controlling Agent must have a HTG Sender ID in the registry";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		declaration.JE_DeclarantType = "";
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		declaration.JE_DeclarantType = "DIR";
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		var orgControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		AssertHasMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		orgControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
		var collection = new SenderInfoCollection();
		var senderInfo = collection.AddNew();
		senderInfo.OrganizationPK = orgControllingAgent.PK;
		senderInfo.SenderID = "123";
		senderInfo.DefaultSenderID = true;
		Factory.Save();

		NLCustomsRegistry.Instance.SenderIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		declaration.JE_DeclarantType = "IND";
		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		messageError = "Agent details required for this procedure type";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "000000";
		entryInstruction.CEI_SubStyle = "V";
		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);
		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		AssertHasMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		entryInstruction.CEI_SubStyle = "Z";
		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);
		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		AssertHasMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);

		entryInstruction.CEI_SubStyle = "A";
		declaration.JE_OH_ControllingAgent = orgControllingAgent.PK;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);
		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OH_ControllingAgentInfo, messageError);
	}

	public override void TestCheckJE_OA_DeclarantAddress()
	{
		base.TestCheckJE_OA_DeclarantAddress();

		var messageError = "The Declarant must have a HTG Sender ID in the registry";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_DeclarantType = "";
		AssertNoMessageError("Declarant is empty", declaration.JE_OA_DeclarantAddressInfo, messageError);

		declaration.JE_DeclarantType = "IND";
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageError("IND", declaration.JE_OA_DeclarantAddressInfo, messageError);

		declaration.JE_DeclarantType = "SEL";
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageError("SEL", declaration.JE_OA_DeclarantAddressInfo, messageError);

		var orgDeclarant = Factory.NewWithValidTestData<OrgHeader>();
		var orgDeclarantAddress = Factory.New<OrgAddress>();
		orgDeclarantAddress.OA_Address1 = "AD1";
		orgDeclarantAddress.OA_OH = orgDeclarant.PK;
		declaration.JE_OA_DeclarantAddress = orgDeclarantAddress.PK;
		AssertHasMessageError("Declarant filled in", declaration.JE_OA_DeclarantAddressInfo, messageError);

		declaration.JE_DeclarantType = "IND";
		declaration.JE_OA_DeclarantAddress = orgDeclarantAddress.PK;
		AssertHasMessageError("IND 2", declaration.JE_OA_DeclarantAddressInfo, messageError);

		orgDeclarant = Factory.NewWithValidTestData<OrgHeader>();
		orgDeclarantAddress = Factory.New<OrgAddress>();
		orgDeclarantAddress.OA_Address1 = "AD1";
		orgDeclarantAddress.OA_OH = orgDeclarant.PK;
		var collection = new SenderInfoCollection();
		var senderInfo = collection.AddNew();
		senderInfo.OrganizationPK = orgDeclarant.PK;
		senderInfo.SenderID = "123";
		senderInfo.DefaultSenderID = true;
		Factory.Save();

		NLCustomsRegistry.Instance.SenderIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		declaration.JE_OA_DeclarantAddress = orgDeclarantAddress.PK;
		AssertNoMessageError("Declarant filled in 2", declaration.JE_OA_DeclarantAddressInfo, messageError);

		declaration.JE_DeclarantType = "SEL";
		declaration.JE_OA_DeclarantAddress = orgDeclarantAddress.PK;
		AssertNoMessageError("SEL 2", declaration.JE_OA_DeclarantAddressInfo, messageError);

		declaration.JE_DeclarantType = "DIR";
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageError("DIR", declaration.JE_OA_DeclarantAddressInfo, messageError);
	}

	public void TestCheckDeclarant()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertOrgEORIorAddress(declaration.JE_OA_DeclarantAddressInfo, "Declarant");
	}

	public void TestCheckBuyer()
	{
		var declaration = Factory.New<JobDeclaration>();
		var expectedMessage = "Fill buyer details";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;

		AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);

		var orgBuyer = Factory.NewWithValidTestData<OrgHeader>();
		var orgBuyerAddress = Factory.New<OrgAddress>();
		orgBuyerAddress.OA_Address1 = "AD1";
		orgBuyerAddress.OA_OH = orgBuyer.PK;
		declaration.JE_OA_ConsigneeAddress = orgBuyerAddress.PK;

		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "0";
		declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;

		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "0";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OA_ConsigneeAddress = orgBuyerAddress.PK;
		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);

		declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OA_ConsigneeAddress = orgBuyerAddress.PK;

		var jobDocAddress = Factory.New<JobDocAddress>();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		jobDocAddress.E2_AddressType = DocAddressTypes.Codes.BuyingParty;
		jobDocAddress.OrganisationPK = orgHeader.PK;
		jobDocAddress.E2_ParentTableCode = "JI";
		jobDocAddress.E2_ParentID = invoiceLine.PK;

		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);
		declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);
	}

	public void TestCheckImporter_ImportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertOrgEORIorAddress(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Importer");
	}

	public void TestCheckImporter_ExportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertOrgAddress(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Importer");
	}

	public void TestCheckSupplier()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertOrgEORIorAddress(declaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, "Supplier");
	}

	public void TestCheckExporter()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertOrgEORI(declaration.JE_OH_ExporterInfo, "Exporter");
	}

	public void TestCheckSeller()
	{
		var declaration = Factory.New<JobDeclaration>();
		var expectedMessage = "Fill seller details";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_OA_SellerAddress = ZGuid.Empty;

		AssertHasMessageError(declaration.JE_OA_SellerAddressInfo, expectedMessage);

		var orgBuyer = Factory.NewWithValidTestData<OrgHeader>();
		var orgBuyerAddress = Factory.New<OrgAddress>();
		orgBuyerAddress.OA_Address1 = "AD1";
		orgBuyerAddress.OA_OH = orgBuyer.PK;
		declaration.JE_OA_SellerAddress = orgBuyerAddress.PK;

		AssertNoMessageError(declaration.JE_OA_SellerAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "0";
		declaration.JE_OA_SellerAddress = ZGuid.Empty;

		AssertNoMessageError(declaration.JE_OA_SellerAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "0";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OA_SellerAddress = orgBuyerAddress.PK;
		AssertNoMessageError(declaration.JE_OA_SellerAddressInfo, expectedMessage);

		declaration.JE_OA_SellerAddress = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OA_SellerAddressInfo, expectedMessage);

		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OA_ConsigneeAddress = orgBuyerAddress.PK;

		var jobDocAddress = Factory.New<JobDocAddress>();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		jobDocAddress.E2_AddressType = DocAddressTypes.Codes.SellingParty;
		jobDocAddress.OrganisationPK = orgHeader.PK;
		jobDocAddress.E2_ParentTableCode = "JI";
		jobDocAddress.E2_ParentID = invoiceLine.PK;

		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);
		declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
		AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, expectedMessage);
	}

	public void TestCheckControllingAgent()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertOrgEORI(declaration.JE_OH_ControllingAgentInfo, "Controlling Agent");
	}

	void AssertOrgEORIorAddress(ZPropertyInfo orgAddressPropertyInfo, ZString fieldText)
	{
		string eoriOrAddressMustExist = "Please add either EORI number for " + fieldText + " OR complete the address details (Name, address, postal code, city and country)";
		string eoriMustExist = "EORI number is missing in organization details of " + fieldText;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		Factory.Save();
		var address = orgHeader.Addresses.AddNew();
		var address1 = orgHeader.Addresses.AddNew();

		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);

		var eori = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Netherlands);
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertNoMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);

		orgHeader.CustomsCodes.RemoveAndDelete(eori);
		address.OA_CompanyNameOverride = "XXX";
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
		address.OA_Address1 = "Address1";
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
		address.OA_City = "City";
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
		address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
		address.OA_PostCode = "PostCode";
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriMustExist);

		orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Netherlands);
		orgAddressPropertyInfo.Value = address1.PK;
		orgAddressPropertyInfo.Value = address.PK;
		AssertNoMessageError(orgAddressPropertyInfo, eoriMustExist);
		AssertNoMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
	}

	void AssertOrgEORI(ZPropertyInfo orgAddressPropertyInfo, ZString fieldText)
	{
		string eoriOrAddressMustExist = "EORI number is missing in organization details of " + fieldText;

		var orgHeader = Factory.New<OrgHeader>();
		orgAddressPropertyInfo.Value = orgHeader.PK;
		AssertHasMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
		orgHeader.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, GlbCompany.CurrentCompany.Country, "999999999888");
		orgAddressPropertyInfo.Value = orgHeader.PK;
		AssertNoMessageError(orgAddressPropertyInfo, eoriOrAddressMustExist);
	}

	void AssertOrgAddress(ZPropertyInfo orgAddressPropertyInfo, ZString fieldText)
	{
		var addressMustExist = "Please complete the address details (Name, address, postal code, city and country) for " + fieldText;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		Factory.Save();
		var address = orgHeader.Addresses.AddNew();
		var address1 = orgHeader.Addresses.AddNew();

		CombineAssertions(() =>
		{
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertHasMessageError(orgAddressPropertyInfo, addressMustExist);

			address.OA_CompanyNameOverride = "XXX";
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertHasMessageError(orgAddressPropertyInfo, addressMustExist);
			address.OA_Address1 = "Address";
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertHasMessageError(orgAddressPropertyInfo, addressMustExist);
			address.OA_City = "City";
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertHasMessageError(orgAddressPropertyInfo, addressMustExist);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertHasMessageError(orgAddressPropertyInfo, addressMustExist);
			address.OA_PostCode = "PostCode";
			orgAddressPropertyInfo.Value = address1.PK;
			orgAddressPropertyInfo.Value = address.PK;
			AssertNoMessageError(orgAddressPropertyInfo, addressMustExist);
		});
	}

	public new void TestCheckJE_ShipmentIncoTerm()
	{
		var declaration = Factory.New<JobDeclaration>();
		string expectedError = MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_ShipmentIncoTermInfo.HumanReadableName);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CL = customEntryInstruction.PK;
		customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
		customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_ShipmentIncoTerm = "CIF";
		AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, expectedError);

		declaration.JE_ShipmentIncoTerm = ZString.Empty;
		AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, expectedError);

		declaration.JE_ShipmentIncoTerm = "CIF";
		invoiceLine.JI_ValuationCode = "2";
		declaration.JE_ShipmentIncoTerm = ZString.Empty;
		AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, expectedError);

		declaration.JE_ShipmentIncoTerm = "CIF";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_ValuationCode = "1";
		declaration.JE_ShipmentIncoTerm = ZString.Empty;
		AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, expectedError);
	}

	public new void TestCheckJE_RN_NKTransportNationality()
	{
		var declaration = Factory.New<JobDeclaration>();
		var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_RN_NKTransportNationalityInfo.HumanReadableName);
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportModes.Road;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);

		declaration.JE_RN_NKTransportNationality = CountryCodes.Netherlands;
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);

		declaration.JE_TransportMode = TransportModes.Rail;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);

		declaration.JE_TransportMode = TransportModes.Mail;
		declaration.JE_RN_NKTransportNationality = CountryCodes.Netherlands;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);

		declaration.JE_TransportMode = TransportModes.OwnPropulsion;
		declaration.JE_RN_NKTransportNationality = CountryCodes.Netherlands;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_TransportMode = TransportModes.Road;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, errorMessage);
	}

	public void TestValidateImporterDocumentaryAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var orgHeaderImporter = Factory.New<OrgHeader>();
		orgHeaderImporter.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		orgHeaderImporter.OH_FullName = "Importername";
		OrgAddress addressImporter = Factory.New<OrgAddress>();
		addressImporter.OA_Code = "BBB";
		addressImporter.OA_OH = orgHeaderImporter.PK;
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertHasMessageError(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Please add VAT number for this organization.");

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertNoMessageError(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Please add VAT number for this organization.");

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.France, "123456", OrgCusCode.FranceCodeTypes.TVA);
		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertHasWarning(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Fiscal representation may be applicable.");

		orgHeaderImporter.CustomsCodes.RemoveAll();
		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.Netherlands, "654321", OrgCusCode.EuropeanUnionSharedCodeTypes.BTW);
		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertNoMessageError(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Please add VAT number for this organization.");
		AssertNoWarning(declaration.ImporterDocumentaryAddress.E2_OA_AddressInfo, "Fiscal representation may be applicable.");
	}

	public void TestCheckJE_TransportMeans()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Mail, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Mail, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.FixedTransportInstallations, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.FixedTransportInstallations, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Air, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Air, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.InlandWaterwayTransport, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.InlandWaterwayTransport, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.OwnPropulsion, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.OwnPropulsion, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Rail, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Rail, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Road, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Road, "10");
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Sea, null);
		AssertInlandTransportCodeErrorMessage(declaration, TransportTypeList.Codes.Sea, "10");

		declaration.JE_TransportMeans = "XX";
		AssertHasMessageErrorContaining(declaration.JE_TransportMeansInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJE_RN_NKTransportNationality_Mandatory()
	{
		var expectedError = "[UCC 7/8] Nationality is mandatory if transport mode is AIR, IWT, OWN, ROA or SEA.";
		AssertNoMessageError(declaration.JE_RN_NKTransportNationalityInfo, expectedError);

		CombineAssertions(() =>
		{
			AssertNoMessageError("No transport mode", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("AIR", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("FIX", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.InlandWaterwayTransport;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("IWT", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("MAI", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.OwnPropulsion;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("OWN", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.Rail;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("RAI", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.Road;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("ROA", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("SEA", declaration.JE_RN_NKTransportNationalityInfo, expectedError);
		});
	}

	public override void TestCheckJE_DeclarantType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_DeclarantTypeInfo, "~", GetRepresentationTypeSelfToTest);

		declaration.JE_DeclarantType = GetRepresentationTypeSelfToTest;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
	}

	void AssertInlandTransportCodeErrorMessage(JobDeclaration declaration, ZString transportMode, ZString inlandTransportCode)
	{
		const string errorMessageMaiFix = "When '[UCC 7/5] Inland M.O.T' = MAI or FIX, then 'Code' must be blank'";
		const string errorMessageOther = "When '[UCC 7/5] Inland M.O.T' = AIR, IWT, OWN, RAI, ROA or SEA, then 'Code' must be specified";

		declaration.JE_TransportModeInland = transportMode;
		declaration.JE_TransportMeans = inlandTransportCode;

		if (inlandTransportCode.IsEmpty)
		{
			switch (transportMode)
			{
				case TransportTypeList.Codes.Mail:
				case TransportTypeList.Codes.FixedTransportInstallations:
					AssertNoMessageError(declaration.JE_TransportMeansInfo, errorMessageMaiFix);
					break;
				case TransportTypeList.Codes.Air:
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.Rail:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Sea:
					AssertHasMessageError(declaration.JE_TransportMeansInfo, errorMessageOther);
					break;
			}
		}
		else
		{
			switch (transportMode)
			{
				case TransportTypeList.Codes.Mail:
				case TransportTypeList.Codes.FixedTransportInstallations:
					AssertHasMessageError(declaration.JE_TransportMeansInfo, errorMessageMaiFix);
					break;
				case TransportTypeList.Codes.Air:
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.Rail:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Sea:
					AssertNoMessageError(declaration.JE_TransportMeansInfo, errorMessageOther);
					break;
			}
		}
	}
}
