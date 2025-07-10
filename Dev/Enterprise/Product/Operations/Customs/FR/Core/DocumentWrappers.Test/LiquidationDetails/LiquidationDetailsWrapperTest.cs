using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails.Testing;

sealed class LiquidationDetailsWrapperTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return LiquidationDetailsWrapper.New(entryHeader, Factory);
	}

	public void TestProperties()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping("FR");
		var rateType = helper.CreateCusRateType("FR", Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
		helper.CreateCusRateCode(Factory, "DTY", rateType.PK);
		var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "EUIAT");
		var cusCode = helper.CreateCusCodeList("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "VCL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListAttribute(cusCode.PK, Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentOutEU, "10");
		helper.CreateCusCodeListAttribute(cusCode.PK, Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentInEu, "20");
		helper.CreateCusCodeListAttribute(cusCode.PK, Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentDomestic, "30");
		var helper1 = new UniversalReferenceTestDataHelper(Factory);
		helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
		helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, Customs.Business.YesNoList.Codes.Yes);
		helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "1001", "AI2 avec dispense de visa (article 275 CGI) - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, Customs.Business.YesNoList.Codes.Yes);
		helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, Customs.Business.YesNoList.Codes.Yes);
		Factory.Save();

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "NJG";
		declarant.OH_FullName = "Declarant";
		SetupAccount(declarant, OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "466CAA0F");

		var declarantAddress = declarant.Addresses.MainAddress;
		declarantAddress.AddressCode = "TestMatchAddress";
		declarantAddress.Address1 = "TestMatchAddress";
		declarantAddress.City = "Shanghai";
		declarantAddress.State = "Shanghai";
		declarantAddress.OA_RN_NKCountryCode = "CN";

		var guarantee = DocSADHTest.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, "ADD", "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
		var supplier = Factory.New<OrgHeader>();
		var importer = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_SystemCreateTimeUtc = new ZDateTime(2021, 8, 6, 0, 0, 0);
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

		supplier.OH_Code = "NJG1";
		supplier.OH_FullName = "Supplier";

		var supplierAddress = supplier.Addresses.MainAddress;
		supplierAddress.AddressCode = "SupplierAddress";
		supplierAddress.Address1 = "SupplierAddress1";
		supplierAddress.City = "Paris";
		supplierAddress.State = "Ile-de-France";
		supplierAddress.OA_RN_NKCountryCode = "FR";

		importer.OH_Code = "NJG2";
		importer.OH_FullName = "Importer";

		var importerAddress = importer.Addresses.MainAddress;
		importerAddress.AddressCode = "ImporterAddress";
		importerAddress.Address1 = "ImporterAddress1";
		importerAddress.City = "New York";
		importerAddress.State = "State of New York";
		importerAddress.OA_RN_NKCountryCode = "US";

		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "000000";
		entryInstruction.CEI_SubStyle = "13";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var charge1_1 = invoiceHeader1.Charges.AddNew();
		charge1_1.J7_ChargeType = "ADD";
		charge1_1.J7_Amount = 1m;
		charge1_1.J7_RX_NKCurrency = "EUR";
		charge1_1.J7_IsDutiable = true;
		charge1_1.J7_IsGSTApplicable = true;
		charge1_1.J7_IsStatisticalValueApplicable = true;
		charge1_1.J7_IsIncludedInITOT = false;

		var charge1_2 = invoiceHeader1.Charges.AddNew();
		charge1_2.J7_ChargeType = "ADV";
		charge1_2.J7_Amount = 2m;
		charge1_2.J7_RX_NKCurrency = "EUR";
		charge1_2.J7_IsDutiable = true;
		charge1_2.J7_IsGSTApplicable = true;
		charge1_2.J7_IsStatisticalValueApplicable = true;
		charge1_2.J7_IsIncludedInITOT = true;

		var grpcharge1_1 = invoiceHeader1.GroupCharges.AddNew();
		grpcharge1_1.J7_ChargeType = "CFE";
		grpcharge1_1.J7_Amount = 3m;
		grpcharge1_1.J7_RX_NKCurrency = "EUR";
		grpcharge1_1.J7_IsDutiable = true;
		grpcharge1_1.J7_IsGSTApplicable = true;
		grpcharge1_1.J7_IsStatisticalValueApplicable = true;
		grpcharge1_1.J7_IsIncludedInITOT = false;

		var grpcharge1_2 = invoiceHeader1.GroupCharges.AddNew();
		grpcharge1_2.J7_ChargeType = "CNE";
		grpcharge1_2.J7_Amount = 4m;
		grpcharge1_2.J7_RX_NKCurrency = "EUR";
		grpcharge1_2.J7_IsDutiable = true;
		grpcharge1_2.J7_IsGSTApplicable = true;
		grpcharge1_2.J7_IsStatisticalValueApplicable = true;
		grpcharge1_2.J7_IsIncludedInITOT = true;

		var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
		var entryLine1 = Factory.New<CusEntryLineForTest>();
		entryHeader.MergedLines.Add(entryLine1);
		entryHeader.AllEntryLines.Add(entryLine1);
		entryLine1.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var charge2_1 = invoiceHeader2.Charges.AddNew();
		charge2_1.J7_ChargeType = "ADD";
		charge2_1.J7_Amount = 5m;
		charge2_1.J7_RX_NKCurrency = "EUR";
		charge2_1.J7_IsDutiable = true;
		charge2_1.J7_IsGSTApplicable = true;
		charge2_1.J7_IsStatisticalValueApplicable = true;
		charge2_1.J7_IsIncludedInITOT = false;

		var charge2_2 = invoiceHeader2.Charges.AddNew();
		charge2_2.J7_ChargeType = "ADV";
		charge2_2.J7_Amount = 6m;
		charge2_2.J7_RX_NKCurrency = "EUR";
		charge2_2.J7_IsDutiable = true;
		charge2_2.J7_IsGSTApplicable = true;
		charge2_2.J7_IsStatisticalValueApplicable = true;
		charge2_2.J7_IsIncludedInITOT = true;

		var grpcharge2_1 = invoiceHeader2.GroupCharges.AddNew();
		grpcharge2_1.J7_ChargeType = "CFE";
		grpcharge2_1.J7_Amount = 7m;
		grpcharge2_1.J7_RX_NKCurrency = "EUR";
		grpcharge2_1.J7_IsDutiable = true;
		grpcharge2_1.J7_IsGSTApplicable = true;
		grpcharge2_1.J7_IsStatisticalValueApplicable = true;
		grpcharge2_1.J7_IsIncludedInITOT = false;

		var grpcharge2_2 = invoiceHeader2.GroupCharges.AddNew();
		grpcharge2_2.J7_ChargeType = "CNE";
		grpcharge2_2.J7_Amount = 8m;
		grpcharge2_2.J7_RX_NKCurrency = "EUR";
		grpcharge2_2.J7_IsDutiable = true;
		grpcharge2_2.J7_IsGSTApplicable = true;
		grpcharge2_2.J7_IsStatisticalValueApplicable = true;
		grpcharge2_2.J7_IsIncludedInITOT = true;

		var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
		var entryLine2 = Factory.New<CusEntryLineForTest>();
		entryHeader.MergedLines.Add(entryLine2);
		entryHeader.AllEntryLines.Add(entryLine2);
		invoiceLine2.JI_CL = entryLine2.PK;

		var wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);

		entryHeader.CH_BGMReference = "CH123456";
		AssertEquals("CH123456", wrapper.EntryReference);

		entryHeader.EntryNumber = "Entry1";
		AssertEquals("Entry1", wrapper.DeltaReference);

		AssertEquals(ZDateTime.Empty, wrapper.EntryDate);

		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertEquals(ZDateTime.Today, wrapper.EntryDate);

		entryHeader.CH_EntryStatus = EntryStatusList.Codes.NotSent;
		AssertEquals("Not Sent", wrapper.EntryStatus);

		AssertEquals(@"Supplier
SupplierAddress1

Paris Ile-de-France
France", wrapper.SupplierAddress);
		AssertEquals(@"Importer
ImporterAddress1

New York State of New York
United States", wrapper.ImporterAddress);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Importer = orgHeader.PK;
		entryHeader.Importer.E2_OA_Address = orgHeader.MainAddress.PK;
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
		orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
		AssertEquals("FR12345678900001", wrapper.ImporterEori);

		AssertEquals(@"Declarant
TestMatchAddress

Shanghai Shanghai
China", wrapper.DeclarantAddress);

		declaration.JE_DeclarantType = "SEL";
		AssertEquals("Self Representation", wrapper.RepresentationType);

		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		AssertEquals("G1", wrapper.DeltaMode);

		declaration.JE_CustomsProfile = "Profile1";
		AssertEquals("Profile1", wrapper.AgreementNumber);

		declaration.JE_DefermentAccountNumber = "Defer1";
		AssertEquals("Defer1", wrapper.DefermentNumber);

		declaration.JE_DeclarantType = Enterprise.Customs.FR.Business.RepresentationTypeList.Codes.SEL;
		declaration.JE_CustomsProfile = "DGE001";
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		AssertEquals("DGUA/REFA/NJG", wrapper.CodNumber);

		declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
		AssertEquals("Auto-liquidation (ATVAI - postponed accounting for import VAT)", wrapper.VatProcedure);

		declaration.JE_TransportMode = "AIR";
		AssertEquals("AIR", wrapper.TransportMode);

		declaration.JE_RL_NKPortOfLoading = "FR123";
		AssertEquals("FR123", wrapper.OriginPort);

		entryInstruction.ZG_BypassCode = "1";
		AssertEquals(" / 1", wrapper.ValuationBypass);

		declaration.JE_AirRouteType = AirRouteTypeList.Codes._1_No;
		AssertEquals("1", wrapper.AirRouteType);

		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKPortOfLoading = "VNDQT";
		AssertEquals(10m, wrapper.FreightChargeFractionOutsideEU);
		AssertEquals(20m, wrapper.FreightChargeFractionInsideEU);
		AssertEquals(30m, wrapper.FreightChargeFractionInFrance);

		declaration.JE_EntryStyle = EU.Business.EntryStyleListExport.Codes.ExportNormal;
		invoiceLine1.JI_Procedure = "4000456";
		AssertEquals("000000 (EXF)", wrapper.EntryTypeFriendlyName);
		AssertEquals("2", wrapper.TotalItems);

		declaration.SupportingDocuments.RemoveAndDeleteAll();
		var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Code = "0001";
		supportingDocument1.CSI_ReferenceNumber = "1111";
		supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 09, 01);
		supportingDocument1.CSI_Value = 10;
		supportingDocument1.CSI_Quantity3 = 1;

		var supportingDocument2 = invoiceLine2.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Code = "0002";
		supportingDocument2.CSI_ReferenceNumber = "2222";
		supportingDocument2.CSI_DateOfIssue = new ZDateTime(2021, 09, 02);

		var supportingDocument3 = invoiceLine2.SupportingDocuments.AddNew();
		supportingDocument3.CSI_Code = "";
		supportingDocument3.CSI_ReferenceNumber = "00022222";
		supportingDocument3.CSI_DateOfIssue = new ZDateTime(2021, 09, 02);
		supportingDocument3.CSI_Value = 30;

		AssertEquals(@"0001  1111                       01/09/21  10
0002  2222                       02/09/21  0
      00022222                   02/09/21  0
", wrapper.SupportingDocuments);

		var supportingDocument4 = declaration.SupportingDocuments.AddNew();
		supportingDocument4.CSI_Code = "1001";
		supportingDocument1.CSI_ReferenceNumber = "1111";
		supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 09, 01);
		supportingDocument4.CSI_DateOfIssue = new ZDateTime(2021, 09, 01);
		supportingDocument4.CSI_Value = 100;
		supportingDocument4.CSI_Quantity3 = 1;
		AssertEquals("Prerequisite to next assertion: Document set at header gets its value set to merged document attached first entry line with number = 1", 110m, entryHeader.TotalD48Amount);
		AssertEquals("110 EUR", wrapper.D48Amount);

		entryHeader.EntryNumber = "Entry1";
		entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
		entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
		entryLine2.CL_InvoiceAmount = entryLine2.GetInvoicedDocumentaryAmountCore();
		entryLine2.CL_RX_NKInvoiceAmountCurrency = entryLine2.GetInvoicedDocumentaryAmountCurrencyCore();
		CombineAssertions(() =>
		{
			AssertEquals(ZDecimal.Zero, wrapper.InvoiceValueInDeclarationCurrency);
			AssertEquals(ZDecimal.Zero, wrapper.InvoiceValueInDeclarationCurrencyInEuro);
			AssertEquals(ZDecimal.Zero, wrapper.CustomsValue);
			AssertEquals(ZDecimal.Zero, wrapper.StatisticalValue);
			AssertEquals(ZDecimal.Zero, wrapper.VatableValueWithoutDuties);
			AssertEquals(ZDecimal.Zero, wrapper.VatableValue);
			AssertEquals("Y", wrapper.ShowOnlyDeltaCalculationsAsString);
			AssertEquals("DELTA Amount in €", wrapper.SummaryValuationCaption);
			AssertEquals("Detailed DELTA taxes amount", wrapper.SummaryTaxationCaption);
		});

		invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
		invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
		invoiceHeader1.JZ_InvoiceCurrExRate = 10m;
		invoiceHeader1.JZ_InvoiceCurrExRate = 10m;

		invoiceLine1.JI_LinePrice = 10m;
		invoiceLine1.JI_Weight = 10m;
		invoiceLine1.JI_WeightUQ = "KG";

		invoiceLine1.JI_NetWeight = 20m;
		invoiceLine1.JI_NetWeightUQ = "KG";

		invoiceLine2.JI_LinePrice = 20m;
		invoiceLine2.JI_Weight = 10m;
		invoiceLine2.JI_WeightUQ = "KG";

		invoiceLine2.JI_NetWeight = 20m;
		invoiceLine2.JI_NetWeightUQ = "KG";

		entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
		entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
		entryLine2.CL_InvoiceAmount = entryLine2.GetInvoicedDocumentaryAmountCore();
		entryLine2.CL_RX_NKInvoiceAmountCurrency = entryLine2.GetInvoicedDocumentaryAmountCurrencyCore();
		wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		CombineAssertions(() =>
		{
			AssertEquals(30m, wrapper.InvoiceValueInDeclarationCurrency);
			AssertEquals("CNY", wrapper.InvoiceValueInInvoiceCurrency);
			AssertEquals(17.96m, wrapper.InvoiceValueInDeclarationCurrencyInEuro);
			AssertEquals(10m, wrapper.InvoiceCurrencyExchangeRate);
			AssertEquals(20m, wrapper.TotalGrossWeightInKg);
			AssertEquals(40m, wrapper.TotalNetWeightInKg);
		});

		entryHeader.EntryNumber = ZString.Empty;
		AssertEquals("Global calculated amount in €", wrapper.SummaryValuationCaption);
		AssertEquals("Detailed calculated taxes amount", wrapper.SummaryTaxationCaption);
		AssertEquals("N", wrapper.ShowOnlyDeltaCalculationsAsString);

		invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
		invoiceHeader1.JZ_InvoiceCurrExRate = 10m;
		invoiceHeader1.JZ_InvoiceCurrExRate = 10m;

		entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
		entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
		entryLine2.CL_InvoiceAmount = entryLine2.GetInvoicedDocumentaryAmountCore();
		entryLine2.CL_RX_NKInvoiceAmountCurrency = entryLine2.GetInvoicedDocumentaryAmountCurrencyCore();
		wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals(19.17m, wrapper.InvoiceValueInDeclarationCurrency);
		AssertEquals(19.17m, wrapper.InvoiceValueInDeclarationCurrencyInEuro);
		AssertEquals("EUR", wrapper.InvoiceValueInInvoiceCurrency);
		AssertEquals(1m, wrapper.InvoiceCurrencyExchangeRate);
		AssertEquals(20m, wrapper.TotalGrossWeightInKg);
		AssertEquals(40m, wrapper.TotalNetWeightInKg);

		var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package1.CW_PackQty = 2;
		package1.CW_PackType = "AA";
		package1.CW_MarksAndNos = "AAAAAAAAAAAA";

		var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 2;
		packing1.PackagePk = package1.PK;
		wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals("2 AA", wrapper.NumberAndTypeOfPackage);

		var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package2.CW_PackQty = 2;
		package2.CW_PackType = "PK";
		package2.CW_MarksAndNos = "AAABBAAAAAAA";

		var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
		packing2.IsLinked = true;
		packing2.PackQty = 2;
		packing2.PackagePk = package2.PK;
		wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals("4 MLT", wrapper.NumberAndTypeOfPackage);

		invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
		entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
		entryLine2.CL_InvoiceAmount = entryLine2.GetInvoicedDocumentaryAmountCore();
		entryLine2.CL_RX_NKInvoiceAmountCurrency = entryLine2.GetInvoicedDocumentaryAmountCurrencyCore();
		wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals(30m, wrapper.InvoiceValueInDeclarationCurrency);
		AssertEquals("USD", wrapper.InvoiceValueInInvoiceCurrency);
		AssertEquals(10m, wrapper.InvoiceCurrencyExchangeRate);

		entryLine1.CL_CustomsValue = 10m;
		entryLine2.CL_CustomsValue = 20m;
		AssertEquals(30m, wrapper.CustomsValue);

		entryLine1.CL_StatisticalValue = 10m;
		entryLine2.CL_StatisticalValue = 20m;
		AssertEquals(30m, wrapper.StatisticalValue);

		entryLine1.CL_ValueForVAT = 10m;
		entryLine2.CL_ValueForVAT = 20m;
		entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1m);
		entryLine2.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 2m);
		AssertEquals(27m, wrapper.VatableValueWithoutDuties);
		AssertEquals(30m, wrapper.VatableValue);

		entryHeader.EntryNumber = "Entry1";
		CombineAssertions(() =>
		{
			AssertEquals(30m, wrapper.CustomsValue);
			AssertEquals(30m, wrapper.StatisticalValue);
			AssertEquals(27m, wrapper.VatableValueWithoutDuties);
			AssertEquals(30m, wrapper.VatableValue);
		});

		entryHeader.EntryNumber = ZString.Empty;

		invoiceHeader1.JZ_IncoTerm = "IT1";
		invoiceHeader2.JZ_IncoTerm = "IT1";
		AssertEquals("IT1", wrapper.IncoTerm);

		invoiceHeader1.JZ_IncoTermPlace = "place";
		invoiceHeader2.JZ_IncoTermPlace = "place";
		AssertEquals("place", wrapper.IncoTermPlace);

		invoiceHeader1.ZG_AgreedPlaceCode = "1";
		invoiceHeader2.ZG_AgreedPlaceCode = "1";
		AssertEquals("1", wrapper.IncoTermType);

		AssertEquals(@"
Code  Amount            Currency  Dutiable  TVA Apply  Statable  Incl. In Line
ADD   6                 EUR       Y         Y          Y         N
ADV   8                 EUR       Y         Y          Y         N
CFE   10                EUR       Y         Y          Y         N
CNE   12                EUR       Y         Y          Y         Y
", wrapper.ChargeBreakdown);

		AssertEquals(false, wrapper.AnyLineWithTariffBypass);

		invoiceLine1.JI_TariffBypassCode = "1";
		AssertEquals(true, wrapper.AnyLineWithTariffBypass);

		var charge = entryHeader.ConfirmedCharges.AddOrUpdate("C11");
		charge.C1_ChargeAmount = 10m;
		charge.C1_MethodOfPayment = "5";
		var fee22 = entryLine1.ConfirmedFees.AddNew();
		fee22.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		fee22.CF_ChargeAmount = 1m;
		fee22.G4_BaseAmount = 1m;
		fee22.G4_RateDuty = "%";
		fee22.G4_RateSuspension = "1";
		fee22.NationalFeeTypeCode = "A325";

		var fee23 = entryLine1.ConfirmedFees.AddNew();
		fee23.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
		fee23.CF_ChargeAmount = 2m;
		fee23.G4_BaseAmount = 2m;
		fee23.G4_RateDuty = "%";
		fee23.G4_RateSuspension = "2";
		fee23.NationalFeeTypeCode = "A325";

		var fee3 = entryLine1.ConfirmedFees.AddNew();
		fee3.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;
		fee3.CF_ChargeAmount = 3m;
		fee3.G4_BaseAmount = 3m;
		fee3.G4_RateDuty = "%";
		fee3.G4_RateSuspension = "3";
		fee3.NationalFeeTypeCode = "A336";

		var fee4 = entryLine1.ConfirmedFees.AddNew();
		fee4.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.TotalAmountPayable;
		fee4.CF_ChargeAmount = 4m;
		fee4.G4_BaseAmount = 4m;
		fee4.G4_RateDuty = "002";
		fee4.G4_RateSuspension = "4";
		fee4.NationalFeeTypeCode = "A365";

		var fee5 = entryLine2.ConfirmedFees.AddNew();
		fee5.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		fee5.CF_ChargeAmount = 5m;
		fee5.G4_BaseAmount = 5m;
		fee5.G4_RateDuty = "%";
		fee5.G4_RateSuspension = "5";
		fee5.NationalFeeTypeCode = "A325";

		AssertEquals(@"
Tax code  Base Amount     Tax rate      Total amount
C11                                     10
          0                             3
A325      8                             8
A336      3                             3
A365      4                             4
", wrapper.TaxBreakdown);

		var fee11 = entryLine1.ConfirmedFees.AddNew();
		fee11.CF_ChargeAmount = 1.1m;
		fee11.CF_MethodOfPayment = "5";
		var fee12 = entryLine1.ConfirmedFees.AddNew();
		fee12.CF_ChargeAmount = 2.1m;
		fee12.CF_MethodOfPayment = "";

		var fee13 = entryLine1.ConfirmedFees.AddNew();
		fee13.CF_ChargeAmount = 3.1m;
		fee13.CF_MethodOfPayment = "1";

		var fee14 = entryLine1.ConfirmedFees.AddNew();
		fee14.CF_ChargeAmount = 4.1m;
		fee14.CF_MethodOfPayment = "2";

		var fee21 = entryLine1.ConfirmedFees.AddNew();
		fee21.CF_ChargeAmount = 5.1m;
		fee21.CF_MethodOfPayment = "3";

		var fee25 = entryLine1.ConfirmedFees.AddNew();
		fee25.CF_ChargeAmount = 6.1m;
		fee25.CF_MethodOfPayment = "6";

		var fee26 = entryLine1.ConfirmedFees.AddNew();
		fee26.CF_ChargeAmount = 7.1m;
		fee26.CF_MethodOfPayment = "5";

		var fee27 = entryLine1.ConfirmedFees.AddNew();
		fee27.CF_ChargeAmount = 8.1m;
		fee27.CF_MethodOfPayment = "1";

		var fee28 = entryLine1.ConfirmedFees.AddNew();
		fee28.CF_ChargeAmount = 9.1m;
		fee28.CF_MethodOfPayment = "4"; // COD

		AssertEquals(15m, wrapper.TotalFeeAmount);  // 11 + 4 = 15
		AssertEquals(28m, wrapper.TotalFeesGuaranteedAmount);
		AssertEquals(11m, wrapper.TotalFeesCautionAmount);
		AssertEquals(4m, wrapper.TotalFeesNonCautionAmount);
		AssertEquals(5m, wrapper.TotalFeesAi2Amount);
		AssertEquals(6m, wrapper.TotalFeesAtvaiAmount);
		AssertEquals(9m, wrapper.TotalFeesCodAmount);
		AssertEquals(8m, wrapper.TotalFeesNonPercuesAmount);

		AssertEquals("15", wrapper.TotalFeeAmountString);  // 11 + 4 = 15
		AssertEquals("28", wrapper.TotalFeesGuaranteedAmountString);
		AssertEquals("11", wrapper.TotalFeesCautionAmountString);
		AssertEquals("4", wrapper.TotalFeesNonCautionAmountString);
		AssertEquals("5", wrapper.TotalFeesAi2AmountString);
		AssertEquals("6", wrapper.TotalFeesAtvaiAmountString);
		AssertEquals("9", wrapper.TotalFeesCodAmountString);
		AssertEquals("8", wrapper.TotalFeesNonPercuesAmountString);
	}

	public void TestTotalGrossWeightInKg_NoException()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_Weight = 10m;
		invoiceLine.JI_WeightUQ = "";

		var wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals("empty uint", false, entryLine.EffectiveGrossWeight.IsValid);
		AssertEquals("No Exception", 0m, wrapper.TotalGrossWeightInKg);
	}

	public void TestTotalNetWeightInKg_NoException()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_NetWeight = 20m;
		invoiceLine.JI_NetWeightUQ = "";

		var wrapper = LiquidationDetailsWrapper.New(entryHeader, Factory);
		AssertEquals("empty uint", false, entryLine.EffectiveNetWeight.IsValid);
		AssertEquals("No Exception", 0m, wrapper.TotalNetWeightInKg);
	}

	public static OrgCusAccount SetupAccount(OrgHeader header, ZString code, ZString type, ZString account, ZString issuer, ZString reportingPeriod, ZString representativeId)
	{
		var orgCusAccount = header.Factory.New<OrgCusAccount>();
		orgCusAccount.CZ_Code = code;
		orgCusAccount.CZ_Type = type;
		orgCusAccount.CZ_OH = header.PK;
		orgCusAccount.CZ_Account = account;
		orgCusAccount.CZ_Issuer = issuer;
		orgCusAccount.CZ_ReportingPeriod = reportingPeriod;
		orgCusAccount.CZ_RepresentativeID = representativeId;
		orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		return orgCusAccount;
	}

	sealed class CusEntryLineForTest : CusEntryLine
	{
		public CusEntryLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ZDecimal GetInvoicedDocumentaryAmountCore() => base.GetInvoicedDocumentaryAmountCore();

		public new ZString GetInvoicedDocumentaryAmountCurrencyCore() => base.GetInvoicedDocumentaryAmountCurrencyCore();
	}
}
