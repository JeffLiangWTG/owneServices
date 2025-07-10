using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	abstract class APInvoiceApprovalModuleTest : TransactionApprovalModuleTest<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public void TestMenuItems()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("&View", module.FormActionMenu[0].Text);
				AssertEquals("&New", module.FormActionMenu[1].Text);
				AssertEquals(2, module.FormActionMenu[1].MenuItems.Count);
				AssertEquals("New I&nvoice", module.FormActionMenu[1].MenuItems[0].Text);
				AssertEquals("New Cre&dit Note", module.FormActionMenu[1].MenuItems[1].Text);
				AssertEquals("&Edit", module.FormActionMenu[2].Text);
				AssertEquals("P&ost", module.FormActionMenu[3].Text);
				AssertEquals("&Cancel", module.FormActionMenu[4].Text);
				AssertEquals("&Preview", module.FormActionMenu[5].Text);
			}
		}

		public virtual void TestNew()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var menuItem = module.FormActionMenu.FindByText(NewMenuItemName, true);
					AssertNotNull(menuItem);

					CheckpointForNew.IsAllowed = false;
					AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						menuItem.PerformClick();
						AssertEquals(CheckpointForNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					});
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					CheckpointForNew.IsAllowed = true;
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("Form caption", "New Unapproved AP", lastForm.Text);
					var formBizo = (InvoicingBase)lastForm.BusinessEntity;
					Assert("Bizo in Editing context", formBizo.HasContext(APInvoiceChargesApprovalRequest.Context.Editing));
					lastForm.Close();
				}
			}
		}

		public virtual void TestEdit()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					AssertEquals(string.Format("Can't Edit request 1 - Only requests with 'Transaction' reference type can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public virtual void TestPost()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Post", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					var bizo2 = GetNewApprovalRequest("2");
					bizo2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					bizo2.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					AssertNoErrors(bizo2);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(2, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					AssertEquals("LastMessage", "To post multiple approval requests, they must all be 'Transaction' ref type.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public virtual void TestPrint()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Preview", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
					var bizo2 = GetNewApprovalRequest("2");
					bizo2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
					bizo1.RunPreSaveValidation();
					bizo2.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					AssertNoErrors(bizo2);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(2, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					AssertStartsWith("LastMessage", "To preview multiple approval requests, they must all be 'Transaction' ref type.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public void TestPrintMode()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var menuItem = module.FormActionMenu.FindByText("Preview", true);
					AssertNotNull(menuItem);

					var approvalRequest = GetNewApprovalRequest("1");
					approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
					InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
					{
						AssertEquals("Precondition: There should be 1 document in the pack", 1, printTask[0].Count);
						AssertNotEquals("Should not be preview mode", DeliveryInstructionDestination.Preview, printTask[0].DeliveryInstructions.Destination);
					};

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();
					menuItem.PerformClick();
				}
			}
		}

		#region Test Cancel

		public void TestCancelDuplicateRequestResponseYes()
		{
			Action messageAction = () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddOKAnswer();
			};

			Action assertAction = () =>
			{
				AssertEquals(@"Relative invalid requests are canceled. Their request IDs are: 00001000", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval Request cancelled.", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, postedRequest.XP_ApprovalStatus);
				AssertEquals("Linked transaction unchanged.", parentIdForRequest, approvalRequest.XP_ParentID);
				AssertEquals(parentIdForRequest, postedRequest.XP_ParentID);
			};

			TestCancelInvalidRequestResponseCore(messageAction, assertAction);
		}

		public void TestCancelDuplicateRequestResponseInBatch()
		{
			var requests1 = GetNewApprovalRequestsWithSameSource("1", ApprovalInvoiceType.Cancelled);
			var requests2 = GetNewApprovalRequestsWithSameSource("2", ApprovalInvoiceType.Posted);
			var requests3 = GetNewApprovalRequestsWithSameSource("3", ApprovalInvoiceType.Posted);
			var requests4 = GetNewApprovalRequestsWithSameSource("4", ApprovalInvoiceType.Unposted);
			var requests5 = GetNewApprovalRequestsWithSameSource("5", ApprovalInvoiceType.Posted, 6);

			var request1_approved = requests1[0];
			request1_approved.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var request1_posted = requests1[1];
			request1_posted.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

			var request2_approved = requests2[0];
			request2_approved.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var request2_posted = requests2[1];
			request2_posted.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

			var request3_approved1 = requests3[0];
			request3_approved1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var request3_approved2 = requests3[1];
			request3_approved2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			var request4_approved1 = requests4[0];
			request4_approved1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var request4_approved2 = requests4[1];
			request4_approved2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			var request5_approved = requests5[0];
			request5_approved.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var request5_posted = requests5[1];
			request5_posted.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			var request5_rejected = requests5[2];
			request5_rejected.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var request5_requested = requests5[3];
			request5_requested.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			var request5_error = requests5[4];
			request5_error.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			var request5_cancelled = requests5[5];
			request5_cancelled.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

			var request6_approved = GetNewApprovalRequestsWithSameSource("6", ApprovalInvoiceType.Posted, 1)[0];
			request6_approved.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			var request7_posted = GetNewApprovalRequestsWithSameSource("7", ApprovalInvoiceType.Posted, 1)[0];
			request7_posted.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

			Factory.Save();

			try
			{
				using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						module.AddAdditionalDisplayFilter = (query) =>
						{
							query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Constants.GenApprovalRequestApprovalStatus.Cancelled);
							query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Constants.GenApprovalRequestApprovalStatus.Posted);
						};

						((IFilterModuleInternalsForTesting)module).PerformSearch();

						var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true)[0] as ZGrid;
						grid.SelectAllElements();
						AssertEquals(11, grid.SelectedElements.Length);

						var methodInfo = module.GetType().GetMethod("HandleCancel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						methodInfo.Invoke(module, new object[] { this, new EventArgs() });

						if (IsCancelInvalidRequestApplied)
						{
							AssertEquals(@"Batch Cancel Aborted.
Some approvals are invalid and fixed: 1, 2, 3, 4, 5, 6
You may select the rest approvals in the batch, and cancel them manually.
", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertEquals("Invalid requests get cancelled.",
										 Constants.GenApprovalRequestApprovalStatus.Cancelled, request1_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request2_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request3_approved1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request3_approved2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request4_approved1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request4_approved2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request5_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request5_rejected.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request5_requested.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request5_error.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request6_approved.XP_ApprovalStatus);

							AssertEquals("Status of valid requests remain unchanged.",
										 Constants.GenApprovalRequestApprovalStatus.Posted, request1_posted.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, request2_posted.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request5_cancelled.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, request5_posted.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, request7_posted.XP_ApprovalStatus);
						}
						else
						{
							AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("Approval Request unchanged.",
										 Constants.GenApprovalRequestApprovalStatus.Approved, request1_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request2_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request3_approved1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request3_approved2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request4_approved1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request4_approved2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request5_approved.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request6_approved.XP_ApprovalStatus);
						}
					}
				}
			}
			finally
			{
				if (ZFormModaliser.LastFormShownForTest != null)
				{
					ZFormModaliser.LastFormShownForTest.Dispose();
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestCancelDuplicateRequestResponseNo()
		{
			Action messageAction = () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddOKAnswer();
			};

			Action assertAction = () =>
			{
				AssertEquals("Transaction '1' linked with the request '00001000' is already posted or canceled. Do you prefer system to cancel all relative invalid requests?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request status unchanged if click NO to auto-fix.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, postedRequest.XP_ApprovalStatus);
				AssertEquals("Parent transaction unchanged.", parentIdForRequest, approvalRequest.XP_ParentID);
				AssertEquals(parentIdForRequest, postedRequest.XP_ParentID);
			};

			TestCancelInvalidRequestResponseCore(messageAction, assertAction);
		}

		APInvoiceChargesApprovalRequest approvalRequest, postedRequest;
		ZGuid parentIdForRequest;

		void TestCancelInvalidRequestResponseCore(Action messageAction, Action assertAction)
		{
			var requests = GetNewApprovalRequestsWithSameSource("1", ApprovalInvoiceType.Posted);

			approvalRequest = requests[0];
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			postedRequest = requests[1];
			postedRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

			parentIdForRequest = approvalRequest.XP_ParentID;

			Factory.Save();

			try
			{
				using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						module.AddAdditionalDisplayFilter = (query) => query.AddToFilter(GenApprovalRequestSchema.PK, approvalRequest.PK);
						((IFilterModuleInternalsForTesting)module).PerformSearch();

						var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true)[0] as ZGrid;
						grid.SelectAllElements();
						AssertEquals(1, grid.SelectedElements.Length);

						var methodInfo = module.GetType().GetMethod("HandleCancel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

						messageAction();

						methodInfo.Invoke(module, new object[] { this, new EventArgs() });

						if (IsCancelInvalidRequestApplied)
						{
							assertAction();
						}
						else
						{
							GetDefaultAssertActionForInvalidRequest()();
						}
					}
				}
			}
			finally
			{
				if (ZFormModaliser.LastFormShownForTest != null)
				{
					ZFormModaliser.LastFormShownForTest.Dispose();
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		protected abstract APInvoiceChargesApprovalRequest[] GetNewApprovalRequestsWithSameSource(string jobNumber, ApprovalInvoiceType type, int numberOfCopy = 2);
		protected enum ApprovalInvoiceType
		{
			Posted,
			Unposted,
			Cancelled
		}

		protected virtual bool IsCancelInvalidRequestApplied => true;

		Action GetDefaultAssertActionForInvalidRequest()
		{
			return () =>
			{
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval Request unchanged.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, postedRequest.XP_ApprovalStatus);
				AssertEquals("Linked transaction unchanged.", parentIdForRequest, approvalRequest.XP_ParentID);
				AssertEquals(parentIdForRequest, postedRequest.XP_ParentID);
			};
		}

		#endregion

		protected void PopulateCharge(Charge charge, ZGuid jobPK, ZGuid chargeCodePK, ZGuid debtor, ZGuid creditor, ZString apInvoiceNumber, ZDateTime invoiceDate)
		{
			charge.JR_JH = jobPK;
			charge.JR_AC = chargeCodePK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_OSSellAmt = 100m;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_OSCostAmt = 100m;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_OH_CostAccount = creditor;
			charge.JR_OH_SellAccount = debtor;
			charge.JR_APInvoiceNum = apInvoiceNumber;
			charge.JR_APInvoiceDate = invoiceDate;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APInvoiceApproval;
		}

		protected override void SetupSecurity(bool allowSecurity)
		{
			Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = allowSecurity;
			Env.Security.APInvoiceApproval_SecondApproval.IsAllowed = allowSecurity;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		protected override Type ApprovalBulklastFormType
		{
			get { return typeof(APInvoiceChargesApprovalBulkForm); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override string ExpectedUserDoesntHaveCancelRightMessage
		{
			get
			{
				return @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Invoice Approval -> Cancel";
			}
		}

		protected override SecurityCheckpoint CheckpointForCancel
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		protected abstract SecurityCheckpoint CheckpointForNew { get; }

		protected abstract string NewMenuItemName { get; }

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => true;
	}

	abstract class APInvoiceApprovalModuleForTransactionLinkedApprovalsTest : APInvoiceApprovalModuleTest
	{
		public override void TestEdit()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButRequested = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Constants.GenApprovalRequestApprovalStatus.Requested);

					foreach (var status in allStatusesButRequested)
					{
						bizo1.XP_ApprovalStatus = status;
						if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
						{
							bizo1.SetContext(BusinessContext.CancelApprovalRequestByUser);
						}
						Factory.Save();
						menuItem.PerformClick();
						AssertEquals(string.Format("Can't Edit request (Creditor: ZLOCCLT, Transaction Number: 1) - Only requests with status 'Requested' can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();
					Env.Security.APInvoiceApproval_Edit_DirectEntered.IsAllowed = false;
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("When user don't have rights View form should be shown", "View Unapproved AP", lastForm.Text);
					lastForm.Close();

					Env.Security.APInvoiceApproval_Edit_DirectEntered.IsAllowed = true;
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("Form caption", "Edit Unapproved AP", lastForm.Text);
					var formBizo = (InvoicingBase)lastForm.BusinessEntity;
					Assert("Bizo in Editing context", formBizo.HasContext(APInvoiceChargesApprovalRequest.Context.Editing));
					Assert("IsIncompleteInvoice", formBizo.IsIncompleteInvoice);
					AssertEquals("Invoice is restored", 1, formBizo.Lines.Count);
					lastForm.Close();

					var linkedInvoice = Factory.Load<TransactionHeader>(bizo1.XP_ParentID);
					linkedInvoice.DeleteFromDB();
					Factory.Save();
					module.LastController_ForTestOnly = null;
					menuItem.PerformClick();
					AssertEquals(string.Format("A transaction can't be found for request (Creditor: ZLOCCLT, Transaction Number: 1)."), UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public override void TestPost()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Post", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButApproved = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Constants.GenApprovalRequestApprovalStatus.Approved);

					foreach (var status in allStatusesButApproved)
					{
						bizo1.XP_ApprovalStatus = status;
						if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
						{
							bizo1.SetContext(BusinessContext.CancelApprovalRequestByUser);
						}
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertEquals("LastMessage",
@"The following request(s) cannot be posted.
Request (Creditor: ZLOCCLT, Transaction Number: 1) - Only requests with status 'Approved' can be posted.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					Factory.Save();
					Env.Security.APInvoiceApproval_Post.IsAllowed = false;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Invoice Approval -> Post",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("When user don't have rights form should not be shown", module.LastController_ForTestOnly);

					Env.Security.APInvoiceApproval_Post.IsAllowed = true;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("Form caption", "Post AP", lastForm.Text);
					var formBizo = (InvoicingBase)lastForm.BusinessEntity;
					Assert("Bizo in Posting context", formBizo.HasContext(APInvoiceChargesApprovalRequest.Context.Posting));
					Assert("IsIncompleteInvoice", formBizo.IsIncompleteInvoice);
					AssertEquals("Invoice is restored", 1, formBizo.Lines.Count);
					AssertNotEquals("Invoice should not be in the module factory.", module.GridCollection.Factory._Instance, formBizo.Factory._Instance);
					lastForm.Close();

					var linkedInvoice = Factory.Load<TransactionHeader>(bizo1.XP_ParentID);
					linkedInvoice.DeleteFromDB();
					Factory.Save();
					module.LastController_ForTestOnly = null;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"The following request(s) cannot be posted.
A transaction can't be found for request (Creditor: ZLOCCLT, Transaction Number: 1).
",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public override void TestPrint()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Preview", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allValidStatuses = new[] { Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Posted };
					var allInvalidStatuses = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => !allValidStatuses.Contains(x));

					module.ParentFormForInvoicePreview_ForTestOnly = form;

					foreach (var status in allInvalidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
						{
							bizo1.SetContext(BusinessContext.CancelApprovalRequestByUser);
						}
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertEquals("LastMessage",
@"The following request(s) cannot be previewed.
Request (Creditor: ZLOCCLT, Transaction Number: 1) - Only approvals with status 'REQ', 'APP', 'PST' can be previewed.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					Factory.Save();
					Env.Security.APInvoiceApproval_Print.IsAllowed = false;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Invoice Approval -> Print",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("When user don't have rights form should not be shown", module.LastController_ForTestOnly);

					Env.Security.APInvoiceApproval_Print.IsAllowed = true;
					var printer = new BusinessObjectFactory().New<StmPrintQueue>();
					printer.Factory.Save();

					var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, new ZGuid(InvoicePrintTask.MenuPkForAPInvoice)));
					var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
					defaultPrinter.SDP_SQ_Printer = printer.PK;
					defaultPrinter.SDP_NumberOfCopies = 1;
					Factory.Save();

					var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
					mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(
						It.IsAny<PrintTask>(),
						It.IsAny<DeliveryInstructions>(),
						It.IsAny<ISecurityCheckpoint>()))
						.Returns(false);
					using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
					{
						foreach (var status in allValidStatuses)
						{
							var printTaskRan = false;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
							InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
							InvoicePrintHelper.PrintTaskRun_ForTestOnly += printTask =>
							{
								printTaskRan = true;
								AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
								AssertEquals("Cost Confirmation Document", printTask[0][0].Name);
								var report = printTask[0][0] as Report;
								AssertNotNull("Report", report);
								var printingBizo = report.BODocDataProvider.ParentBusinessObject as InvoicingBase;
								AssertNotNull("Report ParentBusinessObject", printingBizo);
								AssertNotEquals("Invoice should not be in the module factory.", module.GridCollection.Factory._Instance, printingBizo.Factory._Instance);
								Assert("IsIncompleteInvoice", printingBizo.IsIncompleteInvoice);
								AssertEquals("Invoice is restored", 1, printingBizo.Lines.Count);
								Assert("Invoice should not be with incomplete ledger to allow to be printed", printingBizo.IsCompletingInvoice);
								Assert("Unapproved invoice should not be marked as be printed", !printingBizo.AH_InvoicePrinted);
							};
							grid.SelectAllElements();
							menuItem.PerformClick();
							InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
							Assert("Print task should be run", printTaskRan);
						}
					}

					var linkedInvoice = Factory.Load<TransactionHeader>(bizo1.XP_ParentID);
					linkedInvoice.DeleteFromDB();
					Factory.Save();
					module.LastController_ForTestOnly = null;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"The following request(s) cannot be previewed.
A transaction can't be found for request (Creditor: ZLOCCLT, Transaction Number: 1).
",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
					mockIPrintTaskUIProvider.VerifyAll();
				}
			}
		}
	}

	[TestedType(typeof(APInvoiceApprovalModule))]
	class APInvoiceApprovalModuleForJobLinkedApprovalsTest : APInvoiceApprovalModuleTest
	{
		public override void TestNew()
		{
			Assert("Job linked request doesn't support new action from module", true);
		}

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber, "AUSYD", "AUMEL");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", null, 100m, TestObjectCreator.AALSHI, "123456", null, 100m, TestObjectCreator.Debtor);

			APInvoiceCharges apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.AALSHI.OH_Code, "123456", ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.Add(charge1);
			var approvalRequest = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			approvalRequest.InitializeJobRelated(apInvoiceCharges, job.PK, job.TablePrefix);
			approvalRequest.XP_ReasonDescription = "Desc";
			approvalRequest.XP_ReasonCode = "DAM";

			return approvalRequest;
		}

		protected override APInvoiceChargesApprovalRequest[] GetNewApprovalRequestsWithSameSource(string jobNumber, ApprovalInvoiceType type, int numberOfCopy = 2)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber, "AUSYD", "AUMEL");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", null, 100m, TestObjectCreator.AALSHI, "123456", null, 100m, TestObjectCreator.Debtor);

			var apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.AALSHI.OH_Code, "123456", ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.Add(charge1);

			APInvoiceChargesApprovalRequest request = null;
			var result = new APInvoiceChargesApprovalRequest[numberOfCopy];

			for (int i = 0; i < numberOfCopy; i++)
			{
				request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
				request.InitializeJobRelated(apInvoiceCharges, job.PK, job.TablePrefix);
				request.XP_ReasonDescription = "Desc" + (i + 1);
				request.XP_ReasonCode = "DAM";
				result[i] = request;
			}

			return result;
		}

		protected override bool IsCancelInvalidRequestApplied => false;

		Charge charge1;

		public void TestPost_Extended()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Post", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					TestObjectCreator.CreateTestPeriods(ZDate.Today);

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButApproved = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Constants.GenApprovalRequestApprovalStatus.Approved);

					foreach (var status in allStatusesButApproved)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertEquals("LastMessage",
@"The following request(s) cannot be posted.
Request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - Only requests with status 'Approved' can be posted.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					Factory.Save();
					Env.Security.APInvoiceApproval_Post.IsAllowed = false;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Invoice Approval -> Post",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("When user don't have rights form should not be shown", module.LastController_ForTestOnly);

					Env.Security.APInvoiceApproval_Post.IsAllowed = true;
					ChangeJobDetails(false);
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("Can't post this request because source details have been modified since then.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("IsPosted", !IsPosted);
					AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, bizo1.XP_ApprovalStatus);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);

					ChangeJobDetails(false, makeCreditNote: true);
					menuItem.PerformClick();
					AssertEquals("Nothing to post as there are no charges valid for posting with the request Creditor and Invoice Number.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("IsPosted", !IsPosted);
					AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, bizo1.XP_ApprovalStatus);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);

					ChangeJobDetails(true);
					menuItem.PerformClick();
					AssertEquals("Nothing to post as there are no charges valid for posting with the request Creditor and Invoice Number.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("IsPosted", !IsPosted);
					AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, bizo1.XP_ApprovalStatus);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);

					ChangeJobDetails(false, restoreDetailsOnly: true);
					var initialParentId = bizo1.XP_ParentID;
					bizo1.XP_ParentID = ZGuid.NewZGuid();
					Factory.Save();
					grid.SelectAllElements();
					menuItem.PerformClick();
					var expectedMessage = bizo1.IsJobRelated ?
@"The following request(s) cannot be posted.
Request (Job: , Creditor: AALSHI, Transaction Number: 123456) - Job was not found.
" :
@"The following request(s) cannot be posted.
Request (Job: , Creditor: AALSHI, Transaction Number: 123456) - Consol was not found.
";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("IsPosted", !IsPosted);
					AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, bizo1.XP_ApprovalStatus);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);

					bizo1.XP_ParentID = initialParentId;
					Factory.Save();
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("IsPosted", IsPosted);
					AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, bizo1.XP_ApprovalStatus);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				}
			}
		}

		public void TestPrint_Extended()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				SetupSecurity(allowSecurity: false);
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Preview", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					TestObjectCreator.CreateTestPeriods(ZDate.Today);

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allValidStatuses = new[] { Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Requested };
					var allInvalidStatuses = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => !allValidStatuses.Contains(x));

					module.ParentFormForInvoicePreview_ForTestOnly = form;

					foreach (var status in allInvalidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertEquals("LastMessage",
@"The following request(s) cannot be previewed.
Request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - Only approvals with status 'REQ', 'APP' can be previewed.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					Factory.Save();
					Env.Security.APInvoiceApproval_Print.IsAllowed = false;
					menuItem.PerformClick();
					AssertEquals("LastMessage",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Invoice Approval -> Print",
							UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("When user don't have rights form should not be shown", module.LastController_ForTestOnly);

					Env.Security.APInvoiceApproval_Print.IsAllowed = true;
					var printer = new BusinessObjectFactory().New<StmPrintQueue>();
					printer.Factory.Save();

					foreach (var status in allValidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertType("LastFormShownDialogForTest", typeof(CostConfirmationDocTypePopupForm), ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest = null;
						AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}

					ChangeJobDetails(false);
					ZFormModaliser.LastFormShownForTest = null;

					foreach (var status in allValidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertNull("CostConfirmationDocTypePopupForm must NOT be shown", ZFormModaliser.LastFormShownDialogForTest);
						AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
						AssertEquals(@"The following request(s) cannot be previewed.
Request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - Can't preview this request because source details have been modified since then.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					ChangeJobDetails(false, makeCreditNote: true);
					foreach (var status in allValidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertNull("CostConfirmationDocTypePopupForm must NOT be shown", ZFormModaliser.LastFormShownDialogForTest);
						AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
						AssertEquals(@"The following request(s) cannot be previewed.
Request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - No preview available as there are no charges valid for posting with the request Creditor and Invoice Number.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					ChangeJobDetails(true);
					foreach (var status in allValidStatuses)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertNull("CostConfirmationDocTypePopupForm must NOT be shown", ZFormModaliser.LastFormShownDialogForTest);
						AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
						AssertEquals(@"The following request(s) cannot be previewed.
Request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - No preview available as there are no charges valid for posting with the request Creditor and Invoice Number.
",
							UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					var initialParentId = bizo1.XP_ParentID;
					bizo1.XP_ParentID = ZGuid.NewZGuid();
					Factory.Save();
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertNull("CostConfirmationDocTypePopupForm must NOT be shown", ZFormModaliser.LastFormShownDialogForTest);
					AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
					var expectedMessage = bizo1.IsJobRelated ?
@"The following request(s) cannot be previewed.
Request (Job: , Creditor: AALSHI, Transaction Number: 123456) - Job was not found.
" :
@"The following request(s) cannot be previewed.
Request (Job: , Creditor: AALSHI, Transaction Number: 123456) - Consol was not found.
";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					bizo1.XP_ParentID = initialParentId;
					Factory.Save();
				}
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestRejectWithConcurrencyPost()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var approvalToReject = GetNewApprovalRequest("S001");
			approvalToReject.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approvalToReject.RunPreSaveValidation();
			AssertNoErrors(approvalToReject);
			Factory.Save();
			AssertEquals("In CW1 instance1 there is a approval request with status REQ", Constants.GenApprovalRequestApprovalStatus.Requested, approvalToReject.XP_ApprovalStatus);

			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var menuItem = module.FormActionMenu.FindByText("Reject", true);
					AssertNotNull(menuItem);
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var approval = newFactory.Load<APInvoiceChargesApprovalRequest>(approvalToReject.PK);
					if (approval == null)
					{
						Assert("UT does not support this ApprovalRequest type.", true);
						return;
					}
					approval.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
					newFactory.Save();

					var postingErrorMessage = PostManagerGUIWrapper.PostRequest(approval);
					AssertNullOrEmpty("In CW1 instance2, the transaction is successfully approved and post", postingErrorMessage);
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

					((APInvoiceChargesApprovalRequest)module.GridCollection[0]).XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Requested;

					grid.SelectAllElements();
					menuItem.PerformClick();

					AssertEquals("Should not allow to reject", "Can't Reject - These approvals are NOT in allowed REQ status: S001", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		protected virtual void ChangeJobDetails(bool invoiceDetails, bool makeCreditNote = false, bool restoreDetailsOnly = false)
		{
			#region Original values from GetNewApprovalRequest

			charge1.JR_APInvoiceNum = "123456";
			charge1.JR_OSCostAmt = 100;

			#endregion

			if (!restoreDetailsOnly)
			{
				if (invoiceDetails)
				{
					charge1.JR_APInvoiceNum = "987654";
				}
				else
				{
					var multiplier = makeCreditNote ? -1 : 1;
					charge1.JR_OSCostAmt = 321 * multiplier;
				}
			}
			Factory.Save();
		}

		protected virtual bool IsPosted
		{
			get { return charge1.IsCostPosted; }
		}

		public override void TestEdit()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButRequested = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Constants.GenApprovalRequestApprovalStatus.Requested);

					foreach (var status in allStatusesButRequested)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						menuItem.PerformClick();
						AssertEquals(string.Format("Can't Edit request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - Only requests with status 'Requested' can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();

					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					AssertEquals("Shipment Form Should be opened", "Enterprise.Freight.Forwarding.GUI.ShipmentForm", module.LastController_ForTestOnly.LastShownForm.GetType().ToString());

					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertEquals("Shipment Form Should be opened", typeof(ForwardingShipment), lastForm.BusinessEntity.GetType());
					AssertEquals("Shipment Form Text", "Edit Shipment 1", lastForm.Text);
					lastForm.Close();
				}
			}
		}

		protected override SecurityCheckpoint CheckpointForNew
		{
			get { return null; }
		}

		protected override string NewMenuItemName
		{
			get { return null; }
		}
	}

	[TestedType(typeof(APInvoiceApprovalModule))]
	class APInvoiceApprovalModuleForConsolLinkedApprovalsTest : APInvoiceApprovalModuleForJobLinkedApprovalsTest
	{
		public override void TestEdit()
		{
			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButRequested = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Constants.GenApprovalRequestApprovalStatus.Requested);

					foreach (var status in allStatusesButRequested)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						menuItem.PerformClick();
						AssertEquals(string.Format("Can't Edit request (Job: 1, Creditor: AALSHI, Transaction Number: 123456) - Only requests with status 'Requested' can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();

					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					AssertEquals("Shipment Form Should be opened", "Enterprise.Freight.Forwarding.GUI.ConsolForm", module.LastController_ForTestOnly.LastShownForm.GetType().ToString());

					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertEquals("Shipment Form Should be opened", typeof(ForwardingConsol), lastForm.BusinessEntity.GetType());
					AssertEquals("Shipment Form Text", "Edit Consol 1", lastForm.Text);
					lastForm.Close();
				}
			}
		}

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			var consol = PrepareNewApprovalRequestConsolDataContext(jobNumber);
			APInvoiceCharges apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.AALSHI.OH_Code, "123456", ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.AddRange(Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, cost.ApportionmentCharges.GetPKs())));

			var approvalRequest = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			approvalRequest.InitializeJobRelated(apInvoiceCharges, consol.PK, consol.TablePrefix);
			approvalRequest.XP_ReasonDescription = "Desc";
			approvalRequest.XP_ReasonCode = "DAM";

			return approvalRequest;
		}

		protected override APInvoiceChargesApprovalRequest[] GetNewApprovalRequestsWithSameSource(string jobNumber, ApprovalInvoiceType type, int numberOfCopy = 2)
		{
			var consol = PrepareNewApprovalRequestConsolDataContext(jobNumber);
			APInvoiceCharges apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.AALSHI.OH_Code, "123456", ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.AddRange(Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, cost.ApportionmentCharges.GetPKs())));

			var result = new APInvoiceChargesApprovalRequest[numberOfCopy];
			APInvoiceChargesApprovalRequest request = null;

			for (int i = 0; i < numberOfCopy; i++)
			{
				request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
				request.InitializeJobRelated(apInvoiceCharges, consol.PK, consol.TablePrefix);
				request.XP_ReasonDescription = "Desc" + (i + 1);
				request.XP_ReasonCode = "DAM";
				result[i] = request;
			}

			return result;
		}

		protected override bool IsCancelInvalidRequestApplied => false;

		ForwardingConsol PrepareNewApprovalRequestConsolDataContext(string jobNumber)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", jobNumber);
			var shipment1 = TestObjectCreator.CreateShipment("S1" + jobNumber, consol);
			var shipment2 = TestObjectCreator.CreateShipment("S2" + jobNumber, consol);
			Job job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Job job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var apps = new ApportionmentListing(Factory, consol);
			cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC8.PK;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "123456";
			cost.E6_InvoiceDate = ZDateTime.Now.AddDays(10);
			cost.E6_PaymentDate = ZDateTime.Now.AddDays(25);
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 200m;

			Factory.Save();

			return consol;
		}

		protected override void ChangeJobDetails(bool invoiceDetails, bool makeCreditNote = false, bool restoreDetailsOnly = false)
		{
			#region Original values from GetNewApprovalRequest

			cost.E6_InvoiceNum = "123456";
			cost.E6_OSCostAmount = 200m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 100m;

			#endregion

			if (!restoreDetailsOnly)
			{
				if (invoiceDetails)
				{
					cost.E6_InvoiceNum = "3123";
				}
				else
				{
					var multiplier = makeCreditNote ? -1 : 1;
					cost.E6_OSCostAmount = 200m * multiplier;
					cost.ApportionmentCharges[0].JR_OSCostAmt = 50m * multiplier;
					cost.ApportionmentCharges[1].JR_OSCostAmt = 150m * multiplier;
				}
			}
			Factory.Save();
		}

		protected override bool IsPosted
		{
			get { return cost.IsPosted; }
		}

		JobConsolCost cost;
	}

	[TestedType(typeof(APInvoiceApprovalModule))]
	class APInvoiceApprovalModuleForInvoiceLinkedApprovalsTest : APInvoiceApprovalModuleForTransactionLinkedApprovalsTest
	{
		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			return TestObjectCreator.CreateApprovalRequestWithLinkedInvoice<APInvoice>(TestObjectCreator.LocalClient, 100, jobNumber);
		}

		protected override APInvoiceChargesApprovalRequest[] GetNewApprovalRequestsWithSameSource(string jobNumber, ApprovalInvoiceType type, int numberOfCopy = 2)
		{
			var testObjectCreator = type == ApprovalInvoiceType.Posted ? new TestObjectCreator(new BusinessObjectFactory()) : TestObjectCreator;
			var invoice = testObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.LocalClient, 100, jobNumber);

			var result = new APInvoiceChargesApprovalRequest[numberOfCopy];
			APInvoiceChargesApprovalRequest request = null;

			for (int i = 0; i < numberOfCopy; i++)
			{
				request = Factory.New<APInvoiceChargesApprovalRequest>();
				request.InitializeInvoiceRelated(invoice);
				request.XP_ReasonDescription = "Desc" + (i + 1);
				request.XP_ReasonCode = "DAM";
				request.PrepareFoSaving();
				result[i] = request;
			}

			if (type == ApprovalInvoiceType.Posted)
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				IncompleteInvoiceBOIsSavedByFactoryServiceProvider.DeregisterInvoiceFromSaveOnlyInvoiceHeader(invoice);
				testObjectCreator.Factory.Save();
			}
			else if (type == ApprovalInvoiceType.Cancelled)
			{
				invoice.IsCancelled = true;
			}

			return result;
		}

		protected override SecurityCheckpoint CheckpointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewInvoice; }
		}

		protected override string NewMenuItemName
		{
			get { return "New Invoice"; }
		}

		[TestDate(2016, 10, 20)]
		public void TestCancelWhenIncompleteInvoiceIsTriedToBePosted()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
			var approvalRequest = TestObjectCreator.CreateApprovalRequestWithLinkedInvoice<APInvoice>(TestObjectCreator.TestOrganisation, 100, "123456");
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var loadedInvoice = Factory.Load<APInvoice>(approvalRequest.XP_ParentID);
			loadedInvoice.RestoreSavedData();
			loadedInvoice.MoveFromIncompleteToPayableLedger();
			AssertEquals(true, loadedInvoice.IsCompletingInvoice);
			loadedInvoice.SubmittedFromInvoicingForm = true;
			using (var form = new BaseInvoicingForm(loadedInvoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				loadedInvoice.RunPreSaveValidation();
				AssertEquals("Precondition: FireSaveButton", ContinueWithSave.No, form.FireSaveButton());
				AssertHasRowError("should have a row error", loadedInvoice, "This transaction is linked to approval request but ‘Enable APInvoice Approval’ registry has been set to ‘No’. Please raise an eRequest to request assistance to set this registry to ‘Yes’ before posting the transaction.");
			}

			using (var module = (APInvoiceApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (var form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, ((APInvoiceChargesApprovalRequest)module.GridCollection[0]).XP_ApprovalStatus);
				}
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}

	[TestedType(typeof(APInvoiceApprovalModule))]
	class APInvoiceApprovalModuleForCreditNoteLinkedApprovalsTest : APInvoiceApprovalModuleForTransactionLinkedApprovalsTest
	{
		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			return TestObjectCreator.CreateApprovalRequestWithLinkedInvoice<APCreditNote>(TestObjectCreator.LocalClient, 100, jobNumber);
		}

		protected override APInvoiceChargesApprovalRequest[] GetNewApprovalRequestsWithSameSource(string jobNumber, ApprovalInvoiceType type, int numberOfCopy = 2)
		{
			var testObjectCreator = type == ApprovalInvoiceType.Posted ? new TestObjectCreator(new BusinessObjectFactory()) : TestObjectCreator;
			var invoice = testObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(TestObjectCreator.LocalClient, 100, jobNumber);

			var result = new APInvoiceChargesApprovalRequest[numberOfCopy];
			APInvoiceChargesApprovalRequest request = null;

			for (int i = 0; i < numberOfCopy; i++)
			{
				request = Factory.New<APInvoiceChargesApprovalRequest>();
				request.InitializeInvoiceRelated(invoice);
				request.XP_ReasonDescription = "Desc" + (i + 1);
				request.XP_ReasonCode = "DAM";
				request.PrepareFoSaving();
				result[i] = request;
			}

			if (type == ApprovalInvoiceType.Posted)
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				IncompleteInvoiceBOIsSavedByFactoryServiceProvider.DeregisterInvoiceFromSaveOnlyInvoiceHeader(invoice);
				testObjectCreator.Factory.Save();
			}
			else if (type == ApprovalInvoiceType.Cancelled)
			{
				invoice.IsCancelled = true;
			}

			return result;
		}

		protected override SecurityCheckpoint CheckpointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewCreditNote; }
		}

		protected override string NewMenuItemName
		{
			get { return "New Credit Note"; }
		}
	}
}
