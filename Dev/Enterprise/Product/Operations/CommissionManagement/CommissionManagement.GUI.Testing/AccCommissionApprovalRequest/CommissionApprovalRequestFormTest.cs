using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionApprovalRequestForm))]
	class CommissionApprovalRequestFormTest : ZFormBasherTest
	{
		#region Form Caption

		public void TestFormCaption()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_BatchNumber = "00001002";

			using (var form = new CommissionApprovalRequestForm(request))
			{
				AssertEquals("Commission Approval Request 00001002", form.FormCaption);

				form.SetDataBinding(null, "");
				AssertEquals("Commission Approval Request", form.FormCaption);
			}
		}

		#endregion

		#region Form Load

		public void TestFormLoad_OnlyValidateARInvoiceFullyPaid()
		{
			var testObjCreator = new TestObjectCreator(Factory);

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var chargeCode = testObjCreator.CreateChargeCode("DDD");
			chargeCode.AC_IsActive = false;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment1).TryCreate();
			var invoice = testObjCreator.CreateARInvoice<ARInvoice>("000034812", audCurrency, 1, org);
			invoice.AH_JH = job.PK;
			invoice.AH_InvoiceAmount = 100m;
			invoice.AH_OutstandingAmount = 100m;
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			var invoiceLine = testObjCreator.CreateARInvoiceLine(invoice, job, chargeCode, audCurrency, 1, "", 100);
			testObjCreator.CreateJobCharge(invoiceLine, job, chargeCode, audCurrency, audCurrency);

			var invoiceCommissionHeader = Factory.New<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_AH_Source = invoice.PK;
			invoiceCommissionHeader.CH0_GroupingSourceID = job.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			invoiceCommissionHeader.CH0_CommissionDate = ZDate.Today;
			invoiceCommissionHeader.CH0_SnapshotDateTime = ZDateTime.Now;
			invoiceCommissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;
			invoiceCommissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;
			var invoiceCommissionLine = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionLine.FillWithValidTestData();

			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();

			var group = Factory.New<AccCommissionLineGroup>();
			group.CLG_AC = chargeCode.PK;
			group.CLG_CH0 = invoiceCommissionHeader.PK;
			invoiceCommissionLine.CL0_ParentID = group.PK;

			var item = request.Items.AddNew();
			item.CRI_CL0 = invoiceCommissionLine.PK;
			item.CRI_IsSelected = true;

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				var viewCommissionLine = ((ICommissionPayable)request).CommissionLinesForPayment.First();
				AssertNoNotifications(viewCommissionLine.VCL_ACInfo);
				AssertHasNotifications(viewCommissionLine.HasFullyPaidInfo);
			}
		}

		#endregion

		#region Notifications

		public void TestShowWarningDialogWhenOpeningApprovalRequestWithAlreadyPaidLines()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var line1 = Factory.NewWithValidTestData<AccCommissionLine>();
			line1.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
			var line2 = Factory.NewWithValidTestData<AccCommissionLine>();

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = line1.PK;
			var item2 = request.Items.AddNew();
			item2.CRI_CL0 = line2.PK;

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Obsolete Approval Request", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "This Commission Approval Request contains entity commissions that have already been paid or canceled. You will NOT be able to approve nor process this payment.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}
		}

		public void TestShowWarningDialogWhenOpeningApprovalRequestWithCancelledLines()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var line1 = Factory.NewWithValidTestData<AccCommissionLine>();
			line1.CL0_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);
			var line2 = Factory.NewWithValidTestData<AccCommissionLine>();

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = line1.PK;
			var item2 = request.Items.AddNew();
			item2.CRI_CL0 = line2.PK;

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Obsolete Approval Request", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "This Commission Approval Request contains entity commissions that have already been paid or canceled. You will NOT be able to approve nor process this payment.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}
		}

		public void TestShowWarningDialogWhenOpeningApprovalRequestWithNoLines()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			AssertEquals("Precondition", 0, request.Items.Count);

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Canceled Approval Request", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "This Commission Approval Request has been canceled. You will NOT be able to approve nor process this payment.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}
		}

		public void TestShowWarningDialogWhenOpeningApprovalRequestWithAmendedTransaction()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";

			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.CRQ_GS_NKApprovingStaff2 = "SCW";

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var accHeaderInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice.AH_JH = jobHeader.PK;
			accHeaderInvoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice.AH_Desc = "Invoice Transaction";

			var accHeaderCreditNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderCreditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderCreditNote.AH_TransactionType = TransactionTypes.CreditNote;

			accHeaderCreditNote.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderCreditNote.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderCreditNote.AH_JH = jobHeader.PK;
			accHeaderCreditNote.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderCreditNote.AH_PostDate = ZDateTime.Today;
			accHeaderCreditNote.AH_Desc = "Credit Note Transaction";
			accHeaderCreditNote.AH_TransactionBelongsToGroup = accHeaderInvoice.PK;

			var commissionHeaderInvoice = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderInvoice.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderInvoice.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeaderInvoice.CH0_AH_Source = accHeaderInvoice.PK;

			var invoiceLine = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceLine.CL0_ParentID = commissionHeaderInvoice.PK;
			invoiceLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = invoiceLine.PK;

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals(false, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(false, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(false, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}

			var commissionHeaderCreditNote = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderCreditNote.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderCreditNote.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeaderCreditNote.CH0_AH_Source = accHeaderCreditNote.PK;

			var creditNoteLine = Factory.NewWithValidTestData<AccCommissionLine>();
			creditNoteLine.CL0_ParentID = commissionHeaderCreditNote.PK;
			creditNoteLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			Factory.Save();

			var expectedCaption = "Obsolete Approval Request";
			var expectedErrorMessage = "An amendment to a transaction was made after this Commission Approval Request. You will NOT be able to approve nor process this payment.";

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}

			var grouping = Factory.NewWithValidTestData<AccCommissionLineGroup>();

			var testObjCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjCreator.CreateChargeCode("DDD");
			chargeCode.AC_IsActive = false;
			grouping.CLG_AC = chargeCode.PK;
			grouping.CLG_CH0 = commissionHeaderInvoice.PK;

			invoiceLine.CL0_ParentID = grouping.PK;
			invoiceLine.CL0_ParentTableCode = AccCommissionLineGroupSchema.Constants.Prefix;

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}

			var item2 = Factory.New<AccCommissionApprovalRequestItem>();
			item2.CRI_CL0 = creditNoteLine.PK;
			item2.CRI_CRQ = request.PK;
			request.Items.Add(item2);

			Factory.Save();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals(false, form.ApproveButton1_Exposed.ReadOnly);
					AssertEquals(false, form.ApproveButton2_Exposed.ReadOnly);
					AssertEquals(false, form.ProcessPaymentButton_Exposed.ReadOnly);
				});
			}
		}

		#endregion

		#region ItemGrids

		public void TestItemGrids()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				AssertEquals(1, form.ItemGridsPanel_Exposed.Controls.Count);
				var itemGridsControl = form.ItemGridsPanel_Exposed.Controls[0];
				AssertType(typeof(CommissionApprovalRequestItemGridsControl), itemGridsControl);
			}

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();
				AssertEquals(1, form.ItemGridsPanel_Exposed.Controls.Count);
				var itemGridsControl = form.ItemGridsPanel_Exposed.Controls[0];
				AssertType(typeof(GlobalCommissionApprovalRequestItemGridsControl), itemGridsControl);
			}
		}

		#endregion

		#region Approve Buttons

		public void TestApproveButton1()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_Staff1HasApproved = false;
			var item1 = request.Items.AddNew();
			item1.CRI_IsSelected = false;
			item1.FillWithValidTestData();
			var item2 = request.Items.AddNew();
			item2.CRI_IsSelected = false;
			item2.FillWithValidTestData();

			Factory.Save();

			Env.Security.CommissionAuthorizationLevel1.IsAllowed = true;

			request.CRQ_GS_NKApprovingStaff1 = "XXX";
			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();

				form.ApproveButton1_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Approve", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Please select at least one entity commission to approve.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				item1.CRI_IsSelected = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ApproveButton1_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals(AccCommissionApprovalRequest.Schema.CRQ_Staff1HasApproved, true, request.CRQ_Staff1HasApproved);
					AssertEquals("CRQ_Staff1HasApprovedInfo.OriginalValue", true, request.CRQ_Staff1HasApprovedInfo.OriginalValue);
				});
			}
		}

		public void TestApproveButton1_ReadOnly()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_Staff1HasApproved = false;
			var item = request.Items.AddNew();
			item.CRI_IsSelected = true;
			item.FillWithValidTestData();
			Factory.Save();

			Env.Security.CommissionAuthorizationLevel1.IsAllowed = false;
			{
				request.CRQ_GS_NKApprovingStaff1 = "";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
				}

				request.CRQ_GS_NKApprovingStaff1 = "XXX";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
				}
			}

			Env.Security.CommissionAuthorizationLevel1.IsAllowed = true;
			{
				request.CRQ_GS_NKApprovingStaff1 = "";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
				}

				request.CRQ_GS_NKApprovingStaff1 = "XXX";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.DisplayMode = ODisplayMode.ReadOnly;
					form.Show();
					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
				}

				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(false, form.ApproveButton1_Exposed.ReadOnly);

					form.ApproveButton1_Exposed.PerformClick();
					AssertEquals(true, form.ApproveButton1_Exposed.ReadOnly);
				}
			}
		}

		public void TestApproveButton2()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_Staff2HasApproved = false;
			var item1 = request.Items.AddNew();
			item1.CRI_IsSelected = false;
			item1.FillWithValidTestData();
			var item2 = request.Items.AddNew();
			item2.CRI_IsSelected = false;
			item2.FillWithValidTestData();

			Factory.Save();

			Env.Security.CommissionAuthorizationLevel2.IsAllowed = true;

			request.CRQ_GS_NKApprovingStaff2 = "XXX";
			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.Show();

				form.ApproveButton2_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Approve", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Please select at least one entity commission to approve.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				item1.CRI_IsSelected = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ApproveButton2_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals(AccCommissionApprovalRequest.Schema.CRQ_Staff2HasApproved, true, request.CRQ_Staff2HasApproved);
					AssertEquals("CRQ_Staff2HasApprovedInfo.OriginalValue", true, request.CRQ_Staff2HasApprovedInfo.OriginalValue);
				});
			}
		}

		public void TestApproveButton2_ReadOnly()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_Staff2HasApproved = false;
			var item = request.Items.AddNew();
			item.CRI_IsSelected = true;
			item.FillWithValidTestData();
			Factory.Save();

			Env.Security.CommissionAuthorizationLevel2.IsAllowed = false;
			{
				request.CRQ_GS_NKApprovingStaff2 = "";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
				}

				request.CRQ_GS_NKApprovingStaff2 = "XXX";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
				}
			}

			Env.Security.CommissionAuthorizationLevel2.IsAllowed = true;
			{
				request.CRQ_GS_NKApprovingStaff2 = "";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
				}

				request.CRQ_GS_NKApprovingStaff2 = "XXX";
				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.DisplayMode = ODisplayMode.ReadOnly;
					form.Show();
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
				}

				using (var form = new CommissionApprovalRequestFormForTest(request))
				{
					form.Show();
					AssertEquals(false, form.ApproveButton2_Exposed.ReadOnly);

					form.ApproveButton2_Exposed.PerformClick();
					AssertEquals(true, form.ApproveButton2_Exposed.ReadOnly);
				}
			}
		}

		#endregion

		#region Process Payment Button

		public void TestProcessPaymentButton_ReadOnly()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(true, form.ProcessPaymentButton_Exposed.ReadOnly);
			}

			using (var form = new CommissionApprovalRequestFormForTest(request))
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();

				AssertEquals(false, form.ProcessPaymentButton_Exposed.ReadOnly);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			return new CommissionApprovalRequestForm(approvalRequest);
		}

		#endregion

		#region Classes

		class CommissionApprovalRequestFormForTest : CommissionApprovalRequestForm
		{
			public CommissionApprovalRequestFormForTest(AccCommissionApprovalRequest approvalRequest)
				: base(approvalRequest)
			{
			}

			public ZButton ApproveButton1_Exposed
			{
				get { return base.ApproveButton1; }
			}

			public ZButton ApproveButton2_Exposed
			{
				get { return base.ApproveButton2; }
			}

			public ZButton ProcessPaymentButton_Exposed
			{
				get { return base.ProcessPaymentButton; }
			}

			public ZPanel ItemGridsPanel_Exposed
			{
				get { return base.ItemGridsPanel; }
			}
		}

		#endregion
	}
}
