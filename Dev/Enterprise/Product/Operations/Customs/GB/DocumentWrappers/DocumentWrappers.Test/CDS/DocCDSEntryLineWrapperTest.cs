using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.CDS.Testing
{
	public class DocCDSEntryLineWrapperTest : TestCaseWithFactory
	{
		public void TestDocCDSEntryLineWrapperProperties()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryHeader.EntryNumber = "ENT123456";
			declaration.JE_EntryStyle = "02";
			declaration.CusEntryInstruction.CEI_Style = "EDF";
			declaration.JE_CustomsOffice = "GB012345";

			invoiceHeader.JZ_InvoiceAmount = 1234.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.RelatedIndicator = true;
			invoiceHeader.RelatedIndicator2 = true;
			invoiceHeader.RelatedIndicator3 = true;
			invoiceHeader.RelatedIndicator4 = true;

			invoiceLine.JI_Weight = 120.00m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 110.00m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_ConcessionOrder = "YAY";
			invoiceLine.JI_LinePrice = 234.00m;

			invoiceLine.JI_Procedure = "1234999";
			var additionalProcedureCode1 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode1.CY_Code = "1234567";
			var additionalProcedureCode2 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode2.CY_Code = "1234 ";
			var additionalProcedureCode3 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode3.CY_Code = "1234ABC";
			var additionalProcedureCode4 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode4.CY_Code = "1234XYZ";

			invoiceLine.JI_RN_NKCountryOfExport = "LV";
			invoiceLine.JI_CountryOfOrigin = "EU";
			invoiceLine.ZG_CountryOfSupply = "FR";
			invoiceLine.ZG_CountryOfDestination = "DE";
			invoiceLine.JI_ValuationCode = "1";

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			var line = DocCDSEntryLineWrapper.New(entryLine);

			AssertEquals("35 Gross Mass in KGs", "120", line.Box35ItemGrossMass);
			AssertEquals("38 Net Mass in KGs", 110m, line.Box35ItemNetMass);
			AssertEquals("39 Quota", "YAY", line.Box39ItemQuota);
			AssertEquals("42 Item Price", "234.00 USD", line.Box42ItemPriceAndCurrency);
			AssertEquals("LV", line.CountryOfDispatch);
			AssertEquals("EU", line.CountryOfPrefOrigin);
			AssertEquals("FR", line.CountryOfPrefOriginOverride);
			AssertEquals("DE", line.DestinationCountry);
			AssertEquals("1111", line.ValueAdjCode);
			AssertEquals("1234999", line.ProcedureCode);
			AssertEquals("567,999,ABC,XYZ", line.AdditionalProcedureCodes);

			invoiceLine.JI_Weight = 0;
			entryLine = entryHeader.MergedLines[0];
			line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("35 Gross Mass in KGs should be blank not zero", "", line.Box35ItemGrossMass);
		}

		public void TestLinePackages()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];

			foreach (Customs.Business.NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
			{
				container.IsForInvoiceLine = true;
			}
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			var cw1 = bill.PackingGroups[0].Packages[0];
			cw1.CW_PackQty = 10;
			cw1.CW_PackType = "PK";
			cw1.CW_MarksAndNos = "AS ADDRESSED";
			var cw2 = bill.PackingGroups[0].Packages.AddNew();
			cw2.CW_PackQty = 11;
			cw2.CW_PackType = "BA";
			cw2.CW_MarksAndNos = "BLUE";

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 6;

			var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			DocCDSEntryLineWrapper line = DocCDSEntryLineWrapper.New(entryLine);

			line = DocCDSEntryLineWrapper.New(entryLine);
			AssertContains("6 | PK | AS ADDRESSED\r\n", line.Box22ItemPackages);
		}

		public void TestLinePreviousDocs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			PreviousDocument documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
			documentOnHeader.CSI_SubType = "X";
			documentOnHeader.CSI_Code = "280";
			documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

			PreviousDocument documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "3421789012";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);

			PreviousDocument documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber = "20070701-120-A12345E";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);

			PreviousDocument documentOnGroup = declaration.PreviousDocuments.AddNew();
			documentOnGroup.CSI_SubType = "A";
			documentOnGroup.CSI_Code = "123";
			documentOnGroup.CSI_ReferenceNumber = "987654321";

			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertContains("entryLine.Box40PreviousDocuments", "X | 280 | ABCDEFG\r\n", line.Box40ItemSummaryDeclarationPreviousDocs);
			AssertContains("entryLine.Box40PreviousDocuments", "Z | 380 | 3421789012-02/01/2000", line.Box40ItemSummaryDeclarationPreviousDocs);
			AssertContains("entryLine.Box40PreviousDocuments", "Y | CLE | 20070701-120-A12345E-03/01/2000", line.Box40ItemSummaryDeclarationPreviousDocs);
		}

		public void TestLineAdditionalDocsAndInfos()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var cds = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var gbGroup = helper.CreateNewOrGetExistingDataGrouping(cds);

				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();

				attributeNameValuePairs.Clear();
				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, new string[] { importCodeType, exportCodeType }, "1920", "1920 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, new string[] { importCodeType, exportCodeType }, "1968", "1968 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				Factory.Save();

				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
				var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
				var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;

				invoiceLine1.JI_CL = entryLine1.PK;
				invoiceLine2.JI_CL = entryLine2.PK;

				var documentLine1 = invoiceLine1.SupportingDocuments.AddNew();
				documentLine1.CSI_Code = "1920";
				documentLine1.CSI_ReferenceNumber = "Ref ID 1";

				var documentLine2 = invoiceLine2.SupportingDocuments.AddNew();
				documentLine2.CSI_Code = "1968";
				documentLine2.CSI_ReferenceNumber = "Ref ID 2";

				var infoLine1 = invoiceLine1.AdditionalInfos.AddNew();
				infoLine1.CSI_Code = "Y901";
				infoLine1.CSI_Description = "Description 1";

				var infoLine2 = invoiceLine2.AdditionalInfos.AddNew();
				infoLine2.CSI_Code = "Y902";
				infoLine2.CSI_Description = "Description 2";

				declaration.JE_MasterUCR = "A:08145678901";
				entryHeader.CH_BGMReference = "GB332278957000-B00001258/121";

				var line1 = DocCDSEntryLineWrapper.New(entryLine1);
				var line2 = DocCDSEntryLineWrapper.New(entryLine2);
				var expectedDocs = @"1968 | Ref ID 2 |  |  |  |  |  | 0";
				var expectedInfos = @"Y901 | Description 1";
				AssertEquals("entryLine.Box44ItemAdditionalDocs", expectedDocs, line2.Box44ItemAdditionalDocs);
				AssertEquals("entryLine.Box44ItemAdditionalInformation", expectedInfos, line1.Box44ItemAdditionalInformation);

				var entryLine3 = entryHeader.MergedLines.AddNew();
				entryLine3.CL_LineNumber = 3;

				var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_CL = entryLine3.PK;

				var documentLine3 = invoiceLine3.SupportingDocuments.AddNew();
				documentLine3.CSI_Code = "1968";
				documentLine3.CSI_ReferenceNumber = "R1234";
				documentLine3.CSI_SubType = "AA";
				documentLine3.CSI_Status = "JE";
				documentLine3.CSI_Description = "Test Reason";
				documentLine3.CSI_ReferenceNumber2 = "Test Authority";
				documentLine3.CSI_DateOfIssue = new ZDateTime(2022, 9, 1);
				documentLine3.CSI_UnitOfQuantity = "KAC";
				documentLine3.CSI_Quantity = 11;

				var documentLine4 = invoiceLine3.SupportingDocuments.AddNew();
				documentLine4.CSI_Code = "1968";
				documentLine4.CSI_ReferenceNumber = "R1234";
				documentLine4.CSI_SubType = "AA";
				documentLine4.CSI_Status = "JE";
				documentLine4.CSI_Description = "Test Reason";
				documentLine4.CSI_ReferenceNumber2 = "Test Authority";
				documentLine4.CSI_DateOfIssue = new ZDateTime(2022, 9, 1);
				documentLine4.CSI_UnitOfQuantity = "KAC";
				documentLine4.CSI_Quantity = 22;

				var line3 = DocCDSEntryLineWrapper.New(entryLine3);
				AssertEquals("entryLine.Box44ItemAdditionalDocs", "1968 | R1234-AA | JE | Test Reason | Test Authority | 01/09/2022 | KAC | 33", line3.Box44ItemAdditionalDocs);
			}
		}

		public void TestBox44FiscalReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var fiscalRef = invoiceLine.FiscalReferences.AddNew();
			fiscalRef.CFR_Code = "ABC";
			fiscalRef.CFR_Reference = "1234567890";

			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("entryLine.Box44FiscalReferences", "ABC | 1234567890", line.Box44FiscalReferences);
		}

		public void TestBox2Exporter()
		{
			var entryLine = SetupBox2ExporterData();
			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("entryLine.Box2Exporter", "CompanyName1\r\n1 Address1", line.Box2Exporter);
		}

		public void TestBox2ExporterNumber()
		{
			var entryLine = SetupBox2ExporterData();
			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("entryLine.Box2ExporterNumber", "GB1234", line.Box2ExporterNumber);
		}

		CusEntryLine SetupBox2ExporterData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "CompanyName1";
			org1.MainAddress.Address1 = "1 Address1";
			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "GB";
			cusCode1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode1.OK_CustomsRegNo = "1234";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "CompanyName2";
			org2.MainAddress.Address1 = "2 Address1";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "GB";
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode2.OK_CustomsRegNo = "5678";
			invoiceHeader1.JZ_OA_SupplierAddress = org1.MainAddress.PK;
			invoiceHeader2.JZ_OA_SupplierAddress = org2.MainAddress.PK;
			Factory.Save();
			return entryLine1;
		}

		public void TestBox45AdditionsAndDeductions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("Pre-requisite: AL is item level charge", expected: true, new CDSChargeTypeLevelCalculator(CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(CDSCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, false, null, false)).IsItemLevel);
			AssertEquals("Pre-requisite: BM is item level charge", expected: true, new CDSChargeTypeLevelCalculator(CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(CDSCustomsChargeTypeList.Codes.BuyingCommissionsCharge, false, null, false)).IsItemLevel);
			AssertEquals("Pre-requisite: AK is header level charge", expected: false, new CDSChargeTypeLevelCalculator(CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge, false, null, false)).IsItemLevel);

			var charge1 = invoiceLine.Charges.AddNew(CDSCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
			charge1.J7_Amount = 1.23m;
			var charge2 = invoiceLine.Charges.AddNew(CDSCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			charge2.J7_Amount = 2.34m;
			var charge3 = invoiceLine.Charges.AddNew(CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge);
			charge3.J7_Amount = 3.45m;

			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("entryLine.Box45AdditionsAndDeductions", "AL | 1.23\r\nBM | 2.34", line.Box45AdditionsAndDeductions);
		}

		public void TestBox31ContainerNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C01";
			invoiceLine.ContainersPivot.AddPivotFor(container1);
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C02";
			invoiceLine.ContainersPivot.AddPivotFor(container2);
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C03";

			var line = DocCDSEntryLineWrapper.New(entryLine);
			AssertEquals("entryLine.Box31ContainerNumbers", "C01\r\nC02", line.Box31ContainerNumbers);
		}

		public void TestBox47Taxes()
		{
			var line = CreateTestEntryLineWrapperForTaxes();

			AssertEquals("A00\r\nB00", line.Box47ChargeTypes);
			AssertEquals("26.99\r\n200.34", line.Box47TaxAssessedAmounts);
			AssertEquals("1000.00\r\n1144.81", line.Box47TaxBases);
			AssertEquals("26.99\r\n200.34", line.Box47TaxPayableAmounts);
		}

		public void TestBox47Taxes_ConfirmedFees()
		{
			var line = CreateTestEntryLineWrapperForTaxes((entry, taxDty, taxVat) => AddConfirmedFees(entry));
			AssertEquals("A00\r\nB00", line.Box47ChargeTypes);
			AssertEquals("27.00\r\n200.00", line.Box47TaxAssessedAmounts);
			AssertEquals("999.00\r\n1145.00", line.Box47TaxBases);
			AssertEquals("27.00\r\n200.00", line.Box47TaxPayableAmounts);
		}

		public void TestBox47Taxes_ConfirmedFees_TwoLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var line1 = entryHeader.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			AddFees(line1);
			AddConfirmedFees(line1);

			var line2 = entryHeader.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			AddFees(line2);

			var wrapper1 = DocCDSEntryLineWrapper.New(line1);
			var wrapper2 = DocCDSEntryLineWrapper.New(line2);

			CombineAssertions(() =>
			{
				AssertEquals("Line 1 Box47ChargeTypes", "A00\r\nB00", wrapper1.Box47ChargeTypes);
				AssertEquals("Line 1 Box47TaxAssessedAmounts", "27.00\r\n200.00", wrapper1.Box47TaxAssessedAmounts);
				AssertEquals("Line 1 Box47TaxBases", "999.00\r\n1145.00", wrapper1.Box47TaxBases);
				AssertEquals("Line 1 Box47TaxPayableAmounts", "27.00\r\n200.00", wrapper1.Box47TaxPayableAmounts);

				AssertEquals("Line 2 Box47ChargeTypes", ZString.Empty, wrapper2.Box47ChargeTypes);
				AssertEquals("Line 2 Box47TaxAssessedAmounts", ZString.Empty, wrapper2.Box47TaxAssessedAmounts);
				AssertEquals("Line 2 Box47TaxBases", ZString.Empty, wrapper2.Box47TaxBases);
				AssertEquals("Line 2 Box47TaxPayableAmounts", ZString.Empty, wrapper2.Box47TaxPayableAmounts);
			});
		}

		public void TestBox47Taxes_DeferredPayment()
		{
			var line = CreateTestEntryLineWrapperForTaxes((entry, taxDty, taxVat) =>
			{
				taxVat.CF_IsLandedCostOnly = ZBool.True;
			});

			AssertEquals("A00\r\nB00", line.Box47ChargeTypes);
			AssertEquals("26.99\r\n200.34", line.Box47TaxAssessedAmounts);
			AssertEquals("1000.00\r\n1144.81", line.Box47TaxBases);
			AssertEquals("26.99\r\n0.00", line.Box47TaxPayableAmounts);
			AssertEquals("DTN\r\n", line.Box47TaxMeasureUnits);
		}

		DocCDSEntryLineWrapper CreateTestEntryLineWrapperForTaxes(System.Action<CusEntryLine, CusEntryLineFee, CusEntryLineFee> additionalInitialisationAction = null)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			(var taxDty, var taxVat) = AddFees(entryLine);
			additionalInitialisationAction?.Invoke(entryLine, taxDty, taxVat);

			return DocCDSEntryLineWrapper.New(entryLine);
		}

		(CusEntryLineFee, CusEntryLineFee) AddFees(CusEntryLine entryLine)
		{
			var taxDty = entryLine.Fees.AddNew();
			taxDty.CF_ChargeType = "A00";
			taxDty.CF_ChargeAmount = 26.99;
			taxDty.CF_BaseValue = 1000m;
			taxDty.CF_MethodOfCalculation = "DTN";

			var taxVat = entryLine.Fees.AddNew();
			taxVat.CF_ChargeType = "B00";
			taxVat.CF_ChargeAmount = 200.34;
			taxVat.CF_BaseValue = 1144.81m;
			taxVat.CF_MethodOfCalculation = "%";

			return (taxDty, taxVat);
		}

		void AddConfirmedFees(CusEntryLine entryLine)
		{
			var taxDtyConfirmed = entryLine.ConfirmedFees.AddNew();
			var taxVatConfirmed = entryLine.ConfirmedFees.AddNew();

			taxDtyConfirmed.CF_ChargeType = "A00";
			taxVatConfirmed.CF_ChargeType = "B00";

			taxVatConfirmed.CF_ChargeAmount = 200.00m;
			taxVatConfirmed.CF_BaseValue = 1145.00m;

			taxDtyConfirmed.CF_ChargeAmount = 27.00m;
			taxDtyConfirmed.CF_BaseValue = 999m;
		}
	}
}
