using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobRevenueJournalController))]
	class JobRevenueJournalControllerTest : AccountingTransactionControllerTest
	{
		public void TestGetForm_NoException_OnGetJobRevenueJournalForm()
		{
			var jobRevenueJournal = Factory.New<JobRevenueJournal>();
			var controller = new JobRevenueJournalController();

			ZForm form = null;
			AssertNoExceptionThrown(() => form = (ZForm)controller.ShowFormForNewEntity(jobRevenueJournal));
			form.Dispose();

			AssertType<JobRevenueJournalForm>("Postcondition: form type should be JobRevenueJournalForm", form);
		}

		public void TestGetForm_JournalHasClosedJobReopenerSet()
		{
			var jobRevenueJournal = Factory.New<JobRevenueJournal>();

			AssertNull("Precondition: ClosedJobReopener is null", jobRevenueJournal.ClosedJobReopener);

			var controller = new JobRevenueJournalController();

			using (var form = controller.ShowFormForNewEntity(jobRevenueJournal))
			{
				var closedJobReopener = jobRevenueJournal.ClosedJobReopener;

				AssertNotNull("JRJ ClosedJobReopener", closedJobReopener);
				AssertType<JobRevenueJournalReOpenClosedJobDataProvider>("ReOpenClosedJobDataProvider Type", ((ClosedJobReopener)closedJobReopener).ReOpenClosedJobDataProvider);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobRevenueJournal;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewJobRevenueJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewJobRevenueJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseJobRevenueJournal; }
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return Journal; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			Journal = Factory.New<JobRevenueJournal>();
			Factory.Save();
		}

		JobRevenueJournal Journal;
	}
}
