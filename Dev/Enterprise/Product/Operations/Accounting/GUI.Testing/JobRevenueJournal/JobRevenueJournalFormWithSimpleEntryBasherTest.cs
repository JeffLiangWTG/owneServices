using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JobRevenueJournalForm))]
	public class JobRevenueJournalFormWithSimpleEntryBasherTest : JobRevenueJournalFormBasherTest
	{
		public override void TestJournalWithCreateJobRevenueJournalViaBillingTabBusinessContext()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				form.Show();
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);
				Assert("JRJ created via billing tab should have CreateJobRevenueJournalViaInvoicingPlugin Context", Factory.HasContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule));
				form.Close();
				Assert(!form.Visible);
				Assert("CreateJobRevenueJournalViaInvoicingPlugin Context shoulde be removed when form is closed", !Factory.HasContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule));
			}
		}

		public override void TestActivateSimpleEntry()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);
			}
		}

		[TestDate(2011, 02, 18)]
		public void TestAdvancedEditModeButtonReadOnlyAfterSave()
		{
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			Factory.Save();

			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				TabControl mainTabControl = (TabControl)form.Controls["MainTabControl"];
				TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
				Panel bottomPanel = (Panel)detailsTabPage.Controls["BottomPanel"];
				Button advancedEditModeButton = (ZButton)bottomPanel.Controls["AdvancedEditModeButton"];
				AssertEquals("Precondition: AdvancedEditModeButton should be enabled.", true, advancedEditModeButton.Enabled);

				JobRevenueJournal journal = (JobRevenueJournal)form.BusinessEntity;
				journal.BranchPKTo = journal.BranchPKFrom;
				journal.DepartmentPKTo = journal.DepartmentPKFrom;
				journal.DefaultSharing = 10M;
				journal.AH_PostDate = ZDateTime.Now;
				JobRevenueJournalCharge journalCharge = journal.JournalCharges.AddNew();
				journalCharge.ChargeCode = Env.Registry.FreightChargeCode;
				journalCharge.BillingTabOsAmount = 100M;

				ContinueWithSave result = form.FireSaveButton();
				AssertEquals("Precondition: The form should be saved.", ContinueWithSave.Yes, result);

				AssertEquals("AdvancedEditModeButton should be disabled after saving.", false, advancedEditModeButton.Enabled);
			}
		}

		public void TestAdvancedEditModeButton_Click_UserAnsverYes()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);

				TabControl mainTabControl = (TabControl)form.Controls["MainTabControl"];
				TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
				Panel bottomPanel = (Panel)detailsTabPage.Controls["BottomPanel"];
				Button advancedEditModeButton = (ZButton)bottomPanel.Controls["AdvancedEditModeButton"];

				JobRevenueJournal journal = (JobRevenueJournal)form.BusinessEntity;
				journal.JournalCharges.AddNew();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals("Precondition: journal.JournalCharges.Count", 1, journal.JournalCharges.Count);
				advancedEditModeButton.PerformClick();
				Application.DoEvents();
				AssertEquals(
@"You are trying to switch to advanced edit mode where you can add journal lines directly. Tab for simplified entry will be closed.
Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("journal.JournalCharges.Count", 0, journal.JournalCharges.Count);
				AssertIfJobRevenueJournalForIsInDetailedEntryMode(form);
			}
		}

		public void TestAdvancedEditModeButton_Click_UserAnsverNo()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);

				TabControl mainTabControl = (TabControl)form.Controls["MainTabControl"];
				TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
				Panel bottomPanel = (Panel)detailsTabPage.Controls["BottomPanel"];
				Button advancedEditModeButton = (ZButton)bottomPanel.Controls["AdvancedEditModeButton"];

				JobRevenueJournal journal = (JobRevenueJournal)form.BusinessEntity;
				journal.JournalCharges.AddNew();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				AssertEquals("Precondition: journal.JournalCharges.Count", 1, journal.JournalCharges.Count);
				advancedEditModeButton.PerformClick();
				Application.DoEvents();
				AssertEquals(
@"You are trying to switch to advanced edit mode where you can add journal lines directly. Tab for simplified entry will be closed.
Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("journal.JournalCharges.Count", 1, journal.JournalCharges.Count);
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);

			JobRevenueJournalForm form = (JobRevenueJournalForm)base.GetFormToBashCore();
			form.ActivateSimpleEntry(job);

			return form;
		}
	}
}
