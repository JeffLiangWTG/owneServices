using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.CLE.OrdersDataImport.ServiceTasks.Testing
{
	[TestedType(typeof(CLEOrderDataImportServiceTask))]
	class CLEOrderDataImportServiceTaskTest : ServiceTaskTestCase<CLEOrderDataImportServiceTask>
	{
		public void TestRunTask()
		{
			var resourceRetriver = new EmbeddedResourceRetriever(GetType().Assembly);
			File.WriteAllBytes(TempDirectory + "\\tmp.csv", resourceRetriver.GetBytes(TestFile));
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Running Order Import from file tmp.csv"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Order 352326/46 created"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Order 352333/99 created"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Order 352333/99-1 created"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Order 352333/99-2 created"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Saving the data to the database..."));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Finish Import file tmp.csv"));
			AssertEquals(0, Directory.GetFiles(TempDirectory).Length);
		}

		public void TestRunTask_ImportCrapFile()
		{
			var resourceRetriver = new EmbeddedResourceRetriever(GetType().Assembly);
			File.WriteAllBytes(TempDirectory + "\\tmp.wtf", resourceRetriver.GetBytes(TestFile_Crap));
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Running Order Import from file tmp.wtf"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Error: Invalid file format tmp.wtf"));
			Assert(ServiceTask.GetBuffer().AsString.Contains("Finish Import file tmp.wtf"));
			AssertEquals(0, Directory.GetFiles(TempDirectory).Length);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Order Import Error", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("An error occurred while trying to import file tmp.wtf - Invalid file format (file attached)", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void SetValideRegistrySettings()
		{
			GlbGroup notifyGroup = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = notifyGroup.Staff.AddNew();
			staff.GS_Code = "P.T";
			staff.GS_EmailAddress = "blah@blah.com";
			Factory.Save();
			if (!Directory.Exists(TempDirectory))
			{
				Directory.CreateDirectory(TempDirectory);
			}

			DataTransferSwitchRegistryBusinessObject registry = new DataTransferSwitchRegistryBusinessObject();
			registry.EnableInterface = true;
			registry.Directory = TempDirectory;
			registry.NextRunDateTime = ZDateTime.Now; //only to suspend validation
			registry.Interval = 1; //only to suspend validation
			registry.GroupPK = notifyGroup.PK;
			CLEDataRegistry.Instance.SwitchOrderImportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);
			SystemDefinedOrganisation value = new UnmatchedOrganisation(Factory);
			value.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion
		#region Set Up
		readonly ZString TempDirectory = Path.Combine(Env.TempPath, "Orders");
		const string TestFile = "order-4splits.csv";
		const string TestFile_Crap = "order-exception.csv";
		readonly CLEOrderDataImportServiceTask ServiceTask = new();
		protected override void SetUpCore()
		{
			base.SetUpCore();
			InitialiseTaskSchedule(ServiceTask);
			SetValideRegistrySettings();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			CargoWise.IO.TempDirectory.DeleteDirectory(TempDirectory);
		}
		#endregion
	}
}
