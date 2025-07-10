using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[TestedType(typeof(ImportAccompanyingDocumentEntryLineWrapper))]
	public class ImportAccompanyingDocumentEntryLineWrapperTest : DocumentWrapperTestCase
	{
		public void TestImportAccompanyingDocumentEntryLineWrapperProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "IMP";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryHeader.EntryNumber = "ENT123456";
			declaration.JE_EntryStyle = "02";
			declaration.CustomsEntryInstructions[0].CEI_Style = "EDF";
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
			invoiceLine.JI_Procedure = "400000";
			invoiceLine.JI_CountryOfOrigin = "EU";
			invoiceLine.ZG_CountryOfSupply = "FR";
			invoiceLine.ZG_CountryOfDestination = "DE";
			invoiceLine.JI_ValuationCode = "1";

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			var line = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);

			AssertEquals("35 Gross Mass in KGs", "120", line.Box35ItemGrossMass);
			AssertEquals("38 Net Mass in KGs", 110m, line.Box35ItemNetMass);
			AssertEquals("39 Quota", "YAY", line.Box39ItemQuota);
			AssertEquals("42 Item Price", "168.35 EUR", line.Box42ItemPriceAndCurrency);
			AssertEquals("EU", line.CountryOfPrefOrigin);
			AssertEquals("FR", line.CountryOfPrefOriginOverride);
			AssertEquals("DE", line.DestinationCountry);
			AssertEquals("1111", line.ValueAdjCode);
			AssertEquals("Procedure Code", "4000", line.ProcedureCode);

			invoiceLine.JI_Weight = 0;
			entryLine = entryHeader.MergedLines[0];
			line = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);
			AssertEquals("35 Gross Mass in KGs should be blank not zero", "", line.Box35ItemGrossMass);
		}

		public void TestLinePackages()
		{
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

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

			AssertContains("6 | PK | AS ADDRESSED", wrapper.Box22ItemPackages);
		}

		public void TestLinePreviousDocs()
		{
			var documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
			documentOnHeader.CSI_SubType = "X";
			documentOnHeader.CSI_Code = "280";
			documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

			var documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "3421789012";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			documentOnFirstLine.CSI_LineNo = 1;

			var documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber = "20070701-120-A12345E";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			documentOnSecondLine.CSI_LineNo = 2;

			var documentOnGroup = declaration.PreviousDocuments.AddNew();
			documentOnGroup.CSI_SubType = "A";
			documentOnGroup.CSI_Code = "123";
			documentOnGroup.CSI_ReferenceNumber = "987654321";

			AssertContains("entryLine.Box40PreviousDocuments", "380 | 3421789012 | 1", wrapper.Box40ItemSummaryDeclarationPreviousDocs);
			AssertContains("entryLine.Box40PreviousDocuments", "CLE | 20070701-120-A12345E | 2", wrapper.Box40ItemSummaryDeclarationPreviousDocs);
		}

		public void TestLineAdditionalDocsAndInfos()
		{
			var imp = "IMP";
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(imp, new string[] { importCodeType, exportCodeType }, "1920", "1920 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			helper.CreateCusCodeListsForMultipleTypesWithAttributes(imp, new string[] { importCodeType, exportCodeType }, "1968", "1968 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine2.JI_CL = entryLine2.PK;

			var documentLine1 = invoiceLine.SupportingDocuments.AddNew();
			documentLine1.CSI_Code = "1920";
			documentLine1.CSI_ReferenceNumber = "Ref ID 1";

			var documentLine2 = invoiceLine2.SupportingDocuments.AddNew();
			documentLine2.CSI_Code = "1968";
			documentLine2.CSI_ReferenceNumber = "Ref ID 2";

			var infoLine1 = invoiceLine.AdditionalInfos.AddNew();
			infoLine1.CSI_Code = "Y901";
			infoLine1.CSI_Description = "Description 1";

			var infoLine2 = invoiceLine2.AdditionalInfos.AddNew();
			infoLine2.CSI_Code = "Y902";
			infoLine2.CSI_Description = "Description 2";

			entryHeader.CH_BGMReference = "GB332278957000-B00001258/121";

			var line1 = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);
			var line2 = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine2);
			var expectedDocs = @"1968 | Ref ID 2 |  |  | 0 |  | 0";
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
			documentLine3.CSI_DateOfExpiry = new ZDateTime(2022, 9, 1);
			documentLine3.CSI_UnitOfQuantity = "KAC";
			documentLine3.CSI_Quantity = 11;

			var documentLine4 = invoiceLine3.SupportingDocuments.AddNew();
			documentLine4.CSI_Code = "1968";
			documentLine4.CSI_ReferenceNumber = "R1234";
			documentLine4.CSI_SubType = "AA";
			documentLine4.CSI_Status = "JE";
			documentLine4.CSI_Description = "Test Reason";
			documentLine4.CSI_ReferenceNumber2 = "Test Authority";
			documentLine4.CSI_DateOfExpiry = new ZDateTime(2022, 9, 1);
			documentLine4.CSI_UnitOfQuantity = "KAC";
			documentLine4.CSI_Quantity = 22;

			var line3 = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine3);
			AssertContains("entryLine.Box44ItemAdditionalDocs", new ZString("1968 | R1234 | 01-Sep-22 | KAC | 11 |  | 0"), line3.Box44ItemAdditionalDocs);
			AssertContains("entryLine.Box44ItemAdditionalDocs", new ZString("1968 | R1234 | 01-Sep-22 | KAC | 22 |  | 0"), line3.Box44ItemAdditionalDocs);
		}

		public void TestBox44FiscalReferences()
		{
			var fiscalRef = invoiceLine.FiscalReferences.AddNew();
			fiscalRef.CFR_Code = "ABC";
			fiscalRef.CFR_Reference = "1234567890";

			AssertEquals("entryLine.Box44FiscalReferences", "ABC | 1234567890", wrapper.Box44FiscalReferences);
		}

		public void TestBox2Exporter()
		{
			SetupBox2ExporterData();
			AssertContains("entryLine.Box2Exporter", "CompanyName1", wrapper.Box2Exporter);
			AssertContains("entryLine.Box2Exporter", "1 Address1", wrapper.Box2Exporter);
		}

		public void TestBox2ExporterNumber()
		{
			SetupBox2ExporterData();
			AssertEquals("entryLine.Box2ExporterNumber", "GB1234", wrapper.Box2ExporterNumber);
		}

		void SetupBox2ExporterData()
		{
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
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
			invoiceLine.JI_OA_ExporterAddress = org1.MainAddress.PK;
			invoiceLine2.JI_OA_ExporterAddress = org2.MainAddress.PK;
			Factory.Save();
		}

		public void TestBox31ContainerNumbers()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C01";
			invoiceLine.ContainersPivot.AddPivotFor(container1);
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C02";
			invoiceLine.ContainersPivot.AddPivotFor(container2);
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C03";

			AssertEquals("entryLine.Box31ContainerNumbers", "C01\r\nC02", wrapper.Box31ContainerNumbers);
		}

		public void TestBox47Taxes_Fees()
		{
			var wrapper = CreateTestEntryLineWrapperForTaxes();
			CombineAssertions("Should map Box47Taxes to Fees when ConfirmedFees not present.", () =>
			{
				AssertEquals("A00\r\nB00", wrapper.Box47ChargeTypes);
				AssertEquals("26.99\r\n200.34", wrapper.Box47TaxAssessedAmounts);
				AssertEquals("1000.00\r\n1144.81", wrapper.Box47TaxBases);
				AssertEquals("26.99\r\n200.34", wrapper.Box47TaxPayableAmounts);
			});
		}

		public void TestBox47Taxes_ConfirmedFees()
		{
			var wrapper = CreateTestEntryLineWrapperForTaxes(
				(entry, taxDty, taxVat) => {
					AddConfirmedFees(entry);
					entryLine.Fees.RemoveAndDeleteAll();
				}
			);
			AssertEquals("Box47ChargeTypes: Line breaking separated CF_ChargeType", "A00\r\nB00", wrapper.Box47ChargeTypes);
			AssertEquals("Box47TaxAssessedAmounts: Line breaking separated CF_ChargeAmount", "27.00\r\n200.00", wrapper.Box47TaxAssessedAmounts);
			AssertEquals("Box47TaxBases: Line breaking separated CF_BaseValue", "999.00\r\n1145.00", wrapper.Box47TaxBases);
			AssertEquals("Box47TaxPayableAmounts: Line breaking separated CF_ChargeAmount", "27.00\r\n200.00", wrapper.Box47TaxPayableAmounts);
		}

		public void TestBox47Taxes_ConfirmedFees_TwoLines()
		{
			var line1 = entryHeader.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			AddFees(line1);
			AddConfirmedFees(line1);

			var line2 = entryHeader.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			AddFees(line2);

			var wrapper1 = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, line1);
			var wrapper2 = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, line2);

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
			var wrapper = CreateTestEntryLineWrapperForTaxes((entry, taxDty, taxVat) =>
			{
				var (confirmedTaxDty, confirmedTaxVat) = AddConfirmedFees(entry);
				taxVat.CF_IsLandedCostOnly = ZBool.True;
				confirmedTaxVat.CF_IsLandedCostOnly = ZBool.True;
			});

			AssertEquals("A00\r\nB00", wrapper.Box47ChargeTypes);
			AssertEquals("27.00\r\n200.00", wrapper.Box47TaxAssessedAmounts);
			AssertEquals("999.00\r\n1145.00", wrapper.Box47TaxBases);
			AssertEquals("27.00\r\n0.00", wrapper.Box47TaxPayableAmounts);
			Assert("Invalid Unit returns empty", wrapper.Box47TaxMeasureUnits.IsEmpty);
		}

		public void TestNationalAdditionalCodes()
		{
			invoiceLine.JI_ZZF_NKTaxType = "VATS";

			var tariffDetail1 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail1.BZ_Tariff = "TAR 1";

			var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Tariff = "TAR 2";

			AssertContains("entryLine.NationalAdditionalCodes", "VATS", wrapper.NationalAdditionalCodes);
			AssertContains("entryLine.NationalAdditionalCodes", "TAR 1", wrapper.NationalAdditionalCodes);
			AssertContains("entryLine.NationalAdditionalCodes", "TAR 2", wrapper.NationalAdditionalCodes);
		}

		public void TestAdditionalFiscalReferences()
		{
			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo1.CSI_Code = "CODE1";
			additionalInfo1.CSI_Description = "Description 1";

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo2.CSI_Code = "CODE2";
			additionalInfo2.CSI_Description = "Description 2";

			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo3.CSI_Code = "CODE3";
			additionalInfo3.CSI_Description = "Description 3";

			AssertContains("entryLine.AdditionalFiscalReferences", "CODE1 | Description 1", wrapper.AdditionalFiscalReferences);
			AssertContains("entryLine.AdditionalFiscalReferences", "CODE2 | Description 2", wrapper.AdditionalFiscalReferences);
			AssertNotContains("entryLine.AdditionalFiscalReferences", "CODE3 | Description 3", wrapper.AdditionalFiscalReferences);
		}

		ImportAccompanyingDocumentEntryLineWrapper CreateTestEntryLineWrapperForTaxes(System.Action<CusEntryLine, CusEntryLineFee, CusEntryLineFee> additionalInitialisationAction = null)
		{
			entryLine.CL_LineNumber = 1;

			var (taxDty, taxVat) = AddFees(entryLine);
			additionalInitialisationAction?.Invoke(entryLine, taxDty, taxVat);

			return ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);
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

		(Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLineFee) AddConfirmedFees(CusEntryLine entryLine)
		{
			var taxDtyConfirmed = entryLine.ConfirmedFees.AddNew();
			var taxVatConfirmed = entryLine.ConfirmedFees.AddNew();

			taxDtyConfirmed.CF_ChargeType = "A00";
			taxVatConfirmed.CF_ChargeType = "B00";

			taxVatConfirmed.CF_ChargeAmount = 200.00m;
			taxVatConfirmed.CF_BaseValue = 1145.00m;

			taxDtyConfirmed.CF_ChargeAmount = 27.00m;
			taxDtyConfirmed.CF_BaseValue = 999m;

			return (taxDtyConfirmed, taxVatConfirmed);
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "IMP";
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			wrapper = ImportAccompanyingDocumentEntryLineWrapper.New(entryHeader, entryLine);
			return new DocumentWrapper[] { wrapper };
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Ireland;

		CusEntryLine entryLine;
		CusEntryHeader entryHeader;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		ImportAccompanyingDocumentEntryLineWrapper wrapper;
	}
}
