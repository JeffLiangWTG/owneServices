using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DeclarationWrapperTest : DataProviderTestCase<DeclarationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DeclarationWrapper(null));
	}

	public void TestGoodsShipments()
	{
		AssertNotNull(wrapper.GoodsShipments.FirstOrDefault());
	}

	public void TestMovementReferenceNumber()
	{
		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumberSetter("MRN123", new ZDateTime(2021, 09, 24, 15, 21, 00));
			AssertNotNull(wrapper.Id);
			AssertEquals("MRN", "MRN123", wrapper.Id);
		});
	}

	public void TestLanguageCode()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.LanguageCode);
			AssertEquals("Language code", "NL", wrapper.LanguageCode);
		});
	}

	public void TestTypeCode()
	{
		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportNormal;
			entryInstruction.CEI_SubStyle = "A";

			AssertNotNull(wrapper.TypeCode);
			AssertEquals("TypeCode", "IMA", wrapper.TypeCode);
		});
	}

	public void TestDeclarationOffice()
	{
		CombineAssertions(() =>
		{
			declaration.JE_CustomsOffice = "NL55566677";
			AssertNotNull(wrapper.DeclarationOffice);
			AssertEquals("Declaration Office", "NL55566677", wrapper.DeclarationOffice);
		});
	}

	public void TestAgent()
	{
		CombineAssertions(() =>
		{
			var orgHeaderRepresentative = WrapperTestHelper.CreateOrgHeader(Factory, "Representative Full Name", "", "78787878");
			var addressRepresentative = WrapperTestHelper.CreateAddress(orgHeaderRepresentative, "", "Rijksweg 102", "Deventer", "7201MG");
			declaration.JE_OA_Representative = addressRepresentative.PK;
			AssertType<AgentDirectWrapper>("Type", wrapper.Agent);
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNull("Null", wrapper.Agent);
			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertType<AgentIndirectWrapper>(wrapper.Agent);
		});
	}

	public void TestAuthorisations()
	{
		CombineAssertions(() =>
		{
			var cusAuthorizationUsage1 = entryInstruction.CusAuthorizationUsages.AddNew();
			var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertNotNull(wrapper.Authorisations.FirstOrDefault());
			AssertType<AuthorisationWrapper>(wrapper.Authorisations.FirstOrDefault());
		});
	}

	public void TestDeclarant()
	{
			var orgHeaderDeclarant = WrapperTestHelper.CreateOrgHeader(Factory, "Declarant Full Name", "DECLARANT", "56785678");
			declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;

			AssertType<PartyWrapper>("Type", wrapper.Declarant);
			AssertNotNull(wrapper.Declarant);
	}

	public void TestDeclarant_IND()
	{
		entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);

		var orgHeaderDeclarant = WrapperTestHelper.CreateOrgHeader(Factory, "Declarant Full Name", "DECLARANT", "56785678");
		declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;

		wrapper = new DeclarationWrapper(sendingObject);

		AssertType<PartyWithStaffWrapper>("Type", wrapper.Declarant);
		AssertNotNull(wrapper.Declarant);
	}

	public void TestImporter()
	{
		var orgHeaderImporter = WrapperTestHelper.CreateOrgHeader(Factory, "Importer Full Name", "IMPORTER", "22334455");
		declaration.JE_OH_Importer = orgHeaderImporter.PK;
		var import = wrapper.Importer;
		var importAddress = import.Address;
		CombineAssertions(() =>
		{
			AssertNotNull(import);
			AssertType<PartyWrapper>(import);
			AssertNull("Importer Address", importAddress);
			AssertEquals("Importer ID", "NL22334455", import.Id);
		});
	}

	public void TestExporter()
	{
		CombineAssertions(() =>
		{
			var orgHeaderCarrier = WrapperTestHelper.CreateOrgHeader(Factory, "Carrier Full Name", "DIR", "53212346");
			declaration.ExporterDocAddress.OrganisationPK = orgHeaderCarrier.PK;

			AssertType<PartyWrapper>("Type", wrapper.Exporter);
			AssertEquals("Carrier ID", "NL53212346", wrapper.Exporter.Id);
		});
	}

	public void TestExitOffice()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.CustomsOffices.RemoveAndDeleteAll();

		var cusOffice = declaration.CustomsOffices.AddNew();
		cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		cusOffice.CY_Data = "NL000001";

		AssertEquals("ExitOffice", "NL000001", wrapper.ExitOffice);
	}

	public void TestPresentationDateTime()
	{
		declaration.ZG_PresentationStartDate = new ZDateTime(2023, 08, 03);
		AssertEquals("PresentationDateTime", new ZDateTime(2023, 08, 03).ToString("yyyyMMddHHmmssZ"), wrapper.PresentationDateTime);
	}

	public void TestSecurityCode()
	{
		declaration.ZG_TypeOfSecurity = "1";
		AssertEquals("SecurityCode", "1", wrapper.SecurityCode);
	}

	public void TestSpecificCircumstancesCode()
	{
		declaration.ZG_SpecificCircumstanceIndicator = "A";
		AssertEquals("SpecificCircumstancesCode", "A", wrapper.SpecificCircumstancesCode);
	}

	public void TestPayer()
	{
		CombineAssertions(() =>
		{
			var orgHeaderDeclarant = WrapperTestHelper.CreateOrgHeader(Factory, "Declarant Full Name", "DECLARANT", "56785678");
			orgHeaderDeclarant.MainAddress.OA_Code = "CST";
			orgHeaderDeclarant.MainAddress.Address1 = "Importeursweg 37";
			orgHeaderDeclarant.MainAddress.OA_City = "IJburg";
			orgHeaderDeclarant.MainAddress.OA_PostCode = "2222AB";
			orgHeaderDeclarant.MainAddress.OA_RL_NKRelatedPortCode = "NLRTM";
			orgHeaderDeclarant.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			var contact = orgHeaderDeclarant.Contacts.AddNew();
			contact.OC_ContactName = "Declarant Contact Name";
			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryHeader.Declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;

			AssertEquals("Payer", "NL56785678", wrapper.Payer);

			var orgHeaderControllingAgent = WrapperTestHelper.CreateOrgHeader(Factory, "Controlling Agent Full Name", "AGENT", "43434343");
			declaration.JE_OH_ControllingAgent = orgHeaderControllingAgent.PK;
			entryHeader.Declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;

			AssertEquals("Payer", "NL43434343", wrapper.Payer);

			var orgHeaderImporter = WrapperTestHelper.CreateOrgHeader(Factory, "Importer Full Name", "IMPORTER", "22334455");
			declaration.JE_OH_Importer = orgHeaderImporter.PK;
			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryHeader.Declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;

			AssertEquals("Payer", "NL22334455", wrapper.Payer);

			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryHeader.Declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;

			AssertEquals("Payer", "NL56785678", wrapper.Payer);
		});
	}

	public void TestSupervisingOffice()
	{
		CombineAssertions(() =>
		{
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
			customsOffice.CY_Data = "NL12345678";
			AssertNotNull(wrapper.SupervisingOffice);
			AssertEquals("SupervisingOffice SCO check", "NL12345678", wrapper.SupervisingOffice);
		});
	}

	public void TestSurety()
	{
		CombineAssertions(() =>
		{
			var orgHeadercontrollingCustomer = WrapperTestHelper.CreateOrgHeader(Factory, "Controlling Customer Full Name", "CCustomer", "78907890");
			declaration.JE_OH_ControllingCustomer = orgHeadercontrollingCustomer.PK;
			AssertNotNull(wrapper.Surety);
			AssertEquals("Surety", "NL78907890", wrapper.Surety);
		});
	}

	public void TestObligationGuarantees()
	{
		CombineAssertions(() =>
		{
			var guarantee = Factory.New<EU.Business.Declaration.GuaranteeForDeclaration>();
			guarantee.PW_BondType = "0";
			declaration.Guarantees.Add(guarantee);
			var guarantee2 = Factory.New<EU.Business.Declaration.GuaranteeForDeclaration>();
			guarantee2.PW_BondType = "1";
			declaration.Guarantees.Add(guarantee2);

			AssertEquals("Count", 2, wrapper.ObligationGuarantees.Count);

			var secondObligationGuarantee = wrapper.ObligationGuarantees.ElementAt(1);
			AssertType<ObligationGuaranteeWrapper>("Type", secondObligationGuarantee);
			AssertEquals("SequenceNumeric", 2, secondObligationGuarantee.SequenceNumeric);
		});
	}

	public void TestFunctionalReferenceId()
	{
		entryHeader.CH_BGMReference = "EH00001";
		AssertEquals("Declaration Wrapper - FunctionalReferenceID", "EH00001", wrapper.FunctionalReferenceId);
	}

	public void TestId()
	{
		entryHeader.MovementReferenceNumberSetter("MRN123", new ZDateTime(2021, 09, 24, 15, 21, 00));
		AssertEquals("Declaration Wrapper - ID", "MRN123", wrapper.Id);
	}

	[TestDate(2021, 12, 01, 13, 15, 18)]
	public void TestIssueDateTime()
	{
		AssertEquals("Declaration Wrapper - IssueDateTime", "20211201131518Z", wrapper.IssueDateTime);
	}

	public void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("Declaration Wrapper - AdditionalInformation", wrapper.AdditionalInformation.FirstOrDefault());
			AssertType<AdditionalInformationWrapper>("Declaration Wrapper - AdditionalInformation", wrapper.AdditionalInformation.FirstOrDefault());
			AssertEquals("Declaration Wrapper - Number of AdditionalInformations", 1, wrapper.AdditionalInformation.Count);
			AssertEquals("Declaration Wrapper - StatementDescription of 2nd AdditionalInformation", "Reason for invalidation", wrapper.AdditionalInformation.ElementAt(0).StatementDescription);
		});
	}

	public void TestCustomsValuationValue()
	{
		CombineAssertions(() =>
		{
			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "OFT";
			invoiceCharge.J7_RX_NKCurrency = "EUR";
			invoiceCharge.J7_Amount = 12.5;
			var invoiceCharge2 = invoice.Charges.AddNew();
			invoiceCharge2.J7_ChargeType = "OFT";
			invoiceCharge2.J7_RX_NKCurrency = "EUR";
			invoiceCharge2.J7_Amount = 13.0;

			AssertNotNull(wrapper.CustomsValuationValue);
			AssertEquals("Customs Valuation Value", 25.5m, wrapper.CustomsValuationValue);
		});
	}

	public void TestCarrier()
	{
		CombineAssertions(() =>
		{
			var orgHeaderCarrier = WrapperTestHelper.CreateOrgHeader(Factory, "Carrier Full Name", "DIR", "53212346");
			declaration.ExporterDocAddress.OrganisationPK = orgHeaderCarrier.PK;

			AssertType<PartyWrapper>("Type", wrapper.Carrier);
			AssertEquals("Carrier ID", "NL53212346", wrapper.Carrier.Id);
		});
	}

	public void TestCustomsValuationCurrency()
	{
		CombineAssertions(() =>
		{
			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "OFT";
			invoiceCharge.J7_RX_NKCurrency = "EUR";

			AssertNotNull(wrapper.CustomsValuationCurrency);
			AssertEquals("Customs Valuation Currency", "EUR", wrapper.CustomsValuationCurrency);
		});
	}

	protected override DeclarationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.Declarant.Header.Contacts.AddNew();

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_CEI = entryInstruction.PK;

		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();

		sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		sendingObject.MessageType = "INV";
		sendingObject.ReasonForInvalidation = "Reason for invalidation";

		GlbStaff.CurrentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Dutch;
		GlbStaff.CurrentUser.GS_FullName = "Test user";
		GlbStaff.CurrentUser.GS_WorkPhone = "+31687654321";
		GlbStaff.CurrentUser.GS_EmailAddress = "testuser@test.com";

		wrapper = new DeclarationWrapper(sendingObject);
	}
	Declaration.CusEntryHeader entryHeader;
	JobDeclaration declaration;
	JobDeclarationMessageSendingObject sendingObject;
	DeclarationWrapper wrapper;
	Declaration.CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoice;
}
