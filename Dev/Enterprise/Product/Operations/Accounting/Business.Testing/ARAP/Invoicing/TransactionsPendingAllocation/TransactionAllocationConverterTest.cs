using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionAllocationConverterTest : TestCaseWithFactory
	{
		public void TestConvertUnallocatedWhenTransactionIsDeletedByAnotherUser()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var testObjectCreatorInFactory1 = new TestObjectCreator(factory1);
			var invoicePendingAllocationInFactory1 = testObjectCreatorInFactory1.CreateTransactionPendingAllocation("INV1234", testObjectCreatorInFactory1.Creditor1, 10m);
			factory1.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var invoicePendingAllocationInFactory2 = factory2.Load<TransactionPendingAllocation>(invoicePendingAllocationInFactory1.PK);
			AssertNotNull("Invoice should be in DB", invoicePendingAllocationInFactory2);
			invoicePendingAllocationInFactory2.Delete();
			Assert("Invoice should be deleted in factory2", invoicePendingAllocationInFactory2.IsDeleted);
			factory2.Save();

			var expectedErrorMessage = FormattableString.Invariant($@"The transaction {invoicePendingAllocationInFactory1.AH_TransactionNum} cannot be allocated because another user has deleted the record.");

			(var apInvoice, var errorMessageAP) = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocationInFactory1);
			AssertNull("Expecting null because invoice was deleted in factory2.", apInvoice);
			AssertEquals(expectedErrorMessage, errorMessageAP);

			(var arCreditNote, var errorMessageAR) = TransactionAllocationConverter.ConvertUnallocatedToAR(invoicePendingAllocationInFactory1);
			AssertNull("Expecting null because invoice was deleted in factory2.", arCreditNote);
			AssertEquals(expectedErrorMessage, errorMessageAR);
		}

		public void TestConvertUnallocatedToARWhenPendingAllocationTransactionIsCreditNotePendingAllocation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var creditNotePendingAllocation = Factory.New<TransactionPendingAllocation>();
			creditNotePendingAllocation.AH_TransactionNum = "transaction";
			creditNotePendingAllocation.AH_OH = org.PK;
			creditNotePendingAllocation.AH_OSExTaxAmount = -100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(300, creditNotePendingAllocation.PK, 2);
			creditNotePendingAllocation.AH_OSTaxAmount = -10m;
			creditNotePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var expectedErrorMessage = FormattableString.Invariant($@"Credit Note Pending Allocation transactions are not eligible to allocations as Receivable Invoice. Transaction number: {creditNotePendingAllocation.AH_TransactionNum}.");
			(var invoicingBase, var errorMessage) = TransactionAllocationConverter.ConvertUnallocatedToAR(creditNotePendingAllocation);
			AssertNull("Expecting null because Credit Note Pending Allocation can not be allocated as Receivable Invoice.", invoicingBase);
			AssertEquals(expectedErrorMessage, errorMessage);
		}

		public void TestConvertUnallocatedToARReturnsARCreditNoteWithExpectedComplianceSubTypeWhenPendingAllocationTransactionIsInvoice()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV1234", TestObjectCreator.Creditor1, 10m);
			Factory.Save();

			var (arCreditNote, errorMessageAR) = TransactionAllocationConverter.ConvertUnallocatedToAR(invoicePendingAllocation);
			AssertNotNull("The object should not be null.", arCreditNote);
			Assert("The type of object should be ARCreditNote.", arCreditNote is ARCreditNote);
			AssertNull("The error message should be empty", errorMessageAR);
			Assert("Lines are not read-only", !arCreditNote.Lines.ReadOnly);

			invoicePendingAllocation.AH_ComplianceSubType = "PIN";
			Factory.Save();

			(arCreditNote, errorMessageAR) = TransactionAllocationConverter.ConvertUnallocatedToAR(invoicePendingAllocation);
			AssertEquals("Expected Compliance Sub Type", "PIN", arCreditNote.AH_ComplianceSubType);
			Assert("Lines are not read-only", !arCreditNote.Lines.ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			{
				(arCreditNote, errorMessageAR) = TransactionAllocationConverter.ConvertUnallocatedToAR(invoicePendingAllocation);
				AssertEquals("Expected Compliance Sub Type", "CIN", arCreditNote.AH_ComplianceSubType);
				Assert("Lines are not read-only", !arCreditNote.Lines.ReadOnly);
			}
		}

		[TestDate(2020, 2, 2)]
		public void TestConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsTrue()
		{
			AssertConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol(true, true);
		}

		[TestDate(2020, 2, 2)]
		public void TestConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsFalse()
		{
			AssertConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol(true, false);
		}

		[TestDate(2020, 2, 2)]
		public void TestConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsTrue()
		{
			AssertConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol(false, true);
		}

		[TestDate(2020, 2, 2)]
		public void TestConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsFalse()
		{
			AssertConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol(false, false);
		}

		void AssertConvertUnallocatedToAPRespectsJobBillingExchangeRateConfigurationForForwardingConsol(bool isRegistryDEF, bool shouldUseJobExRateFlag)
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, Core.Constants.TransportModes.All, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.ExchangeRateTypes.Code.CustomsRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoiceDate = ZDateTime.Today.AddDays(-6);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.56M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.78M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 3.22M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 4.39M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 1", TestObjectCreator.Creditor1, 10);
			invoicePendingAllocation.AH_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			invoicePendingAllocation.AH_ExchangeRate = 2.11m;
			invoicePendingAllocation.AH_InvoiceDate = invoiceDate;
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleForeignCurrencyLineFromConsolXml, true);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00000010");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			Factory.Save();

			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryDEF ? AccountingConstants.InvoicePostingExchangeRateOption.Default.Code : AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shouldUseJobExRateFlag))
			{
				var aPInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				AssertEquals(1, aPInvoice.ConsolCosting.ConsolCosts.Count);
				AssertEquals(1, aPInvoice.Lines.Count);
				var expectedExRate = shouldUseJobExRateFlag ? (isRegistryDEF ? (ZDecimal)4.39m : (ZDecimal)3.22m) : aPInvoice.AH_ExchangeRate;
				BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected line exchange rate.", expectedExRate, aPInvoice.Lines[0].AL_ExchangeRate, 0.001m);
			}
		}

		public void TestConvertUnallocatedToAPWhenConsolHasNoShipments()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 1", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromConsolXml2, true);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00000010");
			Factory.Save();

			var aPInvoice = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			AssertEquals(0, aPInvoice.Lines.Count);
			AssertEquals(1, aPInvoice.ConsolCosting.ConsolCosts.Count);
			aPInvoice.Validation.ValidateAll();
			AssertHasRowError("Consol costs without related transaction lines", aPInvoice, "There are consol costs without related transaction lines, please apportion all consol costs again.");
		}

		public void TestConvertUnallocatedToAP_CreatePAToAPTransactionLineMonitor_RegistryEnableTransactionLineMonitorIsON()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 1", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertNull("Precondition", Factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>());

			var invoice = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertNotNull(invoice.Factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>());
		}

		public void TestConvertUnallocatedToAP_CreatePAToAPTransactionLineMonitor_DefaultContext()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 1", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);

			AssertNull("Precondition", Factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>());

			var invoice = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertNull("Should NOT create monitor because feature EnableTransactionLineMonitor is disabled", Factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>());
		}

		public void TestOSTAmountAfterConvertUnallocatedToAP()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			TransactionPendingAllocation invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(200, invoicePendingAllocation.PK, 1);
			invoicePendingAllocation.AH_Desc = "Test Description";

			Factory.Save();

			AssertEquals("Pre-condition: UseJobExchangeRateDefault", false, AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.Value);

			InvoicingBase result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert(result is APInvoice);
			AssertEquals(result.AH_OSTotalAmount, ZDecimal.Zero);
		}

		[TestDate(2015, 10, 24)]
		public void TestPostDateForConvertUnallocatedToAP()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "INV";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(200, invoicePendingAllocation.PK, 1);
			invoicePendingAllocation.AH_OSTaxAmount = 10m;
			invoicePendingAllocation.AH_Desc = "Test Description";
			invoicePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert("Precondition", result is APInvoice);
			AssertEquals("As there is no imported xml, the invoice post date defaults to the today(test date)", new ZDateTime(2015, 10, 24), result.AH_PostDate);

			var invoicePendingAllocation1 = TestObjectCreator.CreateTransactionPendingAllocation("INV 1", TestObjectCreator.Creditor1, 10);
			var request1 = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request1.Initialize(invoicePendingAllocation1, UniversalTransactionXML, false);
			Factory.Save();

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation1).Invoice;
			Assert("Precondition", result is APInvoice);
			AssertEquals("AS there is a imported xml the Post Date is same as the imported xml", new ZDateTime(2015, 04, 30, 19, 09, 0), result.AH_PostDate);

			var universalTransactionXMLWithoutPostDate = UniversalTransactionXML.Replace("<PostDate>2015-04-30T19:09:00</PostDate>", "");
			var invoicePendingAllocation2 = TestObjectCreator.CreateTransactionPendingAllocation("INV 2", TestObjectCreator.Creditor1, 10);
			var request2 = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request2.Initialize(invoicePendingAllocation2, universalTransactionXMLWithoutPostDate, false);
			Factory.Save();

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation2).Invoice;
			Assert("Precondition", result is APInvoice);
			AssertEquals("As imported xml does not have post date, the invoice post date defaults to the today(test date)", new ZDateTime(2015, 10, 24), result.AH_PostDate);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 1", TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 2", TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS, null, null, 0M, null);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.Branch", "SYD", line.Branch.GB_Code);
				AssertEquals("line.Department", "BRN", line.Department.GE_Code);
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.Description", "FREIGHT REVENUE ACTUAL", line.AL_Desc);
				AssertEquals("line.IsFinalCharge", true, line.AL_IsFinalCharge);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("line.Sequence", 2, (int)line.AL_Sequence);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);

				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_AmountDoesNotMatch()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 1", TestObjectCreator.AUD, 60M, TestObjectCreator.ABIGAS, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 2", TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS, null, null, 0M, null);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				AssertLineCreatedFromXml(result);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);

				Assert("No AMD log added", !result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));

				AssertEquals("1 Line added from the original xml as no match was found using auto accrual matching", 1, result.Lines.Count);
			}
		}

		static void AssertLineCreatedFromXml(InvoicingBase result)
		{
			var line = result.Lines[0];
			AssertEquals("line.Branch", "SYD", line.Branch.GB_Code);
			AssertEquals("line.Department", "BRN", line.Department.GE_Code);
			AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
			AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
			AssertEquals("line.Description", "FREIGHT REVENUE ACTUAL", line.AL_Desc);
			AssertEquals("line.IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
			AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals("line.Sequence", 2, (int)line.AL_Sequence);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML_WhenConvertToConsolCostRelatedLines_APINV()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML(TransactionTypes.Invoice, false);
			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-60m, result.AH_LocalTotal);
			AssertEquals(-60m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			var linesCC1 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK.ToGuid()));
			AssertEquals(2, linesCC1.Length);
			AssertEquals(true, linesCC1.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.UnitedStates));
			AssertEquals(30m, linesCC1.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesCC1.Sum(x => x.AL_OverseasTotal));

			var linesCC2 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC2.PK.ToGuid()));
			AssertEquals(2, linesCC2.Length);
			AssertEquals(true, linesCC2.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.Australia));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_OverseasTotal));
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML_WhenConvertToConsolCostRelatedLines_APCRD()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML(TransactionTypes.CreditNote, false);
			AssertEquals(TransactionTypes.CreditNote, result.AH_TransactionType);
			AssertEquals(60m, result.AH_LocalTotal);
			AssertEquals(60m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			var linesCC1 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK.ToGuid()));
			AssertEquals(2, linesCC1.Length);
			AssertEquals(true, linesCC1.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.UnitedStates));
			AssertEquals(30m, linesCC1.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesCC1.Sum(x => x.AL_OverseasTotal));

			var linesCC2 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC2.PK.ToGuid()));
			AssertEquals(2, linesCC2.Length);
			AssertEquals(true, linesCC2.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.Australia));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_OverseasTotal));
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML_WhenCreateSeparateConsolCostBasedOnEachLine_APINV()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML(TransactionTypes.Invoice, true);
			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-60m, result.AH_LocalTotal);
			AssertEquals(-60m, result.AH_OSTotal);
			AssertEquals(8, result.Lines.Count);

			var linesCC1 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK.ToGuid()));
			AssertEquals(4, linesCC1.Length);
			AssertEquals(true, linesCC1.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.UnitedStates));
			AssertEquals(30m, linesCC1.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesCC1.Sum(x => x.AL_OverseasTotal));

			var linesCC2 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC2.PK.ToGuid()));
			AssertEquals(4, linesCC2.Length);
			AssertEquals(true, linesCC2.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.Australia));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_OverseasTotal));
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML_WhenCreateSeparateConsolCostBasedOnEachLine_APCRD()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML(TransactionTypes.CreditNote, true);
			AssertEquals(TransactionTypes.CreditNote, result.AH_TransactionType);
			AssertEquals(60m, result.AH_LocalTotal);
			AssertEquals(60m, result.AH_OSTotal);
			AssertEquals(8, result.Lines.Count);

			var linesCC1 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK.ToGuid()));
			AssertEquals(4, linesCC1.Length);
			AssertEquals(true, linesCC1.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.UnitedStates));
			AssertEquals(30m, linesCC1.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesCC1.Sum(x => x.AL_OSAmount));

			var linesCC2 = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC2.PK.ToGuid()));
			AssertEquals(4, linesCC2.Length);
			AssertEquals(true, linesCC2.All(x => x.AL_RX_NKTransactionCurrency == CurrencyCodes.Australia));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesCC2.Sum(x => x.AL_OSAmount));
		}

		InvoicingBase AssertAddLinesFromLinkedRequestWithUniversalXML_TransactionLineCurrencyShouldBeConsistentWithUniversalXML(string transactionType, bool isJobIsEmpty)
		{
			var isInvoice = transactionType == TransactionTypes.Invoice;
			var universalTransaction = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>SYD</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>{(isInvoice ? "-" : "")}60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>{(isInvoice ? "-" : "")}60.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>{transactionType}</TransactionType>

    <PostingJournalCollection>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        {(isJobIsEmpty ? "" : @"<Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>")}
        <OSAmount>{(isInvoice ? "-" : "")}30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}30</OSTotalAmount>
        <Sequence>1</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        {(isJobIsEmpty ? "" : @"<Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>")}
        <OSAmount>{(isInvoice ? "-" : "")}30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}30</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC2</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        {(isJobIsEmpty ? "" : @"<Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>")}
        <OSAmount>{(isInvoice ? "-" : "")}15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}15</OSTotalAmount>
        <Sequence>3</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC2</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        {(isJobIsEmpty ? "" : @"<Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>")}
        <OSAmount>{(isInvoice ? "-" : "")}15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}15</OSTotalAmount>
        <Sequence>4</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

    </PostingJournalCollection>

	<ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>

  </TransactionInfo>
</UniversalTransaction>";

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2M);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;
			TestObjectCreator.CC2.AC_AT_GSTRate = TestObjectCreator.GSTFREE2.PK;

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, isInvoice ? 60 : -60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			return result;
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_ShouldGroupByTransactionLineCurrency_APINV()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_ShouldGroupByTransactionLineCurrency(TransactionTypes.Invoice);
			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-60m, result.AH_LocalTotal);
			AssertEquals(-60m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			var linesUSD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.UnitedStates));
			AssertEquals(2, linesUSD.Length);
			AssertEquals(30m, linesUSD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesUSD.Sum(x => x.AL_OverseasTotal));

			var linesAUD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.Australia));
			AssertEquals(2, linesAUD.Length);
			AssertEquals(30m, linesAUD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesAUD.Sum(x => x.AL_OverseasTotal));
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_ShouldGroupByTransactionLineCurrency_APCRD()
		{
			var result = AssertAddLinesFromLinkedRequestWithUniversalXML_ShouldGroupByTransactionLineCurrency(TransactionTypes.CreditNote);
			AssertEquals(TransactionTypes.CreditNote, result.AH_TransactionType);
			AssertEquals(60m, result.AH_LocalTotal);
			AssertEquals(60m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			var linesUSD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.UnitedStates));
			AssertEquals(2, linesUSD.Length);
			AssertEquals(30m, linesUSD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(60m, linesUSD.Sum(x => x.AL_OverseasTotal));

			var linesAUD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.Australia));
			AssertEquals(2, linesAUD.Length);
			AssertEquals(30m, linesAUD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesAUD.Sum(x => x.AL_OverseasTotal));
		}

		InvoicingBase AssertAddLinesFromLinkedRequestWithUniversalXML_ShouldGroupByTransactionLineCurrency(string transactionType)
		{
			var isInvoice = transactionType == TransactionTypes.Invoice;
			var universalTransaction = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>SYD</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>{(isInvoice ? "-" : "")}60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>{(isInvoice ? "-" : "")}60.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>{transactionType}</TransactionType>

    <PostingJournalCollection>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>{(isInvoice ? "-" : "")}30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}30</OSTotalAmount>
        <Sequence>1</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>{(isInvoice ? "-" : "")}30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}30</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>{(isInvoice ? "-" : "")}15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}15</OSTotalAmount>
        <Sequence>3</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>{(isInvoice ? "-" : "")}15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>{(isInvoice ? "-" : "")}15</OSTotalAmount>
        <Sequence>4</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

    </PostingJournalCollection>

	<ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>

  </TransactionInfo>
</UniversalTransaction>";

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2M);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, isInvoice ? 60 : -60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			return result;
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_GroupByTransactionLineCurrency_ShouldNotContainZeroValuedConsolCost()
		{
			var universalTransaction = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>SYD</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-60.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>-30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-30</OSTotalAmount>
        <Sequence>1</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>USD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>30</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>30</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>-15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-15</OSTotalAmount>
        <Sequence>3</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>-15</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-15</OSTotalAmount>
        <Sequence>4</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>

    </PostingJournalCollection>

	<ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>

  </TransactionInfo>
</UniversalTransaction>";

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2M);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			result.RunPreSaveValidation();

			Assert(!result.NotificationsIncludingChildren.ContainsNotificationContaining(@"The related consol cost is invalid, please fix the following errors in the consol cost from which this line was apportioned:
Local Cost Amount: Please enter a Local Cost Amount.
Overseas Cost Amount: Please enter an Overseas Cost Amount."));

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-30m, result.AH_LocalTotal);
			AssertEquals(-30m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			var linesUSD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.UnitedStates));
			AssertEquals(2, linesUSD.Length);
			AssertEquals(0m, linesUSD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(0m, linesUSD.Sum(x => x.AL_OverseasTotal));
			AssertEquals(true, linesUSD.All(x => x.ConsolIDFromApportionedCharge.IsEmpty));

			var linesAUD = (InvoicingLineBase[])result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency, CurrencyCodes.Australia));
			AssertEquals(2, linesAUD.Length);
			AssertEquals(30m, linesAUD.Sum(x => x.AL_LocalTotalAmount));
			AssertEquals(30m, linesAUD.Sum(x => x.AL_OverseasTotal));
			AssertEquals(true, linesAUD.All(x => !x.ConsolIDFromApportionedCharge.IsEmpty));
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_ChargeTaxIsCopiedIntoLine()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, "charge line 1", TestObjectCreator.AUD, 90.91M, TestObjectCreator.AALSHI, null, null, 9.09M, null);

			TestObjectCreator.AALSHI.AddRelatedParty(TestObjectCreator.ABIGAS.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			AssertEquals("Precondition:", TestObjectCreator.GST1.PK, charge.JR_AT_CostGSTRate);

			TestObjectCreator.MRG100.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				AssertEquals("Line Tax is copied from charge", charge.JR_AT_CostGSTRate, result.Lines[0].AL_AT);
				AssertEquals("GST for charge is still the same before allocating transaction", TestObjectCreator.GST1.PK, charge.JR_AT_CostGSTRate);

				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_ForApprotionedCharges()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 100M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.Branch", "SYD", line.Branch.GB_Code);
				AssertEquals("line.Department", "BRN", line.Department.GE_Code);
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.Description", "FREIGHT REVENUE ACTUAL", line.AL_Desc);
				AssertEquals("line.IsFinalCharge", true, line.AL_IsFinalCharge);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("line.Sequence", 2, (int)line.AL_Sequence);
				AssertNull("Consol will not be linked to the line as it was not present in the xml", line.Consol);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.Branch", "BNE", line.Branch.GB_Code);
				AssertEquals("line.Department", "FEA", line.Department.GE_Code);
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.Description", "International Freight", line.AL_Desc);
				AssertEquals("line.IsFinalCharge", false, line.AL_IsFinalCharge);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("line.Sequence", 1, (int)line.AL_Sequence);
				AssertNotNull("Consol will be linked to the line even though it was not mentioned in the xml", line.Consol);

				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_NotionalShipmentNumber()
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 4", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromShipmentXml_NotionalShipmentNumber, false);

			var shipment = TestObjectCreator.CreateShipment("S001001", saveIt: false, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP4");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 1", TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 2", TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS, null, null, 0M, null);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);

				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_Consol_ForApprotionedCharges()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithSingleLineFromConsolXml, true);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00000010");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 100M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.Branch", "BNE", line.Branch.GB_Code);
				AssertEquals("line.Department", "FEA", line.Department.GE_Code);
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.Description", "International Freight", line.AL_Desc);
				AssertEquals("line.IsFinalCharge", true, line.AL_IsFinalCharge);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("line.Sequence", 1, (int)line.AL_Sequence);
				AssertNotNull("Consol will not be linked to the line as it was not present in the xml", line.Consol);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 1, result.Lines.Count);
				var line = result.Lines[0];
				AssertEquals("line.Branch", "BNE", line.Branch.GB_Code);
				AssertEquals("line.Department", "FEA", line.Department.GE_Code);
				AssertEquals("line.ChargeCode", "FRT", line.ChargeCode.AC_Code);
				AssertEquals("line.Job", "S001001", line.Job.JH_JobNum);
				AssertEquals("line.Description", "International Freight", line.AL_Desc);
				AssertEquals("line.IsFinalCharge", false, line.AL_IsFinalCharge);
				AssertEquals("line.OSAmount", -100m, line.AL_OSAmount);
				AssertEquals("line.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("line.Sequence", 1, (int)line.AL_Sequence);
				AssertNotNull("Consol will be linked to the line even though it was not mentioned in the xml", line.Consol);

				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MultipleLines()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineXml, true);

			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, "charge line 1", TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS, null, null, 100M, null);

			var shipment2 = TestObjectCreator.CreateShipment("S001002", false);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "charge line 1", TestObjectCreator.USD, 200M, TestObjectCreator.ABIGAS, null, null, 200M, null);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				result.Lines.Sort("AL_Sequence", System.ComponentModel.ListSortDirection.Ascending);

				CombineAssertions(() =>
				{
					var line = result.Lines[0];
					AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line1.Department", "FES", line.Department.GE_Code);
					AssertEquals("line1.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line1.Job", "S001001", line.Job.JH_JobNum);
					AssertEquals("line1.Description", "Some line text", line.AL_Desc);
					AssertEquals("line1.IsFinalCharge", true, line.AL_IsFinalCharge);
					AssertEquals("line1.OSAmount", -100m, line.AL_OSAmount);
					AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line1.Sequence", 3, (int)line.AL_Sequence);
					AssertNull("Consol will not be linked to the line", line.Consol);

					line = result.Lines[1];
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.Department", "FES", line.Department.GE_Code);
					AssertEquals("line2.ChargeCode", "ZZCC3", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S001002", line.Job.JH_JobNum);
					AssertEquals("line2.Description", "Some line text", line.AL_Desc);
					AssertEquals("line2.IsFinalCharge", true, line.AL_IsFinalCharge);
					AssertEquals("line2.OSAmount", -200m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "USD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.Sequence", 4, (int)line.AL_Sequence);
					AssertNull("Consol will not be linked to the line", line.Consol);
				}
				);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
				result.Lines.Sort("AL_Sequence", System.ComponentModel.ListSortDirection.Ascending);

				CombineAssertions(() =>
				{
					var line = result.Lines[0];
					AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line1.Department", "FES", line.Department.GE_Code);
					AssertEquals("line1.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line1.Job", "S001001", line.Job.JH_JobNum);
					AssertEquals("line1.Description", "International Freight", line.AL_Desc);
					AssertEquals("line1.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertEquals("line1.OSAmount", -100m, line.AL_OSAmount);
					AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line1.Sequence", 1, (int)line.AL_Sequence);
					AssertNull("Consol will not be linked to the line", line.Consol);

					line = result.Lines[1];
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.Department", "FES", line.Department.GE_Code);
					AssertEquals("line2.ChargeCode", "ZZCC3", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S001002", line.Job.JH_JobNum);
					AssertEquals("line2.Description", "Charge Code 3", line.AL_Desc);
					AssertEquals("line2.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertEquals("line2.OSAmount", -200m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "USD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.Sequence", 2, (int)line.AL_Sequence);
					AssertNull("Consol will not be linked to the line", line.Consol);
				}
				);
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MultipleLines_MatchesToApportionLines_ButConsolHasMoreShipments()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 3", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineXml3, true);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment3 = TestObjectCreator.CreateShipment("S001003", consol);
			TestObjectCreator.CreateJob(shipment3, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "MAN";
			cost1.E6_OSCostAmount = 600M;

			AssertEquals(3, cost1.ApportionmentCharges.Count);
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 100M;
			cost1.ApportionmentCharges[1].JR_OSCostAmt = 200M;
			cost1.ApportionmentCharges[2].JR_OSCostAmt = 300M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				result.Lines.Sort("AL_Sequence", System.ComponentModel.ListSortDirection.Ascending);

				CombineAssertions(() =>
				{
					AssertLinesCreatedFromXml(result);
				}
				);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				Assert("No AMD log added", !result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
				result.Lines.Sort("AL_Sequence", System.ComponentModel.ListSortDirection.Ascending);

				CombineAssertions(() =>
				{
					AssertLinesCreatedFromXml(result);
				}
				);
			}
		}

		static void AssertLinesCreatedFromXml(InvoicingBase result)
		{
			var line = result.Lines[0];
			AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
			AssertEquals("line1.Department", "FEA", line.Department.GE_Code);
			AssertEquals("line1.ChargeCode", "FRT", line.ChargeCode.AC_Code);
			AssertEquals("line1.Job", "S001001", line.Job.JH_JobNum);
			AssertEquals("line1.Description", "Some line text", line.AL_Desc);
			AssertEquals("line1.IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("line1.OSAmount", -100m, line.AL_OSAmount);
			AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals("line1.Sequence", 3, (int)line.AL_Sequence);
			AssertNull("Consol will not be linked to the line", line.Consol);

			line = result.Lines[1];
			AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
			AssertEquals("line2.Department", "FEA", line.Department.GE_Code);
			AssertEquals("line2.ChargeCode", "FRT", line.ChargeCode.AC_Code);
			AssertEquals("line2.Job", "S001002", line.Job.JH_JobNum);
			AssertEquals("line2.Description", "Some line text", line.AL_Desc);
			AssertEquals("line2.IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("line2.OSAmount", -200m, line.AL_OSAmount);
			AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals("line2.Sequence", 4, (int)line.AL_Sequence);
			AssertNull("Consol will not be linked to the line", line.Consol);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_Consol_MultipleLines_ApportionedAmountDoesMatch_NoOfShipmentsDoNoMatch()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 4", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineConsolXml2, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "11223343";

			var shipment1 = TestObjectCreator.CreateShipment("S0001000", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP5");
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S0001001", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP6");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment3 = TestObjectCreator.CreateShipment("S0001002", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP7");
			TestObjectCreator.CreateJob(shipment3, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 7500M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);

				CombineAssertions(() =>
				{
					AssertLinesCreatedFromXml(job1, job2, result);
				}
				);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				Assert("AMD log not added", !result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));

				CombineAssertions(() =>
				{
					AssertLinesCreatedFromXml(job1, job2, result);
				}
				);
			}
		}

		static void AssertLinesCreatedFromXml(Job job1, Job job2, InvoicingBase result)
		{
			var line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job1.PK.ToGuid()))[0] as InvoicingLineBase;
			AssertNotNull(line);
			AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
			AssertEquals("line1.ChargeCode", "FRT", line.ChargeCode.AC_Code);
			AssertEquals("line1.Job", "S0001000", line.Job.JH_JobNum);
			AssertEquals("line1.OSAmount", -2500.00m, line.AL_OSAmount);
			AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals("line1.IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertNotNull("Consol linked to the line", line.Consol);

			line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job2.PK.ToGuid()))[0] as InvoicingLineBase;
			AssertNotNull(line);
			AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
			AssertEquals("line2.ChargeCode", "FRT", line.ChargeCode.AC_Code);
			AssertEquals("line2.Job", "S0001001", line.Job.JH_JobNum);
			AssertEquals("line2.OSAmount", -2500.00m, line.AL_OSAmount);
			AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals("line2.IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertNotNull("Consol linked to the line", line.Consol);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_Consol_MultipleLinesWithDifferentCurrency()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 4", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineConsolXml3, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "11223343";

			var shipment1 = TestObjectCreator.CreateShipment("S0001000", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP5");
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S0001001", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP6");
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 1M);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost1.E6_ApportionmentMethod = "MAN";
			cost1.E6_OSCostAmount = 60M;

			cost1.ApportionmentCharges[0].JR_OSCostAmt = 20M;
			cost1.ApportionmentCharges[1].JR_OSCostAmt = 40M;

			Factory.Save();

			var cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost2.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost2.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			cost2.E6_ApportionmentMethod = "MAN";
			cost2.E6_OSCostAmount = 40M;

			cost2.ApportionmentCharges[0].JR_OSCostAmt = 10M;
			cost2.ApportionmentCharges[1].JR_OSCostAmt = 30M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 4, result.Lines.Count);
				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_Consol_MultipleLines_ApportionedAmountMatchesButChargeCodeDoesNotMatch()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 4", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineConsolXml2, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "11223343";

			var shipment1 = TestObjectCreator.CreateShipment("S0001000", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP5");
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S0001001", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP6");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.MRG100.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 5000M;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);

				CombineAssertions(() =>
				{
					var line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job1.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line1.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line1.Job", "S0001000", line.Job.JH_JobNum);
					AssertEquals("line1.OSAmount", -2500.00m, line.AL_OSAmount);
					AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line1.IsFinalCharge", true, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);

					line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job2.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S0001001", line.Job.JH_JobNum);
					AssertEquals("line2.OSAmount", -2500.00m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.IsFinalCharge", true, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);
				}
				);
			}

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 2, result.Lines.Count);
				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));

				CombineAssertions(() =>
				{
					var line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job1.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line1.ChargeCode", "ZZMRG100", line.ChargeCode.AC_Code);
					AssertEquals("line1.Job", "S0001000", line.Job.JH_JobNum);
					AssertEquals("line1.OSAmount", -2500.00m, line.AL_OSAmount);
					AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line1.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);

					line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job2.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.ChargeCode", "ZZMRG100", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S0001001", line.Job.JH_JobNum);
					AssertEquals("line2.OSAmount", -2500.00m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);
				}
				);
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MultipleLines_MatchesInMixtureOfJobAndConsolCharges()
		{
			TestObjectCreator.CreateNewCompany("DAU", orgProxy: TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV 4", TestObjectCreator.Creditor1, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();

			request.Initialize(invoicePendingAllocation, UniversalTransactionWithMultipleLineXml2, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			var shipment1 = TestObjectCreator.CreateShipment("S0001000", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP4");
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var shipment2 = TestObjectCreator.CreateShipment("S0001001", consol: consol, transportMode: Core.Constants.TransportModes.Air, housebill: "HBILLIMPORTAP5");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			cost1.E6_ApportionmentMethod = "MAN";
			cost1.E6_OSCostAmount = 50M;

			AssertEquals(2, cost1.ApportionmentCharges.Count);
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 30M;
			cost1.ApportionmentCharges[1].JR_OSCostAmt = 20M;

			Factory.Save();

			TestObjectCreator.CreateCharge(job1, TestObjectCreator.RevenueNoTaxChargeCode, "charge line 1", TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS, null, null, 0M, null);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				Assert(result is APInvoice);
				AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
				AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
				AssertEquals("Lines.Count", 3, result.Lines.Count);
				Assert("AMD log added", result.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code));

				CombineAssertions(() =>
				{
					var line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.RevenueNoTaxChargeCode.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line1.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line1.ChargeCode", "ZZREVFRE", line.ChargeCode.AC_Code);
					AssertEquals("line1.Job", "S0001000", line.Job.JH_JobNum);
					AssertEquals("line1.OSAmount", -10.00m, line.AL_OSAmount);
					AssertEquals("line1.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line1.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertNull("No Consol linked to the line", line.Consol);

					line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job1.PK.ToGuid()).AddToFilter(AccTransactionLinesSchema.AL_AC, TestObjectCreator.FRT.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S0001000", line.Job.JH_JobNum);
					AssertEquals("line2.OSAmount", -30.00m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);

					line = result.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job2.PK.ToGuid()).AddToFilter(AccTransactionLinesSchema.AL_AC, TestObjectCreator.FRT.PK.ToGuid()))[0] as InvoicingLineBase;
					AssertNotNull(line);
					AssertEquals("line2.Branch", "BNE", line.Branch.GB_Code);
					AssertEquals("line2.ChargeCode", "FRT", line.ChargeCode.AC_Code);
					AssertEquals("line2.Job", "S0001001", line.Job.JH_JobNum);
					AssertEquals("line2.OSAmount", -20.00m, line.AL_OSAmount);
					AssertEquals("line2.OSCurrency", "AUD", line.AL_RX_NKTransactionCurrency);
					AssertEquals("line2.IsFinalCharge", false, line.AL_IsFinalCharge);
					AssertNotNull("Consol linked to the line", line.Consol);
				}
				);
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost_PrimaryKey()
		{
			var matchingCriteriaCollection1Xml = @"
				<MatchingCriteria>
					<FieldName>PrimaryKey</FieldName>
					<Value>#JobCharge1PK#</Value>
				</MatchingCriteria>";

			var matchingCriteriaCollection2Xml = @"
				<MatchingCriteria>
					<FieldName>PrimaryKey</FieldName>
					<Value>#JobCharge2PK#</Value>
				</MatchingCriteria>";

			var universalTransaction = string.Format(UniversalTransactionWithTwoCustomizableMatchingCriteriaCollectionXml, matchingCriteriaCollection1Xml, matchingCriteriaCollection2Xml);

			AssertAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost(universalTransaction);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost_DisplaySequence()
		{
			var matchingCriteriaCollection1Xml = @"
				<MatchingCriteria>
					<FieldName>DisplaySequence</FieldName>
					<Value>2</Value>
				</MatchingCriteria>";

			var matchingCriteriaCollection2Xml = @"
				<MatchingCriteria>
					<FieldName>DisplaySequence</FieldName>
					<Value>2</Value>
				</MatchingCriteria>";

			var universalTransaction = string.Format(UniversalTransactionWithTwoCustomizableMatchingCriteriaCollectionXml, matchingCriteriaCollection1Xml, matchingCriteriaCollection2Xml);

			AssertAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost(universalTransaction);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost_MixedCriteria()
		{
			var matchingCriteriaCollection1Xml = @"
				<MatchingCriteria>
					<FieldName>DisplaySequence</FieldName>
					<Value>1</Value>
				</MatchingCriteria>
				<MatchingCriteria>
					<FieldName>PrimaryKey</FieldName>
					<Value>#JobCharge1PK#</Value>
				</MatchingCriteria>";

			var matchingCriteriaCollection2Xml = @"
				<MatchingCriteria>
					<FieldName>DisplaySequence</FieldName>
					<Value>1</Value>
				</MatchingCriteria>
				<MatchingCriteria>
					<FieldName>PrimaryKey</FieldName>
					<Value>#JobCharge2PK#</Value>
				</MatchingCriteria>";

			var universalTransaction = string.Format(UniversalTransactionWithTwoCustomizableMatchingCriteriaCollectionXml, matchingCriteriaCollection1Xml, matchingCriteriaCollection2Xml);

			AssertAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost(universalTransaction);
		}

		void AssertAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost(string universalTransaction)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_ActualWeight = 100m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_ActualWeight = 200m;
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			consolCost1.E6_OSCostAmount = 3000m;
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			consolCost2.E6_OSCostAmount = 9000m;

			Factory.Save();

			var jobCharge1 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge2 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			universalTransaction = universalTransaction.Replace("#JobCharge1PK#", jobCharge1.PK.ToString()).Replace("#JobCharge2PK#", jobCharge2.PK.ToString());
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 300);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-300m, result.AH_LocalTotal);
			AssertEquals(-300m, result.AH_OSTotal);
			AssertEquals(2, result.Lines.Count);

			AssertEquals(1, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK should be set and equal to the consol cost 2", consolCost2.PK, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);

			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnConsol = consolCost2.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			result.Factory.Save();

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			AssertEquals("There should be 3 consol costs for the consol now", 3, consolCosts.Length);
			consolCost1.Reload();
			consolCost2.Reload();
			var newConsolCost = consolCosts.Single(x => x.PK != consolCost1.PK && x.PK != consolCost2.PK);
			AssertEquals("consolCost2 should be matched and then consumed", 300m, consolCost2.E6_OSCostAmount);

			var chargesOnConsolCost1 = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("consolCost1 shouldn't be updated", 3000m, consolCost1.E6_OSCostAmount);
			AssertEquals("ConsolCost1 should have a charge associated to job1 with value 1000", true, chargesOnConsolCost1.Any(x => x.JR_JH == job1.PK && x.CostAmount == 1000m));
			AssertEquals("ConsolCost1 should have a charge associated to job2 with value 2000", true, chargesOnConsolCost1.Any(x => x.JR_JH == job2.PK && x.CostAmount == 2000m));

			var chargesOnConsolCost2 = consolCost2.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("consolCost2 should be matched and then consumed", 300m, consolCost2.E6_OSCostAmount);
			AssertEquals("ConsolCost2 should have a charge associated to job1 with value 120", true, chargesOnConsolCost2.Any(x => x.JR_JH == job1.PK && x.CostAmount == 120m));
			AssertEquals("ConsolCost2 should have a charge associated to job2 with value 180", true, chargesOnConsolCost2.Any(x => x.JR_JH == job2.PK && x.CostAmount == 180m));

			var chargesOnNewConsolCost = newConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("newConsolCost should be generated with unpaid amount", 8700m, newConsolCost.E6_OSCostAmount);
			AssertEquals("newConsolCost should have a charge associated to job1 with value 2880 (3000-120)", true, chargesOnNewConsolCost.Any(x => x.JR_JH == job1.PK && x.CostAmount == 2880m));
			AssertEquals("newConsolCost should have a charge associated to job2 with value 5820 (5000-180)", true, chargesOnNewConsolCost.Any(x => x.JR_JH == job2.PK && x.CostAmount == 5820m));
		}

		public void TestImportXUTWithTaxValueNotMatchingTaxCodeRate_DoesNotThrowErrorDueToTaxValueMismatch()
		{
			// Setting the ChargeCode2 tax rate to 10%
			TestObjectCreator.CC2.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_CoLoadMasterBill = "MasterBill1";
			consol.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			// Setting the XUT with tax values in <PostingJournal> elements set to 5% of the cost
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 200, 10);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, UniversalTransactionWithCustomTaxRatedValuesInPostingJournalCollectionXml, false);

			Factory.Save();

			var (result, errorMsg) = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation);

			AssertEquals(null, errorMsg);
			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-210m, result.AH_LocalTotal);
			AssertEquals(-210m, result.AH_OSTotal);
			AssertEquals(200m, result.AH_OSExTaxAmount);
			AssertEquals(10m, result.AH_OSTaxAmount);
			AssertEquals("The factory on converted invoice should have BusinessContext.AllocatingTransaction removed after processing", false, result.Factory.HasContext(BusinessContext.AllocatingTransaction));

			AssertEquals(1, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals(2, result.Lines.Count);

			var expectedMessage = "Tax amount entered is outside the expected value for the selected tax rate";

			foreach (var line in result.Lines)
			{
				var transactionLine = (TransactionLine)line;

				// No tax mismatch warning on the returned lines
				AssertNoWarning(transactionLine.AL_OSTaxAmountInfo, expectedMessage);

				AssertEquals(100m, transactionLine.AL_DBAH_OSExTaxAmount);
				AssertEquals("New invoice line tax amount should be 5% of the cost amount", 5m, transactionLine.AL_OSTaxAmount);
				AssertEquals("New invoice line local tax rate should be 10%", 10m, transactionLine.AL_TaxRateCalc);
			}

			result.RunPreSaveValidation();

			foreach (var line in result.Lines)
			{
				var transactionLine = (TransactionLine)line;
				// Tax mismatch warning appears on the returned lines after the RunPreSaveValidation() call
				AssertHasWarning("There should be a tax mismatch warning", transactionLine.AL_OSTaxAmountInfo, expectedMessage);
			}

			result.Factory.Save();

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			AssertEquals("There should be a consol cost for the consol now", 1, consolCosts.Length);
			var newConsolCost = consolCosts.Single();

			var chargesOnNewConsolCost = newConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals(200m, newConsolCost.E6_OSCostAmount);
			AssertEquals("New consolCost tax amount should be 5% of the cost amount", 10m, newConsolCost.E6_Calc_LocalGSTAmount);

			foreach (var apportionSplitCharge in chargesOnNewConsolCost)
			{
				AssertEquals(100m, apportionSplitCharge.CostAmount);
				AssertEquals("New consolCost line tax amount should be 5% of the cost amount", 5m, apportionSplitCharge.JR_OSCostGSTAmt_Calc);
				AssertEquals("New consolCost line local calculated tax amount should be 10% of the cost amount", 10m, apportionSplitCharge.JR_Calc_LocalSellTaxAmt);
			}
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchesToShipmentsAccrualInConsolCost_PartiallyMatching()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_ActualWeight = 100m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_ActualWeight = 200m;
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			consolCost1.E6_OSCostAmount = 3000m;

			Factory.Save();

			var jobCharge1 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);

			var postingJournalCollectionXml = $@"
            <PostingJournal>
                <Branch>
                    <Code>SYD</Code>
                </Branch>
                <ChargeCode>
                    <Code>ZZCC1</Code>
                </ChargeCode>
                <ChargeCurrency>
                    <Code>AUD</Code>
                </ChargeCurrency>
                <CostSource>
                    <Type>ForwardingConsol</Type>
                    <Key>C001</Key>
                </CostSource>
                <Department>
                    <Code>BRN</Code>
                </Department>
                <Description>FREIGHT REVENUE ACTUAL</Description>
                <IsFinalCharge>false</IsFinalCharge>
                <Job>
                    <Type>Job</Type>
                    <Key>S001</Key>
                </Job>
                <OSAmount>-300.0000</OSAmount>
                <OSGSTVATAmount>0.0000</OSGSTVATAmount>
                <OSTotalAmount>-300.0000</OSTotalAmount>
                <Sequence>1</Sequence>
                <ImportMetaData>
                    <Instruction>UpdateAndInsertIfNotFound</Instruction>
                    <MatchingCriteriaCollection>
                        <MatchingCriteria>
                            <FieldName>PrimaryKey</FieldName>
                            <Value>{jobCharge1.PK}</Value>
                        </MatchingCriteria>
                    </MatchingCriteriaCollection>
                </ImportMetaData>
            </PostingJournal>";

			var universalTransaction = String.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, postingJournalCollectionXml);

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 300);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-300m, result.AH_LocalTotal);
			AssertEquals(-300m, result.AH_OSTotal);
			AssertEquals(1, result.Lines.Count);

			AssertEquals(1, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK should be set and equal to the PK of consol cost 1", consolCost1.PK, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);

			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnConsol = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);

			result.Factory.Save();

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			AssertEquals("There should be 2 consol costs for the consol now", 2, consolCosts.Length);
			consolCost1.Reload();
			var newConsolCost = consolCosts.Single(x => x.PK != consolCost1.PK);

			var chargesOnConsolCost1 = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("consolCost1 should be updated, the amount in invoice will be deducted", 2700m, consolCost1.E6_OSCostAmount);
			AssertEquals("ConsolCost1 should have a charge associated to job1 with value 700 (1000-300)", true, chargesOnConsolCost1.Any(x => x.JR_JH == job1.PK && x.CostAmount == 700m));
			AssertEquals("ConsolCost1 should have a charge associated to job2 with value 2000", true, chargesOnConsolCost1.Any(x => x.JR_JH == job2.PK && x.CostAmount == 2000m));

			var chargesOnNewConsolCost = newConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("New consol cost should be generated with paid amount", 300m, newConsolCost.E6_OSCostAmount);
			AssertEquals("New consol cost should have a charge associated to job1 with value 300", true, chargesOnNewConsolCost.Any(x => x.JR_JH == job1.PK && x.CostAmount == 300m));
			AssertEquals("New consol cost should have only one charge", 1, chargesOnNewConsolCost.Count());
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchingCriteria_OneLineMatchedToOneConsolCost()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 32m, TestObjectCreator.Creditor1);

			Factory.Save();

			var universalTransaction = UniversalTransactionWithMatchingCriteriaXml
				.Replace("{PrimaryKeyOfConsolCost}", consolCost2.PK.ToString());

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-30m, result.AH_LocalTotal);
			AssertEquals(-30m, result.AH_OSTotal);
			AssertEquals(2, result.Lines.Count);

			AssertEquals(1, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK should be set", consolCost2.PK, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);

			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnConsol = consolCost2.ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			result.Factory.Save();

			consolCost1.Reload();
			consolCost2.Reload();
			AssertEquals("consolCost1 shouldn't be updated", 31m, consolCost1.E6_OSCostAmount);
			AssertEquals("consolCost2 should be matched and then updated", 30m, consolCost2.E6_OSCostAmount);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchingCriteria_TwoLinesMatchedToOneConsolCost()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 21m, TestObjectCreator.Creditor1);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 11m, TestObjectCreator.Creditor1);

			Factory.Save();

			var universalTransaction = UniversalTransactionWithMultipleMatchingCriteriaXml
				.Replace("{PrimaryKeyOfConsolCostForLine1}", consolCost1.PK.ToString())
				.Replace("{PrimaryKeyOfConsolCostForLine2}", consolCost1.PK.ToString());

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-30m, result.AH_LocalTotal);
			AssertEquals(-30m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			AssertEquals(2, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK should be set", consolCost1.PK, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);
			AssertEquals("RelatedConsolCostPK should be set", consolCost1.PK, result.ConsolCosting.ConsolCosts[1].RelatedConsolCostPK);

			var chargesOnConsol = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();

			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			chargesOnInvoice = result.ConsolCosting.ConsolCosts[1].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			result.Factory.Save();

			consolCost1.Reload();
			consolCost2.Reload();
			AssertEquals("consolCost1 should be matched to line 1 by design, and then updated", 10m, consolCost1.E6_OSCostAmount);
			AssertEquals("consolCost2 should be matched to line 2 automatically, and then updated", 20m, consolCost2.E6_OSCostAmount);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchingCriteria_TwoLinesMatchedToTwoConsolCosts()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 21m, TestObjectCreator.Creditor1);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 11m, TestObjectCreator.Creditor1);

			Factory.Save();

			var universalTransaction = UniversalTransactionWithMultipleMatchingCriteriaXml
				.Replace("{PrimaryKeyOfConsolCostForLine1}", consolCost1.PK.ToString())
				.Replace("{PrimaryKeyOfConsolCostForLine2}", consolCost2.PK.ToString());

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-30m, result.AH_LocalTotal);
			AssertEquals(-30m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			AssertEquals(2, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK should be set", consolCost1.PK, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);
			AssertEquals("RelatedConsolCostPK should be set", consolCost2.PK, result.ConsolCosting.ConsolCosts[1].RelatedConsolCostPK);

			var chargesOnConsol = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			chargesOnConsol = consolCost2.ApportionmentCharges.OfType<ApportionSplitCharge>();
			chargesOnInvoice = result.ConsolCosting.ConsolCosts[1].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
			AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);

			result.Factory.Save();

			consolCost1.Reload();
			consolCost2.Reload();
			AssertEquals("consolCost1 should be matched to line 1 by design, and then updated", 10m, consolCost1.E6_OSCostAmount);
			AssertEquals("consolCost2 should be matched to line 2 by design, and then updated", 20m, consolCost2.E6_OSCostAmount);
		}

		public void TestAddLinesFromLinkedRequestWithUniversalXML_MatchingCriteria_MatchedToInvalidConsolCostPKs()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 21m, TestObjectCreator.Creditor1);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 11m, TestObjectCreator.Creditor1);

			Factory.Save();

			var invalidConsolCostPK1 = ZGuid.NewZGuid();
			var invalidConsolCostPK2 = ZGuid.NewZGuid();

			var universalTransaction = UniversalTransactionWithMultipleMatchingCriteriaXml
				.Replace("{PrimaryKeyOfConsolCostForLine1}", invalidConsolCostPK1.ToString())
				.Replace("{PrimaryKeyOfConsolCostForLine2}", invalidConsolCostPK2.ToString());

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("001", TestObjectCreator.Creditor1, 60);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);
			AssertEquals(-30m, result.AH_LocalTotal);
			AssertEquals(-30m, result.AH_OSTotal);
			AssertEquals(4, result.Lines.Count);

			AssertEquals(2, result.ConsolCosting.ConsolCosts.Count);
			AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, result.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK);
			AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, result.ConsolCosting.ConsolCosts[1].RelatedConsolCostPK);

			var chargesOnInvoice = result.ConsolCosting.ConsolCosts[0].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));

			chargesOnInvoice = result.ConsolCosting.ConsolCosts[1].ApportionmentCharges.OfType<ApportionSplitCharge>();
			AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));

			result.Factory.Save();

			consolCost1.Reload();
			consolCost2.Reload();
			AssertEquals("consolCost1 should be matched to line 2 automatically, and then updated", 20m, consolCost1.E6_OSCostAmount);
			AssertEquals("consolCost2 should be matched to line 1 automatically, and then updated", 10m, consolCost2.E6_OSCostAmount);
		}

		[TestDate(2025, 4, 22)]
		public void TestAddLinesFromLinkedRequestWithUniversalXML_HasLocalAmount()
		{
			PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked();

			var postToShipmentLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(false, true));
			var postToApportionedSplitChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(true, true));
			var postToConsolLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalWithSingleForeignLineToConsolXml(true));

			var transNum = 1;

			CombineAssertions("Case: Foreign Invoice, IsRegistryDEF = true, ShouldUseJobExRate = true", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, true, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Foreign Invoice, IsRegistryDEF = true, ShouldUseJobExRate = false", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, true, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Foreign Invoice, IsRegistryDEF = false, ShouldUseJobExRate = true", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, false, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Foreign Invoice, IsRegistryDEF = false, ShouldUseJobExRate = false", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, false, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice, IsRegistryDEF = true, ShouldUseJobExRate = true", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, true, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice, IsRegistryDEF = true, ShouldUseJobExRate = false", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, true, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice, IsRegistryDEF = false, ShouldUseJobExRate = true", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, false, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice, IsRegistryDEF = false, ShouldUseJobExRate = false", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, false, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6583m, 107.1m, 249.9m);
					transNum++;
				}
			});
		}

		[TestDate(2025, 4, 22)]
		public void TestAddLinesFromLinkedRequestWithUniversalXML_NoLocalAmount_InvoicePostingExchangeRateOptionIsDEF_NotUseJobExRate()
		{
			PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked();

			var postToShipmentLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(false, false));
			var postToApportionedSplitChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(true, false));
			var postToConsolLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalWithSingleForeignLineToConsolXml(false));

			var transNum = 1;

			CombineAssertions("Case: Foreign Invoice. Should use exchange rate on the invoice header", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, true, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.8358m, 84.35m, 196.82m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice. Should use today's buy rate", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, true, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.7233M, 97.47m, 227.43m);
					transNum++;
				}
			});
		}

		[TestDate(2025, 4, 22)]
		public void TestAddLinesFromLinkedRequestWithUniversalXML_NoLocalAmount_InvoicePostingExchangeRateOptionIsNotDEF_NotUseJobExRate()
		{
			PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked();

			var postToShipmentLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(false, false));
			var postToApportionedSplitChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(true, false));
			var postToConsolLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalWithSingleForeignLineToConsolXml(false));

			var transNum = 1;

			CombineAssertions("Case: Foreign Invoice. Should use exchange rate on the invoice header", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, false, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.8358m, 84.35m, 196.82m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice. Should use the buy rate as at the date option set in APInvoicePostingExchangeRateOption registry", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, false, false, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6971M, 101.13m, 235.98m);
					transNum++;
				}
			});
		}

		[TestDate(2025, 4, 22)]
		public void TestAddLinesFromLinkedRequestWithUniversalXML_NoLocalAmount_InvoicePostingExchangeRateOptionIsNotDEF_UseJobExRate()
		{
			PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked();

			var postToShipmentLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(false, false));
			var postToApportionedSplitChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(true, false));
			var postToConsolLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalWithSingleForeignLineToConsolXml(false));

			var transNum = 1;

			CombineAssertions("Case: Foreign Invoice. Should use the rate Type as per Job Billing Exchange Rate Configuration at the date option set in APInvoicePostingExchangeRateOption registry", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, false, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.8729m, 80.77m, 188.45m);
					transNum++;
				}
			});

			CombineAssertions("Case: Local Invoice. UseJobExRate will not take effect, so should use the buy rate as at the date option set in APInvoicePostingExchangeRateOption registry", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, false, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.6971M, 101.13m, 235.98m);
					transNum++;
				}
			});
		}

		[TestDate(2025, 4, 22)]
		public void TestAddLinesFromLinkedRequestWithUniversalXML_NoLocalAmount_InvoicePostingExchangeRateOptionIsDEF_UseJobExRate()
		{
			PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked();

			var postToShipmentLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(false, false));
			var postToApportionedSplitChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalsWithTwoForeignLinesXml(true, false));
			var postToConsolLevelChargesXml = string.Format(UniversalTransactionWithCustomizablePostingJournalCollectionXml, GetPostingJournalWithSingleForeignLineToConsolXml(false));

			var transNum = 1;

			CombineAssertions("Case: Foreign Invoice. Should use the rate Type as per Job Billing Exchange Rate Configuration of Today (consol-related charges) / Exchange Rate on Job (shipment-level charges)", () =>
			{
				foreach (var universalTransaction in new[] { postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, true, true, true, universalTransaction, $"INV{transNum}", 0.9855M, 71.54m, 166.92m);
					transNum++;
				}
				AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(true, true, true, false, postToShipmentLevelChargesXml, $"INV{transNum}", 0.5414m, 130.22m, 303.84m);
				transNum++;
			});

			CombineAssertions("Case: Local Invoice. UseJobExRate will not take effect, so should use today's buy rate", () =>
			{
				foreach (var universalTransaction in new[] { postToShipmentLevelChargesXml, postToApportionedSplitChargesXml, postToConsolLevelChargesXml })
				{
					var isConsolRelated = universalTransaction != postToShipmentLevelChargesXml;
					AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(false, true, true, isConsolRelated, universalTransaction, $"INV{transNum}", 0.7233M, 97.47m, 227.43m);
					transNum++;
				}
			});
		}

		void AssertAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked(bool isForeignInvoice, bool isRegistryDEF, bool shouldUseJobExRateFlag, bool isConsolRelated, string universalTransaction, string transactionNum, decimal expectedExRate, decimal expectedLine1Amount, decimal expectedLine2Amount)
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation(transactionNum, TestObjectCreator.Creditor1, isForeignInvoice ? 235m : 357m);
			invoicePendingAllocation.AH_RX_NKTransactionCurrency = isForeignInvoice ? TestObjectCreator.USD.Code : TestObjectCreator.AUD.Code;
			invoicePendingAllocation.AH_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoicePendingAllocation.AH_ExchangeRate = isForeignInvoice ? 0.8358m : 1m;

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(invoicePendingAllocation, universalTransaction, false);
			Factory.Save();

			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryDEF ? AccountingConstants.InvoicePostingExchangeRateOption.Default.Code : AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shouldUseJobExRateFlag))
			{
				var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

				AssertEquals(TransactionTypes.Invoice, result.AH_TransactionType);

				var job1 = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, "S001"));
				var job2 = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, "S002"));

				if (isConsolRelated)
				{
					AssertEquals(1, result.ConsolCosting.ConsolCosts.Count);
					var consolCostOnInvoice = result.ConsolCosting.ConsolCosts[0];
					AssertEquals(235m, consolCostOnInvoice.CostAmount);
					BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected consolCostLocalAmount", expectedLine1Amount + expectedLine2Amount, consolCostOnInvoice.E6_LocalCostAmount, 0.01m);
					BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected consolCostExchangeRate", expectedExRate, consolCostOnInvoice.E6_ExchangeRate, 0.0001m);

					AssertEquals(2, consolCostOnInvoice.ApportionmentCharges.Count);
					var apportionedCharge1 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
					var apportionedCharge2 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
					AssertEquals(70.5m, apportionedCharge1.JR_OSCostAmt);
					AssertEquals(expectedLine1Amount, apportionedCharge1.JR_LocalCostAmt);
					BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected consolCostExchangeRate", expectedExRate, apportionedCharge1.JR_OSCostExRate, 0.0001m);

					AssertEquals(apportionedCharge2.JR_OSCostAmt, 164.5m);
					AssertEquals(expectedLine2Amount, apportionedCharge2.JR_LocalCostAmt);
					BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected consolCostExchangeRate", expectedExRate, apportionedCharge2.JR_OSCostExRate, 0.0001m);
				}
				else
				{
					AssertEquals(0, result.ConsolCosting.ConsolCosts.Count);
				}

				AssertEquals(2, result.Lines.Count);
				var invoiceLine1 = result.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job1.PK);
				var invoiceLine2 = result.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job2.PK);
				AssertEquals(70.5m, invoiceLine1.AL_OSExTaxAmount);
				AssertEquals(expectedLine1Amount, invoiceLine1.AL_LocalExTaxAmount);
				BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected LineExchangeRate", expectedExRate, invoiceLine1.AL_ExchangeRate, 0.0001m);

				AssertEquals(164.5m, invoiceLine2.AL_OSExTaxAmount);
				AssertEquals(expectedLine2Amount, invoiceLine2.AL_LocalExTaxAmount);
				BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected LineExchangeRate", expectedExRate, invoiceLine2.AL_ExchangeRate, 0.0001m);
			}
		}

		void PrepareTestDataForAddLinesFromLinkedRequestWithUniversalXML_WhenForeignLinesLinked()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, Core.Constants.TransportModes.All, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.ExchangeRateTypes.Code.CustomsRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.TransportModes.All, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.ExchangeRateTypes.Code.CustomsRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoiceDate = ZDateTime.Today.AddDays(-5);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6971M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.7233M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 0.8729M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 0.9855M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment1.JS_ActualChargeable = 3m;
			shipment2.JS_ActualChargeable = 7m;

			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job1.ExchangeRates.AddRate(TestObjectCreator.USD, 0.5414m, TestObjectCreator.Creditor1.PK, ExchangeRateOrgTypeEnum.Creditor);
			job2.ExchangeRates.AddRate(TestObjectCreator.USD, 0.5414m, TestObjectCreator.Creditor1.PK, ExchangeRateOrgTypeEnum.Creditor);

			Factory.Save();
		}

		[TestDate(2023, 04, 20, 12, 11, 10)]
		public void TestConvertUnallocatedToAP_BackPostingIsAllowedForAPInvoice_UsesPostDateFromTransactionPendingAllocation()
		{
			// Arrange
			var pastDate = ZDateTime.Today.AddDays(-5);
			var transactionPendingAllocation = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPendingAllocation.AH_PostDate = pastDate;
			transactionPendingAllocation.AH_OSExTaxAmount = 100m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Pre-condition", true, transactionPendingAllocation.AllowBackPosting);

			// Act
			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation).Invoice;

			// Assert
			Assert("pre-condition", invoice is APInvoice);
			AssertEquals(pastDate, invoice.AH_PostDate);
		}

		[TestDate(2023, 04, 20, 12, 11, 10)]
		public void TestConvertUnallocatedToAP_BackPostingIsAllowedForAPCreditNote_UsesPostDateFromTransactionPendingAllocation()
		{
			// Arrange
			var pastDate = ZDateTime.Today.AddDays(-5);
			var transactionPendingAllocation = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPendingAllocation.AH_PostDate = pastDate;
			transactionPendingAllocation.AH_OSExTaxAmount = -100m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Pre-condition", true, transactionPendingAllocation.AllowBackPosting);

			// Act
			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation).Invoice;

			// Assert
			Assert("pre-condition", invoice is APCreditNote);
			AssertEquals(pastDate, invoice.AH_PostDate);
		}

		public void TestConvertUnallocatedToAP_IsComplianceSubTypeNotEligibleForAllocateAsReceivable()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IEnableTransactionsPendingAllocationAllocateAsReceivable>()
				.Setup(x => x.IsComplianceSubTypeNotEligibleForAllocateAsReceivable(It.IsAny<string>()))
				.Returns(false);
			mockICountryComplianceFactory
				.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);

			var transactionPendingAllocation = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPendingAllocation.AH_ComplianceSubType = "ANY";
			Factory.Save();

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation);

			AssertNull(result.Invoice);
			AssertContains("You cannot allocate Return AP Invoice as Payable. Selected invoice must be allocated as Receivable. Transaction number", result.ErrorMessage);

			mockICountryComplianceInfoBase.As<IEnableTransactionsPendingAllocationAllocateAsReceivable>()
				.Setup(x => x.IsComplianceSubTypeNotEligibleForAllocateAsReceivable(It.IsAny<string>()))
				.Returns(true);

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation);

			AssertNotNull(result.Invoice);
			AssertNullOrEmpty(result.ErrorMessage);

			mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()), Times.Exactly(2));
			mockICountryComplianceInfoBase.As<IEnableTransactionsPendingAllocationAllocateAsReceivable>().Verify(x => x.IsComplianceSubTypeNotEligibleForAllocateAsReceivable(It.IsAny<string>()), Times.Exactly(2));
		}

		public void TestConvertUnallocatedToAP_ValidateAndFixTaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var invoicePendingAllocation = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_OH = TestObjectCreator.Creditor1.PK;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Assert(!invoicePendingAllocation.AH_GB_TaxBranch.IsEmpty);

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

			Assert(result.AH_GB_TaxBranch.IsEmpty);
		}

		[TestDate(2012, 12, 24, 15, 55, 55)]
		public void TestConvert()
		{
			var invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = TestObjectCreator.Creditor1.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(200, invoicePendingAllocation.PK, 1);
			invoicePendingAllocation.AH_OSTaxAmount = 10m;
			invoicePendingAllocation.AH_Desc = "Test Description";
			invoicePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-1);

			var creditNotePendingAllocation = Factory.New<TransactionPendingAllocation>();
			creditNotePendingAllocation.AH_TransactionNum = "credit note";
			creditNotePendingAllocation.AH_OH = TestObjectCreator.Creditor1.PK;
			creditNotePendingAllocation.AH_OSExTaxAmount = -100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(300, creditNotePendingAllocation.PK, 2);
			creditNotePendingAllocation.AH_OSTaxAmount = -10m;
			creditNotePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();
			AssertEquals("Pre-condition", false, invoicePendingAllocation.AllowBackPosting);
			AssertEquals("Pre-condition", false, creditNotePendingAllocation.AllowBackPosting);

			var result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert(result is APInvoice);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(110m, result.ExpectedInvoiceTotal);
			AssertEquals(100m, result.ExpectedInvoiceExclTaxTotal);
			AssertEquals(10m, result.ExpectedInvoiceTaxTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("Test Description", result.AH_Desc);
			AssertEquals(ZDateTime.Now, result.AH_PostDate);

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(creditNotePendingAllocation).Invoice;
			Assert(result is APCreditNote);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.CreditNote, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(110m, result.ExpectedInvoiceTotal);
			AssertEquals(100m, result.ExpectedInvoiceExclTaxTotal);
			AssertEquals(10m, result.ExpectedInvoiceTaxTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("UNAPPROVED INVOICE", result.AH_Desc);
			AssertEquals(ZDateTime.Now, result.AH_PostDate);
		}

		public void TestGenericChargeValidationWhenCreatingNewAPInvoice()
		{
			var newDept = Factory.NewWithValidTestData<GlbDepartment>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header.JH_GE = newDept.PK;
			header.JH_GB = newBranch.PK;
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.SubmittedFromInvoicingForm = true; // AP Invoice Controller does this
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "Tran # 1";

			AssertNotEquals("Precondition: Job Dept & Txn Dept", header.Department.GE_Code, invoice.Department.GE_Code);
			AssertNotEquals("Precondition: Job Branch & Txn Branch", header.Branch.GB_Code, invoice.Branch.GB_Code);

			var line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.GenericCharge = TestObjectCreator.CC1.PK;
			line1.AL_JH = header.PK;
			AssertNoErrors(line1.GenericChargeInfo);
			AssertNoErrors(line1.AL_JHInfo);
			AssertEquals("Line1.Branch has been defaulted", header.Branch.GB_Code, line1.Branch.GB_Code);
			AssertEquals("Line1.Dept has been defaulted", header.Department.GE_Code, line1.Department.GE_Code);

			var line2 = (APInvoiceLine)invoice.Lines.AddNew();
			AssertEquals("Line2.Job has been defaulted from Line1.Job", line1.AL_JH, header.PK);
			AssertEquals("Line2.Branch has been defaulted", header.Branch.GB_Code, line2.Branch.GB_Code);
			AssertEquals("Line2.Dept has been defaulted", header.Department.GE_Code, line2.Department.GE_Code);
			AssertHasErrors(line2.GenericChargeInfo);
			line2.GenericCharge = TestObjectCreator.CC1.PK;
			AssertNoErrors(line2.GenericChargeInfo);
		}

		public void TestGenericChargeValidationWhenAllocatingTransaction()
		{
			GlbDepartment newDept = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			AccChargeCode cC1 = TestObjectCreator.CC1;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header.JH_GE = newDept.PK;
			header.JH_GB = newBranch.PK;
			header.JH_ParentID = shipment.PK;
			Factory.Save();

			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_TransactionNum = "Tran # 1";
			transaction.ExchangeRate.Currency = "USD";
			transaction.ExchangeRate.Rate = 1.6352;
			transaction.AH_OSExTaxAmount = 50m;
			transaction.AH_OSTaxAmount = 30m;
			transaction.AH_OutstandingAmount = -48.93;
			transaction.Factory.Save();

			TransactionPendingAllocation reloadedTransaction = new BusinessObjectFactory().Load<TransactionPendingAllocation>(transaction.PK);
			APInvoice transformedTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(reloadedTransaction).Invoice;
			transformedTransaction.SubmittedFromInvoicingForm = true; // AP Invoice Controller does this

			AssertNotEquals("Precondition: Job Dept & Txn Dept", header.Department.GE_Code, transformedTransaction.Department.GE_Code);
			AssertNotEquals("Precondition: Job Branch & Txn Branch", header.Branch.GB_Code, transformedTransaction.Branch.GB_Code);

			APInvoiceLine line1 = (APInvoiceLine)transformedTransaction.Lines.AddNew();
			line1.GenericCharge = cC1.PK;
			line1.AL_JH = header.PK;
			AssertNoErrors(line1.GenericChargeInfo);
			AssertNoErrors(line1.AL_JHInfo);
			AssertEquals("Line1.Branch has been defaulted", header.Branch.GB_Code, line1.Branch.GB_Code);
			AssertEquals("Line1.Dept has been defaulted", header.Department.GE_Code, line1.Department.GE_Code);

			APInvoiceLine line2 = (APInvoiceLine)transformedTransaction.Lines.AddNew();
			AssertEquals("Line2.Job has been defaulted from Line1.Job", header.PK, line2.AL_JH);
			AssertEquals("Line2.Branch has been defaulted", header.Branch.GB_Code, line2.Branch.GB_Code);
			AssertEquals("Line2.Dept has been defaulted", header.Department.GE_Code, line2.Department.GE_Code);
			AssertHasErrors(line2.GenericChargeInfo);
			line2.GenericCharge = cC1.PK;
			AssertNoErrors(line2.GenericChargeInfo);
		}

		public void TestDefaultUseJobExchangeRateSetWhenAllocatingTransaction_LocalCurrencyInvoice()
			=> AssertDefaultUseJobExchangeRateSetWhenAllocatingTransaction(false);

		public void TestDefaultUseJobExchangeRateSetWhenAllocatingTransaction_ForeignCurrencyInvoice()
			=> AssertDefaultUseJobExchangeRateSetWhenAllocatingTransaction(true);

		void AssertDefaultUseJobExchangeRateSetWhenAllocatingTransaction(bool isForeignInvoice)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			TransactionPendingAllocation invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(200, invoicePendingAllocation.PK, 1);
			invoicePendingAllocation.AH_Desc = "Test Description";

			TransactionPendingAllocation creditNotePendingAllocation = Factory.New<TransactionPendingAllocation>();
			creditNotePendingAllocation.AH_TransactionNum = "invoice";
			creditNotePendingAllocation.AH_OH = org.PK;
			creditNotePendingAllocation.AH_OSExTaxAmount = -100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(300, creditNotePendingAllocation.PK, 1);

			if (isForeignInvoice)
			{
				invoicePendingAllocation.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				creditNotePendingAllocation.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			}

			Factory.Save();

			AssertEquals("Pre-condition: UseJobExchangeRateDefault", false, AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.Value);

			InvoicingBase result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert(result is APInvoice);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(100m, result.ExpectedInvoiceTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("Test Description", result.AH_Desc);
			AssertEquals("UseJobExchangeRate", false, result.AH_PostedToEFT);

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(creditNotePendingAllocation).Invoice;
			Assert(result is APCreditNote);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.CreditNote, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(100m, result.ExpectedInvoiceTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("UNAPPROVED INVOICE", result.AH_Desc);
			AssertEquals("UseJobExchangeRate", false, result.AH_PostedToEFT);

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert(result is APInvoice);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.Invoice, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(100m, result.ExpectedInvoiceTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("Test Description", result.AH_Desc);
			AssertEquals("UseJobExchangeRate", isForeignInvoice, result.AH_PostedToEFT);

			result = TransactionAllocationConverter.ConvertUnallocatedToAP(creditNotePendingAllocation).Invoice;
			Assert(result is APCreditNote);
			Assert(result.Factory != invoicePendingAllocation.Factory);
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals("TransactionType", TransactionTypes.CreditNote, result.AH_TransactionType);
			Assert(result.SubmittedFromInvoicingForm);
			AssertEquals(100m, result.ExpectedInvoiceTotal);
			Assert(result.ValidateExpectedInvoiceTotal);
			AssertNull(result.ExportedBatchSequence);
			AssertEquals("UNAPPROVED INVOICE", result.AH_Desc);
			AssertEquals("UseJobExchangeRate", isForeignInvoice, result.AH_PostedToEFT);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			var revenueRecognition = TestObjectCreator.CC1.RevenueRecOverrides.AddNew();
			revenueRecognition.JobType = "ALL";
			revenueRecognition.DirectionCode = "ALL";
			revenueRecognition.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			Factory.Save();
		}

		string UniversalTransactionXML
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>BER</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <GLAccount>
          <AccountCode>1010.10.10</AccountCode>
          <Description>FREIGHT REVENUE ACTUAL</Description>
        </GLAccount>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-60.0000</LocalAmount>
        <OSAmount>-120.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithSingleLineFromShipmentXml
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>-100</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-100</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithSingleLineFromShipmentXml_NotionalShipmentNumber
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Ledger>AP</Ledger>
    <OSExGSTVATAmount>-100.0000</OSExGSTVATAmount>
    <Number>INV4</Number>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Job>
          <Key>Job123</Key>
          <Type>Job</Type>
        </Job>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
		<OSAmount>-100</OSAmount>
        <OSTotalAmount>-100.00</OSTotalAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>Job123</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <WayBillNumber>HBILLIMPORTAP4</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
        </WayBillType>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithSingleLineFromConsolXml
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <BranchAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </BranchAddress>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
	<JobInvoiceNumber>C00000010</JobInvoiceNumber>
    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <OSAmount>100</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>100</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
				<Key>C00000010</Key>
			</DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
			<DataTarget>
				<Type>ForwardingConsol</Type>
				<Key>C00000010</Key>
			</DataTarget>
			<DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithSingleLineFromConsolXml2
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <JobInvoiceNumber>C00000010</JobInvoiceNumber>
    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <GLAccount>
          <AccountCode>1010.10.10</AccountCode>
          <Description>FREIGHT REVENUE ACTUAL</Description>
        </GLAccount>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <OSAmount>100</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
     <DataSource>
        <Type>ForwardingConsol</Type>
        <Key>C00000010</Key>
     </DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
     <DataTarget>
        <Type>ForwardingConsol</Type>
        <Key>C00000010</Key>
     </DataTarget>
     <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithMultipleLineXml
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <BranchAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </BranchAddress>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>Some line text</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>100</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>100</OSTotalAmount>
        <Sequence>3</Sequence>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC3</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>Some line text</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>200</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>200</OSTotalAmount>
        <Sequence>4</Sequence>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>

    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
		  <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataSource>
          </DataSourceCollection>
		  <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>
";
			}
		}

		string UniversalTransactionWithMultipleLineConsolXml2
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Ledger>AP</Ledger>
    <OSExGSTVATAmount>-5000.0000</OSExGSTVATAmount>
    <Number>INV 4</Number>
	<Job>
      <Type>Job</Type>
    </Job>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001000</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-2500.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	  <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001001</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-2500.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>	  
    </PostingJournalCollection>
    <ShipmentCollection>        
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S0001000</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
		<WayBillNumber>081-11223343</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
		<SubShipmentCollection>
			<SubShipment>
				<DataContext>
				  <DataSourceCollection>
					<DataSource>
					  <Type>ForwardingShipment</Type>
					  <Key>S0001000</Key>
					</DataSource>
				  </DataSourceCollection>
				</DataContext>
				<TransportMode>
				  <Code>AIR</Code>
				  <Description>Air Freight</Description>
				</TransportMode>
				<WayBillNumber>HBILLIMPORTAP5</WayBillNumber>
				<WayBillType>
				  <Code>HWB</Code>
				</WayBillType>
			</SubShipment>
		</SubShipmentCollection>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S0001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <WayBillNumber>081-11223343</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
		<SubShipmentCollection>
			<SubShipment>
				<DataContext>
				  <DataSourceCollection>
					<DataSource>
					  <Type>ForwardingShipment</Type>
					  <Key>S0001001</Key>
					</DataSource>
				  </DataSourceCollection>
				</DataContext>
				<TransportMode>
				  <Code>AIR</Code>
				  <Description>Air Freight</Description>
				</TransportMode>
				<WayBillNumber>HBILLIMPORTAP6</WayBillNumber>
				<WayBillType>
				  <Code>HWB</Code>
				</WayBillType>
			</SubShipment>
		</SubShipmentCollection>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithMultipleLineConsolXml3
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Ledger>AP</Ledger>
    <OSExGSTVATAmount>-5000.0000</OSExGSTVATAmount>
    <Number>INV 4</Number>
	<Job>
      <Type>Job</Type>
    </Job>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001000</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-10.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	<PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001000</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>USD</Code>
		</OSCurrency>
        <OSAmount>-20.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	  <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001001</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-30.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	  <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
		<Job>
          <Key>S0001001</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>USD</Code>
		</OSCurrency>
        <OSAmount>-40.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>        
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S0001000</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
		<WayBillNumber>081-11223343</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
		<SubShipmentCollection>
			<SubShipment>
				<DataContext>
				  <DataSourceCollection>
					<DataSource>
					  <Type>ForwardingShipment</Type>
					  <Key>S0001000</Key>
					</DataSource>
				  </DataSourceCollection>
				</DataContext>
				<TransportMode>
				  <Code>AIR</Code>
				  <Description>Air Freight</Description>
				</TransportMode>
				<WayBillNumber>HBILLIMPORTAP5</WayBillNumber>
				<WayBillType>
				  <Code>HWB</Code>
				</WayBillType>
			</SubShipment>
		</SubShipmentCollection>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S0001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <WayBillNumber>081-11223343</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
		<SubShipmentCollection>
			<SubShipment>
				<DataContext>
				  <DataSourceCollection>
					<DataSource>
					  <Type>ForwardingShipment</Type>
					  <Key>S0001001</Key>
					</DataSource>
				  </DataSourceCollection>
				</DataContext>
				<TransportMode>
				  <Code>AIR</Code>
				  <Description>Air Freight</Description>
				</TransportMode>
				<WayBillNumber>HBILLIMPORTAP6</WayBillNumber>
				<WayBillType>
				  <Code>HWB</Code>
				</WayBillType>
			</SubShipment>
		</SubShipmentCollection>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithMultipleLineXml2
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Ledger>AP</Ledger>
    <OSExGSTVATAmount>-60</OSExGSTVATAmount>
    <Number>INV 4</Number>
	<Job>
      <Type>Job</Type>
    </Job>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
		<Job>
          <Key>Job123</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-30.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	  <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
		<Job>
          <Key>Job456</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-20.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
	  <PostingJournal>
        <ChargeCode>
          <Code>ZZREVFRE</Code>
        </ChargeCode>
		<Job>
          <Key>Job123</Key>
          <Type>Job</Type>
        </Job>
		<IsFinalCharge>true</IsFinalCharge>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
        <OSAmount>-10.00</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
        </VATTaxID>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>        
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>Job123</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <WayBillNumber>HBILLIMPORTAP4</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
        </WayBillType>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>Job456</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <WayBillNumber>HBILLIMPORTAP5</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
        </WayBillType>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithMultipleLineXml3
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>Some line text</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001001</Key>
        </Job>
        <OSAmount>100</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>100</OSTotalAmount>
        <Sequence>3</Sequence>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>Some line text</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
          <Key>S001002</Key>
        </Job>
        <OSAmount>200</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>200</OSTotalAmount>
        <Sequence>4</Sequence>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>

    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
		  <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataSource>
          </DataSourceCollection>
		  <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>
";
			}
		}

		string UniversalTransactionWithSingleForeignCurrencyLineFromConsolXml
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<DataContext>
		<Company>
			<Code>DAU</Code>
      </Company>
	</DataContext>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABIGAS</OrganizationCode>
    </OrganizationAddress>
	<JobInvoiceNumber>C00000010</JobInvoiceNumber>
    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <OSAmount>-100</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-100</OSTotalAmount>
        <Sequence>2</Sequence>
        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
				<Key>C00000010</Key>
			</DataSource>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
			<DataTarget>
				<Type>ForwardingConsol</Type>
				<Key>C00000010</Key>
			</DataTarget>
			<DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithCustomizablePostingJournalCollectionXml
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <TransactionInfo>
        <Branch>
            <Code>SYD</Code>
        </Branch>
        <Department>
            <Code>BRN</Code>
        </Department>
        <Description>AP INVOICE</Description>
        <DueDate>2024-05-01T19:09:00</DueDate>
        <Ledger>AP</Ledger>
        <LocalExVATAmount>-300.0000</LocalExVATAmount>
        <Number>APIMPORTA01</Number>
        <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
        <OrganizationAddress>
            <AddressType>None</AddressType>
            <OrganizationCode>ZCreditor1</OrganizationCode>
        </OrganizationAddress>
        <OSCurrency>
            <Code>AUD</Code>
            <Description>United States Dollar</Description>
        </OSCurrency>
        <OSExGSTVATAmount>-300.0000</OSExGSTVATAmount>
        <PostDate>2024-04-30T19:09:00</PostDate>
        <TransactionDate>2024-04-28T19:09:00</TransactionDate>
        <TransactionType>INV</TransactionType>
        <IsCancelled>false</IsCancelled>

        <PostingJournalCollection>
			{0}
        </PostingJournalCollection>
        
        <ShipmentCollection>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
        </ShipmentCollection>
    </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionWithCustomTaxRatedValuesInPostingJournalCollectionXml
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <TransactionInfo>
        <Branch>
            <Code>SYD</Code>
        </Branch>
    <BranchAddress>
      <AddressType>OFC</AddressType>
      <Country>
        <Code>AU</Code>
      </Country>
    </BranchAddress>
        <Department>
            <Code>BRN</Code>
        </Department>
        <Description>AP INVOICE</Description>
        <DueDate>2024-05-01T19:09:00</DueDate>
        <Ledger>AP</Ledger>
        <LocalExVATAmount>-200.0000</LocalExVATAmount>
        <Number>APIMPORTA01</Number>
        <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
        <OrganizationAddress>
            <AddressType>None</AddressType>
            <OrganizationCode>ZCreditor1</OrganizationCode>
        </OrganizationAddress>
        <OSCurrency>
            <Code>AUD</Code>
            <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSExGSTVATAmount>-200.0000</OSExGSTVATAmount>
        <OSGSTVATAmount>-10</OSGSTVATAmount>
        <OSTotal>-210</OSTotal>
        <PostDate>2024-04-30T19:09:00</PostDate>
        <TransactionDate>2024-04-28T19:09:00</TransactionDate>
        <TransactionType>INV</TransactionType>
        <IsCancelled>false</IsCancelled>
        <PostingJournalCollection>
            <PostingJournal>
                <Branch>
                    <Code>SYD</Code>
                </Branch>
                <ChargeCode>
                    <Code>ZZCC2</Code>
                </ChargeCode>
                <ChargeCurrency>
                    <Code>AUD</Code>
                </ChargeCurrency>
                <CostSource>
                    <Type>ForwardingConsol</Type>
                    <Key>C001</Key>
                </CostSource>
                <Department>
                    <Code>BRN</Code>
                </Department>
                <Description>FREIGHT REVENUE ACTUAL</Description>
                <IsFinalCharge>false</IsFinalCharge>
                <Job>
                    <Type>Job</Type>
                    <Key>S001</Key>
                </Job>
                <OSAmount>-100.0000</OSAmount>
                <OSGSTVATAmount>-5.0000</OSGSTVATAmount>
                <VATTaxID>
                    <TaxCode>ZZGST1</TaxCode>
                </VATTaxID>
            </PostingJournal>
            <PostingJournal>
                <Branch>
                    <Code>SYD</Code>
                </Branch>
                <ChargeCode>
                    <Code>ZZCC2</Code>
                </ChargeCode>
                <ChargeCurrency>
                    <Code>AUD</Code>
                </ChargeCurrency>
                <CostSource>
                    <Type>ForwardingConsol</Type>
                    <Key>C001</Key>
                </CostSource>
                <Department>
                    <Code>BRN</Code>
                </Department>
                <Description>FREIGHT REVENUE ACTUAL</Description>
                <IsFinalCharge>false</IsFinalCharge>
                <Job>
                    <Type>Job</Type>
                    <Key>S002</Key>
                </Job>
                <OSAmount>-100.0000</OSAmount>
                <OSGSTVATAmount>-5.0000</OSGSTVATAmount>
                <VATTaxID>
                    <TaxCode>ZZGST1</TaxCode>
                </VATTaxID>
            </PostingJournal>
        </PostingJournalCollection>
        <ShipmentCollection>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
        </ShipmentCollection>
    </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string GetPostingJournalWithSingleForeignLineToConsolXml(bool hasLocalValue)
		{
			StringBuilder xmlBuilder = new StringBuilder();
			xmlBuilder.Append(@"
<PostingJournal>
    <Branch>
        <Code>SYD</Code>
    </Branch>
    <ChargeCode>
		<Code>ZZCC1</Code>
    </ChargeCode>
    <CostSource>
		<Type>ForwardingConsol</Type>
        <Key>C001</Key>
    </CostSource>
    <Department>
        <Code>BRN</Code>
    </Department>
    <OSCurrency>
		<Code>USD</Code>
	</OSCurrency>
    <OSAmount>-235</OSAmount>
    <OSGSTVATAmount>-0</OSGSTVATAmount>
    <OSTotalAmount>-235</OSTotalAmount>
    <Sequence>3</Sequence>
    <IsFinalCharge>true</IsFinalCharge>
    <VATTaxID>
		<TaxCode>FREEGST</TaxCode>
    </VATTaxID>");
			if (hasLocalValue)
			{
				xmlBuilder.Append(@"
    <LocalCurrency>
		<Code>AUD</Code>
    </LocalCurrency>
    <LocalTotalAmount>-357</LocalTotalAmount>
    <LocalGSTVATAmount>0.00</LocalGSTVATAmount>
    <LocalAmount>-357</LocalAmount>");
			}
			xmlBuilder.Append(@"
</PostingJournal>");

			return xmlBuilder.ToString();
		}

		string GetPostingJournalsWithTwoForeignLinesXml(bool hasConsol, bool hasLocalValue)
		{
			StringBuilder xmlBuilder = new StringBuilder();
			xmlBuilder.Append(@"
<PostingJournal>
    <Branch>
        <Code>SYD</Code>
    </Branch>
    <ChargeCode>
		<Code>ZZCC1</Code>
    </ChargeCode>");
			if (hasConsol)
			{
				xmlBuilder.Append(@"
	<CostSource>
		<Type>ForwardingConsol</Type>
        <Key>C001</Key>
    </CostSource>");
			}
			xmlBuilder.Append(@"
	<Job>
        <Key>S001</Key>
        <Type>Job</Type>
    </Job>
    <Department>
        <Code>BRN</Code>
    </Department>
    <OSCurrency>
		<Code>USD</Code>
	</OSCurrency>
    <OSAmount>-70.5</OSAmount>
    <OSGSTVATAmount>-0</OSGSTVATAmount>
    <OSTotalAmount>-70.5</OSTotalAmount>
    <Sequence>3</Sequence>
    <IsFinalCharge>true</IsFinalCharge>
    <VATTaxID>
		<TaxCode>FREEGST</TaxCode>
    </VATTaxID>");
			if (hasLocalValue)
			{
				xmlBuilder.Append(@"
    <LocalCurrency>
		<Code>AUD</Code>
    </LocalCurrency>
    <LocalTotalAmount>-107.1</LocalTotalAmount>
    <LocalGSTVATAmount>0.00</LocalGSTVATAmount>
    <LocalAmount>-107.1</LocalAmount>");
			}
			xmlBuilder.Append(@"
</PostingJournal>");

			xmlBuilder.Append(@"
<PostingJournal>
    <Branch>
        <Code>SYD</Code>
    </Branch>
    <ChargeCode>
		<Code>ZZCC1</Code>
    </ChargeCode>");
			if (hasConsol)
			{
				xmlBuilder.Append(@"
	<CostSource>
		<Type>ForwardingConsol</Type>
        <Key>C001</Key>
    </CostSource>");
			}
			xmlBuilder.Append(@"
	<Job>
        <Key>S002</Key>
        <Type>Job</Type>
    </Job>
    <Department>
        <Code>BRN</Code>
    </Department>
    <OSCurrency>
		<Code>USD</Code>
	</OSCurrency>
    <OSAmount>-164.5</OSAmount>
    <OSGSTVATAmount>-0</OSGSTVATAmount>
    <OSTotalAmount>-164.5</OSTotalAmount>
    <Sequence>3</Sequence>
    <IsFinalCharge>true</IsFinalCharge>
    <VATTaxID>
		<TaxCode>FREEGST</TaxCode>
    </VATTaxID>");
			if (hasLocalValue)
			{
				xmlBuilder.Append(@"
    <LocalCurrency>
		<Code>AUD</Code>
    </LocalCurrency>
    <LocalTotalAmount>-249.9</LocalTotalAmount>
    <LocalGSTVATAmount>0.00</LocalGSTVATAmount>
    <LocalAmount>-249.9</LocalAmount>");
			}
			xmlBuilder.Append(@"
</PostingJournal>");

			return xmlBuilder.ToString();
		}

		const string UniversalTransactionWithMatchingCriteriaXml = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>SYD</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-30.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ZCreditor1</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-30.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>INV</TransactionType>
    <IsCancelled>false</IsCancelled>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <OSAmount>-30</OSAmount>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-30</OSTotalAmount>
        <Sequence>1</Sequence>
        <ImportMetaData>
          <Instruction>UpdateAndInsertIfNotFound</Instruction>
          <MatchingCriteriaCollection>
            <MatchingCriteria>
              <FieldName>PrimaryKey</FieldName>
              <Value>{PrimaryKeyOfConsolCost}</Value>
            </MatchingCriteria>
          </MatchingCriteriaCollection>
        </ImportMetaData>
      </PostingJournal>
    </PostingJournalCollection>

	<ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>

  </TransactionInfo>
</UniversalTransaction>";

		const string UniversalTransactionWithMultipleMatchingCriteriaXml = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>SYD</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <DueDate>2015-05-01T19:09:00</DueDate>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-30.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ZCreditor1</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-30.0000</OSExGSTVATAmount>
    <PostDate>2015-04-30T19:09:00</PostDate>
    <TransactionDate>2015-04-28T19:09:00</TransactionDate>
    <TransactionType>INV</TransactionType>
    <IsCancelled>false</IsCancelled>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <OSAmount>-10</OSAmount>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-10</OSTotalAmount>
        <Sequence>1</Sequence>
        <ImportMetaData>
          <Instruction>UpdateAndInsertIfNotFound</Instruction>
          <MatchingCriteriaCollection>
            <MatchingCriteria>
              <FieldName>PrimaryKey</FieldName>
              <Value>{PrimaryKeyOfConsolCostForLine1}</Value>
            </MatchingCriteria>
          </MatchingCriteriaCollection>
        </ImportMetaData>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>ZZCC1</Code>
        </ChargeCode>
        <ChargeCurrency>
          <Code>AUD</Code>
        </ChargeCurrency>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>C001001</Key>
        </CostSource>
        <Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <OSAmount>-20</OSAmount>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>-20</OSTotalAmount>
        <Sequence>1</Sequence>
        <ImportMetaData>
          <Instruction>UpdateAndInsertIfNotFound</Instruction>
          <MatchingCriteriaCollection>
            <MatchingCriteria>
              <FieldName>PrimaryKey</FieldName>
              <Value>{PrimaryKeyOfConsolCostForLine2}</Value>
            </MatchingCriteria>
          </MatchingCriteriaCollection>
        </ImportMetaData>
      </PostingJournal>
    </PostingJournalCollection>

	<ShipmentCollection>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>S001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataTarget>
          </DataTargetCollection>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>C001001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>

  </TransactionInfo>
</UniversalTransaction>";

		const string UniversalTransactionWithTwoCustomizableMatchingCriteriaCollectionXml = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <TransactionInfo>
        <Branch>
            <Code>SYD</Code>
        </Branch>
        <Department>
            <Code>BRN</Code>
        </Department>
        <Description>AP INVOICE</Description>
        <DueDate>2024-05-01T19:09:00</DueDate>
        <Ledger>AP</Ledger>
        <LocalExVATAmount>-300.0000</LocalExVATAmount>
        <Number>APIMPORTA01</Number>
        <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
        <OrganizationAddress>
            <AddressType>None</AddressType>
            <OrganizationCode>ZCreditor1</OrganizationCode>
        </OrganizationAddress>
        <OSCurrency>
            <Code>AUD</Code>
            <Description>United States Dollar</Description>
        </OSCurrency>
        <OSExGSTVATAmount>-300.0000</OSExGSTVATAmount>
        <PostDate>2024-04-30T19:09:00</PostDate>
        <TransactionDate>2024-04-28T19:09:00</TransactionDate>
        <TransactionType>INV</TransactionType>
        <IsCancelled>false</IsCancelled>

        <PostingJournalCollection>
			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<ChargeCode>
					<Code>ZZCC1</Code>
				</ChargeCode>
				<ChargeCurrency>
					<Code>AUD</Code>
				</ChargeCurrency>
				<CostSource>
					<Type>ForwardingConsol</Type>
					<Key>C001</Key>
				</CostSource>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>FREIGHT REVENUE ACTUAL</Description>
				<IsFinalCharge>false</IsFinalCharge>
				<Job>
					<Type>Job</Type>
					<Key>S001</Key>
				</Job>
				<OSAmount>-120.0000</OSAmount>
				<OSGSTVATAmount>0.0000</OSGSTVATAmount>
				<OSTotalAmount>-120.0000</OSTotalAmount>
				<Sequence>1</Sequence>
				<ImportMetaData>
					<Instruction>UpdateAndInsertIfNotFound</Instruction>
					<MatchingCriteriaCollection>
						{0}
					</MatchingCriteriaCollection>
				</ImportMetaData>
			</PostingJournal>

			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<ChargeCode>
					<Code>ZZCC1</Code>
				</ChargeCode>
				<ChargeCurrency>
					<Code>AUD</Code>
				</ChargeCurrency>
				<CostSource>
					<Type>ForwardingConsol</Type>
					<Key>C001</Key>
				</CostSource>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>FREIGHT REVENUE ACTUAL</Description>
				<IsFinalCharge>false</IsFinalCharge>
				<Job>
					<Type>Job</Type>
					<Key>S002</Key>
				</Job>
				<OSAmount>-180.0000</OSAmount>
				<OSGSTVATAmount>0.0000</OSGSTVATAmount>
				<OSTotalAmount>-180.0000</OSTotalAmount>
				<Sequence>1</Sequence>
				<ImportMetaData>
					<Instruction>UpdateAndInsertIfNotFound</Instruction>
					<MatchingCriteriaCollection>
						{1}
					</MatchingCriteriaCollection>
				</ImportMetaData>
			</PostingJournal>
        </PostingJournalCollection>
        
        <ShipmentCollection>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S002</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
            <Shipment>
                <DataContext>
                    <DataSourceCollection>
                        <DataSource>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataSource>
                    </DataSourceCollection>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingConsol</Type>
                            <Key>C001</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
            </Shipment>
        </ShipmentCollection>
    </TransactionInfo>
</UniversalTransaction>";

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
