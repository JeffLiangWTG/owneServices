using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccComplianceReportWorkflowDescriptor))]
	public class AccComplianceReportWorkflowDescriptorTest : WorkflowDescriptorTestCase<AccComplianceReportWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.AccComplianceReportCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Accounting Compliance Report", WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			IWorkflowProvider report = GetParentsWithConfiguredOrganisationPartiesForTest()[0];
			ProcessTask processTask = report.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML;
			IProcessor resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode });
			Assert("Processor for SendUniversalComplianceReportXML should be IUniversalXmlWorkflowProcessor", resultProcessor is IUniversalXmlWorkflowProcessor);

			processTask = report.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));

			var dummyLogger = new Notifications();
			resultProcessor.Process(dummyLogger);
			AssertEquals("Universal Transaction is not supported by this job type.", dummyLogger.ToString());
		}

		class Notifications : INotifications
		{
			public void Add(INotification notification)
			{
				builder.Append(notification.Message);
			}

			readonly StringBuilder builder = new StringBuilder();

			public override string ToString() => builder.ToString();
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy; }
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 Sub Type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Report Type", "Report Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Without Report Types specified in Registry, list should be empty", 0, WorkflowDescriptor.SubTypeInformation[0].List.Count);

			var creator = new TestObjectCreator(Factory);
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_ReportType = "TST";
			creator.CreateConfigurationForComplianceReport(report1, "AL", "");
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = "TTT";
			creator.CreateConfigurationForComplianceReport(report2, "AL", "");

			var reportTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals("Report Types List Count", 2, reportTypeList.Count);

			AssertEquals("Code 0", report1.ACR_ReportType, reportTypeList[0].Code);
			AssertEquals("Description 0", report1.ACR_ReportType + " Report Title", reportTypeList[0].Description);
			AssertEquals("Code 1", report2.ACR_ReportType, reportTypeList[1].Code);
			AssertEquals("Description 1", report2.ACR_ReportType + " Report Title", reportTypeList[1].Description);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, creator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Sub Type 1 is Report Type", "Report Type", WorkflowDescriptor.SubTypeInformation[0].Description);
				AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

				AssertEquals("Without Report Types specified in Registry for the current Company, list should be empty", 0, WorkflowDescriptor.SubTypeInformation[0].List.Count);

				var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
				report3.ACR_ReportType = "TS3";
				creator.CreateConfigurationForComplianceReport(report3, "AL", "");

				reportTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
				AssertEquals("Report Types List Count", 1, reportTypeList.Count);

				AssertEquals("Code 0", report3.ACR_ReportType, reportTypeList[0].Code);
				AssertEquals("Description 0", report3.ACR_ReportType + " Report Title", reportTypeList[0].Description);
			}

			reportTypeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals("Report Types List Count", 2, reportTypeList.Count);
		}

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
			};
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return Factory.NewWithValidTestData<AccComplianceReport>();
		}

		#endregion
	}
}
