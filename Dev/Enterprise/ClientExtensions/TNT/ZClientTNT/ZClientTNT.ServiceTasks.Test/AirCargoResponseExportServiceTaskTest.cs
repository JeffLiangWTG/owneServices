using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Client.TNT.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	[TestedType(typeof(AirCargoResponseExportServiceTask))]
	class AirCargoResponseExportServiceTaskTest : TNTServiceTaskTest<AirCargoResponseExportServiceTask>
	{
		protected override void TestRunServiceTaskCore()
		{
			TNTDataRegistry.Instance.TNTReplyDirectory = ZString.Empty;
			TNTDataRegistry.Instance.CustomsStatusCode = new CodeDescriptionPairList();
			InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as registries are not set", true, ServiceTask.ServiceLogger.ToString().Contains(AirCargoResponseExportServiceTask.ErrorMessage));
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			TNTDataRegistry.Instance.TNTReplyDirectory = "abc";
			TNTDataRegistry.Instance.CustomsStatusCode = TestHelper.CreateCustomsStatusCode();
			ServiceTask.RunTask();
			AssertEquals("Logger should contain the error message as directory doesn't exist", true, ServiceTask.ServiceLogger.ToString().Contains(AirCargoResponseExportServiceTask.ErrorMessage));
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			TestHelper.SetupAirCargoResponseExportRegistries();
			ServiceTask.RunTask();
			AssertEquals("Logger should not contain the error message as all registries are set", true, !ServiceTask.ServiceLogger.ToString().Contains(AirCargoResponseExportServiceTask.ErrorMessage));
		}

		[TestDate(2005, 8, 22, 10, 30, 35)]
		public void TestExportData()
		{
			CusMAWB mawb = Factory.NewWithValidTestData<CusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			CusHAWB housebill = Factory.New<CusHAWB>();
			housebill.CS_CM = mawb.PK;
			housebill.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "QF168008196106301 TYOSYD050805A-TYO-HM3-SYD");
			housebill.CS_HAWB = "153630584";
			housebill.CS_CustomsStatus = "DOC";
			housebill.CS_TranshipmentEntryNum = "AAE6YLY6X";
			housebill.Logs.AddNew(Events.CustomsEntryStatus, "DOC");
			Factory.Save();
			NUnit.Framework.TestDateAttribute.Date = new System.DateTime(2005, 8, 22, 10, 35, 35);
			InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals("Reply Directory Should Have been created by the SendEDNReply Method", true, Directory.Exists(ReplyDirectory));
			string replyFile = Path.Combine(ReplyDirectory, "TIES20050822103535E.SYD" + QuantumFile.Extension);
			AssertEquals("Reply File Should Have been created by the SendEDNReply Method", true, File.Exists(replyFile));
			AssertEquals("Expected reply data", "153630584      TYO  HM3  AAE6YLY6XDOC  Y" + System.Environment.NewLine, File.ReadAllText(replyFile));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		protected override void TearDownCore()
		{
			base.TearDownCore();
			TestHelper.TidyUp();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			TestHelper.SetupAirCargoResponseExportRegistries();
			ReplyDirectory = TNTDataRegistry.Instance.TNTReplyDirectory;
		}

		ZString ReplyDirectory;
		AirCargoResponseExportServiceTask ServiceTask
		{
			get
			{
				return serviceTask ?? (serviceTask = new AirCargoResponseExportServiceTask());
			}
		}

		AirCargoResponseExportServiceTask serviceTask;
		TNTTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TNTTestHelper(Factory));
			}
		}

		TNTTestHelper testHelper;
	}
}
