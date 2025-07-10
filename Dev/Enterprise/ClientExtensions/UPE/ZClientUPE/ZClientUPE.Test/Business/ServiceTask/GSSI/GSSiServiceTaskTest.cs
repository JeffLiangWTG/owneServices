using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.GSSI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.ServiceTask.GSSi.Testing
{
	[TestedType(typeof(GSSiServiceTask))]
	class GSSiServiceTaskTest : ServiceTaskTestCase<GSSiServiceTask>
	{
		public void TestExecuteBatchWithNoErrors()
		{
			var m1 = Factory.New<GSSMessage>();
			m1.EM_MessageText = "Text1";
			var m2 = Factory.New<GSSMessage>();
			m2.EM_MessageText = "Text2";
			Factory.Save();

			BatchProcessor.SuccessfulSend = true;
			BatchProcessor.RunTask();

			// logging is not completely tested...
			Assert(Logger.ToString().Contains("Failed upload of "));

			AssertEquals("SNT", m1.EM_Status);
			AssertEquals("SNT", m2.EM_Status);
		}

		public void TestExecuteBatchWithErrors()
		{
			var m1 = Factory.New<GSSMessage>();
			var m2 = Factory.New<GSSMessage>();
			Factory.Save();

			BatchProcessor.SuccessfulSend = false;
			BatchProcessor.RunTask();

			Assert(Logger.ToString().Contains("Failed upload of "));

			AssertEquals("QUE", m1.EM_Status);
			AssertEquals("QUE", m2.EM_Status);
		}

		public void TestExecuteValidBranch()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;

			var m1 = Factory.New<GSSMessage>();
			m1.EM_MessageText = "Text1";
			var m2 = Factory.New<GSSMessage>();
			m2.EM_MessageText = "Text2";
			Factory.Save();

			BatchProcessor.SuccessfulSend = true;
			BatchProcessor.RunTask();

			AssertEquals(0, Logger.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			BatchProcessor.RunTask();

			AssertGreaterThan(Logger.Count, 0);
		}

		public void TestExecute_Timeout()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroup = Factory.LoadTop1<GlbGroup>(new ZQuery()).PK.ToGuid();
			Factory.New<GSSMessage>();
			Factory.Save();

			UPEDataRegistry.Instance.SftpServerTimeout = 5;
			new GSSiServiceTaskForTestTimeout().RunTask();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Service Task 'ZU4' has timed out", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"GSSi messages outbound",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=GSS",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y"),
				};
			}
		}

		#region Set Up

		GSSiServiceTaskForTest BatchProcessor => batchProcessor ?? (batchProcessor = new GSSiServiceTaskForTest { ServiceLogger = Logger });
		GSSiServiceTaskForTest batchProcessor;

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "pavlo.tubolets@cargowise.com";
			staff.GS_IsActive = true;
			staff.GS_Code = "ZAC";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff);
			Factory.Save();

			base.SetUpCore();
		}

		#endregion

		#region Tear Down

		protected override void TearDownCore()
		{
			List<FileInfo> files = new List<FileInfo>();
			DirectoryInfo directoryPath = new DirectoryInfo(Env.TempPath);
			files.AddRange(directoryPath.GetFiles("*.txt"));
			foreach (FileInfo file in files)
			{
				file.Delete();
			}

			base.TearDownCore();
		}

		#endregion

		#region Implementation
		public class GSSiServiceTaskForTest : GSSiServiceTask
		{
			public GSSiServiceTaskForTest()
				: base(LoggerTest)
			{
			}

			public bool SuccessfulSend;
			static TestServiceLogger LoggerTest => logger ?? (logger = new TestServiceLogger());

			protected override bool UploadMessage(ZString fileName, ZString firstMessageNumber, ZString lastMessageNumber)
			{
				AssertEquals("Message sending should not work at this point", false, base.UploadMessage(fileName, firstMessageNumber, lastMessageNumber));
				return SuccessfulSend;
			}

			static TestServiceLogger logger;
		}

		public class GSSiServiceTaskForTestTimeout : GSSiServiceTaskForTest
		{
			protected override bool UploadMessage(ZString fileName, ZString firstMessageNumber, ZString lastMessageNumber)
			{
				Thread.Sleep(10000);
				return true;
			}
		}

		#endregion
	}
}
