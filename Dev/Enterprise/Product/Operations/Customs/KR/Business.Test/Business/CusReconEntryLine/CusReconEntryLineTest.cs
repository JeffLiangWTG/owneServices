using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryLine))]
	sealed class CusReconEntryLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CreateCusReconEntryLine(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateCusReconEntryLine(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateCusReconEntryLine(Factory);

		public void TestCusReconCustomsCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.VATRateA, 0.1m, Core.Constants.CountryCodes.KoreaSouth, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			var reconEntryLine = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_CustomsStatus = "AA";
			reconEntryLine.CRL_Description = "AA";
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;

			var fileReader = new TestFileReader(typeof(CusReconEntryLineTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine_1.xml");
			var snapshot = Factory.New<CusReconSnapshot>();
			snapshot.CRS_SnapshotXml = messageText;

			AssertEquals("Default 0 and read-only", 0m, reconEntryLine.VATExemptionValue);

			reconEntryLine.DutyToRefund = 1m;
			reconEntryLine.DutyPenaltyToRefund = 5m;
			reconEntryLine.LQTToRefund = 10m;
			reconEntryLine.LQTPenaltyToRefund = 20m;
			reconEntryLine.EDTToRefund = 40m;
			reconEntryLine.EDTPenaltyToRefund = 80m;
			reconEntryLine.AGTToRefund = 160m;
			reconEntryLine.AGTPenaltyToRefund = 320m;
			reconEntryLine.VATToRefund = 640m;
			reconEntryLine.VATPenaltyToRefund = 1280m;
			reconEntryLine.SCTToRefund = 2560m;
			reconEntryLine.SCTPenaltyToRefund = 5120m;
			reconEntryLine.TRTToRefund = 10240m;
			reconEntryLine.TRTPenaltyToRefund = 20480m;
			reconEntryLine.PenaltyLateDecToRefund = 40960m;
			reconEntryLine.PenaltyMissedDecToRefund = 81920m;
			reconEntryLine.PenaltyLatePaymentToRefund = 163840m;
			reconEntryLine.NonDutyTaxRevenueToRefund = 327680m;

			AssertEquals("VAT to refund / 0.1", 6400m, reconEntryLine.ValueForVAT);
			AssertEquals(655356m, reconEntryLine.TotalRefundAmount);

			AssertEquals(18, reconEntryLine.CusReconCharges.Count);
			AssertReconCharge(ChargeTypeList.Codes.Duty, 1m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AD, 5m);
			AssertReconCharge(ChargeTypeList.Codes.LiquorTax, 10m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AF, 20m);
			AssertReconCharge(ChargeTypeList.Codes.EducationTax, 40m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AI, 80m);
			AssertReconCharge(ChargeTypeList.Codes.AgricultureTax, 160m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AJ, 320m);
			AssertReconCharge(ChargeTypeList.Codes.VAT, 640m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AH, 1280m);
			AssertReconCharge(ChargeTypeList.Codes.SpecialConsumptionTax, 2560m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AE, 5120m);
			AssertReconCharge(ChargeTypeList.Codes.TransportationTax, 10240m);
			AssertReconCharge(EntryTaxTypeList.Codes._5AG, 20480m);
			AssertReconCharge(ChargeTypeList.Codes.PenaltyForLateDeclaration, 40960m);
			AssertReconCharge(ChargeTypeList.Codes.PenaltyForMissedDeclaration, 81920m);
			AssertReconCharge(ChargeTypeList.Codes.PenaltyForLatePayment, 163840m);
			AssertReconCharge(ChargeTypeList.Codes.NonDutyTaxRevenue, 327680m);

			var loadedReconEntry = Factory.Load<CusReconEntry>(reconEntry.PK);
			var loadedReconEntryLine = (CusReconEntryLine)loadedReconEntry.CusReconEntryLines[0];
			AssertEquals(loadedReconEntryLine.DutyToRefund, 1m);
			AssertEquals(loadedReconEntryLine.DutyPenaltyToRefund, 5m);
			AssertEquals(loadedReconEntryLine.LQTToRefund, 10m);
			AssertEquals(loadedReconEntryLine.LQTPenaltyToRefund, 20m);
			AssertEquals(loadedReconEntryLine.EDTToRefund, 40m);
			AssertEquals(loadedReconEntryLine.EDTPenaltyToRefund, 80m);
			AssertEquals(loadedReconEntryLine.AGTToRefund, 160m);
			AssertEquals(loadedReconEntryLine.AGTPenaltyToRefund, 320m);
			AssertEquals(loadedReconEntryLine.VATToRefund, 640m);
			AssertEquals(loadedReconEntryLine.VATPenaltyToRefund, 1280m);
			AssertEquals(loadedReconEntryLine.SCTToRefund, 2560m);
			AssertEquals(loadedReconEntryLine.SCTPenaltyToRefund, 5120m);
			AssertEquals(loadedReconEntryLine.TRTToRefund, 10240m);
			AssertEquals(loadedReconEntryLine.TRTPenaltyToRefund, 20480m);
			AssertEquals(loadedReconEntryLine.PenaltyLateDecToRefund, 40960m);
			AssertEquals(loadedReconEntryLine.PenaltyMissedDecToRefund, 81920m);
			AssertEquals(loadedReconEntryLine.PenaltyLatePaymentToRefund, 163840m);
			AssertEquals(loadedReconEntryLine.NonDutyTaxRevenueToRefund, 327680m);

			AssertEquals(loadedReconEntryLine.TotalPenaltyToRefund, 27305m);
			AssertEquals(loadedReconEntryLine.TotalLateRefundAmount, 614400m);

			void AssertReconCharge(string chargeType, decimal value)
			{
				var charge = reconEntryLine.CusReconCharges.Cast<CusReconCustomsCharge>().FirstOrDefault(x => x.CRC_ChargeType == chargeType);
				AssertEquals(value, charge.CRC_Amount);
			}
		}
		public void TestTotalRefundAmount()
		{
			var reconEntryLine = Factory.New<CusReconDeclaration>().CusReconEntryLines.AddNew();
			reconEntryLine.CusReconCharges.AddNew().CRC_Amount = 1;
			reconEntryLine.CusReconCharges.AddNew().CRC_Amount = 2;
			reconEntryLine.CusReconCharges.AddNew().CRC_Amount = 3;

			AssertEquals(6m, reconEntryLine.TotalRefundAmount);
		}

		public void TestContractRevocation()
		{
			var reconEntryLine = Factory.New<CusReconEntryLine>();
			AssertEquals(0, reconEntryLine.ContractRevocations.Count);
			AssertNull(reconEntryLine.ContractRevocation);

			reconEntryLine.ContractRevocations.AddNew();
			AssertEquals(1, reconEntryLine.ContractRevocations.Count);
			AssertNotNull(reconEntryLine.ContractRevocation);
		}

		public void TestZPropertyInfoChange()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			var cusReconEntryLine = (CusReconEntryLine)cusReconEntry.CusReconEntryLines.AddNew();
			AssertEquals(true, cusReconEntryLine.CRL_OriginalEntryLineNumberInfo.ReadOnly);
			AssertEquals(true, cusReconEntryLine.FormattedOriginalEntryLineNumberInfo.ReadOnly);

			cusReconEntry.CRE_OriginalEntryNumber = "1234567890M";
			AssertEquals(false, cusReconEntryLine.CRL_OriginalEntryLineNumberInfo.ReadOnly);
			AssertEquals(false, cusReconEntryLine.FormattedOriginalEntryLineNumberInfo.ReadOnly);

			cusReconEntryLine.RefundInvoiceLines.AddNew();
			AssertEquals(1, cusReconEntryLine.RefundInvoiceLines.Count);
			AssertEquals(true, cusReconEntryLine.CRL_OriginalEntryLineNumberInfo.ReadOnly);
			AssertEquals(true, cusReconEntryLine.FormattedOriginalEntryLineNumberInfo.ReadOnly);
		}

		public void TestFormattedOriginalEntryLineNumber()
		{
			var cusReconEntryLine = Factory.New<CusReconEntryLine>();
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 1;
			AssertEquals("1", cusReconEntryLine.FormattedOriginalEntryLineNumber);
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 0;
			AssertEquals(ZString.Empty, cusReconEntryLine.FormattedOriginalEntryLineNumber);
		}

		CusReconEntryLine CreateCusReconEntryLine(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			var reconEntryLine = factory.New<CusReconEntryLine>();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_CustomsStatus = "AA";
			reconEntryLine.CRL_Description = "AA";
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			reconEntryLine.CRL_CRE = reconEntry.PK;
			reconEntryLine.ContractRevocations.AddNew();
			return reconEntryLine;
		}

		public void TestPopulateRefundInvoiceLinesFromEntryLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0102399000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-2), "Tariff Description 1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0102399000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Description 2");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234567890M";

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Model = "Test";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_Tariff = "0102399000";
			invoiceLine.JI_LinePrice = 175m;

			var cusStatementHeader = Factory.New<CusStatementHeader>();
			cusStatementHeader.B2_GC = company.PK;
			cusStatementHeader.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			var cusStatementLine = cusStatementHeader.StatementLines.AddNew();
			cusStatementLine.B3_EntryNum = entry.EntryNumber;
			cusStatementLine.B3_AssociatedEntry = "0127012112100053179";

			Factory.Save();

			var customsBillsView = Factory.LoadTop1<KREntryCustomsBillsView>(new ZQuery(KREntryCustomsBillsViewSchema.KEB_CustomsDisbursementBillNumber, "0127012112100053179"));
			var lineDetailsView = Factory.LoadTop1<KREntryLineDetailsView>(new ZQuery(KREntryLineDetailsViewSchema.KEL_EntryNum, customsBillsView.KEB_ImportEntryNum));
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_EntryDate = ZDate.Today.AddDays(-3);
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var reconEntryLine = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.PopulateRefundInvoiceLinesFromEntryLine(lineDetailsView);

			AssertEquals((ZShort)1, reconEntryLine.CRL_OriginalEntryLineNumber);

			AssertEquals(1, reconEntryLine.RefundInvoiceLines.Count);
			var reconInvoiceLine = reconEntryLine.RefundInvoiceLines[0];
			AssertEquals(1, reconInvoiceLine.CSI_LineNo);
			AssertEquals("Tariff Description 1", reconInvoiceLine.CSI_AdditionalDescription);
			AssertEquals("Test", reconInvoiceLine.CSI_Description);
			AssertEquals(10m, reconInvoiceLine.CSI_Quantity2);
			AssertEquals(17.5m, reconInvoiceLine.CSI_Value);

			reconEntry.CRE_EntryDate = ZDate.Today;
			var reconEntryLine2 = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine2.PopulateRefundInvoiceLinesFromEntryLine(lineDetailsView);
			AssertEquals(1, reconEntryLine2.RefundInvoiceLines.Count);
			AssertEquals("Tariff Description 2", reconEntryLine2.RefundInvoiceLines[0].CSI_AdditionalDescription);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}

	sealed class CusReconEntryLineForXmlTest : XMLMessageTestHelper<CusReconEntryLineForXmlTest>
	{
		public void TestPropertiesForPaidAmount()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			var cusReconEntryLine = cusReconDeclaration.CusReconEntryLines.AddNew();
			var cusReconEntry = cusReconEntryLine.Header;
			AssertEquals(0, cusReconEntry.CusReconSnapshots.Count);
			AssertEquals(0, cusReconEntryLine.CusReconSnapshots.Count);
			AssertValueIsZero();

			var snapshot_AttachedToEntryLine = cusReconEntryLine.CusReconSnapshots.AddNew();
			Assert(cusReconEntryLine.FirstSnapShot.CRS_SnapshotXml.IsEmpty);
			AssertValueIsZero();

			var fileReader = new TestFileReader(typeof(CusReconEntryLineForXmlTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "CusReconEntryLine_Paid.xml");
			snapshot_AttachedToEntryLine.CRS_SnapshotXml = messageText;
			Assert(!cusReconEntryLine.FirstSnapShot.CRS_SnapshotXml.IsEmpty);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.DutyAmount", 100m, cusReconEntryLine.PaidDutyAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.LiquorTaxAmount", 200m, cusReconEntryLine.PaidLiquorTaxAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.SpecialConsumptionTaxAmount", 300m, cusReconEntryLine.PaidSpecialConsumptionTaxAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.TransportTaxAmount", 400m, cusReconEntryLine.PaidTransportTaxAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.EducationTaxAmount", 500m, cusReconEntryLine.PaidEducationTaxAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.AgricultureTaxAmount", 600m, cusReconEntryLine.PaidAgricultureTaxAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.VATAmount", 700m, cusReconEntryLine.PaidVATAmount);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.LatePaymentPenalty", 800m, cusReconEntryLine.PaidLatePaymentPenalty);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.TotalPenalty", 1000m, cusReconEntryLine.PaidTotalPenalty);
			AssertEquals("ReconEntryLine.ImportEntryOrEntryLine.PaidAmounts.TotalPaid", 4500m, cusReconEntryLine.TotalPaid);

			snapshot_AttachedToEntryLine.CRS_SnapshotXml = "";
			var snapshot_AttachedToEntry = cusReconEntry.CusReconSnapshots.AddNew();
			messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "CusReconEntry_Paid.xml");
			snapshot_AttachedToEntry.CRS_SnapshotXml = messageText;
			Assert(!cusReconEntry.FirstSnapShot.CRS_SnapshotXml.IsEmpty);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.DutyAmount", 10m, cusReconEntryLine.PaidDutyAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.LiquorTaxAmount", 20m, cusReconEntryLine.PaidLiquorTaxAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.SpecialConsumptionTaxAmount", 30m, cusReconEntryLine.PaidSpecialConsumptionTaxAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.TransportTaxAmount", 40m, cusReconEntryLine.PaidTransportTaxAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.EducationTaxAmount", 50m, cusReconEntryLine.PaidEducationTaxAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.AgricultureTaxAmount", 60m, cusReconEntryLine.PaidAgricultureTaxAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.VATAmount", 70m, cusReconEntryLine.PaidVATAmount);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.LatePaymentPenalty", 80m, cusReconEntryLine.PaidLatePaymentPenalty);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.TotalPenalty", 100m, cusReconEntryLine.PaidTotalPenalty);
			AssertEquals("ReconEntry.ImportEntryOrEntryLine.PaidAmounts.TotalPaid", 450m, cusReconEntryLine.TotalPaid);

			void AssertValueIsZero()
			{
				AssertEquals(0m, cusReconEntryLine.PaidDutyAmount);
				AssertEquals(0m, cusReconEntryLine.PaidLiquorTaxAmount);
				AssertEquals(0m, cusReconEntryLine.PaidSpecialConsumptionTaxAmount);
				AssertEquals(0m, cusReconEntryLine.PaidTransportTaxAmount);
				AssertEquals(0m, cusReconEntryLine.PaidEducationTaxAmount);
				AssertEquals(0m, cusReconEntryLine.PaidAgricultureTaxAmount);
				AssertEquals(0m, cusReconEntryLine.PaidVATAmount);
				AssertEquals(0m, cusReconEntryLine.PaidLatePaymentPenalty);
				AssertEquals(0m, cusReconEntryLine.PaidTotalPenalty);
				AssertEquals(0m, cusReconEntryLine.TotalPaid);
			}
		}
		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
