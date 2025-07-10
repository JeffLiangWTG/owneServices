using System;
using System.Collections.Generic;
using System.Linq;
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
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ARMatchingBase))]
	public class ARMatchingBaseTestCase : MatchingBaseTest
	{
		#region Implementation

		protected override MatchingBase GetTestMatchingBase()
		{
			return new ARMatchingBase(Factory);
		}

		protected override MatchingBase GetTestMatchingBaseInNewFactory(BusinessObjectFactory factoryForNewMatchingObject)
		{
			return new ARMatchingBase(factoryForNewMatchingObject);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARMatchingBase(Factory);
		}

		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}

		protected override ZString LedgerType
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override MatchingBase GetTestMatchingBaseWithReceiptPaymentDetail(ReceiptPaymentBase receiptPaymentBase)
		{
			return new ARMatchingBase(Factory, receiptPaymentBase);
		}

		protected override Journal GetJournalFromDB()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			var journal = Factory.LoadTop1<ARJournal>(query);
			return journal;
		}

		protected override ZBool ShouldTestPayLine => true;

		#endregion

		#region Inherited Tests

		public override void TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg()
		{
			base.TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg();

			// Check Dynamic Transactions
			AssertEquals("There should be 2 dynamic transactions", 2, TestMatchingBase.DynamicTransactions.Count);

			TransferFilter filter1 = new TransferFilter();
			filter1.TransferFromOrg = TestOrg3.PK;
			filter1.TransferToOrg = TestOrg1.PK;
			filter1.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			filter1.TransferFromAmount = 70M;
			filter1.TransferToAmount = -70M;

			ARTransfer dynamicTransfer1 = (ARTransfer)TestMatchingBase.DynamicTransactions.GetMatchingTransfer(filter1);
			AssertNotNull("There should be a transfer with these values", dynamicTransfer1);

			TransferFilter filter2 = new TransferFilter();
			filter2.TransferFromOrg = TestOrg2.PK;
			filter2.TransferToOrg = TestOrg1.PK;
			filter2.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			filter2.TransferFromAmount = -30M;
			filter2.TransferToAmount = 30M;

			ARTransfer dynamicTransfer2 = (ARTransfer)TestMatchingBase.DynamicTransactions.GetMatchingTransfer(filter2);
			AssertNotNull("There should be a transfer with these values", dynamicTransfer2);

			// Check Discount is set correctly

			AssertEquals("OSTotal should be -10", -10M, TestDSC.AH_OSTotal);
			AssertEquals("InvoiceAmount should be -10", -10M, TestDSC.AH_InvoiceAmount);
			AssertEquals("Outstanding amount should be 0", 0M, TestDSC.AH_OutstandingAmount);
			AssertEquals("ExchangeRate should be 1", 1M, TestDSC.AH_ExchangeRate);

			// Check the Matchlink rows
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("8 matchlink rows should be created", 8, matchLinks.Count);

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
			ReceiptPaymentBase receipt = Factory.New<ARReceipt>();
			ZDateTime expectedDate = ZDateTime.Today;
			receipt.AH_PostDate = expectedDate;
			MatchingBase testMatchingBase = new ARMatchingBase(Factory, receipt);
			AssertEquals(string.Format("ARMatchingBase's default MatchDate should be {0}", expectedDate), expectedDate, testMatchingBase.MatchDate);
		}

		public override void TestConstructorSetIsLoadedFromGUI()
		{
			ReceiptPaymentBase receipt = Factory.New<ARReceipt>();
			MatchingBase testMatchingBase1 = new ARMatchingBase(Factory);
			AssertEquals("IsLoadedFromGUI should be true", true, testMatchingBase1.GetIsLoadedFromGUIForTest());
			MatchingBase testMatchingBase2 = new ARMatchingBase(Factory, receipt, false);
			AssertEquals("IsLoadedFromGUI should be false", false, testMatchingBase2.GetIsLoadedFromGUIForTest());
			MatchingBase testMatchingBase3 = new ARMatchingBase(Factory, receipt);
			AssertEquals("IsLoadedFromGUI should be true", true, testMatchingBase3.GetIsLoadedFromGUIForTest());
		}

		#endregion

		public virtual void TestARMatching_InvoiceAndReceiptWithErrorOnWrongARInvoiceAmount()
		{
			TestOrg1 = GetNewTestOrg();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = TestOrg1.PK;
			aRInv.AH_LocalOutstandingAmount = 100M;
			TestObjectCreator.CreateInvoiceLine(aRInv, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = TestOrg1.PK;
			aRRec.AH_LocalExTaxAmount = 110M;
			aRRec.AH_OSExTaxAmount = 110M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();

			((IMatching)aRInv).OSPartialPaymentAmount = 110M;   // set an invalid OSPartialPaymentAmt

			Assert("The row for ARInvoice should have error", ((IMatching)aRInv).OSPartialPaymentAmountInfo.HasError(
				"Pay Amount must be between 0 and " + ((IMatching)aRInv).OutstandingAmount.ToString(2)));
			Assert("Should not be able to match these because the OSPartialPayment amount on APInvoice is positive",
				TestMatchingBase.HasErrors);
		}

		#endregion

		#region CheckpointForNewMiscTransaction

		public void TestCheckpointForNewMiscTransaction_ReturningSecurityCheckpoint()
		{
			AssertSame(Env.Security.ReceivablesNewMatchTransactionsOverpaymentType, GetTestMatchingBase().CheckpointForNewMiscTransaction(TransactionTypes.Overpayment));
		}

		#endregion

		[TestDate(2022, 02, 25)]
		public void TestNoExceptionIsThrownWhenMatchingARInvoiceContainingCashAdvanceRequests()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var creator = new TestObjectCreator(Factory);

				var exportConsol = Factory.New<ForwardingConsol>();
				var shipment = exportConsol.Shipments.AddNew();
				var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.LocalChargesPK = creator.ABIGAS.PK;

				var charge = shipmentJob.Charges.AddNew();
				SetupCharge(charge, true);
				var charge2 = shipmentJob.Charges.AddNew();
				SetupCharge(charge2, false);
				Factory.Save();

				var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsReceivable, 1050M, 1050M, "AUD");
				var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, 1050M, 1050M);
				charge.JR_CAL_ARLine = line.PK;
				header.MarkAsPaid();

				//Creating an AR receipt
				var aRRec = Factory.NewWithValidTestData<ARReceipt>();
				aRRec.AH_OH = creator.ABIGAS.PK;
				aRRec.AH_LocalExTaxAmount = 1050M;
				aRRec.AH_OSExTaxAmount = 1050M;
				Factory.Save();

				//Post Invoice
				var newFactory1 = new BusinessObjectFactory();
				newFactory1.RefreshEnabled = false;
				var shipmentJobInNewFactory1 = newFactory1.Load<Job>(shipmentJob.PK);
				var postManager = new InvoicingPostManager(shipmentJobInNewFactory1);
				postManager.CreateTransactions(JobInvoicingPostingOption.All);
				newFactory1.Save();

				var chargeInNewFactory = shipmentJobInNewFactory1.Charges.FindByPK(charge.PK) as Charge;
				AssertEquals("Precondition: AR should be posted", true, chargeInNewFactory.IsRevenuePosted);
				AssertEquals("Line status should be Invoiced", CashAdvanceStatusCodes.RequestLine.Invoiced, chargeInNewFactory.ARCashAdvanceRequestLine.CAL_Status);

				var invoice = chargeInNewFactory.ARLine.TransactionHeader;
				AssertEquals("Should not be cancelled", false, invoice.AH_IsCancelled);
				AssertEquals("Outstanding amount. One charge is paid via Advance Payment. Another remains unpaid.", 1050M, invoice.AH_OutstandingAmount);
				AssertEquals("Invoice amount.", 2100M, invoice.AH_InvoiceAmount);
				AssertEquals("Fully paid date", ZDateTime.Empty, invoice.AH_FullyPaidDate);

				//Matching Session
				var newFactory2 = new BusinessObjectFactory();
				newFactory2.RefreshEnabled = false;
				var invoiceInNewFactory2 = newFactory2.Load<ARInvoice>(invoice.PK);
				var arRecieptinNewFactory2 = newFactory2.Load<ARReceipt>(aRRec.PK);
				var matchingBase = new ARMatchingBase(newFactory2);
				matchingBase.PrimaryOrganization = creator.ABIGAS.PK;
				matchingBase.UnmatchedTransactions.Add(invoiceInNewFactory2);
				matchingBase.UnmatchedTransactions.Add(arRecieptinNewFactory2);
				matchingBase.MoveAllFromUnmatchToMatch();
				matchingBase.Match_ForTestOnly();
				newFactory2.Save();

				invoice = newFactory2.Load<ARInvoice>(invoice.PK);
				AssertEquals("Outstanding amount", 0M, invoice.AH_OutstandingAmount);
				AssertEquals("Fully paid date", TestDateAttribute.Date, invoice.AH_FullyPaidDate.ToDateTime());

				void SetupCharge(JobCharge localCharge, bool isCashAdvanceRequired)
				{
					localCharge.JR_AC = creator.CC1.PK;
					localCharge.JR_OH_SellAccount = creator.ABIGAS.PK;
					localCharge.JR_OSSellAmt = 1050m;
					localCharge.JR_IsARCashAdvance = isCashAdvanceRequired;
					localCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				}
			}
		}

		public override void TestLoadCashAdvanceRequests()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, "AR", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
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

		public override void TestCashAdvanceFilter()
		{
			AssertType<ARCashAdvanceFilterBusinessObjectForMatchingBase>(TestMatchingBase.CashAdvanceFilter);
		}

		public override void TestCashAdvanceFilterExcludesMatchedCashAdvances()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, "AR", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });

				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
			}
		}

		public override void TestCanCashAdvanceRequestBeMatched()
		{
			foreach (var (enableReceivablesCashAdvanceFunctionality, enableManualUpdateToPaidStatus) in new[] { (false, false), (false, true), (true, false), (true, true) })
			{
				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReceivablesCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableManualUpdateToPaidStatus))
				{
					var expectedValue = enableReceivablesCashAdvanceFunctionality && !enableManualUpdateToPaidStatus;
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
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, "AR", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AR Journal", TestMatchingBase.BalancingARJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertCashAdvanceMatchingJournal(journal, cah, TransactionCategory.Codes.CashAdvanceReceived, TestObjectCreator.Debtor.PK, Core.Constants.DebitCredit.Credit, TestDateAttribute.Date);
			}
		}

		[TestDate(2023, 02, 24, 0, 0, 0)]
		public override void TestMoveAllCashAdvanceFromUnmatchToMatch()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, "AR", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AR Journal", TestMatchingBase.BalancingARJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertCashAdvanceMatchingJournal(journal, cah, TransactionCategory.Codes.CashAdvanceReceived, TestObjectCreator.Debtor.PK, Core.Constants.DebitCredit.Credit, TestDateAttribute.Date);
			}
		}

		public override void TestMoveCashAdvanceFromMatchToUnmatch()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, "AR", 200M, 200M, "AUD");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
				TestMatchingBase.LoadCashAdvanceRequests();
				AssertEquals("Item Count", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				TestMatchingBase.MoveCashAdvanceFromUnmatchToMatch(new CashAdvanceRequestHeader[] { TestMatchingBase.UnmatchedCashAdvanceRequests[0] });
				AssertEquals("Number of MatchedCashAdvanceRequests", 1, TestMatchingBase.MatchedCashAdvanceRequests.Count);
				AssertEquals("Number of UnmmatchedCashAdvanceRequests", 0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

				var journal = TestMatchingBase.MatchedCashAdvanceRequests.Keys.First();
				AssertNotNull(journal);
				Assert("Addded to Balancing AR Journal", TestMatchingBase.BalancingARJournals.Contains(journal));
				Assert("Addded to Matched Transactions", TestMatchingBase.MatchedTransactions.Contains(journal));

				TestMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(new List<Journal> { journal });
				Assert("Removed from Balancing AR Journal", !TestMatchingBase.BalancingARJournals.Contains(journal));
				Assert("Removed from Matched Transactions", !TestMatchingBase.MatchedTransactions.Contains(journal));
				AssertEquals("Number of MatchedCashAdvanceRequests", 0, TestMatchingBase.MatchedCashAdvanceRequests.Count);
				AssertEquals("Number of UnmmatchedCashAdvanceRequests", 1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
			}
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestMatchingJournalCannotBeReversedIfCashAdvanceIsInvoiced()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Charge 01"
													, costCurrency: TestObjectCreator.AUD, osCostAmt: 100M, creditor: TestObjectCreator.Creditor1
													, sellCurrency: TestObjectCreator.AUD, osSellAmt: 100M, debtor: TestObjectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_IsARCashAdvance = true;
			Factory.Save();

			//Creating a cash advance
			var (error, cahs) = new ARCashAdvanceRequestor(job).GenerateRequests();
			var cah = cahs.First();

			//Receiving payment against the cash advance
			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(cah.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			//Invoicing the cash advance
			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			//trying to reverse the cash advance receipt
			var reversingFactory = new ReversingFactory();
			var reverser = reversingFactory.NewReversing(receipt1);
			AssertNotNull(reverser as PayablesAndReceivablesReversing);

			var expectedErrorMessage = FormattableString.Invariant($"Receipt cannot be reversed as it is linked to Advance Payment Request [{cah1.CAH_RequestReferenceNumber}] on Job #[{job.JH_JobNum}] which has invoiced charge lines. Please reverse the invoice before reversing the receipt");
			var receiptReverser = reverser as PayablesAndReceivablesReversing;
			AssertEquals("Should not be able to reverse", false, receiptReverser.CanReverseTransaction);
			AssertEquals("Error message for not being reversed", expectedErrorMessage, receiptReverser.CantReverseErrorMessage.ToString());
		}

		[TestDate(2022, 02, 25)]
		public override void TestCashAdvanceIsUpdatedOnCreationOfMatchingJournal()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var (job, cah, receipt) = CreateJobAndCashAdvanceHeader(creator, 1050M);

				AssertEquals("Local Outstanding Ammount", 1050M, cah.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, cah.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, cah.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, cah.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, cah.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, cah.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", cah.CAH_Status);

				//Matching Session
				var (matchedCAH, matchedReceipt, matchingBase) = MatchARReceiptWithMatchingJournal(creator, cah.PK, receipt.PK);

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
		public void TestReversalOfAReceiptReversesMatchingJournalandCashAdvanceStatus()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var (job, cah, receipt) = CreateJobAndCashAdvanceHeader(creator, 1050M);

				AssertEquals("Local Outstanding Ammount", 1050M, cah.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, cah.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, cah.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, cah.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, cah.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, cah.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", cah.CAH_Status);

				//Matching Session
				var (matchedCAH, matchedReceipt, matchingBase) = MatchARReceiptWithMatchingJournal(creator, cah.PK, receipt.PK);

				AssertEquals("Local Outstanding Ammount", 0M, matchedCAH.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 0M, matchedCAH.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, matchedCAH.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, matchedCAH.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 1050M, matchedCAH.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 1050M, matchedCAH.CAH_OSPaidAmount);
				AssertEquals("Status", "PAI", matchedCAH.CAH_Status);

				var carJournal = matchedCAH.Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvanceReceived));
				AssertNotNull(carJournal);

				var reversingFactory = new ReversingFactory();
				var reverser = reversingFactory.NewReversing(matchedReceipt);
				AssertNotNull(reverser as PayablesAndReceivablesReversing);

				var reverseReceiptPostDate = TestDateAttribute.Date.AddDays(-2);
				var receiptReverser = reverser as PayablesAndReceivablesReversing;
				receiptReverser.DoReverseTransaction_ForTestOnly();
				(matchedReceipt.ReverseTransaction as TransactionHeader).AH_PostDate = reverseReceiptPostDate;
				(matchedReceipt.ReverseTransaction as TransactionHeader).ReversingReason = "Random reason";
				matchedReceipt.Factory.Save();

				AssertEquals("Local Outstanding Ammount", 1050M, matchedCAH.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, matchedCAH.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, matchedCAH.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, matchedCAH.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, matchedCAH.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, matchedCAH.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", matchedCAH.CAH_Status);

				var reversedReceipt = matchedReceipt.Factory.LoadTop1<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, matchedReceipt.PK));
				AssertEquals(true, matchedReceipt.IsReversed);
				AssertNotNull("There should be a reversed receipt", reversedReceipt);
				AssertEquals(nameof(reversedReceipt.AH_OSExTaxAmount), (-1) * matchedReceipt.AH_OSExTaxAmount, reversedReceipt.AH_OSExTaxAmount);
				AssertEquals(nameof(reversedReceipt.AH_RX_NKTransactionCurrency), matchedReceipt.AH_RX_NKTransactionCurrency, reversedReceipt.AH_RX_NKTransactionCurrency);
				AssertEquals(nameof(reversedReceipt.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {matchedReceipt.AH_TransactionNum} Random reason"), reversedReceipt.AH_Desc);

				var reversedJournal = matchedReceipt.Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, carJournal.PK));
				AssertEquals(true, carJournal.IsReversed);
				AssertNotNull("There should be a reversed CAR journal", reversedJournal);
				AssertEquals(nameof(reversedJournal.AH_OSExTaxAmount), (-1) * carJournal.AH_InvoiceAmount, reversedJournal.AH_InvoiceAmount);
				AssertEquals(nameof(reversedJournal.AH_RX_NKTransactionCurrency), carJournal.AH_RX_NKTransactionCurrency, reversedJournal.AH_RX_NKTransactionCurrency);
				AssertEquals(nameof(reversedJournal.AH_Desc), FormattableString.Invariant($"Reversal Related to [{carJournal.AH_TransactionNum}]. Receipt [{matchedReceipt.AH_TransactionNum}] reversed. Random reason"), reversedJournal.AH_Desc);
				AssertEquals(nameof(reversedJournal.AH_PostDate), reverseReceiptPostDate, reversedJournal.AH_PostDate);
			}
		}

		(Job job, AccCashAdvanceRequestHeader cah, ARReceipt receipt) CreateJobAndCashAdvanceHeader(TestObjectCreator creator, ZDecimal amount)
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

			var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsReceivable, 0M, 0M, "AUD");
			var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, amount, amount);
			charge.JR_CAL_ARLine = line.PK;

			//Creating an AR receipt
			var aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = creator.ABIGAS.PK;
			aRRec.AH_LocalExTaxAmount = amount;
			aRRec.AH_OSExTaxAmount = amount;
			Factory.Save();

			return (shipmentJob, header, aRRec);

			void SetupCharge(JobCharge localCharge, bool isCashAdvanceRequired)
			{
				localCharge.JR_AC = creator.CC1.PK;
				localCharge.JR_OH_SellAccount = creator.ABIGAS.PK;
				localCharge.JR_OSSellAmt = amount;
				localCharge.JR_IsARCashAdvance = isCashAdvanceRequired;
				localCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			}
		}

		(CashAdvanceRequestHeader matchedCAH, ARReceipt matchedReceipt, ARMatchingBase matchingBase) MatchARReceiptWithMatchingJournal(TestObjectCreator creator, ZGuid cahPK, ZGuid receiptPK)
		{
			//Matching Session
			var newFactory2 = new BusinessObjectFactory();
			newFactory2.RefreshEnabled = false;
			var cahInNewFactory2 = newFactory2.Load<CashAdvanceRequestHeader>(cahPK);
			var arRecieptinNewFactory2 = newFactory2.Load<ARReceipt>(receiptPK);
			var matchingBase = new ARMatchingBase(newFactory2);
			matchingBase.MatchDate = ZDateTime.Today;
			matchingBase.PrimaryOrganization = creator.ABIGAS.PK;
			matchingBase.UnmatchedCashAdvanceRequests.Add(cahInNewFactory2);
			matchingBase.UnmatchedTransactions.Add(arRecieptinNewFactory2);
			matchingBase.MoveAllFromUnmatchToMatch();
			matchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
			matchingBase.Match_ForTestOnly();
			newFactory2.Save();
			return (cahInNewFactory2, arRecieptinNewFactory2, matchingBase);
		}

		protected override bool CanCashAdvanceRequestBeMatched => true;
	}
}
