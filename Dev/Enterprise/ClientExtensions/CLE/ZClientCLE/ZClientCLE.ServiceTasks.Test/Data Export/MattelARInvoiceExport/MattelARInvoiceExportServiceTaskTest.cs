using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.ServiceTasks.Testing
{
	[TestedType(typeof(MattelARInvoiceExportServiceTask))]
	class MattelARInvoiceExportServiceTaskTest : ServiceTaskTestCase<MattelARInvoiceExportServiceTask>
	{
		public void TestRunTask()
		{
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Mattel Data Export Email Address is not set up or not valid"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Clemenger Email Address is not set up or not valid"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Mattel Data Export Email Subject is not set up"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Mattel Debtor is not set up"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Mattel Mailbox Number is not set up"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("The Clemenger Mailbox Number is not set up"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Mattel AR invoices data export begins"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Mattel AR invoices data export completed"));
			ServiceTask.GetBuffer().Clear();
			SetCalidRegistrySettings();
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Mattel Data Export Email Address is not set up or not valid"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Clemenger Email Address is not set up or not valid"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Mattel Data Export Email Subject is not set up"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Mattel Debtor is not set up"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Mattel Mailbox Number is not set up"));
			Assert(!ServiceTask.GetBuffer().AsString.Contains("The Clemenger Mailbox Number is not set up"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Mattel AR invoices data export begins"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Mattel AR invoices data export completed"));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(MattelARInvoiceExportServiceTask.Code);
			AssertNull("No queue table for this service task; WHERE clause in MattelARInvoiceExporter is non-trivial; task is infrequently used.", queueProvider);
		}

		#region Implementation
		void SetCalidRegistrySettings()
		{
			CLEDataRegistry.Instance.MattelEmailAddress = "test@mattel.com";
			CLEDataRegistry.Instance.ClemengerEmailAddress = "test@clemenger.com";
			CLEDataRegistry.Instance.MattelEmailSubject = "mattel subject";
			CLEDataRegistry.Instance.MattelDebtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			CLEDataRegistry.Instance.MattelMailboxNumber = "1111";
			CLEDataRegistry.Instance.ClemengerMailboxNumber = "2222";
		}

		#endregion
		#region Overridea
		readonly MattelARInvoiceExportServiceTask ServiceTask = new();
		protected override void SetUpCore()
		{
			base.SetUpCore();
			InitialiseTaskSchedule(ServiceTask);
		}
		#endregion
	}
}
