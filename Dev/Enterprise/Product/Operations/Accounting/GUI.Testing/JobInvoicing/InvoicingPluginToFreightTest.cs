using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.DataTransfer.Integration.Testing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.GUI.BrowserInterop.Tests;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.JobInvoicing.JobChargeUserControl;
using static Enterprise.MasterFiles.Business.RevenueRecognitionLookups;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class InvoicingPluginToFreightTest : TestCaseWithFactory
	{
		public void TestCriticalValidationErrorType_WIPACROrganisationDoesNotMatchOneOnJobCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "USLAX", transportMode: Constants.TransportModes.Air);
			shipment[JobShipmentSchema.Constants.JS_INCO] = Constants.IncoTerms.CarriageAndInsurancePaidTo;

			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 100m, debtor: TestObjectCreator.ABIGAS);
			TestObjectCreator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			AssertEquals("Default Debtor on job should be AALSHI", TestObjectCreator.AALSHI.PK, job.GetDebtorPK(charge));
			AssertEquals("Debtor on charge should be ABIGAS", TestObjectCreator.ABIGAS.PK, charge.SellAccount.PK);

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = JobShipmentSchema.Constants.JS_INCO;
			action.PQ_FieldValue = Constants.IncoTerms.CarriagePaidTo;

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionLines>>("Should trigger CriticalValidationError", () => Factory.Save());

				AssertEquals(nameof(CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10), ex.ErrorType);

				AssertType<DeveloperNotificationException>("Should report DeveloperNotificationException", ErrorReporter.LastExceptionReported);
				AssertContains("Should report error with message", "WorkflowSettingPropertiesAfterOnSaving", ErrorReporter.LastKeyReported);
				var expectMessage = @$"The JobCharge is protected from making changes when saving and its properties are not allowed to be changed by the Immediate Field Change (IFC) trigger action. Consider using a Set Field (FLD) trigger action instead.
Trigger Event Code: ADD.
Trigger Description: Trigger 1.
Factory ID: {Factory._Instance}.
Trigger Actions:
IFC - JS_INCO - CPT";
				AssertContains(expectMessage, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		public void TestExecuteAutorating_AdditionalJobs()
		{
			TestObjectCreator.CreateFlatCalculatorClientRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", TestObjectCreator.LocalClient, "FRT", 100);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "USLAX", transportMode: Constants.TransportModes.Air, incoTerm: "CIF");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.LocalClient.PK;

			Factory.Save();

			var dummyBizO = new DummyJobInvoicingBusinessObject(Factory);
			dummyBizO.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { shipment });

			var dummyJob = Factory.NewJobForTesting<Job>();
			dummyJob.JH_ParentID = dummyBizO.PK;
			dummyJob.JH_GE = TestObjectCreator.FIADepartment.PK;
			dummyJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.Addresses.MainAddress.PK;

			using (var form = new ZForm(dummyBizO))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				CombineAssertions("Precondition", () =>
				{
					AssertEquals
					(
						"Should have no error message",
						@"AutoRating has been completed.
Please review the following warnings:
record was auto-costed.
	No costs were found.
Rates Service: Searching from Rates Service cannot proceed - Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LCL: AutoRating -> Rates Service -> Rates Service Subscription
Shipment S00001 was auto-costed.
	No costs were found.
record was auto-rated.
	No rates were found.
Please refer to Notes->AutoRating Log for more information.",
						UnitTestUserNotification.Instance.LastMessage.Text
					);

					AssertContainsExactElementsInAnyOrder
					(
						"Should have shipment charges",
						new[] { "FRT => 100.00" },
						((Job)shipment.Job).Charges.Select(charge => $"{charge.ChargeCode.AC_Code} => {charge.JR_OSSellAmt}")
					);
				});
			}

			var mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
			AssertEquals("GIVEN autorated additional job has charge WHEN form is closed THEN the job mutex should been released (not lock)", false, mutex.IsLocked);
		}

		#region Cash Advance

		public void TestCashAdvanceGridIsRefreshedAfterCreatingNewCashAdvanceRequest()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 100m, debtor: TestObjectCreator.AALSHI);
			var cashAdvanceHeader1 = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, TestObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, TestObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1ForHeader1 = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader1.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_IsARCashAdvance = true;
			charge1.JR_CAL_ARLine = cashAdvanceLine1ForHeader1.PK;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				var control = plugin.UserControl as JobInvoicingUserControl;
				form.Controls.Add(control.JobInvoicingTabControl_ForTest);

				form.Show();

				//Go to cash advances tab and check header grid elements
				control.JobInvoicingTabControl_ForTest.SelectTab(5);
				var headerGrid = control.CashAdvanceRequestUserControl.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull(headerGrid);
				Assert(headerGrid.Visible);

				AssertEquals(1, job.CashAdvanceRequests.Count);
				headerGrid.SelectAllElements();
				AssertEquals(1, headerGrid.SelectedElements.Length);

				//Go to billing tab and generate new cash advance request
				control.JobInvoicingTabControl_ForTest.SelectTab(0);

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_OSCostAmt = 88.99m;
				charge2.JR_OSSellAmt = 88.99m;
				charge2.JR_IsARCashAdvance = true;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();

				plugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"The following Advance Payment requests have been generated:
Debtor/Request ID/Currency/Total Amount
ABIGAS, 00001000, AUD, 88.99"));

				//Go to cash advances tab and check header grid elements again
				control.JobInvoicingTabControl_ForTest.SelectTab(5);
				AssertEquals("Postcondition", 2, job.CashAdvanceRequests.Count);
				Assert(headerGrid.Visible);
				headerGrid.UnSelectAll();
				headerGrid.SelectAllElements();
				AssertEquals(2, headerGrid.SelectedElements.Length);
			}
		}

		public void TestCashAdvance_WhenRequestGenerated_ShouldPopupPrintQuestionMessageAfterConfirmationMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 88.99m, debtor: TestObjectCreator.ABIGAS);

			charge1.JR_OSCostAmt = 88.99m;
			charge1.JR_IsARCashAdvance = true;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				var control = plugin.UserControl as JobInvoicingUserControl;
				form.Controls.Add(control.JobInvoicingTabControl_ForTest);

				form.Show();

				UnitTestUserNotification.Instance.AddYesAnswer();
				plugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());

				AssertEquals(1, job.CashAdvanceRequests.Count);
				AssertEquals(@"The following Advance Payment requests have been generated:
Debtor/Request ID/Currency/Total Amount
ABIGAS, 00001000, AUD, 88.99", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasInformation);

				AssertEquals(@"Do you want to print Advance Payment Request
ABIGAS, 00001000, AUD, 88.99", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasQuestion);

				AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[0], UnitTestUserNotification.Instance.LastMessage);

				AssertEquals("LastFormShownDialogForTest should be a DocDeliveryForm", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
			}
		}

		public void TestCashAdvance_WhenTwoRequestsGenerated_ShouldPopupTwoPrintQuestionMessages_WithOneConfirmation()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 88.99m, debtor: TestObjectCreator.ABIGAS);

			charge1.JR_OSCostAmt = 88.99m;
			charge1.JR_IsARCashAdvance = true;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 188.99m, debtor: TestObjectCreator.AALSHI);

			charge2.JR_OSCostAmt = 188.99m;
			charge2.JR_IsARCashAdvance = true;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				var control = plugin.UserControl as JobInvoicingUserControl;
				form.Controls.Add(control.JobInvoicingTabControl_ForTest);

				form.Show();
				plugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());

				AssertEquals(2, job.CashAdvanceRequests.Count);

				AssertEquals("Should only pop up one confirmation message", 1, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.WasInformation));

				AssertEquals(@"The following Advance Payment requests have been generated:
Debtor/Request ID/Currency/Total Amount
ABIGAS, 00001000, AUD, 88.99
AALSHI, 00001001, AUD, 188.99", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[2].WasInformation);

				AssertEquals(@"Do you want to print Advance Payment Request
ABIGAS, 00001000, AUD, 88.99", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);

				AssertEquals(@"Do you want to print Advance Payment Request
AALSHI, 00001001, AUD, 188.99", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasQuestion);
			}
		}

		public void TestCashAdvanceRequestTabVisibilityDependsOnEnableReceivablesCashAdvanceFunctionalityRegistry()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			foreach (var regValuePair in new (bool enableARCashAdvanceFunctionality, bool enableAPCashAdvanceFunctionality)[]
			{
				(false, false), (false, true), (true, false), (true, true),
			})
			{
				var expectedResult = !regValuePair.enableAPCashAdvanceFunctionality && !regValuePair.enableARCashAdvanceFunctionality;

				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableARCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableAPCashAdvanceFunctionality))
				using (var plugin = new InvoicingPluginToFreight(shipment))
				{
					AssertEquals(expectedResult, ((JobInvoicingUserControl)plugin.UserControl).CashAdvanceRequestsTabPage.IsDisposed);
				}
			}
		}

		public void TestCashAdvanceRequestTabIsNotBindedWhenEnableCashAdvanceFunctionalityRegistryIsDisabled()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 100m, debtor: TestObjectCreator.AALSHI);
			var cashAdvanceHeader1 = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, TestObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, TestObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1ForHeader1 = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader1.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_ARLine = cashAdvanceLine1ForHeader1.PK;
			Factory.Save();

			foreach (var regValuePair in new (bool enableARCashAdvanceFunctionality, bool enableAPCashAdvanceFunctionality)[]
			{
				(false, false), (false, true), (true, false), (true, true),
			})
			{
				var ifNotBinding = !regValuePair.enableAPCashAdvanceFunctionality && !regValuePair.enableARCashAdvanceFunctionality;

				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableARCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableAPCashAdvanceFunctionality))
				using (var plugin = new InvoicingPluginToFreight(shipment))
				{
					plugin.OnGUIShown();
					if (ifNotBinding)
					{
						AssertNull(((JobInvoicingUserControl)plugin.UserControl).CashAdvanceRequestUserControl.CurrentDataItem);
					}
					else
					{
						AssertNotNull(((JobInvoicingUserControl)plugin.UserControl).CashAdvanceRequestUserControl.CurrentDataItem);
					}
				}
			}
		}

		#endregion

		#region NO GUI displayed when change job status in transaction tests
		class JobResetStatusDuringSaving : Job
		{
			public JobResetStatusDuringSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public Action OnResetJobStatusInFactorySavingCore = () => { };

			protected override void OnFactorySavingCore()
			{
				base.OnFactorySavingCore();
				OnResetJobStatusInFactorySavingCore();
			}
		}

		public void TestSuppressGuiMessage_WhenChangeStatusOfCompleteJob_InTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfCompleteJob(true);
		}

		public void TestShowGuiMessage_WhenChangeStatusOfCompleteJob_NotInTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfCompleteJob(false);
		}

		void AssertGuiBehavior_WhenChangeStatusOfCompleteJob(bool insideFactorySave)
		{
			Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;
			var testJob = Factory.NewJobForTesting<JobResetStatusDuringSaving>();
			testJob.JH_JobNum = "job01";
			testJob.JH_Status = JobHeaderStatus.Complete.Code;
			testJob.OnCannotChangeStatusUserMessage += (object sender, UserMessageEventArgs e) => { Globals.Message.Show(e.Message, ((Job)sender).JH_JobNum, MessageBoxButtons.OK, MessageBoxIcon.Information); };
			Factory.Save();
			AssertEquals(JobHeaderStatus.Complete.Code, testJob.JH_Status);

			Action changeJobStatusToWorking = () => { testJob.JH_Status = JobHeaderStatus.Working.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusToWorking;
			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				var fromJobStatus = JobHeaderStatus.Complete.Code;
				var jobStatusUpdateRestrictionRuleCollection = AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.DefaultValue;
				var jobStatusUpdateRestrictionRule = jobStatusUpdateRestrictionRuleCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == fromJobStatus);
				jobStatusUpdateRestrictionRule.Working = "YES";

				using (AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobStatusUpdateRestrictionRuleCollection))
				{
					testJob.JH_Status = JobHeaderStatus.Working.Code;
					testJob.Validation.ValidateJH_Status();
					AssertHasError("Changing Status of Complete Job if no security access should cause error.", testJob.JH_StatusInfo, Env.Security.ChangeStatusOfCompleteJobs.ErrorMessageForNotAllowed + JobValidation.CompleteJobSecurityErrorMessage);
				}

				changeJobStatusToWorking();
				AssertNull("Expect no error message shown when JH_Status changes", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressLoginFormAndGuiMessage_WhenReopenJob_InTransaction()
		{
			AssertGuiBehavior_WhenReopenJob(true);
		}

		public void TestShowLoginFormAndGuiMessage_WhenReopenJob_NotInTransaction()
		{
			AssertGuiBehavior_WhenReopenJob(false);
		}

		void AssertGuiBehavior_WhenReopenJob(bool insideFactorySave)
		{
			Env.Security.ReopenJob.IsAllowed = false;
			var testJob = Factory.NewJobForTesting<JobResetStatusDuringSaving>();
			SecurityOverrideProviderSource.Get(testJob).Provider = new JobInvoicingSecurityOverrideProvider();
			testJob.JH_JobNum = "job01";
			testJob.JH_Status = JobHeaderStatus.Closed.Code;
			testJob.OnCannotChangeStatusUserMessage += (object sender, UserMessageEventArgs e) => { Globals.Message.Show(e.Message, ((Job)sender).JH_JobNum, MessageBoxButtons.OK, MessageBoxIcon.Information); };
			Factory.Save();
			AssertEquals(JobHeaderStatus.Closed.Code, testJob.JH_Status);

			Action changeJobStatusToWorking = () => { testJob.JH_Status = JobHeaderStatus.Working.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusToWorking;

			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no login form shown when it's invoked in a transaction", ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToWorking();
				AssertEquals("Expect login form shown when it's invoked outside a transaction", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Expect error message shown when it's invoked outside a transaction",
					@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Reopen Jobs",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressGuiMessage_WhenChangeStatusOfReadyToPostJob_InTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfReadyToPostJob(true);
		}

		public void TestShowGuiMessage_WhenChangeStatusOfReadyToPostJob_NotInTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfReadyToPostJob(false);
		}

		void AssertGuiBehavior_WhenChangeStatusOfReadyToPostJob(bool insideFactorySave)
		{
			Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed = false;
			var testJob = Factory.NewJobForTesting<JobResetStatusDuringSaving>();
			testJob.JH_JobNum = "job01";
			testJob.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			testJob.OnCannotChangeStatusUserMessage += (object sender, UserMessageEventArgs e) => { Globals.Message.Show(e.Message, ((Job)sender).JH_JobNum, MessageBoxButtons.OK, MessageBoxIcon.Information); };
			Factory.Save();
			AssertEquals(JobHeaderStatus.JobReadyForRevenuePosting.Code, testJob.JH_Status);

			Action changeJobStatusToWorking = () => { testJob.JH_Status = JobHeaderStatus.Working.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += () => { testJob.JH_Status = JobHeaderStatus.Working.Code; };
			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToWorking();
				AssertNull("Expect no error message shown when JH_Status changes", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressGuiMessage_WhenCloseJobContainsUnpostedApportionment_InTransaction()
		{
			AssertGuiBehavior_WhenCloseJobContainsUnpostedApportionment(true);
		}

		public void TestShowGuiMessage_WhenCloseJobContainsUnpostedApportionment_NotInTransaction()
		{
			AssertGuiBehavior_WhenCloseJobContainsUnpostedApportionment(false);
		}

		void AssertGuiBehavior_WhenCloseJobContainsUnpostedApportionment(bool insideFactorySave)
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			consol.Shipments.Add(shipment);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100M;
			Factory.Save();

			var testJob = Factory.Load<JobResetStatusDuringSaving>(shipment.Job.PK);
			testJob.JH_JobNum = "job01";
			testJob.JH_Status = JobHeaderStatus.Working.Code;
			testJob.OnCloseJobError += (Job job, string errorMessage) => { Globals.Message.ShowError(errorMessage); };
			Factory.Save();
			AssertEquals(JobHeaderStatus.Working.Code, testJob.JH_Status);

			Action changeJobStatusToClosed = () => { testJob.JH_Status = JobHeaderStatus.Closed.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusToClosed;
			testJob.JH_Description = "desc";

			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToClosed();
				AssertEquals(@"The job S00010001 contains apportioned charges and cannot be closed.
Please open consol and remove or post the apportionment(s) first and try again after closing and reopening the current form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressGuiMessage_WhenCloseJobHasRelatedUnapprovedTxs_InTransaction()
		{
			AssertGuiBehavior_WhenCloseJobHasRelatedUnapprovedTxs(true);
		}

		public void TestShowGuiMessage_WhenCloseJobHasRelatedUnapprovedTxs_NotInTransaction()
		{
			AssertGuiBehavior_WhenCloseJobHasRelatedUnapprovedTxs(false);
		}

		void AssertGuiBehavior_WhenCloseJobHasRelatedUnapprovedTxs(bool insideFactorySave)
		{
			var testJob = Factory.NewJobForTesting<JobResetStatusDuringSaving>();
			testJob.JH_JobNum = "job01";
			testJob.JH_Status = JobHeaderStatus.Working.Code;
			testJob.OnCloseJobError += (Job job, string errorMessage) => { Globals.Message.ShowError(errorMessage); };
			Factory.Save();
			AssertEquals(JobHeaderStatus.Working.Code, testJob.JH_Status);

			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			var line = uaInvoice.Lines.AddNew();
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var charge = testJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_AL_APLine = line.PK;

			Action changeJobStatusToClosed = () => { testJob.JH_Status = JobHeaderStatus.Closed.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusToClosed;
			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToClosed();
				AssertEquals("You cannot close this Job job01 because it has related unapproved transactions. Please approve or delete these transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressQuestionForm_WhenChangeStatusOfJobContainsUnrecognizedRevenue_InTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfJobContainsUnrecognizedRevenue(true);
		}

		public void TestShowQuestionForm_WhenChangeStatusOfJobContainsUnrecognizedRevenue_NotInTransaction()
		{
			AssertGuiBehavior_WhenChangeStatusOfJobContainsUnrecognizedRevenue(false);
		}

		void AssertGuiBehavior_WhenChangeStatusOfJobContainsUnrecognizedRevenue(bool insideFactorySave)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var line = invoice.Lines[0];
			line.AL_JH = job.PK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_OSAmount = line.AL_LineAmount = -charge.JR_OSCostAmt;
			Factory.Save();
			AssertEquals("Precondition: line shouldn't be recognized.", ZDateTime.Empty, line.AL_ReverseDate);

			var testJob = Factory.Load<JobResetStatusDuringSaving>(shipment.Job.PK);
			Action changeJobStatusToComplete = () => { testJob.JH_Status = JobHeaderStatus.Complete.Code; };
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusToComplete;
			testJob.OnCloseJobYesNoQuestion += (object sender, UserQueryEventArgs e) =>
			{
				DialogResult result = Globals.Message.Show(e.QueryMessage, ((Job)sender).JH_JobNum, MessageBoxButtons.YesNo, e.Response ? DialogResult.Yes : DialogResult.No);
				e.Response = result == DialogResult.Yes;
			};
			testJob.OnCloseJobError += (Job aJob, string errorMessage) => { Globals.Message.ShowError(errorMessage); };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's in a transaction and the user response is No", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToComplete();
				AssertEquals(@"The job S00001001 contains unrecognized revenue and cannot be closed or completed.
Please recognize revenue first and try again.
The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Actual/Estimated Arrival Date'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			testJob.JH_Description = "desc2";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no GUI form shown when it's in a transaction and user response is Yes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusToComplete();
				AssertEquals(@"The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Actual/Estimated Arrival Date'.
The job S00001001 contains unrecognized revenue and cannot be closed or completed.
Do you want to recognize revenue with Today's date?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuppressGUIMessage_WhenPreviousChangeInTheSameSessionTriggeredRevenueRecognitions_InTransaction()
		{
			AssertGuiBehavior_WhenPreviousChangeInTheSameSessionTriggeredRevenueRecognitions(true);
		}

		public void TestShowGUIMessage_WhenPreviousChangeInTheSameSessionTriggeredRevenueRecognitions_NotInTransaction()
		{
			AssertGuiBehavior_WhenPreviousChangeInTheSameSessionTriggeredRevenueRecognitions(false);
		}

		void AssertGuiBehavior_WhenPreviousChangeInTheSameSessionTriggeredRevenueRecognitions(bool insideFactorySave)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var line = invoice.Lines[0];
			line.AL_JH = job.PK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_OSAmount = line.AL_LineAmount = -charge.JR_OSCostAmt;
			Factory.Save();
			AssertEquals("Precondition: line shouldn't be recognized.", ZDateTime.Empty, line.AL_ReverseDate);

			var testJob = Factory.Load<JobResetStatusDuringSaving>(shipment.Job.PK);
			Action changeJobStatusTwiceInTheSameSession = () =>
			{
				testJob.JH_Status = JobHeaderStatus.Complete.Code;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testJob.JH_Status = JobHeaderStatus.Working.Code;
			};
			testJob.OnResetJobStatusInFactorySavingCore += changeJobStatusTwiceInTheSameSession;
			testJob.OnCannotChangeStatusUserMessage += (object sender, UserMessageEventArgs e) => { Globals.Message.Show(e.Message, ((Job)sender).JH_JobNum, MessageBoxButtons.OK, MessageBoxIcon.Information); };
			testJob.OnCloseJobYesNoQuestion += (object sender, UserQueryEventArgs e) =>
			{
				DialogResult result = Globals.Message.Show(e.QueryMessage, ((Job)sender).JH_JobNum, MessageBoxButtons.YesNo, e.Response ? DialogResult.Yes : DialogResult.No);
				e.Response = result == DialogResult.Yes;
			};
			testJob.OnCloseJobError += (Job aJob, string errorMessage) => { Globals.Message.ShowError(errorMessage); };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			testJob.JH_Description = "desc";
			if (insideFactorySave)
			{
				Factory.Save();
				AssertNull("Expect no error message shown when it's invoked in a transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				changeJobStatusTwiceInTheSameSession();
				AssertEquals(@"You have changed the status of this job to 'CMP' before saving. This will trigger revenue recognition.
If you did not intend to recognize revenue, please close the job without saving.
If you intended to recognize revenue, please change the status back to 'CMP' and save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Gateway Target Job Query Tests

		public void TestImportAPInvoicesForShipmentWithJobChargeTarget()
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			var sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			var sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S1112", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			var expectedInvoicePks = new List<ZGuid>();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);

				var invoicePostedFromShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromShipment1.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef;
				invoicePostedFromShipment1.AH_JH = shipment1Job.PK;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromShipment1, shipment1Job, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromGatewayWithoutJobTargets = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromGatewayWithoutJobTargets.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				invoicePostedFromGatewayWithoutJobTargets.AH_JH = gatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromGatewayWithoutJobTargets, gatewayJob, TestObjectCreator.CC1, 200m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromGatewayWithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromGatewayWithJobTargetShipment1.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				invoicePostedFromGatewayWithJobTargetShipment1.AH_JH = gatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromGatewayWithJobTargetShipment1, gatewayJob, TestObjectCreator.CC1, 300m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				var invoicePostedFromGatewayWithJobTargetShipment2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromGatewayWithJobTargetShipment2.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				invoicePostedFromGatewayWithJobTargetShipment2.AH_JH = gatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromGatewayWithJobTargetShipment2, gatewayJob, TestObjectCreator.CC1, 400m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment2, shipment2);

				var invoicePostedFromGatewayWithJobTargetConsol = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromGatewayWithJobTargetConsol.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				invoicePostedFromGatewayWithJobTargetConsol.AH_JH = gatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromGatewayWithJobTargetConsol, gatewayJob, TestObjectCreator.CC1, 500m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, gatewayConsol);

				Factory.Save();

				expectedInvoicePks.Add(invoicePostedFromShipment1.PK);
				expectedInvoicePks.Add(invoicePostedFromGatewayWithJobTargetShipment1.PK);
			}

			using (var plugin = new InvoicingPluginToFreight(shipment1))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals(2, converter.Candidates.Count);
				AssertContainsExactElementsInAnyOrder(expectedInvoicePks, converter.Candidates.Select(x => x.PK));
			}
		}

		public void TestImportAPInvoicesForGatewayConsolWithJobChargeTarget()
		{
			var sisterCompanyOrgProxy1 = TestObjectCreator.CreateOrgHeader("SISOR1", true, false);
			var sisterCompany1 = TestObjectCreator.CreateNewCompany("SI1", orgProxy: sisterCompanyOrgProxy1);
			var sisterBranch1 = TestObjectCreator.CreateNewBranch(sisterCompany1, "SI1");

			var sisterCompanyOrgProxy2 = TestObjectCreator.CreateOrgHeader("SISOR2", true, false);
			var sisterCompany2 = TestObjectCreator.CreateNewCompany("SI2", orgProxy: sisterCompanyOrgProxy2);
			var sisterBranch2 = TestObjectCreator.CreateNewBranch(sisterCompany2, "SI2");

			Factory.Save();

			var consol1 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", sendingGatewayCompany: sisterCompany1, receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", consol1);
			var shipment2 = TestObjectCreator.CreateShipment("S1112", consol1);
			var consol2 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C002", receivingGatewayCompany: sisterCompany1);
			consol2.Shipments.AddRange(new[] { shipment1, shipment2 });
			TestObjectCreator.CreateJob(consol1);
			Factory.Save();

			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			var expectedARInvoicePksForConsol1 = new List<ZGuid>();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				Factory.Save();

				var invoicePostedFromConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1, shipment1Job, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2, shipment1Job, TestObjectCreator.CC1, 200m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				Factory.Save();

				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1.PK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				var consol1GatewayJob = TestObjectCreator.CreateJob(consol1);
				var consol2GatewayJob = TestObjectCreator.CreateJob(consol2);
				Factory.Save();

				var invoicePostedFromConsol1WithoutJobChargeTargets = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithoutJobChargeTargets.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithoutJobChargeTargets.AH_JH = consol1GatewayJob.PK;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithoutJobChargeTargets, consol1GatewayJob, TestObjectCreator.CC1, 300m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol1WithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetShipment1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetShipment1.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetShipment1, consol1GatewayJob, TestObjectCreator.CC1, 400m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				var invoicePostedFromConsol1WithJobTargetConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetConsol2.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetConsol2.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetConsol2, consol1GatewayJob, TestObjectCreator.CC1, 500m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol2);

				var invoicePostedFromConsol1WithJobTargetConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetConsol1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetConsol1.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetConsol1, consol1GatewayJob, TestObjectCreator.CC1, 600m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol1);

				var invoicePostedFromConsol2WithoutJobChargeTargets = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithoutJobChargeTargets.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithoutJobChargeTargets.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithoutJobChargeTargets, consol2GatewayJob, TestObjectCreator.CC1, 700m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol2WithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetShipment1.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetShipment1.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetShipment1, consol2GatewayJob, TestObjectCreator.CC1, 800m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				var invoicePostedFromConsol2WithJobTargetConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetConsol1.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetConsol1.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetConsol1, consol2GatewayJob, TestObjectCreator.CC1, 900m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol1);

				var invoicePostedFromConsol2WithJobTargetConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetConsol2.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetConsol2.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetConsol2, consol2GatewayJob, TestObjectCreator.CC1, 1000m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol2);

				Factory.Save();

				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithoutJobChargeTargets.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetShipment1.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetConsol2.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetConsol1.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol2WithJobTargetConsol1.PK);
			}

			using (var plugin = new InvoicingPluginToFreight(consol1))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals(6, converter.Candidates.Count);
				AssertContainsExactElementsInAnyOrder(expectedARInvoicePksForConsol1, converter.Candidates.Select(x => x.PK));
			}
		}

		public void TestImportAPInvoicesQueryUsesJRT_InvoiceTargetJobIndex()
		{
			CreateSisterCompanyInvoice(out ForwardingShipment shipment, out InvoicingBase[] arInvoices);
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					AssertContainsExactElementsInAnyOrder(arInvoices.Select(x => x.PK), converter.Candidates.Select(x => x.PK));
					var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AH_ConsolidatedInvoiceRef"));
					var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
					Assert("Non clustered index on JRT_InvoiceTargetJob must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__JRT_InvoiceTargetID"));
				}
			}
		}

		void CreateSisterCompanyInvoice(out ForwardingShipment shipment1, out InvoicingBase[] arInvoices)
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			var sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			var sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			TestObjectCreator.CreateShipment("S1112", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);

				var invoicePostedFromShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromShipment1.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef;
				invoicePostedFromShipment1.AH_JH = shipment1Job.PK;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromShipment1, shipment1Job, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromGatewayWithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromGatewayWithJobTargetShipment1.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				invoicePostedFromGatewayWithJobTargetShipment1.AH_JH = gatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromGatewayWithJobTargetShipment1, gatewayJob, TestObjectCreator.CC1, 300m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				Factory.Save();

				arInvoices = new List<InvoicingBase>() { invoicePostedFromShipment1, invoicePostedFromGatewayWithJobTargetShipment1 }.ToArray();
			}
		}

		public void TestClientContractNumberNotShownForTransportBooking()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking = bookingConsolidation.Bookings.AddNew();
			new Job.Loader(booking).TryCreateWithoutMutexForTestOnly();

			using (var form = new ZForm(booking))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);

				var firstVisibleTabPage = new ZTabPage();
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Show();

				tabControl.SelectNextTabPage();
				var clientContractNumberControl = plugin.JobChargeUserControl_ForTestOnly.ClientContractNumber_ForTestOnly;
				AssertEquals("TransportBooking has not support Client Contract No. yet.", false, clientContractNumberControl.Visible);
			}
		}

		public void TestClientContractNumberShownForForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);

				var firstVisibleTabPage = new ZTabPage();
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Show();

				tabControl.SelectNextTabPage();
				var clientContractNumberControl = plugin.JobChargeUserControl_ForTestOnly.ClientContractNumber_ForTestOnly;
				AssertEquals(true, clientContractNumberControl.Visible);

				var quotesCodeFindBox = plugin.JobChargeUserControl_ForTestOnly.GetControl<ZCodeFindBox>("QuotesCodeFindBox");
				AssertGreaterThan(quotesCodeFindBox.Left, clientContractNumberControl.Left + clientContractNumberControl.Width);
			}
		}

		public void TestClientContractNumberShownForQuickBooking()
		{
			var quickBooking = QuotedBooking.CreateNewBooking(Factory);
			new Job.Loader(quickBooking).TryCreateWithoutMutexForTestOnly();

			using (var form = new ZForm(quickBooking))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);

				var firstVisibleTabPage = new ZTabPage();
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Show();

				tabControl.SelectNextTabPage();
				var clientContractNumberControl = plugin.JobChargeUserControl_ForTestOnly.ClientContractNumber_ForTestOnly;
				AssertEquals(true, clientContractNumberControl.Visible);

				var quotesCodeFindBox = plugin.JobChargeUserControl_ForTestOnly.GetControl<ZCodeFindBox>("QuotesCodeFindBox");
				AssertGreaterThan(quotesCodeFindBox.Left, clientContractNumberControl.Left + clientContractNumberControl.Width);
			}
		}

		public void TestClientContractNumberButton_ClickShowsBrowserWindow()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			_ = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			var dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			using (ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"))
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			using (var tabControl = new ZTemplateTabControl())
			using (var firstVisibleTabPage = new ZTabPage())
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();
				tabControl.SelectNextTabPage();

				plugin.JobChargeUserControl_ForTestOnly.ClientContractNumberButton_ForTestOnly.PerformClick();
				AssertEquals(1, dummyBrowserInteropWindowFactory.windowsCreated);
			}
		}

		public void TestClientContractNumberButton_ClickShowsErrorMessageForInvalidJobParent()
		{
			var dtbBooking = Factory.NewWithValidTestData<DtbBooking>();
			_ = new Job.Loader(dtbBooking).TryCreateWithoutMutexForTestOnly();

			var dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			using (ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"))
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(dtbBooking))
			using (var tabControl = new ZTemplateTabControl())
			using (var firstVisibleTabPage = new ZTabPage())
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();
				tabControl.SelectNextTabPage();

				plugin.JobChargeUserControl_ForTestOnly.ClientContractNumberButton_ForTestOnly.Visible = true;
				plugin.JobChargeUserControl_ForTestOnly.ClientContractNumberButton_ForTestOnly.PerformClick();

				AssertEquals(0, dummyBrowserInteropWindowFactory.windowsCreated);
				AssertEquals(
					"This action is only valid for Job Headers linked to a Forwarding Shipment, but a DtbBooking was found.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGatewayTargetJobQueryFilterIsOnlyAddedToShipmentAndGatewayConsol()
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			var sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			var sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();
			}

			foreach (JobInvoicingConsumerType jobInvoicingConsumerType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				var shouldContainJobChargeTargetFilter = false;
				BusinessObject jobHeaderParent = null;
				if (jobInvoicingConsumerType == JobInvoicingConsumerTypes.QuotedBooking)
				{
					var quotedBookingBuilder = ObjectFactory.Get<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
					jobHeaderParent = quotedBookingBuilder.CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, Factory) as BusinessObject;
					var jobHeader = Factory.NewJobForTesting<JobHeader>();
					jobHeader.JH_ParentID = jobHeaderParent.PK;
					jobHeader.JH_ParentTableCode = jobHeaderParent.TablePrefix;
					jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
					jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
					jobHeader.IsManuallyCreated = true;
				}
				else
				{
					jobHeaderParent = Factory.NewWithValidTestData(jobInvoicingConsumerType.BizoType);
					CreateJobHeader(jobHeaderParent as IJobHeaderParent);
				}

				if (jobInvoicingConsumerType == JobInvoicingConsumerTypes.Shipment)
				{
					var consol = TestObjectCreator.CreateConsol();
					var shipment = jobHeaderParent as ForwardingShipment;
					consol.Shipments.Add(shipment);
					shouldContainJobChargeTargetFilter = true;

					var job = TestObjectCreator.CreateJobHeader();
					job.JH_ParentID = consol.PK;
				}
				else if (jobInvoicingConsumerType == JobInvoicingConsumerTypes.GatewayConsol)
				{
					var consol = jobHeaderParent as ForwardingConsol;
					var shipment = TestObjectCreator.CreateShipment("S1234");
					consol.Shipments.Add(shipment);
					shouldContainJobChargeTargetFilter = true;
				}
				Factory.Save();

				using (var plugIn = new InvoicingPluginForTest(jobHeaderParent))
				{
					plugIn.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
					// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
					using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
					{
						var candidates = converter.Candidates;
						var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AH_ConsolidatedInvoiceRef"));
						var devMessage = "JobChargeTarget filter should " + (shouldContainJobChargeTargetFilter ? "" : "NOT ") + $"be added to {jobInvoicingConsumerType.Description}";
						AssertEquals(devMessage, shouldContainJobChargeTargetFilter, queryPlan.Item1.Contains($"SELECT JRT_JR FROM dbo.JobChargeTarget WHERE JRT_InvoiceTargetID ="));
					}
				}
			}
		}

		#endregion

		#region Reverse Invoice with CreditAdjustmentNotePostingApprovalLevels

		public void TestReverseInvoiceWhenUserHasCreditAdjustmentNotePostingApprovalLevelsSecurity_ExistingAPPAndREQRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(1, approvalRequestForInvoice1.Length);
			var previousApprovalRequestForInvoice1 = approvalRequestForInvoice1[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, previousApprovalRequestForInvoice1.XP_ApprovalStatus);
			AssertEquals(1, approvalRequestForInvoice2.Length);
			var previousApprovalRequestForInvoice2 = approvalRequestForInvoice2[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, previousApprovalRequestForInvoice2.XP_ApprovalStatus);

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(invoice1InNewFactory.IsReversed);
					Assert(invoice2InNewFactory.IsReversed);
					AssertEquals(1, approvalRequestForInvoice1.Length);
					AssertEquals("New request not created", previousApprovalRequestForInvoice1.PK, approvalRequestForInvoice1[0].PK);
					AssertEquals("Previous request posted", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForInvoice1[0].XP_ApprovalStatus);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals("New request not created", previousApprovalRequestForInvoice2.PK, approvalRequestForInvoice2[0].PK);
					AssertEquals("Previous request cancelled", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestForInvoice2[0].XP_ApprovalStatus);
				}
			}
		}

		public void TestReverseInvoiceWhenUserHasCreditAdjustmentNotePostingApprovalLevelsSecurity()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(0, approvalRequestForInvoice1.Length);
			AssertEquals(0, approvalRequestForInvoice2.Length);

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(invoice1InNewFactory.IsReversed);
					Assert(invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(0, approvalRequestForInvoice1.Length);
					AssertEquals(0, approvalRequestForInvoice2.Length);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndAllExisitingApprovalRequestsAreAlreadyApproved()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(1, approvalRequestForInvoice1.Length);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice1[0].XP_ApprovalStatus);
			AssertEquals(1, approvalRequestForInvoice2.Length);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice2[0].XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(invoice1InNewFactory.IsReversed);
					Assert(invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(1, approvalRequestForInvoice1.Length);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForInvoice1[0].XP_ApprovalStatus);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForInvoice2[0].XP_ApprovalStatus);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndProvidesSpotOnAuthorisation()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(0, approvalRequestForInvoice1.Length);
			AssertEquals(0, approvalRequestForInvoice2.Length);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "US1", "User1", "pass");

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							loginForm.DoLoginForTest("User1", "pass");
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
						}
					});
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(invoice1InNewFactory.IsReversed);
					Assert(invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(0, approvalRequestForInvoice1.Length);
					AssertEquals(0, approvalRequestForInvoice2.Length);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndProvidesSpotOnAuthorisation_ThereAreExisitngRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(1, approvalRequestForInvoice1.Length);
			var previousApprovalRequestForInvoice1 = approvalRequestForInvoice1[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, previousApprovalRequestForInvoice1.XP_ApprovalStatus);
			AssertEquals(1, approvalRequestForInvoice2.Length);
			var previousApprovalRequestForInvoice2 = approvalRequestForInvoice2[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, previousApprovalRequestForInvoice1.XP_ApprovalStatus);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "US1", "User1", "pass");

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							loginForm.DoLoginForTest("User1", "pass");
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
						}
					});
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(invoice1InNewFactory.IsReversed);
					Assert(invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(1, approvalRequestForInvoice1.Length);
					AssertEquals("New request not created", previousApprovalRequestForInvoice1.PK, approvalRequestForInvoice1[0].PK);
					AssertEquals("Previous request cancelled", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestForInvoice1[0].XP_ApprovalStatus);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals("New request not created", previousApprovalRequestForInvoice2.PK, approvalRequestForInvoice2[0].PK);
					AssertEquals("Previous request cancelled", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestForInvoice2[0].XP_ApprovalStatus);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndCancelsWithoutProvidingSpotOnAuthorisation()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(0, approvalRequestForInvoice1.Length);
			AssertEquals(0, approvalRequestForInvoice2.Length);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; //user cancel out from login screen
						}
					});
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(!invoice1InNewFactory.IsReversed);
					Assert(!invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(0, approvalRequestForInvoice1.Length);
					AssertEquals(0, approvalRequestForInvoice2.Length);
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can reverse these transactions."));
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreUnapprovedApprovalRequests_UserSelectsToCancelExistingRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(1, approvalRequestForInvoice1.Length);
			var previousApprovalRequestForInvoice1 = approvalRequestForInvoice1[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, previousApprovalRequestForInvoice1.XP_ApprovalStatus);
			AssertEquals(1, approvalRequestForInvoice2.Length);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice2[0].XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
					});
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //user wants to cancel exisitng requests
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(!invoice1InNewFactory.IsReversed);
					Assert(!invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(2, approvalRequestForInvoice1.Length);
					AssertEquals("Previous request cancelled", previousApprovalRequestForInvoice1.PK, approvalRequestForInvoice1.First(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled).PK);
					AssertNotEquals("New request created", previousApprovalRequestForInvoice1.PK, approvalRequestForInvoice1.First(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested).PK);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals("Approved request remains approved", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice2[0].XP_ApprovalStatus);
					AssertEquals("You do not have security rights to post a credit note for the required amount. An approval request has been queued. Once the approval has been granted, you can reverse these transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreUnapprovedApprovalRequests_UserDoesNotWantToCancelExistingRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			Factory.Save();

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(1, approvalRequestForInvoice1.Length);
			var previousApprovalRequestForInvoice1 = approvalRequestForInvoice1[0];
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, previousApprovalRequestForInvoice1.XP_ApprovalStatus);
			AssertEquals(1, approvalRequestForInvoice2.Length);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice2[0].XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
					});
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //user does not want to cancel existing requests
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(!invoice1InNewFactory.IsReversed);
					Assert(!invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(1, approvalRequestForInvoice1.Length);
					AssertEquals("New request not created", previousApprovalRequestForInvoice1.PK, approvalRequestForInvoice1[0].PK);
					AssertEquals("Previous request not cancelled", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForInvoice1[0].XP_ApprovalStatus);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals("Approved request remains approved", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForInvoice2[0].XP_ApprovalStatus);
				}
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreNoUnapprovedApprovalRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			var approvalRequestForInvoice1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
			var approvalRequestForInvoice2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
			AssertEquals(0, approvalRequestForInvoice1.Length);
			AssertEquals(0, approvalRequestForInvoice2.Length);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var shipment = Factory.Load<ForwardingShipment>(job.PlugInData.PK);
				using (var pluginToFreight = new InvoicingPluginToFreightWithReversingReasonProvided(shipment))
				{
					pluginToFreight.Job_ForTestOnly = job;
					SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
					});
					pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var newFactory = new BusinessObjectFactory();
					var invoice1InNewFactory = newFactory.Load<InvoicingBase>(invoice1.PK);
					var invoice2InNewFactory = newFactory.Load<InvoicingBase>(invoice2.PK);
					Assert(!invoice1InNewFactory.IsReversed);
					Assert(!invoice2InNewFactory.IsReversed);
					approvalRequestForInvoice1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice1.PK));
					approvalRequestForInvoice2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoice2.PK));
					AssertEquals(1, approvalRequestForInvoice1.Length);
					AssertEquals(1, approvalRequestForInvoice2.Length);
					AssertEquals("You do not have security rights to post a credit note for the required amount. An approval request has been queued. Once the approval has been granted, you can reverse these transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		AuthorizationModeAndSettings GetAuthorisationConfigSetting()
		{
			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			return setting;
		}

		ARCreditNoteApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
			approvalRequest.XP_ReasonDescription = "Invoice Reverse Testing";
			return approvalRequest;
		}

		class InvoicingPluginToFreightWithReversingReasonProvided : InvoicingPluginToFreight
		{
			public InvoicingPluginToFreightWithReversingReasonProvided(IBusiness hostEntity) : base(hostEntity)
			{ }

			protected override void GetReversingReason(ref string reversingReason, ref string reversingCode)
			{
				reversingReason = "Test Invoice Reversal";
				reversingCode = "TST";
			}
		}

		#endregion

		public void TestInvoicingPluginGUIExcludingConsolContextIsNotSetWhenHostBusinessEntityIsForwardingConsol()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Assert("Factory should not have InvoicingPluginGUIExcludingConsol context yet.", !shipment.Factory.HasContext(BusinessContext.InvoicingPluginGUIExcludingConsol));
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				Assert("Factory must have InvoicingPluginGUIExcludingConsol context.", shipment.Factory.HasContext(BusinessContext.InvoicingPluginGUIExcludingConsol));
			}
			Assert("Factory should not have InvoicingPluginGUIExcludingConsol context after form is closed.", !shipment.Factory.HasContext(BusinessContext.InvoicingPluginGUIExcludingConsol));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (var plugin = new InvoicingPluginToFreight(consol))
			{
				Assert("Factory must not have InvoicingPluginGUIExcludingConsol context because host business entity is a consol.", !consol.Factory.HasContext(BusinessContext.InvoicingPluginGUIExcludingConsol));
			}
		}

		public void TestGetJobDescription_InvoicingPluginToFreightDeleted()
		{
			var cartage = Factory.NewWithValidTestData<Freight.LocalCartage.Business.CommonCartage>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var job = new Job.Loader(shipment).TryCreateWithMutex();
			job.PlugInData = shipment;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_ParentID = cartage.PK;

			using (var plugin = new InvoicingPluginToFreight(cartage))
			{
				plugin.Job_ForTestOnly = job;
				cartage.Delete();
				AssertNoExceptionThrown("Cannot access the property of a deleted CommonCartage row.", () => plugin.JobDescription_ForTestOnly.GetType());
			}
		}

		public void TestCaptionWithCrossTradeAndRegistry()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "NZAKL", "FRLYO", null, true);

			using (ZForm form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				var tabControl = new ZTemplateTabControl();
				var tabPage1 = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();

				var jobChargeUserControl = ((JobInvoicingUserControl)plugin.UserControl).JobChargeUserControl;
				using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					tabControl.SelectedTab = plugin.TabPage;
					plugin.OnGUIShown();
					AssertEquals("Caption is Local Client", "Local Client", jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text);
					AssertEquals("Caption is Overseas Agent", "Overseas Agent", jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text);
					AssertEquals("Help bubble is Local Client", jobChargeUserControl.Job.JH_OA_LocalChargesAddrCaption, jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString);
					AssertEquals("Help bubble is Overseas Agent", jobChargeUserControl.Job.JH_OA_AgentCollectAddrCaption, jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString);
				}

				plugin.OnGUIShown();
				AssertEquals("With registry, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text);
				AssertEquals("With registry, caption is Collect Bill-To Party", "Collect Bill-To Party", jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text);
				AssertEquals("With registry, help bubble is Prepaid Bill-To Party", jobChargeUserControl.Job.PrepaidBillToPartyCaption, jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString);
				AssertEquals("With registry, help bubble is Collect Bill-To Party", jobChargeUserControl.Job.CollectBillToPartyCaption, jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString);

				shipment.JS_RL_NKOrigin = "AUSYD";
				plugin.OnGUIShown();
				AssertEquals("When change back to home origin, caption is Local Client", "Local Client", jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text);
				AssertEquals("When change back to home origin, caption is Overseas Agent", "Overseas Agent", jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text);
				AssertEquals("When change back to home origin, help bubble is Local Client", jobChargeUserControl.Job.JH_OA_LocalChargesAddrCaption, jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString);
				AssertEquals("When change back to home origin, help bubble is Overseas Agent", jobChargeUserControl.Job.JH_OA_AgentCollectAddrCaption, jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString);

				shipment.JS_RL_NKOrigin = "FRLYO";
				shipment.JS_RL_NKDestination = "NZAKL";
				plugin.OnGUIShown();
				AssertEquals("When change to cross trade job, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text);
				AssertEquals("When change to cross trade job, caption is Collect Bill-To Party", "Collect Bill-To Party", jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text);
				AssertEquals("When change to cross trade job, help bubble is Prepaid Bill-To Party", jobChargeUserControl.Job.PrepaidBillToPartyCaption, jobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString);
				AssertEquals("When change to cross trade job, help bubble is Collect Bill-To Party", jobChargeUserControl.Job.CollectBillToPartyCaption, jobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString);
			}
		}

		public void TestMarkJobHeaderAsInactiveWhenJobChargeInDifferentCompany()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var otherBranch = TestObjectCreator.NonCurrentCompanyBranch;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var shipmentReloaded = newFactory.Load<ForwardingShipment>(shipment.PK);
				var job2 = new Job.Loader(shipmentReloaded).TryCreateWithoutMutexForTestOnly();

				var creator = new TestObjectCreator(newFactory);
				var charge = creator.CreateCharge(job2, chargeCode: creator.CC1, osCostAmt: 100m, creditor: creator.ABIGAS, osSellAmt: 0m, debtor: creator.Debtor1);

				newFactory.Save();
			}

			using (var plugIn = new InvoicingPluginForTest(shipment))
			{
				plugIn.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals("Should ask user to confirm invoice mark as inactive. No errors should be found.", "Please type 'YES' to confirm deactivation of job.\r\nOn saving, the job will be deactivated and the form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowOverrideFormToPopUp()
		{
			OrgHeader localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			OrgHeader localClient2 = TestObjectCreator.CreateOrgHeader("XCLORG", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(localClient2, "3rd Street");

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");
			var contact3 = TestObjectCreator.CreateContact(localClient2, "contact 3");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				ValueChangedEventArgs e = new ValueChangedEventArgs(address1.PK, job.JH_OA_LocalChargesAddrInfo);
				Assert("Should Allow", plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, address2.Header));
				Assert("Shouldn't Allow. DIfferent Organization", !plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, address3.Header));

				e = new ValueChangedEventArgs(contact1.PK, job.JH_OC_LocalBillingContactInfo);
				Assert("Should Allow", plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, contact2.Header));
				Assert("Shouldn't Allow. DIfferent Organization", !plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, contact3.Header));

				e = new ValueChangedEventArgs(address2.PK, job.JH_OA_AgentCollectAddrInfo);
				Assert("Should Allow", plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, address1.Header));
				Assert("Shouldn't Allow. DIfferent Organization", !plugin.AllowOverrideFormToPopUp_ForTestOnly(job, e, address3.Header));
			}
		}

		public void TestGatewaySellIsSynchronisedWithConsolCostWhenInvoicingTabIsDeselecting()
		{
			var consol = TestObjectCreator.CreateConsol("DEFRA", "AUSYD", "C00001111");
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "DEFRA";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			TestObjectCreator.CreateShipment("S00001111", consol);

			Factory.Save();

			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.PlugIns.Add(ControllerIDs.SellApportionmentForGateway);
				form.Show();

				var invoicingPlugIn = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				var job = invoicingPlugIn.Job_ForTestOnly;

				AssertNotNull(job);

				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				charge.JR_JH_InternalJob = job.PK;

				tabControl.SelectNextTabPage();

				var apportionments = consol.GetApportionments(true);
				AssertEquals(1, apportionments.CostsCollection.Count);
				AssertEquals(charge.JR_AC, apportionments.CostsCollection[0].E6_AC_ChargeCode);
				AssertEquals(charge.JR_OSSellAmt, apportionments.CostsCollection[0].E6_OSCostAmount);
				AssertEquals(apportionments.CostsCollection[0].PK, charge.JR_E6_GatewaySellHeader);
			}
		}

		public void TestSynchronizingGatewaySellApptChargeWhenJobLockedAndOpenSellApptTab()
		{
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C1", true);
			var shipment = creator.CreateShipment("S1", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment).Load());

			using (var shipmentJob = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			using (var form = new ZForm(consol))
			{
				AssertNotNull("Precondition: shipment job is locked by another Factory", shipmentJob);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.Show();

				tabControl.SelectNextTabPage();
				AssertNoExceptionThrown(() => tabControl.SelectNextTabPage());
				AssertEquals("You have created the job S1 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S1 to continue.",
					form.PlugIns.GetPlugIn(ControllerIDs.Apportionment).PlugInNotDisplayedMessage);
			}
		}

		public void TestNotFetchARandAPIvnoicesWhenFormLoading()
		{
			JobStorage whsInvoice = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));

			using (var form = new ZForm(whsInvoice))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				var control = plugin.UserControl as JobInvoicingUserControl;
				form.Controls.Add(control.JobInvoicingTabControl_ForTest);

				form.Show();

				AssertNull("AP Printing Filter should be null.", plugin.FAPPrintingFilter_ForTestOnly);
				AssertNull("AR Printing Filter should be null.", plugin.FARPrintingFilter_ForTestOnly);

				control.JobInvoicingTabControl_ForTest.SelectedIndex = 2;
				AssertNotNull("AR Printing Filter should not be null.", plugin.FARPrintingFilter_ForTestOnly);
				AssertNull("AP Printing Filter should be null.", plugin.FAPPrintingFilter_ForTestOnly);

				control.JobInvoicingTabControl_ForTest.SelectedIndex = 3;
				AssertNotNull("AP Printing Filter should not be null.", plugin.FAPPrintingFilter_ForTestOnly);
				AssertNotNull("AP Printing Filter should not be null.", plugin.FARPrintingFilter_ForTestOnly);
			}
		}

		public void TestNotFetchARandApInvoicesWhenTabNotInitialized()
		{
			JobStorage whsInvoice = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));

			using (var form = new ZForm(whsInvoice))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				var control = plugin.UserControl as JobInvoicingUserControl;
				form.Controls.Add(control.JobInvoicingTabControl_ForTest);

				form.Show();

				AssertNull("AP Printing Filter should be null.", plugin.FAPPrintingFilter_ForTestOnly);
				AssertNull("AR Printing Filter should be null.", plugin.FARPrintingFilter_ForTestOnly);

				plugin.SafeRefreshInvoiceList_ForTestOnly();

				AssertNull("AP Printing Filter should be null.", plugin.FAPPrintingFilter_ForTestOnly);
				AssertNull("AR Printing Filter should be null.", plugin.FARPrintingFilter_ForTestOnly);
			}
		}

		public void TestRequestCashAdvanceMenuItemClick()
		{
			var localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_LocalChargesAddr = address1.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;
			job.JH_OA_AgentCollectAddr = address1.PK;

			Factory.Save();

			using (var shipmentJob = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			using (var form = new ZForm(shipment))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				job.JH_Description = "new desc";
				invoicingPlugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());
				AssertEquals(string.Format("Please save job {0} before creating Advance Payment request.", job.JH_JobNum), UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				invoicingPlugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("No pending charges requiring a Advance Payment were found.", UnitTestUserNotification.Instance.LastMessage.Text);

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_OSCostAmt = 10.23m;
				charge2.JR_OSSellAmt = 10.23m;
				charge2.JR_IsARCashAdvance = true;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;

				var charge3 = job.Charges.AddNew();
				charge3.JR_AC = TestObjectCreator.CC1.PK;
				charge3.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge3.JR_OSCostAmt = 88.99m;
				charge3.JR_OSSellAmt = 88.99m;
				charge3.JR_IsARCashAdvance = true;
				charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				invoicingPlugin.MenuItemRequestCashAdvance_Click_ForTestOnly(null, new EventArgs());
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"No Advance Payment Request was generated for the following charges, because the Invoice Type is 'NON - Not for Invoicing'.
ZZCC1, 10.23, AUD, ABIGAS, ABI GAS & TOOLS"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"The following Advance Payment requests have been generated:
Debtor/Request ID/Currency/Total Amount
ABIGAS, 00001000, AUD, 88.99"));
			}
		}

		public void TestMarkJobAsInactiveWhenChildShipmentsJobLocked()
		{
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol = creator.CreateGatewayConsol("KRSEL", "AUSYD", "C1", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = creator.CreateShipment("S1", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment).Load());

			using (var shipmentJob = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			using (var form = new ZForm(consol))
			{
				AssertNotNull("Precondition: shipment job is locked by another Factory", shipmentJob);

				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoicingPlugin.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(null, new EventArgs());

				Assert("Mark as inactive is blocked because the child job is locked", UnitTestUserNotification.Instance.LastMessage.Text.Contains("on another form, but haven't saved it yet"));
			}
		}

		[ExpectNoExceptions]
		public void TestDoNotAccessDeletedJob()
		{
			var consol = TestObjectCreator.CreateConsol("DEFRA", "AUSYD", "C00001111");
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "DEFRA";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			TestObjectCreator.CreateShipment("S00001111", consol);

			Factory.Save();

			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.PlugIns.Add(ControllerIDs.SellApportionmentForGateway);
				form.Show();

				var invoicingPlugIn = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				var job = invoicingPlugIn.Job_ForTestOnly;
				AssertNotNull(job);

				job.Delete();
				Assert(job.IsDeleted);
				tabControl.SelectNextTabPage();
			}
		}

		[ExpectNoExceptions]
		public void TestLocalAddrAndContactValueChanged_IsInTransaction()
		{
			OrgHeader localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(localClient, "3nd Street");

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");
			var contact3 = TestObjectCreator.CreateContact(localClient, "contact 3");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_LocalChargesAddr = address1.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;
			job.JH_OA_AgentCollectAddr = address1.PK;

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			Factory.Save();

			JobCharge jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, arInvoiceLine.PK));
			AssertEquals("Precondition: invoice and job charge address is overridden", arInvoice.AH_OA_InvoiceAddressOverride, jobCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: invoice and job charge contact is overridden", arInvoice.AH_OC_InvoiceContactOverride, jobCharge.JR_OC_SellInvoiceContact);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			Assert("Precondition: there is atleast one invoice with local client", jobInNewFactory.IsAnyCostOrRevenuePosted(localClient.PK));

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				Type formType = (typeof(OverrideInvoiceDetailsForm));
				FieldInfo closeButtonField = formType.GetField("CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);

				//JH_OA_LocalChargesAddr
				job.JH_OA_LocalChargesAddr = address2.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				ZButton closeButton = (ZButton)closeButtonField.GetValue(ZFormModaliser.ActiveForm);
				AssertNotNull(closeButton);
				closeButton.PerformClick();
				Factory.Save();

				Db.Connection.BeginTransaction(); // Test the value changed inside a transaction
				job.JH_OA_LocalChargesAddr = address3.PK; //There was an exception
				Db.Connection.CommitTransaction(); // Test the value changed inside a transaction

				AssertNull("Shouldn't be showing the address/contact override form as address was changed inside a transaction", ZFormModaliser.ActiveForm);
				AssertEquals("Job local client address is changed as a result of user action", address3.PK, job.JH_OA_LocalChargesAddr);
				Factory.Save();

				//JH_OC_LocalBillingContact
				job.JH_OC_LocalBillingContact = contact2.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());
				closeButton = (ZButton)closeButtonField.GetValue(ZFormModaliser.ActiveForm);
				AssertNotNull(closeButton);
				closeButton.PerformClick();
				Factory.Save();

				Db.Connection.BeginTransaction(); // Test the value changed inside a transaction
				job.JH_OC_LocalBillingContact = contact3.PK; //There was an exception
				Db.Connection.CommitTransaction(); // Test the value changed inside a transaction

				AssertNull("Shouldn't be showing the address/contact override form as contract was changed inside a transaction", ZFormModaliser.ActiveForm);
				AssertEquals("Job local client contact is changed as a result of user action", contact3.PK, job.JH_OC_LocalBillingContact);
				Factory.Save();

				//JH_OA_AgentCollectAddr
				job.JH_OA_AgentCollectAddr = address2.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());
				closeButton = (ZButton)closeButtonField.GetValue(ZFormModaliser.ActiveForm);
				AssertNotNull(closeButton);
				closeButton.PerformClick();
				Factory.Save();

				Db.Connection.BeginTransaction(); // Test the value changed inside a transaction
				job.JH_OA_AgentCollectAddr = address3.PK; //There was an exception
				Db.Connection.CommitTransaction(); // Test the value changed inside a transaction

				AssertNull("Shouldn't be showing the address/contact override form as address was changed inside a transaction", ZFormModaliser.ActiveForm);
				AssertEquals("Job agent collect address is changed as a result of user action", address3.PK, job.JH_OA_AgentCollectAddr);
				Factory.Save();
			}
		}

		public void TestUpdatingLocalClientAddressContact_CaseInvoiceLinkedToOnlyCurrentJob()
		{
			OrgHeader localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			OrgHeader localClient2 = TestObjectCreator.CreateOrgHeader("XCLORG", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(localClient2, "3rd Street");

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");
			var contact3 = TestObjectCreator.CreateContact(localClient2, "contact 3");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_LocalChargesAddr = address1.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;
			arInvoice.AH_OC_InvoiceContactOverride = contact1.PK;

			Factory.Save();

			JobCharge jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, arInvoiceLine.PK));
			AssertEquals("Precondition: invoice and job charge address is overridden", arInvoice.AH_OA_InvoiceAddressOverride, jobCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: invoice and job charge contact is overridden", arInvoice.AH_OC_InvoiceContactOverride, jobCharge.JR_OC_SellInvoiceContact);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			Assert("Precondition: there is atleast one invoice with local client", jobInNewFactory.IsAnyCostOrRevenuePosted(localClient.PK));

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				job.JH_OA_LocalChargesAddr = address2.PK;

				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());
				foreach (InvoicingBase invoice in ((OverrideInvoiceDetailsHelper)((OverrideInvoiceAddressContactForm)ZFormModaliser.ActiveForm).BusinessEntity).WrappedObjects)
				{
					AssertEquals("Default value of invoice.AH_OA_InvoiceAddressOverride updated", address2.PK, invoice.AH_OA_InvoiceAddressOverride);
				}

				Type formType = (typeof(OverrideInvoiceDetailsForm));
				FieldInfo closeButtonField = formType.GetField("CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo continueButtonField = formType.GetField("ContinueButton", BindingFlags.NonPublic | BindingFlags.Instance);

				ZButton closeButton = (ZButton)closeButtonField.GetValue(ZFormModaliser.ActiveForm);
				ZButton continueButton = (ZButton)continueButtonField.GetValue(ZFormModaliser.ActiveForm);

				AssertNotNull(closeButton);
				AssertNotNull(continueButton);

				continueButton.PerformClick();

				Factory.Save();

				AssertEquals("Job local client address is changed as a result of user action", address2.PK, job.JH_OA_LocalChargesAddr);
				AssertEquals("Invoice overridden address should be changed as Job local client address is changed", address2.PK, arInvoice.AH_OA_InvoiceAddressOverride);
				AssertEquals("Job charge sell address should be changed as Job local client address is changed", address2.PK, jobCharge.JR_OA_SellInvoiceAddress);

				job.JH_OC_LocalBillingContact = contact2.PK;

				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				closeButton.PerformClick();

				Factory.Save();

				AssertEquals("Job local client contact is changed as a result of user action", contact2.PK, job.JH_OC_LocalBillingContact);
				AssertEquals("Invoice overridden contact is not changed as user cancelled the operation", contact1.PK, arInvoice.AH_OC_InvoiceContactOverride);
				AssertEquals("Job charge sell address is not changed as user cancelled the operation", contact1.PK, jobCharge.JR_OC_SellInvoiceContact);

				ZFormModaliser.ActiveForm.Close();

				job.JH_OA_LocalChargesAddr = address3.PK;
				AssertNull("Shouldn't be showing the address/contact override form as it is a different Organization", ZFormModaliser.ActiveForm);

				job.JH_OC_LocalBillingContact = contact3.PK;
				AssertNull("Shouldn't be showing the address/contact override form as it is a different Organization", ZFormModaliser.ActiveForm);
			}
		}

		public void TestUpdatingOverseasAgentAddressContact_CaseInvoiceLinkedToOnlyCurrentJob()
		{
			OrgHeader overseasAgent = TestObjectCreator.CreateOrgHeader("OVAGNT", false, true);
			OrgHeader overseasAgent2 = TestObjectCreator.CreateOrgHeader("OVAORG", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(overseasAgent, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(overseasAgent, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(overseasAgent2, "Ovr. Street");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = overseasAgent.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_AgentCollectAddr = address1.PK;

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;

			Factory.Save();

			JobCharge jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, arInvoiceLine.PK));
			AssertEquals("Precondition: invoice and job charge address is overridden", arInvoice.AH_OA_InvoiceAddressOverride, jobCharge.JR_OA_SellInvoiceAddress);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			Assert("Precondition: there is atleast one invoice with overseas agent", jobInNewFactory.IsAnyCostOrRevenuePosted(overseasAgent.PK));

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				job.JH_OA_AgentCollectAddr = address2.PK;

				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());
				foreach (InvoicingBase invoice in ((OverrideInvoiceDetailsHelper)((OverrideInvoiceAddressContactForm)ZFormModaliser.ActiveForm).BusinessEntity).WrappedObjects)
				{
					AssertEquals("Default value of invoice.AH_OA_InvoiceAddressOverride updated", address2.PK, invoice.AH_OA_InvoiceAddressOverride);
				}

				Type formType = (typeof(OverrideInvoiceDetailsForm));
				FieldInfo closeButtonField = formType.GetField("CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo continueButtonField = formType.GetField("ContinueButton", BindingFlags.NonPublic | BindingFlags.Instance);

				ZButton closeButton = (ZButton)closeButtonField.GetValue(ZFormModaliser.ActiveForm);
				ZButton continueButton = (ZButton)continueButtonField.GetValue(ZFormModaliser.ActiveForm);

				AssertNotNull(closeButton);
				AssertNotNull(continueButton);

				continueButton.PerformClick();

				Factory.Save();

				AssertEquals("Job overseas agent address is changed as a result of user action", address2.PK, job.JH_OA_AgentCollectAddr);
				AssertEquals("Invoice overridden address should be changed as Job overseas agent address is changed", address2.PK, arInvoice.AH_OA_InvoiceAddressOverride);
				AssertEquals("Job charge sell address should be changed as Job overseas agent address is changed", address2.PK, jobCharge.JR_OA_SellInvoiceAddress);

				job.JH_OA_AgentCollectAddr = address1.PK;

				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				closeButton.PerformClick();

				Factory.Save();

				AssertEquals("Job overseas agent address is changed as a result of user action", address1.PK, job.JH_OA_AgentCollectAddr);
				AssertEquals("Invoice overridden contact is not changed as user cancelled the operation", address2.PK, arInvoice.AH_OA_InvoiceAddressOverride);
				AssertEquals("Job charge sell address is not changed as user cancelled the operation", address2.PK, jobCharge.JR_OA_SellInvoiceAddress);

				ZFormModaliser.ActiveForm.Close();
				job.JH_OA_AgentCollectAddr = address3.PK;
				AssertNull("Shouldn't be showing the address/contact override form as it is a different Organization", ZFormModaliser.ActiveForm);
			}
		}

		public void TestUpdatingLocalClientAddressContact_CaseInvoiceLinkedToOnlyMultipleJobs()
		{
			OrgHeader localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;

			var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine1.AL_OH = localClient.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_LocalChargesAddr = address1.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S001002");
			var job1 = CreateJobWithCharge(shipment1, arInvoiceLine1);

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;
			arInvoice.AH_OC_InvoiceContactOverride = contact1.PK;

			Factory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				job.JH_OA_LocalChargesAddr = address2.PK;

				AssertNull("Should not be showing the address/contact override form as invoice has lines with different job", ZFormModaliser.ActiveForm);
				var msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice address for these invoices from Transactions module.", arInvoice.AH_TransactionNum);
				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);

				job.JH_OC_LocalBillingContact = contact2.PK;

				AssertNull("Should not be showing the address/contact override form as invoice has lines with different job", ZFormModaliser.ActiveForm);
				msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice contact for these invoices from Transactions module.", arInvoice.AH_TransactionNum);
				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdatingOverseasAgentAddressContact_CaseInvoiceLinkedToOnlyMultipleJobs()
		{
			OrgHeader overseasAgent = TestObjectCreator.CreateOrgHeader("OVAGNT", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(overseasAgent, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(overseasAgent, "2nd Street");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);

			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = overseasAgent.PK;

			var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine1.AL_OH = overseasAgent.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);

			job.JH_OA_AgentCollectAddr = address1.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S001002");
			var job1 = CreateJobWithCharge(shipment1, arInvoiceLine1);

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;
			Factory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				job.JH_OA_AgentCollectAddr = address2.PK;

				AssertNull("Should not be showing the address/contact override form as invoice has lines with different job", ZFormModaliser.ActiveForm);
				var msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice address for these invoices from Transactions module.", arInvoice.AH_TransactionNum);

				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdatingLocalClientAddressContact_CaseInvoiceLinkedToBothCurrentAndMultipleJobs()
		{
			OrgHeader localClient = TestObjectCreator.CreateOrgHeader("LCLORG", false, true);
			OrgHeader localClient2 = TestObjectCreator.CreateOrgHeader("LCLNEW", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(localClient2, "3rd Street");

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");
			var contact3 = TestObjectCreator.CreateContact(localClient2, "contact 3");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = localClient.PK;

			var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine1.AL_OH = localClient.PK;

			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine2.AL_OH = localClient.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);
			AddAnotherCharge(job, arInvoiceLine2);

			job.JH_OA_LocalChargesAddr = address1.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S001002");
			var job1 = CreateJobWithCharge(shipment1, arInvoiceLine1);

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;
			arInvoice.AH_OC_InvoiceContactOverride = contact1.PK;

			Factory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				//address change
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				job.JH_OA_LocalChargesAddr = address2.PK;

				AssertNull("This time a yesno prompt will appear and should not be showing the address/contact override form as as user selected No", ZFormModaliser.ActiveForm);
				var msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice address for these invoices from Transactions module.

There are however other invoices only related to this job. Do you want to continue to update address for those?", arInvoice.AH_TransactionNum);

				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				arInvoice2.AH_OA_InvoiceAddressOverride = address2.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				job.JH_OA_LocalChargesAddr = address1.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				ZFormModaliser.ActiveForm.Close();

				//contact change
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				job.JH_OC_LocalBillingContact = contact2.PK;

				AssertNull("A yesno prompt will appear and should not be showing the address/contact override form as as user selected No", ZFormModaliser.ActiveForm);
				msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice contact for these invoices from Transactions module.

There are however other invoices only related to this job. Do you want to continue to update contact for those?", arInvoice.AH_TransactionNum);

				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				arInvoice2.AH_OC_InvoiceContactOverride = contact2.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				job.JH_OC_LocalBillingContact = contact1.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				ZFormModaliser.ActiveForm.Close();

				job.JH_OA_LocalChargesAddr = address3.PK;
				AssertNull("Shouldn't be showing the address override form, as organization is different", ZFormModaliser.ActiveForm);

				job.JH_OC_LocalBillingContact = contact3.PK;
				AssertNull("Shouldn't be showing the contact override form, as organization is different", ZFormModaliser.ActiveForm);
			}
		}

		public void TestUpdatingOverseasAgentAddress_CaseInvoiceLinkedToBothCurrentAndMultipleJobs()
		{
			OrgHeader overseasAgent = TestObjectCreator.CreateOrgHeader("OVAGNT", false, true);
			OrgHeader overseasAgent2 = TestObjectCreator.CreateOrgHeader("OVALoc", false, true);
			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(overseasAgent, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(overseasAgent, "2nd Street");
			var address3 = TestObjectCreator.CreateAddress(overseasAgent2, "3rd Street");

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);

			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine.AL_OH = overseasAgent.PK;

			var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine1.AL_OH = overseasAgent.PK;

			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M);
			var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1M, 120M);
			arInvoiceLine2.AL_OH = overseasAgent.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = CreateJobWithCharge(shipment, arInvoiceLine);
			AddAnotherCharge(job, arInvoiceLine2);

			job.JH_OA_AgentCollectAddr = address1.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S001002");
			var job1 = CreateJobWithCharge(shipment1, arInvoiceLine1);

			Factory.Save();

			arInvoice.SetContext(BusinessContext.OverrideInvoiceAddressContact);

			arInvoice.AH_OA_InvoiceAddressOverride = address1.PK;
			Factory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				job.JH_OA_AgentCollectAddr = address2.PK;

				AssertNull("This time a yesno prompt will appear and should not be showing the address/contact override form as as user selected No", ZFormModaliser.ActiveForm);
				var msg = string.Format(@"The job has following posted invoice(s) with lines that are related to other job(s):
{0}
Please update invoice address for these invoices from Transactions module.

There are however other invoices only related to this job. Do you want to continue to update address for those?", arInvoice.AH_TransactionNum);

				AssertEquals("Instead should be showing this message", msg, UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				arInvoice2.AH_OA_InvoiceAddressOverride = address2.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				job.JH_OA_AgentCollectAddr = address1.PK;
				AssertEquals("Should be showing the address/contact override form", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.ActiveForm.GetType());

				ZFormModaliser.ActiveForm.Close();

				job.JH_OA_AgentCollectAddr = address3.PK;
				AssertNull("Shouldn't be showing the address/contact override form as it is a different Organization", ZFormModaliser.ActiveForm);
			}
		}

		Job CreateJobWithCharge(ForwardingShipment shipment, InvoicingLineBase arInvoiceLine)
		{
			var job = TestObjectCreator.CreateJob(shipment, false, false);

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = TestObjectCreator.FRT.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_LocalSellAmt = 120M;
			jobCharge.JR_OSSellAmt = 120M;
			jobCharge.JR_AL_ARLine = arInvoiceLine.PK;

			arInvoiceLine.AL_JH = job.PK;

			return job;
		}

		void AddAnotherCharge(Job job, InvoicingLineBase arInvoiceLine)
		{
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = TestObjectCreator.FRT.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_LocalSellAmt = 120M;
			jobCharge.JR_OSSellAmt = 120M;
			jobCharge.JR_AL_ARLine = arInvoiceLine.PK;

			arInvoiceLine.AL_JH = job.PK;
		}

		#region Top Level Menu

		public void TestTopLevelMenu_CostInvoicingOnlySupported()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(false, true, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.CostsOnlyMenu", "Cost Only Menu"), "Cost Only Tab", "Zubs"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("Cost Only Menu", topLevelMenu.Text);

				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs and Zubs");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Zubs");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs");

				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllChargesAndCosts);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostLocalClientCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostOverseasAgentCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllRevenueCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostDisbursementChargesonly);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllSisterCompanyCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly);
				AssertMenuItemExists(false, topLevelMenu, "Create Job Revenue Journal");
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PreviewInvoices);

				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostCosts);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PreviewCosts);

				AssertMenuItemExists(true, topLevelMenu, "Mark Job Header as Inactive");

				AssertMenuItemExists(true, topLevelMenu, "Delete Unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reset Unposted lines Invoice Type");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
				AssertMenuItemExists(true, topLevelMenu, "Reset Default debtor on unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reverse Invoices and redo billing");
				AssertMenuItemExists(true, topLevelMenu, "Import AP invoices issued by other group companies");
				AssertMenuItemExists(false, topLevelMenu, "Recognize Revenue");
				AssertMenuItemExists(true, topLevelMenu, "Audit Billing");
				AssertMenuItemExists(false, topLevelMenu, "Create Profit Share Charges");
				AssertMenuItemExists(true, topLevelMenu, "Re-default Job Billing Exchange Rate");
			}
		}

		public void TestTopLevelMenu_RevenueInvoicingOnlySupported()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(true, false, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.RevOnlyMenu", "Rev Only Menu"), "Rev Only Tab", "Rakhsh"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("Rev Only Menu", topLevelMenu.Text);

				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs and Rakhsh");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Rakhsh");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs");

				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllChargesAndCosts);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostLocalClientCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostOverseasAgentCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllRevenueCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostDisbursementChargesonly);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllSisterCompanyCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly);
				AssertMenuItemExists(true, topLevelMenu, "Create Job Revenue Journal");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PreviewInvoices);

				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostCosts);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PreviewCosts);

				AssertMenuItemExists(true, topLevelMenu, "Mark Job Header as Inactive");

				AssertMenuItemExists(true, topLevelMenu, "Delete Unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reset Unposted lines Invoice Type");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
				AssertMenuItemExists(true, topLevelMenu, "Reset Default debtor on unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reverse Invoices and redo billing");
				AssertMenuItemExists(false, topLevelMenu, "Import AP invoices issued by other group companies");
				AssertMenuItemExists(true, topLevelMenu, "Recognize Revenue");
				AssertMenuItemExists(true, topLevelMenu, "Audit Billing");
				AssertMenuItemExists(true, topLevelMenu, "Create Profit Share Charges");
				AssertMenuItemExists(true, topLevelMenu, "Re-default Job Billing Exchange Rate");
			}
		}

		public void TestTopLevelMenu_CostAndRevenueInvoicingSupported()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(true, true, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.BothMenu", "Both Menu"), "Both Tab", "Lola"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("Both Menu", topLevelMenu.Text);

				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs and Lola");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Lola");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs");

				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllChargesAndCosts);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostLocalClientCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostOverseasAgentCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllRevenueCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostDisbursementChargesonly);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostAllSisterCompanyCharges);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly);
				AssertMenuItemExists(true, topLevelMenu, "Create Job Revenue Journal");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PreviewInvoices);

				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PostCosts);
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.PreviewCosts);

				AssertMenuItemExists(true, topLevelMenu, "Mark Job Header as Inactive");

				AssertMenuItemExists(true, topLevelMenu, "Delete Unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reset Unposted lines Invoice Type");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
				AssertMenuItemExists(true, topLevelMenu, "Reset Default debtor on unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reverse Invoices and redo billing");
				AssertMenuItemExists(true, topLevelMenu, "Import AP invoices issued by other group companies");
				AssertMenuItemExists(true, topLevelMenu, "Recognize Revenue");
				AssertMenuItemExists(true, topLevelMenu, "Audit Billing");
				AssertMenuItemExists(true, topLevelMenu, "Create Profit Share Charges");
				AssertMenuItemExists(true, topLevelMenu, "Re-default Job Billing Exchange Rate");
			}
		}

		public void TestTopLevelMenu_AutorateStandaloneShipmentMenuItemsAvailability()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs (Standalone with Consol Level Charge)");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs (Standalone with Consol Level Charge) and Revenue");
				AssertNotNull(plugin.MenuItemAutorateCostsSTS_ForTestOnly);
				AssertNotNull(plugin.MenuItemAutorateCostsSTSRevenue_ForTestOnly);

				Assert("Standalone Shipment", shipment.Consols.IsNullOrEmpty());
				Assert(plugin.MenuItemAutorateCostsSTS_ForTestOnly.Enabled);
				Assert(plugin.MenuItemAutorateCostsSTSRevenue_ForTestOnly.Enabled);

				shipment.Consols.AddNew();
				Assert("Consolidated Shipment", shipment.Consols.Any());
				Assert(!plugin.MenuItemAutorateCostsSTS_ForTestOnly.Enabled);
				Assert(!plugin.MenuItemAutorateCostsSTSRevenue_ForTestOnly.Enabled);

				shipment.Consols.RemoveAll();
				Assert("Standalone Shipment", shipment.Consols.IsNullOrEmpty());
				Assert(plugin.MenuItemAutorateCostsSTS_ForTestOnly.Enabled);
				Assert(plugin.MenuItemAutorateCostsSTSRevenue_ForTestOnly.Enabled);
			}
		}

		public void TestTopLevelMenu_PenaltyTaxInfoMenuItemAvailability()
		{
			var expectedMenuItemTextForKoreaSouth = "Additional Tax Information";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			AssertPenaltyTaxInfoMenuItemAvailability(shipment);

			var dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			Factory.Save();
			AssertPenaltyTaxInfoMenuItemAvailability(dec);

			var cartage = Factory.NewWithValidTestData<Freight.LocalCartage.Business.CommonCartage>();
			Factory.Save();
			AssertPenaltyTaxInfoMenuItemAvailability(cartage);

			var whsInvoice = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));
			Factory.Save();
			AssertPenaltyTaxInfoMenuItemAvailability(whsInvoice);

			var consol = TestObjectCreator.CreateConsol();
			AssertPenaltyTaxInfoMenuItemAvailability(consol);

			void AssertPenaltyTaxInfoMenuItemAvailability(IBusiness business)
			{
				using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, true))
				using (var plugin = new InvoicingPluginToFreight(business))
				{
					var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
					AssertMenuItemExists(true, topLevelMenu, expectedMenuItemTextForKoreaSouth);
				}

				using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, false))
				using (var plugin = new InvoicingPluginToFreight(business))
				{
					var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
					AssertMenuItemExists(false, topLevelMenu, expectedMenuItemTextForKoreaSouth);
				}
			}
		}

		public void TestTopLevelMenu_PenaltyTaxInfoMenuItemAvailability_OnGatewayConsol()
		{
			var expectedMenuItemTextForKoreaSouth = "Additional Tax Information";

			var receivingAgent = TestObjectCreator.CreateOrgHeader("GTWORG", false, true);
			var receivingAgentCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: receivingAgent);
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("CNSHA", "AUSYD", "C001", GlbCompany.CurrentCompany, receivingAgentCompany);
			AssertEquals("Pre-condition", true, gatewayConsol.IsGateway());

			using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, true))
			using (var form = new ConsolForm(gatewayConsol))
			{
				form.Show();

				var gatewayInvoicingMenuItem = form.Menu.MenuItems.FindByText("Gateway Invoicing");
				AssertMenuItemExists(true, gatewayInvoicingMenuItem, expectedMenuItemTextForKoreaSouth);

				var jobInvoicingMenuItem = form.Menu.MenuItems.FindByText("&Job Invoicing");
				AssertMenuItemExists(true, jobInvoicingMenuItem, expectedMenuItemTextForKoreaSouth);
			}

			using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, false))
			using (var form = new ConsolForm(gatewayConsol))
			{
				form.Show();

				var gatewayInvoicingMenuItem = form.Menu.MenuItems.FindByText("Gateway Invoicing");
				AssertMenuItemExists(false, gatewayInvoicingMenuItem, expectedMenuItemTextForKoreaSouth);

				var jobInvoicingMenuItem = form.Menu.MenuItems.FindByText("&Job Invoicing");
				AssertMenuItemExists(false, jobInvoicingMenuItem, expectedMenuItemTextForKoreaSouth);
			}
		}

		public void TestTopLevelMenu_AutorateStandaloneShipmentMenuItemsAvailability_OnConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (var plugin = new InvoicingPluginToFreight(consol))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertMenuItemExists(false, topLevelMenu, "Autorate Costs (Standalone with Consol Level Charge)");
				AssertMenuItemExists(false, topLevelMenu, "Autorate Costs (Standalone with Consol Level Charge) and Revenue");
				AssertNull(plugin.MenuItemAutorateCostsSTS_ForTestOnly);
				AssertNull(plugin.MenuItemAutorateCostsSTSRevenue_ForTestOnly);
			}
		}

		public void TestTopLevelMenu_MenuItemTextChangeOnCrossTrade()
		{
			//With Cross Trade Direction
			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL", null);

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();
				AssertNotNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(Constants.MenuNameConstants.PostPrepaidBillToParty, true));
				AssertNotNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(Constants.MenuNameConstants.PostCollectBillToParty, true));
			}

			//With Export Direction
			Shipment.JS_RL_NKOrigin = "AUSYD";

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();
				AssertNotNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(Constants.MenuNameConstants.PostLocalClientCharges, true));
				AssertNotNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(Constants.MenuNameConstants.PostOverseasAgentCharges, true));
			}
		}

		public void TestPreviewInvoiceSecurity()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(true, false, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.NonMenu", "None Menu"), "None Tab", "ZayRy"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("None Menu", topLevelMenu.Text);

				var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing, true);
				var previewCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
				previewCheckpoint.IsAllowed = false;

				var previewInvoicesMenuItem = topLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == Constants.MenuNameConstants.PreviewInvoices);
				previewInvoicesMenuItem?.PerformClick();

				AssertEquals(previewCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreviewOnlyInvoiceSecurity()
		{
			var testBusinessObject = new DummyJobInvoicingBusinessObject(Factory);
			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)testBusinessObject.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(true, false, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.NonMenu", "None Menu"), "None Tab", "ZayRy"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("None Menu", topLevelMenu.Text);

				var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing, true);
				var previewCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
				var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
				previewCheckpoint.IsAllowed = false;
				previewOnlyCheckpoint.IsAllowed = false;

				var previewInvoicesMenuItem = topLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == Constants.MenuNameConstants.PreviewInvoices);
				previewInvoicesMenuItem?.PerformClick();

				AssertEquals(previewCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				previewCheckpoint.IsAllowed = true;
				previewInvoicesMenuItem.PerformClick();
				AssertEquals(previewOnlyCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				previewOnlyCheckpoint.IsAllowed = true;
				previewInvoicesMenuItem.PerformClick();
				AssertNotEquals(previewOnlyCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTopLevelMenu_NoInvoicingSupported()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = (DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter;
				supporter.SetConsumerType(new DummyInvoicingConsumerType(false, false, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.NonMenu", "None Menu"), "None Tab", "ZayRy"));

				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertEquals("None Menu", topLevelMenu.Text);

				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs and ZayRy");
				AssertMenuItemExists(true, topLevelMenu, "Autorate ZayRy");
				AssertMenuItemExists(true, topLevelMenu, "Autorate Costs");

				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllChargesAndCosts);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostLocalClientCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostOverseasAgentCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllRevenueCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostDisbursementChargesonly);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostAllSisterCompanyCharges);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly);
				AssertMenuItemExists(false, topLevelMenu, "Create Job Revenue Journal");
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PreviewInvoices);

				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PostCosts);
				AssertMenuItemExists(false, topLevelMenu, Constants.MenuNameConstants.PreviewCosts);

				AssertMenuItemExists(true, topLevelMenu, "Mark Job Header as Inactive");

				AssertMenuItemExists(true, topLevelMenu, "Delete Unposted lines");
				AssertMenuItemExists(true, topLevelMenu, "Reset Unposted lines Invoice Type");
				AssertMenuItemExists(true, topLevelMenu, Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
				AssertMenuItemExists(true, topLevelMenu, "Reset Default debtor on unposted lines");
				AssertMenuItemExists(false, topLevelMenu, "Reverse Invoices and redo billing");
				AssertMenuItemExists(false, topLevelMenu, "Import AP invoices issued by other group companies");
				AssertMenuItemExists(false, topLevelMenu, "Recognize Revenue");
				AssertMenuItemExists(false, topLevelMenu, "Audit Billing");
				AssertMenuItemExists(false, topLevelMenu, "Create Profit Share Charges");
				AssertMenuItemExists(true, topLevelMenu, "Re-default Job Billing Exchange Rate");
			}
		}

		public void TestMarkJobHeaderAsInactiveMenuItemExist()
		{
			var dummy = new DummyJobInvoicingBusinessObject(Factory);
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				AssertMenuItemExists(true, topLevelMenu, "Mark Job Header as Inactive");
			}
		}

		public void TestRequestCashAdvancMenuItemExist()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			{
				var dummy = new DummyJobInvoicingBusinessObject(Factory);
				using (var plugin = new InvoicingPluginToFreight(dummy))
				{
					var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
					AssertMenuItemExists(false, topLevelMenu, "Request Advance Payment");
				}
			}
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				var dummy = new DummyJobInvoicingBusinessObject(Factory);
				using (var plugin = new InvoicingPluginToFreight(dummy))
				{
					var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
					AssertMenuItemExists(true, topLevelMenu, "Request Advance Payment");
				}
			}
		}

		void AssertMenuItemExists(bool expectedExists, MenuItem topLevelMenu, string expectedText)
		{
			AssertEquals(expectedExists, topLevelMenu.MenuItems.Cast<MenuItem>().Any(x => x.Text == expectedText));
		}

		class DummyInvoicingConsumerType : JobInvoicingConsumerType
		{
			public DummyInvoicingConsumerType(bool allowRevenue, bool allowCost, ResourceString menuName, string displayName, string revenueDesc)
				: base("DUM", ResString.GetMultilingualString("DummyTestConsumerType", "Dummy"))
			{
				this.allowRevenue = allowRevenue;
				this.allowCost = allowCost;
				this.menuName = menuName;
				this.displayName = displayName;
				this.revenueDesc = revenueDesc;
			}

			readonly bool allowRevenue = true;
			readonly bool allowCost = true;
			readonly ResourceString menuName = ResString.GetMultilingualString("DummyInvoicingConsumerType.MenuName", "Default Menu");
			readonly string displayName = "Default Display";
			readonly string revenueDesc = "Revenue";

			public override Type BizoType
			{
				get { return typeof(DummyJobInvoicingBusinessObject); }
			}

			public override ControllerID ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return Env.Security.Commodity; }
			}

			public override bool AllowRevenuePosting(IJobInvoicingPlugIn host)
			{
				return allowRevenue;
			}

			public override bool AllowCostPosting(IJobInvoicingPlugIn host)
			{
				return allowCost;
			}

			public override ResourceString MenuName(IJobInvoicingPlugIn host)
			{
				return menuName;
			}

			public override string DisplayName(IJobInvoicingPlugIn host)
			{
				return displayName;
			}

			public override string RevenueChargeDescription(IJobInvoicingPlugIn host)
			{
				return revenueDesc;
			}

			public override bool ShouldDisplayClientContractNumber(IJobInvoicingPlugIn host) => true;
		}

		#endregion

		public void TestExecuteAutoratingWithExistingAutoratingEvent()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var chargeCode = creator.CC1;
			var organisation = creator.ABIGAS;
			var currency = creator.AUD;
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "MAR";
			testUser.GS_LoginName = "MrAutoRating";
			testUser.GS_FullName = "Mr AutoRating";
			creator.NonCurrentBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();

			var newFactoryForLoading = new BusinessObjectFactory();
			shipment = newFactoryForLoading.Load<ForwardingShipment>(shipment.PK);

			using (var form = new ZForm(shipment))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				fAccountingModule.OnGUIShown(); // to init Job property

				var ratingJob = new Job.Loader(newFactoryForLoading, shipment).Load();
				var starter = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
				AssertEquals("Pre-condition: No Charges Should be Loaded", 0, ratingJob.Charges.Count);

				using (creator.NonCurrentBranch.SetAsTemporaryContext())
				{
					var factoryToSimulateSecondUser = new BusinessObjectFactory();
					{
						factoryToSimulateSecondUser.RefreshEnabled = false;
						var shipmentLoadedBySecondUser = factoryToSimulateSecondUser.Load<ForwardingShipment>(shipment.PK);
						var jobLoadedBySecondUser = new Job.Loader(shipmentLoadedBySecondUser).TryLoadOrCreateWithoutMutexForTestOnly();

						var autoratingLog = shipmentLoadedBySecondUser.Logs.AddNew(Events.ChargesHaveBeenAutoRated);
						using (autoratingLog.LockForUpdatingKeyFieldsForTesting())
						{
							autoratingLog.SL_GS_NKUser = "MAR";
							autoratingLog.SL_Reference = "Company: " + Env.CurrentCompany.Code;
							creator.CreateCharge(jobLoadedBySecondUser, chargeCode, "DESC", currency, 100M, organisation, currency, 100M, organisation);
							jobLoadedBySecondUser.LocalChargesPK = organisation.PK;
						}

						factoryToSimulateSecondUser.Save();
					}
				}

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals(false, ratingJob.IsAutoratingInProcess);
				AssertEquals("No Charges Should be Loaded", 0, ratingJob.Charges.Count);

				ZString expectedMessage = @"While you have been working with this form, another user has made changes.

User: Mr AutoRating (MAR) has run autorating on this job.
	
Do you want to load the changes they have made to the charges?";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				// Start a new autorating session
				starter = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				var actualMessage = UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Caption + " : " + x.Text);

				AssertCollectionContains("AutoRating Warning : " + expectedMessage, actualMessage);

				AssertEquals(false, ratingJob.IsAutoratingInProcess);
				AssertEquals("One Charge Should be Loaded", 1, ratingJob.Charges.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				starter = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				actualMessage = UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Caption + " : " + x.Text);

				AssertCollectionNotContains("AutoRating Warning : " + expectedMessage, actualMessage);
			}
		}

		public virtual void TestTextOverride()
		{
			var dummy = new DummyJobInvoicingBusinessObject();

			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				AssertEquals("Billing", plugin.TextOverride_ForTestOnly);
			}

			((DummyJobInvoicingBusinessObjectInvoicingSupporter)dummy.InvoicingSupporter).SetConsumerType(new DummyInvoicingConsumerType(true, true, ResString.GetMultilingualString("DummyInvoiceCOnsumerType.ZayMenu", "Zay"), "Ry", "Lola"));
			using (var plugin = new InvoicingPluginToFreight(dummy))
			{
				AssertEquals("Ry", plugin.TextOverride_ForTestOnly);
			}
		}

		public void TestDeleteJobWhenPlugInJobPKisDifferentFromPlugInHostBusinessEntityPK()
		{
			CommonShipment shipment1 = TestObjectCreator.CreateShipment("S0001");
			Job job = TestObjectCreator.CreateJob(shipment1, false);

			CommonShipment shipment2 = TestObjectCreator.CreateShipment("S0002");
			Job differentJob = TestObjectCreator.CreateJob(shipment2, false);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment1))
			{
				plugin.Job_ForTestOnly = differentJob;
				AssertNotEquals(plugin.Job_ForTestOnly.PK, ((BusinessObject)plugin.HostBusinessEntity_ForTestOnly).PK);

				AssertEquals(false, differentJob.IsDeleted);
				plugin.OnBusinessObjectIsCancelledChanged(ZBool.True);
				AssertEquals(true, differentJob.IsDeleted);
			}
		}

		public void TestNothingHappensWhenPlugInIsDisabled()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				AssertEquals("Precondition", false, job.IsDeleted);

				plugin.Enabled = false;
				plugin.OnBusinessObjectIsCancelledChanged(ZBool.True);
				AssertEquals("Should not delete Job if PlugIn is disabled.", false, job.IsDeleted);
			}
		}

		public void TestReversing()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "imraan.khan@cargowise.com";

			staff.Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff);

			group.Factory.Save();

			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USNYC";
			GlbDepartment fEA = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));

			CreateEmptyJobForShipment(shipment.PK);
			DummyJobInvoicingBusinessObject testBusinessObject = new DummyJobInvoicingBusinessObject(Factory, shipment.PK, "JobShipment");

			Job testJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = fEA.PK;
			testJob.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;
			AssertNoErrors(testJob);

			Charge charge1 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			charge1.JR_GB = testJob.JH_GB;
			charge1.JR_GE = testJob.JH_GE;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Charge charge2 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			charge2.JR_GB = testJob.JH_GB;
			charge2.JR_GE = testJob.JH_GE;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			Factory.Save();

			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				plugin.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());
				Factory.Save();

				// Need to post these manually so the next step will work.
				// MenuItemPostAll_Click doesn't post because testBusinessObject isn't in the db
				InvoicingPostManager postManager = new InvoicingPostManager(testJob);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals("Should be two invoices created", postManager.Poster.PostedInvoices.Count, 2);

				postManager.Factory.Save();

				plugin.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals("Should have sent 2 emails (one for each invoice)", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestDebtorsReDefaulting()
		{
			OrgHeader agent = TestObjectCreator.Agent;
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Factory.Save();

			TestObjectCreator.AALSHI.OH_IsDebtor = true;

			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			shipment[JobShipmentSchema.Constants.JS_RL_NKOrigin] = "AUSYD";
			shipment[JobShipmentSchema.Constants.JS_RL_NKDestination] = "USLAX";
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.Addresses[0].PK;
			job.JH_OA_AgentCollectAddr = agent.Addresses[0].PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			charge1.JR_OSSellAmt = 1m;
			charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

			using (ZForm form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage firstVisibleTabPage = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();

				AssertEquals("Prerequisite: debtor remains as set before", TestObjectCreator.ABIGAS.PK, charge1.JR_OH_SellAccount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipment[JobShipmentSchema.Constants.JS_INCO] = Enterprise.Core.Constants.IncoTerms.ExWorks;
				AssertEquals("Debtor should be redefaulted silently", TestObjectCreator.Agent.PK, charge1.JR_OH_SellAccount);
				AssertEquals("Debtor should be redefaulted silently", null, UnitTestUserNotification.Instance.LastMessage.Text);

				charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				plugin.HasGuiBeenShown_ForTestOnly = true;
				shipment[JobShipmentSchema.Constants.JS_INCO] = Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight;
				AssertEquals("Debtor should be redefaulted silently", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				shipment[JobShipmentSchema.Constants.JS_INCO] = Enterprise.Core.Constants.IncoTerms.ExWorks;
				AssertEquals("Debtor should not be redefaulted this time", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);
				plugin.OnSaving();
				AssertEquals("User should be prompted", @"The Incoterm has been changed.
Do you want the debtors to be re-defaulted on the billing tab?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Debtor should not be redefaulted after negative answer", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugin.OnSaving();
				AssertEquals("Confirmation message should be shown", @"The debtors on the billing tab have been re-defaulted. The data needs to be inspected to confirm that the operation has performed correctly.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Debtor should be redefaulted after confirmation", TestObjectCreator.Agent.PK, charge1.JR_OH_SellAccount);
			}
		}

		public void TestWrapperTransactionFactoryRefreshEnabled()
		{
			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1);
			AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			JobStorage whsInvoice = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));
			Job warehouseJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			warehouseJob.JH_ParentTableCode = whsInvoice.TablePrefix;
			warehouseJob.JH_ParentID = whsInvoice.PK;
			warehouseJob.JH_GC = GlbCompany.CurrentCompany.PK;
			warehouseJob.PlugInData = whsInvoice;
			warehouseJob.Parent = whsInvoice;

			TestObjectCreator creator = new TestObjectCreator(Factory);
			ZGuid chargeCode = creator.MRG100.PK;
			creator.AALSHI.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";
			creator.AALSHI.OH_IsDebtor = true;
			ZGuid debtor = creator.AALSHI.PK;

			AccTaxRate taxrate = Factory.NewWithValidTestData<AccTaxRate>();
			taxrate.AT_Type = AccTaxRate.Types.Rated;

			creator.CreateCharge(warehouseJob, creator.CC1, "test charge1", creator.AUD, 1500M, creator.AALSHI, "abc1234", creator.AUD, 100M, creator.ABIGAS);
			creator.CreateCharge(warehouseJob, creator.CC2, "test charge2", creator.AUD, 2000M, creator.AALSHI, "def1234", creator.AUD, 100M, creator.ABIGAS);
			Factory.Save();

			using (InvoicingPluginToFreight plugin1 = new InvoicingPluginToFreight(whsInvoice))
			using (plugin1.UserControl)
			{
				plugin1.OnGUIShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin1.PostTransactions_ForTestOnly(JobInvoicingPostingOption.Costs);
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals("For Warehouse job Refresh should be disabled", false, plugin1.InvoicingPostWrapper_ForTestOnly.TransactionFactoryRefreshEnabled_ForTestOnly);
			}

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var shipmentJob = creator.CreateJob(shipment, false);
			shipmentJob.JH_OA_AgentCollectAddr = creator.AALSHI.Addresses.First().PK;
			creator.CreateCharge(shipmentJob, creator.CC1, "test charge1", creator.AUD, 1500M, creator.AALSHI, "abc1234", creator.AUD, 0M, creator.ABIGAS);
			creator.CreateCharge(shipmentJob, creator.CC2, "test charge2", creator.AUD, 2000M, creator.AALSHI, "def1234", creator.AUD, 0M, creator.ABIGAS);
			Factory.Save();

			using (InvoicingPluginToFreight plugin2 = new InvoicingPluginToFreight(shipment))
			using (plugin2.UserControl)
			{
				plugin2.OnGUIShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin2.PostTransactions_ForTestOnly(JobInvoicingPostingOption.Costs);
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals("For Shipment job Refresh should be enabled", true, plugin2.InvoicingPostWrapper_ForTestOnly.TransactionFactoryRefreshEnabled_ForTestOnly);
			}
		}

		public void TestTotalsOnGUIRefreshCorrectly()
		{
			JobStorage whsInvoice = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));

			Job warehouseJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			warehouseJob.JH_ParentID = whsInvoice.PK;
			warehouseJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var dummy = new DummyJobInvoicingBusinessObject(Factory);

			Job job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.CreateCharge(warehouseJob, TestObjectCreator.CC1, 10m, 10m);

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.OnGUIShown();

				var totalRevenue = ((JobInvoicingUserControl)invoicingPlugin.UserControl).JobChargeUserControl.TotalAgentAmountCalcEdit;
				AssertEquals("Pre-Condition: TotalRevenue in GUI should be '0'", 0m, ZDecimal.Parse(totalRevenue.Text));

				dummy.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy, (IJobInvoicingPlugIn)whsInvoice });
				TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, 20m, 20m);
				AssertEquals("TotalRevenue should be '20' after new charges added", 20m, ZDecimal.Parse(totalRevenue.Text));

				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals("TotalRevenue should be '30' after Autorating", 30m, ZDecimal.Parse(totalRevenue.Text));
			}
		}

		public void TestParentIsSetOnJobWhenJobIsLoaded()
		{
			IJobInvoicingPlugIn billOfLading = (IJobInvoicingPlugIn)Factory.New<Agency.IBillOfLading>();
			Job job = Factory.NewJobForTesting<Job>();
			job.FillWithValidTestData();
			job.JH_ParentID = billOfLading.PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			IJobInvoicingPlugIn billOfLadingInNewFactory = (IJobInvoicingPlugIn)newFactory.Load<Agency.IBillOfLading>(billOfLading.PK);
			Job jobInNewFactory = newFactory.Load<Job>(job.PK);

			using (ZForm form = new ZForm(billOfLadingInNewFactory))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage firstVisibleTabPage = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();

				form.Menu.MenuItems.Find(ZFormMenuStrategy.ValidateMenuItemName, true)[0].PerformClick();
				AssertNoErrors(jobInNewFactory.Charges[0].JR_InvoiceTypeInfo);
			}
		}

		public void TestPlugInUsesJobInvoicingTabPagePlugIn()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				AssertEquals(typeof(JobInvoicingTabPagePlugIn), plugin.TabPage.GetType());
			}
		}

		public void TestInvoicingPluginToFreightConstructorSetsBusinessContext()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			Assert("Shipment Factory should not have context", !shipment.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			using (new InvoicingPluginToFreight(shipment))
			{
				Assert("Shipment Factory should have InvoicingPlugInGUI context", shipment.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			}
			Assert("Shipment Factory should still have InvoicingPlugInGUI context", shipment.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
		}

		public void TestChargesHaveHeaderCalculationWarningWhenGUIShown()
		{
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Job job = new Job.Loader(Shipment).TryCreateWithMutex();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			job.JH_Status = JobHeaderStatus.Working.Code;

			Charge charge = job.Charges.AddNew();
			charge.JR_OSSellAmt = 100;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

			using (var plugin = new InvoicingPluginToFreight(Shipment))
			{
				plugin.OnGUIShown();
				var warning = "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules.";
				AssertHasWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);
			}
		}

		public void TestStopUsersFromPosting()
		{
			//declaration and shipment countries have to match via current company code, and shipment has to be IMP (import).
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				GlbDepartment.CurrentDepartment.GE_Import = true;
				Factory.Save();

				TestObjectCreator.CreateTestPeriods(new DateTime(DateTime.Now.Year, 1, 1));
				Shipment = TestObjectCreator.CreateShipment("S00010001", "NZAKL", "USA22", null);
				Shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				Shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;

				Job job = new Job.Loader(Shipment).TryCreateWithMutex();
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
				job.JH_Status = JobHeaderStatus.Working.Code;

				Charge charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.MRG100.PK;
				charge.JR_LocalSellAmt = 10.0;
				charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				var charge2CodePK = TestObjectCreator.DSBChargeCode.PK;
				Charge charge2 = job.Charges.AddNew();
				charge2.JR_AC = charge2CodePK;
				charge2.JR_LocalSellAmt = 10.0;
				charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				job.RunPreSaveValidation();
				AssertNoErrors("Pre-condition: Job should be error free", job);

				var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.US.IJobDeclaration)));
				declaration[JobDeclarationSchema.Constants.JE_JS] = Shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = JobMessageTypeList.Codes.Import;
				declaration[JobDeclarationSchema.Constants.JE_GB] = GlbBranch.CurrentBranch.PK;

				Factory.Save();

				TestStopUsersFromPostingChargesWithFutureDates(charge2);

				var options = new AccountingIntegrationOptions();
				options.EnableAccountingIntegration = true;
				CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
				RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, charge2CodePK.ToGuid());

				Factory.Save();

				TestStopUsersFromPostingCostCore(JobInvoicingPostingOption.All, charge2);

				TestStopUsersFromPostingCostCore(JobInvoicingPostingOption.Costs, charge2);

				TestStopUsersFromPostingCostCore(JobInvoicingPostingOption.CustomsDSBChargeAPOnly, charge2);
			}
		}

		public void TestStopUsersFromPostingChargesWithFutureDates(Charge charge)
		{
			var invoicingSupporter = Shipment.InvoicingSupporter;
			var reasonNotToPostCosting = invoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(charge.ChargeCode.PK);

			AssertNull("Pre-condition: should have no reason not to post costing", reasonNotToPostCosting);

			using (var plugin = new InvoicingPluginToFreight(Shipment))
			{
				charge.JR_APInvoiceDate = ZDateTime.Today.AddMonths(2);
				charge.JR_APInvoiceNum = "INV123456";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);

				AssertEquals("Charge set for future date should not be posted for cost", false, charge.IsCostPosted);
			}
		}

		void TestStopUsersFromPostingCostCore(JobInvoicingPostingOption postingOption, Charge charge)
		{
			var invoicingSupporter = Shipment.InvoicingSupporter;
			var reasonNotToPostCosting = invoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(charge.ChargeCode.PK);

			AssertEquals("Pre-condition: should have a reason not to post costing", false, string.IsNullOrEmpty(reasonNotToPostCosting));

			using (var plugin = new InvoicingPluginToFreight(Shipment))
			{
				charge.JR_APInvoiceDate = ZDateTime.Empty;
				charge.JR_APInvoiceNum = "";
				AssertEquals(false, charge.HasValidDataForCostPosting);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.PostTransactions_ForTestOnly(postingOption);

				AssertNotContains("Charge2 does not have valid data and therefore it won't be eligible for cost-posting. So should not stop users", reasonNotToPostCosting, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Charge is not posted for cost due to invalid data", false, charge.IsCostPosted);

				charge.JR_APInvoiceDate = ZDateTime.Now;
				charge.JR_APInvoiceNum = "INV123456";
				AssertEquals(true, charge.HasValidDataForCostPosting);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.PostTransactions_ForTestOnly(postingOption);

				AssertContains("System should have stopped cost-posting", reasonNotToPostCosting, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Charge is not posted for cost", false, charge.IsCostPosted);
			}
		}

		[TestDate(2011, 08, 20)]
		public void TestStopUsersFromPostingRevenueWhenProfitLossReasonRequired()
		{
			TestObjectCreator.CreateTestPeriods(new DateTime(2011, 1, 1));
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			Shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;

			Job job = new Job.Loader(Shipment).TryCreateWithMutex();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			job.JH_Status = JobHeaderStatus.Working.Code;

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid.Empty, null);

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(job.JH_GC.ToGuid(), Guid.Empty, Guid.Empty, plReasonCodes);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 3m;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(job.JH_GC.ToGuid(), Guid.Empty, job.JH_GE.ToGuid(), plRequiringReasonParameters);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.MRG100.PK;
			charge.JR_LocalCostAmt = 5m;
			charge.JR_LocalSellAmt = 10m;
			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: Job should also be error free", job);
			Factory.Save();

			Assert("SHould require Profit/Loss Reason for Invoiced job status", ((JobValidation)job.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(JobHeaderStatus.JobInvoiced.Code));

			DummyJobInvoicingBusinessObject testBusinessObject = new DummyJobInvoicingBusinessObject(Factory, Shipment.PK, "JobShipment");

			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				JobInvoicingPostingOption[] revenuePostingOptions = { JobInvoicingPostingOption.All, JobInvoicingPostingOption.Revenue, JobInvoicingPostingOption.LocalClient, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Disbursement };
				string expectedMessage = @"This job cannot be posted for revenue because it requires a valid Profit/Loss Reason when Job Status will be automatically set to Invoiced after posting the first AR Invoice. 

Please enter a Profit/Loss Reason, save and retry posting.";
				foreach (JobInvoicingPostingOption postingOption in revenuePostingOptions)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugin.PostTransactions_ForTestOnly(postingOption);

					Assert(string.Format("User shoukld be stopped from posting revenue with '{0}' posting option", postingOption), UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));
					AssertEquals("Charge is not posted for revenue", false, job.Charges[0].IsRevenuePosted);
				}

				job.JH_ProfitLossReasonCode = "TST";
				job.RunPreSaveValidation();
				AssertNoErrors("Precondition: Job should also be error free", job);
				Factory.Save();
				// Have to do this for a nonpersistant BizO
				testBusinessObject.SetIsInDatabaseOverride(true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				Assert("Invoice should be posted", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Do you want to print invoice"));
				AssertEquals("Charge is posted for revenue", true, job.Charges[0].IsRevenuePosted);
			}
		}

		public void TestErrorsNotSavedOnJobsRelatedObjectWhenPostingAndConvertingAPtoAR()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Assert(shipment.HasChanges);
			Job newJob = new Job.Loader(shipment).TryCreateWithMutex();
			newJob.PlugInData = shipment;
			newJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			newJob.JH_Status = JobHeaderStatus.Working.Code;
			Charge testCharge = newJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.MRG100.PK;
			testCharge.JR_LocalSellAmt = 10.0;
			testCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			newJob.RunPreSaveValidation();
			AssertNoErrors("Precondition: Job should also be error free", newJob);

			TestObjectCreator.CreateTestPeriods(new DateTime(DateTime.Now.Year, 1, 1));

			Factory.Save();

			AssertEquals("Precondition: Expect factories to be the same", Factory, shipment.Factory);
			Assert(!shipment.HasChanges);
			Assert(!newJob.HasChanges);

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.Job_ForTestOnly = newJob;
				AssertEquals("Precondition: Expect top level business object to be shipment that form was created with ", newJob.Parent.PK, shipment.PK);
				((CommonShipment)newJob.Parent).JS_TransportMode = "BGG";
				((BusinessObject)newJob.Parent).RunPreSaveValidation();
				AssertHasErrors(((CommonShipment)newJob.Parent).JS_TransportModeInfo);
				Assert("Precondition: Shipment has changes to save", ((BusinessObject)newJob.Parent).HasChanges);
				invoicingPlugin.MenuItemPostAll_Click_ForTestOnly(form, EventArgs.Empty);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.ToString(), "Error Please save this form before posting costs and/or charges.\n");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoicingPlugin.MenuItemImportAPInvoices_Click_ForTestOnly(form, EventArgs.Empty);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.ToString(), "Error Please save this form before posting costs and/or charges.\n");
			}
		}

		public void TestMenuItemImportAPInvoices_Click_UnapprovedTransactionAuthorisationFormMarkOwnerForm()
		{
			var differentCompany = TestObjectCreator.CreateNewCompany("ABC");
			var differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			var differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			var originalBranch = GlbBranch.CurrentBranch;
			var originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			Factory.Save();

			var jobShipment = TestObjectCreator.CreateShipment("0097");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			var job1 = new Job.Loader(jobShipment).TryCreateWithMutex();
			job1.PlugInData = jobShipment;
			job1.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			job1.JH_Status = JobHeaderStatus.Working.Code;

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var aRInvoiceSource1 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, -10);
				var aRLine = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource1.AH_JH = job1.PK;
				aRInvoiceSource1.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				aRLine.AL_GB = aRInvoiceSource1.AH_GB;
				Factory.Save();

				var aRInvoiceSource2 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource2.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource2.AH_JH = job1.PK;
				aRInvoiceSource2.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef + "/A";
				aRLine.AL_GB = aRInvoiceSource2.AH_GB;
				Factory.Save();

				var aRInvoiceSource3 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource3.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource3.AH_JH = job1.PK;
				aRInvoiceSource3.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef + "1";
				aRLine.AL_GB = aRInvoiceSource3.AH_GB;
				Factory.Save();

				var aRInvoiceSource4 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR4", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource4.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource4.AH_JH = ZGuid.Empty;
				aRInvoiceSource4.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				aRLine.AL_GB = aRInvoiceSource4.AH_GB;
				Factory.Save();
			}

			using (var form = new ZForm(jobShipment))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.Job_ForTestOnly = job1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoicingPlugin.MenuItemImportAPInvoices_Click_ForTestOnly(form, EventArgs.Empty);

				AssertEquals("ZForm", (ZFormModaliser.LastFormShownDialogForTest as UnapprovedTransactionAuthorisationForm).OwnerName_ForTest);
			}
		}

		public void TestJobRefNum()
		{
			GlbCompany differentCompany = TestObjectCreator.CreateNewCompany("ABC");
			GlbBranch differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			OrgHeader differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			OrgHeader differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			GlbBranch originalBranch = GlbBranch.CurrentBranch;
			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			Factory.Save();

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0097");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			Job job1 = new Job.Loader(jobShipment).TryCreateWithMutex();
			job1.PlugInData = jobShipment;
			job1.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			job1.JH_Status = JobHeaderStatus.Working.Code;

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				ARInvoice aRInvoiceSource1 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, -10);
				ARInvoiceLine aRLine = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource1.AH_JH = job1.PK;
				aRInvoiceSource1.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				aRLine.AL_GB = aRInvoiceSource1.AH_GB;
				Factory.Save();

				ARInvoice aRInvoiceSource2 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource2.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource2.AH_JH = job1.PK;
				aRInvoiceSource2.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef + "/A";
				aRLine.AL_GB = aRInvoiceSource2.AH_GB;
				Factory.Save();

				ARInvoice aRInvoiceSource3 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource3.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource3.AH_JH = job1.PK;
				aRInvoiceSource3.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef + "1";
				aRLine.AL_GB = aRInvoiceSource3.AH_GB;
				Factory.Save();

				ARInvoice aRInvoiceSource4 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR4", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRInvoiceSource4.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRInvoiceSource4.AH_JH = ZGuid.Empty;
				aRInvoiceSource4.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				aRLine.AL_GB = aRInvoiceSource4.AH_GB;
				Factory.Save();
			}

			using (ZForm form = new ZForm(jobShipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.Job_ForTestOnly = job1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoicingPlugin.MenuItemImportAPInvoices_Click_ForTestOnly(form, EventArgs.Empty);

				UnapprovedTransactionConverter converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("Only 2 Candidates (AR Invoices) exist", 2, converter.Candidates.Count);
			}
		}

		public void TestRelatedJobFilterUpdatesMenuItems()
		{
			var consol = TestObjectCreator.CreateGatewayConsolsAndShipments().gC0002;
			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			Assert("Pre-condition: consol is not Gateway when plugin is created", !consol.IsGateway());
			Factory.Save();

			using (var plugIn = new InvoicingPluginToFreight(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Assert("Pre-condition: consol is now gateway, consol does not have to be gateway at plugin creation to subscribe to user control gateway event", consol.IsGateway());
				using (var job = TestObjectCreator.CreateJob(consol))
				{
					var chargeControl = (plugIn.UserControl as JobInvoicingUserControl).JobChargeUserControl;
					chargeControl.Bind(job);
					bool? eventValue = null;
					chargeControl.RelatedJobFilterUpdated += (object sender, RelatedJobFilterUpdatedEventArgs args) => eventValue = args.AnyFiltersApplied;

					foreach (MenuItem item in plugIn.TopLevelMenu.MenuItems)
					{
						Assert("Begins with all items enabled", item.Enabled);
					}

					chargeControl.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
					var details = chargeControl.ShipmentAndBillingDetails;
					AssertNotNull("Precondition: details is set when link is pressed", details?.DetailsRows);
					AssertNotEquals("Precondition: details contains values", 0, details.DetailsRows.Count);
					AssertNull("Precondition: eventValue not yet set", eventValue);

					details.DetailsRows[0].ShowRelatedCharges = true;
					AssertEquals("event triggered with true", true, eventValue);
					foreach (MenuItem item in plugIn.TopLevelMenu.MenuItems)
					{
						Assert("When event called with true, all items disabled", !item.Enabled);
					}

					details.DetailsRows[0].ShowRelatedCharges = false;
					AssertEquals("event triggered with false", false, eventValue);
					foreach (MenuItem item in plugIn.TopLevelMenu.MenuItems)
					{
						Assert("When event called with false, all items enabled", item.Enabled);
					}
				}
			}
		}

		public void TestRefreshGatewayElements()
		{
			var consol = TestObjectCreator.CreateConsol();
			using (var plugIn = new InvoicingPluginToFreight(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				plugIn.GetNewTopLevelMenu_ForTestOnly();
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				var chargeControl = (plugIn.UserControl as JobInvoicingUserControl).JobChargeUserControl;
				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(false);
				Assert(!plugIn.Enabled);
				Assert(!chargeControl.ShipmentAndBillingDetailsLinkLabel.Visible);
				AssertEquals("Billing", plugIn.TabPage.Text);
				AssertEquals("&Job Invoicing", plugIn.TopLevelMenu.Text);
				Assert(plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>().First(x => x.Text == Constants.MenuNameConstants.PostLocalClientCharges).Visible);

				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(true);
				Assert(plugIn.Enabled);
				Assert(chargeControl.ShipmentAndBillingDetailsLinkLabel.Visible);
				AssertEquals("Gateway Billing", plugIn.TabPage.Text);
				AssertEquals("Gateway Invoicing", plugIn.TopLevelMenu.Text);
				Assert(!plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>().First(x => x.Text == Constants.MenuNameConstants.PostLocalClientCharges).Visible);
			}
		}

		public void TestMenuItemImportAPInvoicesFromForwardingConsolToGatewayConsol()
		{
			var forwardingCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			var forwardingCompany = TestObjectCreator.CreateNewCompany("ABC", orgProxy: forwardingCompanyOrgProxy);
			var forwardingBranch = TestObjectCreator.CreateNewBranch(forwardingCompany, "AB1");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(sendingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = gatewayConsol.Shipments.AddNew();
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
			Factory.Save();

			ZGuid expectedInvoicePK = ZGuid.Empty;
			var gatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, forwardingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipmentJob = TestObjectCreator.CreateJob(shipment);
				gatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, gatewayBranchOrgProxy);
				arInvoice.AH_OSExTaxAmount = 150m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				expectedInvoicePK = arInvoice.PK;
				Assert("Posted from Forwarding consol.", arInvoice.AH_JH.IsEmpty);
				var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, shipmentJob, TestObjectCreator.CC1, 150m, TestObjectCreator.AUD, 1.0m);
				TestObjectCreator.CreateCharge(arInvoiceLine);
				Factory.Save();
			}

			using (var plugin = new InvoicingPluginToFreight(gatewayConsol))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				Assert("Should contain AR Invoice", converter.Candidates.Contains(expectedInvoicePK));
			}
		}

		public void TestMenuItemImportAPInvoicesFromGatewayConsolToGatewayConsol()
		{
			var sendingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false);
			var sendingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: sendingGatewayCompanyOrgProxy);
			var sendingGatewayBranch = TestObjectCreator.CreateNewBranch(sendingGatewayCompany, "BGW");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(sendingGatewayCompany: sendingGatewayCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			gatewayConsol.Shipments.AddNew();
			var receivingCompanyGatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
			Factory.Save();

			ZGuid expectedInvoicePK = ZGuid.Empty;
			var receivingGatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sendingGatewayBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				receivingGatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				var sendingCompanyGatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, receivingGatewayBranchOrgProxy);
				arInvoice.AH_OSExTaxAmount = 150m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = sendingCompanyGatewayJob.PK;
				expectedInvoicePK = arInvoice.PK;
				var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, sendingCompanyGatewayJob, TestObjectCreator.CC1, 150m, TestObjectCreator.AUD, 1.0m);
				TestObjectCreator.CreateCharge(arInvoiceLine);
				Factory.Save();
			}

			using (var plugin = new InvoicingPluginToFreight(gatewayConsol))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				Assert("Should contain AR Invoice", converter.Candidates.Contains(expectedInvoicePK));
			}
		}

		T CreateInvoice<T>(GlbBranch branch, OrgHeader org, ZBool isPostedInternal, ZString invoiceNum, ZGuid transactionGroup, ZString transactionReference, ZDecimal amount) where T : InvoicingBase
		{
			T result = Factory.New<T>();
			result.AH_GB = branch.PK;
			result.AH_Ledger = typeof(T).Name.Substring(0, 2);
			result.AH_PostedInternal = isPostedInternal;
			result.AH_OH = org.PK;
			result.AH_TransactionBelongsToGroup = transactionGroup;
			result.AH_TransactionReference = transactionReference;
			result.AH_PostDate = ZDateTime.Now;

			InvoicingLineBase line = (InvoicingLineBase)result.Lines.AddNew();
			line.AL_LineAmount = line.AL_OSAmount = amount;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			result.IsManuallySetTransactionNumber_ForTestOnly = true;
			result.AH_TransactionNum = invoiceNum;
			Factory.Save();

			return result;
		}

		#region OperationWhenJobIsNotCreated

		public void TestImportAPInvoicesWhenJobIsNotCreated_JobWithErrors()
		{
			AssertOperationWhenJobIsNotCreated_JobWithErrors((plugin) => plugin.MenuItemImportAPInvoices_Click_ForTestOnly);
		}

		public void TestCreationJobRevenueJournalsWhenJobIsNotCreated_JobWithErrors()
		{
			AssertOperationWhenJobIsNotCreated_JobWithErrors((plugin) => plugin.MenuItemCreateJobRevenueJournal_Click_ForTestOnly);
		}

		void AssertOperationWhenJobIsNotCreated_JobWithErrors(Func<InvoicingPluginToFreight, EventHandler> getOperationMenuClick)
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				ZTabPage firstTab = new ZTabPage();
				tabControl.TabPages.Add(firstTab);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				AssertEquals("Precondition: TabPages.Count", 2, tabControl.TabPages.Count);
				AssertEquals("Precondition: TabPages[0]", firstTab, tabControl.TabPages[0]);

				Job newJob = new Job.Loader(shipment).Load();
				AssertNull("Precondition: job must not be created.", newJob);

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];

				tabControl.SelectedTab = firstTab;
				AssertEquals("Precondition: the first tab must be selected.", tabControl.SelectedIndex, 0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				invoicingPlugin.OnMenuShown();
				newJob = invoicingPlugin.Job_ForTestOnly;
				AssertEquals("New job must not have changes.", false, newJob.HasChanges);
				AssertEquals("New job must not be saved yet.", false, newJob.IsInDatabase);
				newJob.AddSomeErrorNotValidatedInSetParentCore_ForTestOnly = true;
				getOperationMenuClick(invoicingPlugin)(form, EventArgs.Empty);

				AssertEquals("New job must not have changes.", false, newJob.HasChanges);
				AssertEquals("New job must not be saved yet.", false, newJob.IsInDatabase);
				newJob.RunPreSaveValidation();
				AssertEquals("Precondition: new job must have errors.", true, newJob.HasErrors);
				AssertEquals("Invoicing job must be created for this operation. Do you want to create invoicing job?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Billing tab must not be selected.", tabControl.SelectedIndex, 0);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				tabControl.SelectedTab = firstTab;
				AssertEquals("Precondition: the first tab must be selected.", tabControl.SelectedIndex, 0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoicingPlugin.OnMenuShown();
				AssertEquals("New job must not have changes.", false, newJob.HasChanges);
				AssertEquals("New job must not be saved yet.", false, newJob.IsInDatabase);
				getOperationMenuClick(invoicingPlugin)(form, EventArgs.Empty);

				newJob.RunPreSaveValidation();
				AssertEquals("Precondition: new job must have errors.", true, newJob.HasErrors);
				AssertEquals("New job must not be saved yet.", false, newJob.IsInDatabase);
				AssertEquals("Invoicing job must be created for this operation. Do you want to create invoicing job?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("An error occurred while trying to create the invoicing job header. Please correct these errors on the 'Billing' tab and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Billing tab must be selected.", tabControl.SelectedIndex, 1);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestImportAPInvoicesWhenJobIsNotCreated_JobWithoutErrors()
		{
			AssertOperationWhenJobIsNotCreated_JobWithoutErrors((plugin) => plugin.MenuItemImportAPInvoices_Click_ForTestOnly, typeof(UnapprovedTransactionAuthorisationForm));
		}

		public void TestCreationJobRevenueJournalsWhenJobIsNotCreated_JobWithoutErrors()
		{
			AssertOperationWhenJobIsNotCreated_JobWithoutErrors((plugin) => plugin.MenuItemCreateJobRevenueJournal_Click_ForTestOnly, typeof(JobRevenueJournalForm));
		}

		void AssertOperationWhenJobIsNotCreated_JobWithoutErrors(Func<InvoicingPluginToFreight, EventHandler> getOperationMenuClick, Type formTypeToShow)
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Factory.Save();

			Charge dummyObjectToCheckFactoryWasNotSaved = Factory.New<Charge>();

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				ZTabPage firstTab = new ZTabPage();
				tabControl.TabPages.Add(firstTab);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				Job newJob = new Job.Loader(shipment).Load();
				AssertNull("Precondition: job must not be created.", newJob);

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];

				tabControl.SelectedTab = firstTab;
				AssertEquals("Precondition: the first tab must be selected.", tabControl.SelectedIndex, 0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				invoicingPlugin.OnMenuShown();
				getOperationMenuClick(invoicingPlugin)(form, EventArgs.Empty);

				newJob = invoicingPlugin.Job_ForTestOnly;
				newJob.RunPreSaveValidation();
				AssertEquals("Precondition: new job must not have errors.", false, newJob.HasErrors);
				AssertEquals("New job must not have changes.", false, newJob.HasChanges);
				AssertEquals("New job must not be saved yet.", false, newJob.IsInDatabase);
				AssertEquals("Invoicing job must be created for this operation. Do you want to create invoicing job?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Billing tab must not be selected.", tabControl.SelectedIndex, 0);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				tabControl.SelectedTab = firstTab;
				AssertEquals("Precondition: the first tab must be selected.", tabControl.SelectedIndex, 0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoicingPlugin.OnMenuShown();
				getOperationMenuClick(invoicingPlugin)(form, EventArgs.Empty);

				newJob.RunPreSaveValidation();
				AssertNoErrors("Precondition: new job must not have errors.", newJob);
				AssertEquals("New job must not have changes.", false, newJob.HasChanges);
				AssertEquals("New job must be saved.", true, newJob.IsInDatabase);
				AssertEquals("Main factory must not be saved.", false, dummyObjectToCheckFactoryWasNotSaved.IsInDatabase);
				AssertEquals("Invoicing job must be created for this operation. Do you want to create invoicing job?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Billing tab must not be selected.", tabControl.SelectedIndex, 0);
				AssertType(formTypeToShow, ZFormModaliser.LastFormShownDialogForTest);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestCreateJobRevenueJournalSetDefaultDescription()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Factory.Save();

			Charge dummyObjectToCheckFactoryWasNotSaved = Factory.New<Charge>();

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				ZTabPage firstTab = new ZTabPage();
				tabControl.TabPages.Add(firstTab);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				Job newJob = new Job.Loader(shipment).Load();
				AssertNull("Precondition: job must not be created.", newJob);

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoicingPlugin.OnMenuShown();
				invoicingPlugin.MenuItemCreateJobRevenueJournal_Click_ForTestOnly(form, EventArgs.Empty);

				var journalForm = ZFormModaliser.LastFormShownDialogForTest as JobRevenueJournalForm;
				AssertNotNull("JobRevenueJournalForm", journalForm);

				AssertEquals("JobRevenueJournal Transaction Description", "JOB REVENUE JOURNAL", ((JobRevenueJournal)journalForm.BusinessEntity).AH_Desc);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestCreateJobRevenueJournalSetDefaultDescriptionByRegistrySetting()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Factory.Save();

			Charge dummyObjectToCheckFactoryWasNotSaved = Factory.New<Charge>();

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				ZTabPage firstTab = new ZTabPage();
				tabControl.TabPages.Add(firstTab);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				Job newJob = new Job.Loader(shipment).Load();
				AssertNull("Precondition: job must not be created.", newJob);

				var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
				var item = jobInvoiceDescriptionConfig.AddNew();
				item.JobType = "SHP";
				item.Mode = "AIR";
				item.DirectionCode = "ALL";
				item.InvoiceDescription = "Job Revenue Journal for Shipment: <JobNumber>";
				AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoicingPlugin.OnMenuShown();
				invoicingPlugin.MenuItemCreateJobRevenueJournal_Click_ForTestOnly(form, EventArgs.Empty);

				var journalForm = ZFormModaliser.LastFormShownDialogForTest as JobRevenueJournalForm;
				AssertNotNull("JobRevenueJournalForm", journalForm);

				AssertEquals("JobRevenueJournal Transaction Description from Registry settings", "Job Revenue Journal for Shipment: S00001000", ((JobRevenueJournal)journalForm.BusinessEntity).AH_Desc);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		public void TestCreationJobRevenueJournalsSetupFormCorectly()
		{
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSE";
			consignee.OH_IsConsignee = true;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();
				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.OnGUIShown();
				Factory.Save();

				var pluginJob = invoicingPlugin.Job_ForTestOnly;
				AssertNotNull("Precondition: job must be created.", pluginJob);
				Assert("Precondition: job must be saved.", pluginJob.IsInDatabase);

				MenuItem menu = invoicingPlugin.GetNewTopLevelMenu_ForTestOnly().MenuItems.FindByText("Create Job Revenue Journal");
				AssertNotNull(menu);
				menu.PerformClick();
				AssertType(typeof(JobRevenueJournalForm), ZFormModaliser.LastFormShownDialogForTest);
				JobRevenueJournalForm journalForm = (JobRevenueJournalForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("journalForm.DisplayMode", ODisplayMode.New, journalForm.DisplayMode);
				JobRevenueJournalFormBasherTest.AssertIfJobRevenueJournalForIsInSimpleEntryMode(journalForm);
				JobRevenueJournal journal = (JobRevenueJournal)journalForm.BusinessEntity;
				journal.BranchPKTo = journal.BranchPKFrom;
				journal.DepartmentPKTo = journal.DepartmentPKFrom;
				journal.DefaultSharing = 10M;
				journal.AH_PostDate = ZDateTime.Now;
				JobRevenueJournalCharge journalCharge = journal.JournalCharges.AddNew();
				journalCharge.ChargeCode = Env.Registry.FreightChargeCode;
				journalCharge.BillingTabOsAmount = 100M;
				ContinueWithSave saveResult = journalForm.FireSaveButton();
				AssertEquals("Precondition: Save must be successful.", ContinueWithSave.Yes, saveResult);
				AssertEquals("journalForm.DisplayMode after saving", ODisplayMode.NewSaved, journalForm.DisplayMode);
				journalForm.Dispose();
			}
		}

		public void TestMenuItemCreateJobRevenueJournal_NoExceptionOnGetJRJForm()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var jobInvoicingMenu = form.Menu.MenuItems.FindByText("Job Invoicing");
				AssertNotNull("Precondition: Form should have Job Invoicing Menu", jobInvoicingMenu);

				var menuItem = jobInvoicingMenu.MenuItems.FindByText("Create Job Revenue Journal");
				AssertNotNull("Precondition: Create Job Revenue Journal menuitem menu exists in Job Invoicing", menuItem);

				AssertNoExceptionThrown(() => menuItem.PerformClick());

				AssertType<JobRevenueJournalForm>("Postcondition: form type should be JobRevenueJournalForm", ZFormModaliser.LastFormShownDialogForTest);

				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestMenuItemCreateJobRevenueJournal_JournalHasClosedJobReopenerSet()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var jobInvoicingMenu = form.Menu.MenuItems.FindByText("Job Invoicing");
				AssertNotNull("Precondition: Form should have Job Invoicing Menu", jobInvoicingMenu);

				var menuItem = jobInvoicingMenu.MenuItems.FindByText("Create Job Revenue Journal");
				AssertNotNull("Precondition: Create Job Revenue Journal menuitem menu exists in Job Invoicing", menuItem);

				menuItem.PerformClick();

				var journalForm = (JobRevenueJournalForm)ZFormModaliser.LastFormShownDialogForTest;
				var jobRevenueJournal = (JobRevenueJournal)journalForm.BusinessEntity;

				var closedJobReopener = jobRevenueJournal.ClosedJobReopener;

				AssertNotNull("JRJ ClosedJobReopener", closedJobReopener);
				AssertType<JobRevenueJournalReOpenClosedJobDataProvider>("ReOpenClosedJobDataProvider Type", ((ClosedJobReopener)closedJobReopener).ReOpenClosedJobDataProvider);

				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestJobsNotPostedWhenDebtorIsNotValid()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSE";
			consignee.OH_IsConsignee = true;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Assert(shipment.HasChanges);
			Job newJob = new Job.Loader(shipment).TryCreateWithMutex();
			newJob.PlugInData = shipment;
			newJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			newJob.JH_Status = JobHeaderStatus.Working.Code;
			TestObjectCreator.CreateJobChargeRevRecognition(newJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);

			TestObjectCreator.ABIGAS.OH_IsDebtor = false;

			Charge testCharge = newJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.MRG100.PK;
			testCharge.JR_LocalSellAmt = 10.0;
			testCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			newJob.RunPreSaveValidation();
			AssertNoErrors("Precondition: Job should also be error free", newJob);

			TestObjectCreator.CreateTestPeriods(new DateTime(DateTime.Now.Year, 1, 1));

			Factory.Save();

			Assert(!TestObjectCreator.ABIGAS.OH_IsDebtor);

			DummyJobInvoicingBusinessObject testBusinessObject = new DummyJobInvoicingBusinessObject(Factory, shipment.PK, "JobShipment");

			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				plugin.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("PostedState should be not changed", false, testBusinessObject.PostedStateHasChanges);
			}
		}

		public void TestJobsNotPostedWhenInvoiceTypeIsNotValid()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Assert(shipment.HasChanges);
			Job newJob = new Job.Loader(shipment).TryCreateWithMutex();
			newJob.PlugInData = shipment;
			newJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			newJob.JH_Status = JobHeaderStatus.Working.Code;
			newJob.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			newJob.JH_GS_NKRepOps = GlbStaff.CurrentUser.GS_Code;
			TestObjectCreator.CreateJobChargeRevRecognition(newJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			Charge testCharge = newJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.MRG100.PK;
			testCharge.JR_LocalSellAmt = 10.0;
			testCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			newJob.RunPreSaveValidation();
			AssertNoErrors("Precondition: Job should also be error free", newJob);

			TestObjectCreator.CreateTestPeriods(new DateTime(DateTime.Now.Year, 1, 1));

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Charge testChargeInNewFactory = newFactory.Load<Charge>(testCharge.PK);
			testChargeInNewFactory.JR_InvoiceType = "";
			newFactory.Save();

			DummyJobInvoicingBusinessObject testBusinessObject = new DummyJobInvoicingBusinessObject(Factory, shipment.PK, "JobShipment");

			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				plugin.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("PostedState should be not changed", false, testBusinessObject.PostedStateHasChanges);
			}
		}

		public void TestOrgWarningWhenInvoicePostedAndCreditLimitExceeded()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			OrgHeader org = TestObjectCreator.ABIGAS;
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_ARCreditLimit = 1000m;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: shipment should be errors free", shipment);
			Assert(shipment.HasChanges);
			Job newJob = new Job.Loader(shipment).TryCreateWithMutex();
			newJob.PlugInData = shipment;
			newJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			newJob.JH_Status = JobHeaderStatus.Working.Code;
			newJob.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			newJob.JH_GS_NKRepOps = GlbStaff.CurrentUser.GS_Code;
			newJob.LocalChargesPK = org.PK;
			TestObjectCreator.CreateJobChargeRevRecognition(newJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			Charge testCharge = newJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.MRG100.PK;
			testCharge.JR_LocalSellAmt = 1500m;
			testCharge.JR_OH_SellAccount = org.PK;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			newJob.RunPreSaveValidation();
			AssertNoErrors("Precondition: Job should also be error free", newJob);

			TestObjectCreator.CreateTestPeriods(new DateTime(DateTime.Now.Year, 1, 1));

			Factory.Save();

			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				AssertNoWarning(plugInToFreight.Job_ForTestOnly.JH_OA_LocalChargesAddrInfo, @"The Credit Limit for ABIGAS is set to 1,000.00 AUD. Credit approved.
The Total Outstanding Balance is 1,650.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 1,650.00 AUD");
				plugInToFreight.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());
				AssertHasWarnings(plugInToFreight.Job_ForTestOnly.JH_OA_LocalChargesAddrInfo);
				AssertHasWarning(plugInToFreight.Job_ForTestOnly.JH_OA_LocalChargesAddrInfo, @"The Credit Limit for ABIGAS is set to 1,000.00 AUD. Credit approved.
The Total Outstanding Balance is 1,650.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 1,650.00 AUD");
			}
		}

		public void TestOrgControlCaptions()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (ZForm form = new ZForm(consol))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				AssertEquals("Prepaid Agent", invoicingPlugin.JobChargeUserControl_ForTestOnly.JH_OH_LocalChargesBoundOrgCard.Text);
				AssertEquals("Collect Agent", invoicingPlugin.JobChargeUserControl_ForTestOnly.JH_OH_AgentCollectBoundOrgCard.Text);
			}
		}

		public void TestDontSetPluginDataConditions()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;

			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				plugInToFreight.OnGUIShown();
				Assert(!plugInToFreight.Job_ForTestOnly.HasChanges);
				Charge testCharge = plugInToFreight.Job_ForTestOnly.Charges.AddNew();
				testCharge.JR_AC = TestObjectCreator.MRG100.PK;
				plugInToFreight.OnMenuShown();
				Assert("Should still have changes", plugInToFreight.Job_ForTestOnly.HasChanges);
			}
		}

		[ExpectNoExceptions]
		public void TestReCreatingADeletedJobDoesNotThrowExceptions()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			using (ZForm form = new ZForm(shipment))
			{
				plugin.MakeOrActivateJob_ForTestOnly();
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage tabPage2 = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(plugin.TabPage);
				tabControl.TabPages.Add(tabPage2);
				form.Show();
				tabControl.SelectTab(0);

				AssertNotNull("Job_ForTestOnly should not be null", plugin.Job_ForTestOnly);

				plugin.MakeOrActivateJob_ForTestOnly();
				tabControl.SelectTab(1);
				tabControl.SelectTab(0);
				AssertNotNull("Job_ForTestOnly should not be null", plugin.Job_ForTestOnly);
				plugin.Dispose();
			}
		}

		public void TestActivateJobResetValue()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			using (ZForm form = new ZForm(shipment))
			{
				plugin.MakeOrActivateJob_ForTestOnly();

				var job = plugin.Job_ForTestOnly;
				Factory.Save();

				AssertDefaultValuesOfJob(job, shipment.PK, shipment.JS_UniqueConsignRef);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, GlbCompany.GetDemoCompany(Factory).FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;
					job.JH_OA_AgentCollectAddr = TestObjectCreator.ABIGAS.MainAddress.PK;
					job.JH_Description = "testtest";
					job.JH_A_JCL = ZDateTime.Now.AddDays(2);
					job.JH_ProfitLossReasonCode = "TST";
					job.JH_GS_NKRepSales = "1";
					job.JH_GS_NKRepOps = "2";
					job.JH_TH_NKQuoteNumber = "testtest";
					job.JH_GC = GlbCompany.GetDemoCompany(Factory).PK;
					job.JH_GB = GlbCompany.GetDemoCompany(Factory).FirstActiveBranch.PK;
					job.JH_GE = TestObjectCreator.FISDepartment.PK;
					job.JH_AgentChargesCFX = 10M;
					job.JH_ARInvoiceReference = "TEst";
					job.JH_ExcludeFromPeriodicRating = false;
					job.JH_HeaderType = "TES";
					job.JH_HoldReason = "TEST";
					job.JH_IsProfitSharePosted = false;

					job.JH_JobBufferPercentOverride = 1;
					job.JH_JobNum = "TEST";
					job.JH_JobPlannedStartDate = DateTime.Now.AddDays(1);
					job.JH_LocalChargesCFX = 10M;
					job.JH_LocalClientInvoicingStyle = "Tes";
					job.JH_Name = "ss";

					var org1 = TestObjectCreator.CreateOrgHeader("newOrg", false, false);
					var contact1 = TestObjectCreator.CreateContact(org1, "Jo");
					job.JH_OC_LocalBillingContact = contact1.PK;

					job.JH_PaymentCollectionStatus = "TES";
					job.JH_ProfitShareInvoice = ZGuid.NewZGuid();
					job.JH_RatingHasBeenRun = false;
					job.JH_RevenueRecognizedDate = DateTime.Now.AddDays(1);
					job.JH_SingleAgentsInvoicePerConsol = false;
					job.JH_UniqueJobInvoiceNumber = 6;
					Factory.Save();

					job.MarkAsInactive();
					Factory.Save();
				}

				plugin.MakeOrActivateJob_ForTestOnly();
				job = plugin.Job_ForTestOnly;
				Factory.Save();

				AssertDefaultValuesOfJob(job, shipment.PK, shipment.JS_UniqueConsignRef);
			}
		}

		void AssertDefaultValuesOfJob(Job job, ZGuid parentId, ZString jobNum)
		{
			AssertEquals("Job Parent Table Code should be reset to default", "JS", job.JH_ParentTableCode);
			AssertEquals("Job Parent ID Code should be reset to default", parentId, job.JH_ParentID);
			Assert("Local Client Address should be reset to default", job.JH_OA_LocalChargesAddr.IsDefault);
			Assert("Overseas Agent Address should be reset to default", job.JH_OA_LocalChargesAddr.IsDefault);
			AssertEquals("Job Status should be reset to default", "WRK", job.JH_Status);
			Assert("Job Description should be reset to default", job.JH_Description.IsDefault);
			Assert("Job Close Date should be reset to default", job.JH_A_JCL.IsDefault);
			Assert("Job P/L Reason should be reset to default", job.JH_ProfitLossReasonCode.IsDefault);
			Assert("Sale Rep should be reset to default", job.JH_GS_NKRepSales.IsDefault);
			AssertEquals("Operation Rap should be reset to default", "E", job.JH_GS_NKRepOps);
			Assert("Exclude From Auto Rating should be reset to default", !job.JH_ExcludeFromPeriodicRating);
			AssertEquals("Charges should be reset to default", 0, job.Charges.Count);
			AssertEquals("Exchage Rates should be reset to default", 0, job.ExchangeRates.Count);
			Assert("Quota Number should be reset to default", job.JH_TH_NKQuoteNumber.IsDefault);
			AssertEquals("Company should be reset to default", GlbCompany.CurrentCompany.PK, job.JH_GC);
			AssertEquals("Branch should be reset to default", GlbBranch.CurrentBranch.PK, job.JH_GB);
			AssertEquals("Department should be reset to default", GlbDepartment.CurrentDepartment.PK, job.JH_GE);
			Assert("JH_AgentChargesCFX should be reset to default", job.JH_AgentChargesCFX.IsDefault);
			Assert("JH_ARInvoiceReference should be reset to default", job.JH_ARInvoiceReference.IsDefault);
			Assert("JH_ExcludeFromPeriodicRating should be reset to default", job.JH_ExcludeFromPeriodicRating.IsDefault);
			AssertEquals("JH_HeaderType should be reset to default", "JOB", job.JH_HeaderType);
			Assert("JH_HoldReason should be reset to default", job.JH_HoldReason.IsDefault);
			Assert("JH_IsProfitSharePosted should be reset to default", job.JH_IsProfitSharePosted.IsDefault);
			Assert("JH_JH_ParentJob should be reset to default", job.JH_JH_ParentJob.IsDefault);
			Assert("JH_JobBufferPercentOverride should be reset to default", job.JH_JobBufferPercentOverride.IsDefault);
			AssertEquals("JH_JobNum should be reset to default", jobNum, job.JH_JobNum);
			Assert("JH_JobPlannedStartDate should be reset to default", job.JH_JobPlannedStartDate.IsDefault);
			Assert("JH_LocalChargesCFX should be reset to default", job.JH_LocalChargesCFX.IsDefault);
			Assert("JH_LocalClientInvoicingStyle should be reset to default", job.JH_LocalClientInvoicingStyle.IsDefault);
			Assert("JH_Name should be reset to default", job.JH_Name.IsDefault);
			Assert("JH_OC_LocalBillingContact should be reset to default", job.JH_OC_LocalBillingContact.IsDefault);
			Assert("JH_PaymentCollectionStatus should be reset to default", job.JH_PaymentCollectionStatus.IsDefault);
			Assert("JH_ProfitShareInvoice should be reset to default", job.JH_ProfitShareInvoice.IsDefault);
			Assert("JH_RatingHasBeenRun should be reset to default", job.JH_RatingHasBeenRun.IsDefault);
			Assert("JH_RevenueRecognizedDate should be reset to default", job.JH_RevenueRecognizedDate.IsDefault);
			Assert("JH_SingleAgentsInvoicePerConsol should be reset to default", job.JH_SingleAgentsInvoicePerConsol);
			Assert("JH_UniqueJobInvoiceNumber should be reset to default", job.JH_UniqueJobInvoiceNumber.IsDefault);
		}

		public void TestDeactivateJobByDataRefresh_UserWithoutSecurityRightsToViewBillingTab()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var jobBranch = GlbBranch.CurrentBranch;
			var jobDepartment = GlbDepartment.CurrentDepartment;

			Shipment = TestObjectCreator.CreateShipment("S00001234");
			Job = TestObjectCreator.CreateJob(Shipment);
			Shipment.Job.JH_GB = jobBranch.PK;
			Shipment.Job.JH_GE = jobDepartment.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			using (ZForm form = new ZForm(Shipment))
			{
				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, jobBranch.PK.ToGuid(), jobDepartment.PK.ToGuid());
				security.Login.IsAllowed = false;
				var loginSecurity = Factory.New<GlbSecurity>();
				loginSecurity.GU_GB = jobBranch.PK;
				loginSecurity.GU_GE = jobDepartment.PK;
				loginSecurity.GU_GC = jobBranch.Company.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				Factory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var tabControl = new ZTemplateTabControl();
				var otherTabPage = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(otherTabPage);
				form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.JobInvoicing, 0);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowViewEditBilling).IsAllowed = false;

				form.Show();
				AssertNotNull("Plugin Job_ForTestOnly should not be null", plugin.Job_ForTestOnly);
				AssertNotNullOrEmpty(plugin.PlugInNotDisplayedMessage);
				Assert(((IPlugInInternals)plugin).CoveringLabel.Visible);

				tabControl.SelectedTab = otherTabPage;
				var factory2 = new BusinessObjectFactory();
				var jobReloaded = factory2.Load<Job>(plugin.Job_ForTestOnly.PK);
				jobReloaded.MarkAsInactive();
				factory2.Save();

				Assert("Job should be marked as inactive", Job.IsCancelled);
				plugin.SelectTabPage();
				AssertNullOrEmpty("No exceptions should be reported", ErrorReporter.LastMessageReported);
			}
		}

		public void TestDeactivatingJobHeaderParentTriggersCoveringLabel_DeactivateJob()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.MakeOrActivateJob_ForTestOnly();
				plugin.OnGUIShown();
				plugin.GetNewTopLevelMenu_ForTestOnly();
				plugin.OnSaving();
				Factory.Save();

				shipment.IsCancelled = true;
				// Have to call it explicitely. Should be called by a ZFormMenuStrategy
				plugin.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);

				AssertNull("Job_ForTestOnly should be null after deletion", plugin.Job_ForTestOnly);
				AssertNull("Shipment Job is null because job is inactive.", shipment.Job);
				Assert("CoveringLabel should be visible", ((IPlugInInternals)plugin).CoveringLabel.Visible);
				AssertEquals("You have set this Shipment S00001234 to inactive.\r\nThis will mark the invoicing Job S00001234 as inactive when you save this Shipment.\r\n\r\n", ((IPlugInInternals)plugin).CoveringLabel.Text);
			}
		}

		public void TestDeactivateJobSaveAndClosedFormForSaveSuccessed()
		{
			AssertDeactivateJobSaveAndClosedForm(true);
		}

		public void TestDeactivateJobSaveAndClosedFormForHavingValidationError()
		{
			AssertDeactivateJobSaveAndClosedForm(false);
		}

		void AssertDeactivateJobSaveAndClosedForm(bool shouldSave)
		{
			var shipment = Factory.New<ForwardingShipment>();
			if (shouldSave)
			{
				var consignor = TestObjectCreator.LocalClient;
				consignor.OH_IsConsignor = true;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.LocalClient.PK;
				shipment.JS_TransportMode = "AIR";
			}

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.MakeOrActivateJob_ForTestOnly();
				var job = plugin.Job_ForTestOnly;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				Factory.Save();

				var hasBeenSaved = false;
				var hasValidatingForSave = false;
				var hasClosed = false;
				var tabControl = new ZTemplateTabControl();
				var firstVisibleTabPage = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Saved += delegate
				{ hasBeenSaved = true; };

				form.ValidatingForSave += delegate
				{ hasValidatingForSave = true; };

				form.Closed += delegate
				{ hasClosed = true; };

				form.Show();

				Assert(!hasBeenSaved);
				Assert(!hasValidatingForSave);
				Assert(!hasClosed);

				plugin.DeactivateJobSaveAndClosedForm_ForTestOnly();

				AssertEquals("The form should be saved.", shouldSave, hasBeenSaved);
				AssertEquals("The form should has validating for save.", shouldSave, hasValidatingForSave);
				Assert("The form should has closed.", hasClosed);

				plugin.Dispose();
			}
		}

		[ExpectNoExceptions()]
		public void TestDeactivationWhenTheJobIsNullWillNotCauseException()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				AssertNull("Precondition: Job_ForTestOnly should be null", plugin.Job_ForTestOnly);
				plugin.OnBusinessObjectIsCancelledChanged(ZBool.True);
			}
		}

		public void TestOnGUIShownWillActivateJobIfJobInactive()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					Factory.Save();

					var currentJob = plugInToFreight.Job_ForTestOnly;
					currentJob.MarkAsInactive();
					Assert("IsActive should return false", !currentJob.JH_IsActive);
					plugInToFreight.OnGUIShown();
					Assert("Job_ForTestOnly should be active", plugInToFreight.Job_ForTestOnly.JH_IsActive);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestMakeJobIfNullWillActivateJobIfJobInactive()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					Factory.Save();

					var currentJob = plugInToFreight.Job_ForTestOnly;
					currentJob.MarkAsInactive();
					Assert("IsActive should return false", !currentJob.JH_IsActive);
					plugInToFreight.MakeOrActivateJob_ForTestOnly();
					Assert("Job_ForTestOnly should be active", plugInToFreight.Job_ForTestOnly.JH_IsActive);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestHookingToAnObjectWithEmptyFactoryWillNotCauseException()
		{
			DummyJobInvoicingBusinessObject testObject = new DummyJobInvoicingBusinessObject();
			AssertNull("Precondition: Factory should be null", testObject.Factory);
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(testObject))
			{ }
		}

		[ExpectNoExceptions()]
		public void TestPassingNullAsHostBusinessObjectWillNotCauseException()
		{
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(null))
			{ }
		}

		public void TestJobInvoicingSecurityOverrideProviderResetOnSave()
		{
			bool oldAllowed = Env.Security.ReopenJob.IsAllowed;

			InvoicingPluginToFreightWrapper pluginToFreight = new InvoicingPluginToFreightWrapper();
			Job testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Closed.Code);

			try
			{
				pluginToFreight.Job_ForTestOnly = testJob;
				SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();

				Env.Security.ReopenJob.IsAllowed = false;
				ZFormModaliser.LastFormShownDialogForTest = null;

				pluginToFreight.Job_ForTestOnly.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should NOT reopen", JobHeaderStatus.Closed.Code, testJob.JH_Status);

				pluginToFreight.ShowPreSaveDialogsCore();

				pluginToFreight.Job_ForTestOnly.JH_Status = JobHeaderStatus.Closed.Code;
				pluginToFreight.Job_ForTestOnly.Factory.Save();

				pluginToFreight.Job_ForTestOnly.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should NOT reopen", JobHeaderStatus.Closed.Code, testJob.JH_Status);
			}
			finally
			{
				pluginToFreight.Dispose();
				Env.Security.ReopenJob.IsAllowed = oldAllowed;
			}
		}

		public void TestRecommendEnableAutoJRJInfoControlIsShownDuringPreSave()
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			var sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			var sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");

			sisterCompanyOrgProxy.CompanyData.OB_IsDebtor = true;
			sisterCompanyOrgProxy.CompanyData.SetARTaxApplicable(false);
			sisterCompanyOrgProxy.CompanyData.OB_ARWHTApplicable = false;
			sisterCompanyOrgProxy.CompanyData.OB_GC = sisterCompany.PK;
			Factory.Save();

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_GC = sisterCompany.PK;
			debtor.OH_FullName = "Test Debtor";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S1112", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (Job shipment1Job = TestObjectCreator.CreateJob(shipment1))
			using (Job shipment2Job = TestObjectCreator.CreateJob(shipment2))
			using (Job gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, setCurrentDepartment: false))
			{
				gatewayJob.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
				gatewayJob.PlugInData = gatewayConsol;
				var pluginToGWConsol = new InvoicingPluginToFreight(gatewayConsol);
				try
				{
					UnitTestUserNotification.Instance.ClearMessages();
					var isContinue = pluginToGWConsol.ShowPreSaveDialogsCore();
					AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);
					AssertNotEquals("RecommendEnableAutoJRJInfoControl should not be triggered", "RecommendEnableAutoJRJInfoControl", UnitTestUserNotification.Instance.LastMessage.Text);

					var charge = gatewayJob.Charges.AddNew();
					charge.JR_AC = TestObjectCreator.MRG100.PK;
					charge.JR_OSCostAmt = 100m;
					charge.JR_Calc_RelatedJobNumber = ZString.Empty;
					charge.JR_OH_SellAccount = debtor.PK;
					UnitTestUserNotification.Instance.ClearMessages();
					isContinue = pluginToGWConsol.ShowPreSaveDialogsCore();
					AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);
					AssertNotEquals("RecommendEnableAutoJRJInfoControl should not be triggered", "RecommendEnableAutoJRJInfoControl", UnitTestUserNotification.Instance.LastMessage.Text);

					charge.JR_OH_SellAccount = sisterCompany.OrgProxy.PK;
					UnitTestUserNotification.Instance.ClearMessages();
					isContinue = pluginToGWConsol.ShowPreSaveDialogsCore();
					AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);
					AssertEquals("RecommendEnableAutoJRJInfoControl should be displayed", "RecommendEnableAutoJRJInfoControl", UnitTestUserNotification.Instance.LastMessage.Text);

					using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
					{
						UnitTestUserNotification.Instance.ClearMessages();
						isContinue = pluginToGWConsol.ShowPreSaveDialogsCore();
						AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);
						AssertNotEquals("RecommendEnableAutoJRJInfoControl should not be triggered", "RecommendEnableAutoJRJInfoControl", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					pluginToGWConsol.Dispose();
				}
			}
		}

		public void TestInvoicingPluginToFrieght_Success_WhenPreSaveActionsReturnsSuccess()
		{
			var mockInvoicingPluginToFreightPresentationProvider = new Mock<IInvoicingPluginToFreightPresentationProvider>();
			mockInvoicingPluginToFreightPresentationProvider.Setup(x => x.PreSaveActions()).Returns(PreSaveActionsResult.Success());
			var mockAccountingPresentationProviderFactory = new Mock<IAccountingPresentationProviderFactory>();
			mockAccountingPresentationProviderFactory.Setup(x => x.GetInvoicingPluginToFreightPresentationProvider(It.IsAny<IClosedJobReopener>(), It.IsAny<IJobRevRecognitionDataRetriever>())).Returns(mockInvoicingPluginToFreightPresentationProvider.Object);
			ObjectFactory.Substitute(mockAccountingPresentationProviderFactory.Object);

			var shipment = Factory.NewWithValidTestData<DummyShipment>();
			PopulateShipment(shipment);

			var shipmentJob = Factory.NewJobForTesting<Job>();

			using (var pluginToFreight = new InvoicingPluginToFreight(shipment))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				var isContinue = pluginToFreight.ShowPreSaveDialogsCore();

				mockInvoicingPluginToFreightPresentationProvider.Verify(x => x.PreSaveActions(), Times.Never());

				shipmentJob.PlugInData = shipment;
				pluginToFreight.Job_ForTestOnly = shipmentJob;
				AssertNotNull("Precondition: Shipment Job is not null", shipment.Job);

				ZFormModaliser.LastFormShownDialogForTest = null;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();

				mockInvoicingPluginToFreightPresentationProvider.Verify(x => x.PreSaveActions());
				AssertEquals(ContinueWithSave.Yes, isContinue);
			}
		}

		public void TestInvoicingPluginToFrieght_Failure_WhenPreSaveActionsReturnsFailure()
		{
			var failureMessageText = "Saving requires job reopening.";
			var mockInvoicingPluginToFreightPresentationProvider = new Mock<IInvoicingPluginToFreightPresentationProvider>();
			mockInvoicingPluginToFreightPresentationProvider.Setup(x => x.PreSaveActions()).Returns(PreSaveActionsResult.Failure(failureMessageText));
			var mockAccountingPresentationProviderFactory = new Mock<IAccountingPresentationProviderFactory>();
			mockAccountingPresentationProviderFactory.Setup(x => x.GetInvoicingPluginToFreightPresentationProvider(It.IsAny<IClosedJobReopener>(), It.IsAny<IJobRevRecognitionDataRetriever>())).Returns(mockInvoicingPluginToFreightPresentationProvider.Object);
			ObjectFactory.Substitute(mockAccountingPresentationProviderFactory.Object);

			var shipment = Factory.NewWithValidTestData<DummyShipment>();
			PopulateShipment(shipment);

			var shipmentJob = Factory.NewJobForTesting<Job>();

			using (var pluginToFreight = new InvoicingPluginToFreight(shipment))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				var isContinue = pluginToFreight.ShowPreSaveDialogsCore();

				mockInvoicingPluginToFreightPresentationProvider.Verify(x => x.PreSaveActions(), Times.Never());

				shipmentJob.PlugInData = shipment;
				pluginToFreight.Job_ForTestOnly = shipmentJob;
				AssertNotNull("Precondition: Shipment Job is not null", shipment.Job);

				ZFormModaliser.LastFormShownDialogForTest = null;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();

				mockInvoicingPluginToFreightPresentationProvider.Verify(x => x.PreSaveActions());
				AssertEquals(ContinueWithSave.No, isContinue);
				AssertEquals("User do not reopen closed job", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(failureMessageText));
			}
		}

		#region Test JobInvoicing EditSecurity

		public void TestEditSecurityAllowed()
		{
			var shipment = Factory.NewWithValidTestData<DummyShipment>();
			var shipmentJob = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			Factory.Save();
			var pluginToFreight = new InvoicingPluginToFreight(shipment);
			SecurityTestObject.CreateTestUser(true, shipment.InvoicingSupporter.EditSecurityCheckpoint.Code, "TST", "tst", "passwordRight");

			try
			{
				((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest("TST", "passwordRight");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});
				ContinueWithSave isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);
			}
			finally
			{
				pluginToFreight.Dispose();
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public void TestEditSecurityNotAllowed()
		{
			var shipment = Factory.NewWithValidTestData<DummyShipment>();
			PopulateShipment(shipment);

			var pluginToFreight = new InvoicingPluginToFreight(shipment);
			var shipmentJob = Factory.NewJobForTesting<Job>();

			try
			{
				pluginToFreight.Job_ForTestOnly = shipmentJob;
				ContinueWithSave isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);

				shipmentJob.PlugInData = shipment;
				pluginToFreight.Job_ForTestOnly = shipmentJob;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);

				shipmentJob.HasChanges = true;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);

				((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = true;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);
			}
			finally
			{
				pluginToFreight.Dispose();
			}
		}

		void PopulateShipment(DummyShipment shipment)
		{
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = "FOB";
			shipment.JS_UniqueConsignRef = "S00010001";
			shipment.JS_HouseBill = "UVWXYZ";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ActualChargeable = 100M;
		}

		class DummyShipment : ForwardingShipment
		{
			public DummyShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyShipmentInvoicingSupporter(this);
			}

			public class DummyShipmentInvoicingSupporter : ForwardingShipmentInvoicingSupporter
			{
				public DummyShipmentInvoicingSupporter(DummyShipment parent)
					: base(parent)
				{
				}

				protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
				{
					return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays);
				}

				public bool fEditSecurityLockCore;
				protected override bool EditSecurityLockCore
				{
					get
					{
						return fEditSecurityLockCore;
					}
				}
			}
		}

		#endregion

		public void TestMenuItemReverseInvoicePromptLoginForReopenClosedJob()
		{
			bool oldAllowed = Env.Security.ReopenJob.IsAllowed;

			var testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			ForwardingShipment forwardingShipment = testJob.PlugInData as ForwardingShipment;
			AssertNotNull("Forwarding Shipment should not be null", forwardingShipment);
			InvoicingPluginToFreightWrapper pluginToFreight = new InvoicingPluginToFreightWrapper(forwardingShipment);

			testJob = Factory.Load<Job>(testJob.PK);
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			testJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			try
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				Env.Security.ReopenJob.IsAllowed = false;
				pluginToFreight.Job_ForTestOnly = testJob;
				SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
				pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should NOT reopen", JobHeaderStatus.Closed.Code, testJob.JH_Status);

				ZFormModaliser.LastFormShownDialogForTest = null;
				Env.Security.ReopenJob.IsAllowed = true;
				pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals("Should NOT Prompt Login Form", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should reopen", JobHeaderStatus.Working.Code, testJob.JH_Status);

				pluginToFreight.Job_ForTestOnly.JH_Status = JobHeaderStatus.Closed.Code;
				pluginToFreight.Job_ForTestOnly.Factory.Save();

				ZFormModaliser.LastFormShownDialogForTest = null;
				Env.Security.ReopenJob.IsAllowed = false;
				pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Should Prompt Login Form again", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			finally
			{
				pluginToFreight.Dispose();
				Env.Security.ReopenJob.IsAllowed = oldAllowed;
			}
		}

		[ExpectNoExceptions()]
		public void TestInitialise()
		{
			InvoicingParam @params = new InvoicingParam();
			@params.PK = Guid.NewGuid();
			@params.TableName = JobShipmentSchema.Constants.TableName;

			@params.JobNumber = "test";
			using (ZPlugIn fAccountingModule = new InvoicingPluginToFreight(@params))
			{
				using (fAccountingModule.UserControl)
				{
					fAccountingModule.OnUserControlShown();
				}
			}
		}

		public void TestAuditMenuItem()
		{
			DummyJobInvoicingBusinessObject hostBusinessEntity = new DummyJobInvoicingBusinessObject(Factory);

			using (ZForm form = new ZForm(hostBusinessEntity))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				WriteToLogMenuItem menuItem = (WriteToLogMenuItem)plugin.TopLevelMenu.MenuItems.FindByText("Audit Billing");
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				Tester.Test(menuItem, hostBusinessEntity, null, hostBusinessEntity.AuditSecurity, "Audit Billing");
				AssertEquals("A message should be shown.", "The Audit Billing record has not been created yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Job job = Factory.NewJobForTesting<Job>();
				plugin.Job_ForTestOnly = job;

				Tester.Test(menuItem, hostBusinessEntity, job, hostBusinessEntity.AuditSecurity, "Audit Billing");
				AssertEquals("A message should be shown.", "This record needs to be saved. Please save the form first before write to log.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotificationIconsRenderedCorrectlyWhenTabNotShown()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			using (ZForm form = new ZForm(shipment))
			{
				plugin.MakeOrActivateJob_ForTestOnly();
				shipment.RegisterEditableChildObject(plugin.Job_ForTestOnly);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage firstVisibleTabPage = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(firstVisibleTabPage);
				tabControl.TabPages.Add(plugin.TabPage);

				form.Show();
				Application.DoEvents();
				form.Menu.MenuItems.Find(ZFormMenuStrategy.ValidateMenuItemName, true)[0].PerformClick();
				UserIdleWorker.Flush();
				AssertEquals("Error icon set", Icons.GetImageIndex(IconTypes.Error), plugin.TabPage.ImageIndex);
				plugin.Dispose();
			}
		}

		#region TestDummyJobInvoicingBusinessObjectIJobInvoicingPlugin

		public void TestDummyJobInvoicingBusinessObjectEditSecurity()
		{
			IJobInvoicingPlugIn testJob = new DummyJobInvoicingBusinessObject(Factory);
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		#endregion

		#region Invoicing Job Header Creation & Deletion

		public void TestJobHeaderIsNotCreatedAutomaticallyIfRegistryIsSetToFalse()
		{
			SetUpJobHeaderCreationTest(false);

			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment);

			try
			{
				plugInToFreight.OnSaving();
				Factory.Save();
				Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertNull("Job should not be automatically created", job);

				plugInToFreight.OnGUIShown();
				plugInToFreight.OnSaving();
				Factory.Save();

				job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Job should be created", job);
			}
			finally
			{
				plugInToFreight.Dispose();
				TearDownJobHeaderCreationTest();
			}
		}

		public void TestJobHeaderIsNotCreatedAutomaticallyIfRegistryIsSetToFalseAndUsesProperSecuritySettings()
		{
			Env.Security.MaintainShipmentJobInvoicing.IsAllowed = false;
			Env.Security.CustomsDeclarationJobInvoicing.IsAllowed = true;

			SetUpJobHeaderCreationTest(false);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKOrigin = "USCHI";
			InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(declaration);

			try
			{
				plugInToFreight.OnSaving();
				Factory.Save();
				Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertNull("Job should not be automatically created", job);

				plugInToFreight.OnGUIShown();
				plugInToFreight.OnSaving();

				job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Job should be created", job);
			}
			finally
			{
				plugInToFreight.Dispose();
				TearDownJobHeaderCreationTest();
			}
		}

		public void TestJobHeaderIsCreatedAutomatically()
		{
			SetUpJobHeaderCreationTest(true);

			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment);

			try
			{
				plugInToFreight.OnSaving();
				Factory.Save();
				Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				AssertNotNull("Job should be automatically created", job);

				plugInToFreight.OnGUIShown();
				plugInToFreight.OnSaving();
				Factory.Save();

				job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				AssertNotNull("Job should be created", job);
			}
			finally
			{
				plugInToFreight.Dispose();
				TearDownJobHeaderCreationTest();
			}
		}

		public void TestJobHeaderIsCreatedAutomaticallyWhenEnalbeElectronicProcessingCharge()
		{
			SetUpJobHeaderCreationTest(false);

			var coLoadShipment = TestObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", null);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			shipment.JS_JS_ColoadMasterShipment = coLoadShipment.PK;

			shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(false, shipment.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
			AssertEquals(false, AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Value);

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = collection.AddNew();
			config.JobType = "SHP";
			config.StartDate = ZDate.Today.AddDays(-1);
			config.EndDate = ZDate.Today.AddDays(1);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var plugInToFreight = new InvoicingPluginToFreight(shipment);

			try
			{
				plugInToFreight.OnSaving();
				Factory.Save();
				var job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				AssertNotNull("Job should be automatically created", job);

				plugInToFreight.OnGUIShown();
				plugInToFreight.OnSaving();
				Factory.Save();

				job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				AssertNotNull("Job should be created", job);
			}
			finally
			{
				plugInToFreight.Dispose();
				TearDownJobHeaderCreationTest();
			}
		}

		public void TestCannotDeleteMessage_JobInDatabase()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Factory.Save();

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				AssertEquals("Should have error message before marked job as inactive.", @"This record cannot be deleted.
An Invoicing Job Header (S00001001) has been created in the company EDI.", plugin.CannotDeleteMessage);

				job.MarkAsInactive();
				Factory.Save();

				AssertEquals("Should have error message before marked job as inactive.", @"This record cannot be deleted.
An Invoicing Job Header (S00001001) has been created in the company EDI.", plugin.CannotDeleteMessage);
			}
		}

		public void TestCannotMarkInvoicingJobActiveMessage()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					Assert("Tab page should contain the control", plugInToFreight.TabPage.Controls.Contains(plugInToFreight.UserControl));
					plugInToFreight.OnSaving();
					Factory.Save();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					AccHotCheque accHotCheque = Factory.NewWithValidTestData<AccHotCheque>();
					accHotCheque.AQ_AK = TestObjectCreator.AUDChequeBook.PK;
					accHotCheque.AQ_ChequeNumber = "10006";
					accHotCheque.AQ_JH = plugInToFreight.Job_ForTestOnly.PK;
					Factory.Save();

					AssertMarkInvoicingJobAsInactiveMessageError(plugInToFreight, @"This job cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header.");

					ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_JH = plugInToFreight.Job_ForTestOnly.PK;
					Factory.Save();

					AssertMarkInvoicingJobAsInactiveMessageError(plugInToFreight, @"This job cannot be deactivated.
Accounting Transaction(s) have been saved against this Invoicing Job Header.");
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestCannotMarkInvoicingJobActiveMessage_JobInvoicingCharges()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					Assert("Tab page should contain the control", plugInToFreight.TabPage.Controls.Contains(plugInToFreight.UserControl));
					plugInToFreight.OnSaving();
					Factory.Save();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_JH = plugInToFreight.Job_ForTestOnly.PK;

					ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
					invoiceLine.AL_JH = plugInToFreight.Job_ForTestOnly.PK;
					invoiceLine.AL_OSExTaxAmount = 10m;
					invoiceLine.AL_LocalExTaxAmount = 10m;
					invoice.AH_FullyPaidDate = ZDateTime.Empty;
					var charge = TestObjectCreator.CreateJobCharge(invoiceLine, plugInToFreight.Job_ForTestOnly, TestObjectCreator.CC1, TestObjectCreator.AUD);
					charge.JR_OSCostAmt = 10m;
					charge.JR_LocalCostAmt = 10m;
					Factory.Save();

					AssertMarkInvoicingJobAsInactiveMessageError(plugInToFreight, @"This job cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header.");
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestCannotMarkInvoicingJobActiveMessageForApportionedCharges()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			consol.Shipments.Add(shipment);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100M;
			Factory.Save();

			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					Assert("Tab page should contain the control", plugInToFreight.TabPage.Controls.Contains(plugInToFreight.UserControl));
					plugInToFreight.OnSaving();
					Factory.Save();

					AssertEquals("Job_ForTestOnlyCharge should be created", 1, plugInToFreight.Job_ForTestOnly.Charges.Count);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					AssertMarkInvoicingJobAsInactiveMessageError(plugInToFreight, @"This job cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header.");
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		void AssertMarkInvoicingJobAsInactiveMessageError(InvoicingPluginToFreight plugInToFreight, string expectedError)
		{
			plugInToFreight.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugInToFreight, new EventArgs());
			AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestJobHeaderIsDeactivatedByAnotherUser()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					plugInToFreight.OnSaving();
					Factory.Save();
					var firstJob = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
					AssertNotNull("Job should be created", firstJob);

					var someOtherFactory = new BusinessObjectFactory();
					someOtherFactory.RefreshEnabled = false;
					var firstJobToBeDeactivated = someOtherFactory.Load<Job>(firstJob.PK);
					firstJobToBeDeactivated.MarkAsInactive();
					someOtherFactory.Save();

					var expectedNullJob = someOtherFactory.Load<Job>(firstJob.PK);
					AssertEquals("Job should be cancelled", true, expectedNullJob.IsCancelled);

					plugInToFreight.OnGUIShown();
					plugInToFreight.ShowPreSaveDialogs();

					AssertEquals(plugInToFreight.GetJobWasDeactivatedByAnotherUserMessage_ForTestOnly(firstJob.PK), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestJobObjectDeleted()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					var currentJob = plugInToFreight.Job_ForTestOnly;
					currentJob.Delete();

					AssertEquals(true, plugInToFreight.Job_ForTestOnly.IsDeleted);
					AssertNoExceptionThrown("No exception should be thrown here", () => plugInToFreight.OnSaving());
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestMakeJobIfNullWillMakeNewJobIfJobIsDeactivated()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (var plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					plugInToFreight.OnSaving();
					Factory.Save();

					var currentJob = plugInToFreight.Job_ForTestOnly;
					plugInToFreight.DeactivateJobSaveAndClosedForm_ForTestOnly();
					Assert("IsCancelled should return true", currentJob.IsCancelled);
					plugInToFreight.FJob_ForTestOnly = currentJob;
					plugInToFreight.MakeOrActivateJob_ForTestOnly();
					Assert("Job_ForTestOnly should be re-activate after deactivated", plugInToFreight.Job_ForTestOnly.JH_IsActive);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestJobWillBeDeactivatedOnSaveMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (var plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					plugInToFreight.OnSaving();
					Factory.Save();

					var jobNumber = plugInToFreight.Job_ForTestOnly.JH_JobNum;
					var expectedDeleteMessage = string.Format("Invoicing job {0} will be marked as inactive when you save this Shipment.\r\n\r\nPlease change to another tab, then click back to this tab to activate invoicing job for this Shipment\r\n", jobNumber);
					AssertEquals("Should be standard Deactivate message", expectedDeleteMessage, plugInToFreight.JobWillBeDeactivatedOnSaveMessage_ForTestOnly);

					shipment.IsCancelled = true;
					// Have to call it explicitely. Should be called by a ZFormMenuStrategy
					plugInToFreight.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);

					var expectedDeactivateMessage = string.Format("You have set this Shipment {0} to inactive.\r\nThis will mark the invoicing Job {0} as inactive when you save this Shipment.\r\n\r\n", jobNumber);

					AssertEquals("Should be Deactivate message", expectedDeactivateMessage, plugInToFreight.JobWillBeDeactivatedOnSaveMessage_ForTestOnly);
					Factory.Save();

					plugInToFreight.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);

					expectedDeactivateMessage = string.Format("This Shipment {0} has been set to inactive.\r\n\r\n", jobNumber);

					AssertEquals("Should be Deactivate message", expectedDeactivateMessage, plugInToFreight.JobWillBeDeactivatedOnSaveMessage_ForTestOnly);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestJobWillBeDeactivatedOnSaveMessage_UnsavedJob()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			using (var plugInToFreight = new InvoicingPluginToFreight(shipment))
			{
				try
				{
					plugInToFreight.OnGUIShown();
					plugInToFreight.OnSaving();

					string jobNumber = plugInToFreight.Job_ForTestOnly?.JH_JobNum ?? string.Empty;
					string expectedDeleteMessage = string.Format("Invoicing job {0} has not been created.\r\n\r\nPlease change to another tab, then click back to this tab to create an invoicing job for this Shipment\r\n", jobNumber);
					AssertEquals("Should be standard Deactivate message", expectedDeleteMessage, plugInToFreight.JobWillBeDeactivatedOnSaveMessage_ForTestOnly);

					shipment.IsCancelled = true;
					// Have to call it explicitely. Should be called by a ZFormMenuStrategy
					plugInToFreight.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);

					string expectedDeactivateMessage = string.Format("You have set this Shipment {0} to inactive.\r\nThis will mark the invoicing Job {0} as inactive when you save this Shipment.\r\n\r\n", jobNumber);

					AssertEquals("Should be Deactivate message", expectedDeactivateMessage, plugInToFreight.JobWillBeDeactivatedOnSaveMessage_ForTestOnly);
				}
				finally
				{
					TearDownJobHeaderCreationTest();
				}
			}
		}

		public void TestCancelledJobChargeControlIsReadonly_AfterSubsequentTabChange()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null, true);

			using (ZForm form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage tabPage1 = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();

				var jobChargeUserControl = ((JobInvoicingUserControl)plugin.UserControl).JobChargeUserControl;

				tabControl.SelectedTab = plugin.TabPage;
				plugin.OnGUIShown();
				AssertEquals("JobChargeUserControl.JobChargeBoundGrid.ReadOnly should be false", false, jobChargeUserControl.JobChargeBoundGrid.ReadOnly);
				AssertNotNull(jobChargeUserControl.Job);

				plugin.OnSaving();
				Factory.Save();

				((ICancellable)shipment).IsCancelled = true;
				tabControl.SelectedTab = tabPage1;
				tabControl.SelectedTab = plugin.TabPage;
				plugin.OnGUIShown();
				AssertNotNull(jobChargeUserControl.Job);

				tabControl.SelectedTab = tabPage1;
				tabControl.SelectedTab = plugin.TabPage;
				plugin.OnGUIShown();
				AssertEquals("JobChargeUserControl.JobChargeBoundGrid should be readonly", true, jobChargeUserControl.JobChargeBoundGrid.ReadOnly);
			}
		}

		public void TestCancelledJobNotCreateNewBillingJob_AfterSubsequentTabChange()
		{
			SetUpJobHeaderCreationTest(false);
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null, true);

			using (ZForm form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				InvoicingPluginToFreight plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZTabPage tabPage1 = new ZTabPage();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(plugin.TabPage);
				form.Show();

				var jobChargeUserControl = ((JobInvoicingUserControl)plugin.UserControl).JobChargeUserControl;
				AssertNull(jobChargeUserControl.Job);

				Factory.Save();

				tabControl.SelectedTab = tabPage1;
				((ICancellable)shipment).IsCancelled = true;
				tabControl.SelectedTab = plugin.TabPage;
				plugin.OnGUIShown();

				AssertNull(jobChargeUserControl.Job);

				plugin.OnSaving();
				Factory.Save();

				tabControl.SelectedTab = tabPage1;
				tabControl.SelectedTab = plugin.TabPage;
				plugin.OnGUIShown();
				AssertNull(jobChargeUserControl.Job);
			}

			TearDownJobHeaderCreationTest();
		}

		public void TestCanDeleteJobChargeWhenNoAccountingTransactionRecordedAgainstJob_DeactivateJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S0001");
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);
			job.JH_ParentID = shipment.PK;
			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code; //ready for cost posting
			var charge = job.Charges.AddNew();
			charge.JR_AC = objectCreator.CC1.PK;
			charge.JR_OSCostAmt = 0m; //avoid creating transaction lines
			charge.JR_OSSellAmt = 0m; //avoid creating transaction lines
			Factory.Save();

			using (var plugin = new InvoicingPluginToFreightWrapper(shipment))
			{
				plugin.OnGUIShown(); //Register OnCannotDeleteHandler
				shipment.JS_IsCancelled = true; //Deactivate shipment

				Assert("Job can be deleted as there are no transations referring it", job.CanDeactivate);
				Assert("Charge cannot be delete as it is ready for cost posting", !charge.CanDelete);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugin.OnBusinessObjectIsCancelledChanged(shipment.JS_IsCancelled);

				AssertEquals("No accounting transaction has been recorded against this job. Job charge ZZCC1 will be deleted when you deactivate this Shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);

				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				Assert("Job is canceled", newFactory.Load<Job>(job.PK).IsCancelled);
				AssertNull("Charge is deleted", newFactory.Load<JobCharge>(charge.PK));
			}
		}

		void SetUpJobHeaderCreationTest(bool registryValue)
		{
			OriginalRegistryValue = AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Value;
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}

		void TearDownJobHeaderCreationTest()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OriginalRegistryValue);
		}

		bool OriginalRegistryValue;

		#endregion

		#region Autorating

		public void TestExRatesOfAutoratingWhenHasClientSpecificRates()
		{
			#region Setup

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var consignor = TestObjectCreator.CreateOrgHeader("CNSTR", false, false, "CNSHA");
			var localClient = TestObjectCreator.CreateOrgHeader("AUDST", false, true, "AUSYD");

			var query = new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC"), new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			var ddoc = Factory.Load<AccChargeCode>(query).First();

			var ratingHeader = Factory.New<ClientRate>();
			ratingHeader.TH_OH = localClient.PK;

			var entry = Factory.New<RateEntry>();
			entry.TI_TH = ratingHeader.PK;
			entry.TI_Mode = Core.Constants.RateMode.LSE;
			entry.TI_GC_Publisher = GlbCompany.CurrentCompany.PK;
			entry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			entry.TI_OriginLRC = Core.Constants.CountryCodes.China;
			entry.TI_DestinationLRC = Core.Constants.CountryCodes.Australia;
			entry.TI_RH_NKCommodityCode = "GEN";
			entry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.China;

			var line = entry.RateLines.AddNew();
			line.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.China;
			line.TL_AC = TestObjectCreator.FRT.PK;
			line.TL_RateCalculator = FlatCalculator.Code;
			line.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;

			((RateLineItem)line.RateLineItems.First()).TM_Value = 1000m;

			var newEntry = Factory.New<RateEntry>();
			newEntry.TI_TH = ratingHeader.PK;
			newEntry.TI_GC_Publisher = GlbCompany.CurrentCompany.PK;
			newEntry.TI_Mode = Core.Constants.RateMode.ALL;
			newEntry.TI_RateCategory = RatingConstants.RateCategory.DST;
			newEntry.TI_OriginLRC = Core.Constants.CountryCodes.China;
			newEntry.TI_DestinationLRC = Core.Constants.CountryCodes.Australia;
			newEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			var newLine = newEntry.RateLines.AddNew();
			newLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			newLine.TL_AC = ddoc.PK;
			newLine.TL_RateCalculator = PercentageCalculator.Code;
			newLine.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;

			var newItem = newLine.RateLineItems.AddNew();
			newItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			newItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			newItem.TM_AC = TestObjectCreator.FRT.PK;

			var currency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.China)).FirstOrDefault();
			var buy = currency.ExchangeRates.AddNew();
			buy.RE_StartDate = DateTime.Today.AddDays(-10);
			buy.RE_ExpiryDate = DateTime.Today.AddDays(10);
			buy.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buy.RE_SellRate = 4.55m;

			var sell = currency.ExchangeRates.AddNew();
			sell.RE_StartDate = DateTime.Today.AddDays(-10);
			sell.RE_ExpiryDate = DateTime.Today.AddDays(10);
			sell.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sell.RE_SellRate = 1.37m;

			Factory.Save();

			#endregion

			Shipment = TestObjectCreator.CreateShipment("S00010001", "CNSHA", "AUSYD", null);
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Shipment.ConsigneeDocumentaryAddress.OrganisationPK = localClient.PK;
			Shipment.JS_RL_NKOrigin = "CNSHA";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_E_ARV = DateTime.Today;
			Shipment.JS_E_ARV = DateTime.Today;

			var job = TestObjectCreator.CreateJob(Shipment);
			job.LocalZAddressWithContact.OrgPK = localClient.PK;
			job.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;

			Factory.Save();
			using (var form = new ZForm(Shipment))
			{
				form.Controls.Add(new ZTemplateTabControl());
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.OnGUIShown();
				invoicingPlugin.Job_ForTestOnly.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				Factory.Save();

				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateRevenue);

				var exchangeRates = invoicingPlugin.Job_ForTestOnly.ExchangeRates;
				var jobCharges = invoicingPlugin.Job_ForTestOnly.Charges;

				AssertEquals("This job should contain 2 exchange rates", 2, exchangeRates.Count);
				AssertEquals("This job should contain 2 job charges", 2, jobCharges.Count);

				AssertEquals("This job should contain DDOC charge code", ddoc.PK, jobCharges.ContainsChargeCode(ddoc).JR_AC);
				AssertEquals("This job should contain FRT charge code", TestObjectCreator.FRT.PK, jobCharges.ContainsChargeCode(TestObjectCreator.FRT).JR_AC);

				var sellRate = exchangeRates.FindByRefCurrency(sell.ExCurrency);
				AssertEquals("Sell exchange rate should be 4.55 based on Job Billing Exchange Rates configuration", buy.RE_SellRate, sellRate.JF_BaseRate);

				var buyRate = exchangeRates.FindByRefCurrency(buy.ExCurrency);
				AssertEquals("Buy exchange rate should be 4.55", buy.RE_SellRate, buyRate.JF_BaseRate);
				AssertEquals("Buy exchange rate should be client specific", localClient.PK, buyRate.JF_OH_Org);
				Assert("Buy exchange rate should not override", !buyRate.JF_IsTransformed);

				Assert("This job should not have any errors", !invoicingPlugin.Job_ForTestOnly.HasErrors);
			}
		}

		public void TestAutoratingShowsWarningForNoFoundRates()
		{
			const string Expected = @"AutoRating has been completed.
Please review the following warnings:
Declaration B00001000 was auto-costed.
	No costs were found.
Declaration B00001000 was auto-rated.
	No rates were found.
Please refer to Notes->AutoRating Log for more information.";

			var dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.JE_MessageType.Name] = "EXP";

			Factory.Save();
			using (var form = new ZForm(dec))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var accountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				accountingModule.OnGUIShown(); // to init Job property
				accountingModule.Job_ForTestOnly.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				accountingModule.Job_ForTestOnly.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				accountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals(Expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExecuteAutoratingCreatesAndRemovesFallbackNote_NoteEnabled()
		{
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Func<ForwardingShipment, bool> hasNote = parent => parent.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Any();

			using (var form = new ZForm(Shipment))
			{
				form.Controls.Add(new ZTemplateTabControl());
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				invoicingPlugin.OnGUIShown();
				invoicingPlugin.Job_ForTestOnly.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				invoicingPlugin.Job_ForTestOnly.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				Factory.Save();

				AssertEquals("Pre-condition: No autorating notes exists", false, hasNote(Shipment));
				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var autoRatingNote1 = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).First();
				AssertNotNull("Autorating note exists", autoRatingNote1);
				AssertEquals("Current company is set", Env.CurrentCompanyPK, autoRatingNote1.ST_GC_RelatedCompany);

				Factory.Save();

				AssertEquals("Autorating note IS NOT deleted on saving", true, hasNote(Shipment));

				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				var autoRatingNote2 = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).First();
				autoRatingNote2.ST_NoteText = ZString.Empty;

				AssertHasErrors("Pre-condition: should not have empty note", autoRatingNote2.ST_NoteTextInfo);
				Factory.Save();

				AssertEquals("Autorating note is deleted on saving if the note has errors because the client can't edit or delete the note", false, hasNote(Shipment));

				DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
				invoicingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals("Note should be recreated as autorating has been run again", true, hasNote(Shipment));

				Factory.Save();

				AssertEquals("Autorating note is deleted on saving if the note when the registry is off", false, hasNote(Shipment));
			}
		}

		public void TestDisbursementChargeCodeLoadsCorrectly()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			InvoicingParam @params = new InvoicingParam();
			@params.PK = Guid.NewGuid();
			@params.TableName = JobShipmentSchema.Constants.TableName;
			@params.JobNumber = "test";

			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(@params))
			{
				AssertEquals("Disbursement Charge Code returned", chargeCode.PK, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			}
		}

		public void TestExecuteAutoratingDoesntRateCustomsChargesForExports()
		{
			BusinessObject dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.JE_MessageType.Name] = "EXP";

			using (ZForm form = new ZForm(dec))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				InvoicingPluginToFreight fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				fAccountingModule.OnGUIShown(); // to init Job property
				fAccountingModule.Job_ForTestOnly.JH_GE = GlbDepartment.CurrentDepartment.PK;

				AssertEquals("Pre-condition: No charges before autorating", 0, fAccountingModule.Job_ForTestOnly.Charges.Count);

				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals("Customs Disbursement charge NOT rated as this is an export", 0, fAccountingModule.Job_ForTestOnly.Charges.Count);
			}
		}

		public void TestRunPrePostingValidation()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL");
			ZGuid chargeCode = creator.MRG100.PK;
			creator.AALSHI.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";
			creator.AALSHI.OH_IsDebtor = true;
			ZGuid debtor = creator.AALSHI.PK;
			Factory.Save();

			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(shipment))
			using (fAccountingModule.UserControl)
			{
				fAccountingModule.OnGUIShown();
				fAccountingModule.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals(AccountingPeriodCalculator.GetInvalidPeriodValidationError(ZDateTime.Today), UnitTestUserNotification.Instance.LastMessage.Text);
				AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
				periodManagementTestHelper.SetupPeriods();

				fAccountingModule.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals("You cannot post because no Job Invoices have been created.", UnitTestUserNotification.Instance.LastMessage.Text);

				fAccountingModule.OnUserControlShown();
				Job shipmentJob = new Job.Loader(Factory, shipment).Load();
				shipmentJob.AgentCollectPK = debtor;
				shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				Charge charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = chargeCode;
				charge.JR_OSCostAmt = 100m;
				charge.JR_OH_SellAccount = debtor;
				charge.JR_InvoiceType = "FIN";
				Factory.Save();

				shipmentJob.JH_OA_AgentCollectAddr = ZGuid.Empty;
				shipmentJob.Validation.ValidateAll();

				Factory.Save();

				fAccountingModule.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals(@"You cannot post because job S001 has errors. Please fix errors before posting.
 - Overseas Agent: Please enter Local Client or Overseas Agent.
 - Local Client: Please enter Local Client or Overseas Agent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStorageValidationDuringAutorating()
		{
			AccChargeCode testStorageCharge = Factory.New<AccChargeCode>();
			testStorageCharge.AC_Code = "STOR";
			testStorageCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			testStorageCharge.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			var bizO = new InvoicingParam();
			bizO.PK = Guid.NewGuid();
			bizO.TableName = JobShipmentSchema.Constants.TableName;
			bizO.JobNumber = "test";

			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(bizO))
			using (fAccountingModule.UserControl)
			{
				fAccountingModule.OnUserControlShown();
				fAccountingModule.Job_ForTestOnly.JH_RatingHasBeenRun = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("No warning regarding storage being applicable", null, UnitTestUserNotification.Instance.LastMessage.Text);
				bizO.HasStorage = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("Warning regarding storage being applicable", "Storage is applicable on this job, but there are no Storage charge codes present on the Invoice. Do you want to autorate now?", UnitTestUserNotification.Instance.LastMessage.Text);

				Charge charge = Factory.New<Charge>();
				charge.JR_AC = testStorageCharge.PK;
				fAccountingModule.Job_ForTestOnly.Charges.Add(charge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("No warning regarding storage being applicable as the charge code is there", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoratingNotRunWarning()
		{
			var bizO = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(bizO))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(new ZTabPage());
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();
				UserIdleWorker.Flush();

				InvoicingPluginToFreight fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];

				fAccountingModule.Job_ForTestOnly = new Job.Loader(Factory, bizO).TryCreateWithoutMutexForTestOnly();
				fAccountingModule.Job_ForTestOnly.JH_RatingHasBeenRun = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("Warning regarding autorating not run", "AutoRating has not yet been run on this job. Do you want to autorate now?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("No warning regarding autorating not run because it has been already shown", null, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				fAccountingModule.Job_ForTestOnly.Delete();
				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("No warning regarding autorating not run as the job has been deleted.", null, UnitTestUserNotification.Instance.LastMessage.Text);

				var query = new ZDBOnlyQuery(typeof(StmALog));
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "RTN");
				query.AddToFilter(StmALogSchema.SL_Parent, bizO.PK);

				AssertNull(new BusinessObjectFactory().LoadTop1<StmALog>(query));
			}
		}

		public void TestNoAutoratingOnStorageCFSCausesEvent()
		{
			InvoicingParam bizO = new InvoicingParam();
			bizO.PK = (bizO as BusinessObject).PK;
			bizO.TableName = JobShipmentSchema.Constants.TableName;
			bizO.JobNumber = "test";

			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(bizO))
			using (fAccountingModule.UserControl)
			{
				fAccountingModule.OnUserControlShown();
				fAccountingModule.Job_ForTestOnly.JH_RatingHasBeenRun = true;
				bizO.HasStorage = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				fAccountingModule.ShowPreSaveDialogs();
				AssertEquals("Warning regarding storage being applicable", "Storage is applicable on this job, but there are no Storage charge codes present on the Invoice. Do you want to autorate now?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreAutoratingErrors()
		{
			InvoicingParam @params = new InvoicingParam();
			@params.PK = Guid.NewGuid();
			@params.TableName = JobShipmentSchema.Constants.TableName;

			@params.JobNumber = "test";
			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(@params))
			{
				using (fAccountingModule.UserControl)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					AssertEquals("record : Job has not been created or reactivated yet.", UnitTestUserNotification.Instance.LastMessage.Text);

					fAccountingModule.OnUserControlShown();

					var jobToPost = fAccountingModule.Job_ForTestOnly;
					jobToPost.ReadOnly = true;
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					AssertEquals(@"record : Autorating cannot be run while the billing job is read only.", UnitTestUserNotification.Instance.LastMessage.Text);

					jobToPost.ReadOnly = false;
					jobToPost.Charges.AddNew();
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

					AssertEquals("record : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.\r\n\tError - JR_AC: Please enter a Charge Code.\r\n\tError - JR_Desc: Description cannot be empty.\r\n\tError - JR_GE: Please enter a Department.\r\n\tError - JH_GE: Please enter a Department.\r\n\tWarning - JH_GS_NKRepSales: You have not entered a Sales Rep.\r\n\tError - JH_OA_AgentCollectAddr: Please enter Local Client or Overseas Agent.\r\n\tError - JH_OA_LocalChargesAddr: Please enter Local Client or Overseas Agent.", UnitTestUserNotification.Instance.LastMessage.Text);

					jobToPost.ReadOnly = false;
					jobToPost.MarkAsInactive();
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					AssertEquals("record : Job has not been created or reactivated yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPreAutoratingErrors_InactiveBilling()
		{
			InvoicingParam @params = new InvoicingParam();
			@params.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			AssertEquals("Precondition", false, @params.ConsumerType.IsActive);
			@params.PK = Guid.NewGuid();
			@params.TableName = JobShipmentSchema.Constants.TableName;

			@params.JobNumber = "test";
			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(@params))
			{
				using (fAccountingModule.UserControl)
				{
					fAccountingModule.OnUserControlShown();

					fAccountingModule.Job_ForTestOnly.ReadOnly = true;
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					AssertEquals(@"record : Autorating cannot be run while the billing job is read only.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPreAutoratingErrorsMustSetCustomsDisbursementChargeCode()
		{
			BusinessObject dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";

			var customsDisbursementChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode;
			Guid customsChargeCodePK = customsDisbursementChargeCode.Value;
			using (customsDisbursementChargeCode.DataType.SuspendValidation())
			{
				customsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}

			Factory.Save();
			using (ZForm form = new ZForm(dec))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();
				UserIdleWorker.Flush();

				InvoicingPluginToFreight fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", UnitTestUserNotification.Instance.LastMessage.Text);

				dec[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals("You must set up the 'Customs Disbursement Charge Code' in the registry before Autorating can be run.", UnitTestUserNotification.Instance.LastMessage.Text);

				RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customsChargeCodePK);
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreAutoratingErrorsMustSetCustomsDeferredChargeCode()
		{
			BusinessObject dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";

			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Factory.Save();
			using (ZForm form = new ZForm(dec))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();
				UserIdleWorker.Flush();

				InvoicingPluginToFreight fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", UnitTestUserNotification.Instance.LastMessage.Text);

				dec[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals("You have enabled 'Include Custom Deferred Charge in Invoicing' in the registry, but the corresponding 'Custom Deferred Charge' is not set up in the registry." +
						System.Environment.NewLine +
						"	Please set up 'Custom Deferred Charge' or disable 'Include Custom Deferred Charge in Invoicing' in the registry.",
						UnitTestUserNotification.Instance.LastMessage.Text);

				RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestCustomsDeferredChargeCode.PK.ToGuid());
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreAutoratingErrorsMustSetCustomsChargeCode()
		{
			BusinessObject dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var customsDisbursementChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode;
			using (customsDisbursementChargeCode.DataType.SuspendValidation())
			{
				customsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}

			Factory.Save();
			using (ZForm form = new ZForm(dec))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();
				UserIdleWorker.Flush();

				InvoicingPluginToFreight fAccountingModule = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				AssertEquals("Following registry items are not set up correctly. Please fix them before you run the auto rating." +
						System.Environment.NewLine +
						"	 - You must set up the 'Customs Disbursement Charge Code' in the registry before Autorating can be run." +
						System.Environment.NewLine +
						"	 - You have enabled 'Include Custom Deferred Charge in Invoicing' in the registry, but the corresponding 'Custom Deferred Charge' is not set up in the registry." +
						System.Environment.NewLine +
						"	Please set up 'Custom Deferred Charge' or disable 'Include Custom Deferred Charge in Invoicing' in the registry.",
						UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoRatingObjectThatDoesntImplementIAutoRating()
		{
			InvoicingParamWithoutIAutoRating @params = new InvoicingParamWithoutIAutoRating();
			@params.PK = Guid.NewGuid();
			@params.TableName = JobShipmentSchema.Constants.TableName;

			@params.JobNumber = "test";
			using (InvoicingPluginToFreight fAccountingModule = new InvoicingPluginToFreight(@params))
			{
				using (fAccountingModule.UserControl)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					fAccountingModule.OnUserControlShown();
					fAccountingModule.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					AssertEquals("You cannot perform Autorating. Please enter your invoice charges manually for this kind of job.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Mutex

		public void TestQueryUserShouldPlugInGUIAndBusinessEntityBeCreated_CreatesJobAndAquiresMutex()
		{
			ZGuid plugInPK = ZGuid.NewZGuid();
			InvoicingPluginToFreight plugIn1 = null;
			InvoicingPluginToFreight plugIn2 = null;

			try
			{
				InvoicingParam parent1 = new InvoicingParam();
				parent1.PK = plugInPK;
				plugIn1 = new InvoicingPluginToFreight(parent1);

				InvoicingParam parent2 = new InvoicingParam();
				parent2.PK = plugInPK;
				plugIn2 = new InvoicingPluginToFreight(parent2);

				AssertEquals("Plugin1 mutex acquired", true, plugIn1.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("Plugin2 mutex fails to be acquired", false, plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());

				plugIn1.Dispose();
				plugIn1 = null;

				AssertEquals("Plugin2 mutex acquired", true, plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
			}
			finally
			{
				if (plugIn2 != null)
				{
					plugIn2.Dispose();
				}

				if (plugIn1 != null)
				{
					plugIn1.Dispose();
				}
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated_DoesntCreateJobOrAquireMutex()
		{
			InvoicingParam parent = new InvoicingParam();
			parent.PK = ZGuid.NewZGuid();

			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(parent))
			{
				AssertEquals("Plugin1 cannot have business entity created yet as it doesnt have a job created", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("Plugin1 job created and mutex acquired", true, plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("Plugin1 job created and mutex acquired", true, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("Plugin1 job created and mutex acquired", true, plugIn.ShouldPluginDropdownMenuBeCreated_ForTestOnly());
			}
		}

		public void TestShouldPluginDropdownMenuBeCreated_CreatesJob()
		{
			InvoicingParam parent = new InvoicingParam();
			parent.PK = ZGuid.NewZGuid();

			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(parent))
			{
				AssertNull("Pre-condition: Job_ForTestOnly not created", plugIn.Job_ForTestOnly);
				AssertEquals("PlugIn cannot have business entity created yet as it doesnt have a job created", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());

				AssertEquals("PlugIn job should be created by ShouldPluginDropdownMenuBeCreated_ForTestOnly", true, plugIn.ShouldPluginDropdownMenuBeCreated_ForTestOnly());
				AssertNotNull("PlugIn job created", plugIn.Job_ForTestOnly);
				AssertEquals("PlugIn job created", true, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
			}
		}

		public void TestNoExceptionsWhenCreatingAJobWithAMutex()
		{
			ZGuid plugInPK = ZGuid.NewZGuid();
			InvoicingPluginToFreight plugIn1 = null;
			InvoicingPluginToFreight plugIn2 = null;

			try
			{
				InvoicingParam parent1 = new InvoicingParam();
				((InvoicingParamJobInvoicingSupporter)parent1.InvoicingSupporter).createAccountingJobOnSavingOfOperationsJob = true;
				parent1.PK = plugInPK;
				plugIn1 = new InvoicingPluginToFreight(parent1);
				plugIn1.CreateInvoicingJobIfRequired_ForTestOnly();

				InvoicingParam parent2 = new InvoicingParam();
				((InvoicingParamJobInvoicingSupporter)parent1.InvoicingSupporter).createAccountingJobOnSavingOfOperationsJob = true;
				parent2.PK = plugInPK;
				plugIn2 = new InvoicingPluginToFreight(parent2);
				plugIn2.CreateInvoicingJobIfRequired_ForTestOnly();

				Assert("Plugin 1 Job_ForTestOnly should not be null", plugIn1.Job_ForTestOnly != null);
				Assert("Plugin 2 Job_ForTestOnly should be null", plugIn2.Job_ForTestOnly == null);
			}
			finally
			{
				if (plugIn1 != null)
				{
					plugIn1.Dispose();
				}

				if (plugIn2 != null)
				{
					plugIn2.Dispose();
				}
			}
		}
		public void TestNoMenuWhenNoBillingSecurityRight()
		{
			AssertNoMenuWhenNoBillingSecurityRight(false, false);
			AssertNoMenuWhenNoBillingSecurityRight(false, true);
			AssertNoMenuWhenNoBillingSecurityRight(true, false);
			AssertNoMenuWhenNoBillingSecurityRight(true, true);
		}

		void AssertNoMenuWhenNoBillingSecurityRight(bool isLoginAllowed, bool isViewBillingAllowed)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var jobBranch = GlbBranch.CurrentBranch;
			var jobDepartment = GlbDepartment.CurrentDepartment;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job = TestObjectCreator.CreateJob(shipment);
			shipment.Job.JH_GB = jobBranch.PK;
			shipment.Job.JH_GE = jobDepartment.PK;
			Factory.Save();

			Factory.ClearCachedValue<bool>(("Login BRN:" + jobBranch.GB_Code + " DEP:" + jobDepartment.GE_Code));

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
			{
				plugIn.OnGUIShown();

				var expectedErrorMessage = string.Format("This billing job has a branch {0} and department {1}. Viewing of the billing tab for this job is disallowed because you do not have security rights to login to this branch and department.",
					jobBranch.GB_Code, jobDepartment.GE_Code) + plugIn.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.AllowViewEditBilling);

				plugIn.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowViewEditBilling).IsAllowed = isViewBillingAllowed;
				var securityFactory = new BusinessObjectFactory();
				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, jobBranch.PK.ToGuid(), jobDepartment.PK.ToGuid());
				security.Login.IsAllowed = isLoginAllowed;
				var loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = jobBranch.PK;
				loginSecurity.GU_GE = jobDepartment.PK;
				loginSecurity.GU_GC = jobBranch.Company.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				loginSecurity.GU_SecurityItemIsAllowed = isLoginAllowed;
				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				bool expectedResult = isLoginAllowed || isViewBillingAllowed;
				AssertEquals(expectedResult, plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				if (expectedResult)
				{
					AssertNotEquals(expectedErrorMessage, plugIn.PlugInNotDisplayedMessage);
				}
				else
				{
					AssertEquals(expectedErrorMessage, plugIn.PlugInNotDisplayedMessage);
				}
			}
		}

		public void TestNoMenuAfterAnotherPlugInCreatedJobWithAMutex()
		{
			ZGuid plugInPK = ZGuid.NewZGuid();
			InvoicingPluginToFreight plugIn1 = null;
			InvoicingPluginToFreight plugIn2 = null;

			try
			{
				InvoicingParam parent1 = new InvoicingParam();
				((InvoicingParamJobInvoicingSupporter)parent1.InvoicingSupporter).createAccountingJobOnSavingOfOperationsJob = true;
				parent1.PK = plugInPK;
				plugIn1 = new InvoicingPluginToFreight(parent1);
				plugIn1.CreateInvoicingJobIfRequired_ForTestOnly();

				InvoicingParam parent2 = new InvoicingParam();
				parent2.PK = plugInPK;
				plugIn2 = new InvoicingPluginToFreight(parent2);
				plugIn2.CreateInvoicingJobIfRequired_ForTestOnly();

				Assert("PlugIn 1 Job_ForTestOnly should not be null", plugIn1.Job_ForTestOnly != null);
				Assert("Plugin 2 Job_ForTestOnly should be null", plugIn2.Job_ForTestOnly == null);

				AssertEquals("PlugIn 1 job created and mutex acquired", true, plugIn1.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("PlugIn 1 job created and mutex acquired", true, plugIn1.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("PlugIn 1 job created and mutex acquired", true, plugIn1.ShouldPluginDropdownMenuBeCreated_ForTestOnly());

				AssertEquals("PlugIn 2 should not create dropdown menu  because another PlugIn acquired the mutex", false, plugIn2.ShouldPluginDropdownMenuBeCreated_ForTestOnly());
				AssertEquals("PlugIn 2 job cannot be created because another PlugIn acquired the mutex", false, plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				AssertEquals("PlugIn 2 should not created GUI or BusinessEntity because another PlugIn acquired the mutex", false, plugIn2.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
			}
			finally
			{
				if (plugIn1 != null)
				{
					plugIn1.Dispose();
				}

				if (plugIn2 != null)
				{
					plugIn2.Dispose();
				}
			}
		}

		#endregion

		public void TestPlugQueryDoesTriggerValidateOnSaveIfAllowToViewBillingTab()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job = TestObjectCreator.CreateJob(shipment);
			shipment.Job.JH_GB = GlbBranch.CurrentBranch.PK;
			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.RunPreSaveValidation();

			Factory.Save();
			Assert("Precondition - charge validation should not be suspended", !charge.IsValidationSuspended);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
				{
					Assert("Precondition - IsAllowedToViewBillingTab_ForTestOnly should be true", plugIn.IsAllowedToViewBillingTab_ForTestOnly);
					plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
					Assert("Charge validation should not be suspended due to AllowViewEditBilling is true", !charge.IsValidationSuspended);
				}
			}
		}

		public void TestPlugQueryDoesNotTriggerValidateOnSaveIfNotAllowToViewBillingTab()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var jobBranch = Factory.NewWithValidTestData<GlbBranch>();
			var jobDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job = TestObjectCreator.CreateJob(shipment);
			shipment.Job.JH_GB = jobBranch.PK;
			shipment.Job.JH_GE = jobDepartment.PK;
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.RunPreSaveValidation();
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 200M, null, TestObjectCreator.AUD, 200M, null);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
			Assert("Precondition - charge validation should not be suspended", !charge1.IsValidationSuspended);
			Assert("Precondition - charge validation should not be suspended", !charge2.IsValidationSuspended);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
				{
					plugIn.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowViewEditBilling).IsAllowed = false;
					var securityFactory = new BusinessObjectFactory();

					var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
					security.Login.IsAllowed = false;

					var loginSecurity = securityFactory.New<GlbSecurity>();
					loginSecurity.GU_GB = jobBranch.PK;
					loginSecurity.GU_GE = jobDepartment.PK;
					loginSecurity.GU_GC = jobBranch.Company.PK;
					loginSecurity.GU_GS = Env.CurrentUser.PK;
					loginSecurity.GU_SecurityRight = security.Login.Code;
					loginSecurity.GU_SecurityItemIsAllowed = false;
					securityFactory.Save();

					Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

					Assert("Precondition - IsAllowedToViewBillingTab_ForTestOnly should be false", !plugIn.IsAllowedToViewBillingTab_ForTestOnly);
					Assert("Plugin should not be shown", !plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
					Assert("Charge validation should be suspended due to AllowViewEditBilling is false", charge1.IsValidationSuspended);
					Assert("Charge validation should be suspended due to AllowViewEditBilling is false", charge2.IsValidationSuspended);

					plugIn.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowViewEditBilling).IsAllowed = true;

					Assert(plugIn.IsAllowedToViewBillingTab_ForTestOnly);
					Assert("Plugin should be shown", plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
					Assert("Charge validation is not suspended", !charge1.IsValidationSuspended);
					Assert("Charge validation is not suspended", !charge2.IsValidationSuspended);
				}
			}
		}

		public void TestIsInvoiceDeletionAllowedByParent()
		{
			AssertEquals(true, InvoicingPluginToFreight.IsInvoiceDeletionAllowedByParent(null));

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			jobHeaderParent.AllowInvoiceDeletion = true;
			AssertEquals(true, InvoicingPluginToFreight.IsInvoiceDeletionAllowedByParent(jobHeaderParent));

			jobHeaderParent.AllowInvoiceDeletion = false;
			AssertEquals(false, InvoicingPluginToFreight.IsInvoiceDeletionAllowedByParent(jobHeaderParent));
		}

		JobHeader CreateJobHeader(IJobHeaderParent jobHeaderParent)
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = jobHeaderParent.PK;
			jobHeader.JH_ParentTableCode = ((BusinessObject)jobHeaderParent).TablePrefix;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			jobHeader.Parent = jobHeaderParent;

			return jobHeader;
		}

		#region Mark Invoicing Job as Isactive

		public void TestMarkJobHeaderAsInactiveForJobNotInDatabase()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			using (var plugIn = new InvoicingPluginForTest(shipment))
			{
				plugIn.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals("Should ask user to confirm invoice mark as inactive. No errors should be found.", "Invoicing job has not been created", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemMarkInvoicingJobAsInactive_Click_ForbiddenByParent()
		{
			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeader = CreateJobHeader(jobHeaderParent);
			((DummyJobHeaderParentJobInvoicingSupporter)jobHeaderParent.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Shipment;
			Factory.Save();

			jobHeaderParent.AllowInvoiceDeletion = true;
			using (var plugIn = new InvoicingPluginForTest(jobHeaderParent))
			{
				plugIn.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals("Should ask user to confirm invoice deactivation. No errors should be found.", "Please type 'YES' to confirm deactivation of job.\r\nOn saving, the job will be deactivated and the form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			jobHeaderParent.AllowInvoiceDeletion = false;
			using (var plugIn = new InvoicingPluginForTest(jobHeaderParent))
			{
				plugIn.Job_ForTestOnly = (Job)jobHeader;
				plugIn.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals("Error Message should be shown.", "This invoice cannot be marked as inactive.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadChargesForDeactivatedJobWhenDisposeInvoicingPluginToFreighting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S00001000", true);
			var job = testObjectCreator.CreateJob(shipment, false, false);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newshipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			using (var plugin = new InvoicingPluginToFreight(newshipment))
			{
				AssertNotNull(plugin.Job_ForTestOnly);

				job.MarkAsInactive();
				Factory.Save();
			}
		}

		#endregion

		#region OnGUIShown

		public void TestOnGUIShownSetsDefaultDepartment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();
				Assert("Department on Job_ForTestOnly should not be set", plugin.Job_ForTestOnly.JH_GE.IsEmpty);
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUBNE";
				plugin.OnGUIShown();
				Assert("Department on Job_ForTestOnly should be set", !plugin.Job_ForTestOnly.JH_GE.IsEmpty);
			}
		}

		public void TestOnGUIShown_SetsClosedJobReopener_WhenNewJob()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			AssertNull(shipment.Job);

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();
				Factory.Save();

				var closedJobReopener = ((Job)shipment.Job).ClosedJobReopener;
				AssertNotNull("Job ClosedJobReopener", closedJobReopener);
				AssertType<InvoicingPluginToFreightReopenClosedInternalJobDataProvider>("ReOpenClosedJobDataProvider Type", ((ClosedJobReopener)closedJobReopener).ReOpenClosedJobDataProvider);
			}
		}

		public void TestOnGUIShown_SetsClosedJobReopener_WhenExistingJob()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job = TestObjectCreator.CreateJob(shipment);
			AssertNotNull(shipment.Job);

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				var closedJobReopener = ((Job)shipment.Job).ClosedJobReopener;
				AssertNotNull("Job ClosedJobReopener", closedJobReopener);
				AssertType<InvoicingPluginToFreightReopenClosedInternalJobDataProvider>("ReOpenClosedJobDataProvider Type", ((ClosedJobReopener)closedJobReopener).ReOpenClosedJobDataProvider);
			}
		}

		#endregion

		#region Security

		public void TestSecurityPostAll()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostAll).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostAll);

				plugin.MenuItemPostAll_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostCost()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostCost).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostCost);

				plugin.MenuItemPostCost_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReverseBillingNoInvoiceFoundError()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			string expectedError;
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();
				expectedError = @"No appropriate invoices were found for reversing.";
				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityReverseBilling()
		{
			string expectedError;
			UnitTestUserNotification.Instance.ClearMessages();

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice arInvoice1;
			APInvoice apInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out arInvoice1, out apInvoice1);
			arInvoice1.AH_TransactionCategory = "FIN";

			ARInvoice arInvoice2;
			APInvoice apInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out arInvoice2, out apInvoice2);
			arInvoice2.AH_TransactionCategory = "SBR";

			Factory.Save();

			Assert("Precondition: Standard Invoice", !arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self Billing Invoice", arInvoice2.IsSelfBillingInvoice);
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.ReverseStandardInvoiceSecurity_ForTestOnly.IsAllowed = false;
				plugin.ReverseSelfBilledInvoiceSecurity_ForTestOnly.IsAllowed = true;
				expectedError = @"There are mix of standard and self billed invoices on this job.
You do not have the appropriate security rights to reverse standard invoices.

If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Billings and Re-do Invoice

Alternatively, you may manually reverse the self-billed invoices.
";
				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				plugin.ReverseStandardInvoiceSecurity_ForTestOnly.IsAllowed = true;
				plugin.ReverseSelfBilledInvoiceSecurity_ForTestOnly.IsAllowed = false;
				expectedError = @"There are mix of standard and self billed invoices on this job.
You do not have the appropriate security rights to reverse self-billed invoices.

If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Self-Billings and Re-do Invoice

Alternatively, you may manually reverse the standard invoices.
";
				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityReverseBillingWhenARInvoiceHasAPTrnasactionPaid()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			CommonShipment shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice aRInvoice1;
			APInvoice aPInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out aRInvoice1, out aPInvoice1);
			APPayment aPPayment1 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1);

			ARInvoice aRInvoice2;
			APInvoice aPInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out aRInvoice2, out aPInvoice2);
			APPayment aPPayment2 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2);

			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.AllowReversalWhenRelatedAPTrArePaid);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				int loginFormShownCount = 0;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is LoginForm)
					{
						loginFormShownCount++;
					}
				});
				try
				{
					plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
					AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		public void TestSecurityReverseSelfBilingBillingWhenARInvoiceHasAPTrnasactionPaid()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice arInvoice1;
			APInvoice aPInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out arInvoice1, out aPInvoice1);
			arInvoice1.AH_TransactionCategory = "SBR";
			var payment1 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1);

			ARInvoice arInvoice2;
			APInvoice aPInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out arInvoice2, out aPInvoice2);
			arInvoice2.AH_TransactionCategory = "SBR";
			var payment2 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2);

			Factory.Save();

			Assert("Precondition: Self Billing Invoice", arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self Billing Invoice", arInvoice2.IsSelfBillingInvoice);
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;
				var expectedError = plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.ErrorMessageForNotAllowed;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				int loginFormShownCount = 0;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is LoginForm)
					{
						loginFormShownCount++;
					}
				});
				try
				{
					plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
					AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		public void TestSecurityReverseBillingWhenARInvoiceHasAPTrnasactionPaid_LogInOnce()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			CommonShipment shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice aRInvoice1;
			APInvoice aPInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out aRInvoice1, out aPInvoice1);
			APPayment aPPayment1 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1);

			ARInvoice aRInvoice2;
			APInvoice aPInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out aRInvoice2, out aPInvoice2);
			APPayment aPPayment2 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2);

			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).IsAllowed = false;
				SecurityTestObject.CreateTestUser(true, plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).Code, "US1", "User1", "pass");

				int loginFormShownCount = 0;
				TransactionReasonForm reasonForm = null;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					if (loginForm != null)
					{
						loginFormShownCount++;

						loginForm.DoLoginForTest("User1", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
					else if (reasonForm == null)
					{
						reasonForm = form as TransactionReasonForm;
					}
				});
				try
				{
					plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
					AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
					AssertNotNull("A reason form should be shown to proceed with reversal", reasonForm);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		public void TestSecurityReverseSelfBillingWhenARInvoiceHasAPTrnasactionPaid_LogInOnce()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice arInvoice1;
			APInvoice apInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out arInvoice1, out apInvoice1);
			arInvoice1.AH_TransactionCategory = "SBR";
			var payment1 = creator.CreateAndMatchAPPaymentForAPInvoice(apInvoice1);

			ARInvoice arInvoice2;
			APInvoice apInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out arInvoice2, out apInvoice2);
			arInvoice2.AH_TransactionCategory = "SBR";
			var payment2 = creator.CreateAndMatchAPPaymentForAPInvoice(apInvoice2);

			Factory.Save();

			Assert("Precondition: Self Billing Invoice", arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self Billing Invoice", arInvoice2.IsSelfBillingInvoice);
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;
				SecurityTestObject.CreateTestUser(true, plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.Code, "US1", "User1", "pass");

				int loginFormShownCount = 0;
				TransactionReasonForm reasonForm = null;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					if (loginForm != null)
					{
						loginFormShownCount++;

						loginForm.DoLoginForTest("User1", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
					else if (reasonForm == null)
					{
						reasonForm = form as TransactionReasonForm;
					}
				});
				try
				{
					plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
					AssertEquals("A Login Form should be shown once", 1, loginFormShownCount);
					AssertNotNull("A reason form should be shown to proceed with reversal", reasonForm);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		public void TestSecurityReverseBothTypeInvoiceWhenARInvoiceHasAPTrnasactionPaid()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice arInvoice1;
			APInvoice aPInvoice1;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out arInvoice1, out aPInvoice1);
			arInvoice1.AH_TransactionCategory = "FIN";
			var payment1 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1);

			ARInvoice arInvoice2;
			APInvoice aPInvoice2;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC2, out arInvoice2, out aPInvoice2);
			arInvoice2.AH_TransactionCategory = "SBR";
			var payment2 = creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2);

			Factory.Save();

			Assert("Precondition: Standard Invoice", !arInvoice1.IsSelfBillingInvoice);
			Assert("Precondition: Self Billing Invoice", arInvoice2.IsSelfBillingInvoice);
			string expectedError;
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.ReverseStandardInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;
				plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;
				expectedError = @"There are mix of standard and self billed invoices on this job.
You do not have the appropriate security rights to reverse standard and self-billed invoices.

If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Billings and Re-do Invoice -> Allow Reversal when Related AP Transactions are Paid
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Self-Billings and Re-do Invoice -> Allow Reversal when Related AP Transactions are Paid
";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.ReverseStandardInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;
				plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = true;

				expectedError = @"There are mix of standard and self billed invoices on this job.
You do not have the appropriate security rights to reverse standard invoices.

If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Billings and Re-do Invoice -> Allow Reversal when Related AP Transactions are Paid

Alternatively, you may manually reverse the self-billed invoices.
";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.ReverseStandardInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = true;
				plugin.ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly.IsAllowed = false;

				expectedError = @"There are mix of standard and self billed invoices on this job.
You do not have the appropriate security rights to reverse self-billed invoices.

If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Reverse Self-Billings and Re-do Invoice -> Allow Reversal when Related AP Transactions are Paid

Alternatively, you may manually reverse the standard invoices.
";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityReverseBillingWhenARInvoiceHasAPTrnasactionUnPaid()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			CommonShipment shipment = creator.CreateAndSaveTestForwardingShipmentJob("WRK");

			ARInvoice aRInvoice;
			APInvoice aPInvoice;
			creator.CreateRelatedARAndAPInvoices(shipment.Job.PK, creator.CC1, out aRInvoice, out aPInvoice);

			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).IsAllowed = false;
				LoginForm loginForm = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (loginForm == null)
					{
						loginForm = form as LoginForm;
					}
				});
				try
				{
					plugin.MenuItemReverseInvoices_Click_ForTestOnly(plugin, new EventArgs());
					AssertNull("No Login Form should be shown", loginForm);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		public void TestSecurityRequestCashAdvance()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.RequestCashAdvance).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.RequestCashAdvance);

				plugin.MenuItemRequestCashAdvance_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostRevenue()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostRevenue).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostRevenue);

				plugin.MenuItemPostBoth_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityMarkInvoicingJobAsInactive()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.DeleteJob).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.DeleteJob);

				plugin.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityRevenueRecognitionDate()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.RecognizeRevenue).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.RecognizeRevenue);

				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostLocalClient()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostLocalClient).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostLocalClient);

				plugin.MenuItemPostBillTo_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostOverseas()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostOverseas).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostOverseas);

				plugin.MenuItemPostAgent_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostDisbursement()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostDSB).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostDSB);

				plugin.MenuItemPostDisbursement_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostAllSisterCompanyCharges()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostAllSisterCompanyCharges).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostAllSisterCompanyCharges);

				plugin.MenuItemPostAllSisterCompanyCharges_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityPostLocalSisterCompanyCharges()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PostLocalSisterCompanyChargesOnly).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.PostLocalSisterCompanyChargesOnly);

				plugin.MenuItemPostLocalSisterCompanyChargesOnly_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityResetUnpostedLines()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ResetUnpostedLine).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ResetUnpostedLine);

				plugin.MenuItemRecalculateInvoiceType_Click_ForTestOnly(plugin, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityAutoRating_Shipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				string expectedRevenueError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.AutoRateRevenue);
				string expectedCostError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.AutoRateCost);

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AutoRateRevenue).IsAllowed = false;
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AutoRateCost).IsAllowed = false;

				plugin.HandleAutoRateMenu_ForTestOnly(InvoicingPluginToFreight.AutoRateMenuAction.AutorateCosts | InvoicingPluginToFreight.AutoRateMenuAction.AutorateRevenue);

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedRevenueError.Replace("\r\n\r\n", "\r\n\t")));

				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.AutoRateRevenue).IsAllowed = true;

				plugin.HandleAutoRateMenu_ForTestOnly(InvoicingPluginToFreight.AutoRateMenuAction.AutorateCosts | InvoicingPluginToFreight.AutoRateMenuAction.AutorateRevenue);

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedCostError.Replace("\r\n\r\n", "\r\n\t")));
			}
		}

		public void TestSecurityARJobInvoicingPrintingPanel_True()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ARInvoices).IsAllowed = true;
				plugin.OnGUIShown();
				AssertEquals("Security Panel should not be shown", false, ((JobInvoicingUserControl)plugin.UserControl).ARInvoicingPrintingSecurityLabel.Visible);
			}
		}

		public void TestSecurityARJobInvoicingPrintingPanel_False()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ARInvoices).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ARInvoices);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", expectedError, ((JobInvoicingUserControl)plugin.UserControl).ARInvoicingPrintingSecurityLabel.Text);
			}
		}

		public void TestSecurityAPJobInvoicingPrintingPanel_True()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.APInvoices).IsAllowed = true;
				plugin.OnGUIShown();
				AssertEquals("Security Panel should not be shown", false, ((JobInvoicingUserControl)plugin.UserControl).APInvoicingPrintingSecurityLabel.Visible);
			}
		}

		public void TestSecurityAPJobInvoicingPrintingPanel_False()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.APInvoices).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.APInvoices);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", expectedError, ((JobInvoicingUserControl)plugin.UserControl).APInvoicingPrintingSecurityLabel.Text);
			}
		}

		public void TestSecurityProfitAndLossPanel_True()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ProfitLoss).IsAllowed = true;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ProfitLoss);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", false, ((JobInvoicingUserControl)plugin.UserControl).ProfitLossSecurityLabel.Visible);
			}
		}

		public void TestSecurityProfitAndLossPanel_False()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ProfitLoss).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ProfitLoss);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", expectedError, ((JobInvoicingUserControl)plugin.UserControl).ProfitLossSecurityLabel.Text);
			}
		}

		public void TestSecurityInvoicingPanel_True()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.Invoicing).IsAllowed = true;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.Invoicing);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", false, ((JobInvoicingUserControl)plugin.UserControl).InvoicingSecurityLabel.Visible);
			}
		}

		public void TestSecurityJobInvoicingPanel_False()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.Invoicing).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.Invoicing);

				plugin.OnGUIShown();

				AssertEquals("Security Panel should not be shown", expectedError, ((JobInvoicingUserControl)plugin.UserControl).InvoicingSecurityLabel.Text);
			}
		}

		public void TestSecurityInvoicingEntry_False()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.InvoicingEntry).IsAllowed = false;

				plugin.OnGUIShown();

				var jobChargeUserControl = ((JobInvoicingUserControl)plugin.UserControl).JobChargeUserControl;
				AssertEquals("JobChargeUserControl should be enabled", true, jobChargeUserControl.Enabled);
				AssertEquals("JobChargeUserControl.JobChargeBoundGrid should be enabled", true, jobChargeUserControl.JobChargeBoundGrid.Enabled);
				AssertEquals("JobChargeUserControl.JobChargeBoundGrid should be readonly", true, jobChargeUserControl.JobChargeBoundGrid.ReadOnly);
			}
		}

		#endregion

		#region Test Show message correct when click MenuItem for job status is set to 'JFC'
		public void TestMenuItem_ClickForJobStatusIsJFC()
		{
			var shipment = TestObjectCreator.CreateShipment("S001991");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			var oldValueForModify = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var oldValueForPost = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = oldValueForModify))
			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = oldValueForPost))
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.MenuItemDeleteUnPosted_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot delete unposted lines from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemRecalculateInvoiceType_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot reset invoice types of unposted lines from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemResetDefaultDebtorOnUnpostedLines_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot reset default debtor on unposted lines from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot reset reverse invoices and redo billing from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot import AP invoices issued by other group companies from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemResetTaxDefaults_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot reset tax defaults of unposted lines from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemProfitShare_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot create profit share charges from this job, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemRedefaultExRate_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot re-default Job Billing Exchange Rate, because the invoicing Job has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugin.MenuItemGroupCompanyCharges_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Cannot do group companies charges, because it has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Test RecognizeRevenueMenuItem

		public void TestRecognizeRevenueMenuItemName()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.Mode = Core.Constants.TransportModes.Air;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				setting.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);

				valuesForTest.RemoveAndDeleteAll();
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				AssertRecognizeRevenueMenuItemName(plugin);
			}
		}

		public void TestProfitShareMenuItemWhenIncorrectRegistrySetting_ProfitShareChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AssertProfitShareMenuItemWhenIncorrectRegistrySetting(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Caption);
		}

		public void TestProfitShareMenuItemWhenIncorrectRegistrySetting_ProfitShareChargeCodesPerParty()
		{
			var lookups = new OrgProfitSharePartyLookups(null);
			var collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			var chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = chargeCode.PK;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertProfitShareMenuItemWhenIncorrectRegistrySetting(AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Caption);
		}

		void AssertProfitShareMenuItemWhenIncorrectRegistrySetting(string registryName)
		{
			var shipment = TestObjectCreator.CreateShipment("S001991");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.MenuItemProfitShare_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals($"Incorrect Registry Item value.\r\nPlease set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> {registryName}'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProfitShareMenuItemWhenDifferentCompanyRegistrySetting_ProfitShareChargeCode()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			AssertProfitShareMenuItemWhenDifferentCompanyRegistrySetting(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Caption);
		}

		public void TestProfitShareMenuItemWhenDifferentCompanyRegistrySetting_ProfitShareChargeCodesPerParty()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();

			var lookups = new OrgProfitSharePartyLookups(null);
			var collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = chargeCode.PK;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertProfitShareMenuItemWhenDifferentCompanyRegistrySetting(AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Caption);
		}

		void AssertProfitShareMenuItemWhenDifferentCompanyRegistrySetting(string registryName)
		{
			var shipment = TestObjectCreator.CreateShipment("S001991");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.MenuItemProfitShare_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals($"Incorrect Registry Item value.\r\nPlease set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> {registryName}'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProfitShareSavingHandlesSaveException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(Shipment))
			{
				try
				{
					Factory.Saving += Factory_Saving;
					plugin.MenuItemProfitShare_Click_ForTestOnly(null, EventArgs.Empty);
					AssertContains("Critical Validation Error should be shown", UnitTestUserNotification.Instance.LastMessage.Text, "Unable to Save:");
				}
				catch (OnSavingCriticalCheckException)
				{
					Fail("Critical Validation should catch error");
				}
				finally
				{
					Factory.Saving -= Factory_Saving;
				}
			}
		}

		#region MenuItemProfitShare_Click Test Implementation

		void CreateProfitShareDetails(string rateBasis)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(TestObjectCreator.Agent);
			consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Creditor1);
			Shipment = consol.Shipments.AddNew();
			Shipment.JS_UniqueConsignRef = "JobNumber";

			ProfitShareDetails = new ProfitShareDetailCollection();

			CreateProfitShareProfile(TestObjectCreator.Agent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, rateBasis, ProfitShareDetails, Shipment);

			Job = TestObjectCreator.CreateJob(Shipment, false);
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = Shipment;
			Factory.Save();

			JobCharge = Job.Charges.AddNew();
			JobCharge.JR_AC = Env.Registry.FreightChargeCode;
			JobCharge.JR_IsIncludedInProfitShare = true;
			JobCharge.JR_LocalSellAmt = 600m;
			JobCharge.JR_LocalCostAmt = 100m;
		}

		void CreateProfitShareProfile(OrgHeader agent, string role, decimal percent, string rateBasis, ProfitShareDetailCollection profitShares, ForwardingShipment shipment)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percent;
			party.PS_PartyRateBasis = rateBasis;

			ProfitShareDetail detail = profitShares.AddNew();
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, agent, role, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			throw new OnSavingCriticalCheckException<JobCharge>(JobCharge, CriticalValidationErrorType.DummyErrorKeyForTest, "Unable to Save:", "E=MC2");
		}

		#endregion

		public void TestRecognizeRevenueMenuItemErrorMessages()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				JobChargeRevRecognition revRecog = TestObjectCreator.CreateJobChargeRevRecognition(job, "JOB", AccountingConstants.RevenueRecognitionDateConstants.Immediate);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);

				revRecog.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.JobClosure;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);

				revRecog.D3_RecognitionDate = job.JH_A_JOP;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);

				revRecog.D3_RecognitionDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRecognizeRevenueMenuItemErrorMessagesWithPostDateOfFirstARTransactionSetting()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				TestObjectCreator.CreateJobChargeRevRecognition(job, "JOB", AccountingConstants.RevenueRecognitionDateConstants.PostDateOfFirstARTransaction);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRecognizeRevenueMenuItemBehaviour()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			ZDateTime expectedDate = ZDateTime.Now.AddMonths(-2);
			TestObjectCreator.CreateTestPeriods(expectedDate.Date);
			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message should be shown", "Please save this Shipment before revenue recognition.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message should be shown", "The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Pickup Date'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, job.RevenueRecognitionCollection.Count);
				AssertNull(charge.Accrual);
				AssertNull(charge.WIP);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals("Message should be shown", "The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Pickup Date'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, job.RevenueRecognitionCollection.Count);
				AssertNull(charge.Accrual);
				AssertNull(charge.WIP);

				shipment.DocsAndCartage.JP_EstimatedPickup = expectedDate;
				job.JH_OA_LocalChargesAddr = ZGuid.Empty;
				Factory.Save();
				AssertEquals("Precondition: job.HasErrors", true, job.HasErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message should be shown", "After revenue recognition the job contains errors and can't be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, job.RevenueRecognitionCollection.Count);
				AssertNull(charge.Accrual);
				AssertNull(charge.WIP);
				AssertEquals("Job should not be saved after revenue recognition.", true, job.HasChanges);

				job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				AssertNoErrors("Precondition", job);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedDate, job.GetRevenueRecognitionDate(charge.CostRecognition));
				AssertEquals(expectedDate, job.GetRevenueRecognitionDate(charge.SellRecognition));
				AssertNotNull(charge.Accrual);
				AssertNotNull(charge.WIP);
				AssertEquals("Job should be saved after revenue recognition.", false, job.HasChanges);
			}
		}

		public void TestRecognizeRevenueMenuItemBehaviourForDateBeforeFirstPeriod()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			ZDateTime expectadeDate = ZDateTime.Now.AddMonths(-2);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				shipment.DocsAndCartage.JP_EstimatedPickup = expectadeDate;
				Factory.Save();

				AssertNoErrors("Precondition", job);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Message should be shown", "The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Pickup Date'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, job.RevenueRecognitionCollection.Count);
				AssertNull(charge.Accrual);
				AssertNull(charge.WIP);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertContains("cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.MinSmallDateTimeValue, job.GetRevenueRecognitionDate(charge.CostRecognition));
				AssertEquals(ZDateTime.MinSmallDateTimeValue, job.GetRevenueRecognitionDate(charge.SellRecognition));
				Factory.Save();
				AssertNotNull(charge.Accrual);
				AssertNotNull(charge.WIP);
			}
		}

		void AssertRecognizeRevenueMenuItemName(InvoicingPluginToFreight plugin)
		{
			plugin.MainMenuItem_ForTestOnly = null;
			MenuItem recognizeRevenueMenuItem = null;

			foreach (MenuItem menuItem in plugin.GetNewTopLevelMenu_ForTestOnly().MenuItems)
			{
				if (menuItem.Text.StartsWith("Recognize Revenue"))
				{
					recognizeRevenueMenuItem = menuItem;
				}
			}
			AssertNotNull(recognizeRevenueMenuItem);

			AssertEquals("Recognize Revenue", recognizeRevenueMenuItem.Text);
		}

		[TestDate(2011, 05, 04)]
		public void TestRecognizeRevenueMenuItemWarnings()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var revenueRecognitionConfig = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revenueRecognitionConfig.RemoveAll();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revenueRecognitionConfig);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Now.AddYears(3);

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 200M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 200M, null, TestObjectCreator.AUD, 200M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 300M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);
			charge.JR_AL_APLine = line.PK;

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 400M, null, TestObjectCreator.AUD, 400M, null);
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);
			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				AssertEquals("Message should be shown",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 04-May-14 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.
You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.
The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Delivery Date', 'Pickup Date'.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2011, 05, 04)]
		public void TestMenuItemRecognizeRevenueUpdateReverseDateForREVandCSTLines()
		{
			string sql = @"IF OBJECTPROPERTY(OBJECT_ID('TG_AccTransactionLines_InsertToJobCostingDataQueue'), 'IsTrigger') = 1
					DISABLE TRIGGER TG_AccTransactionLines_InsertToJobCostingDataQueue ON AccTransactionLines;

					IF OBJECTPROPERTY(OBJECT_ID('RptDt_TG_AccTransactionLines_InsertToReversedLinesTable'), 'IsTrigger') = 1
						DISABLE TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable ON AccTransactionLines;";

			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery(sql);

			var overseasAgent = TestObjectCreator.CreateOrgHeader("OVAGNT", false, true);
			var overseasAgent2 = TestObjectCreator.CreateOrgHeader("OVAORG", false, true);
			var address1 = TestObjectCreator.CreateAddress(overseasAgent, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(overseasAgent2, "2nd Street");
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0002");
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			job.JH_OA_AgentCollectAddr = address1.PK;
			job.JH_OA_LocalChargesAddr = address2.PK;
			job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			JobChargeRevRecognition revRec = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRec.D3_JH = job.PK;
			revRec.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			revRec.D3_RecognitionDate = ZDateTime.BrettsBirthday.AddMonths(10);

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M);
			apInvoice.AH_PostDate = ZDateTime.Now.AddDays(-10);

			var apLine = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLine.AL_ReverseDate = ZDateTime.Empty;

			TestObjectCreator.CreateCharge(apLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			ZDateTime expectedDate = ZDateTime.Now.AddMonths(-2);
			TestObjectCreator.CreateTestPeriods(expectedDate.Date);
			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			shipment.JS_E_DEP = expectedDate;
			Factory.Save();

			var factoryObserver = new BusinessObjectFactory();
			factoryObserver.RefreshEnabled = false;
			var apLineObserver = factoryObserver.Load<AccTransactionLines>(apLine.PK);
			Assert("JAL_ReverseDate should be updated after Factory.Save()", apLineObserver.AL_ReverseDate != ZDateTime.Empty);

			//AL_ReverseDate updated by 3rd party
			var command = CargoWise.Data.Db.Connection.Command("UPDATE dbo.AccTransactionLines SET AL_ReverseDate = null, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @LinePK"); // To avoid critical validation errors
			command.AddParameter("@LinePK", System.Data.SqlDbType.UniqueIdentifier, apLine.PK.ToGuid());
			command.ExecuteNonQuery();

			apLineObserver.Reload();
			Assert("JAL_ReverseDate should be changed to empty value", apLineObserver.AL_ReverseDate == ZDateTime.Empty);

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			shipment = factory3.Load<ForwardingShipment>(shipment.PK);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				apLineObserver.Reload();
				Assert("JAL_ReverseDate not updated yet", apLineObserver.AL_ReverseDate == ZDateTime.Empty);

				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);

				apLineObserver.Reload();
				Assert("JAL_ReverseDate should be updated", apLineObserver.AL_ReverseDate != ZDateTime.Empty);
			}
		}

		public void TestJob_OnCloseJobYesNoQuestionForCompletingJob()
		{
			AssertJob_OnCloseJobYesNoQuestion(JobHeaderStatus.Complete.Code);
		}

		public void TestJob_OnCloseJobYesNoQuestionForClosingJob()
		{
			AssertJob_OnCloseJobYesNoQuestion(JobHeaderStatus.Closed.Code);
		}

		void AssertJob_OnCloseJobYesNoQuestion(string jobStatus)
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var line = invoice.Lines[0];
			line.AL_JH = job.PK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_OSAmount = line.AL_LineAmount = -charge.JR_OSCostAmt;
			Factory.Save();
			AssertEquals("Precondition: line shouldn't be recognized.", ZDateTime.Empty, line.AL_ReverseDate);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				job.JH_Status = jobStatus;

				ZString expectedText = "contains unrecognized revenue and cannot be closed or completed.\r\nPlease recognize revenue first and try again.";
				AssertContains("Error should be shown", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("testJob.JH_Status", jobStatus, job.JH_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				job.JH_Status = jobStatus;

				expectedText = "contains unrecognized revenue and cannot be closed or completed.\r\nDo you want to recognize revenue with Today's date?";
				AssertNotNull("Question should be shown", UnitTestUserNotification.Instance.PreviousMessages.ToList().FirstOrDefault(x => x.Contains(expectedText)));
				expectedText = string.Format(@"Setting '{0}' status on the job {1} results in recognizing its revenue.
You would not be able to change this job status until you save or cancel the changes.", jobStatus, job.JH_JobNum);
				AssertHasWarning(job.JH_StatusInfo, expectedText);
				AssertEquals("testJob.JH_Status", jobStatus, job.JH_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				job.JH_Status = JobHeaderStatus.Working.Code;
				expectedText = string.Format(@"You have changed the status of this job to '{0}' before saving. This will trigger revenue recognition.
If you did not intend to recognize revenue, please close the job without saving.
If you intended to recognize revenue, please change the status back to '{0}' and save.", jobStatus);
				AssertContains("Confirmation of changing status should be shown the last", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("testJob.JH_Status was not changed", jobStatus, job.JH_Status);
			}
		}

		[TestDate(2011, 05, 04)]
		public void TestRecognizeRevenueMenuItem_FixRelatedLinesWithEmptyRecognitionType()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			JobChargeRevRecognition revRec = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRec.D3_JH = job.PK;
			revRec.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			revRec.D3_RecognitionDate = ZDateTime.BrettsBirthday.AddMonths(10);

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M);
			apInvoice.AH_PostDate = ZDateTime.Now.AddDays(-10);
			var apLineWithTypeIMM = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLineWithTypeIMM.AL_ReverseDate = ZDateTime.Now.AddDays(-10);
			var apLineWithTypeFromD3Table = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLineWithTypeFromD3Table.AL_ReverseDate = revRec.D3_RecognitionDate;
			var apLineWithTypeFromRegistry_Unrecognized = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLineWithTypeFromRegistry_Unrecognized.AL_ReverseDate = ZDateTime.Empty;
			var apLineWithTypeFromRegistry_Recognized = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLineWithTypeFromRegistry_Recognized.AL_ReverseDate = ZDateTime.BrettsBirthday;
			var apLineWithCorrectType = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 10M);
			apLineWithCorrectType.AL_ReverseDate = ZDateTime.Empty;
			apLineWithCorrectType.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;

			TestObjectCreator.CreateCharge(apLineWithTypeIMM, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateCharge(apLineWithTypeFromD3Table, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateCharge(apLineWithTypeFromRegistry_Unrecognized, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateCharge(apLineWithTypeFromRegistry_Recognized, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateCharge(apLineWithCorrectType, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			ZDateTime expectedDate = ZDateTime.Now.AddMonths(-2);
			TestObjectCreator.CreateTestPeriods(expectedDate.Date);
			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			Factory.Save();

			shipment.JS_E_DEP = expectedDate;
			Factory.Save();

			var command = CargoWise.Data.Db.Connection.Command("UPDATE dbo.AccTransactionLines SET AL_RevRecognitionType = '', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @LinePK"); // To avoid critical validation errors
			command.AddParameter("@LinePK", System.Data.SqlDbType.UniqueIdentifier, apLineWithTypeIMM.PK.ToGuid());
			command.ExecuteNonQuery();
			command.SetParameterValue("@LinePK", apLineWithTypeFromD3Table.PK.ToGuid());
			command.ExecuteNonQuery();
			command.SetParameterValue("@LinePK", apLineWithTypeFromRegistry_Unrecognized.PK.ToGuid());
			command.ExecuteNonQuery();
			command.SetParameterValue("@LinePK", apLineWithTypeFromRegistry_Recognized.PK.ToGuid());
			command.ExecuteNonQuery();

			apLineWithTypeIMM.Reload();
			apLineWithTypeFromD3Table.Reload();
			apLineWithTypeFromRegistry_Unrecognized.Reload();
			apLineWithTypeFromRegistry_Recognized.Reload();
			AssertEquals("Precondition: AL_RevRecognitionType should be empty", "", apLineWithTypeIMM.AL_RevRecognitionType);
			AssertEquals("Precondition: AL_RevRecognitionType should be empty", "", apLineWithTypeFromD3Table.AL_RevRecognitionType);
			AssertEquals("Precondition: AL_RevRecognitionType should be empty", "", apLineWithTypeFromRegistry_Unrecognized.AL_RevRecognitionType);
			AssertEquals("Precondition: AL_RevRecognitionType should be empty", "", apLineWithTypeFromRegistry_Recognized.AL_RevRecognitionType);
			AssertEquals("Precondition: AL_RevRecognitionType should be empty", RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, apLineWithCorrectType.AL_RevRecognitionType);

			AssertEquals("Precondition: job.RevenueRecognitionCollection.Count", 1, job.RevenueRecognitionCollection.Count);
			AssertEquals("Precondition: apLineWithTypeIMM.AL_ReverseDate", ZDateTime.Now.AddDays(-10), apLineWithTypeIMM.AL_ReverseDate);
			AssertEquals("Precondition: apLineWithTypeIMM.AL_PostDate", ZDateTime.Now.AddDays(-10), apLineWithTypeIMM.AL_PostDate);
			AssertNotEquals("Precondition: apLineWithTypeFromD3Table.AL_PostDate", apLineWithTypeFromD3Table.AL_ReverseDate, apLineWithTypeFromD3Table.AL_PostDate);
			AssertEquals("Precondition: apLineWithTypeFromD3Table.AL_ReverseDate", revRec.D3_RecognitionDate, apLineWithTypeFromD3Table.AL_ReverseDate);
			AssertEquals("Precondition: apLineWithTypeFromRegistry_Unrecognized.AL_ReverseDate", ZDateTime.Empty, apLineWithTypeFromRegistry_Unrecognized.AL_ReverseDate);
			AssertNotEquals("Precondition: apLineWithTypeFromRegistry_Recognized.AL_PostDate", apLineWithTypeFromRegistry_Recognized.AL_ReverseDate, apLineWithTypeFromRegistry_Recognized.AL_PostDate);
			AssertEquals("Precondition: apLineWithTypeFromRegistry_Recognized.AL_ReverseDate", ZDateTime.BrettsBirthday, apLineWithTypeFromRegistry_Recognized.AL_ReverseDate);
			AssertEquals("Precondition: apLineWithCorrectType.AL_ReverseDate", ZDateTime.Empty, apLineWithCorrectType.AL_ReverseDate);

			var unsavedBizo = Factory.New<ARInvoice>();

			job.JH_GS_NKRepSales = "XX";
			job.RunPreSaveValidation();
			AssertEquals("Precondition: job.HasErrors", true, job.HasErrors);

			using (InvoicingPluginToFreight plugin = new InvoicingPluginToFreight(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				AssertEquals(2, job.RevenueRecognitionCollection.Count);
				AssertEquals("Job factory was not saved.", false, unsavedBizo.IsInDatabase);

				var anotherFactory = new BusinessObjectFactory();
				string assertMessage = "AL_RevRecognitionType should be fixed and saved even if revenue recognition failed and job was not saved.";
				AssertEquals(assertMessage, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, anotherFactory.Load<TransactionLine>(apLineWithTypeIMM.PK).AL_RevRecognitionType);
				AssertEquals(assertMessage, RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, anotherFactory.Load<TransactionLine>(apLineWithTypeFromD3Table.PK).AL_RevRecognitionType);
				AssertEquals(assertMessage, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, anotherFactory.Load<TransactionLine>(apLineWithTypeFromRegistry_Unrecognized.PK).AL_RevRecognitionType);
				AssertEquals(assertMessage, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, anotherFactory.Load<TransactionLine>(apLineWithTypeFromRegistry_Recognized.PK).AL_RevRecognitionType);
				AssertEquals(assertMessage, RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, anotherFactory.Load<TransactionLine>(apLineWithCorrectType.PK).AL_RevRecognitionType);
			}
		}

		#endregion

		public void TestJobSetDefaultsOnPluginTabEnter()
		{
			GlbBranch otherBranch = TestObjectCreator.CreateNewBranch(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK), "AB2");
			TestObjectCreator.AALSHI.CompanyData.OB_GB_ControllingBranch = otherBranch.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			JobBranchDefaultOrderRule branchOrderRule = new JobBranchDefaultOrderRule();
			branchOrderRule.DefaultToBlank = 0;
			branchOrderRule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			branchOrderRule.DefaultToBranchOfOrganisation = 1;
			branchOrderRule.DefaultToLoginUserDefault = 2;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branchOrderRule);

			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);

			using (ZForm form = new ZForm(shipment))
			{
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				InvoicingPluginToFreight invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				AssertEquals("Initial Job_ForTestOnly Branch", invoicingPlugin.Job_ForTestOnly.JH_GB, GlbBranch.CurrentBranch.PK);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.ZECTRA.PK;

				invoicingPlugin.OnGUIShown();
				AssertEquals("Job_ForTestOnly Branch should be updated", invoicingPlugin.Job_ForTestOnly.JH_GB, otherBranch.PK);
			}
		}

		#region Test PostedStateChanged

		public void TestPostedStateChanged()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USNYC";
			GlbDepartment fEA = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));

			CreateEmptyJobForShipment(shipment.PK);
			DummyJobInvoicingBusinessObject testBusinessObject = new DummyJobInvoicingBusinessObject(Factory, shipment.PK, "JobShipment");

			Job testJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = fEA.PK;
			testJob.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;
			AssertNoErrors(testJob);

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			charge.JR_GB = testJob.JH_GB;
			charge.JR_GE = testJob.JH_GE;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_APInvoiceNum = "ABCD1234";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;

			charge.JR_APInvoiceNum = "ABCD1234";

			Factory.Save();

			using (InvoicingPluginForTest plugin = new InvoicingPluginForTest(testBusinessObject))
			{
				AssertNoErrors(testJob);
				testBusinessObject.PostedStateHasChanges = false;
				plugin.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("PostedState should be changed", true, testBusinessObject.PostedStateHasChanges);
				Factory.Save();

				// Need to post these manually so the next step will work.
				// MenuItemPostAll_Click doesn't post because testBusinessObject isn't in the db
				InvoicingPostManager postManager = new InvoicingPostManager(testJob);
				postManager.CreateTransactions(JobInvoicingPostingOption.All);
				Factory.Save();

				testBusinessObject.PostedStateHasChanges = false;
				plugin.MenuItemReverseInvoices_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("PostedState should be changed", true, testBusinessObject.PostedStateHasChanges);
				Factory.Save();

				testBusinessObject.PostedStateHasChanges = false;
				plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("PostedState should be changed", true, testBusinessObject.PostedStateHasChanges);
			}
		}

		#endregion

		#region Reset Tax Default

		public void TestMenuItemResetTaxDefaults()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(false);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(false);
			org2.CompanyData.OB_APWHTApplicable = false;
			Factory.Save();

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var testJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_GB_TaxBranch = ZGuid.Empty;

			var testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSSellAmt = 100m;

			var testCharge2 = testJob.Charges.AddNew();
			testCharge2.JR_OH_CostAccount = org2.PK;
			testCharge2.JR_AC = chargeCode.PK;
			testCharge2.JR_InvoiceType = "FIN";
			testCharge2.JR_OSCostAmt = 100m;
			testCharge2.JR_LocalCostAmt = 100m;

			AssertEquals(true, AccountingMasterFilesUtils.IsTaxBranchApplicable);

			AssertEquals(false, testCharge.ChargeCode.IsComment);
			Assert("Sell GST should be actual", testCharge.IsSellGSTRateActual);
			AssertEquals("Sell GST should be empty", ZGuid.Empty, testCharge.JR_AT_SellGSTRate);
			AssertEquals("Sell GST amount should be zero", 0M, testCharge.JR_Sell_LocalGSTAmount);
			AssertEquals(false, testCharge2.ChargeCode.IsComment);
			Assert("Cost GST should be actual", testCharge2.IsCostGSTRateActual);
			AssertEquals("Cost GST should be empty", ZGuid.Empty, testCharge2.JR_AT_CostGSTRate);
			AssertEquals("Sell GST amount should be zero", 0M, testCharge2.JR_Cost_LocalGSTAmount);
			Assert("Sell GST should not be applicable", !testCharge.IsSellGSTApplicable);
			AssertEquals("Sell TaxBranch should be empty", ZGuid.Empty, testCharge.JR_GB_SellTaxBranch);
			Assert("Cost GST should not be applicable", !testCharge2.IsCostGSTApplicable);
			AssertEquals("Cost TaxBranch should be empty", ZGuid.Empty, testCharge2.JR_GB_CostTaxBranch);
			AssertEquals("Job TaxBranch should be empty", ZGuid.Empty, testJob.JH_GB_TaxBranch);

			Factory.Save();

			org1.CompanyData.SetARTaxApplicable(true);
			org2.CompanyData.SetAPTaxApplicable(true);

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Assert("Sell GST should not be actual", !testCharge.IsSellGSTRateActual);
			Assert("Cost GST should not be actual", !testCharge2.IsCostGSTRateActual);
			Assert("Sell GST should be applicable", testCharge.IsSellGSTApplicable);
			Assert("Cost GST should be applicable", testCharge2.IsCostGSTApplicable);
			using (var pluginToFreight = new InvoicingPluginForTest(shipment))
			{
				pluginToFreight.MenuItemResetTaxDefaults_Click_ForTestOnly(null, EventArgs.Empty);

				Assert("Sell GST should be actual", testCharge.IsSellGSTRateActual);
				AssertEquals("Sell GST should not be empty", TestObjectCreator.GST1.PK, testCharge.JR_AT_SellGSTRate);
				AssertEquals("Sell GST amount should be 10", 10M, testCharge.JR_Sell_LocalGSTAmount);
				Assert("Cost GST should be actual", testCharge2.IsCostGSTRateActual);
				AssertEquals("Cost GST should not be empty", TestObjectCreator.GST1.PK, testCharge2.JR_AT_CostGSTRate);
				AssertEquals("Sell GST amount should not be zero", 10M, testCharge2.JR_Cost_LocalGSTAmount);
				Assert("Sell GST should be applicable", testCharge.IsSellGSTApplicable);
				AssertEquals("Sell TaxBranch should not be empty", GlbBranch.CurrentBranch.PK, testCharge.JR_GB_SellTaxBranch);
				Assert("Cost GST should be applicable", testCharge2.IsCostGSTApplicable);
				AssertEquals("Cost TaxBranch should not be empty", GlbBranch.CurrentBranch.PK, testCharge2.JR_GB_CostTaxBranch);
				AssertEquals("Job TaxBranch should not be empty", GlbBranch.CurrentBranch.PK, testJob.JH_GB_TaxBranch);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Assert("Sell GST should not be actual", !testCharge.IsSellGSTRateActual);
			Assert("Cost GST should not be actual", !testCharge2.IsCostGSTRateActual);
			Assert("Sell GST should not be applicable", !testCharge.IsSellGSTApplicable);
			Assert("Cost GST should not be applicable", !testCharge2.IsCostGSTApplicable);
			using (var pluginToFreight = new InvoicingPluginForTest(shipment))
			{
				pluginToFreight.MenuItemResetTaxDefaults_Click_ForTestOnly(null, EventArgs.Empty);

				Assert("Sell GST should be actual", testCharge.IsSellGSTRateActual);
				AssertEquals("Sell GST should be empty", ZGuid.Empty, testCharge.JR_AT_SellGSTRate);
				AssertEquals("Sell GST amount should be zero", 0M, testCharge.JR_Sell_LocalGSTAmount);
				Assert("Cost GST should be actual", testCharge2.IsCostGSTRateActual);
				AssertEquals("Cost GST should be empty", ZGuid.Empty, testCharge2.JR_AT_CostGSTRate);
				AssertEquals("Sell GST amount should be zero", 0M, testCharge2.JR_Cost_LocalGSTAmount);
				Assert("Sell GST should not be applicable", !testCharge.IsSellGSTApplicable);
				AssertEquals("Sell TaxBranch should be empty", ZGuid.Empty, testCharge.JR_GB_SellTaxBranch);
				Assert("Cost GST should not be applicable", !testCharge2.IsCostGSTApplicable);
				AssertEquals("Cost TaxBranch should be empty", ZGuid.Empty, testCharge2.JR_GB_CostTaxBranch);
				AssertEquals("Job TaxBranch should not be empty", GlbBranch.CurrentBranch.PK, testJob.JH_GB_TaxBranch);
			}
		}

		#endregion

		public void TestRedefaultJobBillingExchangeRateMenuItem()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR");
			GlbCompany.CurrentCompany.Factory.Save();

			RefExchangeRate rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-11).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-9);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.AddCurrency(TestObjectCreator.USD, 2M, ExchangeRateValidLedgerEnum.AR);

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(-10);

			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
			{
				var menuText = "Re-default Job Billing Exchange Rate";
				var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems.Cast<MenuItem>().Single(m => m.Text == menuText);
				AssertNotNull(menu);

				AssertEquals("Precondition: ", 2M, job.ExchangeRates[0].JF_BaseRate);

				menu.PerformClick();

				AssertEquals("Exchange Rate must be updated.", 3.5M, job.ExchangeRates[0].JF_BaseRate);
			}
		}

		public void TestRedefaultJobBillingExchangeRateOnSaving()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR", prompt: true);
			GlbCompany.CurrentCompany.Factory.Save();

			RefExchangeRate rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-11).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-9);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUMEL";

			var shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_RL_NKLoadPort = "USLAX";
			shipmentTransport.JW_RL_NKDiscPort = "AUMEL";
			shipmentTransport.JW_ETD = ZDateTime.Now.AddDays(10);
			shipmentTransport.JW_ETA = ZDateTime.Now.AddDays(13);
			shipmentTransport.JW_VoyageFlight = "QF105";

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.AddCurrency(TestObjectCreator.USD, 2M, ExchangeRateValidLedgerEnum.AR);

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
			{
				AssertEquals("Precondition: ", 2M, job.ExchangeRates[0].JF_BaseRate);

				string expectedMessage = "Actual/Estimated Departure or Arrival dates have been changed.\r\nDo you want to re-default the exchange rates on the shipment billing tabs?";

				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-10);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Shipment ETA date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Job Exchange Rate must be updated.", 3.5M, job.ExchangeRates[0].JF_BaseRate);

				shipment.JS_E_ARV = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETA date is empty. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-11);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETD date is changed, but registry is set for ETA date. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HDR", prompt: true);
				GlbCompany.CurrentCompany.Factory.Save();
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-12);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Shipment ETD date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_DEP = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETD date is empty. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-10);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETA date is changed, but registry is set for ETD date. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment dates is not changed. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR");
				GlbCompany.CurrentCompany.Factory.Save();
				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-13);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETA date is changed, but registry is not configured for Prompt. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR", prompt: true);
				GlbCompany.CurrentCompany.Factory.Save();
				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-14);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Shipment ETA date is changed again and registry set up to Prompt. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HEA", prompt: true);
				GlbCompany.CurrentCompany.Factory.Save();

				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-16);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("estimated arrival date is changed and registry is set for estimated arrival date. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-16);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("estimated departure date is changed but registry is set for estimated arrival date. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HED", prompt: true);
				GlbCompany.CurrentCompany.Factory.Save();

				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-17);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("estimated arrival date is changed but registry is set for estimated departure date. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-17);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("estimated departure date is changed and registry is set for estimated departure date. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_E_ARV = ZDateTime.Now.AddDays(-15);
				job.ExchangeRates.RemoveAndDeleteAll();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Shipment ETA date is changed, but there aren't rates to update. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemReverseInvoices_Click_ShowsReversalDateForm()
		{
			Job testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			ForwardingShipment forwardingShipment = testJob.PlugInData as ForwardingShipment;
			AssertNotNull("Forwarding Shipment should not be null", forwardingShipment);
			InvoicingPluginToFreightWrapper pluginToFreight = new InvoicingPluginToFreightWrapper(forwardingShipment);

			testJob = Factory.Load<Job>(testJob.PK);
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				true, true);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			try
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				pluginToFreight.Job_ForTestOnly = testJob;
				SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
				pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);

				var message = string.Format("Should Prompt With ReversalDatesForm but was {0}", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
				AssertEquals(message, typeof(ReversalDatesForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			finally
			{
				pluginToFreight.Dispose();
			}
		}

		public void TestCreditLimitRecalculationAfterSaving()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				Factory.Save();

				string assertMessage = "{0} must create cache 1 time and so do 1 dbhit to get credit limit";
				AssertEquals(string.Format(assertMessage, "localClient"), 1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "agent"), 1, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "debtor"), 1, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				string expectedError = @"The Credit Limit for {2} is set to {1:0.00} AUD. Credit approved.
The Total Outstanding Balance is {0:0.00} AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Unposted Recognized Revenue, {0:0.00} AUD
  *  the Unposted Unrecognized Revenue, 0.00 AUD";
				AssertHasWarning("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 200m, 100M, localClient.OH_Code));
				AssertHasWarning("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Local client should be revalidated after saving", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 200m, 100m, localClient.OH_Code));
				AssertHasWarning("Charge Agent should be revalidated after saving", charge2.JR_OH_SellAccountInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Debtor should be revalidated after saving", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 400m, 300m, debtor.OH_Code));

				charge1.JR_OSSellAmt = 300;
				charge2.JR_OSSellAmt = 200;
				charge3.JR_OSSellAmt = 500;
				Factory.Save();

				AssertHasWarning("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 300m, 100m, localClient.OH_Code));
				AssertNoWarnings("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo);
				AssertHasWarning("Charge Local client should be revalidated after saving", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 300m, 100m, localClient.OH_Code));
				AssertNoWarnings("Charge Agent should be revalidated after saving", charge2.JR_OH_SellAccountInfo);
				AssertHasWarning("Charge Debtor should be revalidated after saving", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 500m, 300m, debtor.OH_Code));
			}
		}

		public void TestCreditLimitValidationAfterSavingDoesNotUseCachedValueWhenWebServiceInUse()
		{
			string expectedError = @"The Credit Limit for {2} is set to {1:0.00} AUD. Credit approved.
The Total Outstanding Balance is {0:0.00} AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Unposted Recognized Revenue, {0:0.00} AUD
  *  the Unposted Unrecognized Revenue, 0.00 AUD";

			using (var setClient = new DisposableAction(() => MockCreditLimitServiceClientProvider.Reset(true), () => MockCreditLimitServiceClientProvider.Reset()))
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);

				AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

				var debtor = TestObjectCreator.Debtor;
				debtor.MiscServ.OM_ARCreditLimit = 100;

				var shipment = TestObjectCreator.CreateShipment("S00001001");
				var job = TestObjectCreator.CreateJob(shipment, creator.LocalClient, 0, debtor, 0);
				Factory.Save();

				using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
				{
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 450, debtor);
					charge1.Validation.ValidateJR_OH_SellAccount();
					AssertNoWarning("No Credit Limit related warning should be shown", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 450m, 100m, debtor.OH_Code));

					pluginToFreight.Factory.Save();
					AssertHasWarning("Charge Debtor before saving", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 450m, 100m, debtor.OH_Code));
				}
			}
		}

		public void TestCreditLimitRecalculationAfterPosting()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 250, localClient);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 350, agent);
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 450, debtor);
				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
				var charge5 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
				var charge6 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);
				charge4.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
				charge5.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
				charge6.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
				Factory.Save();

				string expectedError = @"The Credit Limit for {2} is set to {1:0.00} AUD. Credit approved.
The Total Outstanding Balance is {0:0.00} AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Unposted Recognized Revenue, {0:0.00} AUD
  *  the Unposted Unrecognized Revenue, 0.00 AUD";
				AssertHasWarning("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 450m, 100M, localClient.OH_Code));
				AssertHasWarning("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, 650m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Local client should be revalidated after saving", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 450m, 100m, localClient.OH_Code));
				AssertHasWarning("Charge Agent should be revalidated after saving", charge2.JR_OH_SellAccountInfo, string.Format(expectedError, 650m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Debtor should be revalidated after saving", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 850m, 300m, debtor.OH_Code));
				AssertHasWarning("Charge Local client should be revalidated after saving", charge4.JR_OH_SellAccountInfo, string.Format(expectedError, 450m, 100m, localClient.OH_Code));
				AssertHasWarning("Charge Agent should be revalidated after saving", charge5.JR_OH_SellAccountInfo, string.Format(expectedError, 650m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Debtor should be revalidated after saving", charge6.JR_OH_SellAccountInfo, string.Format(expectedError, 850m, 300m, debtor.OH_Code));

				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				pluginToFreight.MenuItemPostAll_Click_ForTestOnly(null, new EventArgs());

				string assertMessage = "{0} must create cache 1 time and so do 1 dbhit to get credit limit";
				AssertEquals(string.Format(assertMessage, "localClient"), 1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "agent"), 1, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "debtor"), 1, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				AssertEquals("Postcondition: charge revenue should be posted", true, charge1.IsRevenuePosted);
				AssertEquals("Postcondition: charge revenue should be posted", true, charge2.IsRevenuePosted);
				AssertEquals("Postcondition: charge revenue should be posted", true, charge3.IsRevenuePosted);
				AssertEquals("Postcondition: charge revenue should not be posted", false, charge4.IsRevenuePosted);
				AssertEquals("Postcondition: charge revenue should not be posted", false, charge5.IsRevenuePosted);
				AssertEquals("Postcondition: charge revenue should not be posted", false, charge6.IsRevenuePosted);

				expectedError = @"The Credit Limit for {0} is set to {1:0.00} AUD. Credit approved.
The Total Outstanding Balance is {2:0.00} AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, {3:0.00} AUD
  *  the Unposted Recognized Revenue, {4:0.00} AUD
  *  the Unposted Unrecognized Revenue, 0.00 AUD";
				AssertHasWarning("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, localClient.OH_Code, 100M, 450m, 250, 200));
				AssertHasWarning("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, agent.OH_Code, 200m, 650m, 350, 300));
				AssertNoWarnings("No warnings on posted changes", charge1.JR_OH_SellAccountInfo);
				AssertNoWarnings("No warnings on posted changes", charge2.JR_OH_SellAccountInfo);
				AssertNoWarnings("No warnings on posted changes", charge3.JR_OH_SellAccountInfo);
				AssertHasWarning("Charge Local client should be revalidated after saving", charge4.JR_OH_SellAccountInfo, string.Format(expectedError, localClient.OH_Code, 100M, 450m, 250, 200));
				AssertHasWarning("Charge Agent should be revalidated after saving", charge5.JR_OH_SellAccountInfo, string.Format(expectedError, agent.OH_Code, 200m, 650m, 350, 300));
				AssertHasWarning("Charge Debtor should be revalidated after saving", charge6.JR_OH_SellAccountInfo, string.Format(expectedError, debtor.OH_Code, 300m, 850m, 450, 400));
			}
		}

		public void TestCreditLimitValidatedOnFirstGUIShown()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);
			Factory.Save();

			string expectedError = @"The Credit Limit for {2} is set to {1:0.00} AUD. Credit approved.
The Total Outstanding Balance is {0:0.00} AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Unposted Recognized Revenue, {0:0.00} AUD
  *  the Unposted Unrecognized Revenue, 0.00 AUD";

			AssertNoWarning("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 200m, 100M, localClient.OH_Code));
			AssertNoWarning("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
			AssertNoWarning("Charge Local client should be revalidated after saving", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 200m, 100m, localClient.OH_Code));
			AssertNoWarning("Charge Agent should be revalidated after saving", charge2.JR_OH_SellAccountInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
			AssertNoWarning("Charge Debtor should be revalidated after saving", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 400m, 300m, debtor.OH_Code));

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			{
				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				localClient.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
				string assertMessage = "{0} must create cache 1 time and so do 1 dbhit to get credit limit";
				AssertEquals(string.Format(assertMessage, "localClient"), 1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				// Leave cache for localClient
				agent.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				debtor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				pluginToFreight.OnGUIShown();

				AssertEquals("localClient must use cache as onGUIShown does not clear it, no extra dbhit to get credit limit", 0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "agent"), 1, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "debtor"), 1, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				AssertHasWarning("Job Local client should be validated OnGUIShown", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 200m, 100M, localClient.OH_Code));
				AssertHasWarning("Job Agent should be validated OnGUIShown", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Local client should be validated OnGUIShown", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 200m, 100m, localClient.OH_Code));
				AssertHasWarning("Charge Agent should be validated OnGUIShown", charge2.JR_OH_SellAccountInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Debtor should be validated OnGUIShown", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 400m, 300m, debtor.OH_Code));

				charge1.JR_OSSellAmt = 300;
				charge2.JR_OSSellAmt = 200;
				charge3.JR_OSSellAmt = 500;

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				agent.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				debtor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				pluginToFreight.OnGUIShown();

				assertMessage = "{0} must not discard cache and do a dbhit to get credit limit on next OnGUIShown";
				AssertEquals(string.Format(assertMessage, "localClient"), 0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "agent"), 0, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "debtor"), 0, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				AssertHasWarning("Job Local client should not be revalidated by next OnGUIShown", job.JH_OA_LocalChargesAddrInfo, string.Format(expectedError, 200m, 100M, localClient.OH_Code));
				AssertHasWarning("Job Agent should not be revalidated by next OnGUIShown", job.JH_OA_AgentCollectAddrInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Local client should not be revalidated by next OnGUIShown", charge1.JR_OH_SellAccountInfo, string.Format(expectedError, 200m, 100m, localClient.OH_Code));
				AssertHasWarning("Charge Agent should not be revalidated by next OnGUIShown", charge2.JR_OH_SellAccountInfo, string.Format(expectedError, 300m, 200m, agent.OH_Code));
				AssertHasWarning("Charge Debtor should not be revalidated by next OnGUIShown", charge3.JR_OH_SellAccountInfo, string.Format(expectedError, 400m, 300m, debtor.OH_Code));
			}
		}

		public void TestCreditLimitValidatedAsynchronously_UserIdleWorkerErrorIsNotReported()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, null, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, localClient);
			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var pluginToFreight = new InvoicingPluginToFreightForCreditLimitValidationTest(shipment))
			{
				// Need to have a Form as it is checked on queueing Validatios to be run by UserIdleWorker
				var tabControl = new ZTabControl();
				tabControl.Controls.Add(pluginToFreight.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)pluginToFreight).InitializePlugin(tabControl, form);

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				pluginToFreight.ChargeValidationSleepMilliSeconds_ForTestOnly = 6000;

				using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
				using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					using (UserIdleWorker.Suspend())
					{
						AssertEquals(0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

						pluginToFreight.OnGUIShown();
						// The following part can potentially cause intermittent test failures
						var originalCount = UserIdleWorker.QueuedWorkItemCount;
						// Wait for Credit Limit Details to be asynchronously fetched and validation queued for local client
						while (UserIdleWorker.QueuedWorkItemCount < originalCount + 1)
						{
							Application.DoEvents();
						}
						AssertEquals(1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					}

					var webserviceError = @$"Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.";

					AssertNoWarningContaining(job.JH_OA_LocalChargesAddrInfo, "Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.");
					AssertNoWarningContaining(charge1.JR_OH_SellAccountInfo, webserviceError);
					AssertNoWarningContaining(charge1.JR_OH_SellAccountInfo, webserviceError);

					ErrorReporter.Clear();
					UserIdleWorker.Flush();

					AssertEquals("Should not report SlowWorkflow Error", string.Empty, ErrorReporter.LastMessageReported);
					AssertHasWarningContaining(job.JH_OA_LocalChargesAddrInfo, "Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.");
					AssertHasWarningContaining(charge1.JR_OH_SellAccountInfo, webserviceError);
					AssertHasWarningContaining(charge1.JR_OH_SellAccountInfo, webserviceError);
				}
			}
		}

		public void TestCreditLimitValidatedAsynchronously_QueueUserIdleWorkerIfLocalClientAndAgentValidationIsTakingMoreThanTwoSeconds()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, null, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var shipment2 = TestObjectCreator.CreateShipment("S00001002");
			var job2 = TestObjectCreator.CreateJob(shipment2, localClient, 0, null, 0);
			Factory.Save();

			using (var form = new ZForm(shipment2))
			using (var pluginToFreight = new InvoicingPluginToFreightForCreditLimitValidationTest(shipment2))
			{
				// Need to have a Form as it is checked on queueing Validatios to be run by UserIdleWorker
				var tabControl = new ZTabControl();
				tabControl.Controls.Add(pluginToFreight.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)pluginToFreight).InitializePlugin(tabControl, form);

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				pluginToFreight.OrgValidationSleepMilliSeconds_ForTestOnly = 3000;
				pluginToFreight.OrgValidationSleepOnce = true;

				using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
				using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					using (UserIdleWorker.Suspend())
					{
						AssertEquals(0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

						pluginToFreight.OnGUIShown();
						// The following part can potentially cause intermittent test failures
						var originalCount = UserIdleWorker.QueuedWorkItemCount;
						// Wait for Credit Limit Details to be asynchronously fetched and validation queued for local client
						while (UserIdleWorker.QueuedWorkItemCount < originalCount + 1)
						{
							Application.DoEvents();
						}
						AssertEquals(1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					}

					var numberOfTimesUserIdleWorkerQueued = 0;
					UserIdleWorker.Queued += (sender, e) => numberOfTimesUserIdleWorkerQueued++;

					UserIdleWorker.Flush();

					AssertEquals(1, numberOfTimesUserIdleWorkerQueued);
				}
			}
		}

		public void TestCreditLimitValidatedAsynchronously_SameChargeNotValidatedAgain()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, null, 0);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var pluginToFreight = new InvoicingPluginToFreightForCreditLimitValidationTest(shipment))
			{
				// Need to have a Form as it is checked on queueing Validatios to be run by UserIdleWorker
				var tabControl = new ZTabControl();
				tabControl.Controls.Add(pluginToFreight.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)pluginToFreight).InitializePlugin(tabControl, form);

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				pluginToFreight.OrgValidationSleepMilliSeconds_ForTestOnly = 3000;
				pluginToFreight.ChargeValidationSleepMilliSeconds_ForTestOnly = 3000;
				pluginToFreight.OrgValidationSleepOnSecondRun = true;

				using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
				using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					using (UserIdleWorker.Suspend())
					{
						AssertEquals(0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

						pluginToFreight.OnGUIShown();
						// The following part can potentially cause intermittent test failures
						var originalCount = UserIdleWorker.QueuedWorkItemCount;
						// Wait for Credit Limit Details to be asynchronously fetched and validation queued for local client
						while (UserIdleWorker.QueuedWorkItemCount < originalCount + 1)
						{
							Application.DoEvents();
						}
						AssertEquals(1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					}
					AssertEquals(0, pluginToFreight.ValidatedChargeCounter);
					UserIdleWorker.Flush();
					AssertEquals(1, pluginToFreight.ValidatedChargeCounter);
				}
			}
		}

		public void TestCreditLimitValidatedAsynchronouslyOnFirstGUIShownIfWebServiceIsUsed()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);
			Factory.Save();

			AssertNoWarnings("Job Local client should be revalidated after saving", job.JH_OA_LocalChargesAddrInfo);
			AssertNoWarnings("Job Agent should be revalidated after saving", job.JH_OA_AgentCollectAddrInfo);
			AssertNoWarnings("Charge Local client should be revalidated after saving", charge1.JR_OH_SellAccountInfo);
			AssertNoWarnings("Charge Agent should be revalidated after saving", charge2.JR_OH_SellAccountInfo);
			AssertEquals("Charge Debtor has 1 notification", 1, charge3.JR_OH_SellAccountInfo.Notifications.Count());
			AssertHasWarning("Charge Debtor should be revalidated after saving. Has a warning about being either Local Client or Overseas Agent", charge3.JR_OH_SellAccountInfo,
@"You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.");

			using (var form = new ZForm(shipment))
			using (var pluginToFreight = new InvoicingPluginToFreightForCreditLimitValidationTest(shipment))
			{
				// Need to have a Form as it is checked on queueing Validatios to be run by UserIdleWorker
				var tabControl = new ZTabControl();

				// Set Charge Validation Sleep 5 seconds for background UserIdleWorker Validation
				pluginToFreight.ChargeValidationSleepMilliSeconds_ForTestOnly = 3000;

				tabControl.Controls.Add(pluginToFreight.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)pluginToFreight).InitializePlugin(tabControl, form);

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				localClient.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
				string assertMessage = "{0} must create cache 1 time and so do 1 dbhit to get credit limit";
				AssertEquals(string.Format(assertMessage, "localClient"), 1, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				// Clear cache for agent and debtor but leave cache for localClient
				agent.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				debtor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				// Need to use Web Service for something to get asynchronous fetching Credit Limit Details
				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				// Safetycatch Registry item must be enabled by default
				Assert("BillingTabBackgroundValidation", AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Value);

				// Temporary suspend to wait for 3 validations to be queued
				using (UserIdleWorker.Suspend())
				{
					AssertEquals("LocalClient not checked yet as it is not scheduled for background", 0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					AssertEquals("OverseasAgent not checked yet as it is not cheduled for background", 0, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					AssertEquals("Charge Debtor not checked yet as it is not scheduled for background", 0, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

					pluginToFreight.OnGUIShown();
					// The following part can potentially cause intermittent test failures
					var originalCount = UserIdleWorker.QueuedWorkItemCount;

					// Wait for Credit Limit Details to be asynchronously fetched and 3 Validations queued
					while (UserIdleWorker.QueuedWorkItemCount < originalCount + 3)
					{
						Application.DoEvents();
					}

					AssertEquals("localClient must use cache as onGUIShown does not clear it, no extra dbhit to get credit limit", 0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					AssertEquals(string.Format(assertMessage, "agent"), 1, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
					AssertEquals(string.Format(assertMessage, "debtor"), 1, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				}
				// Run queued Validations
				UserIdleWorker.Flush();

				Assert("Error should not be reported", ExceptionReporterTestListener.Instance.Count == 0);
				AssertEquals("Should Not contain DeveloperNotificationException", string.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("Should NOT contain DeveloperNotificationException", "to process (twice within 60 seconds)", ErrorReporter.LastMessageReported);

				var webserviceError = @$"Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.";
				AssertHasWarning("Job Local client should be validated OnGUIShown. It gets warning even with cached data because CreditChecker is able to detect chages in Registry settings", job.JH_OA_LocalChargesAddrInfo, webserviceError);
				AssertHasWarning("Job Agent should be validated OnGUIShown", job.JH_OA_AgentCollectAddrInfo, webserviceError);
				AssertHasWarning("Charge Local client should be validated OnGUIShown", charge1.JR_OH_SellAccountInfo, webserviceError);
				AssertHasWarning("Charge Agent should be validated OnGUIShown", charge2.JR_OH_SellAccountInfo, webserviceError);

				AssertHasWarnings(charge3.JR_OH_SellAccountInfo);
				AssertHasNotifications(@$"You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.", charge3.JR_OH_SellAccountInfo);
				AssertHasNotifications("Registry Item 'Accounting -> Credit Limit Check -> Credit Limit Check Web Service URL' is not set.", charge3.JR_OH_SellAccountInfo);

				// Make some changes and try to call OnGUIShown again
				charge1.JR_OSSellAmt = 300;
				charge2.JR_OSSellAmt = 200;
				charge3.JR_OSSellAmt = 500;

				// Go back to synchronous validation as it is easier to assert
				AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				agent.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				debtor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
				debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

				pluginToFreight.OnGUIShown();
				Application.DoEvents();

				// Second OnGUIShown should not cause validation
				assertMessage = "{0} must not discard cache and do a dbhit to get credit limit on next OnGUIShown";
				AssertEquals(string.Format(assertMessage, "localClient"), 0, localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "agent"), 0, agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
				AssertEquals(string.Format(assertMessage, "debtor"), 0, debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

				// Still have the same warnings
				AssertHasWarning("Job Local client should be validated OnGUIShown", job.JH_OA_LocalChargesAddrInfo, webserviceError);
				AssertHasWarning("Job Agent should be validated OnGUIShown", job.JH_OA_AgentCollectAddrInfo, webserviceError);
				AssertHasWarning("Charge Local client should be validated OnGUIShown", charge1.JR_OH_SellAccountInfo, webserviceError);
				AssertHasWarning("Charge Agent should be validated OnGUIShown", charge2.JR_OH_SellAccountInfo, webserviceError);
				AssertHasWarnings(charge3.JR_OH_SellAccountInfo);
				AssertHasNotifications(@$"You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Unable to retrieve a value for Credit Limit. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {BrandingFactory.Instance.ProductName} registry or a problem with an external system.", charge3.JR_OH_SellAccountInfo);
				AssertHasNotifications("Registry Item 'Accounting -> Credit Limit Check -> Credit Limit Check Web Service URL' is not set.", charge3.JR_OH_SellAccountInfo);
			}
		}

		public void TestJobLoadingUserIdleWorkItemAddedOnChangedChanged()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			{
				AssertEquals("Shipment.HasChanges", false, shipment.HasChanges);
				AssertEquals("Queued Idle Work Items Count", 0, UserIdleWorker.QueuedWorkItemCount);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				AssertEquals("Shipment.HasChanges", false, shipment.HasChanges);
				AssertNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;

				AssertEquals("Shipment.HasChanges", true, shipment.HasChanges);
				AssertNotNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				form.Close();
				AssertNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));
			}
		}

		public void TestJobLoadingUserIdleWorkItemAddedOnlyOnce()
		{
			CommonShipment shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			{
				AssertEquals("Shipment.HasChanges", false, shipment.HasChanges);
				AssertEquals("Queued Idle Work Items Count", 0, UserIdleWorker.QueuedWorkItemCount);

				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				AssertEquals("Shipment.HasChanges", false, shipment.HasChanges);
				AssertNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;

				AssertEquals("Shipment.HasChanges", true, shipment.HasChanges);
				AssertNotNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				UserIdleWorker.Flush();
				AssertNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				Factory.Save();
				AssertEquals("Shipment.HasChanges", false, shipment.HasChanges);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ABIGAS.PK;
				AssertEquals("Shipment.HasChanges", true, shipment.HasChanges);
				AssertNull("Queued Idle Work Item for loading Job", UserIdleWorker.QueuedWorkItems.FirstOrDefault(x => x.StartDelay == 3000));

				form.Close();
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGetNewTopLevelMenuCurrentInvoiceDateMenu()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";

			MenuItemTestHelper.ValidateDefaultARInvoiceDateMenuText(
				() =>
				{
					using (var plugin = new InvoicingPluginToFreight(shipment))
					{
						plugin.MakeOrActivateJob_ForTestOnly();
						plugin.OnGUIShown();
						var menu = plugin.GetNewTopLevelMenu_ForTestOnly();
						return menu.MenuItems[0].Text;
					}
				}
				);
		}

		[ExpectNoExceptions]
		public void TestEnumerateWhenCollectionsOrderWillBeChanged()
		{
			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 110, null, "", TestObjectCreator.AUD, 300, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 120, null, "", TestObjectCreator.AUD, 400, debtor);

			Factory.Save();

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				foreach (Charge charge in job.Charges)
				{
					var sellAccount = charge.SellAccount;
					while (sellAccount.ReadOnly)
					{
						sellAccount.SetReadOnlyIncludingChildren(false);
					}

					((IBindingList)sellAccount).ListChanged += (o, e) => job.Charges.Sort("JR_OSSellAmt");
				}

				plugin.OnGUIShown();
			}
		}

		public void TestEnumerateWhenChargeCollectionsOrderIsChangedDuringValidation()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001002");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 110, null, "", TestObjectCreator.AUD, 300, TestObjectCreator.LocalClient);
			Factory.Save();

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				foreach (Charge charge in job.Charges)
				{
					charge.JR_OSCostGSTAmt_CalcInfo.AdditionalValidation += new RunValidationInvoker(() => job.Charges.Sort("JR_OSSellAmt"));
				}

				AssertNoExceptionThrown(() => plugin.DoWarningOnlyValidationOnCharges_ForTestOnly());
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptAutoratingOptions_NoAutorating()
		{
			var job = GetJobThatHasAcceptedGroupCompanyCharges(GroupCompanyChargesForJob.AutoratingOptionsCode.NoAutorating);

			var message = "Group Company Sell Charge should have been accepted as a cost";
			AssertNotNull(message, job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "GROUPCHG"));
			AssertNull("Autorating should not be run", job.Charges.Cast<JobCharge>().SingleOrDefault(x => x.ChargeCode.AC_Code == "FRT"));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptAutoratingOptions_CancelButtonDoesNotAutoRate()
		{
			var job = GetJobThatHasAcceptedGroupCompanyCharges(GroupCompanyChargesForJob.AutoratingOptionsCode.AutoRateCostsAndRevenue, false);

			var message = "Group Company Charge should not be accepted when cancel button is click, nor should autorating have been run";
			AssertEquals(message, 0, job.Charges.Count);
		}

		public void TestCreditCheckDiagnosticInfo_WhenExceptionIsThrownByValidateCreditLimits_ExceptionIsLoggedInDiagnostics()
		{
			//Arrange
			var shipment = TestObjectCreator.CreateShipment("S14112021");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Factory.Save();

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Credit Limit Exception Test", sellCurrency: TestObjectCreator.AUD, osSellAmt: 150m, debtor: TestObjectCreator.ABIGAS);

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				pluginToFreight.AsyncFetchCreditLimitDetailsException_ForTest = new Exception("something went wrong fetching credit limit details");

				// Act
				pluginToFreight.OnGUIShown(); //calls ValidateCreditLimits in AsyncPreFetch mode
				Application.DoEvents();
				UserIdleWorker.Flush();

				try
				{
					Factory.Save(); // calls ValidateCreditLimits with a diagnosticInfoCollector parameter
				}
				catch (DeveloperNotificationException ex)
				{
					//Assert
					AssertNotNull("Expected a critical validation exception to be thrown", ex);

					var expectedExceptionMessage = "System.Exception: something went wrong fetching credit limit details";
					AssertContains(expectedExceptionMessage, ex.Message);
				}
				finally
				{
					AsyncHelper.WaitAllActiveTasksForTest();
				}
			}

			ErrorReporter.Clear();
		}

		public void TestCreditCheckTraceInfo_WhenSourceIsNotEnabled_IsNotPrinted()
		{
			var dummyTracer = new DummyTracer(["none"]);
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;
			agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;
			debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				charge1.JR_OSSellAmt = 300;
				charge2.JR_OSSellAmt = 200;
				charge3.JR_OSSellAmt = 500;
				Factory.Save();

				var actualTraceMessage = string.Join("", dummyTracer.Traces);

				AssertEquals("Error Key & Message", string.Empty, actualTraceMessage);

				dummyTracer.Traces.Clear();
			}
		}

		public void TestCreditCheckTraceInfo_WhenSourceIsEnabled_IsPrinted()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			var localClient = TestObjectCreator.LocalClient;
			localClient.MiscServ.OM_ARCreditLimit = 100;
			localClient.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var agent = TestObjectCreator.Agent;
			agent.MiscServ.OM_ARCreditLimit = 200;
			agent.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var debtor = TestObjectCreator.ABIGAS;
			debtor.MiscServ.OM_ARCreditLimit = 300;
			debtor.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;

			var shipment = TestObjectCreator.CreateShipment("S00001001");
			Factory.Save();

			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 200, localClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 300, agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100, null, "", TestObjectCreator.AUD, 400, debtor);

			using (var pluginToFreight = new InvoicingPluginToFreightWrapper(shipment))
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				charge1.JR_OSSellAmt = 300;
				charge2.JR_OSSellAmt = 200;
				charge3.JR_OSSellAmt = 500;
				Factory.Save();

				var actualTraceMessage = string.Join("", dummyTracer.Traces);

				AssertContains("Error Key", "CreditCheckDiagnosticInfo_Billing_2", actualTraceMessage);
				AssertContains(AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Caption, actualTraceMessage);
				AssertContains(AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.Caption, actualTraceMessage);
				AssertContains(AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Caption, actualTraceMessage);
				AssertContains(AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.Caption, actualTraceMessage);
				AssertContains(AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Caption, actualTraceMessage);
				AssertContains("Debtors", actualTraceMessage);
				AssertContains("Debtors Whose Credit Limit Cache have been cleared", actualTraceMessage);

				AssertContains("CreditCheckerCache", actualTraceMessage);
				AssertContains("CreditLimitCacheReset", actualTraceMessage);

				AssertContains("cachedBalance for ZLOCCLT", actualTraceMessage);
				AssertContains("Resetting cache for ZLOCCLT", actualTraceMessage);

				AssertContains("cachedBalance for ZAgent", actualTraceMessage);
				AssertContains("Resetting cache for ZAgent", actualTraceMessage);

				AssertContains("cachedBalance for ABIGAS", actualTraceMessage);
				AssertContains("Resetting cache for ABIGAS", actualTraceMessage);

				dummyTracer.Traces.Clear();
			}
		}

		Job GetJobThatHasAcceptedGroupCompanyCharges(ZString autoratingOptionCode, bool accept = true)
		{
			var shipment = TestObjectCreator.CreateShipment("S10022016", "GBSUN", "AUSYD");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GROUPCHG");
			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(companyNZ)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);
				TestObjectCreator.CreateChargeWithPaymentBasis(job, chargeCode, companyAU.OrgProxy, 100m);
			}

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(companyAU)))
			using (var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, null, 0))
			using (var dummyForm = new ZForm(shipment))
			{
				TestObjectCreator.CreateFlatCalculatorCosting(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "GBSUN", "AUSYD", null, "FRT", 120);
				TestObjectCreator.CreateFlatCalculatorClientRate(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "GBSUN", "AUSYD", TestObjectCreator.LocalClient, "FRT", 150);
				Factory.Save();

				dummyForm.Controls.Add(new ZTemplateTabControl());
				dummyForm.PlugIns.Add(ControllerIDs.JobInvoicing);
				dummyForm.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)dummyForm.PlugIns.Instances[0];
				using (ZFormModaliser.SuspendDispose())
				{
					ZFormModaliser.ResultToReturnFromShowDialog = accept ? DialogResult.OK : DialogResult.Cancel;
					invoicingPlugin.MenuItemGroupCompanyCharges_Click_ForTestOnly(job, new EventArgs());

					var form = ZFormModaliser.LastFormShownDialogForTest as DebtorsAcceptGroupChargesForm;
					AssertNotNull("Menu Item should open Group Company Sell Charges Form", form);
					var groupCompanyChargesForJob = form.DataSource as GroupCompanyChargesForJob;
					AssertNotNull("form should be bound to GroupCompanyChargesForJob", groupCompanyChargesForJob);
					groupCompanyChargesForJob.AutoratingOption = autoratingOptionCode;

					form.Show(); //Have to Re-Open the form as ZFormModaliser always disposes it for unit tests
					form.DebtorChargesCollectionExposed.SelectAllElements();

					if (accept)
					{
						form.AcceptButton.PerformClick();
					}
					else
					{
						form.CancelButton.PerformClick();
					}
				}

				return job;
			}
		}

		public void TestMarkAsInactive_ConsolHasGatewaySellingApportion()
		{
			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_UniqueConsignRef = "C10011991";
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "CNSHA";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var jobForConsol = TestObjectCreator.CreateJob(gatewayConsol, newFactory: Factory);
			jobForConsol.JH_GE = TestObjectCreator.GEADepartment.PK;

			var shipment = gatewayConsol.Shipments.AddNew();
			Factory.Save();

			var charge = jobForConsol.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_GE = new TestObjectCreator(jobForConsol.Factory).GEADepartment.PK;
			charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
			charge.JR_LocalSellAmt = 250m;
			charge.JR_JH_InternalJob = jobForConsol.PK;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			Factory.Save();

			shipment = gatewayConsol.Shipments.AddNew();
			var consignor = TestObjectCreator.LocalClient;
			consignor.OH_IsConsignor = true;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.LocalClient.PK;
			shipment.JS_TransportMode = "AIR";
			var job = TestObjectCreator.CreateJob(shipment, newFactory: Factory);
			job.JH_OA_LocalChargesAddr = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			job.JH_GE = TestObjectCreator.GEADepartment.PK;
			Factory.Save();

			Assert(job.JH_IsActive);

			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				var plugin = (InvoicingPluginToFreight)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				plugin.OnGUIShown();
				plugin.OnUserControlShown();

				plugin.MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(plugin, new EventArgs());
				Assert(!job.JH_IsActive);
			}
		}

		#region Plugin Parent

		public void TestPluginParentForTransportBookings()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);

			var consolidationBooking = Factory.New<Enterprise.Integration.TransportBooking.IDtbBookingConsolidation>();
			consolidationBooking.KB_JobDirection = "DLV";
			consolidationBooking.KB_JobType = "BKG";
			consolidationBooking.KB_ParentID = shipment.PK;
			consolidationBooking.KB_ParentTableCode = shipment.TablePrefix;

			var booking = Factory.New<Enterprise.Integration.TransportBooking.IDtbBooking>();
			booking.KM_RatingFreightMode = Constants.ContainerModes.Loose;
			booking.KM_KB_Booking = consolidationBooking.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			var jobInAnotherFactory = new JobHeader.Loader(reloadedShipment).TryCreateWithoutMutexForTestOnly();
			jobInAnotherFactory.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;
			newFactory.Save();

			var job = (Job)shipment.Job;

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();

				var message = "Typical case, everything about the InvoicingPlugin belongs to the shipment";
				CombineAssertions(message, () =>
				{
					AssertEquals("PlugInParent_ForTestOnly should be the shipment", shipment, plugin.PlugInParent_ForTestOnly);
					AssertEquals(job.PK, plugin.Job_ForTestOnly.PK);
					AssertEquals(shipment, plugin.Job_ForTestOnly.PlugInData);
					AssertEquals("N/A", "", plugin.Job_ForTestOnly.PlugInData.InvoicingSupporter.OperationalJobRef);
				});
			}

			using (var plugin = new InvoicingPluginToFreight(booking))
			{
				plugin.Job_ForTestOnly = job;
				plugin.Job_ForTestOnly.HasChanges = false;
				plugin.OnGUIShown();

				var message = "Even though we use the Shipment's job, this Job's PlugInData should be the "
					+ "same as the PlugInParent i.e. the Transport Booking. This is how Transport Bookings "
					+ "are meant to work as they share the Shipment's Job.";

				CombineAssertions(message, () =>
				{
					AssertEquals("PlugInParent_ForTestOnly should be the booking", booking, plugin.PlugInParent_ForTestOnly);
					AssertEquals("still the same job", job.PK, plugin.Job_ForTestOnly.PK);
					AssertEquals("Most important part of the test.", booking, plugin.Job_ForTestOnly.PlugInData);
					AssertEquals(booking.KM_JobID, plugin.Job_ForTestOnly.PlugInData.InvoicingSupporter.OperationalJobRef);
				});
			}
		}

		#endregion

		#region ConcurrencyTesting

		[TestDate(2017, 03, 10)]
		[ExpectNoExceptions]
		public void TestRecognizeRevenueMenuItem_ConcurrencyExceptionForAccTransactionLine_AL_RevRecognitionType()
		{
			var userFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var objectCreator = new TestObjectCreator(userFactory);

			var expectedDate = ZDateTime.Now.AddDays(10);
			objectCreator.CreateTestPeriods(expectedDate.Date);

			var shipment = objectCreator.CreateShipment("SHP1");
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);

			var revenueRecognitionCollectionForTest = new RevenueRecognitionCollection();
			var revenueRecognitionSetup = revenueRecognitionCollectionForTest.AddNew();
			revenueRecognitionSetup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognitionSetup.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognitionSetup.Mode = Core.Constants.TransportModes.All;
			revenueRecognitionSetup.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revenueRecognitionCollectionForTest))
			{
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

				var apInvoice = objectCreator.CreateInvoice(typeof(APInvoice), objectCreator.AUD, 1M);
				apInvoice.AH_PostDate = ZDateTime.Now.AddDays(-10);
				var apInvoiceLine = objectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, apInvoice, job, objectCreator.CC1, objectCreator.AUD, 1M, "Desc", 10M);
				apInvoiceLine.AL_ReverseDate = ZDateTime.Empty;
				var charge = objectCreator.CreateCharge(apInvoiceLine, job, objectCreator.CC1, objectCreator.AUD);
				charge.Department.GE_Misc = false;

				job.RunPreSaveValidation();
				AssertNoErrors("Precondition", job);

				userFactory.Save();

				shipment.JS_E_ARV = expectedDate;
				userFactory.Save();

				using (var command = Db.Connection.Command("UPDATE dbo.AccTransactionLines SET AL_RevRecognitionType = '', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @LinePK"))// To avoid critical validation errors
				{
					command.AddParameter("@LinePK", System.Data.SqlDbType.UniqueIdentifier, apInvoiceLine.PK.ToGuid());
					command.ExecuteNonQuery();
				}

				apInvoiceLine.Reload();
				AssertEquals("Precondition: AL_RevRecognitionType should be empty.", ZString.Empty, apInvoiceLine.AL_RevRecognitionType);

				using (var plugin = new InvoicingPluginToFreightWrapperForConcurrencyTesting(shipment, job))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				}

				var expectedMessage = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.";
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
				AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, new BusinessObjectFactory().Load<TransactionLine>(apInvoiceLine.PK).AL_RevRecognitionType);
			}
		}

		[TestDate(2017, 03, 10)]
		[ExpectNoExceptions]
		public void TestRecognizeRevenueMenuItem_ConcurrencyExceptionForJobCharge_JR_AL_APLineAndJR_AL_ARLine()
		{
			var userFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var objectCreator = new TestObjectCreator(userFactory);

			var expectedDate = ZDateTime.Now.AddDays(10);
			objectCreator.CreateTestPeriods(expectedDate.Date);

			var revenueRecognitionCollectionForTest = new RevenueRecognitionCollection();
			var revenueRecognitionSetup = revenueRecognitionCollectionForTest.AddNew();
			revenueRecognitionSetup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognitionSetup.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognitionSetup.Mode = Constants.TransportModes.All;
			revenueRecognitionSetup.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, revenueRecognitionCollectionForTest))
			{
				var shipment = objectCreator.CreateShipment("SHP1");
				shipment.JS_E_ARV = expectedDate;
				var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0m, objectCreator.Agent, 0m);

				var chargeForTest = userFactory.New<Charge>();

				chargeForTest.ProcessAccrualAndWIPViaAnotherUser_ForTestOnly += (object sender, EventArgs args) =>
				{
					if (chargeForTest.IsInDatabase)
					{
						var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
						var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(chargeForTest.ShipmentInfo.PK);
						using (var plugin = new InvoicingPluginToFreight(shipmentInNewFactory))
						{
							plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
						}
					}
				};

				job.Charges.Add(chargeForTest);
				chargeForTest.JR_JH = job.PK;
				chargeForTest.Department.GE_Misc = false;
				chargeForTest.JR_AC = objectCreator.CC2.PK;
				chargeForTest.JR_RX_NKCostCurrency = objectCreator.AUD.RX_Code;
				chargeForTest.JR_OSCostAmt = 300m;
				chargeForTest.JR_RX_NKSellCurrency = objectCreator.AUD.RX_Code;
				chargeForTest.JR_OSSellAmt = 300m;

				job.RunPreSaveValidation();
				AssertNoErrors("Precondition", job);

				userFactory.Save();

				AssertNull(chargeForTest.Accrual);
				AssertNull(chargeForTest.WIP);
				AssertEquals(0, job.RevenueRecognitionCollection.Count);

				using (var plugin = new InvoicingPluginToFreight(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugin.MenuItemRecognizeRevenue_Click_ForTestOnly(null, EventArgs.Empty);
				}

				var expectedMessage = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.";
				AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Accrual created.", chargeForTest.Accrual);
				Assert("Accrual created but not saved.", !chargeForTest.Accrual.IsInDatabase);
				AssertNotNull("WIP created.", chargeForTest.WIP);
				Assert("WIP created but not saved.", !chargeForTest.WIP.IsInDatabase);
				AssertEquals("Job has 1 revenue recognition.", 1, job.RevenueRecognitionCollection.Count);
				AssertEquals("Revenue recognition created by this user is not saved", 1, job.RevenueRecognitionCollection.Count(x => !x.IsInDatabase));

				var reloadFactory = new BusinessObjectFactory();
				var reloadedCharge = reloadFactory.Load<Charge>(chargeForTest.PK);
				AssertNotNull(reloadedCharge.Accrual);
				AssertNotNull(reloadedCharge.WIP);
				var reloadedShipment = reloadFactory.Load<ForwardingShipment>(shipment.PK);
				var reloadedJob = new Job.Loader(reloadedShipment).Load();
				AssertEquals(1, reloadedJob.RevenueRecognitionCollection.Count);
				AssertEquals(expectedDate, reloadedJob.GetRevenueRecognitionDate(reloadedCharge.CostRecognition));
				AssertEquals(expectedDate, reloadedJob.GetRevenueRecognitionDate(reloadedCharge.SellRecognition));
			}
		}

		public void TestMenuItemReverseInvoices_ClickForConcurrencyException()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var shipment = job.PlugInData as ForwardingShipment;
			TestObjectCreator.CreateCharge(job, objectCreator.CC1, "Desc", objectCreator.AUD, 1000m, objectCreator.Creditor1, objectCreator.AUD, 1000m, objectCreator.LocalClient);
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			using (var pluginToFreight = new InvoicingPluginToFreightWrapperForConcurrencyTesting(shipment, job))
			{
				pluginToFreight.Job_ForTestOnly = job;
				SecurityOverrideProviderSource.Get(pluginToFreight.Job_ForTestOnly).Provider = new JobInvoicingSecurityOverrideProvider();
				pluginToFreight.MenuItemReverseInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var expectedErrorMessage = "While you were working, another user has modified this job. Please try again.";
				AssertEquals("User friendly message must be shown", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIfJobDetechedCallOnIsAllowedToViewBillingTabErrorReportMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = TestObjectCreator.CreateJob(shipment);

			using (InvoicingPluginToFreight plugIn = new InvoicingPluginToFreight(shipment))
			{
				ErrorReporter.Clear();

				var isAllowedToViewBillingTab = plugIn.IsAllowedToViewBillingTab_ForTestOnly;

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				job.Delete();
				AssertEquals(DataRowState.Detached, ((INeedRow)job).Row.RowState);
				AssertEquals("Precondition", ZString.Empty, plugIn.AccessJH_GBWhileJobIsDetached_ErrorReportKey);

				isAllowedToViewBillingTab = plugIn.IsAllowedToViewBillingTab_ForTestOnly;

				AssertEquals("AccessJH_GBWhileJobIsDetached", plugIn.AccessJH_GBWhileJobIsDetached_ErrorReportKey);

				Assert(ExceptionReporterTestListener.Instance[0].Message.Contains("Access branch property from a detached Job.\r\nDeleteJobHeader:\r\n   at"));
				Assert(ExceptionReporterTestListener.Instance[1].InnerException.Message.Contains("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row."));

				ErrorReporter.Clear();
			}
		}

		public void TestLoadJobInSameFactoryAndDeactivateIt()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var shipment = consol.Shipments[0];
			var job = creator.CreateJob(shipment);

			using (var plugIn = new InvoicingPluginToFreight(shipment))
			{
				ErrorReporter.Clear();

				var userShouldPlugInGUIAndBusinessEntityBeCreated = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();

				Assert(userShouldPlugInGUIAndBusinessEntityBeCreated);
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				job.MarkAsInactive();
				Factory.Save();

				AssertEquals(DataRowState.Detached, ((INeedRow)job).Row.RowState);
				AssertEquals("Precondition", ZString.Empty, plugIn.AccessJH_GBWhileJobIsDetached_ErrorReportKey);

				userShouldPlugInGUIAndBusinessEntityBeCreated = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();

				Assert(!userShouldPlugInGUIAndBusinessEntityBeCreated);

				AssertEquals(ZString.Empty, plugIn.AccessJH_GBWhileJobIsDetached_ErrorReportKey);

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				Assert("Job_ForTestOnly should be marked as cancelled", plugIn.Job_ForTestOnly.IsDeleted);
				Assert(((IPlugInInternals)plugIn).CoveringLabel.Visible);
				AssertEquals(@"This invoicing job was marked as inactive by another form. You will not be able to save the changes you made to this Shipment.
Please close this Shipment before re-entering your changes.", ((IPlugInInternals)plugIn).CoveringLabel.Text);

				AssertEquals($@"Invoicing job {shipment.JS_UniqueConsignRef} was marked as inactive by another form. You will not be able to save the changes you made to this Shipment.
Please close this Shipment before re-entering your changes.", plugIn.PlugInNotDisplayedMessage);
			}
		}

		public class InvoicingPluginToFreightWrapperForConcurrencyTesting : InvoicingPluginToFreightWrapper
		{
			readonly Job FreightJob;

			public InvoicingPluginToFreightWrapperForConcurrencyTesting(IBusiness hostEntity, Job job) : base(hostEntity)
			{
				FreightJob = job;
			}

			protected override void ReverseAllInvoicesCore(JobInvoicingReverser reverser, string reversingReason, string reversingCode)
			{
				reverser.ReversingFactory.RefreshEnabled = false;
				base.ReverseAllInvoicesCore(reverser, reversingReason, reversingCode);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var job = newFactory.Load<Job>(FreightJob.PK);
				job.Charges[0].Accrual.Reverse();
				newFactory.Save();
			}

			protected override void FixRelatedLinesCore(Job job, BusinessObjectFactory factoryForFixedLines)
			{
				base.FixRelatedLinesCore(job, factoryForFixedLines);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(HostBusinessEntity.Identifier);

				var revenueRecognitionCollectionForTest = new RevenueRecognitionCollection();
				var revenueRecognitionSetup = revenueRecognitionCollectionForTest.AddNew();
				revenueRecognitionSetup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
				revenueRecognitionSetup.DirectionCode = Constants.FreightShipmentDirection.Code.All;
				revenueRecognitionSetup.Mode = Core.Constants.TransportModes.All;
				revenueRecognitionSetup.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
				using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revenueRecognitionCollectionForTest))
				{
					var jobInNewFactory = new Job.Loader(shipmentInNewFactory).Load();
					jobInNewFactory.FixRelatedLinesWithEmptyRecognitionType(newFactory);
					newFactory.Save();
				}
			}
		}

		#endregion

		#region Fix Rev Recognition Data Menu Item

		public void TestFixRevenueRecognitionMenuItem_VisibleForSupportOnly()
		{
			AssertFixRevenueRecognitionMenuItem_VisibleForSupportOnly("CWSupport");
			AssertFixRevenueRecognitionMenuItem_VisibleForSupportOnly("CWWeb");
		}

		void AssertFixRevenueRecognitionMenuItem_VisibleForSupportOnly(string staff)
		{
			var menuItemText = "Fix Revenue Recognition Data(Support Only)";
			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL");

			using (Env.Instance.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();

				if (staff == "CWSupport")
				{
					AssertNotNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText));
				}
				else
				{
					AssertNull(plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText));
				}
			}
		}

		public void TestFixRevenueRecognition_ShowsMessageFromFixRevenueRecognitionData()
		{
			var menuItemText = "Fix Revenue Recognition Data(Support Only)";
			var expectedMessage = "Job revenue recognition data fixed.";

			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL");
			Job = TestObjectCreator.CreateJob(Shipment);

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();

				var mockInvoicingPluginToFreightPresentationProvider = new Mock<IInvoicingPluginToFreightPresentationProvider>();
				mockInvoicingPluginToFreightPresentationProvider.Setup(x => x.FixRevenueRecognitionData(It.IsAny<Func<ZString, ZBool>>())).Returns(expectedMessage);
				var mockAccountingPresentationProviderFactory = new Mock<IAccountingPresentationProviderFactory>();
				mockAccountingPresentationProviderFactory.Setup(x => x.GetInvoicingPluginToFreightPresentationProvider(It.IsAny<IClosedJobReopener>(), It.IsAny<IJobRevRecognitionDataRetriever>())).Returns(mockInvoicingPluginToFreightPresentationProvider.Object);
				ObjectFactory.Substitute(mockAccountingPresentationProviderFactory.Object);

				var menuItem = plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText);
				AssertNotNull("Precondition: menu item should not be null", menuItem);

				menuItem.PerformClick();

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2024, 04, 15)]
		public void TestFixRevenueRecognition_Confirmation_PopupMessage()
		{
			var menuItemText = "Fix Revenue Recognition Data(Support Only)";
			var expectedMessage = @"Before applying the data fix, the Job Revenue Recognition Date is

Empty

After applying the fix, the Job Revenue Recognition Date should be

DEP 15-Apr-24

Do you want to proceed?";

			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL");
			Job = TestObjectCreator.CreateJob(Shipment);

			var wip = TestObjectCreator.CreateWIP(Job);
			wip.AL_PostDate = ZDateTime.Today;
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();

				var menuItem = plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText);
				AssertNotNull("Precondition: menu item should not be null", menuItem);

				menuItem.PerformClick();

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFixRevenueRecognition_Confirmation_UserChooseProceed()
		{
			var menuItemText = "Fix Revenue Recognition Data(Support Only)";
			var expectedMessage = "Job revenue recognition data fixed.";

			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL");
			Job = TestObjectCreator.CreateJob(Shipment);

			var wip = TestObjectCreator.CreateWIP(Job);
			wip.AL_PostDate = ZDateTime.Today;
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();

				var menuItem = plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText);
				AssertNotNull("Precondition: menu item should not be null", menuItem);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFixRevenueRecognition_Confirmation_UserChooseNotProceed()
		{
			var menuItemText = "Fix Revenue Recognition Data(Support Only)";

			Shipment = TestObjectCreator.CreateShipment("S00010001", "USLAX", "NZAKL");
			Job = TestObjectCreator.CreateJob(Shipment);

			var wip = TestObjectCreator.CreateWIP(Job);
			wip.AL_PostDate = ZDateTime.Today;
			wip.AL_RevRecognitionType = RecognitionDateOptionCodes.ActualDepartureDate;

			Factory.Save();

			using (var plugIn = new InvoicingPluginToFreight(Shipment))
			using (var form = new ZForm(Shipment))
			{
				var toplevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				plugIn.OnGUIShown();

				var menuItem = plugIn.MainMenuItem_ForTestOnly.MenuItems.FindByText(menuItemText);
				AssertNotNull("Precondition: menu item should not be null", menuItem);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();

				AssertStartsWith("Should not pop up any messages after click 'No'.", "Before applying the data fix", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		AccChargeCode fTestCustomsDeferredChargeCode;
		AccChargeCode TestCustomsDeferredChargeCode
		{
			get
			{
				if (fTestCustomsDeferredChargeCode == null)
				{
					fTestCustomsDeferredChargeCode = Factory.New<AccChargeCode>();
					fTestCustomsDeferredChargeCode.AC_Code = "TCSDEF";
					fTestCustomsDeferredChargeCode.AC_Desc = "Test Customs Deferred";
					fTestCustomsDeferredChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
				}
				return fTestCustomsDeferredChargeCode;
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
		ForwardingShipment Shipment;
		JobCharge JobCharge;
		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;

		void CreateEmptyJobForShipment(ZGuid shipmentPK)
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipmentPK;
			Factory.Save();
		}

		#region Mock Objects

		public class InvoicingPluginToFreightForCreditLimitValidationTest : InvoicingPluginToFreight
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public int ChargeValidationSleepMilliSeconds_ForTestOnly = 0;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public int OrgValidationSleepMilliSeconds_ForTestOnly = 0;
			public bool OrgValidationSleepOnce;
			public bool OrgValidationSleepOnSecondRun;
			public int ValidatedChargeCounter;
			int OrgValidationSleepCounter;
			int ValidateCreditLimitsCoreCallCounter;

			public InvoicingPluginToFreightForCreditLimitValidationTest(IBusiness hostObject)
				: base(hostObject)
			{
			}

			protected override void SleepAfterChargeValidationForTestOnly()
			{
				System.Threading.Thread.Sleep(ChargeValidationSleepMilliSeconds_ForTestOnly);
				ValidatedChargeCounter++;
			}

			protected override void SleepAfterOrgValidationForTestOnly()
			{
				var shouldSleep = OrgValidationSleepMilliSeconds_ForTestOnly != 0 && (OrgValidationSleepOnce && OrgValidationSleepCounter == 0) || (OrgValidationSleepOnSecondRun && ValidateCreditLimitsCoreCallCounter == 1);
				ValidateCreditLimitsCoreCallCounter++;
				if (shouldSleep)
				{
					System.Threading.Thread.Sleep(OrgValidationSleepMilliSeconds_ForTestOnly);
					OrgValidationSleepCounter++;
				}
			}
		}

		public class InvoicingPluginToFreightWrapper : InvoicingPluginToFreight
		{
			public InvoicingPluginToFreightWrapper()
				: base(null)
			{
			}

			public InvoicingPluginToFreightWrapper(IBusiness hostEntity)
				: base(hostEntity)
			{
			}

			public ZPlugIn[] OtherPlugInsWhichImplement;

			protected override ZPlugIn[] GetOtherPlugInsWhichImplement(Type interfaceType)
			{
				return OtherPlugInsWhichImplement;
			}

			protected override void GetReversingReason(ref string reversingReason, ref string reversingCode)
			{
				reversingReason = "unit test";
				reversingCode = "tst";
			}

			protected override InvoicingPostManagerGUIWrapper PreparePostingWrapperForTest(InvoicingPostManagerGUIWrapper wrapperAsPerProductionCode)
			{
				wrapperAsPerProductionCode.DoTestPostTransactions = true;
				return wrapperAsPerProductionCode;
			}
		}

		public class InvoicingPluginForTest : InvoicingPluginToFreight
		{
			public InvoicingPluginForTest(IBusiness hostObject)
				: base(hostObject)
			{
			}

			public void PerformDelete()
			{
				Delete();
			}

			protected override void GetReversingReason(ref string reversingReason, ref string reversingCode)
			{
				reversingReason = "unit test";
				reversingCode = "tst";
			}
		}

		class DummyJobInvoicingBusinessObject : NonPersistentBusinessObject, IJobInvoicingPlugIn, ICancellable, IStmALogParent, IRatingSupporter, IJobInvoicingPlugInAdditionalJobs
		{
			public DummyJobInvoicingBusinessObject()
			{
			}

			public DummyJobInvoicingBusinessObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyJobInvoicingBusinessObject(BusinessObjectFactory factory, ZGuid jobHeaderParentPK, ZString tableName)
				: base(factory)
			{
				this.JobHeaderParentPK = jobHeaderParentPK;
				this.fTableName = tableName;
			}
			readonly ZGuid JobHeaderParentPK;
			readonly ZString fTableName;

			bool fPostedStateHasChanges;
			public bool PostedStateHasChanges
			{
				get { return fPostedStateHasChanges; }
				set { fPostedStateHasChanges = value; }
			}

			public SecurityCheckpoint AuditSecurity
			{
				get
				{
					if (auditSecurity == null)
					{
						SecurityForTest security = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
						auditSecurity = new SecurityCheckpoint("DummySecurity", (NoResString)"Audit Dummy", null, security);
					}
					return auditSecurity;
				}
			}

			SecurityCheckpoint auditSecurity;

			#region IJobNumber

			string IJobNumber.JobNumber
			{
				get { return "JobNumber"; }
			}

			#endregion

			#region IJobHeaderParent

			ZGuid IJobHeaderParentCore.PK
			{
				get
				{
					if (JobHeaderParentPK != ZGuid.Empty)
					{
						return JobHeaderParentPK;
					}
					else
					{
						return PK;
					}
				}
			}

			string IJobHeaderParentCore.TableName
			{
				get { return fTableName; }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			#endregion

			#region ICancellable Members

			public string CanCancel()
			{
				return ZString.Empty;
			}

			public string CanReactivate()
			{
				return null;
			}

			bool ICancellable.IsCancelled
			{
				get { return fIsCancelled; }
				set { fIsCancelled = value; }
			}
			bool fIsCancelled;

			bool ICancellable.IsCancelledHasChanged
			{
				get { return false; }
			}

			#endregion

			#region IJobInvoicingPlugIn Members

			DummyJobInvoicingBusinessObjectInvoicingSupporter fInvoicingSupporter;
			public IJobInvoicingSupporter InvoicingSupporter
			{
				get { return fInvoicingSupporter ?? (fInvoicingSupporter = new DummyJobInvoicingBusinessObjectInvoicingSupporter(this)); }
			}

			#endregion

			#region IStmALogParent

			Logs IStmALogProvider.Logs
			{
				get { throw new NotImplementedException(); }
			}

			ZGuid IStmALogParent.LogsParentPK
			{
				get { return PK; }
			}

			string IStmALogParent.LogsParentTableName
			{
				get { return TableName; }
			}

			BusinessObjectFactory IStmALogProvider.LogsFactory
			{
				get { return Factory; }
			}

			BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
			{
				get { return Array.Empty<BusinessObject>(); }
			}

			void IStmALogParent.ProcessLog(IStmALog log)
			{
			}

			bool IStmALogParent.DeferFiringWorkflow
			{
				get { return false; }
			}

			#endregion

			#region IRatingSupporter Members

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get
				{
					var provider = new DummyRatingAdaptersProvider<DummyJobInvoicingBusinessObject>(this);
					provider.additionalJobsExposed = additionalJobsExposed;
					return provider;
				}
			}

			public ReadOnlyCollection<IJobInvoicingPlugIn> additionalJobsExposed;

			#endregion

			#region IJobInvoicingPlugInAdditionalJobs Members

			IJobInvoicingPlugIn[] IJobInvoicingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor
			{
				get
				{
					var result = new List<IJobInvoicingPlugIn>();
					if (additionalJobsExposed != null)
					{
						result.AddRange(additionalJobsExposed);
					}

					return result.ToArray();
				}
			}

			#endregion

			public override bool IsInDatabase
			{
				get
				{
					return isInDatabaseOverride || base.IsInDatabase;
				}
			}

			public void SetIsInDatabaseOverride(bool overrideSaved)
			{
				isInDatabaseOverride = overrideSaved;
			}

			bool isInDatabaseOverride;
		}

		class DummyJobInvoicingBusinessObjectInvoicingSupporter : JobInvoicingSupporter
		{
			public DummyJobInvoicingBusinessObjectInvoicingSupporter(DummyJobInvoicingBusinessObject parent)
				: base(parent)
			{
				Parent = parent;
				consumerType = JobInvoicingConsumerTypes.Shipment;
			}

			protected readonly DummyJobInvoicingBusinessObject Parent;

			public override RefCurrency ConsolRateCurrency
			{
				get { return Parent.Factory.LoadTop1<RefCurrency>(new ZQuery()); }
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return consumerType; }
			}

			JobInvoicingConsumerType consumerType;

			public void SetConsumerType(JobInvoicingConsumerType type)
			{
				this.consumerType = type;
			}

			public override RefUNLOCO Destination
			{
				get { return Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery()); }
			}

			public override ZDateTime ATA
			{
				get { return ZDateTime.Now; }
			}

			public override ZDateTime ATD
			{
				get { return ZDateTime.Now; }
			}

			public override ZDateTime ETA
			{
				get { return ZDateTime.Now; }
			}

			public override ZDateTime ETD
			{
				get { return ZDateTime.Now; }
			}

			public override bool IsImport
			{
				get { return true; }
			}

			public override RefUNLOCO Origin
			{
				get { return Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery()); }
			}

			public override RefUNLOCO GetTranshipmentPort(CostSell costOrSell)
			{
				return Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			}

			protected override SecurityCheckpoint GetAuditSecurityCore()
			{
				return Parent.AuditSecurity;
			}

			public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
			{
				return ZDateTime.Now;
			}

			public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
			{
				return ZDateTime.Now;
			}

			public override void PostedStateChanged()
			{
				Parent.PostedStateHasChanges = true;
			}

			protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
			{
				return ConsumerType == JobInvoicingConsumerTypes.CFSShipment
					? Env.Security.CFSShipmentJobInvoicing
					: Env.Security.MaintainShipmentJobInvoicing;
			}
		}

		class DummyRatingAdaptersProvider<T> : RatingOrCostingAdaptersProvider<T>
			where T : DummyJobInvoicingBusinessObject
		{
			public DummyRatingAdaptersProvider(T parent) : base(parent, JobInvoicingConsumerTypes.Consol) { }

			public ReadOnlyCollection<IJobInvoicingPlugIn> additionalJobsExposed;

			protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
			{
				return null;
			}

			protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(T parent)
			{
				return additionalJobsExposed ?? base.GetAdditionalJobs(parent);
			}
		}
		#endregion

		#endregion
	}
}
