using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.GUI.BrowserInterop.Tests;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobChargeUserControlTest : TestCaseWithFactory
	{
		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		public void TestAutoScrollForCostTabPage()
		{
			using (var control = new JobChargeUserControl())
			{
				var tabControl = control.Controls.Find("ChargesDetailsTabControl", true).First() as ZTabControl;
				var costTabPage = tabControl.GetTabPage("CostTabPage");
				AssertEquals(true, costTabPage.AutoScroll);
			}
		}

		public void TestAutoRateNotePopupButton2()
		{
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				var noteType = control.AutoRateNotePopupButton2_ForTestOnly.NoteType;
				AssertEquals("AutoRating Log", noteType);
			}
		}

		public void TestAutoRateNotePopupButton()
		{
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				var noteType = control.AutoRateNotePopupButton_ForTestOnly.NoteType;
				AssertEquals("AutoRating Log", noteType);
			}
		}

		public void TestQuickCalculateMenuExists()
		{
			Job job = Factory.NewJobForTesting<Job>();
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(job);
				MenuItem quickCalculateMenuItem = control.JobChargeBoundGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);
			}
		}

		#region AP Cash Advance

		[ExpectNoExceptions]
		public void TestCashAdvanceButtonWithoutSelectingValidJobCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge 01", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 250M, TestObjectCreator.Debtor);

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);
				testForm.Show();

				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Please select a charge line in the above listing of charges before pressing 'Advance Payment'.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.JobChargeBoundGrid.Select(1);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Please select a charge line in the above listing of charges before pressing 'Advance Payment'.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.JobChargeBoundGrid.Select(0);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Job should be saved before pressing 'Advance Payment'.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				control.JobChargeBoundGrid.Select(0);
				control.JobChargeBoundGrid_Click_ForTestOnly(null, null);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				control.CashAdvanceButton_Click_ForTestOnly(null, null);

				Assert(ZFormModaliser.LastFormShownDialogForTest is APCashAdvanceNewForm);
				AssertEquals("New Advance Payment", control.CashAdvanceButton_ForTestOnly.Text);
				AssertEquals(ODisplayMode.New, ((APCashAdvanceNewForm)ZFormModaliser.LastFormShownDialogForTest).DisplayMode);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest is ChargeWithCost);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest.IsInDatabase);
			}
		}

		public void TestCashAdvanceButtonWithInvalidCreditorOrNegativeAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge 01", TestObjectCreator.AUD, -200M, null, TestObjectCreator.AUD, 250M, TestObjectCreator.Debtor);

			Factory.Save();

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);
				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Advance Payment cannot be requested for negative charge amounts. Please enter a positive charge amount.", UnitTestUserNotification.Instance.LastMessage.Text);

				charge.JR_OSCostAmt = 0m;
				Factory.Save();
				control.JobChargeBoundGrid.Select(0);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Advance Payment cannot be requested until Creditor and OS Amount are specified. Please enter charge amount and Creditor code.", UnitTestUserNotification.Instance.LastMessage.Text);

				charge.JR_OSCostAmt = 200m;
				Factory.Save();
				control.JobChargeBoundGrid.Select(0);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("Advance Payment cannot be requested until Creditor and OS Amount are specified. Please enter charge amount and Creditor code.", UnitTestUserNotification.Instance.LastMessage.Text);

				charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				Factory.Save();

				control.JobChargeBoundGrid.Select(0);
				control.JobChargeBoundGrid_Click_ForTestOnly(null, null);
				AssertEquals("New Advance Payment", control.CashAdvanceButton_ForTestOnly.Text);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				Assert(ZFormModaliser.LastFormShownDialogForTest is APCashAdvanceNewForm);
				AssertEquals(ODisplayMode.New, ((APCashAdvanceNewForm)ZFormModaliser.LastFormShownDialogForTest).DisplayMode);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest is ChargeWithCost);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest.IsInDatabase);
			}
		}

		public void TestCashAdvanceButtonWithPostedCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 200m, 20m, 0m, 200m, 20m, 0m, TestObjectCreator.Creditor1);
			apInvoice.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			apInvoice.Lines.RemoveAndDeleteAll();
			var line = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "AP Line 001", 200m);
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			Assert(charge.IsCostPosted);

			Factory.Save();
			job.RefreshCharges();

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);
				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.JobChargeBoundGrid_Click_ForTestOnly(null, null);
				AssertEquals("View Advance Payment", control.CashAdvanceButton_ForTestOnly.Text);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);
				AssertEquals("This charge has been posted without an AP Advance Payment request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, TestObjectCreator.AALSHI.PK, LedgerTypes.AccountsPayable, 100m, 100m, TestObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge.JR_CAL_APLine = cashAdvanceLine.PK;
			cashAdvanceHeader.Lines.Add(cashAdvanceLine);
			Factory.Save();

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);
				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.JobChargeBoundGrid_Click_ForTestOnly(null, null);
				AssertEquals("View Advance Payment", control.CashAdvanceButton_ForTestOnly.Text);
				control.CashAdvanceButton_Click_ForTestOnly(null, null);

				Assert(ZFormModaliser.LastFormShownDialogForTest is APCashAdvanceViewForm);
				AssertEquals(ODisplayMode.ReadOnly, ((APCashAdvanceViewForm)ZFormModaliser.LastFormShownDialogForTest).DisplayMode);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest is CashAdvanceRequestHeader);
				Assert(ZFormModaliser.LastIBusinessShownOnDialogForTest.IsInDatabase);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestAutoPopulateWithoutSelectingValidJobCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);

			Factory.Save();

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);

				testForm.Show();

				control.AutoPopulateButton_Click_ForTestOnly(null, null);
				AssertEquals("Please select a charge line in the above listing of charges before pressing 'Populate AP Details'.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.JobChargeBoundGrid.Select(1);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);
				AssertEquals("Please select a charge line in the above listing of charges before pressing 'Populate AP Details'.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobAutoPopulationForm);
			}
		}

		public void TestAutoPopulateWithJobStatusIsJFC()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);

			Factory.Save();

			var oldValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);

				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);
				AssertEquals("You can't use Populate AP Details because Job has Ready For Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);

				control.Bind(job);
				control.Bind(null);

				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobAutoPopulationForm);
			}
		}

		public void TestStatusBarForJobExRate()
		{
			const string expectedBuyRateString = "Exchange Rate is based on Rate Type and Date Preference specified in Job Billing Exchange Rate Configuration (at Organization, Account Group, Company or System level).";
			const string expectedTodayRateString = "Exchange Rate is based on Rate Type and Date Preference specified in Job Billing Exchange Rate Configuration (at Organization, Account Group, Company or System level).";

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);

			Factory.Save();

			using (var testForm = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				testForm.Controls.Add(control);
				control.Dock = DockStyle.Fill;
				testForm.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(new Size(control.Size.Width, control.Size.Height), false) + CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(new Size(100, 0));
				control.Bind(job);

				testForm.Show();

				AssertStatusBarText("JF_BaseRate", expectedBuyRateString);
				AssertStatusBarText("JF_TodayRate", expectedTodayRateString);

				void AssertStatusBarText(string columnName, string expectedMessage)
				{
					for (var i = 0; i < control.JobExRateBoundGrid_ForTestOnly.Columns.Count; i++)
					{
						if (control.JobExRateBoundGrid_ForTestOnly.Columns[i].ColumnName == columnName)
						{
							control.JobExRateBoundGrid_ForTestOnly.CurrentCell = new DataGridCell(0, i);
							AssertEquals(expectedMessage, testForm.MessageStatusBarPanel.Text);
							break;
						}
					}
				}
			}
		}

		public void TestAutoPopulate()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "ABC";
			OrgHeader provider = Factory.New<OrgHeader>();
			provider.OH_Code = "PROV1";

			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			var mockPlugin = new Mock<IJobInvoicingPlugIn>();
			mockPlugin.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			SetupMockIJobInvoicingPlugin(mockPlugin, mockSupporter);
			mockPlugin.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			mockPlugin.Setup(m => m.Factory).Returns(Factory);
			mockPlugin.Setup(m => m.IsInDatabase).Returns(true);

			var parent = Factory.New<DummyJobParent>();
			Job job = new Job.Loader(mockPlugin.Object).TryCreateWithoutMutexForTestOnly();

			Charge ch = job.Charges.AddNew();
			ch.JR_AC = chargeCode1.PK;
			ch.JR_OH_CostAccount = provider.PK;
			ch.JR_RX_NKSellCurrency = "GBP";
			ch.JR_APInvoiceNum = "1111";
			ch.JR_APInvoiceDate = ZDateTime.Today;
			ch.JR_PaymentDate = ZDateTime.Today;
			ch.JR_APDocumentReceivedDate = ZDateTime.Today;
			ch.JR_GE = GlbDepartment.CurrentDepartment.PK;

			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();

			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);
				control.Bind(job);
				testForm.Show();
				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("You can't use Populate AP Details because Job has Ready to Post Cost status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			provider.CompanyData.OB_APCostsSelfBilled = true;
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);
				control.Bind(job);
				testForm.Show();
				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("You can't use Populate AP Details because the Creditor is flagged to receive Self Billing Invoices.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			provider.CompanyData.OB_APCostsSelfBilled = false;
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				testForm.Controls.Add(control);
				control.Bind(job);
				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);

				Assert(ZFormModaliser.LastFormShownDialogForTest is JobAutoPopulationForm);
				JobAutoPopulationForm form = (JobAutoPopulationForm)ZFormModaliser.LastFormShownDialogForTest;

				AssertEquals(ch.JR_OH_CostAccount, form.ParentJob.Creditor);
				AssertEquals(ch.JR_APInvoiceNum, form.ParentJob.InvoiceNum);
				AssertEquals(ch.JR_APInvoiceDate, form.ParentJob.InvoiceDate);
				AssertEquals(ch.JR_PaymentDate, form.ParentJob.InvoiceDueDate);
				AssertEquals(ch.JR_APDocumentReceivedDate, form.ParentJob.DocumentReceivedDate);
			}
		}

		public void TestAutoPopulate_WhenJobIsNull()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "ABC";
			OrgHeader provider = Factory.New<OrgHeader>();
			provider.OH_Code = "PROV1";

			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			var mockPlugin = new Mock<IJobInvoicingPlugIn>();
			mockPlugin.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			SetupMockIJobInvoicingPlugin(mockPlugin, mockSupporter);
			mockPlugin.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			mockPlugin.Setup(m => m.Factory).Returns(Factory);
			mockPlugin.Setup(m => m.IsInDatabase).Returns(true);

			var parent = Factory.New<DummyJobParent>();
			Job job = new Job.Loader(mockPlugin.Object).TryCreateWithoutMutexForTestOnly();
			Charge ch = job.Charges.AddNew();
			ch.JR_AC = chargeCode1.PK;
			ch.JR_OH_CostAccount = provider.PK;
			ch.JR_RX_NKSellCurrency = "GBP";
			ch.JR_APInvoiceNum = "1111";
			ch.JR_APInvoiceDate = ZDateTime.Today;
			ch.JR_PaymentDate = ZDateTime.Today;
			ch.JR_GE = GlbDepartment.CurrentDepartment.PK;

			// create a new parent with a different pk and set it to job.Parent so as to simulate the scenario where job is null in function AutoPopulateButton_Click(object sender, EventArgs e)
			parent = Factory.New<DummyJobParent>();
			job.Parent = parent;
			Factory.Save();
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Controls.Add(control);
				control.Bind(job);
				testForm.Show();
				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("This job information is not found", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoPopulateLoadsJobWithPluginData()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "ABC";
			OrgHeader provider = Factory.New<OrgHeader>();
			provider.OH_Code = "PROV1";
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTOR1";
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

			ZGuid pK = ZGuid.NewZGuid();

			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			var mockPlugin = new Mock<IJobInvoicingPlugIn>();
			mockPlugin.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			SetupMockIJobInvoicingPlugin(mockPlugin, mockSupporter);
			mockPlugin.Setup(m => m.PK).Returns(pK);
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.AgencyBillOfLading);
			mockSupporter.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			mockPlugin.Setup(m => m.Factory).Returns(Factory);
			mockPlugin.Setup(m => m.IsInDatabase).Returns(true);

			var parent = Factory.New<DummyJobParent>();
			Job job = new Job.Loader(mockPlugin.Object).TryCreateWithoutMutexForTestOnly();

			Charge ch = job.Charges.AddNew();
			ch.JR_AC = chargeCode1.PK;
			ch.JR_OH_CostAccount = provider.PK;
			ch.JR_RX_NKSellCurrency = "GBP";
			ch.JR_APInvoiceNum = "1111";
			ch.JR_APInvoiceDate = ZDateTime.Today;
			ch.JR_PaymentDate = ZDateTime.Today;
			ch.JR_OH_SellAccount = debtor.PK;
			ch.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			ch.JR_GE = GlbDepartment.CurrentDepartment.PK;

			AssertNoErrors("Precondition: Invoice Type shouldn't have any errors", ch.JR_InvoiceTypeInfo);

			Factory.Save();

			provider.CompanyData.OB_APCostsSelfBilled = false;
			using (ZForm testForm = new ZForm(job))
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				testForm.Controls.Add(control);
				control.Bind(job);
				testForm.Show();

				control.JobChargeBoundGrid.Select(0);
				control.AutoPopulateButton_Click_ForTestOnly(null, null);

				Assert(ZFormModaliser.LastFormShownDialogForTest is JobAutoPopulationForm);
				JobAutoPopulationForm form = (JobAutoPopulationForm)ZFormModaliser.LastFormShownDialogForTest;

				AssertNotNull("Job's Plugin Data", form.ParentJob.PlugInData);
				AssertEquals("Job's Plugin Data should be set", pK, form.ParentJob.PlugInData.PK);

				AssertEquals(ch.JR_OH_CostAccount, form.ParentJob.Creditor);
				AssertEquals(ch.JR_APInvoiceNum, form.ParentJob.InvoiceNum);
				AssertEquals(ch.JR_APInvoiceDate, form.ParentJob.InvoiceDate);
				AssertEquals(ch.JR_PaymentDate, form.ParentJob.InvoiceDueDate);

				AssertNoErrors("Invoice Type shouldn't have any errors", ch.JR_InvoiceTypeInfo);
			}
		}

		void SetupMockIJobInvoicingPlugin(Mock<IJobInvoicingPlugIn> mockPlugin, Mock<IJobInvoicingSupporter> mockSupporter)
		{
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDirectShipment).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.IsPlugInReadOnly).Returns(false);
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);
			mockSupporter.Setup(m => m.CreateAccountingJobOnSavingOfOperationsJob).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns(ZString.Empty);

			mockSupporter.Setup(m => m.ActualChargeable).Returns(ZDecimal.Zero);
			mockSupporter.Setup(m => m.ActualChargeableUnit).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ActualVolume).Returns(ZDecimal.Zero);
			mockSupporter.Setup(m => m.ActualVolumeUnit).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ActualWeight).Returns(ZDecimal.Zero);
			mockSupporter.Setup(m => m.ActualWeightUnit).Returns(ZString.Empty);

			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Now);
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Now);
			mockSupporter.Setup(m => m.Broker).Returns((OrgHeader)null);
			mockSupporter.Setup(m => m.Consignee).Returns((OrgHeader)null);
			mockSupporter.Setup(m => m.Consignor).Returns((OrgHeader)null);
			mockSupporter.Setup(m => m.ConsolExchangeRate).Returns(ZDecimal.Zero);
			// RefCurrency ConsolRateCurrency
			mockSupporter.Setup(m => m.ContainerCount).Returns(ZInt.Zero);
			mockSupporter.Setup(m => m.ContainerMode).Returns(ZString.Empty);

			mockSupporter.Setup(m => m.CreateAccountingJobOnSavingOfOperationsJob).Returns(false);
			mockSupporter.Setup(m => m.EditSecurityCheckpoint).Returns((SecurityCheckpoint)null);
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);
			mockSupporter.Setup(m => m.EditSecurityMessage).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.HouseBillNumber).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.JobInvoicingSecurity).Returns((SecurityCheckpoint)null);
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.MasterBillNumber).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ShipmentNumberOfColoadMaster).Returns(ZString.Empty);
			mockPlugin.Setup(m => m.JobNumber).Returns("12345678");
			mockPlugin.Setup(m => m.IsDeleted).Returns(false);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ShowOperationalJobRefFilter).Returns(false);
			mockSupporter.Setup(m => m.ServiceLevel).Returns("STD");
		}

		public void TestProcessDialogKey()
		{
			Job job = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (ZForm form = new ZForm(job))
			{
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(0, 0);

					//Cost
					SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, JobChargeSchema.Constants.JR_RX_NKCostCurrency);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("OS Cost Currency column should have focus", JobChargeSchema.Constants.JR_RX_NKCostCurrency, control.CurrentCellMappingName_ForTestOnly);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge tab control should have focus", control.ChargesDetailsTabControl.ContainsFocus);
					Assert("OS Cost currency find box should have focus", control.OSCostCurrencyCodeFindBox_ForTestOnly.ContainsFocus);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("OS Cost Currency column should have focus", JobChargeSchema.Constants.JR_RX_NKCostCurrency, control.ActiveControl.Name);

					//Revenue
					SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, JobChargeSchema.Constants.JR_RX_NKSellCurrency);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("Sell amount column should have focus", JobChargeSchema.Constants.JR_RX_NKSellCurrency, control.CurrentCellMappingName_ForTestOnly);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge tab control should have focus", control.ChargesDetailsTabControl.ContainsFocus);
					Assert("OS Sell amount calc edit should have focus", control.OSSellAmountCurrencyCodeFindBox_ForTestOnly.ContainsFocus);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("Sell amount column should have focus", JobChargeSchema.Constants.JR_RX_NKSellCurrency, control.ActiveControl.Name);

					//Common
					SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, Charge.Schema.JR_GB);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("Branch column should have focus", Charge.Schema.JR_GB, control.CurrentCellMappingName_ForTestOnly);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge tab control should have focus", control.ChargesDetailsTabControl.ContainsFocus);
					Assert("Branch find box should have focus", control.BranchesGuidFindBox_ForTestOnly.ContainsFocus);

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Job Charge Grid should have focus", control.JobChargeBoundGrid.ContainsFocus);
					AssertEquals("Department column should have focus", Charge.Schema.JR_GB, control.ActiveControl.Name);

					//When Only ChargeDetails has the focus, not any child control of it
					control.ChargesDetailsTabControl.Focus();
					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					Assert("Pressing F6 Should not throw any error becuase of no child Control of Details tab has the focus", control.ChargesDetailsTabControl.Focused);
				}
			}
		}

		public void TestChangingTabPageOnuncommittedRow()
		{
			Job job = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (ZForm form = new ZForm(job))
			{
				ZTextBox textBox = new ZTextBox();
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					form.Controls.Add(textBox);
					control.Bind(job);
					form.Show();
					control.JobChargeBoundGrid.Focus();
					AssertEquals("Focus on grid, should have one row", 1, control.JobChargeBoundGrid.ListManager.Count);

					textBox.Focus();
					AssertEquals("Focus off grid, should have zero rows", 0, control.JobChargeBoundGrid.ListManager.Count);

					control.JobChargeBoundGrid.Focus();
					AssertEquals("Focus on grid, should have one row", 1, control.JobChargeBoundGrid.ListManager.Count);
					AssertEquals("Should show correct tab page", control.DetailsTabPage_ForTestOnly, control.ChargesDetailsTabControl.SelectedTab);

					Charge chargeBefore = (Charge)control.JobChargeBoundGrid.ListManager.List[0];

					SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, Charge.Schema.JR_OSSellAmt);

					AssertEquals("Should show correct tab page", control.RevenueTabPage_ForTestOnly, control.ChargesDetailsTabControl.SelectedTab);
					AssertEquals("Changing tab page on uncommitted row should not remove Charge", 1, control.JobChargeBoundGrid.ListManager.Count);

					Charge chargeAfter = (Charge)control.JobChargeBoundGrid.ListManager.List[0];

					AssertEquals("Changing tab page on uncommitted row should not remove Charge", chargeBefore.PK, chargeAfter.PK);

					textBox.Focus();
					AssertEquals("Focus off grid, should have zero rows", 0, control.JobChargeBoundGrid.ListManager.Count);

					control.JobChargeBoundGrid.Focus();
					AssertEquals("Focus on grid, should have one row", 1, control.JobChargeBoundGrid.ListManager.Count);
					chargeBefore = (Charge)control.JobChargeBoundGrid.ListManager.List[0];

					control.ProcessDialogKey_ForTestOnly(Keys.F6);
					AssertEquals("Switching from grids to tabs on uncommitted row should not remove Charge", 1, control.JobChargeBoundGrid.ListManager.Count);
					chargeAfter = (Charge)control.JobChargeBoundGrid.ListManager.List[0];
					AssertEquals("Switching from grids to tabs on uncommitted row should not remove Charge", chargeBefore.PK, chargeAfter.PK);
				}
			}
		}

		public void TestJobChargeBoundGrid_CurrentCellChanged()
		{
			Job job = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (ZForm form = new ZForm(job))
			{
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(0, 0);

					AssertCorrectTabPageShown(control, JobChargeSchema.Constants.JR_RX_NKCostCurrency, control.CostTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, JobChargeSchema.Constants.JR_OSCostAmt, control.CostTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, JobChargeSchema.Constants.JR_LocalCostAmt, control.CostTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, JobChargeSchema.Constants.JR_OH_CostAccount, control.CostTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, BaseCharge.Schema.JR_IsCostPosted, control.CostTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, BaseCharge.Schema.JR_IsApportioned, control.CostTabPage_ForTestOnly);

					AssertCorrectTabPageShown(control, Charge.Schema.JR_RX_NKSellCurrency, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_OSSellAmt, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_LocalSellAmt, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_OH_SellAccount, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_CFXAmt, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_PreventInvoicePrintGrouping, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_InvoiceType, control.RevenueTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_IsRevenuePosted, control.RevenueTabPage_ForTestOnly);

					AssertCorrectTabPageShown(control, Charge.Schema.JR_AC, control.DetailsTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.ChargeType, control.DetailsTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.MarginPercentage, control.DetailsTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_Desc, control.DetailsTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_GB, control.DetailsTabPage_ForTestOnly);
					AssertCorrectTabPageShown(control, Charge.Schema.JR_GE, control.DetailsTabPage_ForTestOnly);
				}
			}
		}

		public void TestJobChargeBoundGrid_CurrentCellChanged_LogIndexOutOfRangeException()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.FilteredCharges.AddNew();

			var parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (var form = new ZForm(job))
			{
				using (var control = new JobChargeUserControl())
				using (new DisposableAction(
					() => control.JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly = JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly
				, () => control.JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly = null))
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(0, 0);
					SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, Charge.Schema.JR_RX_NKSellCurrency);

					CombineAssertions("record message when error", () =>
					{
						AssertEquals(ErrorReporter.LastKeyReported, "JobChargeBoundGrid_CurrentCellChanged_IndexOutOfRange");
						Assert(ErrorReporter.LastMessageReported.StartsWith("testing exception ,some list items may be changed during processing,tracing log below:"));
						Assert(ErrorReporter.LastMessageReported.Contains("ItemDeleted"));
						Assert(ErrorReporter.LastMessageReported.Contains("ItemAdded"));
						Assert("trace back which part cause error", ErrorReporter.LastMessageReported.Contains(nameof(TestJobChargeBoundGrid_CurrentCellChanged_LogIndexOutOfRangeException)));
					});
				}
				ErrorReporter.Clear();
			}

			void JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly(JobChargeUserControl sender, FilteredChargeCollection charges)
			{
				var addItem = job.FilteredCharges.AddNew();
				job.FilteredCharges.Remove(addItem);

				throw new IndexOutOfRangeException("testing exception");
			}
		}

		void AssertCorrectTabPageShown(JobChargeUserControl chargeControl, ZString columnName, ZTabPage expectedTab)
		{
			SetFocusOnColumnOnGrid(chargeControl.JobChargeBoundGrid, columnName);
			AssertEquals("Should show correct tab page", expectedTab, chargeControl.ChargesDetailsTabControl.SelectedTab);
		}

		void SetFocusOnColumnOnGrid(ZGrid parentGrid, ZString mappingName)
		{
			int columnNumber = 0;

			foreach (DataGridColumnStyle columnStyle in parentGrid.TableStyles[0].GridColumnStyles)
			{
				if (columnStyle.MappingName == mappingName)
				{
					parentGrid.CurrentCell = new DataGridCell(parentGrid.CurrentRowIndex, columnNumber);
					break;
				}
				columnNumber++;
			}

			Application.DoEvents();
		}

		public void TestSetJobChargeColumns()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.RevenueTabPage_ForTestOnly.Show();
				Application.DoEvents();
				ZGridColumnInfo info = control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CFXAmtReverseSign);
				AssertNull("CFX not enabled, should not contain column", info);
			}

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				ZGridColumnInfo info = control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CFXAmtReverseSign);
				AssertNotNull("CFX is enabled, should contain column", info);

				int expectedDecimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
				AssertHasCorrectDecimalPlaces(control.JobChargeBoundGrid, Charge.Schema.JR_LocalSellAmt, expectedDecimals);
				AssertHasCorrectDecimalPlaces(control.JobChargeBoundGrid, Charge.Schema.JR_LocalCostAmt, expectedDecimals);
				AssertHasCorrectDecimalPlaces(control.JobChargeBoundGrid, Charge.Schema.JR_CFXAmtReverseSign, expectedDecimals);
			}
		}

		public void TestSetExchangeRateColumns()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				int expectedDecimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				AssertHasCorrectDecimalPlaces(control.JobExRateBoundGrid_ForTestOnly, ExchangeRate.Schema.JF_BaseRate, expectedDecimals);
				AssertHasCorrectDecimalPlaces(control.JobExRateBoundGrid_ForTestOnly, ExchangeRate.Schema.JF_TodayRate, expectedDecimals);
			}
		}

		public void TestChargesGridColourDeciding()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_LocalSellAmt = 100m;
			var charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_LocalSellAmt = 100m;
			charge2.JR_JH = job2.PK;

			var chargeNoJob = Factory.New<Charge>();
			chargeNoJob.JR_AC = Env.Registry.FreightChargeCode;
			chargeNoJob.JR_LocalSellAmt = 110m;

			job2.JH_ParentID = shipment.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			job.Charges.IncludeChargesFromJobs_ForTestOnly(job2.PK);
			job.Charges.Load();

			using (var control = new JobChargeUserControl())
			{
				var args0 = new ColourDecidingEventArgs(charge1);
				control.JobChargeBoundGrid_ColourDeciding_ForTestOnly(this, args0);
				AssertEquals(Color.Empty, args0.Colour);

				control.Bind(job);

				var args1 = new ColourDecidingEventArgs(charge1);
				control.JobChargeBoundGrid_ColourDeciding_ForTestOnly(this, args1);
				AssertEquals(Color.Empty, args1.Colour);

				var args2 = new ColourDecidingEventArgs(charge2);
				control.JobChargeBoundGrid_ColourDeciding_ForTestOnly(this, args2);
				AssertEquals(Color.LightBlue, args2.Colour);

				var args3 = new ColourDecidingEventArgs(chargeNoJob);
				control.JobChargeBoundGrid_ColourDeciding_ForTestOnly(this, args3);
				AssertEquals(Color.Empty, args3.Colour);
			}
		}

		public void TestExRateGridColourDeciding()
		{
			using (var control = new JobChargeUserControl())
			{
				var rate = Factory.New<ExchangeRate>();

				rate.JF_BaseRate = rate.JF_TodayRate;
				var args1 = new ColourDecidingEventArgs(rate);
				control.JobExRateBoundGrid_ColourDeciding_ForTestOnly(this, args1);
				AssertEquals(Color.Empty, args1.Colour);

				rate.JF_BaseRate = rate.JF_TodayRate + 1;
				var args2 = new ColourDecidingEventArgs(rate);
				control.JobExRateBoundGrid_ColourDeciding_ForTestOnly(this, args2);
				AssertEquals(Color.Gold, args2.Colour);
			}
		}

		public void TestDeletedExRateGridColourDeciding()
		{
			using (var control = new JobChargeUserControl())
			{
				var rate = Factory.New<ExchangeRate>();
				rate.JF_BaseRate = rate.JF_TodayRate + 1;
				rate.Delete();
				var args1 = new ColourDecidingEventArgs(rate);
				AssertNoExceptionThrown(() => control.JobExRateBoundGrid_ColourDeciding_ForTestOnly(this, args1));
			}
		}

		void AssertHasCorrectDecimalPlaces(ZGrid grid, ZString schemaColumnName, int expectedDecimals)
		{
			ZCalcEditColumnStyleInfo info = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(schemaColumnName);
			AssertEquals("Should have correct number of decimal places", expectedDecimals, info.Decimals);
		}

		public void TestCurrencyTextBoxForAgentDeclaredCostAndSellAreReadOnly()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;
			JobCharge charge = job.Charges.AddNew();
			charge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Italy;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.France;

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();

				Assert("Should be read-only", control.AgentDeclaredCostCurrencyTextBox_ForTestOnly.ReadOnly);
				Assert("Should be read-only", control.AgentDeclaredSellCurrencyTextBox_ForTestOnly.ReadOnly);
				Assert("Should be read-only", control.LocalAgentDeclaredCostCurrencyTextBox_ForTestOnly.ReadOnly);
				Assert("Should be read-only", control.LocalAgentDeclaredSellCurrencyTextBox_ForTestOnly.ReadOnly);
				AssertEquals(Core.Constants.CurrencyCodes.Italy, control.AgentDeclaredCostCurrencyTextBox_ForTestOnly.Text);
				AssertEquals(Core.Constants.CurrencyCodes.France, control.AgentDeclaredSellCurrencyTextBox_ForTestOnly.Text);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, control.LocalAgentDeclaredSellCurrencyTextBox_ForTestOnly.Text);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, control.LocalAgentDeclaredCostCurrencyTextBox_ForTestOnly.Text);
			}
		}

		public void TestOverrideTransactionDescriptionMenuItem()
		{
			Env.Security.OverrideTransactionLineDescription.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToString());

			var shipment = testObjectCreator.CreateShipment("1001");
			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(shipment.InvoicingSupporter.JobInvoicingSecurity, SecurityCore.OverrideTransactionLineDescriptionForJob);
			securityCheckPoint.IsAllowed = true;

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.AgentCollectPK = TestObjectCreator.Agent.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var postedCharge = TestObjectCreator.CreateCharge(line);

			var unPostedCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "DESC", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);

			Factory.Save();

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				var otdItems = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("Override Transaction Line Description menu item is available", 1, otdItems.Count);
				var otdItem = otdItems[0];

				// Code to check why amnesty e.g. http://crikey.corporate.cargowise.com/UserTestResultsForm.aspx?UserTestPK=3b006315-c647-4cad-b241-aa09dee09173
				// is happening >>>
				var securityHelper = new JobInvoicingSecurityHelper(job.PlugInData.InvoicingSupporter.JobInvoicingSecurity);
				var jobSecurityForOverrideTxnLine = securityHelper.GetInvSecurity(SecurityCore.OverrideTransactionLineDescriptionForJob);

				var debugString = string.Format(@"Debug info: AllowOverridePostedTransactionDescriptionForJob : {0}
PlugInData == null : {1}
Parent == null : {2}
Parent is IJobInvoicingPlugIn : {3}
PlugInData.InvoicingSupporter.JobInvoicingSecurity.DisplayTextPathToSecurityRight : {4}
securityHelper.GetInvSecurity(SecurityCore.OverrideTransactionLineDescriptionForJob).IsAllowed : {5}
", job.AllowOverridePostedTransactionDescriptionForJob,
				job.PlugInData == null,
				job.Parent == null,
				job.Parent is IJobInvoicingPlugIn,
				job.PlugInData == null ? "" : job.PlugInData.InvoicingSupporter.JobInvoicingSecurity.DisplayTextPathToSecurityRight,
				jobSecurityForOverrideTxnLine.IsAllowed);
				// <<<

				// Nothing selected
				otdItem.PerformClick();
				AssertEquals(debugString, "Please select one or more charge lines", UnitTestUserNotification.Instance.LastMessage.Text);

				// Unposted selected
				chargesGrid.SelectSingleElement(unPostedCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				otdItem.PerformClick();
				AssertEquals("No suitable charge lines were selected. Please select lines with posted revenue.", UnitTestUserNotification.Instance.LastMessage.Text);

				// Posted selected
				chargesGrid.SelectSingleElement(postedCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				otdItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				var overridePopupForm = Application.OpenForms.Cast<Form>().Single(f => f is OverrideInvoiceLineDescriptionForm);
				AssertNotNull("An OverrideInvoiceLineDescriptionForm was opened", overridePopupForm);
				overridePopupForm.Close();
			}

			securityCheckPoint.IsAllowed = false;

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				var otdItems = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("Override Transaction Line Description menu item is available", 1, otdItems.Count);
				var otdItem = otdItems[0];

				// Nothing selected
				otdItem.PerformClick();
				AssertEquals(@"You do not have sufficient rights to override transaction line descriptions. You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Override Transaction Line Description", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}

			securityCheckPoint.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				var otdItems = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("No charge codes set up in registry so menu item hidden", 0, otdItems.Count);
				form.Close();
			}
		}

		public void TestAppendToUnpostedChargeDescription()
		{
			var shipment = TestObjectCreator.CreateShipment("1001");
			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(shipment.InvoicingSupporter.JobInvoicingSecurity, SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob);
			securityCheckPoint.IsAllowed = true;

			var job = TestObjectCreator.CreateJob(shipment);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = false;
			var sellpostedCharge = TestObjectCreator.CreateCharge(line);
			TestObjectCreator.CC2.AC_AllowDescriptionOvertype = false;
			var unPostedCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "DESC", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);
			TestObjectCreator.CC3.AC_AllowDescriptionOvertype = true;
			var unPostedCharge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "DESC2", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);

			var testInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1);
			var costPostedLine = TestObjectCreator.CreateInvoiceLine(testInvoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			costPostedLine.AL_LineType = TransactionLineTypes.Cost;
			costPostedLine.AL_JH = job.PK;
			costPostedLine.AL_AC = TestObjectCreator.CC4.PK;
			TestObjectCreator.CC4.AC_AllowDescriptionOvertype = false;
			var costPostedCharge = TestObjectCreator.CreateCharge(costPostedLine);

			Factory.Save();

			var invalidCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "DESC2", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);
			invalidCharge.JR_AC = ZGuid.Invalid;

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				var items = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Append Charge Line Description")).ToList();
				AssertEquals("Override Transaction Line Description menu item is available", 1, items.Count);
				var item = items[0];

				var securityHelper = new JobInvoicingSecurityHelper(job.PlugInData.InvoicingSupporter.JobInvoicingSecurity);
				var jobSecurityForOverrideTxnLine = securityHelper.GetInvSecurity(SecurityCore.OverrideTransactionLineDescriptionForJob);

				// Nothing selected
				item.PerformClick();
				AssertEquals("Please select one or more charge lines", UnitTestUserNotification.Instance.LastMessage.Text);

				// Sell Posted selected
				chargesGrid.SelectSingleElement(sellpostedCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("No suitable charge lines were selected. Please select unposted lines", UnitTestUserNotification.Instance.LastMessage.Text);

				// Cost Posted selected
				chargesGrid.SelectSingleElement(unPostedCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				// Unposted selected
				chargesGrid.SelectSingleElement(unPostedCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				// Charges that allow description override selected
				chargesGrid.SelectSingleElement(unPostedCharge2);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals(string.Format(@"Charges that allow description override cannot be selected. 

These Charges are: 
{0}

For these charges, you can simply override description as required.
Please revise selection.", unPostedCharge2.ChargeCode.AC_Code), UnitTestUserNotification.Instance.LastMessage.Text);

				//Invalid Charge
				chargesGrid.SelectSingleElement(invalidCharge);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				AssertEquals("No suitable charge lines were selected. Please select unposted lines", UnitTestUserNotification.Instance.LastMessage.Text);

				var popupForm = Application.OpenForms.Cast<Form>().Last(f => f is AppendToUnpostedChargeDescriptionForm);
				AssertNotNull("An OverrideInvoiceLineDescriptionForm was opened", popupForm);
				popupForm.Close();
			}

			securityCheckPoint.IsAllowed = false;

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				var items = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Append Charge Line Description")).ToList();
				AssertEquals("Override Transaction Line Description menu item is available", 1, items.Count);
				var item = items[0];

				// Nothing selected
				item.PerformClick();
				AssertEquals(@"You do not have sufficient rights to append transaction line descriptions. You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Append to Sell Unposted Charge Description", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}
		}

		[ExpectNoExceptions]
		public void TestAppendToUnpostedChargeDescriptionForDescChange()
		{
			var shipment = TestObjectCreator.CreateShipment("1001");
			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(shipment.InvoicingSupporter.JobInvoicingSecurity, SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob);
			securityCheckPoint.IsAllowed = true;

			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CC2.AC_AllowDescriptionOvertype = false;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Charge", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);

			Factory.Save();

			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.JobChargeBoundGrid;
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();
				Application.DoEvents();
				chargesGrid.SelectSingleElement(charge);
				var item = chargesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Append Charge Line Description")).ToList().First();
				AssertNotNull(item);
				item.PerformClick();

				var descForm = (AppendToUnpostedChargeDescriptionForm)ZFormModaliser.ActiveForm;
				var adapter = (ChargeDescriptionOverrideAdaptor)descForm.BusinessEntity;
				adapter.SelectedWrappedCharges[0].TextToAppend = "ABC";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				descForm.Close();

				TestObjectCreator.CC2.AC_AllowDescriptionOvertype = true;
				testObjectCreator.CC2.Factory.Save();
				((Charge)control.JobChargeBoundGrid.ListManager.List[0]).JR_Desc = "Char";

				testObjectCreator.CC2.AC_AllowDescriptionOvertype = false;
				item.PerformClick();

				var descForm1 = (AppendToUnpostedChargeDescriptionForm)ZFormModaliser.ActiveForm;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				descForm1.Close();
			}
		}

		public void TestCharacterCasingOfAPInvoiceNumInGrid()
		{
			var job = TestObjectCreator.Job1;
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				var apInvoiceNumTextBoxInfo = (ZTextBoxColumnStyleInfo)control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_APInvoiceNum);
				AssertNotNull(apInvoiceNumTextBoxInfo);
				AssertEquals(CharacterCasing.Upper, apInvoiceNumTextBoxInfo.CharacterCasing);
			}
		}

		public void TestHideIsApprovedWhenInvoiceApprovalEnabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var job = TestObjectCreator.Job1;
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert("JR_IsApproved.IsUnavailable", !control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsApproved).IsUnavailable);
			}

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert("JR_IsApproved.IsUnavailable", control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsApproved).IsUnavailable);
			}
		}

		public void TestInvoiceCurrencyTypeVisibility()
		{
			var job = TestObjectCreator.Job1;

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert("column Invoice currency type is removed", control.JobExRateBoundGrid.GetColumnStyle(ExchangeRate.Schema.JF_InvoiceCurrencyType).IsUnavailable);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert("column Invoice currency type is available", !control.JobExRateBoundGrid.GetColumnStyle(ExchangeRate.Schema.JF_InvoiceCurrencyType).IsUnavailable);
			}
		}

		public void TestGovtChargeCodeColumnsAreInAvailableColumnList()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var job = TestObjectCreator.Job1;
					using (var form = new ZForm(job))
					using (var control = new JobChargeUserControl())
					{
						form.Controls.Add(control);
						control.Bind(job);
						form.Show();
						AssertEquals(!enableGovtChargeCode, control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_SellGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CostGovtChargeCode).IsUnavailable);
					}
				}
			}
		}

		public void TestPlaceOfSupplyColumnsAreInAvailableColumnList()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				AssertPlaceOfSupplyColumnsAreInAvailableColumnList(Core.Constants.CountryCodes.India, true);
			}
			AssertPlaceOfSupplyColumnsAreInAvailableColumnList(Core.Constants.CountryCodes.Australia, false);
		}

		void AssertPlaceOfSupplyColumnsAreInAvailableColumnList(string countryCode, bool isColumnsAvailable)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var job = TestObjectCreator.Job1;
				using (var form = new ZForm(job))
				using (var control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					AssertEquals(isColumnsAvailable, !control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CostPlaceOfSupply).IsUnavailable);
					AssertEquals(isColumnsAvailable, !control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_SellPlaceOfSupply).IsUnavailable);
				}
			}
		}

		public void TestProfitShareColumnsAreInAvailableColumnList()
		{
			Job job = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			job.PlugInData = parent;

			using (ZForm form = new ZForm(job))
			{
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					Assert("Agent Declared Cost Amt", !control.JobChargeBoundGrid.Columns["JR_AgentDeclaredCostAmt"].IsUnavailable);
					Assert("Agent Declared Cost Amt Local", !control.JobChargeBoundGrid.Columns["JR_AgentDeclaredCostAmtLocal"].IsUnavailable);
					Assert("Agent Declared Sell Amt", !control.JobChargeBoundGrid.Columns["JR_AgentDeclaredSellAmt"].IsUnavailable);
					Assert("Agent Declared Sell Amt Local", !control.JobChargeBoundGrid.Columns["JR_AgentDeclaredSellAmtLocal"].IsUnavailable);
					Assert("Included In Profit Share", !control.JobChargeBoundGrid.Columns["JR_IsIncludedInProfitShare"].IsUnavailable);
				}
			}
		}

		public void TestJR_CostTaxDateControls()
		{
			var job = Factory.NewJobForTesting<Job>();
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.CostTabPage_ForTestOnly.Show();
				control.CostTabPage_ForTestOnly.Show();
				var costTaxDateControl = control.CostGSTPanel.Controls["JR_CostTaxDateEdit"];
				AssertNotNull(costTaxDateControl);
				AssertEquals(true, costTaxDateControl.Visible);
				var costTaxColumnInfo = control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CostTaxDate);
				AssertNotNull(costTaxColumnInfo);
				AssertEquals(false, costTaxColumnInfo.IsVisible);
			}
		}

		public void TestJR_SellTaxDateControls()
		{
			var job = Factory.NewJobForTesting<Job>();
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.RevenueTabPage_ForTestOnly.Show();
				var sellTaxDateControl = control.SellGSTPanel.Controls["SellTaxDateEdit"];
				AssertNotNull(sellTaxDateControl);
				AssertEquals(true, sellTaxDateControl.Visible);
				var sellTaxColumnInfo = control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_SellTaxDate);
				AssertNotNull(sellTaxColumnInfo);
				AssertEquals(false, sellTaxColumnInfo.IsVisible);
			}
		}

		public void TestRatingBehaviorColumnsVisible()
		{
			var job = Factory.NewJobForTesting<Job>();
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				var sellRatingBehaviorColumnInfo = control.JobChargeBoundGrid.GetColumnStyle("JR_Calc_SellRatingBehavior");
				var costRatingBehaviorColumnInfo = control.JobChargeBoundGrid.GetColumnStyle("JR_Calc_CostRatingBehavior");
				var costRatingOverrideCommentColumnInfo = control.JobChargeBoundGrid.GetColumnStyle("JR_CostRatingOverrideComment");

				AssertNotNull("Sell Rating Behavior Column should exist.", sellRatingBehaviorColumnInfo);
				AssertEquals("JR_Calc_SellRatingBehavior should not be visible by default.", false, sellRatingBehaviorColumnInfo.IsVisible);
				AssertNotNull("Cost Rating Behavior Column should exist.", costRatingBehaviorColumnInfo);
				AssertEquals("JR_Calc_CostRatingBehavior should not be visible by default.", false, costRatingBehaviorColumnInfo.IsVisible);
				AssertNotNull("Cost Rating Override Comment Column should exist.", costRatingOverrideCommentColumnInfo);
				AssertEquals("JR_CostRatingOverrideComment should not be visible by default.", false, costRatingOverrideCommentColumnInfo.IsVisible);
			}
		}

		public void TestJobChargeBoundGrid_CurrentCellChangedGetCurrentIsNull()
		{
			var job = Factory.NewJobForTesting<Job>();
			using (var form = new ZForm(job))
			{
				using (var control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					control.Bind(job);
					form.Show();
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(0, 0);
					control.JobChargeBoundGrid.IsListManagerNotNull = false;
					AssertEquals("Should show correct tab page", control.DetailsTabPage_ForTestOnly, control.ChargesDetailsTabControl.SelectedTab);
				}
			}
		}

		public void TestTaxTransactionAuditTabVisibility()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertTaxTransactionAuditTabVisibility(true);

			taxConfig1.Delete();
			Factory.Save();

			AssertTaxTransactionAuditTabVisibility(false);

			var taxConfig2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig2.ETC_ParentId = GlbCompany.CurrentCompany.FirstActiveBranch.PK;

			Factory.Save();

			AssertTaxTransactionAuditTabVisibility(true);
		}

		static void AssertTaxTransactionAuditTabVisibility(ZBool isVisible)
		{
			using (var form = new ZForm())
			{
				using (var control = new JobChargeUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var tabControl = control.Controls.Find("ChargesDetailsTabControl", true).First() as ZTabControl;
					var taxTransactionTabPage = tabControl.GetTabPage("TaxTransactionTabPage");
					if (!isVisible)
					{
						AssertNull("TaxTransactionTabPage", taxTransactionTabPage);
					}
					else
					{
						AssertNotNull("TaxTransactionTabPage", taxTransactionTabPage);
					}
				}
			}
		}

		public void TestSupplyTypeControls()
		{
			AssertSupplyTypeControls(true);
			AssertSupplyTypeControls(false);

			void AssertSupplyTypeControls(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					var job = TestObjectCreator.Job1;
					using (var form = new ZForm(job))
					using (var control = new JobChargeUserControl())
					{
						form.Controls.Add(control);
						control.Bind(job);
						form.Show();

						SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, Charge.Schema.JR_CostSupplyType);
						var costSupplyTypeDropEdit = control.Controls.Find("CostSupplyTypeDropEdit", true)[0];
						AssertEquals(enableSupplyTypeClassificationCodes, costSupplyTypeDropEdit.Visible);

						SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, Charge.Schema.JR_SellSupplyType);
						var sellSupplyTypeDropEdit = control.Controls.Find("SellSupplyTypeDropEdit", true)[0];
						AssertEquals(enableSupplyTypeClassificationCodes, sellSupplyTypeDropEdit.Visible);

						AssertEquals(!enableSupplyTypeClassificationCodes, control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CostSupplyType).IsUnavailable);
						AssertEquals(!enableSupplyTypeClassificationCodes, control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_SellSupplyType).IsUnavailable);
					}
				}
			}
		}

		#region SellComplianceDescription

		public void TestSellComplianceDescriptionColumnVisibility()
		{
			var job = Factory.NewJobForTesting<Job>();
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				var sellComplianceDescriptionColumnInfo = control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.SellComplianceDescription);

				AssertNotNull("Sell Compliance Description Column should exist.", sellComplianceDescriptionColumnInfo);
				AssertEquals("SellComplianceDescription should not be visible by default.", false, sellComplianceDescriptionColumnInfo.IsVisible);
			}
		}

		public void TestSellComplianceDescriptionColumnAvailabilityWhenRegistriesEnabledOrDiabled()
		{
			AssertSellComplianceDescriptionColumnAvailability(true, true, true);
			AssertSellComplianceDescriptionColumnAvailability(true, false, false);
			AssertSellComplianceDescriptionColumnAvailability(false, true, false);
			AssertSellComplianceDescriptionColumnAvailability(false, false, false);
		}

		void AssertSellComplianceDescriptionColumnAvailability(bool enableSupplyTypeCodesRegValue, bool enableSellCompDescRegValue, bool expectedDescriptionColumnIsAvailable)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeCodesRegValue))
			using (AccountingMasterFilesRegistry.Instance.EnableSellComplianceDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSellCompDescRegValue))
			{
				var job = Factory.NewJobForTesting<Job>();
				using (var control = new JobChargeUserControl())
				{
					control.Bind(job);

					AssertEquals("SellComplianceDescription column availability", expectedDescriptionColumnIsAvailable, !control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.SellComplianceDescription).IsUnavailable);
				}
			}
		}

		#endregion

		public void TestTaxBranchControls()
		{
			AssertTaxBranchControls(true, true, true);
			AssertTaxBranchControls(true, false, false);
			AssertTaxBranchControls(false, true, false);
			AssertTaxBranchControls(true, false, false);

			void AssertTaxBranchControls(bool enableTaxBranchReporting, bool isGSTRegistered, bool shouldEnableTaxBranchReportingControls)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
					var job = TestObjectCreator.Job1;
					using (var form = new ZForm(job))
					using (var control = new JobChargeUserControl())
					{
						form.Controls.Add(control);
						control.Bind(job);
						form.Show();

						var taxBranchGuidFindBox = control.Controls.Find("TaxBranchGuidFindBox", true)[0];
						AssertEquals(shouldEnableTaxBranchReportingControls, taxBranchGuidFindBox.Visible);

						SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, JobChargeSchema.JR_GB_CostTaxBranch.Name);
						var costSupplyTypeDropEdit = control.Controls.Find("CostTaxBranchGuidFindBox", true)[0];
						AssertEquals(shouldEnableTaxBranchReportingControls, costSupplyTypeDropEdit.Visible);

						SetFocusOnColumnOnGrid(control.JobChargeBoundGrid, JobChargeSchema.JR_GB_SellTaxBranch.Name);
						var sellSupplyTypeDropEdit = control.Controls.Find("SellTaxBranchGuidFindBox", true)[0];
						AssertEquals(shouldEnableTaxBranchReportingControls, sellSupplyTypeDropEdit.Visible);

						AssertEquals(!shouldEnableTaxBranchReportingControls, control.JobChargeBoundGrid.GetColumnStyle(JobChargeSchema.JR_GB_CostTaxBranch.Name).IsUnavailable);
						AssertEquals(!shouldEnableTaxBranchReportingControls, control.JobChargeBoundGrid.GetColumnStyle(JobChargeSchema.JR_GB_SellTaxBranch.Name).IsUnavailable);

						if (!shouldEnableTaxBranchReportingControls)
						{
							var salesRepFindBox = control.Controls.Find("SalesRepFindBox", true)[0];
							var operatorFindBox = control.Controls.Find("OperatorFindBox", true)[0];
							var jobLocalReferenceTextBox = control.Controls.Find("JobLocalReferenceTextBox", true)[0];
							AssertEquals(salesRepFindBox.Left, jobLocalReferenceTextBox.Left);
							AssertEquals(operatorFindBox.Left, jobLocalReferenceTextBox.Left);
						}
					}
				}
			}
		}

		public void TestHideAPCashAdvanceButton()
		{
			var job = Factory.NewJobForTesting<Job>();

			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.CostTabPage_ForTestOnly.Show();
				control.CostTabPage_ForTestOnly.Show();

				var cashAdvanceButton = control.CostTotalsPanel.Controls["CashAdvanceButton"];
				AssertNotNull(cashAdvanceButton);
				Assert(!cashAdvanceButton.Visible);
			}

			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.CostTabPage_ForTestOnly.Show();
				control.CostTabPage_ForTestOnly.Show();

				var cashAdvanceButton = control.CostTotalsPanel.Controls["CashAdvanceButton"];
				AssertNotNull(cashAdvanceButton);
				Assert(cashAdvanceButton.Visible);
			}
		}

		public void TestHideCashAdvanceRelatedGridColumns()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var job = TestObjectCreator.Job1;
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert(control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsARCashAdvance).IsUnavailable);
				Assert(control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ARCashAdvanceRequestStatus).IsUnavailable);
				Assert(control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ARCashAdvanceRequestID).IsUnavailable);
			}

			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZForm(job))
			using (var control = new JobChargeUserControl())
			{
				form.Controls.Add(control);
				control.Bind(job);
				form.Show();

				Assert(!control.JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsARCashAdvance).IsUnavailable);
				Assert(!control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ARCashAdvanceRequestStatus).IsUnavailable);
				Assert(!control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ARCashAdvanceRequestID).IsUnavailable);
			}
		}

		public void TestHideCashAdvanceRelatedTabFields()
		{
			var job = Factory.NewJobForTesting<Job>();

			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.RevenueTabPage_ForTestOnly.Show();
				var cashAdvanceRequiedCheckBox = control.CashAdvancePanel.Controls["cashAdvanceRequiedCheckBox"];
				AssertNotNull(cashAdvanceRequiedCheckBox);
				Assert(!cashAdvanceRequiedCheckBox.Visible);
				var cashAdvanceRequestIDTextBox = control.CashAdvancePanel.Controls["cashAdvanceRequestIDTextBox"];
				AssertNotNull(cashAdvanceRequestIDTextBox);
				Assert(!cashAdvanceRequestIDTextBox.Visible);
				var cashAdvanceRequestStatusTextBox = control.CashAdvancePanel.Controls["cashAdvanceRequestStatusTextBox"];
				AssertNotNull(cashAdvanceRequestStatusTextBox);
				Assert(!cashAdvanceRequestStatusTextBox.Visible);
			}

			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.RevenueTabPage_ForTestOnly.Show();
				var cashAdvanceRequiedCheckBox = control.CashAdvancePanel.Controls["cashAdvanceRequiedCheckBox"];
				AssertNotNull(cashAdvanceRequiedCheckBox);
				Assert(cashAdvanceRequiedCheckBox.Visible);
				var cashAdvanceRequestIDTextBox = control.CashAdvancePanel.Controls["cashAdvanceRequestIDTextBox"];
				AssertNotNull(cashAdvanceRequestIDTextBox);
				Assert(cashAdvanceRequestIDTextBox.Visible);
				var cashAdvanceRequestStatusTextBox = control.CashAdvancePanel.Controls["cashAdvanceRequestStatusTextBox"];
				AssertNotNull(cashAdvanceRequestStatusTextBox);
				Assert(cashAdvanceRequestStatusTextBox.Visible);
			}
		}

		#region Client Contracts

		public void TestClientContractLookupsConditionalVisibilityWhenDisplayIsTrue()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Assert("Precondition: ShouldDisplayClientContractNumber should be true making displayClientContractNumber true.", job.JobType.ShouldDisplayClientContractNumber(job.PlugInData));

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				AssertEquals("Client contract number button should be visible if registry item is enabled.", true, control.Controls.Find("ClientContractNumberButton", false).First().Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				AssertEquals("Client contract number button should not be visible if registry item is disabled.", false, control.Controls.Find("ClientContractNumberButton", false).First().Visible);
			}
		}

		public void TestClientContractLookupsConditionalVisibilityWhenDisplayIsFalse()
		{
			var job = Factory.NewJobForTesting<Job>();
			AssertNull("Precondition: JobType should be null making displayClientContractNumber false.", job.JobType);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				AssertEquals("Client contract number button should not be visible.", false, control.Controls.Find("ClientContractNumberButton", false).First().Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				AssertEquals("Client contract number button should not be visible.", false, control.Controls.Find("ClientContractNumberButton", false).First().Visible);
			}
		}

		public void TestJobNumberSetOnClose()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			var dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			using (ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory))
			{
				RunTestWithContractNumberButtonSetup(job, (button) =>
				{
					button.PerformClick();
					dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.SelectRow, "CCA001");
					AssertEquals("client contract number is set", "CCA001", job.JH_ClientContractNumber);
				});
			}
		}

		public void TestUpdateFilterSetOnListenerSetup()
		{
			ZDateTime depDate = new DateTime(2021, 1, 1);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_E_DEP = depDate;

			var orgHeader = Factory.New<IOrgHeader>();
			var orgAddress = Factory.New<IOrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = orgAddress.PK;
			job.JH_ClientContractNumber = "ExistingNum";

			var expectedMessageData = new ClientUpdateFilterData();
			expectedMessageData.startDate = depDate;
			expectedMessageData.expiryDate = depDate;
			expectedMessageData.contractID = "ExistingNum";
			expectedMessageData.clientPK = orgHeader.PK;
			var expectedMessage = new BrowserMessageEventArgs<ClientUpdateFilterData>
			{
				Kind = WebViewCommands.SetFilters,
				Payload = expectedMessageData
			};
			var expectedMessageAsStr = JsonConvert.SerializeObject(expectedMessage);

			var dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			using (ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory))
			{
				RunTestWithContractNumberButtonSetup(job, button =>
				{
					button.PerformClick();
					dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);
					dummyBrowserInteropWindowFactory.SendMessageToBrowserMock.Verify(v => v.SendToBrowserAsync(expectedMessageAsStr), "update filter should be called");
				});
			}

			Assert("not an empty test", true);
		}

		void RunTestWithContractNumberButtonSetup(Job job, Action<ZButton.Bare> test)
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"))
			using (var form = new ZForm())
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				form.Controls.Add(control);
				form.Show();

				test(control.ClientContractNumberButton_ForTestOnly);
			}
		}

		#endregion

		public void TestChargeGroupRelatedGridColumns()
		{
			var job = TestObjectCreator.Job1;
			using var form = new ZForm(job);
			using var control = new JobChargeUserControl();
			form.Controls.Add(control);
			control.Bind(job);
			form.Show();

			var chargeGroupColumnStyle = control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ChargeGroup);
			var chargeCodeSubGroupColumnStyle = control.JobChargeBoundGrid.GetColumnStyle(BaseCharge.Schema.ChargeCodeSubGroup);

			AssertNotNull(chargeGroupColumnStyle);
			AssertNotNull(chargeCodeSubGroupColumnStyle);

			AssertEquals("Charge Group", chargeGroupColumnStyle.CaptionResourceString.Caption);
			AssertEquals("Charge Code Sub Group", chargeCodeSubGroupColumnStyle.CaptionResourceString.Caption);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be available.", false, chargeGroupColumnStyle.IsUnavailable);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be available.", false, chargeCodeSubGroupColumnStyle.IsUnavailable);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be invisible for default.", false, chargeGroupColumnStyle.IsVisible);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be invisible for default.", false, chargeCodeSubGroupColumnStyle.IsVisible);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be read-only.", true, chargeGroupColumnStyle.IsReadOnly);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be read-only.", true, chargeCodeSubGroupColumnStyle.IsReadOnly);
		}

		public void TestAutoRateDescTextBoxFont()
		{
			var job = TestObjectCreator.Job1;

			using var form = new ZForm(job);
			using var control = new JobChargeUserControl();
			
			form.Controls.Add(control);
			control.Bind(job);
			form.Show();

			AssertEquals("Lucida Console", control.AutoRateDescCostTextBox_ForTestOnly.Font.Name);
			AssertEquals("Lucida Console", control.AutoRateDescRevenueTextBox_ForTestOnly.Font.Name);
		}
	}
}
