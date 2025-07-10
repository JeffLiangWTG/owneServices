using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestGetStatementCodeViaExemptCodeForSIMA()
		{
			AssertEquals(ZString.Empty, CusEntryLine.GetStatementCodeViaExemptCode(ZString.Empty));
			AssertEquals("N", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C10));
			AssertEquals("U", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C20));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C30));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C31));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C32));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C40));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C50));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C51));
			AssertEquals("S", CusEntryLine.GetStatementCodeViaExemptCode(SIMACodes.Codes.C52));
		}

		public void TestAmendmentDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();

			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;

			var action = Factory.New<CADCorrectionMessageSendingAction>();
			action.CSI_Type = Customs.Common.CA.CusSupportingInfoTypeList.Codes.CadCorrectionMessageSendingAction;
			action.CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			action.CSI_ParentID = entryLine.PK;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationLookupUsingClassificationDescriptionAlways;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.CustomsEntryHeaders[0].CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Factory.Save();
			AssertEquals(1, entryLine.AmendmentDetails.Count);
		}

		public void TestFeesAndCusFees()
		{
			var cusEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			var cw1CusEntryLineFee = Factory.New<CusEntryLineFee>();
			cw1CusEntryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			cw1CusEntryLineFee.CF_ChargeType = "GST";
			cw1CusEntryLineFee.CF_ChargeAmount = 1.1m;
			cw1CusEntryLineFee.CF_CL = cusEntryLine.PK;

			var cusEntryLineFee = Factory.New<CusEntryLineFee>();
			cusEntryLineFee.CF_ChargeType = "DTY";
			cusEntryLineFee.CF_ChargeAmount = 1.2m;
			cusEntryLineFee.CF_CL = cusEntryLine.PK;

			var cusCusEntryLineFee = Factory.New<CusEntryLineFee>();
			cusCusEntryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			cusCusEntryLineFee.CF_ChargeType = "DTY";
			cusCusEntryLineFee.CF_ChargeAmount = 2.2m;
			cusCusEntryLineFee.CF_CL = cusEntryLine.PK;
			Factory.Save();

			AssertEquals(2, cusEntryLine.Fees.Count);
			AssertEquals(1, cusEntryLine.ConfirmedFees.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cw1CusEntryLineFee, cusEntryLineFee }, cusEntryLine.Fees);
			AssertContainsExactElementsInAnyOrder(new[] { cusCusEntryLineFee }, cusEntryLine.ConfirmedFees);

			var cw1Fee = cusEntryLine.Fees.AddNew();
			AssertEquals(CusEntryLineFeeSourceCodeList.Codes.CW1, cw1Fee.CF_Source);

			var cusFee = cusEntryLine.ConfirmedFees.AddNew();
			AssertEquals(CusEntryLineFeeSourceCodeList.Codes.CUS, cusFee.CF_Source);
		}

		public void TestDutyFeeChangedSinceLastResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CUD, 1m);
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 2m);
			Factory.Save();
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			Factory.Save();
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 1m);
			Factory.Save();
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			Factory.Save();
			Assert(!entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.RemoveAndDeleteAll();
			entryLine.ConfirmedFees.RemoveAndDeleteAll();
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 3m);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 3m);
			Factory.Save();
			Assert(!entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.ADD, 1m);
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CVD, 0.5m);
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.SUR, 3m);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 4.5m);
			Factory.Save();
			Assert(!entryLine.DutyFeeChangedSinceLastResponse);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLine to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestDescriptionWhenMergedByClassificationLookupUsingClassificationDescriptionAlways()
		{
			const string tarrifCode = "0000.00.00.00Y";
			var classification = Factory.New<CusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationLookupUsingClassificationDescriptionAlways;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.JI_CC = classification.PK;
			line1.JI_Description = "LINE";

			AssertDescription(declaration, B3MergeByList.Codes.ClassificationLookupUsingClassificationDescriptionAlways, classification.CC_Description);

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = tarrifCode;
			line2.JI_CC = classification.PK;
			line2.JI_Description = "LINE 2";

			AssertDescription(declaration, B3MergeByList.Codes.ClassificationLookupUsingClassificationDescriptionAlways, classification.CC_Description);
		}

		public void TestDescriptionWhenMergedByUsingProductNumberInDescription()
		{
			const string tarrifCode = "0000.00.00.00Y";
			const string descriptionWithProductNumber = "NUM - PART";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_Desc = "PART";
			part.OP_PartNum = "NUM";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var line1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.SetPartForTesting(part);

			var line2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line2.JI_Tariff = tarrifCode;
			line2.SetPartForTesting(part);

			AssertDescription(declaration, B3MergeByList.Codes.ProductNumberUsingProductNumberInDescription, descriptionWithProductNumber);
			AssertDescription(declaration, B3MergeByList.Codes.ProductNumber, part.OP_Desc);
			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, descriptionWithProductNumber);
			AssertDescription(declaration, B3MergeByList.Codes.NotMerge, part.OP_Desc);
			AssertDescription(declaration, B3MergeByList.Codes.ClassificationTariff, part.OP_Desc);
			AssertDescription(declaration, B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices, part.OP_Desc);
		}

		public override void TestDescriptionFromLines()
		{
			const string tarrifCode = "0000.00.00.00Y";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_Desc = "PART1";
			part1.OP_PartNum = "NUM1";

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_Desc = "PART2";
			part2.OP_PartNum = "NUM2";

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_Desc = "PART3";
			part3.OP_PartNum = "NUM3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.JI_LinePrice = 30;
			line1.SetPartForTesting(part1);

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = tarrifCode;
			line2.JI_LinePrice = 150;
			line2.SetPartForTesting(part2);

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = tarrifCode;
			line3.JI_LinePrice = 100;
			line3.SetPartForTesting(part3);

			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, "NUM1 - PART1");
			AssertDescription(declaration, B3MergeByList.Codes.ClassificationTariff, "PART2");
			AssertDescription(declaration, B3MergeByList.Codes.ClassificationLookup, "PART2");
			AssertDescription(declaration, B3MergeByList.Codes.ProductNumber, "PART2");
			AssertDescription(declaration, B3MergeByList.Codes.ProductNumberUsingProductNumberInDescription, "NUM2 - PART2");
			AssertDescription(declaration, B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices, "PART2");
		}

		public void TestDescriptionForByUsingNotMergeUsingProductNumberInDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1230000000", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "LONG TARIFF DESCRIPTION.");
			helper.CreateTariffUOM(tariff, "CU1", "KGM");

			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "2300123490";
			classification.CC_Description = "CLASS TEST DESCRIPTION";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_Desc = "PART1";
			part1.OP_PartNum = "NUM1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();

			line1.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, tariff.ZZ1_Description);

			line1.JI_CC = classification.PK;
			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, classification.CC_Description);

			line1.JI_Description = "TEST - DESCRIPTION";
			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, "TEST - DESCRIPTION");

			line1.SetPartForTesting(part1);
			AssertDescription(declaration, B3MergeByList.Codes.NotMergeUsingProductNumberInDescription, "NUM1 - PART1");
		}

		public void TestValueForTaxOfIDutyAndTaxData()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.CA_ValueForTax = 10m;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.CA_ValueForTax = 20m;
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var dutyAndTaxData = (IDutyAndTaxData)entryLine;
			dutyAndTaxData.ValueForTax = 30m;
			AssertEquals(10m, invoiceLine1.CA_ValueForTax);
			AssertEquals(20m, invoiceLine2.CA_ValueForTax);
		}

		public void TestSequenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			AssertEquals("[0,0]", entryLine.SequenceNumber);

			entryLine.CL_GoodsShipmentSequence = 3;
			entryLine.CL_CommoditySequence = 4;
			AssertEquals("[3,4]", entryLine.SequenceNumber);
		}

		void AssertDescription(JobDeclaration declaration, string mergeBy, string expectedDescription)
		{
			declaration.CA_MergeBy = mergeBy;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Description when merge by " + mergeBy, expectedDescription, entryLine.Description);
		}

		protected override Type ExpectedTypeOfFees => typeof(CW1CusEntryLineFeeCollection);

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}

		protected override ZString ExpectedFallbackEntrylineDescription
		{
			get { return InvoiceLinePartClassificationTariffDescriptionSyncroniserTest.TariffDescriptionCore; }
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = (JobDeclaration)base.ImportJobDeclaration;
				if (CurrentTestName.EndsWith("TestDescriptionFromLines"))
				{
					result.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
					result.UseBaseMergeStrategyForTesting = true;
				}
				return result;
			}
		}

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		internal const string refFilesData = @"
RA108466939090        9960
RA20846693909090020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
RA3084669390902009080199999999N   N2009-06-CPFTA
RA40V001400000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA520012008010199999999Y1V000500000   NORMAL RATE                                                 N
RA54E901991040399999999V000300000   ALL OTHER CONDITIONALLY EXEMPT GOODS                        N
";

		public void TestIClassificationLine1Properties()
		{
			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refFilesData))));
			var factory = ((IFactoryProvider)parser).Factory;
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5);

			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "8466939090", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "LTR");
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 123.45;
			declaration.JE_TotalWeightUnit = "KG";

			var invoice = declaration.Invoices.AddNew();
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 2000m, 222m);
			line.JI_InvoiceQuantity = 10.1m;
			line.JI_InvoiceUQ = "PCE";
			var line2 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line2, 2, 700m, 80m);
			line2.JI_InvoiceQuantity = 20.2m;
			line2.JI_InvoiceUQ = "PCE";

			var line3 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line3, 3, 1, 2000m, 0m, 0, 2345, Core.Constants.Weight.Kilograms, 222m);
			var tariff = JobComInvoiceLineTestHelper.AddTariffRecord(line3);
			line3.JI_InvoiceQuantity = 10.1m;
			line3.JI_InvoiceUQ = "PCE";
			declaration.ResumeApportionment();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryHeader.IsB3GrossWeightSet = true;
			var entryLine = entryHeader.AllEntryLines[0];
			AssertEquals("CL_DutyPercent should be first not empty duty rate", 14m, entryLine.CL_DutyPercent); //Test for DoMergeInvoiceLine
			AssertEquals("JI_Calc_DutyAmount. Duties.", 280m, line.JI_Calc_DutyAmount);
			AssertEquals("JI_Calc_SIMADutyAmount. SIMAs.", 222m, line.JI_Calc_SIMADutyAmount);
			AssertEquals("JI_Calc_GSTVATAmount. GSTs", 125.10m, line.JI_Calc_GSTVATAmount);
			AssertEquals("JI_Calc_ExciseTaxesAmount", 0m, line.JI_Calc_ExciseTaxesAmount);

			IClassificationLine1 classLine = entryLine;
			AssertEquals("B3LineNumber", entryLine.CL_LineNumber, classLine.B3LineNumber);

			var amount = entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency, amount * -1);
			AssertEquals("RecordIdentifier", MessageConstants.B3RecordIdentifiers.Negative, classLine.RecordIdentifier);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency, amount);
			AssertEquals("RecordIdentifier", MessageConstants.B3RecordIdentifiers.Positive, classLine.RecordIdentifier);

			AssertEquals("B3SubHeaderNumber", 1, classLine.B3SubHeaderNumber);
			AssertEquals("ClassificationNumber", entryLine.RandomLine.JI_Tariff, classLine.ClassificationNumber);

			entryLine.RandomLine.CA_ValueForDutyCode = string.Empty;
			AssertEquals("ValueForDutyCode", invoice.CA_ValueForDutyCode, classLine.ValueForDutyCode);
			entryLine.RandomLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			AssertEquals("ValueForDutyCode", entryLine.RandomLine.CA_ValueForDutyCode, classLine.ValueForDutyCode);

			AssertEquals("TariffCode", entryLine.RandomLine.CA_99TariffCode, classLine.TariffCode);
			AssertEquals("ValueForCurrency", 4700m, classLine.ValueForCurrency);
			AssertEquals("ValueForDuty", 4700m, classLine.ValueForDuty);
			AssertEquals("ValueForTax", 5882.00m, classLine.ValueForTax);
			AssertEquals("AuthorityNumber", entryLine.RandomLine.CA_AuthorityNumber, classLine.AuthorityNumber);
			AssertEquals("TRSNumber", entryLine.RandomLine.CA_TRSNumber, classLine.TRSNumber);
			AssertEquals("PartNumberDescriptions", entryLine.RandomLine.JI_Description, classLine.PartNumberDescriptions[0]);
			AssertEquals("InvoiceCrossReferences count", 1, classLine.InvoiceCrossReferences.Count());
			AssertEquals("SIMAAssessment", 524m, classLine.SIMAAssessment);
			AssertEquals("ExciseTaxRate", 0m, classLine.ExciseTaxRate);
			AssertEquals("ExciseTaxRateToPrint", 0m, classLine.ExciseTaxRateToPrint);
			AssertEquals("ExciseTaxAmount", 0m, classLine.ExciseTaxAmount);
			AssertEquals("RateOfGST", 5m, classLine.RateOfGST);
			AssertEquals("GSTAmount", 294.10m, classLine.GSTAmount);
			AssertEquals("CountOfConsolidatedLines", entryLine.InvoiceLines.Count, classLine.CountOfConsolidatedLines);
			AssertEquals("CustomsQuantity", entryLine.CustomsQuantity, classLine.CustomsQuantity);
			AssertEquals("CustomsUnitQty", entryLine.CustomsUnitQty, classLine.CustomsUnitQty);
			AssertEquals("InvoiceQuantity", entryLine.InvoiceQuantity, classLine.InvoiceQuantity);
			AssertEquals("InvoiceUQ", entryLine.InvoiceUQ, classLine.InvoiceUQ);
			AssertEquals("TotalLinePrice", entryLine.TotalLinePrice, classLine.TotalLinePrice);
			AssertEquals("CustomsValue", entryLine.CustomsValue, classLine.CustomsValue);
			AssertEquals("FOB", entryLine.FOB, classLine.FOB);

			var clssLines = new List<IClassificationLine2>(classLine.ClassificationLines);
			AssertEquals("ClassificationLines count", 1, clssLines.Count);
			AssertClassificationLine2(clssLines[0], classLine.B3LineNumber, entryLine.RandomLine.JI_CustomsUnitQty, 0, 123, 14, 658, RateTypes.Codes.AdValorem, 40.4m, entryLine.RandomLine.JI_InvoiceUQ);
			AssertEquals("RelevantLine", line.PK, classLine.RelevantLine.PK);
		}

		public void TestIClassificationLine1Properties_DutiesAndTaxes()
		{
			#region Create Test Data
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();
			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			sur1.C1_ExemptCode = SIMACodes.Codes.C20;
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			add1.C1_ExemptCode = SIMACodes.Codes.C20;
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			cvd1.C1_ExemptCode = SIMACodes.Codes.C20;
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			saf1.C1_ExemptCode = SIMACodes.Codes.C10;
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			var exd1 = line1.DutiesAndTaxes.AddNew();
			exd1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd1.C1_Amount = 47m;
			exd1.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var cpt2 = line2.DutiesAndTaxes.AddNew();
			cpt2.C1_Override = true;
			cpt2.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt2.C1_Amount = 33m;
			var cta2 = line2.DutiesAndTaxes.AddNew();
			cta2.C1_Override = true;
			cta2.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta2.C1_Amount = 34m;
			var dty2 = line2.DutiesAndTaxes.AddNew();
			dty2.C1_Override = true;
			dty2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty2.C1_Amount = 35m;
			dty2.C1_Code = "AB";
			var exs2 = line2.DutiesAndTaxes.AddNew();
			exs2.C1_Override = true;
			exs2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs2.C1_Amount = 36m;
			exs2.C1_Code = "BC";
			var sur2 = line2.DutiesAndTaxes.AddNew();
			sur2.C1_Override = true;
			sur2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur2.C1_Amount = 37m;
			sur2.Quantity = 38m;
			sur2.C1_UnitOfMeasure = "M3";
			sur2.C1_Code = "CD";
			sur2.C1_ExemptCode = SIMACodes.Codes.C20;
			var add2 = line2.DutiesAndTaxes.AddNew();
			add2.C1_Override = true;
			add2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add2.C1_Amount = 39m;
			add2.Quantity = 40m;
			add2.C1_UnitOfMeasure = "M4";
			add2.C1_Code = "DE";
			add2.C1_ExemptCode = SIMACodes.Codes.C20;
			var cvd2 = line2.DutiesAndTaxes.AddNew();
			cvd2.C1_Override = true;
			cvd2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd2.C1_Amount = 43;
			cvd2.Quantity = 44;
			cvd2.C1_UnitOfMeasure = "M5";
			cvd2.C1_Code = "EF";
			cvd2.C1_ExemptCode = SIMACodes.Codes.C20;
			var saf2 = line2.DutiesAndTaxes.AddNew();
			saf2.C1_Override = true;
			saf2.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf2.C1_Amount = 45m;
			saf2.C1_Code = "FG";
			saf2.C1_ExemptCode = SIMACodes.Codes.C10;
			var ded2 = line2.Charges.AddNew();
			ded2.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			ded2.J7_Amount = 46m;
			ded2.J7_RX_NKCurrency = "USD";
			var exd2 = line2.DutiesAndTaxes.AddNew();
			exd2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd2.C1_Amount = 47m;
			exd2.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			#endregion

			var classificationLine1 = entryLine as IClassificationLine1;
			AssertEquals("SalesTaxAmount", 66m, classificationLine1.SalesTaxAmount);
			AssertEquals("CTAAmount", 68m, classificationLine1.CTAAmount);
			AssertEquals("DeductionChargeAmountAndCurrency.Amount", 92m, classificationLine1.DeductionChargeAmountAndCurrency.Amount);
			AssertEquals("DeductionChargeAmountAndCurrency.Currency.RX_Code", "USD", classificationLine1.DeductionChargeAmountAndCurrency.Currency.RX_Code);
			AssertEquals("CustomsDutyCode", "AB", classificationLine1.CustomsDutyCode);
			AssertEquals("ExciseCode", "BC", classificationLine1.ExciseCode);
			AssertEquals("ADDAmount", 78m, classificationLine1.ADDAmount);
			AssertEquals("ADDQuantity", 80m, classificationLine1.ADDQuantity);
			AssertEquals("ADDUnitOfMeasure", "M4", classificationLine1.ADDUnitOfMeasure);
			AssertEquals("ADDCode", "DE", classificationLine1.ADDCode);
			AssertEquals("ADDIsOverride", true, classificationLine1.ADDIsOverride);
			AssertEquals("HasADD", true, classificationLine1.HasADD);
			AssertEquals("CVDAmount", 86m, classificationLine1.CVDAmount);
			AssertEquals("CVDQuantity", 88m, classificationLine1.CVDQuantity);
			AssertEquals("CVDUnitOfMeasure", "M5", classificationLine1.CVDUnitOfMeasure);
			AssertEquals("CVDCode", "EF", classificationLine1.CVDCode);
			AssertEquals("CVDIsOverride", true, classificationLine1.CVDIsOverride);
			AssertEquals("HasCVD", true, classificationLine1.HasCVD);
			AssertEquals("SafeguardCode", "FG", classificationLine1.SafeguardCode);
			AssertEquals("SafeguardStatementCode", "N", classificationLine1.SafeguardStatementCode);
			AssertEquals("HasSafeguard", true, classificationLine1.HasSafeguard);
			AssertEquals("SIMAStatementCode", "U", classificationLine1.SIMAStatementCode);
			AssertEquals("HasSurtax", true, classificationLine1.HasSurtax);
			AssertEquals("SurtaxQuantity", 76m, classificationLine1.SurtaxQuantity);
			AssertEquals("SurtaxStatementCode", "U", classificationLine1.SurtaxStatementCode);
			AssertEquals("SurtaxUnitOfMeasure", "M3", classificationLine1.SurtaxUnitOfMeasure);
			AssertEquals("SurtaxCode", "CD", classificationLine1.SurtaxCode);
			AssertEquals("ExciseDutyAmount", 94m, classificationLine1.ExciseDutyAmount);
		}

		public void TestWeightInKGMOfClassificationLine2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 123.45;
			declaration.JE_TotalWeightUnit = "KG";

			var invoice = declaration.Invoices.AddNew();
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 2000m, 222m);
			var gst = line.DutiesAndTaxes.AddNew();
			gst.C1_Override = true;
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_Amount = 33m;
			var duty = line.DutiesAndTaxes.AddNew();
			duty.C1_Override = true;
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty.C1_Amount = 22m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var b3ImportMessageWrapper = new B3ImportMessageWrapper(entryHeader);
			var classLine = b3ImportMessageWrapper.PositiveClassificationLines.First().ClassificationLines.First();
			AssertEquals(123m, classLine.WeightInKGM);
			var classLine1 = b3ImportMessageWrapper.PositiveClassificationLines.First().ClassificationLines.First();
			AssertEquals("It will still be recalculated on the second access", 123m, classLine1.WeightInKGM);

			var lowValueShipmentsMessageWrapper = new LowValueShipmentsMessageWrapper(entryHeader);
			var classLine2 = lowValueShipmentsMessageWrapper.PositiveClassificationLines.First().ClassificationLines.First();
			AssertEquals(123m, classLine2.WeightInKGM);
			var classLine3 = lowValueShipmentsMessageWrapper.PositiveClassificationLines.First().ClassificationLines.First();
			AssertEquals("It will still be recalculated on the second access", 123m, classLine3.WeightInKGM);
		}

		public void TestIClassificationLine1SIMAandDirectGSTProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 123.45;
			declaration.JE_TotalWeightUnit = "KG";
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 2000m, 222m);
			var line2 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line2, 2, 700m, 80m);
			line2.DutyAndTaxManager.SIMADuties.First().C1_ExemptCode = SIMACodes.Codes.C32;

			var gst = line2.DutiesAndTaxes.AddNew();
			gst.C1_Override = true;
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_Amount = 33m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryHeader.IsB3GrossWeightSet = true;

			var entryLine = entryHeader.AllEntryLines[0];
			IClassificationLine1 classLine = entryLine;
			AssertEquals("SIMAAssessment", 222m, classLine.SIMAAssessment);
			AssertEquals("SIMAAmount", 222m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount));
			AssertEquals("Non billable SIMAAmount", 0m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount));

			entryLine = entryHeader.AllEntryLines[1];
			classLine = entryLine;
			AssertEquals("SIMAAssessment", 80m, classLine.SIMAAssessment);
			AssertEquals("SIMAAmount", 0m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount));
			AssertEquals("Non billable SIMAAmount", 80m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount));

			AssertEquals("GST amount", 0m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTAmount));
			AssertEquals("DirectGST amount", 33m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount));
			AssertEquals("Total GST", 33m, classLine.GSTAmount);
			AssertEquals("Entry line GST", 33m, entryLine.GSTVATAmount);
			AssertEquals("Entry line deferred GST", 0m, entryLine.GSTVATDeferred);
		}

		internal static void AssertClassificationLine2(IClassificationLine2 line2, ZInt number, ZString uom, decimal qty, decimal weight, decimal rate, decimal amount, ZString rateType, decimal invoiceQty, ZString invUM)
		{
			AssertEquals("B3LineNumber", number, line2.B3LineNumber);
			AssertEquals("UnitOfMeasureCode", uom, line2.UnitOfMeasureCode);
			AssertEquals("ClassificationLineQuantity", qty, line2.ClassificationLineQuantity);
			AssertEquals("WeightInKGM", weight, line2.WeightInKGM);
			AssertEquals("CustomsDutyRate", rate, line2.CustomsDutyRate);
			AssertEquals("CustomsDutyAmount", amount, line2.CustomsDutyAmount);
			AssertEquals("CustomsDutyRateType", rateType, line2.CustomsDutyRateType);
			AssertEquals("PreviousTransactionNumber", "", line2.PreviousTransactionNumber);
			AssertEquals("PreviousLineNumber", 0, line2.PreviousLineNumber);
			AssertEquals("InvoiceUnitOfMeasureCode", invUM, line2.InvoiceUnitOfMeasureCode);
			AssertEquals("ClassificationLineInvoiceQuantity", invoiceQty, line2.ClassificationLineInvoiceQuantity);
		}

		[ExpectNoExceptions]
		public void TestB3GrossWeight()
		{
			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refFilesData))));
			var factory = ((IFactoryProvider)parser).Factory;

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 700;
			declaration.JE_TotalWeightUnit = "LB";

			var invoice = declaration.Invoices.AddNew();
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 2000m, 222m);
			line.JI_InvoiceQuantity = 10.1m;
			line.JI_InvoiceUQ = "PCE";

			declaration.ResumeApportionment();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryHeader.IsB3GrossWeightSet = true;
			var entryLine = entryHeader.AllEntryLines[0];

			IClassificationLine1 classLine = entryLine;
			var clssLines = new List<IClassificationLine2>(classLine.ClassificationLines);
			AssertEquals("ClassificationLines count", 1, clssLines.Count);
			AssertEquals("WeightInKGM Should be 318", new ZDecimal(318), clssLines[0].WeightInKGM);

			declaration.JE_TotalWeightUnit = "GM";
			declaration.DoMerge();
			entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			entryHeader.IsB3GrossWeightSet = true;
			entryLine = entryHeader.AllEntryLines[0];
			classLine = entryLine;
			clssLines = new List<IClassificationLine2>(classLine.ClassificationLines);
			AssertEquals("ClassificationLines count", 1, clssLines.Count);
			AssertEquals("WeightInKGM Should be 1", new ZDecimal(1), clssLines[0].WeightInKGM);
		}

		public void TestInvoiceCrossReferences()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var lines = invoice.JobComInvoiceLines;

			const string classNum1 = "8201.10.00 10";
			const string classNum2 = "8201.40.10 00";

			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 1, 1, classNum1, 110m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 2, 1, classNum1, 100m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 3, 2, classNum1, 120m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 4, 2, classNum1, 130m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 5, 3, classNum1, 150m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 6, 3, classNum2, 140m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 7, 3, classNum2, 160m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			foreach (JobComInvoiceLine line in lines)
			{
				AssertEquals("CA_PageRelativeLineNumber is not set", 0, line.InvoiceCrossReferencePageLineNumber);
			}

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var wrapper = new B3ImportMessageWrapper(entryHeader);

			var classificationLines = wrapper.PositiveClassificationLines.ToList();
			AssertEquals("ClassificationLines.Count", 2, classificationLines.Count);

			var classLine1 = classificationLines[0];
			var invoiceCrossReferences = classLine1.InvoiceCrossReferences.ToList();
			AssertEquals("classLine1.InvoiceCrossReferences count", 3, invoiceCrossReferences.Count);
			AssertInvoiceCrossReferences(invoiceCrossReferences[0], 1, 0, 210m);
			AssertInvoiceCrossReferences(invoiceCrossReferences[1], 2, 0, 250m);
			AssertInvoiceCrossReferences(invoiceCrossReferences[2], 3, 1, 150m);

			var classLine2 = classificationLines[1];
			invoiceCrossReferences = classLine2.InvoiceCrossReferences.ToList();
			AssertEquals("classLine2.InvoiceCrossReferences count", 2, invoiceCrossReferences.Count);
			AssertInvoiceCrossReferences(invoiceCrossReferences[0], 3, 2, 140m);
			AssertInvoiceCrossReferences(invoiceCrossReferences[1], 3, 3, 160m);

			var invoiceLines = lines.Cast<JobComInvoiceLine>();
			AssertInvoiceCrossReferencePageLineNumber(0, 1, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(0, 2, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(0, 3, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(0, 4, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(1, 5, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(2, 6, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(3, 7, invoiceLines);

			AssertEquals("line1.CA_PageRelativeLineNumber", 1, lines[0].CA_PageRelativeLineNumber);
			AssertEquals("line2.CA_PageRelativeLineNumber", 2, lines[1].CA_PageRelativeLineNumber);
			AssertEquals("line3.CA_PageRelativeLineNumber", 1, lines[2].CA_PageRelativeLineNumber);
			AssertEquals("line4.CA_PageRelativeLineNumber", 2, lines[3].CA_PageRelativeLineNumber);
			AssertEquals("line5.CA_PageRelativeLineNumber", 1, lines[4].CA_PageRelativeLineNumber);
			AssertEquals("line6.CA_PageRelativeLineNumber", 2, lines[5].CA_PageRelativeLineNumber);
			AssertEquals("line7.CA_PageRelativeLineNumber", 3, lines[6].CA_PageRelativeLineNumber);

			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 8, 2, classNum2, 130m);
			declaration.DoMerge();
			wrapper = new B3ImportMessageWrapper(entryHeader);
			classificationLines = wrapper.PositiveClassificationLines.ToList();
			AssertEquals("ClassificationLines.Count", 2, classificationLines.Count);

			invoiceLines = lines.Cast<JobComInvoiceLine>();
			AssertInvoiceCrossReferencePageLineNumber(0, 1, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(0, 2, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(1, 3, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(2, 4, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(3, 8, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(1, 5, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(2, 6, invoiceLines);
			AssertInvoiceCrossReferencePageLineNumber(3, 7, invoiceLines);
		}

		public void TestInvoiceCrossReferences_LuxuryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;
			line.CA_ApplyLuxuryTax = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var wrapper = new B3ImportMessageWrapper(entryHeader);
			var classificationLines = wrapper.PositiveClassificationLines.ToList();
			AssertEquals("ClassificationLines.Count", 2, classificationLines.Count);
			var classLine = classificationLines[1];
			var invoiceCrossReferences = classLine.InvoiceCrossReferences.ToList();
			AssertEquals(0.01m, invoiceCrossReferences[0].InvoiceValue);
		}

		void AssertInvoiceCrossReferencePageLineNumber(int invoiceCrossReferencePageLineNumber, int ji_LineNo, IEnumerable<JobComInvoiceLine> lines)
		{
			AssertEquals($"line{ji_LineNo}.InvoiceCrossReferencePageLineNumber", invoiceCrossReferencePageLineNumber, lines.FirstOrDefault(x => x.JI_LineNo == ji_LineNo)?.InvoiceCrossReferencePageLineNumber);
		}

		static void AssertInvoiceCrossReferences(IInvoiceCrossReference reference, int pageNumber, int lineNumber, decimal linePrice)
		{
			AssertEquals("InvoicePageNumber", pageNumber, reference.InvoicePageNumber);
			AssertEquals("InvoiceLineNumber", lineNumber, reference.InvoiceLineNumber);
			AssertEquals("InvoiceValue", linePrice, reference.InvoiceValue);
		}

		public void TestIDLMDetailLineProperies()
		{
			var helper = new DeclarationTestHelper(Factory, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = helper.NewZealand.Code;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "08010101";
			invoiceLine.JI_LinePrice = 4000m;
			invoiceLine.JI_CustomsQuantity = 1023.30m;
			invoiceLine.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.Kilogram;
			invoiceLine.JI_Description = "A VERY FUNNY LOOKING DOG";
			invoiceLine.CA_ConveyanceIdentificationNumber = "VIN2342342";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("declaration.CustomsEntryHeaders", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("entry.MergedLines.Count", 1, entry.MergedLines.Count);
			CusEntryLine entryLine = entry.MergedLines[0];
			IDLMDetailLine detailLine = entryLine;

			AssertEquals("CountryOfOrigin", helper.NewZealand.RN_Desc, detailLine.CountryOfOrigin);
			AssertEquals("ProvinceOfOrigin", ZString.Empty, detailLine.ProvinceOfOrigin);
			invoiceLine.JI_CountryOfOrigin = helper.Canada.Code;
			invoiceLine.JI_StateOrRegionOfOrigin = CanadianProvinceList.Codes.NewfoundlandAndLabrador;
			AssertEquals("CountryOfOrigin", helper.Canada.RN_Desc, detailLine.CountryOfOrigin);
			AssertEquals("ProvinceOfOrigin", CanadianProvinceList.Descriptions.NewfoundlandAndLabrador, detailLine.ProvinceOfOrigin);

			AssertEquals("HarmonizedSystemCode", "08010101", detailLine.HarmonizedSystemCode);
			AssertEquals("ProductDescription", "A VERY FUNNY LOOKING DOG", detailLine.ProductDescription);
			AssertEquals("ConveyanceIdentificationNumber", "VIN2342342", detailLine.ConveyanceIdentificationNumber);
			AssertEquals("Quantity", 1023.30m, detailLine.Quantity);
			AssertEquals("UnitOfMeasure", UnitOfMeasureListForDLM.Descriptions.Kilogram, detailLine.UnitOfMeasure);
			AssertEquals("ValueFOBPointOfExit", invoiceLine.JI_Calc_FOB, detailLine.ValueFOBPointOfExit);
		}

		public void TestDutiesAndTaxesForMergedLines1()
		{
			#region Ref Files Data

			const string refFilesData = @"
RA108466939090        9960
RA20846693909090020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
RA3084669390902009080199999999N   N2009-06-CPFTA
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000012N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000013N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000023N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000024N
RA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
RA520012008010199999999Y1V000500000   NORMAL RATE                                                 N
RA70996020090801999999992009080199999999YR2009-CPFTA NNN
RA80Y02F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y07F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y08F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y09F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y10F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y11F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y12F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y13F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y14F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y21F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y22F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y23F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y24F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
RA80Y25F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 000000000000000000000000000N
";

			#endregion

			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refFilesData))));
			var factory = ((IFactoryProvider)parser).Factory;
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5);
			var helper = new DeclarationTestHelper(factory, true);
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Japan;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;

			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 6259.74m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 83.12m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 363.74m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);

			IClassificationLine1 classLine1 = entryHeader.MergedLines[0];
			AssertEquals("classLine1.ValueForCurrency", 6706.60m, classLine1.ValueForCurrency);
			AssertEquals("classLine1.ValueForDuty", 6706.60m, classLine1.ValueForDuty);
			AssertEquals("classLine1.DutyLines.Count", 1, classLine1.ClassificationLines.Count());
			AssertEquals("classLine1.CustomsDutyAmount", 0m, classLine1.ClassificationLines.First().CustomsDutyAmount);
			AssertEquals("classLine1.ValueForTax", 6706.60m, classLine1.ValueForTax);
			AssertEquals("classLine1.SIMAAssessment", 0m, classLine1.SIMAAssessment);
			AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
			AssertEquals("classLine1.GSTAmount", 335.33m, classLine1.GSTAmount);

			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			declaration.DoMerge();
			var entryLine = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0];
			AssertEquals("DirectGST amount", 335.33m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount));
			AssertEquals("GST amount", 0m, entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTAmount));
			AssertEquals("GST amount", 335.33m, ((IClassificationLine1)entryLine).GSTAmount);
		}

		public void TestDutiesAndTaxesForMergedLines2()
		{
			#region Ref Files Data

			const string classRefFileData = @"
GA20820110001090020090801999999992009-06-CPFTANMBNN2009-06-CPFTA2009080199999999NN
GA3082011000102009080199999999N   N2009-06-CPFTA
GA40V000500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
GA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
GA40V000300000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000012N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000013N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000023N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000024N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
GA5082011000102009080199999999N001
GA20820140100090020090801999999992009-06-CPFTANMBNN2009-06-CPFTA2009080199999999NN
GA3082014010002009080199999999N   N2009-06-CPFTA
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
GA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000012N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000013N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000023N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000024N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
GA5082014010002009080199999999N001
GA20820160100090020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
GA3082016010002009080199999999N   N2009-06-CPFTA
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
GA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000009N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000012N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000013N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000023N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000024N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
GA5082016010002009080199999999N001
";

			const string gstRefFileData = "GC100012008010199999999Y1V000500000   NORMAL RATE                                                 N";

			#endregion

			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parser = new CACTestingDataHelper.CadexMessageProcessor();
				parser.ProcessAA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(classRefFileData))));
				parser.ProcessAC(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(gstRefFileData))));
				var factory = ((IFactoryProvider)parser).Factory;
				JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5);
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "JHARD7-1XX";
				invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
				invoice.CA_USStateOfExport = USStatesList.Codes.Michigan;
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9955);

				const string classNum1 = "8201.10.00 10";
				const string classNum2 = "8201.40.10 00";
				const string classNum3 = "8201.60.10 00";

				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 12, 2, classNum1, 160.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 20, 2, classNum1, 135.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 21, 2, classNum1, 120.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 36, 2, classNum1, 80.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 37, 2, classNum1, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 42, 3, classNum1, 45.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 43, 2, classNum1, 160.00m);

				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 1, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 2, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 3, 1, classNum2, 50.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 4, 1, classNum2, 60.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 5, 1, classNum2, 60.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 6, 1, classNum2, 60.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 7, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 8, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 9, 1, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 10, 1, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 11, 1, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 13, 2, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 14, 2, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 15, 2, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 16, 2, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 17, 1, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 22, 2, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 23, 2, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 24, 2, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 25, 1, classNum2, 70.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 26, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 27, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 28, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 29, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 30, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 31, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 32, 1, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 33, 2, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 35, 2, classNum2, 50.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 39, 2, classNum2, 40.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 40, 3, classNum2, 40.00m);

				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 18, 2, classNum3, 33.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 19, 2, classNum3, 30.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 34, 2, classNum3, 36.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 38, 2, classNum3, 35.00m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 41, 3, classNum3, 30.00m);

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 3, entryHeader.MergedLines.Count);

				IClassificationLine1 classLine1 = entryHeader.MergedLines[0];
				AssertEquals("classLine1.ValueForCurrency", 1620m, classLine1.ValueForCurrency);
				AssertEquals("classLine1.ValueForDuty", 1612.71m, classLine1.ValueForDuty);
				AssertEquals("classLine1.DutyLines.Count", 1, classLine1.ClassificationLines.Count());
				AssertEquals("classLine1.CustomsDutyAmount", 0m, classLine1.ClassificationLines.First().CustomsDutyAmount);
				AssertEquals("classLine1.ValueForTax", 1612.71m, classLine1.ValueForTax);
				AssertEquals("classLine1.SIMAAssessment", 0m, classLine1.SIMAAssessment);
				AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
				AssertEquals("classLine1.GSTAmount", 80.64m, classLine1.GSTAmount);

				AssertEntryTotalsEqualToLinesTotals((CusEntryLine)classLine1);

				IClassificationLine1 classLine2 = entryHeader.MergedLines[1];
				AssertEquals("classLine2.ValueForCurrency", 740m, classLine2.ValueForCurrency);
				AssertEquals("classLine2.ValueForDuty", 736.67m, classLine2.ValueForDuty);
				AssertEquals("classLine2.DutyLines.Count", 1, classLine2.ClassificationLines.Count());
				AssertEquals("classLine2.CustomsDutyAmount", 36.83m, classLine2.ClassificationLines.First().CustomsDutyAmount);
				AssertEquals("classLine2.ValueForTax", 773.5m, classLine2.ValueForTax);
				AssertEquals("classLine2.SIMAAssessment", 0m, classLine2.SIMAAssessment);
				AssertEquals("classLine2.ExciseTaxAmount", 0m, classLine2.ExciseTaxAmount);
				AssertEquals("classLine2.GSTAmount", 38.68m, classLine2.GSTAmount);

				AssertEntryTotalsEqualToLinesTotals((CusEntryLine)classLine2);

				IClassificationLine1 classLine3 = entryHeader.MergedLines[2];
				AssertEquals("classLine3.ValueForCurrency", 164m, classLine3.ValueForCurrency);
				AssertEquals("classLine3.ValueForDuty", 163.26m, classLine3.ValueForDuty);
				AssertEquals("classLine3.DutyLines.Count", 1, classLine3.ClassificationLines.Count());
				AssertEquals("classLine3.CustomsDutyAmount", 0m, classLine3.ClassificationLines.First().CustomsDutyAmount);
				AssertEquals("classLine3.ValueForTax", 163.26m, classLine3.ValueForTax);
				AssertEquals("classLine3.SIMAAssessment", 0m, classLine3.SIMAAssessment);
				AssertEquals("classLine3.ExciseTaxAmount", 0m, classLine3.ExciseTaxAmount);
				AssertEquals("classLine3.GSTAmount", 8.16m, classLine3.GSTAmount);

				AssertEntryTotalsEqualToLinesTotals((CusEntryLine)classLine3);
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestApportionRoundingAmountsOverLines1()
		{
			const string classNum1 = "8201.10.00 10";
			#region Ref Files Data

			const string classRefFileData = @"
GA20820110001090020090801999999992009-06-CPFTANMBNN2009-06-CPFTA2009080199999999NN
GA3082011000102009080199999999N   N2009-06-CPFTA
GA40V000531000000000000000000000V000531000000000000000000000V000531000000000000000000000 00000000000000000000000000002N
GA5082011000102009080199999999N001E40
";

			const string gstRefFileData = "GC100012008010199999999Y1V000531000   NORMAL RATE                                                 N";
			const string exsRefFileData = "GD10E402008070199999999V000531000NMBPACKAGE SIZE >150 BUT <OR =200 GRAMS                        N";

			#endregion

			PrepareGlobalTariffData(Factory, "8201100010");
			var classHeader = GetClassHeader("8201100010");
			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parser = new CACTestingDataHelper.CadexMessageProcessor();
				parser.ProcessAA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(classRefFileData))));
				parser.ProcessAC(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(gstRefFileData))));
				parser.ProcessAD(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(exsRefFileData))));
				var factory = ((IFactoryProvider)parser).Factory;
				JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5.31);
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9955);

				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 1, 1, classNum1, 161.25m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 2, 1, classNum1, 94.67m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 3, 1, classNum1, 94.67m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 4, 1, classNum1, 12.38m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 5, 1, classNum1, 12.38m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 6, 1, classNum1, 12.38m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 7, 1, classNum1, 41.32m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 8, 1, classNum1, 41.32m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 9, 1, classNum1, 41.32m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 10, 1, classNum1, 299.17m);
				declaration.ResumeApportionment();
				var lines = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().OrderByDescending(l => l.JI_LinePrice).ToArray();

				//Pre-Condition: Unadjusted amounts
				AssertLineAmounts(lines[0], 297.82m, 0m, 18.33m, 15.81m, 15.81m, 15.81m);
				AssertLineAmounts(lines[1], 160.52m, 0m, 9.88m, 8.52m, 8.52m, 8.52m);
				AssertLineAmounts(lines[2], 94.24m, 0m, 5.80m, 5.00m, 5.00m, 5.00m);
				AssertLineAmounts(lines[3], 94.24m, 0m, 5.80m, 5.00m, 5.00m, 5.00m);
				AssertLineAmounts(lines[4], 41.13m, 0m, 2.53m, 2.18m, 2.18m, 2.18m);
				AssertLineAmounts(lines[5], 41.13m, 0m, 2.53m, 2.18m, 2.18m, 2.18m);
				AssertLineAmounts(lines[6], 41.13m, 0m, 2.53m, 2.18m, 2.18m, 2.18m);
				AssertLineAmounts(lines[7], 12.32m, 0m, 0.76m, 0.65m, 0.65m, 0.65m);
				AssertLineAmounts(lines[8], 12.32m, 0m, 0.76m, 0.65m, 0.65m, 0.65m);
				AssertLineAmounts(lines[9], 12.32m, 0m, 0.76m, 0.65m, 0.65m, 0.65m);
				AssertLinesTotalAmounts(lines, 807.17m, 0m, 49.68m, 128.46m);

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				IClassificationLine1 classLine1 = entryHeader.MergedLines[0];
				lines = entryHeader.MergedLines[0].InvoiceLines.Cast<JobComInvoiceLine>().OrderByDescending(l => l.JI_LinePrice).ToArray();

				//Adjusted amounts
				AssertEntryTotalsEqualToLinesTotals((CusEntryLine)classLine1);
				AssertLineAmounts(lines[0], 297.83m, 0m, 18.34m, 15.82m, 15.82m, 15.81m);
				AssertLineAmounts(lines[1], 160.53m, 0m, 9.88m, 8.53m, 8.53m, 8.52m);
				AssertLineAmounts(lines[2], 94.25m, 0m, 5.8m, 5.01m, 5.00m, 5.00m);
				AssertLineAmounts(lines[3], 94.25m, 0m, 5.8m, 5.01m, 5.00m, 5.00m);
				AssertLineAmounts(lines[4], 41.13m, 0m, 2.53m, 2.19m, 2.18m, 2.18m);
				AssertLineAmounts(lines[5], 41.13m, 0m, 2.53m, 2.19m, 2.18m, 2.18m);
				AssertLineAmounts(lines[6], 41.13m, 0m, 2.53m, 2.19m, 2.18m, 2.18m);
				AssertLineAmounts(lines[7], 12.32m, 0m, 0.76m, 0.66m, 0.65m, 0.65m);
				AssertLineAmounts(lines[8], 12.32m, 0m, 0.76m, 0.66m, 0.65m, 0.65m);
				AssertLineAmounts(lines[9], 12.32m, 0m, 0.76m, 0.66m, 0.65m, 0.65m);
				AssertLinesTotalAmounts(lines, 807.21m, 0m, 49.69m, 128.58m);
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestApportionRoundingAmountsOverLines2()
		{
			#region Ref Files Data

			const string classRefFileData = @"
GA20820110001090020090801999999992009-06-CPFTANMBNN2009-06-CPFTA2009080199999999NN
GA3082011000102009080199999999N   N2009-06-CPFTA
GA40V000131000000000000000000000V000531000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
GA5082011000102009080199999999N001E40
";

			const string gstRefFileData = "GC100012008010199999999Y1V000531000   NORMAL RATE                                                 N";
			const string exsRefFileData = "GD10E402008070199999999V000531000NMBPACKAGE SIZE >150 BUT <OR =200 GRAMS                        N";

			#endregion

			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parser = new CACTestingDataHelper.CadexMessageProcessor();
				parser.ProcessAA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(classRefFileData))));
				parser.ProcessAC(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(gstRefFileData))));
				parser.ProcessAD(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(exsRefFileData))));
				var factory = ((IFactoryProvider)parser).Factory;
				JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5.31);
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9955);

				const string classNum1 = "8201.10.00 10";

				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 1, 1, classNum1, 160.90m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 2, 1, classNum1, 94.13m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 3, 1, classNum1, 94.13m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 4, 1, classNum1, 12.03m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 5, 1, classNum1, 12.03m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 6, 1, classNum1, 12.03m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 7, 1, classNum1, 40.98m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 8, 1, classNum1, 40.98m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 9, 1, classNum1, 40.98m);
				JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 10, 1, classNum1, 298.63m);
				declaration.ResumeApportionment();

				var lines = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().OrderByDescending(l => l.JI_LinePrice).ToArray();

				//Unadjusted amounts
				AssertLineAmounts(lines[0], 297.29m, 0m, 16.83m, 15.79m, 3.89m);
				AssertLineAmounts(lines[1], 160.18m, 0m, 9.07m, 8.51m, 2.10m);
				AssertLineAmounts(lines[2], 93.71m, 0m, 5.31m, 4.98m, 1.23m);
				AssertLineAmounts(lines[3], 93.71m, 0m, 5.31m, 4.98m, 1.23m);
				AssertLineAmounts(lines[4], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[5], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[6], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[7], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLineAmounts(lines[8], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLineAmounts(lines[9], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLinesTotalAmounts(lines, 803.23m, 0m, 45.49m, 53.21m);

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				IClassificationLine1 classLine1 = entryHeader.MergedLines[0];

				lines = entryHeader.MergedLines[0].InvoiceLines.Cast<JobComInvoiceLine>().OrderByDescending(l => l.JI_LinePrice).ToArray();

				//Adjusted amounts
				AssertEntryTotalsEqualToLinesTotals((CusEntryLine)classLine1);
				AssertLineAmounts(lines[0], 297.28m, 0m, 16.82m, 15.78m, 3.89m);
				AssertLineAmounts(lines[1], 160.17m, 0m, 9.06m, 8.50m, 2.10m);
				AssertLineAmounts(lines[2], 93.70m, 0m, 5.31m, 4.97m, 1.23m);
				AssertLineAmounts(lines[3], 93.70m, 0m, 5.31m, 4.97m, 1.23m);
				AssertLineAmounts(lines[4], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[5], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[6], 40.80m, 0m, 2.31m, 2.17m, 0.53m);
				AssertLineAmounts(lines[7], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLineAmounts(lines[8], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLineAmounts(lines[9], 11.98m, 0m, 0.68m, 0.64m, 0.16m);
				AssertLinesTotalAmounts(lines, 803.19m, 0m, 45.47m, 53.17m);
			}
		}

		static void AssertEntryTotalsEqualToLinesTotals(CusEntryLine entryLine)
		{
			CombineAssertions(
				"Entry line: " + entryLine.RandomLine.JI_Tariff,
				delegate
				{
					var expected = entryLine.InvoiceLines.Sum(l => ((JobComInvoiceLine)l).CA_CustomsValue);
					var actual = entryLine.CL_CustomsValue;
					AssertEquals("Customs Value For Duty", expected, actual);

					expected = entryLine.InvoiceLines.Sum(l => ((JobComInvoiceLine)l).JI_Calc_NormalDutyAmount);
					actual = entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalDutyAmount);
					AssertEquals("Customs Duty", expected, actual);

					expected = entryLine.InvoiceLines.Sum(l => ((JobComInvoiceLine)l).JI_Calc_ExciseTaxesAmount);
					actual = entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount);
					AssertEquals("Excise Tax", expected, actual);

					expected = entryLine.InvoiceLines.OfType<BaseJobComInvoiceLine>().Sum(l => l.JI_Calc_GSTVATAmount);
					actual = entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTAmount);
					AssertEquals("GST", expected, actual);
				});
		}

		static void AssertLineAmounts(JobComInvoiceLine line, decimal vfd, decimal exs, decimal gst, params decimal[] duties)
		{
			CombineAssertions(
				"Line " + line.JI_LineNo,
				delegate
				{
					AssertEquals("Customs Value", vfd, line.CA_CustomsValue);
					AssertEquals("Excise Tax", exs, line.JI_Calc_ExciseTaxesAmount);
					AssertEquals("GST", gst, line.JI_Calc_GSTVATAmount);

					var actualDuties = line.DutyAndTaxManager.Duties.OrderByDescending(d => d.C1_Amount);
					AssertEquals("Duties.Count", duties.Length, actualDuties.Count());

					for (var i = 0; i < Math.Min(actualDuties.Count(), duties.Length); i++)
					{
						AssertEquals("Duty " + (i + 1), duties[i], actualDuties.ElementAt(i).C1_Amount);
					}
				});
		}

		static void AssertLinesTotalAmounts(IEnumerable<JobComInvoiceLine> lines, decimal vfd, decimal exs, decimal gst, decimal duties)
		{
			CombineAssertions(
				"Lines Totals",
				delegate
				{
					AssertEquals("Customs Value", vfd, lines.Sum(l => l.CA_CustomsValue));
					AssertEquals("Excise Tax", exs, lines.Sum(l => l.JI_Calc_ExciseTaxesAmount));
					AssertEquals("GST", gst, lines.Sum(l => l.JI_Calc_GSTVATAmount));
					AssertEquals("Duties", duties, lines.Sum(l => l.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty)));
				});
		}

		public void TestDDPCalculationsForMergedLines()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Japan;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 7386.5m);

			line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 98.08m);

			line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 429.21m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);

			IClassificationLine1 classLine1 = entryHeader.MergedLines[0];
			AssertEquals("classLine1.ValueForCurrency", 5781.55m, classLine1.ValueForCurrency);
			AssertEquals("classLine1.ValueForDuty", 5781.55m, classLine1.ValueForDuty);
			AssertEquals("classLine1.DutyLines.Count", 3, classLine1.ClassificationLines.Count());
			AssertEquals("classLine1.CustomsDutyAmount", 1040.68m, classLine1.ClassificationLines.Last().CustomsDutyAmount);
			AssertEquals("classLine1.ValueForTax", 6822.23m, classLine1.ValueForTax);
			AssertEquals("classLine1.SIMAAssessment", 0m, classLine1.SIMAAssessment);
			AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
			AssertEquals("classLine1.GSTAmount", 1091.56m, classLine1.GSTAmount);
			AssertEquals("total DDP price = vfd + dutyies + taxes", invoice.InvoiceLines[0].JI_LinePrice + invoice.InvoiceLines[1].JI_LinePrice + invoice.InvoiceLines[2].JI_LinePrice,
					classLine1.ValueForDuty + classLine1.ClassificationLines.Last().CustomsDutyAmount + classLine1.GSTAmount);
		}

		[TestDate(2014, 1, 1)]
		public void TestDDPCalculationsForExciseTax()
		{
			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesForExciseTestReturningFactory();
				JobComInvoiceLineTestHelper.CreateCAGSTRateCode(factory, 5);
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "CA-1482";
				invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Japan;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9932003);
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				var oft = invoice.Charges.AddNew();
				oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 100m;
				oft.J7_RX_NKCurrency = "CAD";
				oft.J7_IsIncludedInITOT = true;

				var line = invoice.JobComInvoiceLines.AddNew();
				line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
				var simaDuty = line.DutyAndTaxManager.SIMADuties.FirstOrDefault() ?? line.DutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C31;
				simaDuty.C1_Amount = 200m;
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 1, "2403.19.00 41", 3000m);
				line.JI_CustomsUnitQty = "NMB";
				line.JI_CustomsQuantity = 20;

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);

				IClassificationLine1 classLine1 = entryHeader.MergedLines[0];

				AssertEquals("classLine1.ValueForCurrency", 2461.43m, classLine1.ValueForCurrency);
				AssertEquals("classLine1.ValueForDuty", 2444.69m, classLine1.ValueForDuty);
				AssertEquals("classLine1.DutyLines.Count", 1, classLine1.ClassificationLines.Count());
				AssertEquals("classLine1.CustomsDutyAmount", 97.79m, classLine1.ClassificationLines.Last().CustomsDutyAmount);
				AssertEquals("classLine1.ValueForTax", 2742.48m, classLine1.ValueForTax);
				AssertEquals("classLine1.SIMAAssessment", 200m, classLine1.SIMAAssessment);
				AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
				AssertEquals("classLine1.ExciseTaxRate", 0m, classLine1.ExciseTaxRate);
				AssertEquals("classLine1.GSTAmount", 137.12m, classLine1.GSTAmount);
				AssertEquals("total DDP price less freight = vfd + dutyies + taxes", invoice.InvoiceLines[0].JI_LinePriceInLocalCurrency - 100,
						classLine1.ValueForDuty + classLine1.ClassificationLines.Last().CustomsDutyAmount + classLine1.SIMAAssessment + classLine1.GSTAmount + classLine1.ExciseTaxAmount);

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);
				classLine1 = entryHeader.MergedLines[0];
				AssertEquals("classLine1.ValueForDuty", 2444.69m, classLine1.ValueForDuty);
				AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
				AssertEquals("classLine1.ExciseTaxRate", 0m, classLine1.ExciseTaxRate);
				AssertEquals("total DDP price less freight = vfd + dutyies + taxes", invoice.InvoiceLines[0].JI_LinePriceInLocalCurrency - 100,
						classLine1.ValueForDuty + classLine1.ClassificationLines.Last().CustomsDutyAmount + classLine1.SIMAAssessment + classLine1.GSTAmount + classLine1.ExciseTaxAmount);

				declaration.ResumeApportionment();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);
				classLine1 = entryHeader.MergedLines[0];
				AssertEquals("classLine1.ValueForDuty", 2444.69m, classLine1.ValueForDuty);
				AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
				AssertEquals("total DDP price less freight = vfd + dutyies + taxes", invoice.InvoiceLines[0].JI_LinePriceInLocalCurrency - 100,
						classLine1.ValueForDuty + classLine1.ClassificationLines.Last().CustomsDutyAmount + classLine1.SIMAAssessment + classLine1.GSTAmount + classLine1.ExciseTaxAmount);
			}
		}

		public void TestDDPCalculationsWithExemptTaxes()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2403190041", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);

			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesForExciseTestReturningFactory();
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "CA-1482";
				invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Japan;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9932);
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				var oft = invoice.Charges.AddNew();
				oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 100m;
				oft.J7_RX_NKCurrency = "CAD";
				oft.J7_IsIncludedInITOT = true;

				var line = invoice.JobComInvoiceLines.AddNew();
				line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
				var simaDuty = line.DutyAndTaxManager.SIMADuties.FirstOrDefault() ?? line.DutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C31;
				simaDuty.C1_Amount = 200m;
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 1, "2403.19.00 41", 3000m);
				line.JI_CustomsUnitQty = "NMB";
				line.JI_CustomsQuantity = 20;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				AssertEquals("There should be 1 GST line", 1, line.DutyAndTaxManager.GSTaxes.Count());
				line.DutyAndTaxManager.GSTaxes.First().C1_ExemptCode = GSTStatusCodes.Codes.C81;
				AssertEquals("There should be 1 EXT line", 1, line.DutyAndTaxManager.ExciseTaxes.Count());
				line.DutyAndTaxManager.ExciseTaxes.First().C1_ExemptCode = ExciseTaxExemptionCodes.Codes.C85;
				declaration.MarkApportionmentDirty();
				declaration.DoMerge();

				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);

				IClassificationLine1 classLine1 = entryHeader.MergedLines[0];

				AssertEquals("classLine1.ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
				AssertEquals("classLine1.GSTAmount", 0m, classLine1.GSTAmount);
				AssertEquals("classLine1.ValueForCurrency", 2594.18m, classLine1.ValueForCurrency);
				AssertEquals("classLine1.ValueForDuty", 2576.54m, classLine1.ValueForDuty);
				AssertEquals("classLine1.DutyLines.Count", 1, classLine1.ClassificationLines.Count());
				AssertEquals("classLine1.CustomsDutyAmount", 103.06m, classLine1.ClassificationLines.Last().CustomsDutyAmount);
				AssertEquals("classLine1.ValueForTax", 2879.60m, classLine1.ValueForTax);
				AssertEquals("classLine1.SIMAAssessment", 200m, classLine1.SIMAAssessment);
				AssertEquals("total DDP price less freight = vfd + dutyies + taxes", invoice.InvoiceLines[0].JI_LinePriceInLocalCurrency - 100,
						classLine1.ValueForDuty + classLine1.ClassificationLines.Last().CustomsDutyAmount + classLine1.SIMAAssessment);
			}
		}

		void PrepareGlobalTariffData(BusinessObjectFactory factory, string tariffNumber)
		{
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, tariffNumber, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			factory.Save();
		}

		CACClassHeader GetClassHeader(string classificationNumber)
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber;
			classHeader.ZA_EffectiveDate = ZDateTime.MinSmallDateTimeValue;
			classHeader.ZA_ExpiryDate = ZDateTime.MaxSmallDateTime;
			classHeader.ZA_AreaCode = "900";
			return classHeader;
		}

		#endregion
	}
}
