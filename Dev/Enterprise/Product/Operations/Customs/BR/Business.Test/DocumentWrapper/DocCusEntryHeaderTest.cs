using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		public void TestNetWeight()
		{
			var invoiceLines = entryHeader.Declaration.InvoiceLines.AddNew();
			invoiceLines.JI_NetWeight = 10m;
			invoiceLines.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLines.JI_CL = entryLine.PK;

			AssertEquals("NetWeight should be", 10m, entryHeaderWrapper.NetWeight);
		}

		public void TestEntranceOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			var brCode = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "12345", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(brCode, "PT", "Teste1");
			Factory.Save();

			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.Declaration.EntranceOfficeCode = "12345";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			AssertEquals("12345 - Teste1", entryHeaderWrapper.EntranceOffice.CodeAndDescription);
		}

		public void TestUCRNumber()
		{
			entryHeader.UniqueConsignmentReference = "111111";
			AssertEquals("111111", entryHeaderWrapper.UCRNumber);
		}

		public void TestTypeOfOperationExportDescription()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarantType = "1";

			AssertEquals("TypeOfOperationExport should be ", "Por conta própria", entryHeaderWrapper.TypeOfOperationExport.Description);
		}

		public void TestBoardingOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			var brCode = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "111111", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(brCode, "PT", "Teste1");
			Factory.Save();

			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.Declaration.BoardingOfficeCode = "111111";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			AssertEquals("111111 - Teste1", entryHeaderWrapper.BoardingOffice.CodeAndDescription);
		}

		public void TestIsConsortedExport()
		{
			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();

			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			cusEntryInstruction.CEI_IsConsortedExport = true;
			AssertEquals(true, entryHeaderWrapper.IsConsortedExport);

			cusEntryInstruction.CEI_IsConsortedExport = false;
			AssertEquals(false, entryHeaderWrapper.IsConsortedExport);
		}

		public void TestBoardingOfficeIsCustomsEnclosure()
		{
			entryHeader.Declaration.BoardingOfficeIsCustomsEnclosure = true;
			AssertEquals(true, entryHeaderWrapper.BoardingOfficeIsCustomsEnclosure);

			entryHeader.Declaration.BoardingOfficeIsCustomsEnclosure = false;
			AssertEquals(false, entryHeaderWrapper.BoardingOfficeIsCustomsEnclosure);
		}

		public void TestClearanceOfficeIsCustomsEnclosure()
		{
			entryHeader.Declaration.ClearanceOfficeIsCustomsEnclosure = true;
			AssertEquals(true, entryHeaderWrapper.ClearanceOfficeIsCustomsEnclosure);

			entryHeader.Declaration.ClearanceOfficeIsCustomsEnclosure = false;
			AssertEquals(false, entryHeaderWrapper.ClearanceOfficeIsCustomsEnclosure);
		}

		public void TestClearanceOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			var brCode = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "222222", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(brCode, "PT", "Teste1");
			Factory.Save();

			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.Declaration.JE_CustomsOffice = "222222";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			AssertEquals("222222 - Teste1", entryHeaderWrapper.ClearanceOffice.CodeAndDescription);
		}

		public void TestClearanceEnclosure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Enclosure Code");
			var brCode = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "000001", "Customs Enclosure", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(brCode, "PT", "Recinto Alfandegário");
			Factory.Save();

			entryHeader.Declaration.JE_LocationOfGoods = "000001";
			AssertEquals("000001 - Recinto Alfandegário", entryHeaderWrapper.ClearanceEnclosure.CodeAndDescription);
		}

		public void TestClearanceLocalInvolvedParty()
		{
			var docAddress = entryHeader.Declaration.ClearanceLocalInvolvedParty;
			docAddress.DocAddressType = DocAddressType.ClearanceLocalInvolvedParty;
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_Address1 = "Address1";
			docAddress.E2_Address2 = "Address2";
			docAddress.E2_Longitude = 5m;
			docAddress.E2_Latitude = 10m;
			docAddress.E2_GovRegNum = "97442770000126";

			AssertEquals("Address1", entryHeaderWrapper.ClearanceLocalInvolvedParty.E2_Address1);
			AssertEquals("Address2", entryHeaderWrapper.ClearanceLocalInvolvedParty.E2_Address2);
			AssertEquals(5m, entryHeaderWrapper.ClearanceLocalInvolvedParty.E2_Longitude);
			AssertEquals(10m, entryHeaderWrapper.ClearanceLocalInvolvedParty.E2_Latitude);
			AssertEquals("97442770000126", entryHeaderWrapper.ClearanceLocalInvolvedParty.E2_GovRegNum);
		}

		public void TestClearanceOfficeIsHomeDispatch()
		{
			entryHeader.Declaration.ClearanceOfficeIsHomeDispatch = true;
			AssertEquals(true, entryHeaderWrapper.ClearanceOfficeIsHomeDispatch);

			entryHeader.Declaration.ClearanceOfficeIsHomeDispatch = false;
			AssertEquals(false, entryHeaderWrapper.ClearanceOfficeIsHomeDispatch);
		}

		public void TestBoardingLocalAddress()
		{
			var oDocAddress = entryHeader.Declaration.BoardingLocalAddress;
			oDocAddress.DocAddressType = DocAddressType.BoardingLocalDocumentaryAddress;
			oDocAddress.E2_AddressOverride = ZBool.True;
			oDocAddress.E2_Address1 = "Address1";
			oDocAddress.E2_Address2 = "Address2";
			oDocAddress.E2_CompanyName = "Company Name";

			AssertEquals("Address1", entryHeaderWrapper.BoardingLocalAddress.E2_Address1);
			AssertEquals("Address2", entryHeaderWrapper.BoardingLocalAddress.E2_Address2);
			AssertEquals("Company Name", entryHeaderWrapper.BoardingLocalAddress.E2_CompanyName);
		}

		public void TestSpecialCustomsClearanceDescription()
		{
			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_SpecialCustomsClearance = "2002";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			AssertEquals("SpecialCustomsClearance should be ", "Embarque antecipado", entryHeaderWrapper.SpecialCustomsClearance.Description);
		}

		public void TestLegalDocumentDescription()
		{
			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_LegalDocument = "SNF";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			AssertEquals("LegalDocument should be ", "No Invoice", entryHeaderWrapper.LegalDocument.Description);
		}

		public void TestCargoArrivalDocumentType()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_CargoArrivalDocumentType = "3";

			AssertEquals("CargoArrivalDocumentType should be ", "DTA", entryHeaderWrapper.CargoArrivalDocumentType.Description);
		}

		public void TestNetWeightUQ()
		{
			AssertEquals("NetWeightUQ should be ", Enterprise.Core.Constants.Weight.Kilograms, entryHeaderWrapper.NetWeightUQ);
		}

		public void TestGrossWeightUQ()
		{
			AssertEquals("GrossWeightUQ should be ", Enterprise.Core.Constants.Weight.Kilograms, entryHeaderWrapper.GrossWeightUQ);
		}

		public void TestNew()
		{
			AssertNull("Created with null", DocCusEntryHeader.New(null, Factory));
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertNotNull("Created with a valild object", DocCusEntryHeader.New(entryHeader, Factory));
		}

		public void TestInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("InvoiceLines", 1, headerWrapper.InvoiceLines.Count);
		}

		public void TestInvoiceHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);
			AssertEquals("InvoiceHeaders", 1, headerWrapper.InvoiceHeaders.Count);
		}
		public void TestDeclaration()
		{
			AssertNotNull("Declaration", entryHeader.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), entryHeaderWrapper.Declaration.GetType());
		}

		public void TestEntryLinesCollection()
		{
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();

			AssertEquals(2, entryHeaderWrapper.EntryLines.Count);
		}

		public void TestCargoArrivalDocumentNumber()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_CargoArrivalDocumentNumber = BRUtilizationList.Codes.Total;
			var invoiceLines = entryHeader.Declaration.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLines.JI_CL = entryLine.PK;

			AssertEquals("JE_CargoArrivalDocumentNumber should be ", "1", entryHeaderWrapper.CargoArrivalDocumentNumber);
		}

		public void TestGrossWeight()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_TotalWeight = 150m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Hectograms;

			AssertEquals("GrossWeight should be", 15m, entryHeaderWrapper.GrossWeight);
		}

		public void TestValuesInLocalCurrency()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var entryLine = entryHeader.MergedLines.AddNew();

			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line1.JI_CL = entryLine.PK;

			var line2 = invoiceHeader.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.10.10 10";
			line2.JI_LinePrice = 200.0m;
			line2.JI_CL = entryLine.PK;

			var line1ONS = line1.Charges.AddNew(ImportChargesProvider.OverseasInsurance.Code, 5.0m);
			var line1OFP = line1.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 10.0m);
			var line1OFC = line1.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code, 1.0m);
			var line2ONS = line2.Charges.AddNew(ImportChargesProvider.OverseasInsurance.Code, 10.0m);
			var line2OFP = line2.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 20.0m);
			var line2OFC = line2.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code, 3.0m);

			Factory.Save();
			entryHeader.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("FOB in local currency", 300.0m, entryHeaderWrapper.TotalFOBInLocalCurrency);
				AssertEquals("Freight in local currency", 34.0m, entryHeaderWrapper.TotalFreightInLocalCurrency);
				AssertEquals("Insurance in local currency", 15.0m, entryHeaderWrapper.TotalInsuranceInLocalCurrency);
				AssertEquals("Total customs value", 349.0m, entryHeaderWrapper.TotalCustomsValue);
			});
		}

		public void TestAdditionalInformation()
		{
			var cusEntryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.AdditionalInformation = "AdditionalInformation";
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			AssertEquals("AdditionalInformation", entryHeaderWrapper.AdditionalInformation);
		}

		public void TestEntryIssueDate()
		{
			entryHeader.MovementReferenceNumberSetter("123", issueDate: ZDateTime.Today);
			AssertEquals("MovementReferenceNumberIssueDate", ZDateTime.Today, entryHeaderWrapper.EntryIssueDate);
		}

		public void TestSumTaxes()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(Enterprise.Customs.Business.ChargeTypesList.Codes.DTY, 10m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.IPI, 20m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.PIS, 30m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 40m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 50m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.ICMS, 99.253m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.ICMSFCP, 18.20m);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Enterprise.Customs.Business.ChargeTypesList.Codes.DTY, 1m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.IPI, 2m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.PIS, 3m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 4m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 5m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.ICMS, 6m);
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.ICMSFCP, 13.50m);

			CombineAssertions(() =>
			{
				AssertEquals("DTY Charge Amount", 11m, entryHeaderWrapper.TotalDutyAmount);
				AssertEquals("IPI Charge Amount", 22m, entryHeaderWrapper.TotalIPIChargeAmount);
				AssertEquals("DTY Charge Amount", 33m, entryHeaderWrapper.TotalPISChargeAmount);
				AssertEquals("Cofins Charge Amount", 44m, entryHeaderWrapper.TotalCofinsChargeAmount);
				AssertEquals("Antidumping Charge Amount", 55m, entryHeaderWrapper.TotalAntidumpingChargeAmount);
				AssertEquals("TotalICMSChargeAmount Charge Amount", 105.25m, entryHeaderWrapper.TotalICMSChargeAmount);
				AssertEquals("TotalFCPChargeAmount Charge Amount", 31.7m, entryHeaderWrapper.TotalFCPChargeAmount);
			});
		}

		public void TestPacking()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Units");
			var packCode1 = helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "01", "Test 1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			var packCode2 = helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "02", "Test 2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(packCode1, "PT", "Teste 1");
			helper.CreateNewOrGetExistingCusCodeListLanguage(packCode2, "PT", "Teste 2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = "MB";
			houseBill.CU_BillNum = "123";
			var packing = houseBill.PackingGroups.AddNew();

			var pack1 = packing.Packages.AddNew();
			pack1.CW_PackType = "01";
			pack1.CW_PackQty = 10;

			var pack2 = packing.Packages.AddNew();
			pack2.CW_PackType = "02";
			pack2.CW_PackQty = 15;

			var pack3 = packing.Packages.AddNew();
			pack3.CW_PackType = "01";
			pack3.CW_PackQty = 10;

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			Factory.Save();
			entryHeader.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var headerWrapper = CreateEntryHeaderWrapper(entryHeader);

			AssertEquals("PackagesCollection must contain", 3, headerWrapper.Packs.Count);
			CombineAssertions(() =>
			{
				AssertEquals("First Unit position must be", "01", headerWrapper.Packs[0].Unit);
				AssertEquals("Second Unit position must be", "01", headerWrapper.Packs[1].Unit);
				AssertEquals("Third Unit position must be", "02", headerWrapper.Packs[2].Unit);
			});
		}

		public void TestLocalCurrency()
		{
			AssertEquals("LocalCurrency", "BRL", entryHeaderWrapper.LocalCurrency);
		}

		public void TestEntryAccessKey()
		{
			entryHeader.EntryAccessKey = "ACCESSKEY";
			AssertEquals("ACCESSKEY", entryHeaderWrapper.EntryAccessKey);
		}

		public void TestSectorCodeAndDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "Warehousing Sectors");
			var refCusCodeType = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00001;001", "Warehousing Sector Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeType, "PT", "Teste1 Setor de Armazenagem");
			Factory.Save();

			var declaration = entryHeader.Declaration;
			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = "CE00001";
			declaration.JE_SubLocationOfGoods = "001";
			AssertEquals("Sector", "001 - Teste1 Setor de Armazenagem", entryHeaderWrapper.Sector.CodeAndDescription);
		}

		public void TestWarehouseAreasConcatenated()
		{
			var declaration = entryHeader.Declaration;
			var warehouseID = declaration.WarehouseAreas.AddNew();
			warehouseID.CY_Code = "00001";
			var warehouseID2 = declaration.WarehouseAreas.AddNew();
			warehouseID2.CY_Code = "00002";
			var warehouseID3 = declaration.WarehouseAreas.AddNew();
			warehouseID3.CY_Code = "00003";

			AssertEquals("WarehouseAreasConcatenated", "00001,00002,00003", entryHeaderWrapper.WarehouseAreasConcatenated);
		}

		public void TestEntryStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);
			var brCode = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E10", "Registered", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(brCode, "PT", "Registrado");
			Factory.Save();

			entryHeader.CH_EntryStatus = "E10";
			AssertEquals("E10 - Registrado", entryHeaderWrapper.EntryStatus.CodeAndDescription);
		}

		public void TestCargoStatus()
		{
			entryHeader.CH_CargoStatus = "2";

			AssertEquals("CargoStatus should be ", "2 - Em trânsito", entryHeaderWrapper.CargoStatus.CodeAndDescription);
		}

		public void TestAdministrativeStatus()
		{
			entryHeader.CH_AdministrativeStatus = "1";

			AssertEquals("AdministrativeStatus should be ", "1 - Adiado", entryHeaderWrapper.AdministrativeStatus.CodeAndDescription);
		}

		public void TestCargoProvenance()
		{
			var declaration = entryHeader.Declaration;
			AssertNull("CargoProvenance", entryHeaderWrapper.CargoProvenance);

			declaration.JE_GoodsOrigin = "US";

			AssertNotNull("CargoProvenance", entryHeaderWrapper.CargoProvenance);
			AssertEquals("CargoProvenance should be", "US", entryHeaderWrapper.CargoProvenance.Code);
		}

		public void TestValidityILShipmentDate()
		{
			entryHeader.CH_ValidityILShipmentDate = ZDateTime.Today;
			AssertEquals("ValidityILShipmentDate", ZDateTime.Today, entryHeaderWrapper.ValidityILShipmentDate);
		}

		public void TestValidityILDispatchDate()
		{
			entryHeader.CH_ValidityILDispatchDate = ZDateTime.Today;
			AssertEquals("ValidityILDispatchDate", ZDateTime.Today, entryHeaderWrapper.ValidityILDispatchDate);
		}

		public void TestDeclarant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = "Declarant";
			declarant.PrimaryRegistrationNumber.Number = "29022515830001";
			var declarantOA = declarant.Addresses.AddNewMainAddress();

			AssertNull("Declarant", entryHeaderWrapper.Declarant);
			var declaration = entryHeader.Declaration;

			declaration.JE_OA_DeclarantAddress = declarantOA.PK;
			AssertEquals("Declarant.Name", "Declarant", entryHeaderWrapper.Declarant.Name);
			AssertEquals("Declarant.BusinessRegNo", "29022515830001", entryHeaderWrapper.Declarant.BusinessRegNo);
		}

		public void TestValuesInInvoiceHeadersMultiCurrencies()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var line1 = invoiceHeader1.InvoiceLines.AddNew();
			line1.JI_LinePrice = 100.0m;
			line1.JI_CL = entryLine.PK;
			line1.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 20.0m);

			var line2 = invoiceHeader2.InvoiceLines.AddNew();
			line2.JI_LinePrice = 200.0m;
			line2.JI_CL = entryLine.PK;
			line2.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 40.0m);

			AssertInvoiceHeadersValues(ECC.CurrencyCodes.UnitedStates, ECC.CurrencyCodes.UnitedStates, ECC.CurrencyCodes.UnitedStates, 300.0m, 360.0m);
			AssertInvoiceHeadersValues(ECC.CurrencyCodes.EuropeanUnion, ECC.CurrencyCodes.UnitedStates, ECC.CurrencyCodes.Brazil, 204.86m, 248.03m);

			void AssertInvoiceHeadersValues(ZString invoiceCurrency1, ZString invoiceCurrency2, ZString expectedCurrency, ZDecimal expectedFob, ZDecimal expectedCif)
			{
				invoiceHeader1.JZ_RX_NKInvoice_Currency = invoiceCurrency1;

				invoiceHeader2.JZ_RX_NKInvoice_Currency = invoiceCurrency2;

				entryHeader.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("InvoiceHeadersFOBCurrency", expectedCurrency, entryHeaderWrapper.InvoiceHeadersFOBCurrency);
					AssertEquals("InvoiceHeadersCustomsValueCurrency", expectedCurrency, entryHeaderWrapper.InvoiceHeadersCustomsValueCurrency);
					AssertEquals("TotalFOBInInvoiceHeaders", expectedFob, entryHeaderWrapper.TotalFOBInInvoiceHeaders);
					AssertEquals("TotalCustomsValueInInvoiceHeaders", expectedCif, entryHeaderWrapper.TotalCustomsValueInInvoiceHeaders);
				});
			}
		}

		public void TestTotalNFeLinePrice()
		{
			var invoiceLines = entryHeader.Declaration.InvoiceLines.AddNew();
			invoiceLines.JI_NFeLinePrice = 35m;

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLines.JI_CL = entryLine.PK;

			AssertEquals("TotalNFeLinePrice should be", 35m, entryHeaderWrapper.TotalNFeLinePrice);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Brazil; }
		}

		protected CusEntryHeader entryHeader;
		protected DocCusEntryHeader entryHeaderWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = EntryHeaderInternal;
			entryHeaderWrapper = EntryHeaderWrapperInternal;
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		#endregion
	}
}
