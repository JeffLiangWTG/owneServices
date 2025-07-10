using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(NewMatchGroupForm))]
	public class NewMatchGroupFormBasherTest : ZFormBasherTest
	{
		#region Balancing Journals Test

		public void TestHandleMoveUpAskAboutDeletingBalancingJournals()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				apJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.PaymentBasisWithholding;
				TestMatchingBase.MatchedTransactions.Add(apJournal);
				TestMatchingBase.MatchedTransactions.Add(invoice);
				testForm.Show();
				testForm.MatchTransactionsGrid_ForTestOnly.SelectAllElements();
				testForm.HandleMoveUp_ForTestOnly();

				AssertEquals("No prompt should be shown to user as journal is not balancing journal but is Withholding journal", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, TestMatchingBase.MatchedTransactions.Count);

				apJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Standard;
				TestMatchingBase.MatchedTransactions.Add(apJournal);
				TestMatchingBase.MatchedTransactions.Add(invoice);
				apJournal.RelatedInvoice = invoice;
				testForm.Show();
				testForm.MatchTransactionsGrid_ForTestOnly.SelectAllElements();
				testForm.HandleMoveUp_ForTestOnly();

				ZString message = @"This action will delete balancing journals.
Do you want to continue?";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, TestMatchingBase.MatchedTransactions.Count);

				apJournal = Factory.NewWithValidTestData<APJournal>();
				TestMatchingBase.MatchedTransactions.Add(apJournal);
				TestMatchingBase.MatchedTransactions.Add(invoice);
				apJournal.RelatedInvoice = invoice;
				testForm.MatchTransactionsGrid_ForTestOnly.Select(1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNotNull("Invoice is the only transaction selected", testForm.MatchTransactionsGrid_ForTestOnly.SelectedElements.Single(e => e == invoice));
				testForm.HandleMoveUp_ForTestOnly();
				AssertEquals("Message should still show for the journal because it is linked to the invoice", message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, TestMatchingBase.MatchedTransactions.Count);
			}
		}

		public void TestHandleMoveUpDoesNotIncludeBankFeeInBalancingJournals()
		{
			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
				Journal bankFee = (Journal)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Journal);
				TestMatchingBase.AddMiscellaneousTransaction(bankFee);
				testForm.Show();
				testForm.MatchTransactionsGrid_ForTestOnly.SelectAllElements();

				AssertCollectionContains("Bank fee journal should be in the matched transactions collection", bankFee, TestMatchingBase.MatchedTransactions);
				AssertCollectionNotContains("Bank fee journal should not be in the unmatched transactions collection", bankFee, TestMatchingBase.UnmatchedTransactions);
				AssertNoExceptionThrown("HandleMoveUp() should not attempt to delete the bank fee through balancing journals, otherwise it will throw an exception when it tries to remove it from the unmatched transactions collection",
					() => testForm.HandleMoveUp_ForTestOnly());
				Assert(bankFee.IsDeleted);
				ZString message = @"This action will delete balancing journals.
Do you want to continue?";
				AssertNotEquals("Message should not appear because bank fee is deleted", message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, TestMatchingBase.MatchedTransactions.Count);
			}
		}

		public void TestMainTabControl_SelectedIndexChanged()
		{
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			APJournal relatedAPJournal = Factory.NewWithValidTestData<APJournal>();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				APJournal apJournal = TestMatchingBase.BalancingAPJournals.AddNew();
				apJournal.RelatedJournal = relatedAPJournal;

				ARJournal arJournal = TestMatchingBase.BalancingARJournals.AddNew();
				arJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionAlreadyPaid;

				TestMatchingBase.MatchedTransactions.Add(arInvoice);
				TestMatchingBase.MatchedTransactions.Add(arJournal);
				testForm.Show();

				Assert(!apJournal.UseJournalValidation);
				Assert(!arJournal.UseJournalValidation);

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.APJournalsTabPage_ForTestOnly;
				Assert(!testForm.APJournalsGrid_ForTestOnly.Visible);
				Assert(apJournal.UseJournalValidation);

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.ARJournalsTabPage_ForTestOnly;
				Assert(testForm.ARJournalsGrid_ForTestOnly.Visible);
				Assert(arJournal.UseJournalValidation);

				TestMatchingBase.MatchedTransactions.Add(apInvoice);
				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.APJournalsTabPage_ForTestOnly;
				Assert(testForm.APJournalsGrid_ForTestOnly.Visible);

				TestMatchingBase.MatchedTransactions.Add(relatedAPJournal);

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.GridsTabPage_ForTestOnly;

				Assert(!apJournal.UseJournalValidation);
				Assert(!arJournal.UseJournalValidation);

				Assert(!TestMatchingBase.MatchedTransactions.Contains(arJournal));
				Assert(TestMatchingBase.MatchedTransactions.Contains(arJournal.RelatedJournal));

				Assert(!TestMatchingBase.MatchedTransactions.Contains(relatedAPJournal));
				Assert(relatedAPJournal.IsDeleted);
				AssertNull(apJournal.RelatedJournal);
				Assert(TestMatchingBase.MatchedTransactions.Contains(apJournal));

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.APJournalsTabPage_ForTestOnly;
				apJournal.RelatedJournal = relatedAPJournal;

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.GridsTabPage_ForTestOnly;
				Assert(!TestMatchingBase.MatchedTransactions.Contains(apJournal));
			}
		}

		public void TestMatchingRunJournalValidationToCheckCreatedJournals()
		{
			SetupDataForTest();

			APJournal relatedAPJournal = Factory.NewWithValidTestData<APJournal>();
			relatedAPJournal.AH_OH = TestOrg.PK;
			relatedAPJournal.AH_AB = TestBank.PK;
			relatedAPJournal.AH_OSExTaxAmount = 10m;
			Assert("Pre-condition: Should not have any errors", !relatedAPJournal.HasErrors);
			relatedAPJournal.AH_AG = ZGuid.Empty;
			Assert("Journal should have errors now", relatedAPJournal.HasErrors);

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = APInv.AH_OH;

				APJournal apJournal = TestMatchingBase.BalancingAPJournals.AddNew();
				apJournal.RelatedJournal = relatedAPJournal;
				apJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionAlreadyPaid;
				apJournal.DebitCreditSign = nameof(DebitCredit.DR);
				apJournal.AH_OSExTaxAmount = 10m;
				apJournal.AH_AG = ZGuid.Empty;
				Assert("Journal should have errors", apJournal.HasErrors);

				ARJournal arJournal = TestMatchingBase.BalancingARJournals.AddNew();
				arJournal.DebitCreditSign = nameof(DebitCredit.DR);
				arJournal.AH_OSExTaxAmount = 10m;

				TestMatchingBase.MatchedTransactions.Add(APInv);
				TestMatchingBase.MatchedTransactions.Add(APPay);
				TestMatchingBase.MatchedTransactions.Add(arJournal);
				TestMatchingBase.MatchedTransactions.Add(relatedAPJournal);

				testForm.Show();

				testForm.ValidateAll_ForTestOnly(ValidationType.Full);
				Assert("Should not be any errors on MatchingBase because it uses MatchingValidation", !TestMatchingBase.HasErrors);

				testForm.MatchAndContinueButton.PerformClick();

				AssertNotEquals("Should not be matched and saved with all those errors on Journals", SaveFactoryFlag.OK, testForm.SaveFactoryResult);
			}
		}

		public void TestInvoiceRemittanceReferenceIsReadonly()
		{
			using (var testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				testForm.Show();
				var matchTransationsGrid = testForm.MatchTransactionsGrid_ForTestOnly;

				Assert("InvoiceRemittanceReference has to be readonly", matchTransationsGrid.GetColumnStyle("InvoiceRemittanceReference").IsReadOnly);
			}
		}

		#endregion

		#region Implementation

		protected void SetupData()
		{
			ZGuid genericChargeCodeGuid = TestObjectCreator.RevenueNoTaxChargeCode.PK;
			Factory.Save();
			TestARINV = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARINV, TestARINV.TransactionCurrency, TestARINV.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m, genericChargeCodeGuid);
			TestARINV.AH_LocalOutstandingAmount = 100M;
			((IMatching)TestARINV).OSPartialPaymentAmount = 100M;
		}

		void SetupDataForTest()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			TestOrg = GetNewOrgHeader();
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			TestCheques = GetNewChequeBookForBank(TestBank);
			Factory.Save();

			APInv = GetNewAPInvoice(30M);
			Factory.Save();

			APPay = GetNewAPPayment(20M);
		}

		OrgHeader SetupDataForTestWithCreditNoteAndInvoice()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsDebtor = true;

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			Factory.Save();

			ZDecimal sum = 50M;

			ARCreditNote creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = orgHeader.PK;
			creditNote.AH_LocalExTaxAmount = sum;
			creditNote.AH_OSExTaxAmount = sum;

			InvoicingLineBase creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
			creditNoteLine.AL_LocalExTaxAmount = sum;
			creditNoteLine.AL_OSExTaxAmount = sum;
			creditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			creditNote.AH_FullyPaidDate = ZDateTime.Empty;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_LocalExTaxAmount = sum;
			invoice.AH_OSExTaxAmount = sum;

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_LocalExTaxAmount = sum;
			invoiceLine.AL_OSExTaxAmount = sum;
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			return orgHeader;
		}

		protected override Form GetFormToBashCore()
		{
			TestMatchingBase = new ARMatchingBase(Factory);
			return new NewMatchGroupForm(TestMatchingBase);
		}

		MatchingBase TestMatchingBase;
		ARInvoice TestARINV;
		APInvoice APInv;
		APPayment APPay;
		AccBankAccount TestBank;
		AccChequeBook TestCheques;
		OrgHeader TestOrg;
		ZBool MatchAndCloseWasPressed;

		#region Test Object Creation

		APPayment GetNewAPPayment(ZDecimal amount)
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = TestOrg.PK;
			aPPay.AH_AB = TestBank.PK;
			aPPay.ChequeBook = TestCheques.PK;
			aPPay.AH_ChequeOrReference = "90";
			aPPay.AH_OSExTaxAmount = amount;
			return aPPay;
		}

		APInvoice GetNewAPInvoice(ZDecimal amount)
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = TestOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;
			aPInv.AH_FullyPaidDate = ZDateTime.Empty;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(aPInv, aPInv.TransactionCurrency, aPInv.AH_ExchangeRate, amount, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;

			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, aPInv.TransactionCurrency);

			Factory.Save();

			return aPInv;
		}

		AccChequeBook GetNewChequeBookForBank(AccBankAccount bank)
		{
			AccChequeBook cheques = Factory.NewWithValidTestData<AccChequeBook>();
			cheques.AK_StartNo = 1;
			cheques.AK_LastNo = 100;
			cheques.AK_CurrentNo = 1;
			cheques.AK_AB = bank.PK;
			return cheques;
		}

		OrgHeader GetNewOrgHeader()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			org.CompanyData.SetAPTaxApplicable(false);
			org.OH_IsDebtor = true;
			return org;
		}

		APMatchingBase GetNewAPMatchingBaseWithPayment(Payment paymentBizO)
		{
			APMatchingBase matchingBizO = new APMatchingBase(Factory, paymentBizO);
			return matchingBizO;
		}

		#endregion

		#endregion

		#region TestSaveFactoryFlag

		public void TestSaveFactoryFlag()
		{
			SetupData();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestARINV.AH_OH = testOrg.PK;

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(testAPINV, testAPINV.TransactionCurrency, testAPINV.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;

			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPINV.TransactionCurrency);

			testAPINV.AH_LocalOutstandingAmount = 100M;
			((IMatching)testAPINV).OSPartialPaymentAmount = -100M;
			testAPINV.AH_OH = testOrg.PK;
			Factory.Save();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				AssertEquals("Should not be able to save a newly created form", SaveFactoryFlag.Cancel, testForm.SaveFactoryResult);
				TestMatchingBase.PrimaryOrganization = testOrg.PK;
				testForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should not be able to save", SaveFactoryFlag.Cancel, testForm.SaveFactoryResult);

				TestMatchingBase.MatchedTransactions.Add(testAPINV);
				TestMatchingBase.MatchedTransactions.Add(TestARINV);

				testForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should be able to save", SaveFactoryFlag.OK, testForm.SaveFactoryResult);

				TransactionMatchLink unbalancedLink = ((IMatching)testAPINV).CurrentMatchGroup.AddNew();
				unbalancedLink.AP_AH = testAPINV.PK;
				unbalancedLink.AP_Amount = 10M;
				unbalancedLink.AP_MatchGroupNum = ((IMatching)testAPINV).CurrentMatchGroup[0].AP_MatchGroupNum;

				testForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should not be able to save", SaveFactoryFlag.Cancel, testForm.SaveFactoryResult);
			}
		}

		#endregion

		#region Finding and Selecting

		#region TestHandleFind

		public void TestHandleFind()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;
			Factory.Save();

			TestARINV = Factory.NewWithValidTestData<ARInvoice>();
			TestARINV.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(TestARINV, TestARINV.TransactionCurrency, 1M, 90M);

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, 1M, 90M);
			Factory.Save();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = testOrg.PK;
				testForm.Show();
				testForm.HandleFind_ForTestOnly();
				AssertEquals("Should be 2 transactions in UnmatchedTransactions", 2, TestMatchingBase.UnmatchedTransactions.Count);

				var legderFilter = (DependentListFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];
				legderFilter.Property1 = LedgerTypes.AccountsPayable;
				legderFilter.IsActive = true;

				testForm.HandleFind_ForTestOnly();

				int indexOfAPInv = ((IList)TestMatchingBase.UnmatchedTransactions).IndexOf(testAPInv);
				Assert("APInvoice should be in grid", indexOfAPInv != -1);
				Assert("APInvoice should be selected", testForm.UnmatchedTransactionsGrid_ForTestOnly.IsSelected(indexOfAPInv));

				int indexOfARInv = ((IList)TestMatchingBase.UnmatchedTransactions).IndexOf(TestARINV);
				Assert("ARInvoice should not be in grid because we reload collection", indexOfARInv == -1);
			}
		}

		#endregion

		#region TestHandleAutoSelect

		public void TestHandleAutoSelect()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;

			ARInvoice testARInv = Factory.NewWithValidTestData<ARInvoice>();
			testARInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testARInv, testARInv.TransactionCurrency, testARInv.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);

			Factory.Save();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				testForm.Show();
				TestMatchingBase.PrimaryOrganization = testOrg.PK;

				var legderFilter = (DependentListFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];
				legderFilter.Property1 = LedgerTypes.AccountsPayable;
				legderFilter.IsActive = true;

				testForm.HandleAutoSelect_ForTestOnly();

				AssertEquals("TestAPInv should be in MatchedTransactions", 1, TestMatchingBase.MatchedTransactions.Count);
				Assert("TestAPInv should be in MAtchedTransactions", TestMatchingBase.MatchedTransactions.Contains(testAPInv));
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestNoExceptionsOnMoveUpHandle()
		{
			APPaymentApprovalWithAuthorisation payment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.MatchedTransactions.Add(payment);

				testForm.Show();
				testForm.MatchTransactionsGrid_ForTestOnly.SelectAllElements();
				testForm.HandleMoveUp_ForTestOnly();

				TestMatchingBase.MatchedTransactions.Add(payment);
				testForm.HandleUnselectAll_ForTestOnly();
			}
		}

		#endregion

		#region Settlement Organisation Grid

		#region TestSettlementOrgGridEditable

		public void TestSettlementOrgGridEditable()
		{
			APPayment testPay = Factory.NewWithValidTestData<APPayment>();
			TestMatchingBase = new ARMatchingBase(Factory, testPay);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(TestMatchingBase))
			{
				testForm.Show();
				testForm.SettlementOrgsTabPage_ForTestOnly.Show();
				Assert("SettlementOrgGrid should be editable", !testForm.OrgInfoGrid_ForTestOnly.ReadOnly);
			}

			TestMatchingBase = new ARMatchingBase(Factory);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(TestMatchingBase))
			{
				testForm.Show();
				testForm.SettlementOrgsTabPage_ForTestOnly.Show();
				Assert("SettlementOrgGrid should not be editable because Primary org has not been entered", testForm.OrgInfoGrid_ForTestOnly.ReadOnly);
			}

			TestMatchingBase = new ARMatchingBase(Factory);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(TestMatchingBase))
			{
				testForm.Show();
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
				testForm.SettlementOrgsTabPage_ForTestOnly.Show();
				Assert("SettlementOrgGrid should be editable because Primary org has already been entered", !testForm.OrgInfoGrid_ForTestOnly.ReadOnly);
			}
		}

		#endregion

		#region TestShowGridsReloadsFromDB

		public void TestShowGridsReloadsFromDB()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_IsDebtor = true;
			testOrg2.OH_IsCreditor = true;
			Factory.Save();

			TestARINV = Factory.NewWithValidTestData<ARInvoice>();
			TestARINV.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(TestARINV, TestARINV.TransactionCurrency, TestARINV.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);

			ARJournal testARJnl = Factory.NewWithValidTestData<ARJournal>();
			testARJnl.AH_OH = testOrg.PK;
			testARJnl.AH_LocalExTaxAmount = 80M;
			testARJnl.AH_OSExTaxAmount = 80M;
			testARJnl.AH_LocalOutstandingAmount = 80M;

			Factory.Save();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = testOrg2.PK;
				testForm.Show();

				TestMatchingBase.MatchedTransactions.Add(testARJnl);

				OrgLedgerFilter settlementOrg2 = TestMatchingBase.MatchingFilterBizO.SettlementOrgInfos.AddNew();
				settlementOrg2.OrganisationBizO = testOrg;

				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.SettlementOrgsTabPage_ForTestOnly;
				testForm.MatchingTabControl_ForTestOnly.SelectedTab = testForm.GridsTabPage_ForTestOnly;

				AssertEquals("Unmatched Transactions should reload from the DB", 1, TestMatchingBase.UnmatchedTransactions.Count);
				Assert("Unmatched Transactions should contain the TestARINV", TestMatchingBase.UnmatchedTransactions.Contains(TestARINV));
			}
		}

		#endregion

		#endregion

		#region Miscellaneous Transactions

		#region TestEditMiscellaneousTransaction

		public void TestEditMiscellaneousTransaction()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;
			Factory.Save();

			TestARINV = Factory.NewWithValidTestData<ARInvoice>();
			TestARINV.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(TestARINV, TestARINV.TransactionCurrency, 1M, 90M);
			Factory.Save();

			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = testOrg.PK;
				testForm.ControllerID = ControllerIDs.ZARMatching;
				testForm.Show();
				testForm.HandleSelectAll_ForTestOnly();
				TransactionViewForm miscForm = (TransactionViewForm)testForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);

				miscForm.FireSaveButton();
				miscForm.Dispose();

				ZController miscTransController = AccountingControllerCreator.GetNewController(TestMatchingBase.ExchangeDiffCurrent);
				if (miscTransController != null)
				{
					miscTransController.ShowEditForm(TestMatchingBase.ExchangeDiffCurrent);
				}

				miscTransController.LastShownForm.Dispose();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, TestMatchingBase.MatchedTransactions.Count);
			}
		}

		#endregion

		#region TestMiscTransactionFormIsModal

		public void TestMiscTransactionFormIsModal()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;

			Factory.Save();

			ARInvoice testInv = Factory.NewWithValidTestData<ARInvoice>();
			testInv.AH_OH = testOrg.PK;
			testInv.AH_LocalExTaxAmount = 90M;

			Factory.Save();

			APMatchingBase matchingBizO = new APMatchingBase(Factory);
			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(matchingBizO))
			{
				matchingForm.ControllerID = ControllerIDs.ZARMatching;
				TransactionViewForm oVPForm = null;
				try
				{
					matchingBizO.PrimaryOrganization = testOrg.PK;

					matchingForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.Overpayment);

					oVPForm = (TransactionViewForm)ZFormModaliser.ActiveForm;
					AssertEquals("New OVP Form should be shown modally", matchingForm, ZFormModaliser.GetParentFormForModalForm(oVPForm));
					AssertEquals("New OVP Form should be shown modally", oVPForm, ZFormModaliser.GetActiveChildFormForParentForm(matchingForm));
				}
				finally
				{
					if (oVPForm != null)
					{
						oVPForm.Dispose();
					}
				}
			}
		}

		#endregion

		#region Misc Transaction Checkpoints

		public void TestMiscTransactionOverpaymentARCheckpointIsAllowed()
			=> TestMiscTransactionCheckpoint(true, Env.Security.ReceivablesNewMatchTransactionsOverpaymentType, "Manage -> Receivables -> Match Transactions -> New Receivables Transaction Matching -> Overpayment Transactions", new ARMatchingBase(Factory), typeof(ARInvoice), ControllerIDs.ZARMatching, frm => frm.OverpaymentButton_ForTestOnly, frm => frm.OVPMenuItem_ForTestOnly);
		public void TestMiscTransactionOverpaymentARCheckpointIsDenied()
			=> TestMiscTransactionCheckpoint(false, Env.Security.ReceivablesNewMatchTransactionsOverpaymentType, "Manage -> Receivables -> Match Transactions -> New Receivables Transaction Matching -> Overpayment Transactions", new ARMatchingBase(Factory), typeof(ARInvoice), ControllerIDs.ZARMatching, frm => frm.OverpaymentButton_ForTestOnly, frm => frm.OVPMenuItem_ForTestOnly);

		public void TestMiscTransactionOverpaymentAPCheckpointIsAllowed()
			=> TestMiscTransactionCheckpoint(true, Env.Security.PayablesNewMatchTransactionsOverpaymentType, "Manage -> Payables -> Match Transactions -> New Payables Transaction Matching -> Overpayment Transactions", new APMatchingBase(Factory), typeof(APInvoice), ControllerIDs.ZAPMatching, frm => frm.OverpaymentButton_ForTestOnly, frm => frm.OVPMenuItem_ForTestOnly);
		public void TestMiscTransactionOverpaymentAPCheckpointIsDenied()
			=> TestMiscTransactionCheckpoint(false, Env.Security.PayablesNewMatchTransactionsOverpaymentType, "Manage -> Payables -> Match Transactions -> New Payables Transaction Matching -> Overpayment Transactions", new APMatchingBase(Factory), typeof(APInvoice), ControllerIDs.ZAPMatching, frm => frm.OverpaymentButton_ForTestOnly, frm => frm.OVPMenuItem_ForTestOnly);

		public void TestMiscTransactionCheckpoint(bool checkpointIsAllowed, Security.SecurityCheckpoint checkpoint, string expectedCheckpointPath, MatchingBase matchingBizo, Type ledgerBizoType, ControllerID controllerID, Func<NewMatchGroupForm, Button> buttonGetter, Func<NewMatchGroupForm, MenuItem> menuItemGetter)
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;

			var testInv = (InvoicingBase)Factory.NewWithValidTestData(ledgerBizoType);
			testInv.AH_OH = testOrg.PK;
			testInv.AH_LocalExTaxAmount = 90M;
			TestObjectCreator.CreateInvoiceLine(testInv, testInv.TransactionCurrency, 1M, 90M);
			Factory.Save();

			checkpoint.IsAllowed = checkpointIsAllowed;
			try
			{
				using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(matchingBizo))
				{
					AssertNull("Precondition: no active form", ZFormModaliser.ActiveForm);
					UnitTestUserNotification.Instance.ClearMessages();

					matchingBizo.PrimaryOrganization = testOrg.PK;
					matchingForm.ControllerID = controllerID;
					matchingForm.Show();
					matchingForm.HandleSelectAll_ForTestOnly();

					var testButton = buttonGetter(matchingForm);
					testButton.PerformClick();
					AssertCheckpointAndResetUIState();

					var testMenuItem = menuItemGetter(matchingForm);
					testMenuItem.PerformClick();
					AssertCheckpointAndResetUIState();
				}
			}
			finally
			{
				ZFormModaliser.ActiveForm?.Dispose();
				checkpoint.ClearOverriddenSecurityValue();
			}

			void AssertCheckpointAndResetUIState()
			{
				if (checkpointIsAllowed)
				{
					CombineAssertions(() =>
					{
						AssertNull("No notification should be shown when security is allowed", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertType("Form should be shown when security is allowed", typeof(TransactionViewForm), ZFormModaliser.ActiveForm);
					});
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertEquals("Security notification should be shown when security is denied", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + expectedCheckpointPath, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNull("No form should be shown when security is denied", ZFormModaliser.ActiveForm);
					});
				}

				ZFormModaliser.ActiveForm?.Dispose();
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		#endregion

		#endregion

		#region TestControlEnterForReceiptMatching

		public void TestControlEnterForReceiptMatching()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			Factory.Save();

			ARReceipt testRec = Factory.NewWithValidTestData<ARReceipt>();
			testRec.AH_OH = testOrg.PK;
			testRec.AH_AB = testBank.PK;
			testRec.AH_OSExTaxAmount = 90M;
			testRec.AH_ChequeOrReference = "90";
			testRec.AH_DrawerBank = "TestBank";
			testRec.AH_DrawerBranch = "TestBranch";
			testRec.AH_ChequeDrawer = "TestDrawer";

			ARInvoice testInv = Factory.NewWithValidTestData<ARInvoice>();
			testInv.AH_OH = testOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.RevenueNoTaxChargeCode.PK;
			TestObjectCreator.CreateInvoiceLine(testInv, testInv.TransactionCurrency, testInv.AH_ExchangeRate, 90m, 0m, 0m, genericChargeCodeGuid);
			Factory.Save();
			ARMatchingBase testMatchingBase = new ARMatchingBase(Factory, testRec);
			testMatchingBase.PrimaryOrganization = testOrg.PK;

			using (NewMatchGroupForm testMatchForm = new NewMatchGroupForm(testMatchingBase))
			{
				testMatchForm.Show();
				testMatchForm.HideMatchAndContinueButtonForReceiptPayment();
				testMatchForm.HandleSelectAll_ForTestOnly();

				testMatchForm.Closed += new EventHandler(TestMatchForm_Closed);
				MatchAndCloseWasPressed = false;
				TestKeyStrokeHelper.SendKeyToControl(testMatchForm, (Keys.Control | Keys.Enter), true);

				Assert("Match and Close was not pressed", MatchAndCloseWasPressed);
			}
		}

		#endregion

		#region TestFormClosesAfterSelectedTransactionsEdited

		public void TestFormClosesAfterSelectedTransactionsEdited()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(TestMatchingBase))
			{
				matchingForm.Show();
				TestMatchingBase.PrimaryOrganization = TestOrg.PK;
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				APInv.AH_OutstandingAmount = -20M;  // This will set HasChanges on MatchingBase to true; note that setting OSPartialPayment does not work

				Assert("HasChanges should be true on MatchingBase", TestMatchingBase.HasChanges);
				AssertEquals("MatchingBase DisplayMode should be 'NewSaved'", ZArchitecture.Core.ODisplayMode.NewSaved, matchingForm.DisplayMode);

				matchingForm.CancelButton.PerformClick();

				Assert("The Dialog prompt for saving should come up", UnitTestUserNotification.Instance.LastMessage.Contains("Would you like to save the changes?"));
			}
		}

		#endregion

		#region TestRefreshBalanceValue

		public void TestRefreshBalanceValue()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(TestMatchingBase))
			{
				matchingForm.Show();
				TestMatchingBase.PrimaryOrganization = TestOrg.PK;
				TestMatchingBase.MoveAllFromUnmatchToMatch();

				AssertEquals("Balance should be equl -10.00", "-10.00", matchingForm.BalanceGroupBox_ForTestOnly.Controls["zCalcFindBox5"].Controls["AmountCalcEdit"].Text);
			}
		}

		#endregion

		#region TestFormClosingAfterMatchAndClosePressed

		public void TestFormClosingAfterMatchAndClosePressed()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(TestMatchingBase))
			{
				matchingForm.Show();
				TestMatchingBase.PrimaryOrganization = TestOrg.PK;
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				APInv.Lines[0].AL_LineAmount = -20M;    // This will set HasChanges on MatchingBase to true; note that setting OSPartialPayment does not work
				APInv.Lines[0].AL_LineAmount = -30M;
				((IMatching)APInv).OSPartialPaymentAmount = -20M;

				Assert("HasChanges should be true on MatchingBase", TestMatchingBase.HasChanges);

				TestMatchingBase.RunPreSaveValidation();
				Assert("Matching base should have no errors", !TestMatchingBase.HasErrors);

				matchingForm.Closed += new EventHandler(TestMatchForm_Closed);
				MatchAndCloseWasPressed = false;

				matchingForm.MatchAndCloseButton.PerformClick();

				Assert("Form should close", MatchAndCloseWasPressed);
				Assert("Matching form should close after matching", !matchingForm.Visible);
			}
		}

		#endregion

		#region TestSelectedTransactionsEditedDoesNotAlterButtons

		public void TestSelectedTransactionsEditedDoesNotAlterButtons()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(TestMatchingBase))
			{
				matchingForm.Show();
				TestMatchingBase.PrimaryOrganization = TestOrg.PK;
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				APInv.AH_OutstandingAmount = -20M;  // This will set HasChanges on MatchingBase to true; note that setting OSPartialPayment does not work

				Assert("Matching Base HasChanges = true", TestMatchingBase.HasChanges);
				Assert("MatchAndContinueButton should NOT be enabled", !matchingForm.MatchAndContinueButton.Enabled);
			}
		}

		#endregion

		void TestMatchForm_Closed(object sender, EventArgs e)
		{
			MatchAndCloseWasPressed = true;
		}

		#region TestExchangeRateDecimalPlaces

		public void TestExchangeRateDecimalPlaces()
		{
			SetupDataForTest();

			ZBool originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				TestMatchingBase = new APMatchingBase(Factory);
				using (NewMatchGroupForm matchGroupForm = new NewMatchGroupForm(TestMatchingBase))
				{
					matchGroupForm.Show();
					ZCalcEditColumnStyle matchGridExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.MatchTransactionsGrid_ForTestOnly.Columns["ExchangeRateAmount"].ColumnStyle;
					AssertEquals("ExchangeRate Decimal places should be 6", 6, matchGridExRateColumn.Decimals);

					ZCalcEditColumnStyle unmatchGridExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.UnmatchedTransactionsGrid_ForTestOnly.Columns["ExchangeRateAmount"].ColumnStyle;
					AssertEquals("ExchangeRate Decimal places should be 6", 6, unmatchGridExRateColumn.Decimals);

					ZCalcEditColumnStyle avgExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.CurrencySummaryGrid_ForTestOnly.Columns["AverageExRate"].ColumnStyle;
					AssertEquals("Average ExchangeRate decimal places should be 6", 6, avgExRateColumn.Decimals);
				}

				GlbCompany.CurrentCompany.GC_IsReciprocal = false;

				using (NewMatchGroupForm matchGroupForm = new NewMatchGroupForm(TestMatchingBase))
				{
					matchGroupForm.Show();
					ZCalcEditColumnStyle matchGridExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.MatchTransactionsGrid_ForTestOnly.Columns["ExchangeRateAmount"].ColumnStyle;
					AssertEquals("ExchangeRate Decimal places should be 6", 6, matchGridExRateColumn.Decimals);

					ZCalcEditColumnStyle unmatchGridExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.UnmatchedTransactionsGrid_ForTestOnly.Columns["ExchangeRateAmount"].ColumnStyle;
					AssertEquals("ExchangeRate Decimal places should be 6", 6, unmatchGridExRateColumn.Decimals);

					ZCalcEditColumnStyle avgExRateColumn = (ZCalcEditColumnStyle)matchGroupForm.CurrencySummaryGrid_ForTestOnly.Columns["AverageExRate"].ColumnStyle;
					AssertEquals("Average ExchangeRate decimal places should be 6", 6, avgExRateColumn.Decimals);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		#endregion

		#region TestChequeNumConflictMakesChequeNumEditable

		public void TestChequeNumConflictMakesChequeNumEditable()
		{
			SetupDataForTest();
			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm matchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				matchForm.Show();
				TestMatchingBase.PrimaryOrganization = TestOrg.PK;
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				((IMatching)APInv).OSPartialPaymentAmount = -20M;

				Assert("AH_ChequeOrReference should be read only on Payment", APPay.AH_ChequeOrReferenceInfo.ReadOnly);
				Assert("ChequeOrReference should be read only", ((IMatching)APPay).ChequeOrReferenceInfo.ReadOnly);

				//Another Payment
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				APPayment testPay = newFactory.NewWithValidTestData<APPayment>();
				testPay.AH_OH = TestOrg.PK;
				testPay.AH_AB = TestBank.PK;
				testPay.ChequeBook = TestCheques.PK;
				testPay.AH_ChequeOrReference = "90";
				testPay.AH_OSExTaxAmount = 20M;

				newFactory.Save();

				matchForm.ValidateAndSave_ForTestOnly();

				Assert("AH_ChequeOrReference should NOT be valid", APPay.AH_ChequeOrReferenceInfo.HasErrors());
				Assert("AH_ChequeOrReference should NOT be read only", !APPay.AH_ChequeOrReferenceInfo.ReadOnly);
				Assert("ChequeOrReference should NOT be read only", !((IMatching)APPay).ChequeOrReferenceInfo.ReadOnly);
			}
		}

		#endregion

		#region TestAlterPaymentAmountFunction

		public void TestChangeReceiptAmountButtonIsDisabledWhenMatchingReceiptOrPaymentAlreadyInDatabase()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var receipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 20m, TestObjectCreator.AUDBankAccount.PK);
			receipt.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			var matchingBase = new ARMatchingBase(Factory, receipt);

			using (var matchForm = new NewMatchGroupForm(matchingBase))
			{
				matchForm.Show();
				matchForm.ChangePaymentAmountButton_ForTestOnly.PerformClick();
				var receiptForm = ZFormModaliser.LastFormShownForTest as ReceiptForm;

				Assert("Change Receipt Amount Button must be disabled when receipt is in DB.", !matchForm.ChangePaymentAmountButton_ForTestOnly.Enabled);
				AssertNull("Change Receipt Amount Button is disabled, so receipt form is not opened.", receiptForm);
			}
		}

		public void TestAlterPaymentAmountFunction()
		{
			SetupDataForAlterPaymentTest();

			PaymentApprovalBase approval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			TestMatchingBase = new APPaymentApprovalMatching(Factory, approval);

			using (NewMatchGroupForm newMatchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				newMatchForm.Show();
				TestMatchingBase.UnmatchedTransactions.Add(APInv);
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				newMatchForm.ChangePaymentAmountButton_Click_ForTestOnly(null, new EventArgs());

				AlterPaymentApprovalForm alterPayForm = ZFormModaliser.ActiveForm as AlterPaymentApprovalForm;
				AssertNotNull("The AlterPaymentApprovalForm should have popped up", alterPayForm);

				PaymentApprovalBase paymentToAlter = alterPayForm.BusinessEntity as PaymentApprovalBase;
				Assert("Payment in matching screen and business entity under change payment amount form should be the same",
					paymentToAlter.Equals(approval));
				AssertEquals("The Payment objects should be around the same row", paymentToAlter.PK, approval.PK);

				alterPayForm.Close();
			}
		}

		public void TestAlterReceiptAmountFunction()
		{
			SetupDataForAlterPaymentTest();

			APReceipt receipt = Factory.New<APReceipt>();
			TestMatchingBase = new APMatchingBase(Factory, receipt);

			using (NewMatchGroupForm newMatchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				newMatchForm.Show();
				TestMatchingBase.UnmatchedTransactions.Add(APInv);
				TestMatchingBase.MoveAllFromUnmatchToMatch();
				newMatchForm.ChangePaymentAmountButton_Click_ForTestOnly(null, new EventArgs());

				AlterReceiptForm alterRecForm = ZFormModaliser.ActiveForm as AlterReceiptForm;
				AssertNotNull("The AlterReceiptForm should have popped up", alterRecForm);

				alterRecForm.Close();
			}
		}

		public void TestAlterPaymentButtonOnlyEnabledForPayments()
		{
			SetupDataForAlterPaymentTest();

			using (NewMatchGroupForm newMatchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				Assert("ChangePaymentAmount button should be enabled", newMatchForm.ChangePaymentAmountButton_ForTestOnly.Enabled);
				Assert("ChangePaymentAmount button should not be readonly", !newMatchForm.ChangePaymentAmountButton_ForTestOnly.ReadOnly);
			}

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			TestMatchingBase = new ARMatchingBase(Factory, aRRec);

			using (NewMatchGroupForm newMatchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				Assert("ChangePaymentAmount button should be enabled", newMatchForm.ChangePaymentAmountButton_ForTestOnly.Enabled);
				Assert("ChangePaymentAmount button should not be readonly", !newMatchForm.ChangePaymentAmountButton_ForTestOnly.ReadOnly);
				AssertEquals("Change Receipt Amount", newMatchForm.ChangePaymentAmountButton_ForTestOnly.Text);
			}

			TestMatchingBase = new APMatchingBase(Factory);

			using (NewMatchGroupForm newMatchForm = new NewMatchGroupForm(TestMatchingBase))
			{
				Assert("ChangePaymentAmount button should not be enabled", !newMatchForm.ChangePaymentAmountButton_ForTestOnly.Enabled);
				Assert("ChangePaymentAmount button should be readonly", newMatchForm.ChangePaymentAmountButton_ForTestOnly.ReadOnly);
			}
		}

		void SetupDataForAlterPaymentTest()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			TestOrg = GetNewOrgHeader();
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			TestCheques = GetNewChequeBookForBank(TestBank);
			Factory.Save();

			APInv = GetNewAPInvoice(10M);
			Factory.Save();

			APPay = GetNewAPPayment(20M);
			TestMatchingBase = GetNewAPMatchingBaseWithPayment(APPay);
		}

		#endregion

		#region CurrencySummaryForm Test

		public void TestCurrencySummaryFormCaching()
		{
			using (NewMatchGroupForm form = (NewMatchGroupForm)GetFormToBash())
			{
				form.CurrencySummaryButton_Click_ForTestOnly(form, EventArgs.Empty);
				CurrencySummaryForm summaryForm = OpenedFormCache.GetInstance().GetForm(form.FMatchingBase_ForTestOnly.CurrencySummary.PK.ToGuid(), form.CurrencySummaryFormKey_ForTestOnly) as CurrencySummaryForm;
				AssertNotNull("Summary form should be cached", summaryForm);
				AssertEquals("There should be 1 form in the cache", 1, OpenedFormCache.GetInstance().Count);

				form.CurrencySummaryButton_Click_ForTestOnly(form, EventArgs.Empty);
				AssertEquals("There should still be 1 form in the cache", 1, OpenedFormCache.GetInstance().Count);
				summaryForm.Dispose();
			}
		}

		public void TestClosingMatchingFormClosesSummaryForm()
		{
			using (NewMatchGroupForm form = (NewMatchGroupForm)GetFormToBash())
			{
				form.Show();
				form.CurrencySummaryButton_Click_ForTestOnly(form, EventArgs.Empty);
				AssertEquals("There should be 1 form in the cache", 1, OpenedFormCache.GetInstance().Count);
				form.Close();
				AssertEquals("There should be no forms in the cache", 0, OpenedFormCache.GetInstance().Count);
			}
		}

		public void TestReopeningCurrencySummaryFormUsesCachedBizO()
		{
			using (NewMatchGroupForm form = (NewMatchGroupForm)GetFormToBash())
			{
				form.CurrencySummaryButton_Click_ForTestOnly(form, EventArgs.Empty);
				CurrencySummary summaryBizO = form.FMatchingBase_ForTestOnly.CurrencySummary;
				CurrencySummaryForm summaryForm = OpenedFormCache.GetInstance().GetForm(form.FMatchingBase_ForTestOnly.CurrencySummary.PK.ToGuid(), form.CurrencySummaryFormKey_ForTestOnly) as CurrencySummaryForm;
				summaryForm.Close();
				AssertEquals("There should be no forms in the cache", 0, OpenedFormCache.GetInstance().Count);

				form.CurrencySummaryButton_Click_ForTestOnly(form, EventArgs.Empty);
				summaryForm = OpenedFormCache.GetInstance().GetForm(form.FMatchingBase_ForTestOnly.CurrencySummary.PK.ToGuid(), form.CurrencySummaryFormKey_ForTestOnly) as CurrencySummaryForm;
				AssertEquals("Should be using the cached business object", summaryBizO.PK, summaryForm.BusinessEntity.Identifier);
				summaryForm.Close();
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestViewUnMatchGridTransaction()
		{
			SetupDataForTest();
			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm form = new NewMatchGroupForm(TestMatchingBase))
			{
				form.Show();
				form.ViewUnMatchGridTransaction_ForTestOnly(null, EventArgs.Empty);
			}
		}

		public void TestSaveFailureMakesFormReadonlyAfterCriticalValidationError()
		{
			OrgHeader orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			ARMatchingBase matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(matchingBase))
			{
				matchingForm.Show();

				matchingForm.HandleSelectAll_ForTestOnly();
				AssertEquals(2, matchingBase.MatchedTransactions.Count);

				matchingBase.RunPreSaveValidation();
				AssertNoErrors("Matching base should have no errors.", matchingBase);

				// It is not important for this test to throw Critical Validation exception. Code that is tested should raise Corrupted flag on any kind of exception that causes Save to fail.
				matchingBase.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.CannotSaveAfterError);
				try
				{
					Assert("MatchAndContinueButton should be visible and enabled.", matchingForm.MatchAndContinueButton.Visible & matchingForm.MatchAndContinueButton.Enabled);
					matchingForm.MatchAndContinueButton.PerformClick();
				}
				finally
				{
					matchingBase.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
					ExceptionReporterTestListener.Instance.Clear();
				}

				Assert("MatchingBase should be in corrupted state after concurrency error.", matchingBase.Corrupted);

				AssertEquals(ODisplayMode.ReadOnly, matchingForm.DisplayMode);

				Assert("MatchAndContinueButton should be hidden.", !matchingForm.MatchAndContinueButton.Visible);
				Assert("MatchAndCloseButton button should be hidden.", !matchingForm.MatchAndCloseButton.Visible);

				Assert("UnmatchedTransactionsGrid_ForTestOnly should be read only.", matchingForm.UnmatchedTransactionsGrid_ForTestOnly.ReadOnly);
				Assert("MatchTransactionsGrid_ForTestOnly should be read only.", matchingForm.MatchTransactionsGrid_ForTestOnly.ReadOnly);
			}
		}

		/// <summary>
		/// We cannot properly emulate two separate applications in tests because PersistentFactoryCacheManager is shared between all instances of BusinessObjectFactory.
		/// Calling Save method on one factory causes PersistentFactoryCacheManager to invalidate caches for all other factories.
		/// In this test it causes Critical Validation exception to happen before concurrency exception.
		/// To avoid Critical Validation exception, and to get to concurrency exception SuspendCriticalValidation attribute is used.
		/// </summary>
		public void TestSaveFailureMakesFormReadonlyAfterConcurrencyError()
		{
			OrgHeader orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			ARMatchingBase matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(matchingBase))
			{
				matchingForm.Show();

				matchingForm.HandleSelectAll_ForTestOnly();
				AssertEquals(2, matchingBase.MatchedTransactions.Count);

				// We use partial paying, because if transaction is fully paid, than we will have critical validation error instead of concurrency error.
				// That is the way to reproduce error in ediProd. In this test it is not necessary, because test has SuspendCriticalValidation attribute.
				IMatching invoice = matchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARInvoice);
				invoice.OSPartialPaymentAmount = 10;

				IMatching creditNote = matchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARCreditNote);
				creditNote.OSPartialPaymentAmount = -10;
				{
					var anotherFactory = new BusinessObjectFactory();
					anotherFactory.RefreshEnabled = false;
					var staff = anotherFactory.NewWithValidTestData<GlbStaff>();
					staff.GS_Code = "TST";

					using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					{
						ARMatchingBase anotherMatchingBase = new ARMatchingBase(anotherFactory);
						anotherMatchingBase.PrimaryOrganization = orgHeader.PK;

						using (NewMatchGroupForm anotherMatchingForm = new NewMatchGroupForm(anotherMatchingBase))
						{
							anotherMatchingForm.Show();

							anotherMatchingForm.HandleSelectAll_ForTestOnly();
							AssertEquals(2, anotherMatchingBase.MatchedTransactions.Count);

							// We use partial paying, because if transaction is fully paid, than we will have critical validation error instead of concurrency error.
							// That is the way to reproduce error in ediProd. In this test it is not necessary, because test has SuspendCriticalValidation attribute.
							IMatching anotherInvoice = anotherMatchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARInvoice);
							anotherInvoice.OSPartialPaymentAmount = 10;

							IMatching anotherCreditNote = anotherMatchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARCreditNote);
							anotherCreditNote.OSPartialPaymentAmount = -10;

							anotherMatchingBase.RunPreSaveValidation();
							AssertNoErrors("Matching base should have no errors.", anotherMatchingBase);

							Assert("MatchAndContinueButton should be visible and enabled.", anotherMatchingForm.MatchAndContinueButton.Visible & anotherMatchingForm.MatchAndContinueButton.Enabled);
							anotherMatchingForm.MatchAndContinueButton.PerformClick();

							Assert("MatchingBase should not be in corrupted state after matching by first user.", !anotherMatchingBase.Corrupted);
						}
					}
				}

				matchingBase.RunPreSaveValidation();
				AssertNoErrors("Matching base should have no errors.", matchingBase);

				Assert("MatchAndContinueButton should be visible and enabled.", matchingForm.MatchAndContinueButton.Visible & matchingForm.MatchAndContinueButton.Enabled);
				matchingForm.MatchAndContinueButton.PerformClick();

				Assert("MatchingBase should be in corrupted state after concurrency error.", matchingBase.Corrupted);

				AssertEquals(ODisplayMode.ReadOnly, matchingForm.DisplayMode);

				Assert("MatchAndContinueButton should be hidden.", !matchingForm.MatchAndContinueButton.Visible);
				Assert("MatchAndCloseButton button should be hidden.", !matchingForm.MatchAndCloseButton.Visible);

				Assert("UnmatchedTransactionsGrid_ForTestOnly should be read only.", matchingForm.UnmatchedTransactionsGrid_ForTestOnly.ReadOnly);
				Assert("MatchTransactionsGrid_ForTestOnly should be read only.", matchingForm.MatchTransactionsGrid_ForTestOnly.ReadOnly);
			}
		}

		public void TestViewMatchGridTransaction()
		{
			SetupDataForTest();
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestBank, TestCheques);
			TestMatchingBase = new APMatchingBase(Factory);

			using (NewMatchGroupForm form = new NewMatchGroupForm(TestMatchingBase))
			{
				TestMatchingBase.MatchedTransactions.Add(paymentApproval);
				form.Show();
				TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { APInv });
				((IMatching)APInv).OSPartialPaymentAmount = -50m;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.ViewMatchGridTransaction_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message for Payment", "Please use the 'Change Payment Amount' button to view the details for this payment.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.MatchTransactionsGrid_ForTestOnly.UnSelectAll();
				form.MatchTransactionsGrid_ForTestOnly.Select(2);
				form.ViewMatchGridTransaction_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message for Journals", "Please use the 'AR Journals' and 'AP Journals' tab to view the details for this journal.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMatchTransactionsGridContextMenu()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory, APPay);

			using (NewMatchGroupForm form = new NewMatchGroupForm(TestMatchingBase))
			{
				form.Show();
				form.MatchTransactionsGridMenu_Popup_ForTestOnly(null, EventArgs.Empty);

				Assert("Should contain Match Transaction Lines menu item", form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.Contains(form.PayLinesMenuItem_ForTestOnly));
				AssertEquals("Match Transaction Lines menu item index", 0, form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.IndexOf(form.PayLinesMenuItem_ForTestOnly));
				AssertEquals("Match Transaction Lines menu item shortcut", Shortcut.CtrlL, form.PayLinesMenuItem_ForTestOnly.Shortcut);
				Assert("Should contain Match Transaction Lines menu separator", form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.Contains(form.PayLinesMenuSeparator_ForTestOnly));
				AssertEquals("Match Transaction Lines menu separator index", 1, form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.IndexOf(form.PayLinesMenuSeparator_ForTestOnly));
			}
		}

		public void TestMatchTransactionLinesFunctionNotAvailableForPartiallyPaidTransactions()
		{
			SetupDataForTest();

			TestMatchingBase = new APMatchingBase(Factory);
			APInvoice invoice = (APInvoice)TestMatchingBase.MatchedTransactions.AddNew(typeof(APInvoice));
			invoice.AH_OSTotal = 10m;
			invoice.AH_GSTAmount = 5m;
			using (NewMatchGroupForm form = new NewMatchGroupForm(TestMatchingBase))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PayLines_ForTestOnly(null, null);
				AssertEquals(null, ZFormModaliser.LastFormShownForTest);
				ZString errorMessage = "You cannot match transaction lines on this transaction because the transaction has already been part paid without that previous part payment being matched at a line level.";
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestControllerID()
		{
			NewMatchGroupForm form;
			using (form = new NewMatchGroupForm(new APMatchingBase(Factory)))
			{
				AssertEquals(ControllerIDs.ZAPMatching, form.ControllerID);
			}
			using (form = new NewMatchGroupForm(new ARMatchingBase(Factory)))
			{
				AssertEquals(ControllerIDs.ZARMatching, form.ControllerID);
			}
			using (form = new NewMatchGroupForm(new APPaymentApprovalMatching(Factory, Factory.New<APPaymentApprovalWithoutAuthorisation>())))
			{
				AssertEquals(ControllerIDs.ZAPMatching, form.ControllerID);
			}
			using (form = new NewMatchGroupForm(new ARPaymentApprovalMatching(Factory, Factory.New<ARPaymentApprovalWithoutAuthorisation>())))
			{
				AssertEquals(ControllerIDs.ZARMatching, form.ControllerID);
			}
		}

		[TestDate(2017, 06, 27)]
		public void TestSimultaneousPaymentItemCreation()
		{
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			TestOrg = GetNewOrgHeader();
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestCheques = GetNewChequeBookForBank(TestBank);
			var checkBook = Factory.NewWithValidTestData<AccChequeBook>();
			checkBook.AK_AB = TestBank.PK;
			checkBook.AK_Code = "TestBook";
			Factory.Save();

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			apInvoice1.AH_OH = TestOrg.PK;
			apInvoice1.AH_InvoiceAmount = -90M;
			apInvoice1.AH_OutstandingAmount = -90M;

			Factory.Save();

			var newFactory2 = new BusinessObjectFactory();
			var payment1 = newFactory2.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 90M;
			payment1.AV_AB = TestBank.PK;
			var item1 = newFactory2.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = 90M;
			item1.A2_AH = apInvoice1.PK;
			item1.A2_AV = payment1.PK;

			newFactory2.Save();

			var newFactory3 = new BusinessObjectFactory();

			var apPayment1 = newFactory3.NewWithValidTestData<APPayment>();
			apPayment1.AH_OH = TestOrg.PK;
			apPayment1.AH_AB = TestBank.PK;
			apPayment1.ChequeBook = TestCheques.PK;
			apPayment1.AH_ChequeOrReference = "90";
			apPayment1.AH_OSExTaxAmount = 90M;

			var testMatchingBase2 = new APMatchingBase(newFactory3, apPayment1);

			using (var matchForm = new NewMatchGroupForm(testMatchingBase2))
			{
				matchForm.Show();
				testMatchingBase2.PrimaryOrganization = TestOrg.PK;
				testMatchingBase2.MoveFromUnmatchToMatch(new BusinessObject[] { apInvoice1 });
				((IMatching)apInvoice1).OSPartialPaymentAmount = -90M;

				matchForm.ValidateAndSave_ForTestOnly();
				AssertEquals(true, testMatchingBase2.HasErrors());
				Assert(testMatchingBase2.GetErrors().ContainsNotificationContaining("This transaction is fully paid by the following Unapproved payment(s):"));
				Assert(testMatchingBase2.GetErrors().ContainsNotificationContaining("Pay. Date   Bank Account   Check Book   Check/Reference   Amount"));
				Assert(testMatchingBase2.GetErrors().ContainsNotificationContaining("27-Jun-17  QFK7JIVCRB               N/A                N/A          90.00 AUD"));
			}
		}

		[TestDate(2017, 06, 27)]
		public void TestSimultaneousPartialPaymentItemCreation()
		{
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			TestOrg = GetNewOrgHeader();
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestCheques = GetNewChequeBookForBank(TestBank);
			var checkBook = Factory.NewWithValidTestData<AccChequeBook>();
			checkBook.AK_AB = TestBank.PK;
			checkBook.AK_Code = "TestBook";
			Factory.Save();

			var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			apInvoice2.AH_OH = TestOrg.PK;
			apInvoice2.AH_InvoiceAmount = -120M;
			apInvoice2.AH_OutstandingAmount = -120M;
			Factory.Save();

			var newFactory2 = new BusinessObjectFactory();

			var partialPayment1 = newFactory2.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			partialPayment1.AV_Amount = 10M;
			partialPayment1.AV_AB = TestBank.PK;
			var partialPaymentApprovalItem1 = newFactory2.NewWithValidTestData<PaymentApprovalItem>();
			partialPaymentApprovalItem1.A2_PaymentThisRun = 10M;
			partialPaymentApprovalItem1.A2_AH = apInvoice2.PK;
			partialPaymentApprovalItem1.A2_AV = partialPayment1.PK;
			newFactory2.Save();

			var newFactory3 = new BusinessObjectFactory();

			var partialPayment2 = newFactory3.NewWithValidTestData<APPayment>();
			partialPayment2.AH_OH = TestOrg.PK;
			partialPayment2.AH_AB = TestBank.PK;
			partialPayment2.ChequeBook = TestCheques.PK;
			partialPayment2.AH_ChequeOrReference = "25";
			partialPayment2.AH_OSExTaxAmount = 25M;

			var testMatchingBase3 = new APMatchingBase(newFactory3, partialPayment2);

			using (var matchForm = new NewMatchGroupForm(testMatchingBase3))
			{
				matchForm.Show();
				testMatchingBase3.PrimaryOrganization = TestOrg.PK;
				testMatchingBase3.MoveFromUnmatchToMatch(new BusinessObject[] { apInvoice2 });
				((IMatching)apInvoice2).OSPartialPaymentAmount = -25M;

				matchForm.ValidateAndSave_ForTestOnly();
				AssertEquals(true, testMatchingBase3.HasErrors());
			}
		}

		public void TestMatchStatusAndReasonColumns()
		{
			using (NewMatchGroupForm testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				AssertNotNull(testForm.MatchTransactionsGrid_ForTestOnly.GetColumnStyle("MatchStatus"));
				AssertNotNull(testForm.MatchTransactionsGrid_ForTestOnly.GetColumnStyle("MatchStatusReasonCode"));
			}
		}

		public void TestFilterWithTooManyParameters()
		{
			using (var form = (NewMatchGroupForm)GetFormToBashCore())
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
				form.Show();

				var chequeRefNumberFilter = (ModuleNumberFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.AllNumbers];
				chequeRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				var tooManyParameters = "QN,KL,TJ,9Z,XD,VI,A1,9E,IM,TI,5S,33,L0,EO,7N,JD,58,AI,ZK,3T,1V,HB,17,3K,X0,QZ,DW,UY,G6,VP,EF,W6,5E,VA,9Z,4I,N1,ED,E2,73,II,QL,LO,51,IT,3K,QH,30,YT,9P,88,TM,XU,SW,YD,B8,VN,6F,5Z,YO,7E,PV,CU,3P,UN,90,48,F0,F0,IG,S4,WM,0N,IU,9N,GP,0C,HJ,SX,PL,2D,1J,OG,AE,XX,RH,1M,3B,C5,GO,FA,NZ,DZ,Z7,DR,G6,X5,DM,Y7,PN,TD,FE,Y6,TE,8A,P3,35,FF,7Q,AV,PY,T8,TB,KN,U7,PD,5L,8N,NS,00,MQ,NA,2B,F8,DX,TS,63,ZA,VG,3P,X9,WO,7L,04,4D,09,DQ,I4,9A,MJ,X3,AG,1P,U0,LH,04,IR,1N,CJ,ST,BK,XC,H6,P3,DW,TZ,VP,4Q,W1,64,3J,59,FL,UI,TR,TA,QO,X1,FL,VM,9O,1S,81,A2,IA,80,XU,MB,6S,8E,5G,1B,LP,OA,NI,8W,OY,X0,PB,O2,CI,SP,YY,87,PN,OL,YD,JL,QZ,K5,ZL,HB,XU,4G,P8,4B";
				chequeRefNumberFilter.Property = tooManyParameters;
				chequeRefNumberFilter.IsActive = true;
				AssertNotNull(chequeRefNumberFilter);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => form.HandleFind_ForTestOnly());
				AssertEquals("The transaction search has too many parameters (2104), the search supports a maximum of 2100 parameters. Reduce the number of parameters and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAvailabilityOfWHTColumns()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());

			using (var testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				Assert(testForm.MatchTransactionsGrid_ForTestOnly, false);
				Assert(testForm.UnmatchedTransactionsGrid_ForTestOnly, false);
			}

			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);

			using (var testForm = (NewMatchGroupForm)GetFormToBashCore())
			{
				Assert(testForm.MatchTransactionsGrid_ForTestOnly, true);
				Assert(testForm.UnmatchedTransactionsGrid_ForTestOnly, true);
			}

			void Assert(ZGrid grid, bool expectedResult)
			{
				var notionalWHTTaxColumnExist = (grid.GetColumnStyle(nameof(IMatching.NotionalWHTTax)) != null);
				var realizedWHTTaxColumnExist = (grid.GetColumnStyle(nameof(IMatching.RealizedWHTTax)) != null);

				AssertEquals(expectedResult, notionalWHTTaxColumnExist);
				AssertEquals(expectedResult, realizedWHTTaxColumnExist);
			}
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
