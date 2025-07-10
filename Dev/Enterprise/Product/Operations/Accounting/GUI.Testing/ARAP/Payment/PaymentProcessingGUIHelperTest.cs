using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public class PaymentProcessingGUIHelperTest : TestCaseWithFactory
	{
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		PaymentBatchPostingTestHelper BatchPostingHelper => batchPostingHelper ?? (batchPostingHelper = new PaymentBatchPostingTestHelper(Factory));
		PaymentBatchPostingTestHelper batchPostingHelper;

		PaymentProcessingGUIHelper PaymentProcessingGUIHelper => paymentProcessingGUIHelper ?? (paymentProcessingGUIHelper = new PaymentProcessingGUIHelper());
		PaymentProcessingGUIHelper paymentProcessingGUIHelper;

		void SetupDataForBaseTest()
		{
			BatchPostingHelper.PrepareForBaseTest();
		}

		#region IsPaymentAuthorisationRequired

		public void Test_IsPaymentAuthorisationRequired()
		{
			SetupDataForBaseTest();
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty()))
			{
				AssertEquals(0, AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.Count);
				AssertEquals(false, PaymentProcessingGUIHelper.IsPaymentAuthorisationRequired());
			}

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			{
				AssertEquals(true, PaymentProcessingGUIHelper.IsPaymentAuthorisationRequired());
			}

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettings(RangeCodes.Above, 0, AuthorisationCodes.NoApprovalRequired)))
			{
				AssertEquals(false, PaymentProcessingGUIHelper.IsPaymentAuthorisationRequired());
			}
		}

		#endregion

		#region Test process multiple Approvals

		public void TestProcessMultipleApprovals_SubmitForApproval()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

			AssertEquals("Percondition", true, payment1.IsDraft);
			AssertEquals("Percondition", true, payment2.IsDraft);
			AssertEquals("Percondition", true, payment3.IsDraft);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.SubmitForApproval(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsFullyApproved);
			AssertEquals(true, payment2.IsFullyApproved);
			AssertEquals(true, payment3.IsFullyApproved);
		}

		public void TestProcessMultipleApprovals_ApproveForPosting()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

			AssertEquals("Percondition", true, payment1.IsDraft);
			AssertEquals("Percondition", true, payment2.IsDraft);
			AssertEquals("Percondition", true, payment3.IsDraft);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.ApproveForPosting(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsFullyApproved);
			AssertEquals(true, payment2.IsFullyApproved);
			AssertEquals(true, payment3.IsFullyApproved);
		}

		public void TestProcessMultipleApprovals_Authorise()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsAwaitingApproval);
			AssertEquals("Percondition", true, payment2.IsAwaitingApproval);
			AssertEquals("Percondition", true, payment3.IsAwaitingApproval);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsFullyApproved);
			AssertEquals(true, payment2.IsFullyApproved);
			AssertEquals(true, payment3.IsFullyApproved);
		}

		public void TestProcessMultipleApprovals_UnAuthorise()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_GS_NKApproval1st = payment2.AV_GS_NKApproval1st = payment3.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			var payableAuthorizationSettings = new PaymentAuthorisationSettingsCollection();
			var setting = payableAuthorizationSettings.AddNew();
			setting.Amount = 0m;
			setting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			setting.Range = RangeCodes.Above;
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, payableAuthorizationSettings))
			{
				AssertEquals("Percondition", true, payment1.IsFullyApproved);
				AssertEquals("Percondition", true, payment2.IsFullyApproved);
				AssertEquals("Percondition", true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(paymentApprovalCollection.ToArray());

				AssertEquals(true, payment1.IsAwaitingApproval);
				AssertEquals(true, payment2.IsAwaitingApproval);
				AssertEquals(true, payment3.IsAwaitingApproval);
			}
		}

		public void TestProcessMultipleApprovals_Reject()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsAwaitingApproval);
			AssertEquals("Percondition", true, payment2.IsAwaitingApproval);
			AssertEquals("Percondition", true, payment3.IsAwaitingApproval);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(x => ((PaymentRejectionReasonForm)x).SetReason("INS", "Not enough money"));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.RejectPaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsRejected);
			AssertEquals(true, payment2.IsRejected);
			AssertEquals(true, payment3.IsRejected);
		}

		public void TestProcessMultipleApprovals_Post()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsFullyApproved);
			AssertEquals("Percondition", true, payment2.IsFullyApproved);
			AssertEquals("Percondition", true, payment3.IsFullyApproved);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.PostPaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsPosted);
			AssertEquals(true, payment2.IsPosted);
			AssertEquals(true, payment3.IsPosted);
		}

		public void TestProcessMultipleApprovals_PopulateChequeNo()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_ChequeOrReference = payment2.AV_ChequeOrReference = payment3.AV_ChequeOrReference = ZString.Empty;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.AV_ChequeOrReference.IsEmpty);
			AssertEquals("Percondition", true, payment2.AV_ChequeOrReference.IsEmpty);
			AssertEquals("Percondition", true, payment3.AV_ChequeOrReference.IsEmpty);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
			AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
			AssertEquals(false, payment3.AV_ChequeOrReference.IsEmpty);
		}

		public void TestProcessMultipleApprovals_PopulateChequeNoAndPost()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_ChequeOrReference = payment2.AV_ChequeOrReference = payment3.AV_ChequeOrReference = ZString.Empty;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.AV_ChequeOrReference.IsEmpty);
			AssertEquals("Percondition", true, payment2.AV_ChequeOrReference.IsEmpty);
			AssertEquals("Percondition", true, payment3.AV_ChequeOrReference.IsEmpty);
			AssertEquals("Percondition", true, payment1.IsFullyApproved);
			AssertEquals("Percondition", true, payment2.IsFullyApproved);
			AssertEquals("Percondition", true, payment3.IsFullyApproved);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
			AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
			AssertEquals(false, payment3.AV_ChequeOrReference.IsEmpty);
			AssertEquals(true, payment1.IsPosted);
			AssertEquals(true, payment2.IsPosted);
			AssertEquals(true, payment3.IsPosted);
		}

		public void TestProcessMultipleApprovals_Cancel()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

			AssertEquals("Percondition", true, payment1.IsDraft);
			AssertEquals("Percondition", true, payment2.IsDraft);
			AssertEquals("Percondition", true, payment3.IsDraft);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.CancelPaymentApprovals(paymentApprovalCollection.ToArray());

			AssertEquals(true, payment1.IsCancelled);
			AssertEquals(true, payment2.IsCancelled);
			AssertEquals(true, payment3.IsCancelled);
		}

		public void TestProcessMultipleApprovals_Print()
		{
			var exceptedMessage = "Please select a transaction to print";

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			PaymentProcessingGUIHelper.PrintPaymentApproval(poster.PaymentApprovalCollection.ToArray());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Test When No Payment Approvals

		public void TestWhenNoPaymentApprovals_SubmitForApproval()
		{
			var exceptedMessage = "Please select one or more Payments to Submit for Approval.";

			PaymentProcessingGUIHelper.SubmitForApproval(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.SubmitForApproval(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_ApproveForPosting()
		{
			var exceptedMessage = "Please select one or more Payments to Approve for Posting.";

			PaymentProcessingGUIHelper.ApproveForPosting(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.ApproveForPosting(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_Authorise()
		{
			var exceptedMessage = "Please select one or more Payments to authorize.";

			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_UnAuthorises()
		{
			var exceptedMessage = "Please select one or more Payments to unauthorize.";

			PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_Reject()
		{
			var exceptedMessage = "Please select one or more Payments to Reject.";

			PaymentProcessingGUIHelper.RejectPaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.RejectPaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_Post()
		{
			var exceptedMessage = "Please select one or more Transactions to process.";

			PaymentProcessingGUIHelper.PostPaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.PostPaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_PopulateChequeNo()
		{
			var exceptedMessage = "Please select one or more Transactions to process.";

			PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_PopulateChequeNoAndPost()
		{
			var exceptedMessage = "Please select one or more Transactions to process.";

			PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_Cancel()
		{
			var exceptedMessage = "Please select one or more Payments to Cancel.";

			PaymentProcessingGUIHelper.CancelPaymentApprovals(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.CancelPaymentApprovals(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenNoPaymentApprovals_Print()
		{
			var exceptedMessage = "Please select a transaction to print";

			PaymentProcessingGUIHelper.PrintPaymentApproval(null);
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			PaymentProcessingGUIHelper.PrintPaymentApproval(Array.Empty<BusinessObject>());
			AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Test For Cancelled Approval

		public void TestForCancelledPaymentApproval_SubmitForApproval()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.SubmitForApproval(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be submitted for approval:
{paymentApproval.GetDescription()}
This Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_ApproveForPosting()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.ApproveForPosting(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be approved for posting:
{paymentApproval.GetDescription()}
This Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_Authorise()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be authorized:
{paymentApproval.GetDescription()}
This Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_UnAuthorise()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be unauthorized:
{paymentApproval.GetDescription()}
This Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_Reject()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.RejectPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be rejected:
{paymentApproval.GetDescription()}
This Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_Post()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.PostPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be processed:
{paymentApproval.GetDescription()}
This payment has been canceled.

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_PopulateChequeNo()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be processed:
{paymentApproval.GetDescription()}
This payment has been canceled.

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_PopulateChequeNoAndPost()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be processed:
{paymentApproval.GetDescription()}
This payment has been canceled.

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_Cancel()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			PaymentProcessingGUIHelper.CancelPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be canceled:
{paymentApproval.GetDescription()}
This Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForCancelledPaymentApproval_Print()
		{
			var paymentApproval = PrepareForCancelledPaymentApproval();
			using (new DisposableAction(() => PaymentProcessingGUIHelper.EnablePrint_ForTestOnly = true, () => PaymentProcessingGUIHelper.EnablePrint_ForTestOnly = false))
			{
				PaymentProcessingGUIHelper.PrintPaymentApproval(new BusinessObject[1] { paymentApproval });
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<PaymentDocumentsPrintPopup>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		PaymentApprovalBase PrepareForCancelledPaymentApproval()
		{
			SetupDataForBaseTest();

			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			paymentApproval.AV_AB = BatchPostingHelper.TestBank.PK;
			paymentApproval.AV_AK = BatchPostingHelper.TestCheques.PK;
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			AssertEquals(true, paymentApproval.IsCancelled);

			return paymentApproval;
		}

		#endregion

		#region Test For Posted Payment Approval

		public void TestForPostedPaymentApproval_SubmitForApproval()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.SubmitForApproval(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be submitted for approval:
{paymentApproval.GetDescription()}
This Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_ApproveForPosting()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.ApproveForPosting(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be approved for posting:
{paymentApproval.GetDescription()}
This Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_Authorise()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be authorized:
{paymentApproval.GetDescription()}
This Payment is already Posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_UnAuthorise()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be unauthorized:
{paymentApproval.GetDescription()}
This Payment is already Posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_Reject()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.RejectPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be rejected:
{paymentApproval.GetDescription()}
This Payment is already Posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_Post()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.PostPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be processed:
{paymentApproval.GetDescription()}
This Payment is already posted

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_PopulateChequeNo()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			Factory.Save();

			AssertEquals("Percondition", "000001", paymentApproval.AV_ChequeOrReference);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(new BusinessObject[1] { paymentApproval });

			AssertEquals(false, paymentApproval.AV_ChequeOrReference.IsEmpty);
			AssertEquals($@"The following Payments will be processed:
{paymentApproval.GetDescription()}

Do you want to populate the cheque number for these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Not changed", "000001", paymentApproval.AV_ChequeOrReference);
		}

		public void TestForPostedPaymentApproval_PopulateChequeNoAndPost()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			var description = paymentApproval.GetDescription();
			PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be processed:
{description}
This Payment is already posted

These transactions cannot be posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_Cancel()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			PaymentProcessingGUIHelper.CancelPaymentApprovals(new BusinessObject[1] { paymentApproval });
			AssertEquals($@"The following Payments cannot be canceled:
{paymentApproval.GetDescription()}
This Payment is already Posted", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForPostedPaymentApproval_Print()
		{
			var paymentApproval = PrepareForPostedPaymentApproval();
			using (new DisposableAction(() => PaymentProcessingGUIHelper.EnablePrint_ForTestOnly = true, () => PaymentProcessingGUIHelper.EnablePrint_ForTestOnly = false))
			{
				PaymentProcessingGUIHelper.PrintPaymentApproval(new BusinessObject[1] { paymentApproval });
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<PaymentDocumentsPrintPopup>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		PaymentApprovalBase PrepareForPostedPaymentApproval()
		{
			SetupDataForBaseTest();

			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			paymentApproval.AV_AB = BatchPostingHelper.TestBank.PK;
			paymentApproval.AV_AK = BatchPostingHelper.TestCheques.PK;
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.IsPostWithoutMatching = true;
			Factory.Save();

			AssertEquals(true, paymentApproval.IsPosted);

			return paymentApproval;
		}

		#endregion

		#region Test Bank Transfer form is prompted when post EPayment approval

		public void TestBankTransferFormIsPromptedWhenPostEPaymentApproval()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			var ePaymentBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval1 = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "2", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval1);
			deal1.Quote.QU_FromAmount = 200m;

			var paymentApproval2 = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "3", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval2);
			deal2.Quote.QU_FromAmount = 200m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			paymentApproval1.IsPostWithoutMatching = true;
			PaymentProcessingGUIHelper.PostPaymentApprovals(new BusinessObject[1] { paymentApproval1 });
			AssertEquals("Expect bank transfer form prompted when registry PromptBankTransferOnPostingEPayment is on", 1, PaymentProcessingGUIHelper.bankTransferFormsPrompted.Count);
			AssertEquals(typeof(BankTransferForm), PaymentProcessingGUIHelper.bankTransferFormsPrompted[0].GetType());
			PaymentProcessingGUIHelper.bankTransferFormsPrompted[0].Dispose();

			PaymentProcessingGUIHelper.bankTransferFormsPrompted.Clear();
			paymentApproval2.IsPostWithoutMatching = true;
			using (AccountingMasterFilesRegistry.Instance.PromptBankTransferOnPostingEPayment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				PaymentProcessingGUIHelper.PostPaymentApprovals(new BusinessObject[1] { paymentApproval2 });
				AssertEquals("Expect nothing prompted when registry is off", 0, PaymentProcessingGUIHelper.bankTransferFormsPrompted.Count);
			}
		}

		public void TestBankTransferFormIsPromptedWhenPostEPaymentApproval_WithSameFundingBankAccount()
			=> AssertBankTransferFormIsPrompted(false);

		public void TestBankTransferFormIsPromptedWhenPostEPaymentApproval_WithDifferentFundingBankAccount()
			=> AssertBankTransferFormIsPrompted(true);

		void AssertBankTransferFormIsPrompted(bool isDifferentAccount)
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			var ePaymentBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval1 = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "2", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval1);
			deal1.Quote.QU_FromAmount = 200m;
			paymentApproval1.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;

			var paymentApproval2 = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "3", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval2);
			paymentApproval2.AV_AB_FundingBankAccount = isDifferentAccount ? TestObjectCreator.EURBankAccount.PK : TestObjectCreator.USDBankAccount.PK;
			deal2.Quote.QU_FromAmount = 200m;

			var paymentApproval3 = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "4", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			var deal3 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval3);
			paymentApproval3.AV_AB_FundingBankAccount = ZGuid.Empty;
			deal3.Quote.QU_FromAmount = 200m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			paymentApproval1.IsPostWithoutMatching = true;
			PaymentProcessingGUIHelper.PostPaymentApprovals(new BusinessObject[3] { paymentApproval1, paymentApproval2, paymentApproval3 });
			AssertEquals("Expect same number of bank transfer forms prompted with FundingBankAccounts.", isDifferentAccount ? 3 : 2, PaymentProcessingGUIHelper.bankTransferFormsPrompted.Count);
			AssertEquals(typeof(BankTransferForm), PaymentProcessingGUIHelper.bankTransferFormsPrompted[0].GetType());

			PaymentProcessingGUIHelper.bankTransferFormsPrompted.ForEach(x => x.Dispose());
			PaymentProcessingGUIHelper.bankTransferFormsPrompted.Clear();
		}

		#endregion

		#region Test Display Deal Error messages before Batch is Saved

		public void TestDisplayDealErrorMessagesBeforeBatchIsSaved_PaymentTypeNotEPayment_NoMessage()
		{
			var payment = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			// just try a couple
			foreach (var paymentType in new string[] { ReceiptTypes.Cheque, ReceiptTypes.CreditCard, ReceiptTypes.EFT })
			{
				var result = PaymentProcessingGUIHelper.DisplayDealErrorMessagesBeforeBatchIsSaved(new APPaymentApprovalWithoutAuthorisation[] { payment }, paymentType);
				AssertEquals("Return value", ContinueWithSave.Yes, result);
				AssertNull("No message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDisplayDealErrorMessagesBeforeBatchIsSaved_PaymentTypeEPaymentAndNoPayments_NoMessage()
		{
			var result = PaymentProcessingGUIHelper.DisplayDealErrorMessagesBeforeBatchIsSaved(Array.Empty<PaymentApprovalBase>(), ReceiptTypes.EPayment);
			AssertEquals("Return value", ContinueWithSave.Yes, result);
			AssertNull("No message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDisplayDealErrorMessagesBeforeBatchIsSaved_PaymentTypeEPaymentAndNoDeals_MessageShown()
		{
			SetupDataForBaseTest();
			BatchPostingHelper.SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPostingHelper.BatchPoster.APB_AB = bankAccount.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;

			var payment1 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[0];
			payment1.AV_ChequeOrReference = "0001";
			payment1.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var payment2 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[1];
			payment2.AV_ChequeOrReference = "0002";
			payment2.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var payment3 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[2];
			payment3.AV_ChequeOrReference = "0003";
			payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment3.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, payment1);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Pending, payment2);

			Factory.Save();
			var result = PaymentProcessingGUIHelper.DisplayDealErrorMessagesBeforeBatchIsSaved(new PaymentApprovalBase[] { payment1, payment2, payment3 }, ReceiptTypes.EPayment);

			AssertEquals("Return value", ContinueWithSave.No, result);

			var expectedMessage = $@"The following Payments cannot be processed:
{payment1.GetDescription()}
Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.
{payment2.GetDescription()}
Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.
{payment3.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.

These transactions cannot be posted

Alternatively, change the payment type from E-Payment to another payment type";
			AssertEquals("Correct Message displayed", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDisplayDealErrorMessagesBeforeBatchIsSaved_PaymentTypeEPaymentAndHaveDeals_NoMessage()
		{
			SetupDataForBaseTest();
			BatchPostingHelper.SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPostingHelper.BatchPoster.APB_AB = bankAccount.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;

			var payment1 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[0];
			payment1.AV_ChequeOrReference = "0001";
			payment1.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var payment2 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[1];
			payment2.AV_ChequeOrReference = "0002";
			payment2.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var payment3 = BatchPostingHelper.BatchPoster.PaymentApprovalCollection[2];
			payment3.AV_ChequeOrReference = "0003";
			payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment3.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, payment1);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.InProgress, payment2);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Paid, payment3);

			Factory.Save();

			var result = PaymentProcessingGUIHelper.DisplayDealErrorMessagesBeforeBatchIsSaved(BatchPostingHelper.BatchPoster.PaymentApprovalCollection.ToArray<PaymentApprovalBase>(), BatchPostingHelper.BatchPoster.APB_PaymentType);

			AssertEquals("Return value", ContinueWithSave.Yes, result);
			AssertNull("No message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
	}
}
