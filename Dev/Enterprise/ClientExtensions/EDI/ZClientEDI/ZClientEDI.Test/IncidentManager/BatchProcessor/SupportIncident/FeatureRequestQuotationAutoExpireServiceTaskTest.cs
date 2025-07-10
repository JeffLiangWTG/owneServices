using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ServiceTasks.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(FeatureRequestQuotationAutoExpireServiceTask))]
	class FeatureRequestQuotationAutoExpireServiceTaskTest : ServiceTaskTestCase<FeatureRequestQuotationAutoExpireServiceTask>
	{
		[TestDate(2014, 6, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "FQE Quotation Expired";
			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "QUO";
			task11.P9_Description = "Invoice Raised - Cancellation Fee";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2014, 7, 1, 12, 0, 0);
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, loadedIncident.IM_ResolutionCode);
			TestDateAttribute.Date = new DateTime(2014, 8, 1, 12, 0, 0);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => process.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		[TestDate(2014, 6, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "FQE Quotation Expired";
			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "QUO";
			task11.P9_Description = "Invoice Raised - Cancellation Fee";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2014, 7, 1, 12, 0, 0);
			process.RunTask();
			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, loadedIncident.IM_ResolutionCode);
			TestDateAttribute.Date = new DateTime(2014, 8, 1, 12, 0, 0);
			process.RunTask();
			loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.Status.Open, loadedIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FormalQuotationExpired, loadedIncident.IM_ResolutionCode);
			var task = loadedIncident.WorkflowItems.Find(t => t.P9_Description == "Invoice Raised - Cancellation Fee").First();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("QUO", task.P9_Type);
			AssertEquals(10, task.P9_Sequence);
			AssertEquals("Quotation Expired", task.ProcessHeader.FH_CompletionStatement);
			var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "No action from client for 20 days - quotation is expired"));
		}

		[TestDate(2014, 6, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTask_ExceptionShouldReportError()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "FQE Quotation Expired";
			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "QUO";
			task11.P9_Description = "Invoice Raised - Cancellation Fee";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			ErrorReporter.Clear();
			TestDateAttribute.Date = new DateTime(2014, 8, 1, 12, 0, 0);
			process.ServiceLogger = new ServiceLoggerForExceptionTest();
			process.RunTask();
			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Failed to process Feature Request Incident", ErrorReporter.LastKeyReported);
			AssertEquals(incident.IM_IncidentNumber + ' ' + ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override void SetUpCore()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INC");
			base.SetUpCore();
			process = new FeatureRequestQuotationAutoExpireServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
		}

		FeatureRequestQuotationAutoExpireServiceTask process;
		TestServiceLogger logger;
	}
}
