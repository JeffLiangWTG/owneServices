using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.MFI.ServiceTasks.Testing
{
	[TestedType(typeof(DocumentAllocationServiceTask))]
	class DocumentAllocationServiceTaskTest : ServiceTaskTestCase<DocumentAllocationServiceTask>
	{
		public void TestRunTask()
		{
			// PLAY WITH MOCKS LATER
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.Notify.AsString.Contains("Called"));
			SetValidRegistry();
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.Notify.AsString.Contains("Called"));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void SetValidRegistry()
		{
			using (var testFileName = TempFile.New())
			{
				var testFile = new FileInfo(testFileName.Filename);
				string testDirectory = testFile.Directory.ToString();
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = testDirectory;
				MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory = testDirectory;
				var postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				var staff = postmasterGroup.Staff.AddNew();
				staff.GS_EmailAddress = "blah@blah.com";
				staff.GS_Code = "ZA";
				MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup = postmasterGroup.PK.ToGuid();
				Factory.Save();
			}
		}

		DocumentAllocationServiceTaskForTest ServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new DocumentAllocationServiceTaskForTest();
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
		}

		class DocumentAllocationServiceTaskForTest : DocumentAllocationServiceTask
		{
			protected override void Run(CancellationToken token)
			{
				Notify.Notify(new InfoNotification("Called"));
			}

			public new NotificationBuffer Notify
			{
				get
				{
					return base.Notify;
				}
			}
		}
		#endregion
	}
}
