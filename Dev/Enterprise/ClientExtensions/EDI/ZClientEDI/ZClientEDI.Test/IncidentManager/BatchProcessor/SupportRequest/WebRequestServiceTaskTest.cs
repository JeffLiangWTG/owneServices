using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(WebRequestServiceTask))]
	class WebRequestServiceTaskTest : ServiceTaskTestCase<WebRequestServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new WebRequestServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestServiceTaskCanRunInAnyBranch_ProcessAllNewWebMessages()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.AddNew();
			contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant.JCP_ParticipantID = reportingContact.PK;
			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			var legacyMsg1 = convo.Messages.AddNew(contactParticpant, "legacyMsg1", false);
			legacyMsg1.JCM_IsLocal = false;
			var legacy1 = Factory.New<EdiLegacyConversationMessage>();
			legacy1.ELC_JCM_Message = legacyMsg1.PK;

			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;

			var staffMsg1 = convo.Messages.AddNew(staffParticpant, "staffMsg1", false);
			staffMsg1.JCM_IsLocal = false;

			Factory.Save();

			AssertEquals("count of new remote messages in queue", 2, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new WebRequestServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
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

		public void TestHostedServiceBusinessObjectBindingAttribute()
		{
			var matchCount1 = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().Count(x => x.ServiceTaskCode == "WRQ" && x.Table == IncidentRequestSchema.Constants.TableName && x.ClientSpecificCode == Clients.EDI);
			var matchCount2 = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().Count(x => x.ServiceTaskCode == "WRQ" && x.Table == JobConversationMessageSchema.Constants.TableName && x.ClientSpecificCode == Clients.EDI);
			var matchCount3 = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().Count(x => x.ServiceTaskCode == "WRQ" && x.Table == EdiERequestDocumentQueueSchema.Constants.TableName && x.ClientSpecificCode == Clients.EDI);
			AssertEquals(5, matchCount1);
			AssertEquals(1, matchCount2);
			AssertEquals(1, matchCount3);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override void SetUpCore()
		{
			base.SetUpCore();
			process = new WebRequestServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
		}

		WebRequestServiceTask process;
		TestServiceLogger logger;
	}
}
