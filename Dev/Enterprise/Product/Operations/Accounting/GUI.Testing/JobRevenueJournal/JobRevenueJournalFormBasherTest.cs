using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JobRevenueJournalForm))]
	public class JobRevenueJournalFormBasherTest : AccountingZFormBasherTest
	{
		public void TestJRJForm_Exception_WhenJRJFormPresentationProviderIsNull()
		{
			var ex = AssertExceptionThrown<ArgumentNullException>(() => new JobRevenueJournalForm(Factory.New<JobRevenueJournal>(), null));
			AssertEquals("jobRevenueJournalPresentationProvider", ex.ParamName);
		}

		public virtual void TestJournalWithCreateJobRevenueJournalViaBillingTabBusinessContext()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				AssertIfJobRevenueJournalForIsInDetailedEntryMode(form);
				Assert("JRJ created via job costing should not have CreateJobRevenueJournalViaInvoicingPlugin Context", !Factory.HasContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule));
			}
		}

		public void TestCurrencyBinding()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				TabControl mainTabControl = (TabControl)form.Controls["MainTabControl"];
				TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
				TabControl linesTabControl = (TabControl)detailsTabPage.Controls["LinesTabControl"];
				TabPage detailedTabPage = linesTabControl.TabPages["DetailedTabPage"];
				ZGrid linesGrid = (ZGrid)detailedTabPage.Controls["LinesGrid"];
				AssertNotNull("Currency column must be bound to AH_RX_NKTransactionCurrency field to correctly set exchange rate from job when new currency entered.",
					linesGrid.GetColumnStyle(JobRevenueJournalLine.Schema.AL_RX_NKTransactionCurrency));
			}
		}

		public virtual void TestActivateSimpleEntry()
		{
			using (JobRevenueJournalForm form = (JobRevenueJournalForm)GetFormToBash())
			{
				AssertIfJobRevenueJournalForIsInDetailedEntryMode(form);

				form.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());
				AssertIfJobRevenueJournalForIsInSimpleEntryMode(form);
			}
		}

		public static void AssertIfJobRevenueJournalForIsInSimpleEntryMode(JobRevenueJournalForm journalForm)
		{
			TabControl mainTabControl = (TabControl)journalForm.Controls["MainTabControl"];
			TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
			TabControl linesTabControl = (TabControl)detailsTabPage.Controls["LinesTabControl"];
			ZTabPage simpleEntryTabPage = (ZTabPage)linesTabControl.TabPages["SimpleEntryTabPage"];
			AssertNotNull("SimpleEntryTabPage is visible.", simpleEntryTabPage);
			TabPage detailedTabPage = linesTabControl.TabPages["DetailedTabPage"];
			ZGrid linesGrid = (ZGrid)detailedTabPage.Controls["LinesGrid"];
			AssertEquals("journal lines should be readonly", true, linesGrid.ReadOnly);
			Panel bottomPanel = (Panel)detailsTabPage.Controls["BottomPanel"];
			Button advancedEditModeButton = (ZButton)bottomPanel.Controls["AdvancedEditModeButton"];
			AssertEquals("advancedEditModeButton.Enabled", true, advancedEditModeButton.Enabled);
		}

		protected void AssertIfJobRevenueJournalForIsInDetailedEntryMode(JobRevenueJournalForm journalForm)
		{
			TabControl mainTabControl = (TabControl)journalForm.Controls["MainTabControl"];
			TabPage detailsTabPage = mainTabControl.TabPages["DetailsTabPage"];
			TabControl linesTabControl = (TabControl)detailsTabPage.Controls["LinesTabControl"];
			ZTabPage simpleEntryTabPage = (ZTabPage)linesTabControl.TabPages["SimpleEntryTabPage"];
			AssertNull("SimpleEntryTabPage is not visible.", simpleEntryTabPage);
			TabPage detailedTabPage = linesTabControl.TabPages["DetailedTabPage"];
			ZGrid linesGrid = (ZGrid)detailedTabPage.Controls["LinesGrid"];
			AssertEquals("journal lines should be readonly", false, linesGrid.ReadOnly);
			Panel bottomPanel = (Panel)detailsTabPage.Controls["BottomPanel"];
			Button advancedEditModeButton = (ZButton)bottomPanel.Controls["AdvancedEditModeButton"];
			AssertEquals("advancedEditModeButton.Enabled", false, advancedEditModeButton.Enabled);
		}

		public void TestJobRevenueJournalPrintPrompting()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0001", consol);
			Job job = TestObjectCreator.CreateJob(shipment);
			JobRevenueJournal revenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			Factory.Save();

			var mockJobRevenueJournalFormPresentationProvider = GetIJobRevenueJournalFormPresentationProviderMock();

			using (JobRevenueJournalFormForTest form = new JobRevenueJournalFormForTest(revenueJournal, mockJobRevenueJournalFormPresentationProvider.Object))
			{
				AccountingConfigurationRegistry.Instance.JobRevenueJournalPrintPrompting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Env.Security.PrintJobRevenueJournal.IsAllowed = true;
				form.ValidateAndSaveCalled();
				Assert("User should not be prompted to print job revenue journal", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

				AccountingConfigurationRegistry.Instance.JobRevenueJournalPrintPrompting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Env.Security.PrintJobRevenueJournal.IsAllowed = false;
				form.ValidateAndSaveCalled();
				Assert("User should not be prompted to print job revenue journal", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

				AccountingConfigurationRegistry.Instance.JobRevenueJournalPrintPrompting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Env.Security.PrintJobRevenueJournal.IsAllowed = true;
				form.ValidateAndSaveCalled();
				AssertEquals("User should be prompted to print job revenue journal", "Do you want to print Job Revenue Journal now?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJobRevenueJournalSave_Success_WhenPreSaveActionReturnsSuccess()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0001"));
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			Factory.Save();

			var mockJobRevenueJournalFormPresentationProvider = GetIJobRevenueJournalFormPresentationProviderMock();

			using (var form = new JobRevenueJournalFormForTest(jobRevenueJournal, mockJobRevenueJournalFormPresentationProvider.Object))
			{
				form.ValidateAndSaveCalled();

				Assert("User should not be prompted", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				mockJobRevenueJournalFormPresentationProvider.Verify(x => x.PreSaveActions());
			}
		}

		public void TestJobRevenueJournalSave_Failure_WhenPreSaveActionReturnsFailure()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0001"));
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			Factory.Save();

			var mockJobRevenueJournalFormPresentationProvider = GetIJobRevenueJournalFormPresentationProviderMock();
			mockJobRevenueJournalFormPresentationProvider.Setup(x => x.PreSaveActions()).Returns(PreSaveActionsResult.Failure("Posting requires JobReopening."));

			using (var form = new JobRevenueJournalFormForTest(jobRevenueJournal, mockJobRevenueJournalFormPresentationProvider.Object))
			{
				form.ValidateAndSaveCalled();

				AssertEquals("User should be prompted job", "Posting requires JobReopening.", UnitTestUserNotification.Instance.LastMessage.Text);
				mockJobRevenueJournalFormPresentationProvider.Verify(x => x.PreSaveActions());
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var mockJobRevenueJournalFormPresentationProvider = GetIJobRevenueJournalFormPresentationProviderMock();
			return new JobRevenueJournalForm(Factory.New<JobRevenueJournal>(), mockJobRevenueJournalFormPresentationProvider.Object);
		}

		Mock<IJobRevenueJournalFormPresentationProvider> GetIJobRevenueJournalFormPresentationProviderMock()
		{
			var mockJobRevenueJournalFormPresentationProvider = new Mock<IJobRevenueJournalFormPresentationProvider>();
			mockJobRevenueJournalFormPresentationProvider.Setup(x => x.PreSaveActions()).Returns(PreSaveActionsResult.Success());

			return mockJobRevenueJournalFormPresentationProvider;
		}

		#endregion

		public class JobRevenueJournalFormForTest : JobRevenueJournalForm
		{
			public JobRevenueJournalFormForTest(JobRevenueJournal bizO, IJobRevenueJournalFormPresentationProvider jobRevenueJournalFormPresentationProvider)
				: base(bizO, jobRevenueJournalFormPresentationProvider)
			{ }

			public void ValidateAndSaveCalled()
			{
				this.ValidateAndSave();
			}
		}
	}
}
