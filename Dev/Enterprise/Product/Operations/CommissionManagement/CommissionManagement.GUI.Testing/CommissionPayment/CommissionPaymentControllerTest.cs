using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	class CommissionPaymentControllerTest : TestCaseWithFactory
	{
		[TestDate(2002, 2, 2)]
		public void TestPromptUserForProcessPayment()
		{
			var grouping = Factory.NewWithValidTestData<AccCommissionLineGroup>();
			grouping.CommissionHeader.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			grouping.CommissionHeader.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var line1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var line2 = Factory.NewWithValidTestData<AccCommissionLine>();
			grouping.CommissionHeader.Lines.Add(line1);
			grouping.CommissionHeader.Lines.Add(line2);

			line1.CL0_GS_NKStaff = "AR";
			line1.CL0_RX_NKCommissionCurrency = "AUD";
			line1.CL0_RX_NKTransactionCurrency = "AUD";
			line1.CL0_TransactionAmount = 1000m;
			line1.CL0_CommissionType = "PCT";
			line1.CL0_RX_NKCommissionCurrency = "AUD";
			line1.CL0_TotalCommissionableAmount = 1000m;
			line1.CL0_ShareTotal = 1;
			line1.CL0_SharePortion = 1;
			line1.CL0_ShareCommissionAmount = 1000m;
			line1.CL0_EntityCommissionAmount = 100m;
			line1.CL0_EntityPercentage = 10;

			line2.CL0_GS_NKStaff = "AR";
			line2.CL0_RX_NKCommissionCurrency = "AUD";
			line2.CL0_RX_NKTransactionCurrency = "AUD";
			line2.CL0_TransactionAmount = 1000m;
			line2.CL0_CommissionType = "PCT";
			line2.CL0_RX_NKCommissionCurrency = "AUD";
			line2.CL0_TotalCommissionableAmount = 1000m;
			line2.CL0_ShareTotal = 1;
			line2.CL0_SharePortion = 1;
			line2.CL0_ShareCommissionAmount = 1000m;
			line2.CL0_EntityCommissionAmount = 100m;
			line2.CL0_EntityPercentage = 10;

			Factory.Save();

			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.CRQ_Staff1HasApproved = true;

			request.Items.AddNew().CRI_CL0 = line1.PK;
			var item1 = request.Items[0];
			item1.CRI_IsSelected = true;

			request.Items.AddNew().CRI_CL0 = line2.PK;
			var item2 = request.Items[0];
			item2.CRI_IsSelected = true;

			Factory.Save();

			using (var form = new ZForm(request))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, request);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Are you sure you want to flag these entity commissions as paid?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line1.CL0_PaidDateTimeUtc", ZDateTime.Empty, item1.CommissionLine.CL0_PaidDateTimeUtc);
					AssertEquals("line2.CL0_PaidDateTimeUtc", ZDateTime.Empty, item2.CommissionLine.CL0_PaidDateTimeUtc);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // for 'Are you sure' Dialog
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;  // for PrintTask Dialog
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment if print task cancelled", () =>
				{
					AssertType(typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("line1.CL0_PaidDateTimeUtc", ZDateTime.Empty, item1.CommissionLine.CL0_PaidDateTimeUtc);
					AssertEquals("line2.CL0_PaidDateTimeUtc", ZDateTime.Empty, item2.CommissionLine.CL0_PaidDateTimeUtc);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // for 'Are you sure' Dialog
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;  // for PrintTask Dialog
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Payment successfully processed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line1.CL0_PaidDateTimeUtc", new ZDateTime(2002, 2, 2), item1.CommissionLine.CL0_PaidDateTimeUtc);
					AssertEquals("line2.CL0_PaidDateTimeUtc", new ZDateTime(2002, 2, 2), item2.CommissionLine.CL0_PaidDateTimeUtc);
					AssertEquals("line1.CL0_PaidDateTimeUtcInfo.OriginalValue", new ZDateTime(2002, 2, 2), item1.CommissionLine.CL0_PaidDateTimeUtcInfo.OriginalValue);
					AssertEquals("line2.CL0_PaidDateTimeUtcInfo.OriginalValue", new ZDateTime(2002, 2, 2), item2.CommissionLine.CL0_PaidDateTimeUtcInfo.OriginalValue);
				});
			}
		}
		public void TestPromptUserForProcessPayment_ChecksSecurityAllowed()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.CRQ_Staff1HasApproved = true;
			var item1 = request.Items.AddNew();
			item1.CRI_IsSelected = true;
			item1.FillWithValidTestData();

			Factory.Save();

			Env.Security.CommissionProcessPayment.IsAllowed = false;

			using (var form = new ZForm(request))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, request);
				paymentController.PromptUserForProcessPayment();
				AssertEquals(Env.Security.CommissionProcessPayment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestPromptUserForProcessPayment_Obsolete()
		{
			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			approvalRequest.CRQ_Staff1HasApproved = true;
			var item = approvalRequest.Items.AddNew();
			item.FillWithValidTestData();
			item.CommissionLine.CL0_PaidDateTimeUtc = new ZDateTime(2001, 1, 1);
			item.CRI_IsSelected = true;

			Factory.Save();

			using (var form = new ZForm(approvalRequest))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, approvalRequest);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Cannot process payment because it contains entity commissions that have already been paid or canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line.CL0_PaidDateTimeUtc", new ZDateTime(2001, 1, 1), item.CommissionLine.CL0_PaidDateTimeUtc);
				});
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestPromptUserForProcessPayment_NoneSelected()
		{
			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var item = approvalRequest.Items.AddNew();
			item.FillWithValidTestData();
			item.CRI_IsSelected = false;

			Factory.Save();

			using (var form = new ZForm(approvalRequest))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, approvalRequest);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "No entity commissions were selected for payment.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line.CL0_PaidDateTimeUtc", ZDateTime.Empty, item.CommissionLine.CL0_PaidDateTimeUtc);
				});
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestPromptUserForProcessPayment_WithErrors()
		{
			var commissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var item = approvalRequest.Items.AddNew();
			item.CRI_CL0 = commissionLine.PK;
			item.CRI_IsSelected = true;
			item.ViewCommissionLine.AddRowError("Problem!");

			Factory.Save();

			using (var form = new ZForm(approvalRequest))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, approvalRequest);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Selected entity commission(s) have issues that must be fixed before they can be paid. Please reference the warning(s) for additional information.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line.CL0_PaidDateTimeUtc", ZDateTime.Empty, item.CommissionLine.CL0_PaidDateTimeUtc);
				});
			}

			var grouping = Factory.NewWithValidTestData<AccCommissionLineGroup>();

			var testObjCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjCreator.CreateChargeCode("DDD");
			chargeCode.AC_IsActive = false;
			grouping.CLG_AC = chargeCode.PK;

			commissionLine.CL0_ParentID = grouping.PK;
			commissionLine.CL0_ParentTableCode = AccCommissionLineGroupSchema.Constants.Prefix;

			item.ViewCommissionLine.RemoveRowError("Problem!");

			Factory.Save();

			using (var form = new ZForm(approvalRequest))
			{
				form.Show();

				var paymentController = new CommissionPaymentController(form, approvalRequest);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // for 'Are you sure' Dialog
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;  // for PrintTask Dialog
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Payment successfully processed.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestLocalCommissionPaymentSummaryTemplateExists()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var request = Factory.New<AccCommissionApprovalRequest>();
			using (var form = new ZForm())
			{
				var controller = new CommissionPaymentController(form, request);
				AssertNotNull("If this fails, make sure to save the Local CommissionPaymentSummaryTemplate in Documents.xml", controller.CommissionPaymentSummaryTemplate);
			}
		}

		public void TestGlobalCommissionPaymentSummaryTemplateExists()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var request = Factory.New<AccCommissionApprovalRequest>();
			using (var form = new ZForm())
			{
				var controller = new CommissionPaymentController(form, request);
				AssertNotNull("If this fails, make sure to save the Local CommissionPaymentSummaryTemplate in Documents.xml", controller.CommissionPaymentSummaryTemplate);
			}
		}
	}
}
