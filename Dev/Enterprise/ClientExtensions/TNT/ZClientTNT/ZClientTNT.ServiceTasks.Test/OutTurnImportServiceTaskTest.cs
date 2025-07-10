using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Client.TNT.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Client.TNT.ServiceTasks.Testing.TNTTestFileUtils;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	[TestedType(typeof(OutTurnImportServiceTask))]
	class OutTurnImportServiceTaskTest : ServiceTaskTestCase<OutTurnImportServiceTask>
	{
		public void TestRunTask()
		{
			TNTDataRegistry.Instance.OutTurnFileSourceDirectory = ZString.Empty;
			TNTDataRegistry.Instance.QuantumFileProcessedDirectory = ZString.Empty;
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories are not set", true, ServiceTask.ServiceLogger.ToString().Contains(OutTurnImportServiceTask.ErrorMessage));
			TNTDataRegistry.Instance.OutTurnFileSourceDirectory = "ABC";
			TNTDataRegistry.Instance.OutTurnFileProcessedDirectory = "EFG";
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories do not exist", true, ServiceTask.ServiceLogger.ToString().Contains(OutTurnImportServiceTask.ErrorMessage));
			SetAllRegistries();
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger shouldn't contain the error message as directories are all set properly", true, !ServiceTask.ServiceLogger.ToString().Contains(OutTurnImportServiceTask.ErrorMessage));
		}

		public void TestImportValidFile()
		{
			CreateTestFile();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Procssed Directory has no Files", 0, ProcessedDirectory.GetFiles().Length);
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(ServiceTask);
			CusHAWB housebill = Factory.Load<CusHAWB>(HawbPK);
			housebill.Reload();
			var outturn = housebill.MAWB.Underbonds[0].Outturns[0];
			AssertEquals("Pieces Landed should have been updated", 5, outturn.C5_PackagesOutturned);
			AssertEquals("A new Directory was created int the Processed Directory", 1, ProcessedDirectory.GetDirectories().Length);
			AssertEquals("1 Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", TNTConstants.NotificationEmailSubjectForOutTurnFiles, email.Subject);
		}

		public void TestImportEmptyFile()
		{
			using (TempFile tempFile = TempFile.New(SourceDirectory.FullName, "out"))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals("Procssed Directory has no Files", 0, ProcessedDirectory.GetFiles().Length);
				InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
				scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
				RunTaskSchedule(ServiceTask);
				CusHAWB housebill = Factory.Load<CusHAWB>(HawbPK);
				AssertEquals("Pieces Landed should NOT have been updated", (short)4, housebill.CS_PiecesLanded);
				AssertEquals("A new Directory was created int the Processed Directory", 1, ProcessedDirectory.GetDirectories().Length);
				AssertEquals("1 Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email Subject", TNTConstants.NotificationEmailSubjectForOutTurnFiles, email.Subject);
			}
		}

		public void TestExecuteWithNoFiles()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(ServiceTask);
			CusHAWB housebill = Factory.Load<CusHAWB>(HawbPK);
			AssertEquals("Pieces Landed should NOT have been updated", (short)4, housebill.CS_PiecesLanded);
			AssertEquals("No File was moved to the Processed Directory", 0, ProcessedDirectory.GetDirectories().Length);
			AssertEquals("No Email(s) should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestExecuteHasWarnings()
		{
			CreateTestFile();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Procssed Directory has no Files", 0, ProcessedDirectory.GetFiles().Length);
			CusHAWB housebill = Factory.Load<CusHAWB>(HawbPK);
			housebill.Notes.RemoveAndDeleteAll();
			Factory.Save();
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(ServiceTask);
			AssertEquals("Pieces Landed should NOT and should still be 4", (short)4, housebill.CS_PiecesLanded);
			AssertEquals("A new Directory was created int the Processed Directory", 1, ProcessedDirectory.GetDirectories().Length);
			AssertEquals("1 Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", TNTConstants.NotificationEmailSubjectForOutTurnFiles, email.Subject);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		#region Setup
		string OriginalFile;
		EmbeddedResourceRetriever resourceRetriever;
		DirectoryInfo SourceDirectory;
		DirectoryInfo ProcessedDirectory;
		const string TestFile = "SYD.20050805.090432.out";
		OutTurnImportServiceTask ServiceTask
		{
			get
			{
				return serviceTask ?? (serviceTask = new OutTurnImportServiceTask());
			}
		}

		OutTurnImportServiceTask serviceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			SetAllRegistries();
			CreateCusMAWB();
			TestHelper.CreateNotificationGroupForSendingEmails();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			OriginalFile = resourceRetriever.SaveResourceToFile(GetDataImportExportFilePath(TestFile));
		}

		void SetAllRegistries()
		{
			SourceDirectory = CreateTestDirectory(Path.Combine(Env.TempPath, "Source"));
			ProcessedDirectory = CreateTestDirectory(Path.Combine(Env.TempPath, "Processed"));
			TNTDataRegistry.Instance.OutTurnFileSourceDirectory = SourceDirectory.FullName;
			TNTDataRegistry.Instance.OutTurnFileProcessedDirectory = ProcessedDirectory.FullName;
		}

		protected override void TearDownCore()
		{
			TempDirectory.DeleteDirectory(SourceDirectory.FullName);
			TempDirectory.DeleteDirectory(ProcessedDirectory.FullName);
			base.TearDownCore();
			resourceRetriever.Dispose();
		}

		void CreateTestFile()
		{
			ZString testFileFullPath = Path.Combine(SourceDirectory.FullName, TestFile);
			File.Copy(OriginalFile, testFileFullPath, true);
			File.SetAttributes(testFileFullPath, FileAttributes.Normal);
		}

		TNTTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TNTTestHelper(Factory));
			}
		}

		TNTTestHelper testHelper;
		DirectoryInfo CreateTestDirectory(string path)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			if (directoryInfo.Exists)
			{
				TempDirectory.DeleteDirectory(directoryInfo.FullName);
			}

			directoryInfo.Create();
			return directoryInfo;
		}

		void CreateCusMAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB.CM_MAWB = "08196106301";
			mAWB.CM_RL_NKDischargePort = "SYD";
			mAWB.CM_RL_NKLoadPort = "TYO";
			mAWB.CM_FlightNo = "QF1680";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 8, 05);
			CusHAWB hAWB = Factory.New<CusHAWB>();
			hAWB.CS_PiecesManifested = (short)6;
			hAWB.CS_PiecesLanded = (short)4;
			hAWB.CS_RL_NKOrigin = "TYO";
			hAWB.CS_RL_NKDestination = "HM3";
			hAWB.CS_HAWB = "153630584";
			StmNote note = hAWB.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "QF168008196106301 TYOSYD050805A");
			hAWB.CS_CM = mAWB.PK;
			HawbPK = hAWB.PK;
			Factory.Save();
		}

		ZGuid HawbPK;
		#endregion
	}
}
