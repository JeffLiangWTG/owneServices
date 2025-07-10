using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestMergeEntryLinesForIM2()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			ZString hsCode = "0123";
			ZString tariff1 = "7326909031";
			ZString tariff2 = "7326909021";
			ZString tariff3 = "7326909022";
			ZString auNumber = "1234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line4 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line5 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line6 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line7 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			InitInvoiceLine(line1, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line2, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line3, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line4, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line5, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line6, tariff2, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line7, tariff2, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);

			declaration.DoMerge(notifier);
			var b2Declaration = declaration.GetNewCopyToB2Declaration();
			b2Declaration.TransactionNumber.SequentialNumber = "00000002";
			b2Declaration.DoMerge(notifier);
			Factory.Save();
			var entryLine = b2Declaration.B3EntryHeader.AllEntryLines;
			AssertEquals("Merged to 2 entry lines", 2, entryLine.Count);
			AssertEquals("1", entryLine[0].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[1].CA_B2SubHeader.ToString());
			AssertEquals(line1.JI_B3LineNumber, entryLine[0].CA_B2LineNo.ToString());
			AssertEquals(line6.JI_B3LineNumber, entryLine[1].CA_B2LineNo.ToString());

			b2Declaration.Invoices[0].InvoiceLines[2].JI_FormattedTariff = tariff2;
			b2Declaration.Invoices[0].InvoiceLines[3].JI_FormattedTariff = tariff2;
			b2Declaration.Invoices[0].InvoiceLines[4].JI_FormattedTariff = tariff3;
			b2Declaration.DoMerge(notifier);
			Factory.Save();
			entryLine = b2Declaration.B3EntryHeader.AllEntryLines;

			AssertEquals("Merged to 4 entry lines", 4, entryLine.Count);
			AssertEquals("1", entryLine[0].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[1].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[2].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[3].CA_B2SubHeader.ToString());

			AssertEquals(line1.JI_B3LineNumber, entryLine[0].CA_B2LineNo.ToString());
			AssertEquals(line1.JI_B3LineNumber + "/SL", entryLine[1].CA_B2LineNo.ToString());
			AssertEquals(line1.JI_B3LineNumber + "/SL", entryLine[2].CA_B2LineNo.ToString());
			AssertEquals(line6.JI_B3LineNumber, entryLine[3].CA_B2LineNo.ToString());

			b2Declaration.Invoices[0].InvoiceLines[5].JI_FormattedTariff = tariff1;
			b2Declaration.DoMerge(notifier);
			Factory.Save();
			entryLine = b2Declaration.B3EntryHeader.AllEntryLines;

			AssertEquals("Merged to 5 entry lines", 5, entryLine.Count);
			AssertEquals("1", entryLine[0].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[1].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[2].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[3].CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine[4].CA_B2SubHeader.ToString());

			AssertEquals(line1.JI_B3LineNumber, entryLine[0].CA_B2LineNo.ToString());
			AssertEquals(line1.JI_B3LineNumber + "/SL", entryLine[1].CA_B2LineNo.ToString());
			AssertEquals(line1.JI_B3LineNumber + "/SL", entryLine[2].CA_B2LineNo.ToString());
			AssertEquals(line6.JI_B3LineNumber + "/SL", entryLine[3].CA_B2LineNo.ToString());
			AssertEquals(line6.JI_B3LineNumber, entryLine[4].CA_B2LineNo.ToString());
		}

		void InitInvoiceLine(JobComInvoiceLine line, ZString tariff, ZString hsCode, ZString auNumber, ZString valuefordutycode, ZString calculationmethod)
		{
			line.JI_FormattedTariff = tariff;
			line.CA_99TariffCode = hsCode;
			line.CA_AuthorityNumber = auNumber;
			line.CA_ValueForDutyCode = valuefordutycode;
			line.CA_CalculationMethod = calculationmethod;
		}

		public new void TestGetEntryCreationStrategies()
		{
			var declaration = Factory.New<JobDeclaration>();
			line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var lineMerger = new LineMergerForTesting(declaration);

			var strategies = lineMerger.GetEntryCreationStrategies_Exposed();
			AssertEquals("Strategies Count", 1, strategies.Length);
			AssertStrategy(strategies[0], typeof(ExportMergeStrategy), MessageTypeList.Codes.G7Export);

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			strategies = lineMerger.GetEntryCreationStrategies_Exposed();
			AssertEquals("Strategies Count", 1, strategies.Length);
			AssertStrategy(strategies[0], typeof(ExportMergeStrategy), MessageTypeList.Codes.DataLoadingModule);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			strategies = lineMerger.GetEntryCreationStrategies_Exposed();
			AssertEquals("Strategies Count", 2, strategies.Length);
			AssertStrategy(strategies[0], typeof(ImportMergeStrategy), MessageTypeList.Codes.EDIRelease);
			AssertStrategy(strategies[1], typeof(ImportMergeStrategy), MessageTypeList.Codes.B3CUSDEC);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;

			strategies = lineMerger.GetEntryCreationStrategies_Exposed();
			AssertEquals("Strategies Count", 1, strategies.Length);
			AssertStrategy(strategies[0], typeof(EntryCreationStrategy), ZString.Empty);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			strategies = lineMerger.GetEntryCreationStrategies_Exposed();
			AssertEquals("Strategies Count", 1, strategies.Length);
			AssertStrategy(strategies[0], typeof(ImportMergeStrategy), MessageTypeList.Codes.B3CUSDEC);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				strategies = lineMerger.GetEntryCreationStrategies_Exposed();
				AssertEquals("Strategies Count", 2, strategies.Length);
				AssertStrategy(strategies[0], typeof(ImportMergeStrategy), MessageTypeList.Codes.EDIRelease);
				AssertStrategy(strategies[1], typeof(ImportMergeStrategy), MessageTypeList.Codes.CommercialAccountingDeclaration);

				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entry.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;

				strategies = lineMerger.GetEntryCreationStrategies_Exposed();
				AssertEquals("Strategies Count", 2, strategies.Length);
				AssertStrategy(strategies[0], typeof(ImportMergeStrategy), MessageTypeList.Codes.EDIRelease);
				AssertStrategy(strategies[1], typeof(ImportMergeStrategy), MessageTypeList.Codes.B3CUSDEC);
			}
		}

		void AssertStrategy(EntryCreationStrategy strategy, Type expectedType, ZString messageType)
		{
			AssertEquals("Strategy Type", expectedType, strategy.GetType());
			AssertEquals("Message Type", true, strategy.GetKeyForHeader(line).Contains(messageType));
		}

		public void TestDutiesAreNotResetIfNotApportioned()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ApportionmentDirty = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), 100);
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), 100);
			declaration.DoMerge(notifier);
			Factory.Save();

			var entryLine = declaration.B3EntryHeader.MergedLines[0];
			AssertEquals("ValueForTax calculated on merge", 200m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueForTax));

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			entryLine = declaration.B3EntryHeader.MergedLines[0];
			AssertEquals("ValueForTax is loaded", 200m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueForTax));
			declaration.JE_CarrierCode = "123";
			declaration.DoMerge(notifier);
			entryLine = declaration.B3EntryHeader.MergedLines[0];
			AssertEquals("ValueForTax should not be reset if not apportioned", 200m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueForTax));

			declaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			declaration.DoMerge(notifier);

			entryLine = declaration.B3EntryHeader.MergedLines[0];
			AssertEquals("ValueForTax should be re-calculated as far as merge type is changed", 100m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueForTax));
		}

		public void TestSetB3LineNumbers()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var notifier = new SendsMessagesToCustomsShutterUpperer();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ApportionmentDirty = true;
				JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
				var invoiceline11 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
				var invoiceline12 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;

				JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
				var invoiceline21 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
				var invoiceline22 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;

				declaration.DoMerge(notifier);
				invoiceline11.B3EntryLine.CL_GoodsShipmentSequence = 1;
				invoiceline12.B3EntryLine.CL_GoodsShipmentSequence = 1;
				invoiceline11.B3EntryLine.CL_CommoditySequence = 1;
				invoiceline12.B3EntryLine.CL_CommoditySequence = 2;
				invoiceline21.B3EntryLine.CL_GoodsShipmentSequence = 2;
				invoiceline22.B3EntryLine.CL_GoodsShipmentSequence = 2;
				invoiceline21.B3EntryLine.CL_CommoditySequence = 3;
				invoiceline22.B3EntryLine.CL_CommoditySequence = 4;
				Factory.Save();

				var invoice3 = declaration.Invoices.AddNew();
				var invoiceline31 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
				var invoiceline13 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
				declaration.DoMerge(notifier);

				AssertEquals("New line under existing invoice", (ZShort)5, invoiceline13.B3EntryLine.CL_LineNumber);
				AssertEquals("New line under new invoice", (ZShort)6, invoiceline31.B3EntryLine.CL_LineNumber);
			}
		}

		public void TestSetB3SubHeaderNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = CreateInvoiceHeader(declaration, "1", 1, TimeLimitUnitCodes.Codes.Day);
			var invoice2 = CreateInvoiceHeader(declaration, "2", 1, TimeLimitUnitCodes.Codes.Day);
			var invoice3 = CreateInvoiceHeader(declaration, "3", 1, TimeLimitUnitCodes.Codes.Month);
			var line1 = CreateInvoiceLine(invoice1, 1, "03", "US", "CN");
			var line2 = CreateInvoiceLine(invoice1, 2, "03", "US", "CA");
			var line3 = CreateInvoiceLine(invoice1, 3, "03", "CA", "CN");
			var line4 = CreateInvoiceLine(invoice2, 4, "03", "US", "CN");
			var line5 = CreateInvoiceLine(invoice3, 5, "02", "US", "CN");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("Line2", 2, line2.CA_B3SubHeaderNumber);
			AssertEquals("Line3", 3, line3.CA_B3SubHeaderNumber);
			AssertEquals("Line4", 1, line4.CA_B3SubHeaderNumber);
			AssertEquals("Line5", 4, line5.CA_B3SubHeaderNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("Line2", 2, line2.CA_B3SubHeaderNumber);
			AssertEquals("Line3", 3, line3.CA_B3SubHeaderNumber);
			AssertEquals("Line4", 1, line4.CA_B3SubHeaderNumber);
			AssertEquals("Line5", 4, line5.CA_B3SubHeaderNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.B3SubHeaderNumberForLVX);
			AssertEquals("Line2", 1, line2.B3SubHeaderNumberForLVX);
			AssertEquals("Line3", 2, line3.B3SubHeaderNumberForLVX);
			AssertEquals("Line4", 3, line4.B3SubHeaderNumberForLVX);
			AssertEquals("Line5", 4, line5.B3SubHeaderNumberForLVX);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("Line2", 1, line2.CA_B3SubHeaderNumber);
			AssertEquals("Line3", 2, line3.CA_B3SubHeaderNumber);
			AssertEquals("Line4", 3, line4.CA_B3SubHeaderNumber);
			AssertEquals("Line5", 4, line5.CA_B3SubHeaderNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("Line2", 1, line2.CA_B3SubHeaderNumber);
			AssertEquals("Line3", 2, line3.CA_B3SubHeaderNumber);
			AssertEquals("Line4", 3, line4.CA_B3SubHeaderNumber);
			AssertEquals("Line5", 4, line5.CA_B3SubHeaderNumber);
		}

		public void TestSetB3SubHeaderNumbersPerInvoiceSeq()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = CreateInvoiceHeader(declaration, "1", 1, TimeLimitUnitCodes.Codes.Day);
			var invoice2 = CreateInvoiceHeader(declaration, "3", 1, TimeLimitUnitCodes.Codes.Day);
			var invoice3 = CreateInvoiceHeader(declaration, "2", 1, TimeLimitUnitCodes.Codes.Month);
			var line1 = CreateInvoiceLine(invoice1, 1, "03", "US", "CN");
			var line2 = CreateInvoiceLine(invoice1, 2, "03", "US", "CA");
			var line3 = CreateInvoiceLine(invoice1, 3, "03", "CA", "CN");
			var line4 = CreateInvoiceLine(invoice2, 4, "03", "US", "CN");
			var line5 = CreateInvoiceLine(invoice3, 5, "02", "US", "CN");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;
			declaration.SetB3SubHeaderNumbers();
			AssertEquals("Line1", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("Line2", 2, line2.CA_B3SubHeaderNumber);
			AssertEquals("Line3", 3, line3.CA_B3SubHeaderNumber);
			AssertEquals("Line4", 1, line4.CA_B3SubHeaderNumber);
			AssertEquals("Line5", 4, line5.CA_B3SubHeaderNumber);
		}

		public void TestRenumberSubHeadersOnCopiedJob()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			ZString hsCode = "0123";
			ZString tariff1 = "7326909031";
			ZString auNumber = "1234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LineNo = 3;
			var line4 = header.JobComInvoiceLines.AddNew();
			line4.JI_LineNo = 4;
			var line5 = header.JobComInvoiceLines.AddNew();
			line5.JI_LineNo = 5;
			var line6 = header.JobComInvoiceLines.AddNew();
			line6.JI_LineNo = 6;
			var line7 = header.JobComInvoiceLines.AddNew();
			line7.JI_LineNo = 7;
			var line8 = header.JobComInvoiceLines.AddNew();
			line8.JI_LineNo = 8;
			var line9 = header.JobComInvoiceLines.AddNew();
			line9.JI_LineNo = 9;
			InitInvoiceLine(line1, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line2, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line3, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line4, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line5, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line6, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line7, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line8, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			InitInvoiceLine(line9, tariff1, hsCode, auNumber, ValueForDutyCodes.Codes.RelatedFirmsComputedValue, CalculationMethods.Codes.NoRemission);
			line1.CA_TreatmentCode = "01";
			line2.CA_TreatmentCode = "01";
			line3.CA_TreatmentCode = "02";
			line4.CA_TreatmentCode = "01";
			line5.CA_TreatmentCode = "02";
			line6.CA_TreatmentCode = "01";
			line7.CA_TreatmentCode = "02";
			line8.CA_TreatmentCode = "03";
			line9.CA_TreatmentCode = "01";
			declaration.DoMerge(notifier);
			Factory.Save();

			AssertEquals(1, line1.CA_B3SubHeaderNumber);
			AssertEquals(1, line2.CA_B3SubHeaderNumber);
			AssertEquals(2, line3.CA_B3SubHeaderNumber);
			AssertEquals(1, line4.CA_B3SubHeaderNumber);
			AssertEquals(2, line5.CA_B3SubHeaderNumber);
			AssertEquals(1, line6.CA_B3SubHeaderNumber);
			AssertEquals(2, line7.CA_B3SubHeaderNumber);
			AssertEquals(3, line8.CA_B3SubHeaderNumber);
			AssertEquals(1, line9.CA_B3SubHeaderNumber);

			var b2Declaration = declaration.GetNewCopyToB2Declaration();
			Factory.Save();

			var b2Line1 = b2Declaration.Invoices[0].JobComInvoiceLines[0];
			var b2Line2 = b2Declaration.Invoices[0].JobComInvoiceLines[1];
			var b2Line3 = b2Declaration.Invoices[0].JobComInvoiceLines[2];
			var b2Line4 = b2Declaration.Invoices[0].JobComInvoiceLines[3];
			var b2Line5 = b2Declaration.Invoices[0].JobComInvoiceLines[4];
			var b2Line6 = b2Declaration.Invoices[0].JobComInvoiceLines[5];
			var b2Line7 = b2Declaration.Invoices[0].JobComInvoiceLines[6];
			var b2Line8 = b2Declaration.Invoices[0].JobComInvoiceLines[7];
			var b2Line9 = b2Declaration.Invoices[0].JobComInvoiceLines[8];

			b2Line4.CA_TreatmentCode = "02";
			b2Line6.CA_TreatmentCode = "04";
			b2Line7.CA_TreatmentCode = "04";
			b2Line9.CA_TreatmentCode = "05";
			b2Declaration.DoMerge(notifier);
			Factory.Save();

			AssertEquals(1, b2Line1.CA_B3SubHeaderNumber);
			AssertEquals(1, b2Line2.CA_B3SubHeaderNumber);
			AssertEquals(2, b2Line3.CA_B3SubHeaderNumber);
			AssertEquals(2, b2Line4.CA_B3SubHeaderNumber);
			AssertEquals(2, b2Line5.CA_B3SubHeaderNumber);
			AssertEquals(4, b2Line6.CA_B3SubHeaderNumber);
			AssertEquals(4, b2Line7.CA_B3SubHeaderNumber);
			AssertEquals(3, b2Line8.CA_B3SubHeaderNumber);
			AssertEquals(5, b2Line9.CA_B3SubHeaderNumber);
		}

		JobComInvoiceHeader CreateInvoiceHeader(JobDeclaration declaration, ZString invoiceNumber, ZInt timeLimit, ZString timeLimitCode)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = invoiceNumber;
			invoice.CA_TimeLimit = timeLimit;
			invoice.CA_TimeLimitCode = timeLimitCode;
			return invoice;
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, ZShort lineNo, ZString treatmentCode, ZString countryOfOrigin, ZString countryOfExport)
		{
			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line.JI_LineNo = lineNo;
			line.CA_TreatmentCode = treatmentCode;
			line.JI_CountryOfOrigin = countryOfOrigin;
			line.CA_RN_NKExport = countryOfExport;
			return line;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.UseBaseMergeStrategyForTesting = true;
			return jobDeclaration;
		}

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		BaseJobComInvoiceLine line;

		#region LineMergerForTesting

		class LineMergerForTesting : LineMerger
		{
			public LineMergerForTesting(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public EntryCreationStrategy[] GetEntryCreationStrategies_Exposed()
			{
				return GetEntryCreationStrategies();
			}
		}

		#endregion
	}
}
