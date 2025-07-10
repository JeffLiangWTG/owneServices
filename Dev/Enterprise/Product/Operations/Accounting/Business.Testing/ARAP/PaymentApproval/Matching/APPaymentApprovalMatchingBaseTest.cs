using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalMatching))]
	public class APPaymentApprovalMatchingBaseTest : PaymentApprovalMatchingBaseTest
	{
		public override void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhenBothMatchFails(string exceptionMessage)
		{
			var expectError = $@"Balance is not zero, Balance is 98.0

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AP, Transaction type: UNA, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AP, Transaction type: PAY, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: EXX, Payment Amount: -7, Outstanding Amount: -7, OS Payment Amount: -7, OS Outstanding Amount: -7, Currency: AUD, Exchange Rate Amount: 1

There are matching errors:
Balance of transaction match result is not zero.

MatchedTransactions contains transaction from primary organization: true

Match group info: There is no data collected.";

			AssertContains(expectError, exceptionMessage);
		}

		public override void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhen2ndMatchFails(string exceptionMessage)
		{
			var expectError = @"Balance is not zero, Balance is -2.0

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AP, Transaction type: UNA, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AP, Transaction type: PAY, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: EXX, Payment Amount: -7, Outstanding Amount: -7, OS Payment Amount: -7, OS Outstanding Amount: -7, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

There are matching errors:
Balance of transaction match result is not zero.

MatchedTransactions contains transaction from primary organization: true

Match group info: There is no data collected.";

			AssertContains(expectError, exceptionMessage);
		}

		#region Inherited Tests

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetTestMatchingBase();
		}

		protected override MatchingBase GetTestMatchingBase()
		{
			PaymentApprovalBase aPApproval = GetNewPaymentApproval();
			return new APPaymentApprovalMatching(Factory, aPApproval);
		}

		protected override MatchingBase GetTestMatchingBaseInNewFactory(BusinessObjectFactory factoryForNewMatchingObject)
		{
			PaymentApprovalBase aPApproval = GetNewPaymentApproval();
			return new APPaymentApprovalMatching(factoryForNewMatchingObject, aPApproval);
		}

		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			PaymentApprovalBase aPApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			aPApproval.AV_OH = TestOrgHeader.PK;

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 3;
			chequeBook.AK_CurrentNo = 2;
			chequeBook.AK_AB = bankAccount.PK;
			aPApproval.AV_AB = bankAccount.PK;
			aPApproval.AV_AK = chequeBook.PK;

			return aPApproval;
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
			//APPaymentApprovalMatching does not have ReceiptPaymentDetail
			throw new NotImplementedException();
		}

		protected override bool CanCashAdvanceRequestBeMatched => true;

		#region TestMatchDateDefaultValue

		public override void TestMatchDateDefaultValue()
		{
			base.TestMatchDateDefaultValue();
			PaymentApprovalBase aPApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			ZDateTime expectedDate = ZDateTime.Today;
			aPApproval.AV_PostDate = expectedDate;
			MatchingBase testMatchingBase = new APPaymentApprovalMatching(Factory, aPApproval);
			AssertEquals(string.Format("APPaymentApprovalMatching's default MatchDate should be {0}", expectedDate), expectedDate, testMatchingBase.MatchDate);
		}

		public override void TestMatchDateUsesMaxPostDateFromMatchedTransactions()
		{
			base.TestMatchDateUsesMaxPostDateFromMatchedTransactions();
			PaymentApprovalBase aPApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			ZDateTime approvalPostDate = ZDateTime.Today.AddMonths(-1);
			aPApproval.AV_PostDate = approvalPostDate;
			aPApproval.AV_OH = TestOrgHeader.PK;
			aPApproval.AV_Amount = 300M;

			APInvoice invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_OSExTaxAmount = 150M;
			invoice1.AH_PostDate = approvalPostDate.AddDays(5);

			APInvoice invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_TransactionNum = "00001002";
			invoice2.AH_OSExTaxAmount = 150M;
			invoice2.AH_PostDate = approvalPostDate;

			PaymentApprovalItem approvalItem1 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			approvalItem1.A2_AV = aPApproval.PK;
			approvalItem1.A2_AH = invoice1.PK;
			approvalItem1.A2_PaymentThisRun = -150m;

			PaymentApprovalItem approvalItem2 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			approvalItem2.A2_AV = aPApproval.PK;
			approvalItem2.A2_AH = invoice2.PK;
			approvalItem2.A2_PaymentThisRun = -150m;

			Factory.Save();

			MatchingBase testMatchingBase = new APPaymentApprovalMatching(Factory, aPApproval);
			AssertEquals(string.Format("APPaymentApprovalMatching's default MatchDate should be {0}", approvalPostDate), invoice1.AH_PostDate, testMatchingBase.MatchDate);
		}

		#endregion

		#region TestMatchLinksPostDate

		[TestDate(2020, 1, 8, 00, 00, 0)]
		public override void TestMatchLinksMatchDate()
		{
			SetUpTestDataSet();
			Factory.Save();

			Period testPeriod = Factory.NewWithValidTestData<Period>();
			testPeriod.AM_Year = (ZShort)ZDateTime.Now.Year;
			testPeriod.AM_StartDate = new ZDateTime(testPeriod.AM_Year, ZDateTime.Today.Month, 1);
			testPeriod.AM_EndDate = testPeriod.AM_StartDate.AddMonths(1).AddDays(-1);

			var expectedDate = ZDateTime.Today.AddDays(-7);
			PaymentApprovalBase aPApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			aPApproval.AV_OH = TestOrg1.PK;
			aPApproval.AV_Amount = 10M;
			aPApproval.AV_PaymentType = ReceiptTypes.EFT;
			aPApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			aPApproval.AV_ChequeOrReference = "121";

			fTestMatchingBase = aPApproval.PaymentMatchingBaseObject;

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Discount);
			TestMatchingBase.AddMiscellaneousTransaction(testDSC);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			TestMatchingBase.MatchDate = expectedDate;
			TestMatchingBase.MatchAndClearTransactions();
			aPApproval.PostDate = expectedDate;
			testDSC.AH_PostDate = expectedDate;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 Match link Rows should be created", 2, matchLinks.Count);
			foreach (TransactionMatchLink matchLink in matchLinks)
			{
				AssertEquals("MatchLink should have the same Match Date as in MatchingBase", expectedDate, matchLink.AP_MatchDate);
			}
		}

		#endregion

		#region Cash Advance

		public override void TestLoadCashAdvanceRequests()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
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
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
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
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
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

				TestMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(new List<Journal.Journal> { journal });
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

				var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsPayable, 0M, 0M, "AUD");
				var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, 1050M, 1050M);
				charge.JR_CAL_APLine = line.PK;

				//Creating an AP payment approval
				var apPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				apPaymentApproval.AV_OH = creator.ABIGAS.PK;
				apPaymentApproval.AV_Amount = 1050M;
				Factory.Save();

				AssertEquals("Local Outstanding Ammount", 1050M, header.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 1050M, header.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, header.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, header.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 0M, header.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 0M, header.CAH_OSPaidAmount);
				AssertEquals("Status", "REQ", header.CAH_Status);

				//Matching Session
				var newFactory2 = new BusinessObjectFactory();
				newFactory2.RefreshEnabled = false;
				var cahInNewFactory2 = newFactory2.Load<CashAdvanceRequestHeader>(header.PK);
				var apPaymentApprovalInNewFactory2 = newFactory2.Load<APPaymentApprovalWithoutAuthorisation>(apPaymentApproval.PK);
				var matchingBase = new APPaymentApprovalMatching(newFactory2, apPaymentApprovalInNewFactory2);
				matchingBase.MatchDate = ZDateTime.Today;
				matchingBase.PrimaryOrganization = creator.ABIGAS.PK;
				matchingBase.UnmatchedCashAdvanceRequests.Add(cahInNewFactory2);
				matchingBase.UnmatchedTransactions.Add(apPaymentApprovalInNewFactory2);
				matchingBase.MoveAllFromUnmatchToMatch();
				matchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
				matchingBase.Match_ForTestOnly();
				newFactory2.Save();

				AssertEquals("Local Outstanding Ammount", 0M, cahInNewFactory2.CAH_LocalOutstandingAmount);
				AssertEquals("OS Outstanding Ammount", 0M, cahInNewFactory2.CAH_OSOutstandingAmount);
				AssertEquals("Local Ammount", 1050M, cahInNewFactory2.CAH_LocalAmount);
				AssertEquals("OS Ammount", 1050M, cahInNewFactory2.CAH_OSAmount);
				AssertEquals("Local Paid Ammount", 1050M, cahInNewFactory2.CAH_LocalPaidAmount);
				AssertEquals("OS Paid Ammount", 1050M, cahInNewFactory2.CAH_OSPaidAmount);
				AssertEquals("Status", "PAI", cahInNewFactory2.CAH_Status);

				void SetupCharge(JobCharge localCharge, bool isCashAdvanceRequired)
				{
					localCharge.JR_AC = creator.CC1.PK;
					localCharge.JR_OH_CostAccount = creator.ABIGAS.PK;
					localCharge.JR_OSCostAmt = 1050m;
					localCharge.JR_IsAPCashAdvance = isCashAdvanceRequired;
					localCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				}
			}
		}

		#endregion

		#endregion
	}
}
