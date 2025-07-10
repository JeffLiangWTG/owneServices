using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(APMatchingBase))]
	public class APMatchingBaseTestCase : MatchingBaseTest
	{
		#region Implementation

		protected override MatchingBase GetTestMatchingBase()
		{
			return new APMatchingBase(Factory);
		}

		protected override MatchingBase GetTestMatchingBaseInNewFactory(BusinessObjectFactory factoryForNewMatchingObject)
		{
			return new APMatchingBase(factoryForNewMatchingObject);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new APMatchingBase(Factory);
		}

		protected override Type InvoiceType
		{
			get { return typeof(APInvoice); }
		}

		protected override ZString LedgerType
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override MatchingBase GetTestMatchingBaseWithReceiptPaymentDetail(ReceiptPaymentBase receiptPaymentBase)
		{
			return new APMatchingBase(Factory, receiptPaymentBase);
		}

		protected override bool CanCashAdvanceRequestBeMatched => true;

		protected override Journal GetJournalFromDB()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			var journal = Factory.LoadTop1<APJournal>(query);
			return journal;
		}

		protected override ZBool ShouldTestPayLine => true;

		#endregion

		#region Inherited Tests

		#region Cash Advance

		public override void TestLoadCashAdvanceRequests()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, "AP", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				var currenyFilter = TestMatchingBase.CashAdvanceFilter["Currency"] as ModuleNkFilter;
				currenyFilter.Property = Core.Constants.CurrencyCodes.Philippines;
				currenyFilter.IsActive = true;
				AssertNotNull(currenyFilter);

				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				currenyFilter.Property = Core.Constants.CurrencyCodes.Australia;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
			}
		}

		public override void TestCashAdvanceFilterExcludesMatchedCashAdvances()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, "AP", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });

				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
			}
		}

		public override void TestCashAdvanceFilter()
		{
			AssertType<APCashAdvanceFilterBusinessObjectForMatchingBase>(TestMatchingBase.CashAdvanceFilter);
		}

		public override void TestCanCashAdvanceRequestBeMatched()
		{
			foreach (var (enablePayablesCashAdvanceFunctionality, enableManualUpdateToPaidStatus) in new[] { (false, false), (false, true), (true, false), (true, true) })
			{
				using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enablePayablesCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableManualUpdateToPaidStatus))
				{
					var expectedValue = enablePayablesCashAdvanceFunctionality && !enableManualUpdateToPaidStatus;
					AssertEquals(nameof(TestMatchingBase.CanCashAdvanceRequestBeMatched), expectedValue, TestMatchingBase.CanCashAdvanceRequestBeMatched);
				}
			}
		}

		[TestDate(2023, 02, 24, 0, 0, 0)]
		public override void TestCashAdvanceMatchingJournalsAreCreated()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, "AP", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AP Journal", TestMatchingBase.BalancingAPJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertCashAdvanceMatchingJournal(journal, cah, TransactionCategory.Codes.CashAdvancePaid, TestObjectCreator.Creditor1.PK, Core.Constants.DebitCredit.Debit, TestDateAttribute.Date);
			}
		}

		[TestDate(2023, 02, 24, 0, 0, 0)]
		public override void TestMoveAllCashAdvanceFromUnmatchToMatch()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, "AP", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AP Journal", TestMatchingBase.BalancingAPJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertCashAdvanceMatchingJournal(journal, cah, TransactionCategory.Codes.CashAdvancePaid, TestObjectCreator.Creditor1.PK, Core.Constants.DebitCredit.Debit, TestDateAttribute.Date);
			}
		}

		public override void TestMoveCashAdvanceFromMatchToUnmatch()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, "AP", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);
				AssertEquals("Number of UnmmatchedCashAdvanceRequests", 0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AP Journal", TestMatchingBase.BalancingAPJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));

				TestMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(new List<Journal> { journal });
				Assert("Removed from Balancing AP Journal", !TestMatchingBase.BalancingAPJournals.Contains(journal));
				Assert("Removed from Matched Transactions", !TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertEquals("Number of MatchedCashAdvanceRequests", 0, TestMatchingBase.MatchedCashAdvanceRequests.Count);
				AssertEquals("Number of UnmmatchedCashAdvanceRequests", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
			}
		}

		[TestDate(2022, 02, 25)]
		public override void TestCashAdvanceIsUpdatedOnCreationOfMatchingJournal()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var (job, header, apPayment) = CreateJobAndCashAdvanceHeader(creator, 1050M);

				AssertEquals("Local Outstanding Ammount", 1050M, header.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, header.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, header.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, header.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, header.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, header.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", header.CAH_Status);

				//Matching Session
				var (matchedCAH, matchedPayment, matchingBase) = MatchAPPaymentWithMatchingJournal(creator, header.PK, apPayment.PK);

				AssertEquals("Local Outstanding Ammount", 0M, matchedCAH.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 0M, matchedCAH.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, matchedCAH.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, matchedCAH.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 1050M, matchedCAH.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 1050M, matchedCAH.CAH_OSPaidAmount);
				AssertEquals("Status", "PAI", matchedCAH.CAH_Status);
			}
		}

		[TestDate(2022, 02, 25)]
		public void TestMatchLink_APAmount_Zero_DeveloperException()
		{
			var creator = new TestObjectCreator(Factory);
			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);

			var invoiceInNewFactory = testObjectCreatorInNewFactory.CreateAPInvoice<APInvoice>("INV", testObjectCreatorInNewFactory.AUD, 1M, 300M, 0M, 0M, 300M, 0M, 0M);
			invoiceInNewFactory.AH_OH = testObjectCreatorInNewFactory.AALSHI.PK;
			invoiceInNewFactory.AH_OutstandingAmount = -300M;

			var invoice2InNewFactory = testObjectCreatorInNewFactory.CreateAPInvoice<APInvoice>("INV2", testObjectCreatorInNewFactory.AUD, 1M, 150M, 0M, 0M, 150M, 0M, 0M);
			invoice2InNewFactory.AH_OH = testObjectCreatorInNewFactory.AALSHI.PK;
			invoice2InNewFactory.AH_OutstandingAmount = -150M;

			var creditNote = TestObjectCreator.CreateAPCreditNote("CRD", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1M, "Desc");
			TestObjectCreator.CreateAPCreditNoteLine(creditNote, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 300M);
			TestObjectCreator.AttachJobToAPLine(creditNote.Lines[0]);
			creditNote.AH_OutstandingAmount = 300M;
			TestObjectCreator.AttachChargeToAPLine(creditNote.Lines[0]);

			newFactory.Save();

			var invoice = Factory.Load<APInvoice>(invoiceInNewFactory.PK);
			var invoice2 = Factory.Load<APInvoice>(invoice2InNewFactory.PK);
			AssertEquals("Precondition: ", -300M, invoice.AH_OutstandingAmount);
			AssertEquals("Precondition: ", -300M, invoice.OutstandingAmountMatching);

			var matchingBase = new APMatchingBase(Factory);
			matchingBase.MatchDate = ZDateTime.Today;
			matchingBase.PrimaryOrganization = creator.AALSHI.PK;
			matchingBase.UnmatchedTransactions.Add(invoice);
			matchingBase.UnmatchedTransactions.Add(invoice2);
			matchingBase.UnmatchedTransactions.Add(creditNote);
			matchingBase.MoveAllFromUnmatchToMatch();

			((IMatching)invoice).OSPartialPaymentAmount = -300M;
			((IMatching)invoice2).OSPartialPaymentAmount = 0M;
			invoice2.AH_Desc = "Modify"; // To make sure MatchLinkLinkedTransactionHasNotChanged Critical validation doesn't get triggered for not doing any changes to the transaction
			((IMatching)creditNote).OSPartialPaymentAmount = 300M;

			matchingBase.Match_ForTestOnly();
			Factory.Save();

			AssertEquals("MatchLink transaction AP_Amount is set to 0", ErrorReporter.LastKeyReported);
			AssertContains("Transactions:", ErrorReporter.LastMessageReported);
			AssertContains("Type = INV, Ledger = AP, AP_Amount = -300.0000, Outstanding amount = 0, Bizo = Enterprise.Accounting.Business.ARAP.Invoicing.APInvoice", ErrorReporter.LastMessageReported);
			AssertContains("Type = INV, Ledger = AP, AP_Amount = 0, Outstanding amount = -150.0000, Bizo = Enterprise.Accounting.Business.ARAP.Invoicing.APInvoice", ErrorReporter.LastMessageReported);
			AssertContains("Type = CRD, Ledger = AP, AP_Amount = 300, Outstanding amount = 0, Bizo = Enterprise.Accounting.Business.ARAP.Invoicing.APCreditNote", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2022, 02, 25)]
		public void TestReversalOfPaymentReversesMatchingJournalandCashAdvanceStatus()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var (job, header, apPayment) = CreateJobAndCashAdvanceHeader(creator, 1050M);

				AssertEquals("Local Outstanding Ammount", 1050M, header.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, header.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, header.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, header.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, header.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, header.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", header.CAH_Status);

				//Matching Session
				var (matchedCAH, matchedPayment, matchingBase) = MatchAPPaymentWithMatchingJournal(creator, header.PK, apPayment.PK);

				AssertEquals("Local Outstanding Ammount", 0M, matchedCAH.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 0M, matchedCAH.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, matchedCAH.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, matchedCAH.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 1050M, matchedCAH.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 1050M, matchedCAH.CAH_OSPaidAmount);
				AssertEquals("Status", "PAI", matchedCAH.CAH_Status);

				var capJournal = matchedCAH.Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvancePaid));
				AssertNotNull(capJournal);

				var reversingFactory = new ReversingFactory();
				var reverser = reversingFactory.NewReversing(matchedPayment);
				AssertNotNull(reverser as PayablesAndReceivablesReversing);

				var reversePaymentPostDate = TestDateAttribute.Date.AddDays(-3);
				var receiptReverser = reverser as PayablesAndReceivablesReversing;
				receiptReverser.DoReverseTransaction_ForTestOnly();
				(matchedPayment.ReverseTransaction as TransactionHeader).AH_PostDate = reversePaymentPostDate;
				(matchedPayment.ReverseTransaction as TransactionHeader).ReversingReason = "Reversing for random reason";
				matchedPayment.Factory.Save();

				AssertEquals("Local Outstanding Ammount", 1050M, matchedCAH.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, matchedCAH.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, matchedCAH.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, matchedCAH.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, matchedCAH.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, matchedCAH.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", matchedCAH.CAH_Status);

				var reversedPayment = matchedPayment.Factory.LoadTop1<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, matchedPayment.PK));
				AssertEquals(true, matchedPayment.IsReversed);
				AssertNotNull("There should be a reversed payment", reversedPayment);
				AssertEquals(nameof(reversedPayment.AH_OSExTaxAmount), (-1) * matchedPayment.AH_OSExTaxAmount, reversedPayment.AH_OSExTaxAmount);
				AssertEquals(nameof(reversedPayment.AH_RX_NKTransactionCurrency), matchedPayment.AH_RX_NKTransactionCurrency, reversedPayment.AH_RX_NKTransactionCurrency);
				AssertEquals(nameof(reversedPayment.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {matchedPayment.AH_TransactionNum} Reversing for random reason"), reversedPayment.AH_Desc);

				var reversedJournal = reversedPayment.Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, capJournal.PK));
				AssertEquals(true, capJournal.IsReversed);
				AssertNotNull("There should be a reversed CAP journal", reversedJournal);
				AssertEquals(nameof(reversedJournal.AH_OSExTaxAmount), (-1) * capJournal.AH_InvoiceAmount, reversedJournal.AH_InvoiceAmount);
				AssertEquals(nameof(reversedJournal.AH_RX_NKTransactionCurrency), capJournal.AH_RX_NKTransactionCurrency, reversedJournal.AH_RX_NKTransactionCurrency);
				AssertEquals(nameof(reversedJournal.AH_Desc), FormattableString.Invariant($"Reversal Related to [{capJournal.AH_TransactionNum}]. Payment [{matchedPayment.AH_TransactionNum}] reversed. Reversing for random reason"), reversedJournal.AH_Desc);
				AssertEquals(nameof(reversedJournal.AH_PostDate), reversePaymentPostDate, reversedJournal.AH_PostDate);
			}
		}

		(Job job, AccCashAdvanceRequestHeader cah, APPayment payment) CreateJobAndCashAdvanceHeader(TestObjectCreator creator, ZDecimal amount)
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var exportConsol = Factory.New<ForwardingConsol>();
			var shipment = exportConsol.Shipments.AddNew();
			var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.ABIGAS.PK;

			var charge = shipmentJob.Charges.AddNew();
			SetupCharge(charge, true);
			var charge2 = shipmentJob.Charges.AddNew();
			SetupCharge(charge2, false);
			Factory.Save();

			var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsPayable, 0M, 0M, "AUD");
			var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, 1050M, 1050M);
			charge.JR_CAL_APLine = line.PK;

			//Creating an AP payment
			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_OH = creator.ABIGAS.PK;
			apPayment.AH_LocalExTaxAmount = 1050M;
			apPayment.AH_OSExTaxAmount = 1050M;
			Factory.Save();

			return (shipmentJob, header, apPayment);

			void SetupCharge(JobCharge localCharge, bool isCashAdvanceRequired)
			{
				localCharge.JR_AC = creator.CC1.PK;
				localCharge.JR_OH_CostAccount = creator.ABIGAS.PK;
				localCharge.JR_OSCostAmt = 1050m;
				localCharge.JR_IsAPCashAdvance = isCashAdvanceRequired;
				localCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			}
		}

		(CashAdvanceRequestHeader matchedCAH, APPayment matchedPayment, APMatchingBase matchingBase) MatchAPPaymentWithMatchingJournal(TestObjectCreator creator, ZGuid cahPK, ZGuid apPaymentPK)
		{
			//Matching Session
			var newFactory2 = new BusinessObjectFactory();
			newFactory2.RefreshEnabled = false;
			var cahInNewFactory2 = newFactory2.Load<CashAdvanceRequestHeader>(cahPK);
			var apPaymentInNewFactory2 = newFactory2.Load<APPayment>(apPaymentPK);
			var matchingBase = new APMatchingBase(newFactory2);
			matchingBase.MatchDate = ZDateTime.Today;
			matchingBase.PrimaryOrganization = creator.ABIGAS.PK;
			matchingBase.UnmatchedCashAdvanceRequests.Add(cahInNewFactory2);
			matchingBase.UnmatchedTransactions.Add(apPaymentInNewFactory2);
			matchingBase.MoveAllFromUnmatchToMatch();
			matchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
			matchingBase.Match_ForTestOnly();
			newFactory2.Save();
			return (cahInNewFactory2, apPaymentInNewFactory2, matchingBase);
		}

		#endregion

		public override void TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg()
		{
			base.TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg();

			// Check Dynamic Transactions
			AssertEquals("There should be 3 dynamic transactions", 3, TestMatchingBase.DynamicTransactions.Count);

			ContraFilter filter1 = new ContraFilter();
			filter1.AROrg = TestOrg1.PK;
			filter1.APOrg = TestOrg1.PK;
			filter1.APAmount = 50M;
			filter1.ARAmount = -50M;

			Contra dynamicContra1 = TestMatchingBase.DynamicTransactions.GetMatchingContra(filter1);
			AssertNotNull("There should be a contra with these values", dynamicContra1);

			ContraFilter filter2 = new ContraFilter();
			filter2.AROrg = TestOrg2.PK;
			filter2.APOrg = TestOrg1.PK;
			filter2.ARAmount = -30M;
			filter2.APAmount = 30M;

			Contra dynamicContra2 = TestMatchingBase.DynamicTransactions.GetMatchingContra(filter2);
			AssertNotNull("There should be a Conrta with these values", dynamicContra2);

			// Check Discount is set correctly

			AssertEquals("OSTotal should be -10", -10M, TestDSC.AH_OSTotal);
			AssertEquals("InvoiceAmount should be -10", -10M, TestDSC.AH_InvoiceAmount);
			AssertEquals("Outstanding amount should be 0", 0M, TestDSC.AH_OutstandingAmount);
			AssertEquals("ExchangeRate should be 1", 1M, TestDSC.AH_ExchangeRate);

			// Check the Matchlink rows
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("10 matchlink rows should be created", 10, matchLinks.Count);

			// DSC
			ZQuery dSC_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestDSC.PK);
			TransactionMatchLink dSC_Match = Factory.LoadTop1<TransactionMatchLink>(dSC_Filter);
			AssertNotNull("Should be a matchlink for DSC", dSC_Match);

			ZString matchGroup = dSC_Match.AP_MatchGroupNum;
			ZDateTime matchDate = dSC_Match.AP_MatchDate;
			AssertEquals("Matchlinks amount should be -10", -10M, dSC_Match.AP_Amount);

			// REC
			ZQuery rEC_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARReceipt.PK);
			TransactionMatchLink rEC_Match = Factory.LoadTop1<TransactionMatchLink>(rEC_Filter);
			AssertNotNull("There should be a matchlink for REC", rEC_Match);
			AssertEquals("Matchgroup should be the same", matchGroup, rEC_Match.AP_MatchGroupNum);
			AssertEquals("Matchdate should be the same", matchDate, rEC_Match.AP_MatchDate);
			AssertEquals("Amount should be -70", -70M, rEC_Match.AP_Amount);
		}

		#region TestMatchDateDefaultValue

		public override void TestMatchDateDefaultValue()
		{
			base.TestMatchDateDefaultValue();
			ReceiptPaymentBase receipt = Factory.New<APReceipt>();
			ZDateTime expectedDate = ZDateTime.Today;
			receipt.AH_PostDate = expectedDate;
			MatchingBase testMatchingBase = new APMatchingBase(Factory, receipt);
			AssertEquals(string.Format("APMatchingBase's default MatchDate should be {0}", expectedDate), expectedDate, testMatchingBase.MatchDate);
		}

		public override void TestConstructorSetIsLoadedFromGUI()
		{
			ReceiptPaymentBase receipt = Factory.New<APReceipt>();
			MatchingBase testMatchingBase1 = new APMatchingBase(Factory);
			AssertEquals("IsLoadedFromGUI should be true", true, testMatchingBase1.GetIsLoadedFromGUIForTest());
			MatchingBase testMatchingBase2 = new APMatchingBase(Factory, receipt, false);
			AssertEquals("IsLoadedFromGUI should be false", false, testMatchingBase2.GetIsLoadedFromGUIForTest());
			MatchingBase testMatchingBase3 = new APMatchingBase(Factory, receipt);
			AssertEquals("IsLoadedFromGUI should be true", true, testMatchingBase3.GetIsLoadedFromGUIForTest());
		}

		#endregion

		#endregion

		#region CheckpointForNewMiscTransaction

		public void TestCheckpointForNewMiscTransaction_ReturningSecurityCheckpoint()
		{
			AssertSame(Env.Security.PayablesNewMatchTransactionsOverpaymentType, GetTestMatchingBase().CheckpointForNewMiscTransaction(TransactionTypes.Overpayment));
		}

		#endregion
	}
}
