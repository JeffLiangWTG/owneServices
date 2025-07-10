using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PayableOrder.Testing
{
	[TestedType(typeof(AccPayableOrderForm))]
	public class TestAccPayableOrderForm : ZFormBasherTest
	{
		public void TestApprove()
		{
			SetOrderApprovalRegistryRestrictions(false);
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			Factory.Save();
			using (AccPayableOrderForm form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();

				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				approveMenuItem.PerformClick();

				AssertEquals(@"This Purchase Order's disposition is not 'Pending Approval' due to one of the below mentioned reasons:  
* No order lines created for this order or
* No changes made to order line values of existing order lines that have already been approved", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPost()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.DeliveryInProgress;
			order.APH_GoodsDescription = "Test";
			order.APH_InvoiceNumber = "I5456454";
			order.APH_InvoiceDate = ZDate.Today;
			order.APH_DueDate = ZDate.Today;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			order.SupplierDocumentaryAddress.OrganisationPK = org.PK;

			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 1;
			line.APL_ItemPrice = 100;
			line.APL_QtyInvoiced = 2;

			Factory.Save();
			using (AccPayableOrderForm form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();

				var approveMenuItem = GetActionsMenuItem(form, "Post Accounts Payable Invoice");
				approveMenuItem.PerformClick();

				var expectedMessage = "The Invoiced amount may not be more than the approved Order Line Price";
				AssertEquals("Should show error message as invoiced amount is greater than line price", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApproveOrderInitiatorAllowedToApprove()
		{
			SetOrderApprovalRegistryRestrictions(false);
			SetUpRegistryForPurchaseOrderApprovalTest();
			var initiator = TestObjectCreator.CreateStaffWithSecurityRights("Thang Do", "MTD", Env.Security.PayableOrderApprovalSecondLevelApproval.Code);

			using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				// Purchase order, which requires Second Level Approval
				// Any orders require the same approval level and below are self-approvable in this context
				var order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10000m);
				AssertEquals("Order has been successfully approved.", SimulateExpectSuccess(order));

				// Purchase order, which requires Third Level Approval
				// Any orders require higher approval level are NOT self-approvable in this context
				order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(11000m);
				SimulateExpectErrorWithLoginPrompt(@"This transaction must be approved by a user with Third Level Approval authority.
If a user with this level of authority enters their username and password below you may continue.
Otherwise, hit cancel to continue without approving this order.", order);
			}
		}

		public void TestApproveOrderInitiatorNotAllowedToApprove()
		{
			SetOrderApprovalRegistryRestrictions(true, false, false, false);
			SetUpRegistryForPurchaseOrderApprovalTest();
			var initiator = TestObjectCreator.CreateStaffWithSecurityRights("Thang Do", "MTD", Env.Security.PayableOrderApprovalFirstLevelApproval.Code);

			using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				// Purchase order, which requires Second Level Approval
				var order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10000m);
				SimulateExpectErrorWithLoginPrompt(@"Order approval by initiator is disallowed in the registry.
This transaction must be approved by a user with Second Level Approval authority.
If a user with this level of authority enters their username and password below you may continue.
Otherwise, hit cancel to continue without approving this order.", order);

				// Purchase order, which requires None Level Approval
				order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10m);
				SimulateExpectErrorWithLoginPrompt(@"Order approval by initiator is disallowed in the registry.
This transaction does not require any approval authorization level.
However, this transaction must be approved by another user.
Otherwise, hit cancel to continue without approving this order.", order);
			}
		}

		public void TestNone()
		{
			SetUpRegistryForPurchaseOrderApprovalTest();
			SetOrderApprovalRegistryRestrictions(true, false, false, false);
			var initiator = TestObjectCreator.CreateStaffWithSecurityRights("John Doe", "JD", Env.Security.None.Code);
			var other = TestObjectCreator.CreateStaffWithSecurityRights("Sam Smith", "SS", Env.Security.PayableOrderApprovalSecondLevelApproval.Code);
			Factory.Save();

			using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10m);
				AssertEquals("Precondition:", order.APH_Disposition, Constants.PayableOrderDisposition.PendingApproval);

				SimulateExpectErrorWithLoginPrompt(@"Order approval by initiator is disallowed in the registry.
This transaction does not require any approval authorization level.
However, this transaction must be approved by another user.
Otherwise, hit cancel to continue without approving this order.", order, other);

				AssertEquals("Postcondition: Order is now approved", Constants.PayableOrderDisposition.OrderToBePlaced, order.APH_Disposition);
			}
		}

		public void TestShowMessageIfOrderAlreadyApproved()
		{
			var payableOrderApprovalByInitiator = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			SetOrderApprovalRegistryRestrictions(false);

			try
			{
				var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
				var line = order.OrderLines.AddNew();
				line.GenericCharge = TestObjectCreator.CC1.PK;
				order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
				Factory.Save();
				using (AccPayableOrderForm form = new AccPayableOrderForm(order))
				{
					form.Show();
					Application.DoEvents();

					var approveMenuItem = GetActionsMenuItem(form, "Approve");
					approveMenuItem.PerformClick();
					AssertNotEquals(@"This order has been already approved", UnitTestUserNotification.Instance.LastMessage.Text);

					approveMenuItem.PerformClick();
					AssertEquals(@"This order has been already approved", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, payableOrderApprovalByInitiator);
			}
		}

		public void TestShowMessageForBlankSupplierRestrictionRegistry()
		{
			SetOrderApprovalRegistryRestrictions(true, true, false, false);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			AssertEquals(PayableOrderRegistryHelper.BlankSupplierRestrictionMessage, SimulateExpectSuccess(Order));
		}

		public void TestShowMessageForOverriddenSupplierRestrictionRegistry()
		{
			SetOrderApprovalRegistryRestrictions(false, false, true, false);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			OverrideOrderSupplier();
			AssertEquals(PayableOrderRegistryHelper.OverriddenSupplierRestrictionMessage, SimulateExpectSuccess(Order));
		}

		public void TestShowMessageForTemporaryOrganizationRestrictionRegistry()
		{
			SetOrderApprovalRegistryRestrictions(false, false, false, true);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_IsTempAccount = true;
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(PayableOrderRegistryHelper.TemporaryOrganizationRestrictionMessage, SimulateExpectSuccess(Order));
		}

		public void TestChangeOrderDispositiononSupplierChanged()
		{
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			SetOrderApprovalRegistryRestrictions(false);
			//Case: Blank supplier -> New supplier
			SimulateExpectSuccess(Order);
			AssertEquals(Constants.PayableOrderDisposition.OrderToBePlaced, Order.APH_Disposition);
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(Constants.PayableOrderDisposition.PendingApproval, Order.APH_Disposition);

			//Case: Old supplier -> New supplier
			SimulateExpectSuccess(Order);
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(Constants.PayableOrderDisposition.PendingApproval, Order.APH_Disposition);

			//Case: Override supplier
			SimulateExpectSuccess(Order);
			OverrideOrderSupplier();
			AssertEquals(Constants.PayableOrderDisposition.PendingApproval, Order.APH_Disposition);
		}

		public void TestShowMessageIfOrderNotReadyForApproval()
		{
			SetOrderApprovalRegistryRestrictions(true, false, false, false);

			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			Factory.Save();
			using (AccPayableOrderForm form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				approveMenuItem.PerformClick();
				AssertEquals(@"This Purchase Order's disposition is not 'Pending Approval' due to one of the below mentioned reasons:  
* No order lines created for this order or
* No changes made to order line values of existing order lines that have already been approved", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
				approveMenuItem.PerformClick();
				AssertEquals(@"This Purchase Order's disposition is not 'Pending Approval' due to one of the below mentioned reasons:  
* No order lines created for this order or
* No changes made to order line values of existing order lines that have already been approved", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionMenusEnable()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
			Factory.Save();
			using (AccPayableOrderForm form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var splitOrderMenuItem = GetActionsMenuItem(form, "Split Order");
				Assert(splitOrderMenuItem.Enabled);
				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				Assert(approveMenuItem.Enabled);
				var printAndBookMenuItem = GetActionsMenuItem(form, "Place Order");
				Assert(printAndBookMenuItem.Enabled);
				var postAPInvoiceMenuItem = GetActionsMenuItem(form, "Post Accounts Payable Invoice");
				Assert(postAPInvoiceMenuItem.Enabled);

				order.APH_Disposition = Constants.PayableOrderDisposition.Complete;
				Assert(!splitOrderMenuItem.Enabled);
				Assert(!approveMenuItem.Enabled);
				Assert(!printAndBookMenuItem.Enabled);
				Assert(!postAPInvoiceMenuItem.Enabled);
			}

			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			order.IsCancelled = true;
			Factory.Save();
			using (AccPayableOrderForm form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var splitOrderMenuItem = GetActionsMenuItem(form, "Split Order");
				Assert(!splitOrderMenuItem.Enabled);
				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				Assert(!approveMenuItem.Enabled);
				var printAndBookMenuItem = GetActionsMenuItem(form, "Place Order");
				Assert(!printAndBookMenuItem.Enabled);
				var postAPInvoiceMenuItem = GetActionsMenuItem(form, "Post Accounts Payable Invoice");
				Assert(!postAPInvoiceMenuItem.Enabled);
			}
		}

		public void TestPrintBookingRequestDoesNotCauseTypeMismatch()
		{
			AssertPrintBookingRequestDoesNotCauseTypeMismatch(Constants.PayableOrderDisposition.PendingConfirmation);
			AssertPrintBookingRequestDoesNotCauseTypeMismatch(Constants.PayableOrderDisposition.OrderToBePlaced);

			void AssertPrintBookingRequestDoesNotCauseTypeMismatch(string disposition)
			{
				SetOrderApprovalRegistryRestrictions(false);

				var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
				order.APH_Disposition = disposition;
				AssertEquals("Order should not be booked yet, no matter the Disposition", order.IsBooked, false);

				var line = order.OrderLines.AddNew();
				line.GenericCharge = TestObjectCreator.CC1.PK;
				Factory.Save();

				using (var form = new AccPayableOrderForm(order))
				{
					form.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var approveMenuItem = GetActionsMenuItem(form, "Approve");
					Assert(approveMenuItem.Enabled);
					approveMenuItem.PerformClick();
					Factory.Save();
					AssertEquals(@"Order has been successfully approved.", UnitTestUserNotification.Instance.LastMessage.Text);

					var printBookingRequestMenuItem = GetActionsMenuItem(form, "Place Order");
					Assert(printBookingRequestMenuItem.Enabled);
					AssertNoExceptionThrown("No exception should be thrown when attempting to Place Order", () => printBookingRequestMenuItem.PerformClick());
					AssertEquals("LastFormShownDialogForTest should be a DocDeliveryForm",
						"Enterprise.DocumentEngine.GUI.DocDeliveryForm",
						ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
				}
			}
		}

		MenuItem GetActionsMenuItem(Form form, string text)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text)
				{
					return actionItem;
				}
			}
			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}
			return null;
		}

		protected override Form GetFormToBashCore()
		{
			AccPayableOrderHeader testOrder = Factory.New<AccPayableOrderHeader>();
			var result = new AccPayableOrderForm(testOrder);
			result.ControllerID = ControllerIDs.AccPayableOrder;
			return result;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		void SetUpRegistryForPurchaseOrderApprovalTest()
		{
			var collection = new PaymentThreeLevelAuthorisationSettingsCollection();
			var newSetting = collection.AddNew();

			newSetting.Amount = 1000m;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;

			newSetting = collection.AddNew();
			newSetting.Amount = 5000m;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;

			newSetting = collection.AddNew();
			newSetting.Amount = 10000m;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;

			newSetting = collection.AddNew();
			newSetting.Amount = 10000m;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.PayableOrderAuthorizationSettings.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
		}

		void SimulateExpectErrorWithLoginPrompt(ZString expected, AccPayableOrderHeader order, GlbStaff authoriser = null)
		{
			using (var form = new AccPayableOrderForm(order))
			{
				if (authoriser != null)
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((f) =>
					{
						var loginFormCast = f as MasterFiles.GUI.LoginForm;
						if (loginFormCast != null)
						{
							loginFormCast.DoLoginForTest(authoriser.GS_LoginName, authoriser.StaffPlainTextPassword);
							ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
						}
					});
				}

				form.Show();
				Application.DoEvents();

				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				approveMenuItem.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				var loginForm = ZFormModaliser.LastFormShownDialogForTest as MasterFiles.GUI.LoginForm;
				AssertNotNull(loginForm);

				AssertEquals(expected, loginForm.Message);
			}
		}

		string SimulateExpectSuccess(AccPayableOrderHeader order)
		{
			var result = string.Empty;
			using (var form = new AccPayableOrderForm(order))
			{
				form.Show();
				Application.DoEvents();

				var approveMenuItem = GetActionsMenuItem(form, "Approve");
				approveMenuItem.PerformClick();

				result = UnitTestUserNotification.Instance.LastMessage.Text;
			}
			return result;
		}

		#region Helper methods for PO approval registry

		AccPayableOrderHeader Order;

		void SetOrderApprovalRegistryRestrictions(bool value) => SetOrderApprovalRegistryRestrictions(value, value, value, value);

		void SetOrderApprovalRegistryRestrictions(bool enableCAR, bool enableBSR, bool enableOSD, bool enableTOR)
		{
			var codeDescriptionList = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			codeDescriptionList
				.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code == PayableOrderRestrictionList.Codes.CreatorApproval)
					{
						x.Bool = enableCAR;
					}
					if (x.Code == PayableOrderRestrictionList.Codes.BlankSuppliers)
					{
						x.Bool = enableBSR;
					}
					if (x.Code == PayableOrderRestrictionList.Codes.OverriddenSupplier)
					{
						x.Bool = enableOSD;
					}
					if (x.Code == PayableOrderRestrictionList.Codes.SupplierIsTempOrg)
					{
						x.Bool = enableTOR;
					}
				});
			AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionList);
		}

		void OverrideOrderSupplier()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			Order.SupplierDocumentaryAddress.E2_AddressOverride = true;
			Order.SupplierDocumentaryAddress.E2_CompanyName = "FAKE COMPANY";
			Order.SupplierDocumentaryAddress.E2_Address1 = "I DON'T KNOW";
			Order.SupplierDocumentaryAddress.E2_City = "Sydney";
			Order.SupplierDocumentaryAddress.E2_Postcode = "2020";
			Order.SupplierDocumentaryAddress.E2_State = "NSW";
		}
		#endregion
	}
}
