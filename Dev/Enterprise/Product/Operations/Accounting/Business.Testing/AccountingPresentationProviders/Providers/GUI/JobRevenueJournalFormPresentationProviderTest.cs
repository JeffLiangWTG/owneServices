using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Moq;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class JobRevenueJournalFormPresentationProviderTest : TestCaseWithFactory
	{
		public void TestConstractorExceptionWhenClosedJobReopenerIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: closedJobReopener", () => new JobRevenueJournalFormPresentationProvider(null));
			AssertNoExceptionThrown(() => new JobRevenueJournalFormPresentationProvider(new Mock<IClosedJobReopener>().Object));
		}

		public void TestPreSaveAction_Success_WhenReopenClosedJobIsTrue()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			mockClosedJobReopener.Setup(x => x.ReopenClosedJobs()).Returns(true);
			IJobRevenueJournalFormPresentationProvider jobRevenueJournalFormPresentationProvider = new JobRevenueJournalFormPresentationProvider(mockClosedJobReopener.Object);

			var actualResult = jobRevenueJournalFormPresentationProvider.PreSaveActions();

			Assert(actualResult.CanProceed);
			Assert(!actualResult.HasError);
		}

		public void TestPreSaveAction_Failure_WhenReopenClosedJobIsFalse()
		{
			var mockClosedJobReopener = new Mock<IClosedJobReopener>();
			mockClosedJobReopener.Setup(x => x.ReopenClosedJobs()).Returns(false);
			IJobRevenueJournalFormPresentationProvider jobRevenueJournalFormPresentationProvider = new JobRevenueJournalFormPresentationProvider(mockClosedJobReopener.Object);

			var actualResult = jobRevenueJournalFormPresentationProvider.PreSaveActions();

			Assert(!actualResult.CanProceed);
			AssertEquals("Posting requires job reopening.", actualResult.ErrorMessage);
		}
	}
}
