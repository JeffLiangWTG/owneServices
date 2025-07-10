using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BatchProcessor
{
	public abstract class EmailReaderBatchProcess_Test : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var processor = GetEmailReaderProcessor();
			processor.ExecuteInternal();
			var mail = (MailItem)processor.MailFilter.Load(Factory, 1).Single();
			var log = $@"
Loaded batch of size [1]
Processing New Email, From : {mail.MI_From}, Subject : {mail.MI_Subject}
Processed batch of size [1]
Finished batch of size [1]";
			AssertMultilineASCIIEquals(log.Trim(), Buffer.AsString);
			AssertEmailIncludedInFilter(mail);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateMailItemsForEMailFilter();
		}

		protected abstract int NumberOfEmailsExpectedToBeProcessed { get; }
		protected abstract void CreateMailItemsForEMailFilter();
		protected abstract EmailReaderBatchProcess GetEmailReaderProcessor();
		protected abstract void AssertEmailIncludedInFilter(MailItem mail);
		protected abstract NotificationBuffer Buffer { get; }
	}
}
