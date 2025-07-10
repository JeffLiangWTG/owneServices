using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	public void TestUpdateToWarehouseAuthorizationForH2_CEI_OA_Warehouse2()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2;
		var warehouse = Factory.New<OrgHeader>();
		var authorisationHeader = SetupAuthHeaderAndWarehouseAddress("WHP", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, warehouse);
		var warehouse2 = Factory.New<OrgHeader>();
		var authorisationHeader2 = SetupAuthHeaderAndWarehouseAddress("WH2", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, warehouse2);
		AssertEquals("Precondition: CusAuthorizationUsages is empty", 0, entryInstruction.CusAuthorizationUsages.Count);

		entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

		AssertEquals("Add Usage when To Warehouse is set to warehouse", true, entryInstruction.CusAuthorizationUsages.Any(usage => usage.IsForAuthorisationHeader(authorisationHeader)));

		entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
		CombineAssertions("Replace Usage when To Warehouse changes from warehouse to warehouse2", () =>
		{
			AssertEquals("Add Usage for warehouse2", true, entryInstruction.CusAuthorizationUsages.Any(usage => usage.IsForAuthorisationHeader(authorisationHeader2)));
			AssertEquals("Delete Usages of warehouse", false, entryInstruction.CusAuthorizationUsages.Any(usage => usage.IsForAuthorisationHeader(authorisationHeader)));
		});

		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertEquals("Delete Usage of warehouse2 when To Warehouse is set to Empty", false, entryInstruction.CusAuthorizationUsages.Any(usage => usage.IsForAuthorisationHeader(authorisationHeader2)));
	}

	public void TestAddInfoLookups()
	{
		AssertType<AddInfoCusEntryInstructionLookups>(Factory.New<CusEntryInstruction>().AddInfoLookups);
	}

	public void TestLookups()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertType<CusEntryInstructionLookups>(instruction.Lookups);
	}

	public void TestCEI_SubStyleMaxLength()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertEquals(1, CusEntryInstruction.Schema.CEI_SubStyleMaxLength);
		AssertEquals(CusEntryInstruction.Schema.CEI_SubStyleMaxLength, instruction.CEI_SubStyleInfo.MaxLength);
	}

	public void TestZG_TempProcLimitDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2017, 11, 2);
		Factory.Save();
		AssertContains("TempProcLimitDate=2017-11-02 00:00:00.000", entryInstruction.CEI_AddInfo);
	}

	public void TestCEI_ProcedureMaxLength()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Max Length", 2, instruction.CEI_ProcedureInfo.MaxLength);
	}

	public void TestAllRelatedInvoicesHaveSameIncoTerms()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.JZ_IncoTerm = "EXW";
		invoiceHeader2.JZ_IncoTerm = "FOB";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Incoterm?", false, entryInstruction1.AllRelatedInvoicesHaveSameIncoTerms);

		invoiceHeader1.JZ_IncoTerm = "FOB";
		AssertEquals("Do the invoices all have the same Incoterm?", true, entryInstruction1.AllRelatedInvoicesHaveSameIncoTerms);
	}

	public void TestAllRelatedInvoicesHaveSameValuationCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.JZ_ValuationCode = "1";
		invoiceHeader2.JZ_ValuationCode = "2";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Valuation Code?", false, entryInstruction1.AllRelatedInvoicesHaveSameValuationCode);

		invoiceHeader1.JZ_ValuationCode = "2";
		AssertEquals("Do the invoices all have the same Valuation Code?", true, entryInstruction1.AllRelatedInvoicesHaveSameValuationCode);
	}

	public void TestAllRelatedInvoicesHaveSameCurrency()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceHeader2.JZ_RX_NKInvoice_Currency = "AUD";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Currency Code?", false, entryInstruction1.AllRelatedInvoicesHaveSameCurrency);

		invoiceHeader1.JZ_RX_NKInvoice_Currency = "AUD";
		AssertEquals("Do the invoices all have the same Currency Code?", true, entryInstruction1.AllRelatedInvoicesHaveSameCurrency);
	}

	public void TestAllRelatedInvoicesHaveSameDeliveryTerms()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.JZ_AdditionalTerms = "123";
		invoiceHeader2.JZ_AdditionalTerms = "456";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Delivery Terms?", false, entryInstruction1.AllRelatedInvoicesHaveSameDeliveryTerms);

		invoiceHeader1.JZ_AdditionalTerms = "456";
		AssertEquals("Do the invoices all have the same Delivery Terms?", true, entryInstruction1.AllRelatedInvoicesHaveSameDeliveryTerms);
	}

	public void TestIncoterm()
	{
		SetUpRefData();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLineInvoice1 = invoice1.InvoiceLines.AddNew();
		var invoiceLineInvoice2 = invoice2.InvoiceLines.AddNew();
		invoiceLineInvoice1.JI_CEI = ZGuid.Empty;
		invoiceLineInvoice2.JI_CEI = ZGuid.Empty;
		AssertEquals("EntryInstruction has no invoices. Incoterm cannot be evaluated", "", entryInstruction.Incoterm);

		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice but incoterm is empty. Incoterm cannot be evaluated", "", entryInstruction.Incoterm);

		invoice1.JZ_IncoTerm = "XXX";
		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice with incoterm. Incoterm has been evaluated", "XXX", entryInstruction.Incoterm);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_IncoTerm = "XXX";
		AssertEquals("EntryInstruction has 2 linked invoices with same incoterm. Incoterm has been evaluated", "XXX", entryInstruction.Incoterm);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_IncoTerm = "";
		AssertEquals("Entry Instruction has 2 linked invoices with different incoterms. Incoterm cannot be evaluated", "", entryInstruction.Incoterm);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_IncoTerm = "ZZZ";
		AssertEquals("Entry Instruction has 2 linked invoices with different incoterms. Incoterm cannot be evaluated", "", entryInstruction.Incoterm);

		entryInstruction.CEI_Procedure = "71";
		invoice2.JZ_IncoTerm = "XXX";
		AssertEquals("EntryInstruction has 2 linked invoices with same incoterm but procedure code is warehouse procedure. Incoterm cannot be evaluated", "", entryInstruction.Incoterm);
	}

	public void TestValuationCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLineInvoice1 = invoice1.InvoiceLines.AddNew();
		var invoiceLineInvoice2 = invoice2.InvoiceLines.AddNew();
		invoiceLineInvoice1.JI_CEI = ZGuid.Empty;
		invoiceLineInvoice2.JI_CEI = ZGuid.Empty;
		AssertEquals("EntryInstruction has no invoices. Valuation code cannot be evaluated", "", entryInstruction.ValuationCode);

		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice but valuation code is empty. Valuation code cannot be evaluated", "", entryInstruction.ValuationCode);

		invoice1.JZ_ValuationCode = "XX";
		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice with valuation code. Valuation code has been evaluated", "XX", entryInstruction.ValuationCode);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_ValuationCode = "XX";
		AssertEquals("EntryInstruction has 2 linked invoices with same valuation code. Valuation code has been evaluated", "XX", entryInstruction.ValuationCode);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_ValuationCode = "";
		AssertEquals("Entry Instruction has 2 linked invoices with different valuation code. Valuation code cannot be evaluated", "", entryInstruction.ValuationCode);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_ValuationCode = "ZZ";
		AssertEquals("Entry Instruction has 2 linked invoices with different valuation code. Valuation code cannot be evaluated", "", entryInstruction.ValuationCode);

		entryInstruction.CEI_Procedure = "71";
		invoice2.JZ_ValuationCode = "XX";
		AssertEquals("EntryInstruction has 2 linked invoices with same valuation code but procedure code is warehouse procedure. Valuation code cannot be evaluated", "", entryInstruction.Incoterm);
	}

	public void TestCurrency()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLineInvoice1 = invoice1.InvoiceLines.AddNew();
		var invoiceLineInvoice2 = invoice2.InvoiceLines.AddNew();
		invoiceLineInvoice1.JI_CEI = ZGuid.Empty;
		invoiceLineInvoice2.JI_CEI = ZGuid.Empty;
		AssertEquals("EntryInstruction has no invoices. Currency cannot be evaluated", "", entryInstruction.Currency);

		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice but currency is empty. Currency cannot be evaluated", "", entryInstruction.Currency);

		invoice1.JZ_RX_NKInvoice_Currency = "XXX";
		invoiceLineInvoice1.JI_CEI = entryInstruction.PK;
		AssertEquals("EntryInstruction has 1 linked invoice with currency. Currency has been evaluated", "XXX", entryInstruction.Currency);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "XXX";
		AssertEquals("EntryInstruction has 2 linked invoices with same currency. Currency has been evaluated", "XXX", entryInstruction.Currency);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "";
		AssertEquals("Entry Instruction has 2 linked invoices with different currencies. Currency cannot be evaluated", "", entryInstruction.Currency);

		invoiceLineInvoice2.JI_CEI = entryInstruction.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "ZZZ";
		AssertEquals("Entry Instruction has 2 linked invoices with different currencies. Currency cannot be evaluated", "", entryInstruction.Currency);

		entryInstruction.CEI_Procedure = "71";
		invoice2.JZ_RX_NKInvoice_Currency = "XX";
		AssertEquals("EntryInstruction has 2 linked invoices with same currency but procedure code is warehouse procedure. Currency cannot be evaluated", "", entryInstruction.Incoterm);
	}

	public void TestIncotermReadOnly()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		Assert("Incoterm readonly", entryInstruction.IncotermInfo.ReadOnly);
	}

	public void TestValuationCodeReadOnly()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		Assert("Valuation code readonly", entryInstruction.ValuationCodeInfo.ReadOnly);
	}

	public void TestCurrencyReadOnly()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		Assert("Currency readonly", entryInstruction.CurrencyInfo.ReadOnly);
	}

	public void TestWarehouseFor27()
	{
		var warehouseOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var warehouse = warehouseOrgHeader.Addresses.AddNew();
		var warehouseCustomsCode = warehouseOrgHeader.CustomsCodes.AddNew();
		warehouseCustomsCode.OK_CodeType = "CCP";
		warehouseCustomsCode.OK_RN_NKCodeCountry = "IT";
		warehouseCustomsCode.OK_CustomsRegNo = "A123456ZB";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		AssertNull("EntryInstruction -> WarehouseFor27 should be null", entryInstruction.WarehouseFor27);
		declaration.JE_MessageType = "IMP";
		entryInstruction.CEI_OA_Warehouse2 = warehouse.PK;
		AssertNotNull("EntryInstruction -> WarehouseFor27 should be not null", entryInstruction.WarehouseFor27);
		AssertEquals("EntryInstruction -> WarehouseFor27 should be", warehouse.PK, entryInstruction.WarehouseFor27.PK);
		declaration.JE_MessageType = "EXP";
		AssertNotNull("EntryInstruction -> WarehouseFor27 should be not null", entryInstruction.WarehouseFor27);
		AssertEquals("EntryInstruction -> WarehouseFor27 should be", warehouse.PK, entryInstruction.WarehouseFor27.PK);
	}

	public void TestWarehouseIDFor27Caption()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("WarehouseIDFor27 Caption", "[49] To Warehouse", DataBoundResourceStrings.GetDataForProperty(entryInstruction.WarehouseIDFor27Info).Caption);
	}

	public void TestWarehouseIDFor27()
	{
		var warehouseOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var warehouseAddress = warehouseOrgHeader.Addresses.AddNew();
		warehouseAddress.OA_RN_NKCountryCode = "IT";
		var warehouseCustomsCode = warehouseOrgHeader.CustomsCodes.AddNew();
		warehouseCustomsCode.OK_CodeType = "CCP";
		warehouseCustomsCode.OK_CustomsRegNo = "A123456ZB";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("WarehoudeIDFor27 should be empty when Entry Instruction Warehouse is empty", ZString.Empty, entryInstruction.WarehouseIDFor27);
		entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;

		declaration.JE_MessageType = "IMP";
		AssertEquals("WarehoudeIDFor27 should be empty when Warehouse Custums Code CPC is not linked to an Address (IMP)", ZString.Empty, entryInstruction.WarehouseIDFor27);

		declaration.JE_MessageType = "EXP";
		AssertEquals("WarehoudeIDFor27 should be empty when Warehouse Custums Code CPC is not linked to an Address (EXP)", ZString.Empty, entryInstruction.WarehouseIDFor27);

		warehouseCustomsCode.OK_OA_PremisesAddress = warehouseAddress.PK;

		declaration.JE_MessageType = "IMP";
		AssertEquals("WarehoudeIDFor27 value should be visible for IMP", "A123456ZB", entryInstruction.WarehouseIDFor27);

		declaration.JE_MessageType = "EXP";
		AssertEquals("WarehoudeIDFor27 value should be visible for EXP", "A123456ZB", entryInstruction.WarehouseIDFor27);
	}

	public void TestFromWarehouseCodeCaption()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("FromWarehouseCode Caption", "[49] From Warehouse", DataBoundResourceStrings.GetDataForProperty(entryInstruction.FromWarehouseCodeInfo).Caption);
	}

	public void TestAllRelatedInvoicesHaveSameIncoTermPlace()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.JZ_IncoTermPlace = "PLACE1";
		invoiceHeader2.JZ_IncoTermPlace = "PLACE2";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Incoterm Place?", false, entryInstruction1.AllRelatedInvoicesHaveSameIncoTermPlace);

		invoiceHeader1.JZ_IncoTermPlace = "PLACE2";
		AssertEquals("Do the invoices all have the same Incoterm Place?", true, entryInstruction1.AllRelatedInvoicesHaveSameIncoTermPlace);
	}

	public void TestAllRelatedInvoicesHaveSameAgreedPlaceCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader1.ZG_AgreedPlaceCode = "1";
		invoiceHeader2.ZG_AgreedPlaceCode = "2";

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		AssertEquals("Do the invoices all have the same Agreed Place Code?", false, entryInstruction1.AllRelatedInvoicesHaveSameAgreedPlaceCode);

		invoiceHeader1.ZG_AgreedPlaceCode = "2";
		AssertEquals("Do the invoices all have the same Agreed Place Code?", true, entryInstruction1.AllRelatedInvoicesHaveSameAgreedPlaceCode);
	}

	public void TestHasIntoWarehouseProcedure()
	{
		SetUpRefData();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "71";
		AssertEquals("EntryInstruction.HasIntoWarehouseProcedure should be", true, entryInstruction.HasIntoWarehouseProcedure);

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("EntryInstruction.HasIntoWarehouseProcedure should be", false, entryInstruction.HasIntoWarehouseProcedure);
	}

	public void TestHasReimportProcedure()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);

		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_Procedure = "";
		Assert("It is not an reimport procedure", !entryInstruction.HasReimportProcedure);

		testDataHelper.CreateNewRefCusProcedure("4000");
		entryInstruction.CEI_Procedure = "40";
		Assert("It is not an reimport procedure", !entryInstruction.HasReimportProcedure);

		testDataHelper.CreateNewRefCusProcedure("6100");
		entryInstruction.CEI_Procedure = "61";
		Assert("It is an reimport procedure", entryInstruction.HasReimportProcedure);
	}

	public void TestFinancialAndBankingDataLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.FinancialAndBankingDataLine1 = "FINBANKLINE1";
		entryInstruction.FinancialAndBankingDataLine2 = "FINBANKLINE2";
		Factory.Save();

		entryInstruction = new BusinessObjectFactory().Load<CusEntryInstruction>(entryInstruction.PK);

		AssertEquals("Assert FinancialAndBankingDataLine1 has been saved and loaded", "FINBANKLINE1", entryInstruction.FinancialAndBankingDataLine1);
		AssertEquals("Assert FinancialAndBankingDataLine2 has been saved and loaded", "FINBANKLINE2", entryInstruction.FinancialAndBankingDataLine2);
	}

	public void TestZG_UseDeclarationOfIntent()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Factory.Save();
		AssertNotContains("Empty AddInfo field", "UseDeclarationOfIntent", entryInstruction.CEI_AddInfo);

		entryInstruction.ZG_UseDeclarationOfIntent = true;
		Factory.Save();
		AssertContains("ZG_UseDeclarationOfIntent=true", "UseDeclarationOfIntent=Y", entryInstruction.CEI_AddInfo);

		entryInstruction.ZG_UseDeclarationOfIntent = false;
		Factory.Save();
		AssertNotContains("ZG_UseDeclarationOfIntent=false", "UseDeclarationOfIntent", entryInstruction.CEI_AddInfo);
	}

	public void TestZG_UseDeclarationOfIntentReadOnly()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("Empty Importer", entryInstruction.ZG_UseDeclarationOfIntentInfo.ReadOnly);

		declaration.JE_OH_Importer = organisationWithDOI.PK;
		Assert("Importer with DOI", !entryInstruction.ZG_UseDeclarationOfIntentInfo.ReadOnly);

		var organisationWithoutDOI = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = organisationWithoutDOI.PK;
		Assert("Importer without DOI", entryInstruction.ZG_UseDeclarationOfIntentInfo.ReadOnly);
	}

	public void TestAddCustomsDecisionSupportingDocumentChangingCustomsWarehouse()
	{
		var (customsWarehouseOrganization, importer, authorisation, _) = InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.SetupAndGetDataForDefaultingSupportingDocument(Factory, "AUTH_NUMBER", "CWP");
		InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AddDocRuleToAuthorisation(authorisation, "C122");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_OH_Importer = importer.PK;
		entryInstruction.CEI_Procedure = "71";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		invoiceLine1.JI_Procedure = "7100";
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Procedure = "7100";
		invoiceLine2.JI_CEI = entryInstruction.PK;

		invoiceLine1.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine2.SupportingDocuments.RemoveAndDeleteAll();
		entryInstruction.CEI_OA_Warehouse2 = customsWarehouseOrganization.MainAddress.PK;

		CombineAssertions("Assert Supporting Document has been added both Invoice Line", () =>
		{
			InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine1, "C122", "AUTH_NUMBER");
			InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine2, "C122", "AUTH_NUMBER");
		});
	}

	public void TestIsPreliminaryDeclarationUnderCodeA()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("When SubStyle is A, IsPreliminaryDeclarationUnderCodeA", false, entryInstruction.IsPreliminaryDeclarationUnderCodeA);

		entryInstruction.CEI_SubStyle = "";
		AssertEquals("When SubStyle is Empty, IsPreliminaryDeclarationUnderCodeA", false, entryInstruction.IsPreliminaryDeclarationUnderCodeA);

		entryInstruction.CEI_SubStyle = "D";
		AssertEquals("When SubStyle is D, IsPreliminaryDeclarationUnderCodeA", true, entryInstruction.IsPreliminaryDeclarationUnderCodeA);
	}

	public void TestIsSimplifiedDeclaration()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_Style = "COD";
		AssertEquals("When Style is COD, IsSimplifiedDeclaration", false, entryInstruction.IsSimplifiedDeclaration);

		entryInstruction.CEI_Style = "";
		AssertEquals("When Style is Empty, IsSimplifiedDeclaration", false, entryInstruction.IsSimplifiedDeclaration);

		entryInstruction.CEI_Style = "DSE";
		AssertEquals("When Style is DSE, IsSimplifiedDeclaration", true, entryInstruction.IsSimplifiedDeclaration);
	}

	[TestDate(2021, 01, 13, 0, 0, 0)]
	public void TestDefaultDateForDuty_WhenMessageTypeIsIMP()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = "IMP";
		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("When SubStyle is A, DateForDuty", new ZDateTime(2021, 01, 13), entryInstruction.CEI_DateForDuty);

		entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
		entryInstruction.CEI_SubStyle = "D";
		AssertEquals("When SubStyle is D, DateForDuty should not be defaulted", ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2021, 01, 28);
		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("When SubStyle is A but DateForDuty is not Empty. No default expected", new ZDateTime(2021, 01, 28), entryInstruction.CEI_DateForDuty);
	}

	[TestDate(2021, 01, 13, 0, 0, 0)]
	public void TestDefaultDateForDuty_WhenMessageTypeIsEXP()
	{
		var expectedDate = new ZDateTime(2021, 01, 13);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_Style = "B1";
			AssertEquals("When Declaration is Exp UCC6, CEI_DateForDuty", expectedDate, entryInstruction.CEI_DateForDuty);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_Style = "";
			AssertEquals("When Declaration is Exp UCC6 and Style is empty, CEI_DateForDuty", ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

			entryInstruction.CEI_Style = "";
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_SubStyle = "D";
			AssertEquals("When Declaration is Exp UCC6, CEI_DateForDuty", expectedDate, entryInstruction.CEI_DateForDuty);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_Style = "B1";
			AssertEquals("When Declaration is Exp but not UCC6, CEI_DateForDuty", ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

			entryInstruction.CEI_Style = "";
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			entryInstruction.CEI_SubStyle = "D";
			AssertEquals("When Declaration is Exp but not UCC6, CEI_DateForDuty", ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

			entryInstruction.CEI_SubStyle = "A";
			AssertEquals("When Declaration is Exp but not UCC6 and SubStyle = A, CEI_DateForDuty", expectedDate, entryInstruction.CEI_DateForDuty);
		}
	}

	public void TestDefaultDateForDuty_WhenNoDeclaration()
	{
		var entryInstructionWithNoDeclaration = Factory.New<CusEntryInstruction>();
		entryInstructionWithNoDeclaration.CEI_SubStyle = "D";
		entryInstructionWithNoDeclaration.CEI_Style = "B1";
		AssertEquals("When Entry Instruction has not Declaration, CEI_DateForDuty", ZDateTime.Empty, entryInstructionWithNoDeclaration.CEI_DateForDuty);
	}

	public void TestIsTriangulationOrJointDeclaration()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ZG_ParticipantType = ZString.Empty;
		Assert("Empty ZG_ParticipantType", !entryInstruction.IsTriangulationOrJointDeclaration);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		Assert("ZG_ParticipantType = BUY", !entryInstruction.IsTriangulationOrJointDeclaration);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		Assert("ZG_ParticipantType = TRG", entryInstruction.IsTriangulationOrJointDeclaration);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		Assert("ZG_ParticipantType = JTD", entryInstruction.IsTriangulationOrJointDeclaration);
	}

	public void TestIsBuyersConsol()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ZG_ParticipantType = ZString.Empty;
		AssertEquals("When ZG_ParticipantType is Empty, IsBuyersConsol", false, entryInstruction.IsBuyersConsol);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("When ZG_ParticipantType = TRG, IsBuyersConsol", false, entryInstruction.IsBuyersConsol);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertEquals("When ZG_ParticipantType = BUY, IsBuyersConsol", true, entryInstruction.IsBuyersConsol);
	}

	public void TestSetZG_ParticipantTypeResetOrDefaultZG_PreviousInvoiceCurrencyAndExchangeRate()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";

		CombineAssertions("Empty ZG_ParticipantType, reset empty", () =>
		{
			entryInstruction.ZG_ParticipantType = ZString.Empty;
			AssertEquals(ZString.Empty, entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(0m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});

		CombineAssertions("ZG_ParticipantType = TRG, default EUR", () =>
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			AssertEquals("EUR", entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(1m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});

		CombineAssertions("ZG_ParticipantType = BUY, reset empty", () =>
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			AssertEquals(ZString.Empty, entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(0m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});

		CombineAssertions("ZG_ParticipantType = JTD, default EUR", () =>
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
			AssertEquals("EUR", entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(1m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});

		CombineAssertions("Invalid ZG_ParticipantType, reset empty", () =>
		{
			entryInstruction.ZG_ParticipantType = "XXX";
			AssertEquals(ZString.Empty, entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(0m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});

		CombineAssertions("ZG_ParticipantType = JTD, no default as already entered", () =>
		{
			entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
			AssertEquals("XXX", entryInstruction.ZG_PreviousInvoiceCurrency);
			AssertEquals(0m, entryInstruction.PreviousInvoiceCurrencyExRate);
		});
	}

	public void TestSetZG_ParticipantTypeResetZG_PreviousInvoiceAmount()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ZG_PreviousInvoiceAmount = 1m;

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		AssertEquals("Empty ZG_ParticipantType, reset", 0m, entryInstruction.ZG_PreviousInvoiceAmount);

		entryInstruction.ZG_PreviousInvoiceAmount = 1m;
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("ZG_ParticipantType = TRG, no reset", 1m, entryInstruction.ZG_PreviousInvoiceAmount);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		AssertEquals("ZG_ParticipantType = JTD, no reset", 1m, entryInstruction.ZG_PreviousInvoiceAmount);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertEquals("ZG_ParticipantType = BUY, reset", 0m, entryInstruction.ZG_PreviousInvoiceAmount);

		entryInstruction.ZG_PreviousInvoiceAmount = 1m;
		entryInstruction.ZG_ParticipantType = "XXX";
		AssertEquals("Invalid ZG_ParticipantType, reset", 0m, entryInstruction.ZG_PreviousInvoiceAmount);
	}

	public void TestPreviousInvoiceCurrencyExRateReadOnly()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		Assert(entryInstruction.PreviousInvoiceCurrencyExRateInfo.ReadOnly);
	}

	public void TestSetPreviousInvoiceCurrencyExRate()
	{
		var foreignCurrency = RefCurrency.New(Factory);
		foreignCurrency.RX_Code = "XYZ";
		foreignCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8119m);

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_PreviousInvoiceCurrency = "AAA";
		AssertEquals("Invalid Currency", 0m, entryInstruction.PreviousInvoiceCurrencyExRate);

		entryInstruction.ZG_PreviousInvoiceCurrency = Core.Constants.CurrencyCodes.Italy;
		AssertEquals("Local Currency", 1m, entryInstruction.PreviousInvoiceCurrencyExRate);

		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrency.RX_Code;
		AssertEquals("Foreign Currency", 0.8119m, entryInstruction.PreviousInvoiceCurrencyExRate);
	}

	public void TestCurrencyConverter()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		CombineAssertions(() =>
		{
			AssertType<CurrencyConverterWithDataProvider>("Type", entryInstruction.CurrencyConverter);
			AssertSame("Cached", entryInstruction.CurrencyConverter, entryInstruction.CurrencyConverter);
		});
	}

	public void TestRandomProcedure()
	{
		SetUpRefData();

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_Procedure = "";
		AssertNull("When Procedure Code is empty, RandomProcedure", entryInstruction.RandomProcedure);

		entryInstruction.CEI_Procedure = "XX";
		AssertNull("When Procedure Code is invalid, RandomProcedure", entryInstruction.RandomProcedure);

		entryInstruction.CEI_Procedure = "40";
		AssertNotNull("When Procedure Code is a valid NonWarehouseProcedure, RandomProcedure", entryInstruction.RandomProcedure);
		CombineAssertions("Assert RandomProcedure", () =>
		{
			AssertEquals("ProcedureCode", "40", entryInstruction.RandomProcedure.ProcedureCode);
			AssertEquals("IsIntoWarehouse", false, entryInstruction.RandomProcedure.IsIntoWarehouse);
		});

		entryInstruction.CEI_Procedure = "71";
		AssertNotNull("When Procedure Code is a valid WarehouseProcedure, RandomProcedure", entryInstruction.RandomProcedure);
		CombineAssertions("Assert RandomProcedure", () =>
		{
			AssertEquals("ProcedureCode", "71", entryInstruction.RandomProcedure.ProcedureCode);
			AssertEquals("IsIntoWarehouse", true, entryInstruction.RandomProcedure.IsIntoWarehouse);
		});
	}

	public void TestCEI_SubStyleDefaultingWithDeclarationType_H1() => AssertCEI_SubStyleDefaultValueForDeclarationType("H1", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_H2() => AssertCEI_SubStyleDefaultValueForDeclarationType("H2", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_H3() => AssertCEI_SubStyleDefaultValueForDeclarationType("H3", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_H4() => AssertCEI_SubStyleDefaultValueForDeclarationType("H4", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_H5() => AssertCEI_SubStyleDefaultValueForDeclarationType("H5", "");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_COD() => AssertCEI_SubStyleDefaultValueForDeclarationType("COD", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_COL() => AssertCEI_SubStyleDefaultValueForDeclarationType("COL", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_DSE() => AssertCEI_SubStyleDefaultValueForDeclarationType("DSE", "A");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_I1() => AssertCEI_SubStyleDefaultValueForDeclarationType("I1", "");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_I2() => AssertCEI_SubStyleDefaultValueForDeclarationType("I2", "");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_Empty() => AssertCEI_SubStyleDefaultValueForDeclarationType("", "");

	public void TestCEI_SubStyleDefaultingWithDeclarationType_Unknown() => AssertCEI_SubStyleDefaultValueForDeclarationType("R", "");

	public void TestIElectronicFolderSupporterMembers()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var electronicFolderSupporter = (IElectronicFolderSupporter)entryInstruction;

		entryInstruction.ElectronicDocuments = false;
		AssertEquals(nameof(electronicFolderSupporter.UseElectronicFolder), false, electronicFolderSupporter.UseElectronicFolder);

		entryInstruction.ElectronicDocuments = true;
		AssertEquals(nameof(electronicFolderSupporter.UseElectronicFolder), true, electronicFolderSupporter.UseElectronicFolder);
	}

	public void TestHasIntoTemporaryExportProcedure()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		dataHelper.CreateRefCusProcedure23ForCurrentCountry();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("EntryInstruction.HasIntoTemporaryExportProcedure", false, entryInstruction.HasIntoTemporaryExportProcedure);

		entryInstruction.CEI_Procedure = "23";
		AssertEquals("EntryInstruction.HasIntoTemporaryExportProcedure", true, entryInstruction.HasIntoTemporaryExportProcedure);
	}

	public void TestHasIntoTemporaryImportProcedure()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		dataHelper.CreateRefCusProcedure53ForCurrentCountry();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("EntryInstruction.HasIntoTemporaryImportProcedure", false, entryInstruction.HasIntoTemporaryImportProcedure);

		entryInstruction.CEI_Procedure = "53";
		AssertEquals("EntryInstruction.HasIntoTemporaryImportProcedure", true, entryInstruction.HasIntoTemporaryImportProcedure);
	}

	public void TestHasIntoOutwardProcessingProcedure()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure21And22ForCurrentCountry();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("EntryInstruction.HasIntoOutwardProcessingProcedure", false, entryInstruction.HasIntoOutwardProcessingProcedure);

		entryInstruction.CEI_Procedure = "21";
		AssertEquals("EntryInstruction.HasIntoOutwardProcessingProcedure", true, entryInstruction.HasIntoOutwardProcessingProcedure);
	}

	public void TestHasIntoInwardProcessingProcedure()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure51ForCurrentCountry();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("EntryInstruction.HasIntoInwardProcessingProcedure", false, entryInstruction.HasIntoInwardProcessingProcedure);

		entryInstruction.CEI_Procedure = "51";
		AssertEquals("EntryInstruction.HasIntoInwardProcessingProcedure", true, entryInstruction.HasIntoInwardProcessingProcedure);
	}

	public void TestHasAtLeastOneAuthorizationUsage()
	{
		AssertEquals("Without any authorization usages", expected: false, entryInstruction.HasAtLeastOneAuthorizationUsage("ZZZ"));

		entryInstruction.CusAuthorizationUsages.AddNew().AGC_Code = "AAA";
		AssertEquals("With authorization usages of different code", expected: false, entryInstruction.HasAtLeastOneAuthorizationUsage("ZZZ"));

		entryInstruction.CusAuthorizationUsages.AddNew().AGC_Code = "ZZZ";
		AssertEquals("With authorization usages of matching code", expected: true, entryInstruction.HasAtLeastOneAuthorizationUsage("ZZZ"));
	}

	public void TestHasAtLeastOneSupportingDocument()
	{
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		invoiceLine2.SupportingDocuments.AddNew().CSI_Code = "ZZZ";
		AssertSame("[PRE-CONDITION] invoiceLine1 Instruction is instruction1", entryInstruction, invoiceLine1.EntryInstruction);
		AssertSame("[PRE-CONDITION] invoiceLine2 Instruction is instruction2", entryInstruction2, invoiceLine2.EntryInstruction);

		AssertEquals("Without any supporting documents", expected: false, entryInstruction.HasAtLeastOneSupportingDocument("ZZZ"));

		invoiceLine1.SupportingDocuments.AddNew().CSI_Code = "AAA";
		AssertEquals("With supporting documents of different code", expected: false, entryInstruction.HasAtLeastOneSupportingDocument("ZZZ"));

		invoiceLine1.SupportingDocuments.AddNew().CSI_Code = "ZZZ";
		AssertEquals("With supporting documents of matching code", expected: true, entryInstruction.HasAtLeastOneSupportingDocument("ZZZ"));
	}

	public void TestTempProcLimitDateRequired()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		dataHelper.CreateRefCusProcedure23ForCurrentCountry();
		dataHelper.CreateRefCusProcedure53ForCurrentCountry();
		dataHelper.CreateRefCusProcedure21And22ForCurrentCountry();
		dataHelper.CreateRefCusProcedure51ForCurrentCountry();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "40";
		AssertEquals("When ProcedureCode = 40, TempProcLimitDateRequired", false, entryInstruction.TempProcLimitDateRequired);

		entryInstruction.CEI_Procedure = "23";
		AssertEquals("When ProcedureCode = 23 (ZZ6_IntoTemporaryExport = 'Y'), TempProcLimitDateRequired", true, entryInstruction.TempProcLimitDateRequired);

		entryInstruction.CEI_Procedure = "53";
		AssertEquals("When ProcedureCode = 53 (ZZ6_IntoTemporaryImport = 'Y'), TempProcLimitDateRequired", true, entryInstruction.TempProcLimitDateRequired);

		entryInstruction.CEI_Procedure = "21";
		AssertEquals("When ProcedureCode = 21 (ZZ6_IntoOutwardProcessing = 'Y'), TempProcLimitDateRequired", true, entryInstruction.TempProcLimitDateRequired);

		entryInstruction.CEI_Procedure = "51";
		AssertEquals("When ProcedureCode = 51 (ZZ6_IntoInwardProcessing = 'Y'), TempProcLimitDateRequired", true, entryInstruction.TempProcLimitDateRequired);
	}

	public void TestExchangeRateValidation_AcceptanceDateChange()
	{
		var expectedExchangeRateErrorMessage = "This exchange rate was set when the currency was set. But the current rate for the valuation date is";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "7", "7 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var foreignCurrency = RefCurrency.New(Factory);
		foreignCurrency.RX_Code = "ABC";
		foreignCurrency.SetCustomsRate(new ZDateTime(2022, 01, 18), new ZDateTime(2022, 01, 26), 1.53m);
		foreignCurrency.SetCustomsRate(new ZDateTime(2022, 02, 01), new ZDateTime(2022, 02, 28), 1.7m);
		foreignCurrency.SetCustomsRate(new ZDateTime(2022, 03, 01), new ZDateTime(2022, 03, 31), 1.9m);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Procedure = "40";

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceNumber = "INV#1234";
		invoiceHeader.JZ_RX_NKInvoice_Currency = "ABC";
		invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
		invoiceHeader.JZ_IncoTerm = "DAP";
		invoiceHeader.JZ_IncoTermPlace = "TEST";
		invoiceHeader.ZG_AgreedPlaceCode = "1";
		invoiceHeader.JZ_ValuationCode = "7";
		invoiceHeader.Charges.RemoveAndDeleteAll();
		invoiceHeader.SupportingDocuments.AddNew().CSI_Code = "N380";
		invoiceHeader.PreviousDocuments.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_LinePrice = 1000;

		instruction.CEI_DateForDuty = new ZDateTime(2022, 01, 23);
		invoiceHeader.JZ_InvoiceAmount = 1000;

		declaration.RunPreSaveValidation();
		AssertEquals(1.53m, invoiceHeader.JZ_InvoiceCurrExRate);
		AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceCurrExRateInfo, expectedExchangeRateErrorMessage);

		instruction.CEI_DateForDuty = new ZDateTime(2022, 03, 23);
		declaration.RunPreSaveValidation();
		AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceCurrExRateInfo, expectedExchangeRateErrorMessage);
	}

	public void TestClearanceByEntryLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ClearanceByEntryLine = true;
		Factory.Save();

		entryInstruction = new BusinessObjectFactory().Load<CusEntryInstruction>(entryInstruction.PK);
		AssertEquals("ClearanceByEntryLine has been saved and loaded", true, entryInstruction.ClearanceByEntryLine);
	}

	public void TestClearanceByEntryLineCaptions()
	{
		var resourceString = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.ClearanceByEntryLine));
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Clearance by Entry Line", resourceString.Caption);
			AssertEquals("FullDescription", "Request clearance by entry line", resourceString.FullDescription);
		});
	}

	public void TestCusAuthorizationUsages()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>("CusAuthorizationUsages Type", entryInstruction.CusAuthorizationUsages);
	}

	public void TestGuarantees()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertType<GuaranteeForEntryInstructionCollection>("Guarantees Type", entryInstruction.Guarantees);
	}

	public void TestElectronicDocuments()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "IT_UseElectronicDocuments");

		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ElectronicDocuments = true;
		AssertEquals("ElectronicDocuments", true, entryInstruction.ElectronicDocuments);
		AssertNotNull("ElectronicDocuments is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));

		entryInstruction.ElectronicDocuments = false;
		AssertEquals("ElectronicDocuments", false, entryInstruction.ElectronicDocuments);
		AssertNull("ElectronicDocuments is deleted from dbo.GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
	}

	public void TestCusSupplyChainActorReferences()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nameof(entryInstruction.CusSupplyChainActorReferences), entryInstruction.CusSupplyChainActorReferences);
	}

	public void TestZG_SimplifiedDecAcceptanceDate_SetToEmptyForSubStylesOtherThanXYZ()
	{
		var dateTime = new ZDateTime(2022, 9, 2);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		UpdateEntryInstruction(entryInstruction, ITEntrySubStyleList.Codes.StandardDeclarationA, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: ZDateTime.Empty);

		UpdateEntryInstruction(entryInstruction, ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: ZDateTime.Empty);

		UpdateEntryInstruction(entryInstruction, ZString.Empty, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: ZDateTime.Empty);
	}

	public void TestZG_SimplifiedDecAcceptanceDate_SetToEmptyForStyleC1()
	{
		var dateTime = new ZDateTime(2022, 9, 2);
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			entryInstruction.CEI_SubStyle = "X";
			entryInstruction.CEI_Style = "B1";
			entryInstruction.ZG_SimplifiedDecAcceptanceDate = dateTime;
			entryInstruction.CEI_Style = "C1";
			AssertEquals("Changing CEI_Style to C1, ZG_SimplifiedDecAcceptanceDate", ZDateTime.Empty, entryInstruction.ZG_SimplifiedDecAcceptanceDate);

			entryInstruction.CEI_Style = "B1";
			entryInstruction.ZG_SimplifiedDecAcceptanceDate = dateTime;
			entryInstruction.CEI_Style = "B2";
			AssertEquals("ZG_SimplifiedDecAcceptanceDate", dateTime, entryInstruction.ZG_SimplifiedDecAcceptanceDate);
		}
	}

	public void TestZG_SimplifiedDecAcceptanceDate_DoNotSetValueToEmptyForSubStylesXYZ()
	{
		var dateTime = new ZDateTime(2022, 9, 2);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		UpdateEntryInstruction(entryInstruction, ITEntrySubStyleList.Codes.SupplementaryDeclarationX, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: dateTime);

		UpdateEntryInstruction(entryInstruction, ITEntrySubStyleList.Codes.SupplementaryDeclarationY, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: dateTime);

		UpdateEntryInstruction(entryInstruction, ITEntrySubStyleList.Codes.SupplementaryDeclarationZ, dateTime);
		AssertEquals(message: nameof(CusEntryInstruction.ZG_SimplifiedDecAcceptanceDate),
			actual: entryInstruction.ZG_SimplifiedDecAcceptanceDate,
			expected: dateTime);
	}

	public void TestCanSetSimplifiedDecAcceptanceDate_WhenDeclarationIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = "IMP";

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			false,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationX;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			true,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationY;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			true,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationZ;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			true,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.StandardDeclarationA;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			false,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);

		entryInstruction.CEI_SubStyle = ZString.Empty;
		AssertEquals($"When CEI_SubStyle = {entryInstruction.CEI_SubStyle}",
			false,
			entryInstruction.CanSetSimplifiedDecAcceptanceDate);
	}

	public void TestCanSetSimplifiedDecAcceptanceDate_WhenDeclarationIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration Is UCC6 And Is Export", () =>
			{
				entryInstruction.CEI_Style = "C1";
				entryInstruction.CEI_SubStyle = "X";
				AssertEquals("When Style is C1, CanSetSimplifiedDecAcceptanceDate", false, entryInstruction.CanSetSimplifiedDecAcceptanceDate);

				entryInstruction.CEI_Style = "B1";
				AssertEquals("When Style is not C1 and SubStyle is in (X, Y, Z), CanSetSimplifiedDecAcceptanceDate", true, entryInstruction.CanSetSimplifiedDecAcceptanceDate);

				entryInstruction.CEI_SubStyle = "A";
				AssertEquals("When Style is not C1 but SubStyle is not in (X, Y, Z), CanSetSimplifiedDecAcceptanceDate", false, entryInstruction.CanSetSimplifiedDecAcceptanceDate);

				entryInstruction.CEI_Style = "C2";
				AssertEquals("When Style is C2 and SubStyle is Any, CanSetSimplifiedDecAcceptanceDate", true, entryInstruction.CanSetSimplifiedDecAcceptanceDate);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			entryInstruction.CEI_Style = "B2";
			entryInstruction.CEI_SubStyle = "X";
			AssertEquals("When Declaration is Export but not UCC6 and Style is not C1 but SubStyle is in (X, Y, Z), CanSetSimplifiedDecAcceptanceDate", true, entryInstruction.CanSetSimplifiedDecAcceptanceDate);
		}
	}

	public void TestAmendingReadOnlyFields()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstructionNotLinked = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions("When there are no entry headers",
			() => AssertEntryFieldsReadOnlyState("Non linked Entry", entryInstructionNotLinked, expectedReadOnly: false));

		var orphanEntryInstruction = Factory.New<CusEntryInstruction>();
		CombineAssertions("When entry instruction has no declaration",
			() => AssertEntryFieldsReadOnlyState("Standalone Entry", orphanEntryInstruction, expectedReadOnly: false));

		var entryHeaderLinkedToInstruction = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();
		var entryInstructionLinkedToEntry = declaration.CustomsEntryInstructions.AddNew();
		entryHeaderLinkedToInstruction.CH_CEI_Instruction = entryInstructionLinkedToEntry.PK;

		entryHeaderLinkedToInstruction.CH_EntryStatus = "AMG";
		CombineAssertions("When linked entry status is AMG", () =>
		{
			AssertEntryFieldsReadOnlyState("Linked Entry", entryInstructionLinkedToEntry, expectedReadOnly: true);
			AssertEntryFieldsReadOnlyState("Non linked Entry", entryInstructionNotLinked, expectedReadOnly: false);
			AssertEntryFieldsReadOnlyState("Standalone Entry", orphanEntryInstruction, expectedReadOnly: false);
		});

		entryHeaderLinkedToInstruction.CH_EntryStatus = "";
		CombineAssertions("When linked entry status is empty", () =>
		{
			AssertEntryFieldsReadOnlyState("Linked Entry", entryInstructionLinkedToEntry, expectedReadOnly: false);
			AssertEntryFieldsReadOnlyState("Non linked Entry", entryInstructionNotLinked, expectedReadOnly: false);
			AssertEntryFieldsReadOnlyState("Standalone Entry", orphanEntryInstruction, expectedReadOnly: false);
		});

		entryHeaderLinkedToInstruction.CH_EntryStatus = "AMD";
		CombineAssertions("When linked entry status is AMD", () =>
		{
			AssertEntryFieldsReadOnlyState("Linked Entry", entryInstructionLinkedToEntry, expectedReadOnly: false);
			AssertEntryFieldsReadOnlyState("Non linked Entry", entryInstructionNotLinked, expectedReadOnly: false);
			AssertEntryFieldsReadOnlyState("Standalone Entry", orphanEntryInstruction, expectedReadOnly: false);
		});

		void AssertEntryFieldsReadOnlyState(string message, CusEntryInstruction entryInstruction, bool expectedReadOnly)
		{
			AssertEquals($"{message}, CEI_SubStyleInfo.ReadOnly", expectedReadOnly, entryInstruction.CEI_SubStyleInfo.ReadOnly);
			AssertEquals($"{message}, ClearanceByEntryLineInfo.ReadOnly", expectedReadOnly, entryInstruction.ClearanceByEntryLineInfo.ReadOnly);
			AssertEquals($"{message}, CEI_ProcedureInfo.ReadOnly", expectedReadOnly, entryInstruction.CEI_ProcedureInfo.ReadOnly);
		}
	}

	public void TestZG_PresentationStartDateResourceStringData()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryInstruction.ZG_PresentationStartDateInfo);

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Date/Time Pres. Goods", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Pres. Goods", resourceStringData.ShortCaption);
			AssertEquals("FullDescription", "Date and Time of Presentation of the Goods", resourceStringData.FullDescription);
		});
	}

	public void TestZG_TempProcLimitDateResourceStringData()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryInstruction.ZG_TempProcLimitDateInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Temporary Procedure Limit Date", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Limit Date", resourceStringData.ShortCaption);
			AssertEquals("MediumCaption", "Temp. Procedure Limit Date", resourceStringData.MediumCaption);
		});
	}

	public void TestAllowDutiesAndFeeCalculation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		Func<bool> isDutiesAndFeesCalculationAllowed = () => entryInstruction.IsDutiesAndFeeCalculationAllowed();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", true, isDutiesAndFeesCalculationAllowed());

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", true, isDutiesAndFeesCalculationAllowed());

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", true, isDutiesAndFeesCalculationAllowed());

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", false, isDutiesAndFeesCalculationAllowed());

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", false, isDutiesAndFeesCalculationAllowed());

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", false, isDutiesAndFeesCalculationAllowed());

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", true, isDutiesAndFeesCalculationAllowed());
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4;
			AssertEquals("IsDutiesAndFeeCalculationAllowed", true, isDutiesAndFeesCalculationAllowed());
		}
	}

	public void TestRemoveFeesIfNotAllowed()
	{
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, true, ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, 2);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, true, ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, 2);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, true, ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, 0);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, true, ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, 0);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, true, ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2, 0);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Export, false, ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2, 2);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Import, false, ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1, 2);
		SetDeclarationDataAndAssertFeesCount(EUJobMessageTypeList.Codes.Import, true, ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5, 2);

		void SetDeclarationDataAndAssertFeesCount(string declarationType, bool isUcc6, string ceiStyle, int expectedCount)
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
			{
				declaration.JE_MessageType = declarationType;
				entryInstruction.CEI_Style = ceiStyle;
				AssertEquals($"JE_MessageType={declarationType}, UCC6={isUcc6}, CEI_Style={ceiStyle}, Fees Count", expectedCount, entryLine.Fees.Count);
			}
		}
	}

	public void TestUpdateToWarehouseIdAndType()
	{
		SetupDataAndAssertWarehouseTypeAndId(warehouseIdPropInfo: i => i.ZG_ToWarehouseIDInfo,
			warehouseTypePropInfo: i => i.ZG_ToWarehouseTypeInfo,
			warehousePropInfo: i => i.CEI_OA_Warehouse2Info);
	}

	public void TestUpdateFromWarehouseIdAndType()
	{
		SetupDataAndAssertWarehouseTypeAndId(warehouseIdPropInfo: i => i.ZG_FromWarehouseIDInfo,
			warehouseTypePropInfo: i => i.ZG_FromWarehouseTypeInfo,
			warehousePropInfo: i => i.CEI_OA_WarehouseInfo);
	}

	public void TestHasIntoWarehouseProcedureOnAnyInvoiceLine()
	{
		SetUpRefData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertEquals("No Procedure Code", false, entryInstruction.HasIntoWarehouseProcedureOnAnyInvoiceLine);

		invoiceLine.JI_Procedure = "7100";
		AssertEquals("Procedure Code with IntoWarehouse=Y", true, entryInstruction.HasIntoWarehouseProcedureOnAnyInvoiceLine);

		invoiceLine.JI_Procedure = "4000";
		AssertEquals("Procedure Code with IntoWarehouse=N", false, entryInstruction.HasIntoWarehouseProcedureOnAnyInvoiceLine);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = ZString.Empty;
		invoiceLine2.JI_Procedure = "7100";
		AssertEquals("Procedure Code with IntoWarehouse=Y on InvoiceLine 2", true, entryInstruction.HasIntoWarehouseProcedureOnAnyInvoiceLine);
	}

	public void TestRemoveAuthorizationUsageRecordsWithType()
	{
		CreateCusAuthorizationUsage("DPO", "12343");
		CreateCusAuthorizationUsage("DPO", "56222");
		CreateCusAuthorizationUsage("ABC", "56222");

		AssertEquals("[PRE] Authorization Count", 3, entryInstruction.CusAuthorizationUsages.Count);

		AssertExceptionThrown<ArgumentException>(() => entryInstruction.RemoveAuthorizationUsageRecordsWithTypeIfExists(""));
		AssertExceptionThrown<ArgumentException>(() => entryInstruction.RemoveAuthorizationUsageRecordsWithTypeIfExists(null));

		entryInstruction.RemoveAuthorizationUsageRecordsWithTypeIfExists("DPO");
		AssertRecordCountAndCodeNotPresent(1, "DPO");

		CreateCusAuthorizationUsage("DPO", "878922");
		entryInstruction.RemoveAuthorizationUsageRecordsWithTypeIfExists("ABC");
		AssertRecordCountAndCodeNotPresent(1, "ABC");

		void CreateCusAuthorizationUsage(string code, string referenceNumber)
		{
			var cusAuthUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthUsage.AGC_Code = code;
			cusAuthUsage.AGC_Number = referenceNumber;
		}

		void AssertRecordCountAndCodeNotPresent(int expectedRecordCount, string code)
		{
			AssertEquals("Authorization Count", expectedRecordCount, entryInstruction.CusAuthorizationUsages.Count);
			Assert($"No record with {code} is present", entryInstruction.CusAuthorizationUsages.All(c => c.AGC_Code != code));
		}
	}

	public void TestAddAuthorizationUsage()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

		AssertExceptionThrown<ArgumentException>(() => entryInstruction.AddAuthorizationUsage("", null, null));
		AssertExceptionThrown<ArgumentException>(() => entryInstruction.AddAuthorizationUsage(null, null, null));

		var authorizationUsage = entryInstruction.AddAuthorizationUsage("DPO", orgHeader, "12343");
		AssertNotNull(authorizationUsage);
		AssertCusAuthorizationRecord("DPO", "12343", orgHeader.PK);

		authorizationUsage = entryInstruction.AddAuthorizationUsage("DPO", null, "77878");
		AssertNotNull(authorizationUsage);
		AssertCusAuthorizationRecord("DPO", "77878", ZGuid.Empty);

		authorizationUsage = entryInstruction.AddAuthorizationUsage("XYZ", orgHeader, "378276378");
		AssertNotNull(authorizationUsage);
		AssertCusAuthorizationRecord("XYZ", "378276378", orgHeader.PK);

		void AssertCusAuthorizationRecord(string code, string referenceNumber, ZGuid expectedOwnerId)
		{
			var record = entryInstruction.CusAuthorizationUsages.FirstOrDefault(r => r.AGC_Code == code && r.AGC_Number == referenceNumber);
			AssertNotNull($"Authorization Record-> Code:{code}, Reference Number:{referenceNumber}", record);
			AssertEquals("AGC_OH_Owner", expectedOwnerId, record.AGC_OH_Owner);
		}
	}

	public void TestAdditionalInfo_TypeConfiguration()
	{
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)entryInstruction).GetCusSupportingInfoTypes();
		var additionalInfosKey = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;

		CombineAssertions("AdditionalInfos configuration", () =>
		{
			AssertEquals("AdditionalInfo Key in CusSupportingInfoTypes", true, supportingInfoTypes.ContainsKey(additionalInfosKey));
			AssertEquals("AdditionalInfos Configured Type in CusSupportingInfoTypes", typeof(AdditionalInfo), supportingInfoTypes[additionalInfosKey]);
			AssertType<AdditionalInfoCollection>("New AdditionalInfos Collection type", entryInstruction.AdditionalInfos);
		});
	}

	public void TestGetAuthorisationHeaders()
	{
		var today = ZDate.Today;

		var warehouse = Factory.New<OrgHeader>();
		SetupAuthHeaderAndWarehouseAddress("WH1", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse);
		SetupAuthHeaderAndWarehouseAddress("WH2", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, warehouse);
		SetupAuthHeaderAndWarehouseAddress("WHP", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, warehouse, today.AddMonths(-4), today.AddMonths(-2));
		var entryInstruction = Factory.New<CusEntryInstruction>();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_DateForDuty = today;
			var auth = GetAuthorisationHeaderNumbers();
			AssertContainsExactElementsInAnyOrder("Authorisation Headers", new ZString[] { "WH1", "WH2" }, auth);

			entryInstruction.CEI_DateForDuty = today.AddMonths(-3);
			auth = GetAuthorisationHeaderNumbers();
			AssertContainsExactElementsInAnyOrder("Authorisation Headers", new ZString[] { "WHP" }, auth);
		});

		ZString[] GetAuthorisationHeaderNumbers()
		{
			return entryInstruction.GetAuthorisationHeaders(warehouse.MainAddress.PK)
				.Select(x => x.CPH_Number)
				.ToArray();
		}
	}

	public void TestRequireGuaranteeNumberInReference2()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = ZString.Empty;
		AssertEquals("When IMP, CEI_Style is empty", false, entryInstruction.RequireGuaranteeNumberInReference2);

		entryInstruction.CEI_Style = "H3";
		AssertEquals("When IMP, CEI_Style=H3", true, entryInstruction.RequireGuaranteeNumberInReference2);

		entryInstruction.CEI_Style = "H2";
		AssertEquals("When IMP, CEI_Style=H2", false, entryInstruction.RequireGuaranteeNumberInReference2);

		entryInstruction.CEI_Style = "H4";
		AssertEquals("When IMP, CEI_Style=H4", true, entryInstruction.RequireGuaranteeNumberInReference2);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When EXP, CEI_Style=H4", false, entryInstruction.RequireGuaranteeNumberInReference2);
	}

	public void TestSupportingDocuments()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertType<SupportingDocumentCollection>(instruction.SupportingDocuments);
	}

	public void TestPreviousDocuments()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertType<PreviousDocumentCollection>(instruction.PreviousDocuments);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)instruction;
		var supportedTypes = supportingInfoTypeSupporter.GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("Contains SupportingDocument?", true, supportedTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
			AssertEquals("SupportingDocument Type", typeof(SupportingDocument), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);

			AssertEquals("Contains PreviousDocument?", true, supportedTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));
			AssertEquals("PreviousDocument Type", typeof(PreviousDocument), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = ZDate.Today;
	}

	void SetupDataAndAssertWarehouseTypeAndId(Func<CusEntryInstruction, ZPropertyInfo> warehouseIdPropInfo, Func<CusEntryInstruction, ZPropertyInfo> warehouseTypePropInfo, Func<CusEntryInstruction, ZPropertyInfo> warehousePropInfo)
	{
		AssertWarehouseTypeAndIdForUcc6MessageType(EUJobMessageTypeList.Codes.Import);
		AssertWarehouseTypeAndIdForUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		AssertWarehouseTypeAndIdForNonUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		void AssertWarehouseTypeAndIdForUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = messageType;
				var warehouse = Factory.NewWithValidTestData<OrgHeader>();
				warehousePropInfo(entryInstruction).Value = warehouse.MainAddress.PK;

				CombineAssertions("No CusAuthHeader Records", () =>
				{
					AssertEquals("Warehouse Type", ZString.Empty, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", ZString.Empty, warehouseIdPropInfo(entryInstruction).Value);
				});

				var warehouse2 = Factory.NewWithValidTestData<OrgHeader>();
				SetupAuthHeaderAndWarehouseAddress("REF1", CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, warehouse2);
				warehousePropInfo(entryInstruction).Value = warehouse2.MainAddress.PK;

				CombineAssertions("No Matching CusAuthorizationHeader", () =>
				{
					AssertEquals("Warehouse Type", ZString.Empty, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", ZString.Empty, warehouseIdPropInfo(entryInstruction).Value);
				});

				var warehouse3 = Factory.NewWithValidTestData<OrgHeader>();
				SetupAuthHeaderAndWarehouseAddress("12345", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse3);
				warehousePropInfo(entryInstruction).Value = warehouse3.MainAddress.PK;

				CombineAssertions("With CusAuthorisationHeader", () =>
				{
					AssertEquals("Warehouse Type", WarehouseTypeList.Codes.CW1, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", "12345", warehouseIdPropInfo(entryInstruction).Value);
				});

				var warehouse4 = Factory.NewWithValidTestData<OrgHeader>();
				SetupAuthHeaderAndWarehouseAddress("12345", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse4);
				SetupAuthHeaderAndWarehouseAddress("12344", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, warehouse4);
				warehousePropInfo(entryInstruction).Value = warehouse4.MainAddress.PK;
				CombineAssertions("Multiple CusAuthorisationHeader", () =>
				{
					AssertEquals("Warehouse Type", ZString.Empty, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", ZString.Empty, warehouseIdPropInfo(entryInstruction).Value);
				});

				warehouseIdPropInfo(entryInstruction).Value = new ZString("12345");
				warehouseTypePropInfo(entryInstruction).Value = new ZString("S");
				warehousePropInfo(entryInstruction).Value = ZGuid.Empty;
				CombineAssertions("Empty WarehouseID", () =>
				{
					AssertEquals("Warehouse Type", ZString.Empty, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", ZString.Empty, warehouseIdPropInfo(entryInstruction).Value);
				});
			}
		}

		void AssertWarehouseTypeAndIdForNonUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = messageType;
				var warehouse = Factory.NewWithValidTestData<OrgHeader>();
				SetupAuthHeaderAndWarehouseAddress("12345", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse);

				warehouseIdPropInfo(entryInstruction).Value = new ZString("12345");
				warehouseTypePropInfo(entryInstruction).Value = new ZString("S");
				warehousePropInfo(entryInstruction).Value = warehouse.MainAddress.PK;
				CombineAssertions("Empty WarehouseID for EXP Declaration", () =>
				{
					AssertEquals("Warehouse Type", ZString.Empty, warehouseTypePropInfo(entryInstruction).Value);
					AssertEquals("Warehouse ID", ZString.Empty, warehouseIdPropInfo(entryInstruction).Value);
				});
			}
		}
	}

	CusAuthorisationHeader SetupAuthHeaderAndWarehouseAddress(string referenceNumber, string authHeaderType, OrgHeader warehouse, ZDate? startDate = null, ZDate? endDate = null)
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Type = authHeaderType;

		var owner = Factory.NewWithValidTestData<OrgHeader>();
		authorizationHeader.CPH_OH_PermitHolder = owner.PK;
		authorizationHeader.CPH_Number = referenceNumber;

		authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;
		authorizationHeader.CPH_StartDate = startDate ?? ZDate.Today.AddMonths(-1);
		authorizationHeader.CPH_EndDate = endDate ?? ZDate.Today.AddMonths(1);

		return authorizationHeader;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	void UpdateEntryInstruction(CusEntryInstruction cusEntryInstruction, string subStyle, ZDateTime acceptanceDate)
	{
		cusEntryInstruction.ZG_SimplifiedDecAcceptanceDate = acceptanceDate;
		cusEntryInstruction.CEI_SubStyle = subStyle;
	}

	void SetUpRefData()
	{
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure40And71ForCurrentCountry();
	}

	void AssertCEI_SubStyleDefaultValueForDeclarationType(string declarationType, string expectedDefaultSubStyle)
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_Style = declarationType;
		AssertEquals($"When Declaration type = {declarationType}", expectedDefaultSubStyle, entryInstruction.CEI_SubStyle);
	}
}
