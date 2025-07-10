using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class APInvoiceCreatorTest : BaseTransactionCreatorTest
	{
		#region Approval Request Authorization

		public void TestCreatedAndCanceledRequestFactoriesAreSavedWithMainFactory()
		{
			AssertEquals("Precondition: there are no requests in db", 0, Factory.GetDatabaseCount(typeof(APInvoiceChargesApprovalRequest)));

			var charge = CreateChargeAndSetupAuthorizationRegistry();
			charge.JR_APInvoiceNum = "INV2";
			var job = charge.InvoicingJob;

			var anotherInvoiceCharges = new APInvoiceCharges(TestObjectCreator.Creditor1.OH_Code, "INV2", job.PK, job.TablePrefix, null);
			anotherInvoiceCharges.Charges.Add(charge);
			var requestToBeCanceled = Factory.New<APInvoiceChargesApprovalRequest>();
			requestToBeCanceled.InitializeJobRelated(anotherInvoiceCharges, job.PK, job.TablePrefix);

			charge.JR_APInvoiceNum = "INV1";

			Factory.Save();

			ReleaseFactory();

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.OK);
			postGUIProviderMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);

			var jobInNewFactory = Factory.Load<Job>(job.PK);
			var creator = new APInvoiceCreator(jobInNewFactory, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				creator.CreateTransactions(transactions);
				Factory.Save();
			}
			AssertEquals("Postcondition: AP Invoice is not created", 0, transactions.GetAllAPTransactions().Length);
			AssertEquals("GetAllAPInvoiceApprovalRequests count", 1, transactions.GetAllAPInvoiceApprovalRequests().Length);
			var request = transactions.GetAllAPInvoiceApprovalRequests()[0];
			Assert("request.IsInDatabase", request.IsInDatabase);
			AssertEquals("request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNotEquals("All requests are created in separate Factories.", Factory._Instance, request.Factory._Instance);

			var otherRequests = Factory.Load<APInvoiceChargesApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, SQLComparisonOperator.NotEqual, request.PK));
			AssertEquals("otherRequests.Count", 1, otherRequests.Length);
			var cancelledRequest = otherRequests[0];
			Assert("cancelledRequest.IsInDatabase", cancelledRequest.IsInDatabase);
			AssertEquals("cancelledRequest ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, cancelledRequest.XP_ApprovalStatus);
			AssertEquals("requestToBeCanceled isCancelled", requestToBeCanceled.PK, cancelledRequest.PK);
		}

		public void TestDontShowPostingConfirmationFormForPreview()
		{
			var charge = CreateChargeAndSetupAuthorizationRegistry();
			var job = charge.InvoicingJob;
			Factory.Save();

			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(true);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.SecurityItemForTest).Returns(Env.Security.APInvoiceApproval.Code);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			postGUIProviderMock.Verify(m => m.RollbackPosting(), Times.Never);
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			creator.CreateTransactions(transactions);
			postGUIProviderMock.VerifyAll();
		}

		public void TestDontShowPostingConfirmationFormForBulkPosting()
		{
			var charge = CreateChargeAndSetupAuthorizationRegistry();
			var job = charge.InvoicingJob;
			Factory.Save();
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(true);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			creator.CreateTransactions(transactions);
			postGUIProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice created", 1, transactions.GetAllAPTransactions().Length);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Verify(m => m.ShouldApprovalRequestBeCreated, Times.Never);
			postGUIProviderMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.NotifyBulkPostingIsNotAuthorized(@"Creditor: ZCreditor1, Number: INV1, Ref. Number:S001, Amount:110.
You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.

-----------"));
			transactions = new TransactionCreatorHashtable();
			Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				creator.CreateTransactions(transactions);
			}
			postGUIProviderMock.VerifyAll();
			securityProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice is not created", 0, transactions.GetAllAPTransactions().Length);
		}

		public void TestShowPostingConfirmationFormForCancelledRequestsNotInThisPosting()
		{
			var charge = CreateChargeAndSetupAuthorizationRegistry();
			var job = charge.InvoicingJob;
			Factory.Save();
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(false);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			creator.CreateTransactions(transactions);
			postGUIProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice created", 1, transactions.GetAllAPTransactions().Length);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			invoiceCharges.Charges.Add(charge);
			request.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			charge.JR_APInvoiceNum += "_1";
			Factory.Save();

			AssertEquals("Precondition: request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			postGUIProviderMock.Reset();
			postGUIProviderMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.OK);
			transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			creator.CreateTransactions(transactions);
			postGUIProviderMock.VerifyAll();
			AssertEquals("Cancelled request is not included in requests created and so can be printed", 0, transactions.GetAllAPInvoiceApprovalRequests().Length);
			AssertEquals("Postcondition: AP Invoice created", 1, transactions.GetAllAPTransactions().Length);
			Factory.Save();
			Assert("Postcondition: charge posted", charge.IsCostPosted);
			AssertEquals("Postcondition:Request not in posting action is canceled.", Constants.GenApprovalRequestApprovalStatus.Cancelled, request.XP_ApprovalStatus);
		}

		public void TestShowPostingConfirmationForm()
		{
			var charge = CreateChargeAndSetupAuthorizationRegistry();
			var job = charge.InvoicingJob;
			Factory.Save();
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(false);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			var helper = new Mock<ITransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(true);
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(true);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			creator.CreateTransactions(transactions);
			postGUIProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice created", 1, transactions.GetAllAPTransactions().Length);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false);
			postGUIProviderMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			transactions = new TransactionCreatorHashtable();
			Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				creator.CreateTransactions(transactions);
			}
			postGUIProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice is not created", 0, transactions.GetAllAPTransactions().Length);

			postGUIProviderMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.Cancel);
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertExceptionThrown<InterruptPostingException>(() => creator.CreateTransactions(transactions));
			}
			postGUIProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice is not created", 0, transactions.GetAllAPTransactions().Length);
		}

		public void TestRequestToCompareAndCreditNoteCharge()
		{
			AsserRequestToCompareAndCreditNoteCharge(false);
		}

		public void TestRequestToCompareAndCreditNoteCharge_Preview()
		{
			AsserRequestToCompareAndCreditNoteCharge(true);
		}

		void AsserRequestToCompareAndCreditNoteCharge(bool isForPreviewOnly)
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider> { CallBase = true };
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(isForPreviewOnly);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request.RequisitionStatus = "TST";
			var requisitionDate = new ZDateTime(2016, 3, 14);
			request.RequisitionDate = requisitionDate;
			var invoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, job.PK, job.TablePrefix, postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(charge);
			request.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			Factory.Save();

			postGUIProviderMock.Setup(m => m.RequestToCompare).Returns(request);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();

			charge.JR_OSCostAmt *= -1;
			Factory.Save();
			var result = creator.CreateTransactions(transactions);
			Assert(!result);
			AssertEquals("No transactions is created.", 0, transactions.GetAllAPTransactions().Length);
			postGUIProviderMock.Verify();

			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.SecurityItemForTest).Returns(Env.Instance.Security.APInvoiceApproval.Code);
			charge.JR_OSCostAmt *= -1;
			Factory.Save();
			result = creator.CreateTransactions(transactions);
			Assert(result);
			AssertEquals("A transactions created.", 1, transactions.GetAllAPTransactions().Length);
			if (!isForPreviewOnly)
			{
				AssertEquals("TST", transactions.GetAllAPTransactions()[0].AH_RequisitionStatus);
				AssertEquals(requisitionDate, transactions.GetAllAPTransactions()[0].AH_RequisitionDate);
			}
			postGUIProviderMock.Verify();
		}

		Charge CreateChargeAndSetupAuthorizationRegistry()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			return TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
		}

		#endregion

		#region TEST: Create All Cost Invoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesForShipmentsHavingSubShipments()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001000");
			var shipment2 = TestObjectCreator.CreateShipmentWithCoLoadMaster("S00001001", "AUSYD", "NZAKL", shipment1);

			var job2 = TestObjectCreator.CreateJob(shipment2);
			var charge2 = TestObjectCreator.CreateCharge(job2, CC2, "desc2", USD, 100M, Creditor2, USD, 100M, LocalClient);
			SetAPInvoiceInfo(charge2, "125", Now.AddDays(10), Now.AddDays(20));

			var job1 = TestObjectCreator.CreateJob(shipment1, localClientOrg: LocalClient);
			var charge1 = TestObjectCreator.CreateCharge(job1, CC1, "desc1", USD, 100M, Creditor1, USD, 100M, LocalClient);
			SetAPInvoiceInfo(charge1, "123", Now.AddDays(10), Now.AddDays(20));

			Factory.Save();

			APInvoiceCreator creator = new APInvoiceCreator(job1);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);

			Assert("Charge posted.", charge1.IsCostPosted);
			Assert("Charge posted.", charge2.IsCostPosted);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnly()
		{
			SetupCharges();

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			APInvoiceCreator creator = new APInvoiceCreator(Job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;
				creator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", 5, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, Job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, Job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor2Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor2, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, Job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor2Inv1, Job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor3Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor3, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, Job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor3Inv1, Job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv2 is APInvoice);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, Job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv2, Job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, Charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, Charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyWithNoApprovalConfiguration()
		{
			SetupCharges();

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			APInvoiceCreator creator = new APInvoiceCreator(Job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			Env.Security.APUnapprovedInvoices.IsAllowed = false;
			creator.CreateTransactions(transactions);

			AssertEquals("Invoice Count", 5, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, Job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, Job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor2Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor2, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, Job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor2Inv1, Job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor3Inv1 is APInvoice);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor3, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, Job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor3Inv1, Job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv2 is APInvoice);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, Job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv2, Job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, Charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, Charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		[TestDate(2021, 2, 4)]
		public void TestPostCostWhenChargeHasZeroLocalCostAmount()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "charge", TestObjectCreator.USD, 100M);
			using (charge.Calculations.SuspendCalculations())
			{
				job.ExchangeRates.RemoveAndDeleteAll();
				charge.JR_LocalCostAmt = 0m;
			}
			Factory.Save();

			AssertEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
			AssertNotEquals("Pre-condition", 0m, charge.JR_OSCostAmt);

			ReleaseFactory();
			job = Factory.Load<Job>(job.PK);
			APInvoiceCreator creator = new APInvoiceCreator(job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Invoice Count", 0, transactions.APTransactionsCount);
		}

		[TestDate(2021, 2, 4)]
		public void TestPostCostWhenChargeHasZeroOSCostAmount()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "charge", TestObjectCreator.USD, 100M);
			using (charge.Calculations.SuspendCalculations())
			{
				job.ExchangeRates.RemoveAndDeleteAll();
				charge.JR_OSCostAmt = 0m;
			}
			Factory.Save();

			AssertNotEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
			AssertEquals("Pre-condition", 0m, charge.JR_OSCostAmt);

			ReleaseFactory();
			job = Factory.Load<Job>(job.PK);
			APInvoiceCreator creator = new APInvoiceCreator(job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Invoice Count", 0, transactions.APTransactionsCount);
		}

		#region TestCreateInvoicesCostsOnlyAsUAInvoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoices()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(false);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsAsRequestsWhenChargeApprovalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(true);
		}

		void AssertTestCreateInvoicesCostsOnlyAsUAInvoices(bool isChargeApprovalActivated)
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupCharges();
			Charge3.JR_OSCostAmt = -Charge3.JR_OSCostAmt;

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(Job.PK, (ZString)Job.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.OK);
			}
			APInvoiceCreator creator = new APInvoiceCreator(Job, guiWrapperMock.Object);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			SetUpRegistryForTest();

			Factory.Save();

			try
			{
				if (isChargeApprovalActivated)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				else
				{
					Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				}
				creator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			Factory.Save();

			AssertEquals("Invoice Count (Three UAInvoices)", isChargeApprovalActivated ? 2 : 5, transactions.Count);
			AssertEquals("AP Transactions Count", 2, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 3 : 0, requests.Length);

			#region Creditor 1 Invoice 1

			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1");
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC7.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
				Assert("Should be correct type of invoice", creditor1Inv1.AH_TransactionType == (isChargeApprovalActivated ? TransactionTypes.Invoice : TransactionTypes.UAInvoice));

				TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(isChargeApprovalActivated ? TransactionLineTypes.Cost : TransactionLineTypes.UnapprovedCost, CC1, Job.PK);
				AssertNotNull("Should be correct type of line", cC1Line);

				TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(isChargeApprovalActivated ? TransactionLineTypes.Cost : TransactionLineTypes.UnapprovedCost, CC7, Job.PK);
				AssertNotNull("Should be correct type of line", cC7Line);
			}

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor2Inv1.AH_TransactionType == TransactionTypes.Invoice && creditor2Inv1.AH_Ledger == LedgerTypes.AccountsPayable);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC2, Job.PK);
			AssertNotNull("Should be APInvoiceLine", cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor3Inv1.AH_TransactionType == TransactionTypes.Invoice && creditor3Inv1.AH_Ledger == LedgerTypes.AccountsPayable);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC3, Job.PK);
			AssertNotNull("Should be APInvoiceLine", cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "2");
				AssertEquals("Request lines", 1, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC5.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
				AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
				Assert("Should be correct type of invoice", creditor1Inv2.AH_TransactionType == (isChargeApprovalActivated ? TransactionTypes.Invoice : TransactionTypes.UAInvoice));

				TransactionLine cC5Line = creditor1Inv2.FindTransactionLine(isChargeApprovalActivated ? TransactionLineTypes.Cost : TransactionLineTypes.UnapprovedCost, CC5, Job.PK);
				AssertNotNull("Should be correct type of line", cC5Line);
			}

			#endregion

			#region Creditor 3 Invoice 2

			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor3.OH_Code && x.PostingDetails.TransactionNumber == "2");
				AssertEquals("Request lines", 1, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC8.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
				AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
				Assert("Should be correct type of invoice", creditor1Inv2.AH_TransactionType == (isChargeApprovalActivated ? TransactionTypes.Invoice : TransactionTypes.UAInvoice));

				TransactionLine cC8Line = creditor1Inv2.FindTransactionLine(isChargeApprovalActivated ? TransactionLineTypes.Cost : TransactionLineTypes.UnapprovedCost, CC8, Job.PK);
				AssertNotNull("Should be correct type of line", cC8Line);
			}

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", !isChargeApprovalActivated, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 hasn't Accrual", true, Charge3Accrual == null);
			AssertEquals("Charge 5 Accrual Reversed", !isChargeApprovalActivated, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", !isChargeApprovalActivated, Charge7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 8 Accrual Reversed", !isChargeApprovalActivated, Charge8Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TestCreateInvoicesCostsOnlyAsUAInvoicesWhenOnlySecondLineExceedApprovedLimit

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoicesWhenOnlySecondLineExceedApprovedLimit()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoicesWhenOnlySecondLineExceedApprovedLimit(false);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsRequestsWhenOnlySecondLineExceedApprovedLimit_WhenChargeApporvalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoicesWhenOnlySecondLineExceedApprovedLimit(true);
		}

		void AssertTestCreateInvoicesCostsOnlyAsUAInvoicesWhenOnlySecondLineExceedApprovedLimit(bool isChargeApprovalActivated)
		{
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(Job, GBP, .4M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 300M, Creditor1, AUD, 150M, LocalClient);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(Charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(Job.PK, (ZString)Job.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.OK);
			}
			APInvoiceCreator creator = new APInvoiceCreator(Job, guiWrapperMock.Object);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			SetUpRegistryForTest();
			try
			{
				if (isChargeApprovalActivated)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				else
				{
					Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				}
				creator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", isChargeApprovalActivated ? 0 : 1, transactions.Count);
			AssertEquals("AP Transactions Count after finalizing the operation", 0, transactions.APTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 1 : 0, requests.Length);
			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1");
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC7.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
				AssertEquals("Should be correct type of line", TransactionTypes.UAInvoice, creditor1Inv1.AH_TransactionType);

				TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, Job.PK);
				AssertNotNull("Should be correct type of line", cC1Line);

				TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC7, Job.PK);
				AssertNotNull("Should be correct type of line", cC7Line);
			}
		}

		#endregion

		[TestDate(2008, 8, 15)]
		public void TestCreateInvoicesForSelfBilledCreditors()
		{
			Job job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Creditor1.CompanyData.OB_APCostsSelfBilled = true;
			Creditor1.Factory.Save();

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge3, "2", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			APInvoiceCreator creator = new APInvoiceCreator(job);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			Factory.Save();

			AssertEquals("Invoice Count", 3, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			Assert("Charge 1 ap invoice number should be set to SBXXXXXXXX", charge1.JR_APInvoiceNum.StartsWith("SB"));
			Assert("Charge 5 ap invoice number should be set to SBXXXXXXXX", charge1.JR_APInvoiceNum.StartsWith("SB"));
			Assert("Charge 7 ap invoice number should be set to SBXXXXXXXX", charge1.JR_APInvoiceNum.StartsWith("SB"));
			AssertEquals(charge2.JR_APInvoiceNum, "1");
			AssertEquals(charge3.JR_APInvoiceNum, "2");

			AssertEquals("Charge 1 ap invoice date should be set", ZDateTime.Now, charge1.JR_APInvoiceDate);
			AssertEquals("Charge 5 ap invoice date should be set", ZDateTime.Now, charge5.JR_APInvoiceDate);
			AssertEquals("Charge 7 ap invoice date should be set", ZDateTime.Now, charge7.JR_APInvoiceDate);

			AssertEquals("Charge 1 ap payment date should be set", ZDateTime.Now, charge1.JR_PaymentDate);
			AssertEquals("Charge 5 ap payment date should be set", ZDateTime.Now, charge5.JR_PaymentDate);
			AssertEquals("Charge 7 ap payment date should be set", ZDateTime.Now, charge7.JR_PaymentDate);
		}

		public void TestInvoiceHeaderBranch_SetTransactionHeaderBranch()
		{
			var testBranch = TestObjectCreator.CreateBranch("TS2", GlbCompany.CurrentCompany);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job.JH_GB = TestObjectCreator.CreateBranch("TS1", GlbCompany.CurrentCompany).PK;

			var charge = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, testBranch.PK);
			SetAPInvoiceInfo(charge, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();

			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			var invoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			invoiceCharges.Charges.Add(charge);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);

			//Post to login branch registry - turned on
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertAPInvoiceCreatorSetTransactionHeaderBranch("Post to login company registry turned on", invoice, GlbBranch.CurrentBranch.PK);

			//Post to login branch registry - turned off
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertAPInvoiceCreatorSetTransactionHeaderBranch("Post to login company registry turned off", invoice, job.JH_GB);

			void AssertAPInvoiceCreatorSetTransactionHeaderBranch(string expectedMessage, InvoicingBase invoicingBase, ZGuid expectedBranch)
			{
				mockIAccountingDependencyFactory.Reset();
				branchLevelPostingHelperMock.Reset();

				var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
				mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);
				branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<InvoicingBase>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
					.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
						(transaction, invoiceLevel, charges) =>
						{
							initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						}
					);

				APInvoiceCreator.SetInvoiceHeadeValues(invoice, invoiceCharges);

				AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranch, initialBranchPKValueInPassedTransaction);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<InvoicingBase>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Once);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, invoiceCharges));
			}
		}

		[TestDate(2017, 01, 05)]
		public void TestInvoiceHeaderBranch_DefaultPostingBehaviour()
		{
			//with default registry values
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			Job job;
			Charge charge;
			SetupForInvoiceBranchTest(out job, out charge);
			var apInvoiceCreator = new APInvoiceCreator(job);

			var transactions = new TransactionCreatorHashtable();
			apInvoiceCreator.CreateTransactions(transactions);

			var apInvoices = transactions.GetAllAPTransactions();
			AssertEquals("1 AP invoice created", 1, apInvoices.Length);

			AssertEquals("Invoice Branch is set from Job branch", job.JH_GB, apInvoices[0].AH_GB);
		}

		[TestDate(2017, 01, 05)]
		public void TestInvoiceHeaderBranch_BranchLevelPostingEnabled()
		{
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			//branch level posting enabled
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			Job job;
			Charge charge;
			SetupForInvoiceBranchTest(out job, out charge);
			var apInvoiceCreator = new APInvoiceCreator(job);

			var transactions = new TransactionCreatorHashtable();
			apInvoiceCreator.CreateTransactions(transactions);

			var apInvoices = transactions.GetAllAPTransactions();
			AssertEquals("1 AP invoice created", 1, apInvoices.Length);

			AssertEquals("Invoice Branch is set from charge line branch", charge.JR_GB, apInvoices[0].AH_GB);
		}

		[TestDate(2017, 01, 05)]
		public void TestInvoiceHeaderBranch_PostToLoginBranchEnabled()
		{
			//Post to login branch enabled
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			Job job;
			Charge charge;
			SetupForInvoiceBranchTest(out job, out charge);
			var apInvoiceCreator = new APInvoiceCreator(job);

			var transactions = new TransactionCreatorHashtable();
			apInvoiceCreator.CreateTransactions(transactions);

			var apInvoices = transactions.GetAllAPTransactions();
			AssertEquals("1 AP invoice created", 1, apInvoices.Length);

			AssertEquals("Invoice Branch is set to login branch", GlbBranch.CurrentBranch.PK, apInvoices[0].AH_GB);
		}

		[TestDate(2017, 01, 05)]
		public void TestInvoiceHeaderBranch_BranchLevelPosting_PostToLoginBranch_Enabled()
		{
			//Both the registries are enabled
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			Job job;
			Charge charge;
			SetupForInvoiceBranchTest(out job, out charge);
			var apInvoiceCreator = new APInvoiceCreator(job);

			var transactions = new TransactionCreatorHashtable();
			apInvoiceCreator.CreateTransactions(transactions);

			var apInvoices = transactions.GetAllAPTransactions();
			AssertEquals("1 AP invoice created", 1, apInvoices.Length);

			AssertEquals("Invoice Branch is set from charge line branch", charge.JR_GB, apInvoices[0].AH_GB);
		}

		void SetupForInvoiceBranchTest(out Job job, out Charge charge)
		{
			var branch = TestObjectCreator.CreateBranch("TS1", GlbCompany.CurrentCompany);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001");
			job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;

			charge = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, branch.PK);

			SetAPInvoiceInfo(charge, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();
		}

		public void TestSetInvoiceHeadeValues_AH_GB_TaxBranch()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			Factory.Save();

			AssertSetInvoiceHeadeValues_AH_GB_TaxBranch(true);
			AssertSetInvoiceHeadeValues_AH_GB_TaxBranch(false);

			void AssertSetInvoiceHeadeValues_AH_GB_TaxBranch(bool enableTaxBranchReporting)
			{
				var charge1 = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
				var charge2 = CreateCharge(job, TestObjectCreator.CC2, "Charge Code 2", AUD, 200M, TestObjectCreator.Creditor1, AUD, 250M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);

				SetAPInvoiceInfo(charge1, "S001-1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
				charge1.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				SetAPInvoiceInfo(charge2, "S001-1", charge1.JR_APInvoiceDate, charge1.JR_PaymentDate);
				charge2.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var apInvoiceCreator = new APInvoiceCreator(job);
					var transactions = new TransactionCreatorHashtable();

					apInvoiceCreator.CreateTransactions(transactions);
					var apInvoices = transactions.GetAllAPTransactions();

					AssertEquals("Count of AP invoice created", 1, apInvoices.Length);
					AssertNotNull("InvoiceNumber", transactions.RetrieveAPInvoice(TestObjectCreator.Creditor1, "S001-1"));
					var invoice = apInvoices.OfType<InvoicingBase>().First();

					AssertEquals(enableTaxBranchReporting ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, invoice.AH_GB_TaxBranch);
					AssertEquals("Two lines in Invoice", 2, invoice.Lines.Count);
					Assert(invoice.Lines.Cast<InvoicingLineBase>().All(x => x.AL_GB_TaxBranch == (enableTaxBranchReporting ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty)));
				}
			}
		}

		public void TestInvoiceHeaderPlaceOfSupply_PlaceOfSupplyPopulatedToInvoiceOnPosting()
		{
			Assert("Pre-condition: Australia does not support Place Of Supply", !PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));

			AssertAPInvoicesCreatedWithPlaceOfSupply("S001001", placeOfSUpplyOnHeader: false);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				Assert("India does support Place Of Supply", PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
				Assert("Pre-condtion: Posting at Fixed Place of Supply is enforced for India", AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value);

				AssertAPInvoicesCreatedWithPlaceOfSupply("S001002", placeOfSUpplyOnHeader: true);

				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertAPInvoicesCreatedWithPlaceOfSupply("S001003", placeOfSUpplyOnHeader: false);
				}
			}
		}

		void AssertAPInvoicesCreatedWithPlaceOfSupply(string jobNumber, bool placeOfSUpplyOnHeader)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var charge1 = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
			var charge2 = CreateCharge(job, TestObjectCreator.CC2, "Charge Code 2", AUD, 200M, TestObjectCreator.Creditor1, AUD, 250M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);

			// Set the same Invoice details and the same Place of Supply as Validation would not allow us to save with different places of Supply 
			SetAPInvoiceInfo(charge1, jobNumber + "-1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			charge1.JR_CostPlaceOfSupply = "DL";
			SetAPInvoiceInfo(charge2, jobNumber + "-1", charge1.JR_APInvoiceDate, charge1.JR_PaymentDate);
			charge2.JR_CostPlaceOfSupply = "DL";

			Factory.Save();

			var apInvoiceCreator = new APInvoiceCreator(job);
			var transactions = new TransactionCreatorHashtable();

			apInvoiceCreator.CreateTransactions(transactions);
			var apInvoices = transactions.GetAllAPTransactions();

			AssertEquals("Count of AP invoice created", 1, apInvoices.Length);
			AssertNotNull("InvoiceNumber", transactions.RetrieveAPInvoice(TestObjectCreator.Creditor1, jobNumber + "-1"));
			var invoice = apInvoices.OfType<InvoicingBase>().First();

			AssertEquals("AH_PlaceOfSupply", placeOfSUpplyOnHeader ? "DL" : "", invoice.AH_PlaceOfSupply);
			AssertEquals("Two lines in Invoice", 2, invoice.Lines.Count);

			Assert($"Both Lines with 'DL' Place of Supply", invoice.Lines.Cast<InvoicingLineBase>().All(x => x.AL_PlaceOfSupply == "DL"));

			AssertEquals("JR_CostPlaceOfSupply still has its original value", "DL", charge1.JR_CostPlaceOfSupply);
			AssertEquals("JR_CostPlaceOfSupply still has its original value", "DL", charge2.JR_CostPlaceOfSupply);
		}

		public void TestInvoiceHeaderPlaceOfSupply_SelfBilledCostOnJob_PlaceOfSupplyLevelPosting()
		{
			Assert("Pre-condition: Australia does not support Place Of Supply", !PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
			TestObjectCreator.Creditor1.CompanyData.OB_APCostsSelfBilled = true;    // To make charges cost postable without setting the AP Invoice details 

			AssertAPInvoicesCreatedByPlaceOfSupply("S001001", 1);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				Assert("India does support Place Of Supply", PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
				Assert("Pre-condtion: Posting at Fixed Place of Supply is enforced for India", AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value);

				AssertAPInvoicesCreatedByPlaceOfSupply("S001002", 2);

				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertAPInvoicesCreatedByPlaceOfSupply("S001003", 1);
				}
			}
		}

		void AssertAPInvoicesCreatedByPlaceOfSupply(string jobNumber, int expectedCount)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var charge1 = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
			charge1.JR_CostPlaceOfSupply = "JH";
			var charge2 = CreateCharge(job, TestObjectCreator.CC2, "Charge Code 2", AUD, 200M, TestObjectCreator.Creditor1, AUD, 250M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
			charge2.JR_CostPlaceOfSupply = "DL";

			Factory.Save();

			var apInvoiceCreator = new APInvoiceCreator(job);
			var transactions = new TransactionCreatorHashtable();

			apInvoiceCreator.CreateTransactions(transactions);
			var apInvoices = transactions.GetAllAPTransactions();

			AssertEquals("Count of AP invoice created", expectedCount, apInvoices.Length);
			if (expectedCount == 1)
			{
				AssertNotNull("InvoiceNumber", transactions.RetrieveAPInvoice(TestObjectCreator.Creditor1, "###" + jobNumber));
				var invoice = apInvoices.OfType<InvoicingBase>().First();

				Assert("AH_PlaceOfSupply expected empty", invoice.AH_PlaceOfSupply.IsEmpty);
				AssertEquals("Two lines in Invoice", 2, invoice.Lines.Count);
				AssertNotNull("Line with 'JH' Place of Supply", invoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_PlaceOfSupply == "JH"));
				AssertNotNull("Line with 'DL' Place of Supply", invoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_PlaceOfSupply == "DL"));
			}
			else
			{
				AssertNotNull("InvoiceNumber for JH", transactions.RetrieveAPInvoice(TestObjectCreator.Creditor1, "###" + jobNumber + "/JH"));
				AssertNotNull("InvoiceNumber for DL", transactions.RetrieveAPInvoice(TestObjectCreator.Creditor1, "###" + jobNumber + "/DL"));

				var jhPlaceOfSupplyInvoice = apInvoices.OfType<InvoicingBase>().FirstOrDefault(x => x.AH_PlaceOfSupply == "JH");
				var dlPlaceOfSupplyInvoice = apInvoices.OfType<InvoicingBase>().FirstOrDefault(x => x.AH_PlaceOfSupply == "DL");

				AssertNotNull("Expect AP Invoice with 'JH' Place of Supply", jhPlaceOfSupplyInvoice);
				AssertEquals("One line in Invoice", 1, jhPlaceOfSupplyInvoice.Lines.Count);
				AssertEquals("AL_PlaceOfSupply", "JH", jhPlaceOfSupplyInvoice.Lines[0].AL_PlaceOfSupply);

				AssertNotNull("Expect AP Invoice with 'DL' Place of Supply", dlPlaceOfSupplyInvoice);
				AssertEquals("One line in Invoice", 1, dlPlaceOfSupplyInvoice.Lines.Count);
				AssertEquals("AL_PlaceOfSupply", "DL", dlPlaceOfSupplyInvoice.Lines[0].AL_PlaceOfSupply);
			}

			AssertEquals("JR_CostPlaceOfSupply still has its original value", "JH", charge1.JR_CostPlaceOfSupply);
			AssertEquals("JR_CostPlaceOfSupply still has its original value", "DL", charge2.JR_CostPlaceOfSupply);
		}

		#endregion

		#region TEST: Create All Cost Invoices With An Apportionment

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyWithAnApportionment()
		{
			SetupCharges();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();

			Charge8.JR_E6 = cost.PK;
			cost.E6_OSCostAmount = 300m;
			cost.E6_LocalCostAmount = 300m;

			var shipment = TestObjectCreator.CreateShipment("C00001000");
			Charge8.InvoicingJob.JH_ParentID = shipment.PK;
			Charge8.InvoicingJob.JH_ParentTableCode = "JS";

			SetupAPInvoiceInfo();
			cost.E6_InvoiceDate = Charge8.JR_APInvoiceDate;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_AC_ChargeCode = CC8.PK;
			cost.E6_PaymentDate = Charge8.JR_PaymentDate;
			cost.E6_InvoiceNum = "2";

			Factory.Save();

			InitializeWIPAccruals();

			APInvoiceCreator creator = new APInvoiceCreator(Job);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertEquals("Invoice Count", 4, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, Job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, Job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv1, Job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor2, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, Job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor2Inv1, Job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, ZDateTime.Now, ZBool.False, Creditor3, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, Job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor3Inv1, Job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "C00001000", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, ZDateTime.Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, Job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, ZDateTime.Now, ZDateTime.Now,
				ZBool.False, creditor1Inv2, Job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 8 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, Charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, Charge7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 8 Accrual Reversed", false, Charge8Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Comment Charge Code not to be Included in the AP Invoice

		public void TestCommentChargeCodePosting()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, TestObjectCreator.CommentChargeCode, "Comment Charge 1", AUD, 0M, Creditor1, AUD, 0M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			APInvoiceCreator creator = new APInvoiceCreator(job);
			creator.CreateTransactions(transactions);

			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");

			AssertNotNull(creditor1Inv1);
			AssertEquals(1, creditor1Inv1.Lines.Count);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job.PK);
			AssertNotNull(cC1Line);
			AssertEquals(-100M, cC1Line.AL_LineAmount);

			TransactionLine commentChargeLine = creditor1Inv1.FindTransactionLine("CST", TestObjectCreator.CommentChargeCode, job.PK);
			AssertNull(commentChargeLine);
		}

		public void TestCommentChargeCodeOnlyJobInvoice()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge2 = CreateCharge(job, TestObjectCreator.CommentChargeCode, "Comment Charge 1", AUD, 0M, Creditor1, AUD, 0M, LocalClient);

			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			APInvoiceCreator creator = new APInvoiceCreator(job);
			creator.CreateTransactions(transactions);

			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("No other transactions", 0, transactions.Count);
			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");

			AssertNull(creditor1Inv1);
		}

		#endregion

		public void TestApprovedEventAddedOnApprovingInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);

			var request1 = Factory.New<APInvoiceChargesApprovalRequest>();
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request1.RequisitionStatus = "TST";
			request1.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			request1.XP_GS_NKApprovingUser1 = staff.GS_Code;
			var requisitionDate = new ZDateTime(2016, 3, 14);
			request1.RequisitionDate = requisitionDate;

			var invoiceCharges = new APInvoiceCharges(charge1.CostAccount.OH_Code, charge1.JR_APInvoiceNum, job.PK, job.TablePrefix, postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(charge1);
			request1.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			Factory.Save();

			postGUIProviderMock.Setup(m => m.RequestToCompare).Returns(request1);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			Factory.Save();

			var result = creator.CreateTransactions(transactions);
			Factory.Save();
			Assert("Precondition: CreateTransactions result", result);
			AssertEquals("Precondition: Transactions are created.", 1, transactions.GetAllAPTransactions().Length);

			var invoice = transactions.GetAllAPTransactions()[0];
			var invoiceApprovedLog = invoice.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNotNull("Invoice approved log should be added", invoiceApprovedLog);
			AssertEquals("Approving Reference", "AP|INV|Approved and Posted", invoiceApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", new ZDateTime(2016, 3, 15), invoiceApprovedLog.SL_EventTime);
			AssertEquals("Approving user", "ABC", invoiceApprovedLog.SL_GS_NKUser);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV2", null, 0, null);

			var request2 = Factory.New<APInvoiceChargesApprovalRequest>();
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request2.RequisitionStatus = "TST";
			request2.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			request2.XP_GS_NKApprovingUser1 = staff.GS_Code;
			request2.RequisitionDate = requisitionDate;

			invoiceCharges = new APInvoiceCharges(charge2.CostAccount.OH_Code, charge2.JR_APInvoiceNum, job.PK, job.TablePrefix, postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(charge2);
			request2.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			Factory.Save();

			postGUIProviderMock.Setup(m => m.RequestToCompare).Returns(request2);
			creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			transactions = new TransactionCreatorHashtable();
			Factory.Save();

			result = creator.CreateTransactions(transactions);
			Factory.Save();
			Assert("Precondition: CreateTransactions result", result);
			AssertEquals("Precondition: Transactions are created.", 1, transactions.GetAllAPTransactions().Length);

			invoice = transactions.GetAllAPTransactions()[0];
			invoiceApprovedLog = invoice.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNull("No approved log should be added as EnableAPInvoiceApproval registry is set to false", invoiceApprovedLog);
		}

		[TestDate(2016, 11, 09)]
		public void TestApprovedEventAddedForInPlaceInvoiceApproval()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			var securityoverride = new SecurityCore(null, staff, Guid.Empty, Guid.Empty, Guid.Empty);
			securityoverride.APInvoiceApproval.IsAllowed = true;
			Factory.Save();

			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			var invoiceCharges = new APInvoiceCharges(charge1.CostAccount.OH_Code, charge1.JR_APInvoiceNum, job.PK, job.TablePrefix, postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(charge1);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.UserSecurityOverride).Returns(securityoverride);
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(false);
			postGUIProviderMock.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			postGUIProviderMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			postGUIProviderMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), JobHeaderSchema.Constants.Prefix));
			postGUIProviderMock.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.IsLevelAuthorizationRequired_ForTestOnly = x => true;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => true;

			var creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			Factory.Save();

			var result = creator.CreateTransactions(transactions);
			Factory.Save();
			Assert("Precondition: CreateTransactions result", result);
			AssertEquals("Precondition: Transactions are created.", 1, transactions.GetAllAPTransactions().Length);

			var invoice = transactions.GetAllAPTransactions()[0];
			var invoiceApprovedLog = invoice.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNotNull("Invoice approved log should be added", invoiceApprovedLog);
			AssertEquals("Approving Reference", "AP|INV|Approved and Posted", invoiceApprovedLog.SL_Reference);
			AssertEquals("Approving Date", ZDateTime.Today.Date, invoiceApprovedLog.SL_EventTime.Date);
			AssertEquals("Approving user", "ABC", invoiceApprovedLog.SL_GS_NKUser);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV2", null, 0, null);
			invoiceCharges = new APInvoiceCharges(charge2.CostAccount.OH_Code, charge2.JR_APInvoiceNum, job.PK, job.TablePrefix, postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(charge2);

			creator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			transactions = new TransactionCreatorHashtable();
			Factory.Save();

			result = creator.CreateTransactions(transactions);
			Factory.Save();
			Assert("Precondition: CreateTransactions result", result);
			AssertEquals("Precondition: Transactions are created.", 1, transactions.GetAllAPTransactions().Length);

			invoice = transactions.GetAllAPTransactions()[0];
			invoiceApprovedLog = invoice.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNull("No approved log should be added as EnableAPInvoiceApproval registry is set to false", invoiceApprovedLog);
		}

		public void TestChangeJob()
		{
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			Charge charge1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Job job2 = CreateJob("Z00002000", LocalClient, 5M, Agent, 10M);
			Charge charge2 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			SetAPInvoiceInfo(charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var creator = new APInvoiceCreator(job1);
			creator.CreateTransactions(transactions);
			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			APInvoice invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertNotNull("Invoice for Creditor1 is created", invoice);
			AssertEquals("invoice.Lines.Count", 1, invoice.Lines.Count);
			TransactionLine line = invoice.FindTransactionLine("CST", CC1, job1.PK);
			AssertNotNull("Invoice line for job1 is created.", line);
			AssertEquals("line.AL_LineAmount", -100M, line.AL_LineAmount);

			creator.ChangeJob(job2);
			transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			invoice = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertNotNull("Invoice for Creditor2 is created", invoice);
			AssertEquals("invoice.Lines.Count", 1, invoice.Lines.Count);
			line = invoice.FindTransactionLine("CST", CC1, job2.PK);
			AssertNotNull("Invoice line for job2 is created.", line);
			AssertEquals("line.AL_LineAmount", -200M, line.AL_LineAmount);
		}

		#region TestMultiJobOperation

		public void TestMultiJobOperation()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestMultiJobOperation(false);
		}

		public void TestMultiJobOperation_WhenChargeApporvalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestMultiJobOperation(true);
		}

		void AssertTestMultiJobOperation(bool isChargeApprovalActivated)
		{
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 150M, LocalClient);
			var job2 = CreateJob("C00001001", LocalClient, 5M, Agent, 10M);
			var charge2 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 150M, LocalClient);
			SetAPInvoiceInfo(Charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			SetUpRegistryForTest();
			if (isChargeApprovalActivated)
			{
				Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
			}
			else
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			}

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_OSCostAmount = 400m;
			Charge1.JR_E6 = cost.PK;
			charge2.JR_E6 = cost.PK;

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, (ZString)consol.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns(ZDialogResult.OK);
			}

			var creator = new APInvoiceCreator(Job, consol, false, new JobConsolCostCollection(Factory, consol), true, guiWrapperMock.Object);
			var transactions = new TransactionCreatorHashtable();

			using (creator.InitializeMultiJobOperation(transactions))
			{
				creator.ChangeJob(Job);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);

				creator.ChangeJob(job2);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);
			}
			AssertEquals("Invoice Count after finalizing the operation", isChargeApprovalActivated ? 0 : 1, transactions.Count);
			AssertEquals("AP Transactions Count after finalizing the operation", 0, transactions.APTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 1 : 0, requests.Length);
			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1");
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == job2.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, invoice.AH_Ledger);
				AssertEquals("Should be correct type of line", TransactionTypes.UAInvoice, invoice.AH_TransactionType);
				var line = invoice.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, Job.PK);
				AssertNotNull("Should be correct type of line", line);
				line = invoice.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, job2.PK);
				AssertNotNull("Should be correct type of line", line);
			}
		}

		public void TestMultiJobOperationCreateInvoicesPerJob()
		{
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 150M, LocalClient);
			var job2 = CreateJob("C00001001", LocalClient, 5M, Agent, 10M);
			var charge2 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 150M, LocalClient);
			SetAPInvoiceInfo(Charge1, "INV123", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge2, "INV123", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			var creator = new APInvoiceCreator(Job);
			var transactions = new TransactionCreatorHashtable();

			var operationDisposable = creator.InitializeMultiJobOperation(transactions);

			creator.ChangeJob(Job);
			creator.CreateTransactionsForMultiJobOperation();
			AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);

			creator.ChangeJob(job2);
			creator.CreateTransactionsForMultiJobOperation();
			AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);

			try
			{
				operationDisposable.Dispose();
				Fail("Exception is expected");
			}
			catch (DuplicatedInvoiceException e)
			{
				AssertEquals("ZCreditor1", e.Creditor);
				AssertEquals("INV123", e.InvoiceNumber);
			}
		}

		public void TestMultiJobOperation_HandlesConsolCostOnly()
		{
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 150M, LocalClient);
			SetAPInvoiceInfo(Charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			SetUpRegistryForTest();
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			var transactions = new TransactionCreatorHashtable();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_OSCostAmount = 200m;
			Charge1.JR_E6 = cost.PK;
			var creator = new APInvoiceCreator(Job, consol, false, new JobConsolCostCollection(Factory, consol), true);
			using (creator.InitializeMultiJobOperation(transactions))
			{
				creator.ChangeJob(Job);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);
			}
			AssertEquals("Invoice Count", 1, transactions.Count);
			AssertEquals("AP Transactions Count", 1, transactions.APTransactionsCount);

			creator = new APInvoiceCreator(Job, consol, false, new JobConsolCostCollection(Factory, consol), false);
			transactions = new TransactionCreatorHashtable();
			using (creator.InitializeMultiJobOperation(transactions))
			{
				creator.ChangeJob(Job);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);
			}
			AssertEquals("Invoice Count", 0, transactions.Count);
			AssertEquals("AP Transactions Count", 0, transactions.APTransactionsCount);
		}

		public void TestInvoiceHeaderCurrencyInMultiJobOperation_DifferentForeignCurrencies()
		{
			TestObjectCreator.CreateExchangeRate(USD, .7m);
			TestObjectCreator.CreateExchangeRate(GBP, .4m);
			AssertEquals("For test to be effective, we assume local currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertInvoiceHeaderCurrencyInMultiJobOperation(GBP, USD, AUD);
		}

		public void TestInvoiceHeaderCurrencyInMultiJobOperation_SameForeignCurrency()
		{
			TestObjectCreator.CreateExchangeRate(GBP, .4m);
			AssertEquals("For test to be effective, we assume local currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertInvoiceHeaderCurrencyInMultiJobOperation(USD, USD, USD);
		}

		public void TestInvoiceHeaderCurrencyInMultiJobOperation_LocalCurrency()
		{
			AssertEquals("For test to be effective, we assume local currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertInvoiceHeaderCurrencyInMultiJobOperation(AUD, AUD, AUD);
		}

		public void TestInvoiceHeaderCurrencyInMultiJobOperation_BothForeighAndLocalCurrency()
		{
			AssertEquals("For test to be effective, we assume local currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertInvoiceHeaderCurrencyInMultiJobOperation(USD, AUD, AUD);
			Factory.Save();
		}

		void AssertInvoiceHeaderCurrencyInMultiJobOperation(RefCurrency currency1, RefCurrency currency2, RefCurrency expectedCurrency)
		{
			TestObjectCreator.CreateExchangeRate(NZD, .7m);
			TestObjectCreator.CreateExchangeRate(currency1, 1m);
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			Charge1 = CreateCharge(Job, TestObjectCreator.ManualJobAccrualChargeCode, "Charge Code 1", currency1, 200M, Creditor1, NZD, 150M, LocalClient);
			var job2 = CreateJob("C00001001", LocalClient, 5M, Agent, 10M);
			var charge2 = CreateCharge(job2, TestObjectCreator.ManualJobAccrualChargeCode, "Charge Code 1", currency2, 200M, Creditor1, NZD, 150M, LocalClient);
			SetAPInvoiceInfo(Charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			Factory.Save();

			AssertEquals("Currency of charges are as expected", currency1.RX_Code, Charge1.JR_CostCurrency);
			AssertEquals("Currency of charges are as expected", currency2.RX_Code, charge2.JR_CostCurrency);
			AssertEquals("Currency of charges are as expected", "NZD", Charge1.JR_SellCurrency);
			AssertEquals("Currency of charges are as expected", "NZD", charge2.JR_SellCurrency);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Charge1.JR_AC;
			cost.CostExchangeRate.Currency = Charge1.JR_RX_NKCostCurrency;
			cost.E6_OSCostAmount = Charge1.JR_OSCostAmt;
			cost.E6_AT_TaxRate = Charge1.JR_AT_CostGSTRate;
			cost.E6_OH_Creditor = Charge1.JR_OH_CostAccount;
			cost.E6_InvoiceNum = Charge1.JR_APInvoiceNum;
			cost.E6_InvoiceDate = Charge1.JR_APInvoiceDate;
			cost.E6_PaymentDate = Charge1.JR_PaymentDate;
			Charge1.JR_E6 = cost.PK;
			JobConsolCost cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = charge2.JR_AC;
			cost2.CostExchangeRate.Currency = charge2.JR_RX_NKCostCurrency;
			cost2.E6_OSCostAmount = charge2.JR_OSCostAmt;
			cost2.E6_AT_TaxRate = charge2.JR_AT_CostGSTRate;
			cost2.E6_OH_Creditor = charge2.JR_OH_CostAccount;
			cost2.E6_InvoiceNum = charge2.JR_APInvoiceNum;
			cost2.E6_InvoiceDate = charge2.JR_APInvoiceDate;
			cost2.E6_PaymentDate = charge2.JR_PaymentDate;
			charge2.JR_E6 = cost2.PK;

			var creator = new APInvoiceCreator(Job, consol, false, new JobConsolCostCollection(Factory, consol), true);
			var transactions = new TransactionCreatorHashtable();

			using (creator.InitializeMultiJobOperation(transactions))
			{
				creator.ChangeJob(Job);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);

				creator.ChangeJob(job2);
				creator.CreateTransactionsForMultiJobOperation();
				AssertEquals("No invoices is created here. Only charge grouping is done.", 0, transactions.Count);
			}
			AssertEquals("Invoice Count after finalizing the operation", 1, transactions.Count);
			AssertEquals("AP Transactions Count after finalizing the operation", 1, transactions.APTransactionsCount);
			APInvoice invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
			AssertEquals("Due to different currencies, invoice created in local currency", expectedCurrency.RX_Code, invoice.AH_RX_NKTransactionCurrency);

			cost.E6_AH_APInvoice = invoice.PK;
			cost2.E6_AH_APInvoice = invoice.PK;
		}

		public void TestPostingWithARAPInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var rate1 = CreateExchangeRate(job, USD, .7M);

			var charge1 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			SetAPInvoiceInfo(charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			AssertEquals("charge rate unchanged yet.", 0.7m, charge1.JR_OSCostExRate);
			AssertEquals("job rate unchanged yet.", 0.7m, rate1.JF_BaseRate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2000, 1, 1), new DateTime(2070, 1, 1));
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var transactions = new TransactionCreatorHashtable();
			var creator = new APInvoiceCreator(job);
			creator.CreateTransactions(transactions);

			AssertEquals("should be new rate on the charge", 2m, charge1.JR_OSCostExRate);
			AssertEquals("job exchange rate will be updated later on posting", 0.7m, rate1.JF_BaseRate);
			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");

			AssertNotNull(creditor1Inv1);
			AssertEquals(1, creditor1Inv1.Lines.Count);

			AssertEquals("USD", creditor1Inv1.AH_RX_NKTransactionCurrency);
			AssertEquals("should be new rate", 2m, creditor1Inv1.AH_ExchangeRate);

			TransactionLine cc1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job.PK);
			AssertNotNull(cc1Line);
			AssertEquals("USD", cc1Line.AL_RX_NKTransactionCurrency);
			AssertEquals("should be new rate", 2m, cc1Line.AL_ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#endregion

		[TestDate(2015, 5, 10)]
		public void TestPerformAPInvoiceBackDatingAndUpdateExRate()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			//AP backdating
			var regBackDateAPConfig = new BackDateAPInvoicesConfiguration();
			var regBackDateAPConfig1 = regBackDateAPConfig.PostDateConfigurationCollection[0];
			regBackDateAPConfig1.JobType = "ALL";
			regBackDateAPConfig1.DirectionCode = "";
			regBackDateAPConfig1.Mode = "";
			regBackDateAPConfig1.BrokerCode = "";
			regBackDateAPConfig1.SignificantDateCode = "ADD";
			regBackDateAPConfig1.PriorClosedPeriod = "";
			regBackDateAPConfig1.PriorOpenPeriod = "";
			regBackDateAPConfig1.CurrentPeriod = "EPM";
			regBackDateAPConfig1.FuturePeriod = "";
			regBackDateAPConfig1.ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regBackDateAPConfig);

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10)); //today's rate
			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 4, 30)); //backdating rate

			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var rate1 = CreateExchangeRate(job, USD, 0.5M);
			var charge1 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", ZDateTime.Today.Date, ZDateTime.Today.Date);

			Factory.Save();

			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, charge1.JR_OSCostExRate);
			AssertEquals(200m, charge1.JR_LocalCostAmt);
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(0.475m, charge1.JR_OSSellExRate);
			AssertEquals(315.79m, charge1.JR_LocalSellAmt);
			AssertEquals(150m, charge1.JR_OSSellAmt);
			AssertEquals(true, !charge1.IsRevenuePosted && !charge1.IsCostPosted);

			var transactions = new TransactionCreatorHashtable();
			var apInvoiceCreator = new APInvoiceCreator(job);
			apInvoiceCreator.CreateTransactions(transactions);

			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
			var invoice1 = transactions.GetAllAPTransactions()[0];
			AssertEquals("USD", invoice1.AH_RX_NKTransactionCurrency);
			AssertEquals("PreCondition", true, invoice1.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.34.", 1.499864m, invoice1.AH_ExchangeRate);

			Factory.Save();

			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(1.5m, charge1.JR_OSCostExRate);
			AssertEquals(66.67m, charge1.JR_LocalCostAmt);
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(0.475m, charge1.JR_OSSellExRate);
			AssertEquals(315.79m, charge1.JR_LocalSellAmt);
			AssertEquals(150m, charge1.JR_OSSellAmt);
			AssertEquals(true, !charge1.IsRevenuePosted && charge1.IsCostPosted);

			AssertEquals("USD", invoice1.AH_RX_NKTransactionCurrency);
			AssertEquals(true, invoice1.AH_PostedToEFT);
			AssertEquals(charge1.JR_OSCostAmt, invoice1.AH_OSExTaxAmount);
			AssertEquals(charge1.JR_LocalCostAmt, invoice1.AH_LocalExTaxAmount);
			AssertEquals(10m, invoice1.AH_OSTaxAmount);
			AssertEquals(6.67m, invoice1.AH_LocalTaxAmount);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.34.", 1.499864m, invoice1.AH_ExchangeRate);
			AssertEquals(new DateTime(2015, 4, 30), invoice1.AH_PostDate);
			AssertEquals(new DateTime(2015, 5, 10), invoice1.AH_InvoiceDate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		public void TestPerformAPInvoiceBackDatingAndUpdateExRateWithIgnoredInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			//AP backdating
			var regBackDateAPConfig = new BackDateAPInvoicesConfiguration();
			var regBackDateAPConfig1 = regBackDateAPConfig.PostDateConfigurationCollection[0];
			regBackDateAPConfig1.JobType = "ALL";
			regBackDateAPConfig1.DirectionCode = "";
			regBackDateAPConfig1.Mode = "";
			regBackDateAPConfig1.BrokerCode = "";
			regBackDateAPConfig1.SignificantDateCode = "ADD";
			regBackDateAPConfig1.PriorClosedPeriod = "";
			regBackDateAPConfig1.PriorOpenPeriod = "";
			regBackDateAPConfig1.CurrentPeriod = "EPM";
			regBackDateAPConfig1.FuturePeriod = "";
			regBackDateAPConfig1.ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regBackDateAPConfig);

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10)); //today's rate
			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 4, 30)); //backdating rate

			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var rate1 = CreateExchangeRate(job, USD, 0.5M);
			var charge1 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			var charge2 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", ZDateTime.Today.Date, ZDateTime.Today.Date);
			SetAPInvoiceInfo(charge2, "2", ZDateTime.Today.Date, ZDateTime.Today.Date);

			Factory.Save();

			foreach (var charge in new Charge[] { charge1, charge2 })
			{
				AssertEquals("USD", charge.JR_RX_NKCostCurrency);
				AssertEquals(0.5m, charge.JR_OSCostExRate);
				AssertEquals(200m, charge.JR_LocalCostAmt);
				AssertEquals(100m, charge.JR_OSCostAmt);
				AssertEquals(0.475m, charge.JR_OSSellExRate);
				AssertEquals(315.79m, charge.JR_LocalSellAmt);
				AssertEquals(150m, charge.JR_OSSellAmt);
				AssertEquals(true, !charge.IsRevenuePosted && !charge.IsCostPosted);
			}

			bool isCharge1ExcludedFromPosting = false;
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()))
				.Returns<APInvoiceCharges[]>(
					(apInvoiceCharges) =>
					{
						apInvoiceCharges.First(x => x.InvoiceNumber == "1_1").IsExcludedFromPosting = true;
						isCharge1ExcludedFromPosting = apInvoiceCharges.First(x => x.InvoiceNumber == "1_1").IsExcludedFromPosting;
						return ZDialogResult.OK;
					});

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceCharges = new APInvoiceCharges(charge1.CostAccount.OH_Code, charge1.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			invoiceCharges.Charges.Add(charge1);
			request.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			charge1.JR_APInvoiceNum += "_1";
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var apInvoiceCreator = new APInvoiceCreator(job, postGUIProviderMock.Object);
			apInvoiceCreator.CreateTransactions(transactions);

			AssertEquals(true, isCharge1ExcludedFromPosting);
			AssertEquals("Payables Transaction Count", 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
			var invoice1 = transactions.GetAllAPTransactions()[0];
			AssertEquals("USD", invoice1.AH_RX_NKTransactionCurrency);
			AssertEquals("PreCondition", true, invoice1.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.34.", 1.499864m, invoice1.AH_ExchangeRate);

			Factory.Save();

			//charge 1 should be unchanged
			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, charge1.JR_OSCostExRate);
			AssertEquals(200m, charge1.JR_LocalCostAmt);
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(0.475m, charge1.JR_OSSellExRate);
			AssertEquals(315.79m, charge1.JR_LocalSellAmt);
			AssertEquals(150m, charge1.JR_OSSellAmt);
			AssertEquals(true, !charge1.IsRevenuePosted && !charge1.IsCostPosted);

			//charge 2 should be posted with new ex. rate
			AssertEquals("USD", charge2.JR_RX_NKCostCurrency);
			AssertEquals(1.5m, charge2.JR_OSCostExRate);
			AssertEquals(66.67m, charge2.JR_LocalCostAmt);
			AssertEquals(100m, charge2.JR_OSCostAmt);
			AssertEquals(0.475m, charge2.JR_OSSellExRate);//Revenue not posted, therefore SellExRate is in sync with ExRate on the Job header (charge2 and charge1 shares the same ex rate)
			AssertEquals(315.79m, charge2.JR_LocalSellAmt);
			AssertEquals(150m, charge2.JR_OSSellAmt);
			AssertEquals(true, !charge2.IsRevenuePosted && charge2.IsCostPosted);

			AssertEquals("USD", invoice1.AH_RX_NKTransactionCurrency);
			AssertEquals("2", invoice1.AH_TransactionNum);
			AssertEquals(true, invoice1.AH_PostedToEFT);
			AssertEquals(charge2.JR_OSCostAmt, invoice1.AH_OSExTaxAmount);
			AssertEquals(charge2.JR_LocalCostAmt, invoice1.AH_LocalExTaxAmount);
			AssertEquals(10m, invoice1.AH_OSTaxAmount);
			AssertEquals(6.67m, invoice1.AH_LocalTaxAmount);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.34.", 1.499864m, invoice1.AH_ExchangeRate);
			AssertEquals(new DateTime(2015, 4, 30), invoice1.AH_PostDate);
			AssertEquals(new DateTime(2015, 5, 10), invoice1.AH_InvoiceDate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		public void TestDontChangeExRatesIfNoSecurityRights()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateUSDBuyRate(5.1m, new DateTime(2015, 5, 10)); //today's rate
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 6.10m;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = TestObjectCreator.AALSHI.PK;

			rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 6.10m;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = TestObjectCreator.ABIGAS.PK;

			rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 6.10m;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = TestObjectCreator.Debtor.PK;

			rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 6.10m;
			rate.OrgType = ExchangeRateOrgTypeEnum.Creditor;
			rate.JF_OH_Org = TestObjectCreator.Creditor1.PK;

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, TestObjectCreator.AALSHI);
			charge1.JR_RX_NKCostCurrency = charge1.JR_RX_NKSellCurrency = "USD";
			charge1.JR_OSCostAmt = charge1.JR_OSSellAmt = 611;
			charge1.JR_OSCostExRate = 6.11m;
			charge1.RevenueExchangeRate.SetBuyRate_ForTestOnly(6.11m);

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 110, TestObjectCreator.Creditor1, "INV1", null, 0, TestObjectCreator.ABIGAS);
			charge2.JR_RX_NKCostCurrency = charge2.JR_RX_NKSellCurrency = "USD";
			charge2.RevenueExchangeRate.SetBuyRate_ForTestOnly(6.12m);
			charge2.JR_OSCostExRate = 6.12m;

			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 112, TestObjectCreator.Creditor1, "INV2", null, 0, TestObjectCreator.Debtor);
			charge3.JR_RX_NKCostCurrency = charge3.JR_RX_NKSellCurrency = "USD";
			charge3.JR_OSCostAmt = charge3.JR_OSSellAmt = 613;
			charge3.JR_OSCostExRate = 6.13m;
			charge3.RevenueExchangeRate.SetBuyRate_ForTestOnly(6.13m);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = apps.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost.E6_OSCostAmount = 613;
			consolCost.E6_ExchangeRate = 6.13m;
			consolCost.E6_InvoiceNum = "INV2";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today.AddDays(1);
			charge3.JR_E6 = consolCost.PK;

			Factory.Save();
			consolCost.ApportionmentCharges.Load();

			AssertEquals("USD", job.ExchangeRates[0].JF_RX_NKRateCurrency);
			AssertEquals(6.11m, job.ExchangeRates[0].JF_BaseRate);

			AssertEquals("USD", charge1.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge1.JR_OSSellCurrencyCode);
			AssertEquals(6.11m, charge1.JR_OSCostExRate);
			AssertEquals(6.11m, charge1.JR_OSSellExRate);

			AssertEquals("USD", charge2.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge2.JR_OSSellCurrencyCode);
			AssertEquals(6.12m, charge2.JR_OSCostExRate);
			AssertEquals(6.12m, charge2.JR_OSSellExRate);

			AssertEquals("USD", charge3.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge3.JR_OSSellCurrencyCode);
			AssertEquals(6.13m, charge3.JR_OSCostExRate);
			AssertEquals(6.13m, charge3.JR_OSSellExRate);

			AssertEquals("USD", consolCost.E6_RX_NKCurrency);
			AssertEquals(6.13m, consolCost.E6_ExchangeRate);

			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
			postGUIProviderMock.Setup(m => m.IsBulkPosting).Returns(true);
			postGUIProviderMock.Verify(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>()), Times.Never);
			postGUIProviderMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, (ZString)consol.TablePrefix));
			var creator = new APInvoiceCreator(job, consol, false, new JobConsolCostCollection(Factory, consol), false, postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();
			ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Verify(m => m.ShouldApprovalRequestBeCreated, Times.Never);
			postGUIProviderMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);

			//charge1+charge2.osAmount (611 + 612) / today's rate(5.1) * GST(1.1) = 263.78 //276.51
			postGUIProviderMock.Setup(m => m.NotifyBulkPostingIsNotAuthorized(@"Creditor: ZCreditor1, Number: INV1, Ref. Number:S001, Amount:276.51.
You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.

-----------"));

			//charge3.osAmount(613) / today's rate(5.1) * GST(1.1) = 132.22
			postGUIProviderMock.Setup(m => m.NotifyBulkPostingIsNotAuthorized(@"Creditor: ZCreditor1, Number: INV2, Ref. Number:26FYS0PM3GCY3VHFAX26, Amount:132.22.
You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.

-----------"));
			transactions = new TransactionCreatorHashtable();
			Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				creator.CreateTransactions(transactions);
			}
			postGUIProviderMock.VerifyAll();
			securityProviderMock.VerifyAll();
			AssertEquals("Postcondition: AP Invoice is not created", 0, transactions.GetAllAPTransactions().Length);

			Factory.Save();

			//everything should be unchanged, except for cost ex rate which is the same for each charge (because the charges have the same cost currency and creditor)
			AssertEquals("USD", job.ExchangeRates[0].JF_RX_NKRateCurrency);
			AssertEquals(6.11m, job.ExchangeRates[0].JF_BaseRate);

			AssertEquals("USD", charge1.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge1.JR_OSSellCurrencyCode);
			AssertEquals(6.13m, charge1.JR_OSCostExRate);
			AssertEquals(6.11m, charge1.JR_OSSellExRate);

			AssertEquals("USD", charge2.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge2.JR_OSSellCurrencyCode);
			AssertEquals(6.13m, charge2.JR_OSCostExRate);
			AssertEquals(6.12m, charge2.JR_OSSellExRate);

			AssertEquals("USD", charge3.JR_OSCostCurrencyCode);
			AssertEquals("USD", charge3.JR_OSSellCurrencyCode);
			AssertEquals(6.13m, charge3.JR_OSCostExRate);
			AssertEquals(6.13m, charge3.JR_OSSellExRate);

			AssertEquals("USD", consolCost.E6_RX_NKCurrency);
			AssertEquals(6.13m, consolCost.E6_ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTransactions_UseJobExchangeRateDefaultIsFalse_JobCharge()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var shipment = TestObjectCreator.CreateShipment("S00001000");
			var job = TestObjectCreator.CreateJob(shipment, localClientOrg: LocalClient);
			var charge = TestObjectCreator.CreateCharge(job, CC1, "desc1", USD, 80M, Creditor1, USD, 100M, LocalClient);
			SetAPInvoiceInfo(charge, "123", Now.AddDays(10), Now.AddDays(20));

			Factory.Save();

			var creator = new APInvoiceCreator(job);
			var transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			Assert("Charge posted.", charge.IsCostPosted);

			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("PreCondition", 1, transactions.Count);
			AssertEquals("PreCondition", 1, apTransactions.Length);
			AssertEquals("Since transaction is posted from job, we should always enable UseJobExchangeRate", true, apTransactions[0].UseJobExchangeRate);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTransactions_UseJobExchangeRateDefaultIsFalse_ConsolCost()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var shipmentJob = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.1234m, 150M, creditor: TestObjectCreator.Creditor1);
			consolCost.E6_InvoiceNum = "INV2";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today.AddDays(1);
			AssertEquals("PreCondition", 1, consolCost.ApportionmentCharges.Count);
			Factory.Save();

			var creator = new APInvoiceCreator(shipmentJob, consol, false, new JobConsolCostCollection(Factory, consol), true);
			var transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			Assert("Charge posted.", consolCost.ApportionmentCharges[0].IsCostPosted);

			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("PreCondition", 1, transactions.Count);
			AssertEquals("PreCondition", 1, apTransactions.Length);
			AssertEquals("Since transaction is posted from job, we should always enable UseJobExchangeRate", true, apTransactions[0].UseJobExchangeRate);
		}

		#region Implementation

		protected override BaseTransactionCreator GetTransactionCreator(Job job, IJobCostingPlugIn consol)
		{
			return new APInvoiceCreator(job, consol, false, null);
		}

		#region NZD

		protected RefCurrency nzd;
		protected RefCurrency NZD
		{
			get
			{
				if (nzd == null)
				{
					nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
				}
				return nzd;
			}
		}

		#endregion

		void SetupCharges()
		{
			Job = CreateJob("C00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(Job, GBP, .4M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge2 = CreateCharge(Job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge3 = CreateCharge(Job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge4 = CreateCharge(Job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge5 = CreateCharge(Job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge6 = CreateCharge(Job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge8 = CreateCharge(Job, CC8, "Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
		}

		void SetupAPInvoiceInfo()
		{
			SetAPInvoiceInfo(Charge1, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge2, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge3, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge5, "2", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
			SetAPInvoiceInfo(Charge8, "2", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));
		}

		void InitializeWIPAccruals()
		{
			Charge1WIP = Charge1.WIP;
			Charge2WIP = Charge2.WIP;
			Charge3WIP = Charge3.WIP;
			Charge4WIP = Charge4.WIP;
			Charge5WIP = Charge5.WIP;
			Charge6WIP = Charge6.WIP;
			Charge7WIP = Charge7.WIP;

			Charge1Accrual = Charge1.Accrual;
			Charge2Accrual = Charge2.Accrual;
			Charge3Accrual = Charge3.Accrual;
			Charge5Accrual = Charge5.Accrual;
			Charge6Accrual = Charge6.Accrual;
			Charge7Accrual = Charge7.Accrual;
			Charge8Accrual = Charge8.Accrual;
		}

		#region Registry Setup

		internal static PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 250, AuthorisationCodes.NoApprovalRequired);
			var above = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 250, AuthorisationCodes.FirstApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		protected void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		PaymentTwelveLevelAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		#endregion

		Job Job;
		Charge Charge1;
		Charge Charge2;
		Charge Charge3;
		Charge Charge4;
		Charge Charge5;
		Charge Charge6;
		Charge Charge7;
		Charge Charge8;

		AccTransactionLines Charge1WIP;
		AccTransactionLines Charge2WIP;
		AccTransactionLines Charge3WIP;
		AccTransactionLines Charge4WIP;
		AccTransactionLines Charge5WIP;
		AccTransactionLines Charge6WIP;
		AccTransactionLines Charge7WIP;

		AccTransactionLines Charge1Accrual;
		AccTransactionLines Charge2Accrual;
		AccTransactionLines Charge3Accrual;
		AccTransactionLines Charge5Accrual;
		AccTransactionLines Charge6Accrual;
		AccTransactionLines Charge7Accrual;
		AccTransactionLines Charge8Accrual;

		#endregion
	}
}
