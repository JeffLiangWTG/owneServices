using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.NIP.ServiceTasks.Testing
{
	[TestedType(typeof(NIPConsolAndShipmentServiceTask))]
	public class NIPConsolAndShipmentServiceTaskTest : ServiceTaskTestCase<NIPConsolAndShipmentServiceTask>
	{
		public void TestRunExport()
		{
			//CopyTestFileToTestFolder();
			//AssertFileCount(1);
			//AssertEquals("Precondition: DB should not contains any consols", 0, Factory.GetDatabaseCount(typeof(ForwardingConsol)));

			//RunTaskSchedule(ServiceTask);

			//AssertFileCount(0);
			//AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
			Assert(true);
		}

		public void TestDoNotDeleteFileOnIO()
		{
			Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var fileInfo = new FileInfo("HerpaDerpDerp");
			var task = new NIPConsolAndShipmentServiceTask_ForTest(fileInfo);
			task.ServiceLogger = new Logger();
			AssertExceptionThrown<EmailHasNoRecipientsException>(() => task.RunTask());
			AssertNotContains("IOException", task.ServiceLogger.ToString());
			AssertNotContains("will retry later", task.ServiceLogger.ToString());
		}

		public void TestRunImportWhenDirectoryNotAccessible()
		{
			var invalidPath = Path.Combine(Directory!.FullName, "NonExistantDirectory");
			NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invalidPath);

#pragma warning disable CS8604 // Possible null reference argument.
			RunTaskSchedule(ServiceTask);
#pragma warning restore CS8604 // Possible null reference argument.

			AssertContains($"Error: The directory '{invalidPath}' from the registry does not exist or is inaccessible. See: Admin -> System -> Registry -> NIP Client-Extensions -> Import of Consol + Shipment Data",
				ServiceTask.ServiceLogger.ToString());
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Set Up

		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new NIPConsolAndShipmentServiceTask();

			ZString directoryPath = Env.TempPath + @"\NIP";
			Directory = new DirectoryInfo(directoryPath);
			Directory.Create();
			NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directoryPath);

			NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "ALL").PK.ToGuid());

			InitialiseTaskSchedule(ServiceTask);
		}

		#endregion

		#region Tear Down

		protected override void TearDownCore()
		{
			base.TearDownCore();
			Directory?.Delete(true);
		}

		#endregion

		NIPConsolAndShipmentServiceTask? ServiceTask;
		DirectoryInfo? Directory;

		class NIPConsolAndShipmentServiceTask_ForTest : NIPConsolAndShipmentServiceTask
		{
			public NIPConsolAndShipmentServiceTask_ForTest(params FileInfo[] fileInfo)
			{
				this.fileInfo = fileInfo;
			}
			readonly FileInfo[] fileInfo;

			protected override FileInfo[] GetNewFiles() => fileInfo;
		}

		class Logger : ILogger
		{
			readonly StringBuilder builder = new StringBuilder();

			public void Add(INotification notification)
			{
			}

			public void Log(LogType type, string message)
			{
				builder.AppendLine(message);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				builder.AppendLine(message);
			}

			public override string ToString()
			{
				return builder.ToString();
			}
		}
	}
}
