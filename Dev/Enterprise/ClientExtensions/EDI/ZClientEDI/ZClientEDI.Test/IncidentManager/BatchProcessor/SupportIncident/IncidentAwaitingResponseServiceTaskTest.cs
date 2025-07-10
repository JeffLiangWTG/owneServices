using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.ServiceTasks.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(IncidentAwaitingResponseServiceTask))]
	class IncidentAwaitingResponseServiceTaskTest : ServiceTaskTestCase<IncidentAwaitingResponseServiceTask>
	{
		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 3, 4, 12, 0, 0);
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
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

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 5, 0);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("5 minutes - No logs", 0, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("First notice in 5 days left - One added to log", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No notice in between 5 days and 3 days - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Second notice in 3 days left - One added to log", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No further notice - No new logs", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("7 Days - No new logs", 2, logsCount);
				AssertEquals("7 Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("7 Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				var noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				var noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				var noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage2));
				AssertEquals(true, messageList.Any(msg => msg.Body == "Support Incident Closed: No Response from Client"));
				AssertEquals("Resolved time should be in utc", new DateTime(2013, 2, 22, 12, 0, 0), incident.IM_ResolveTimeUtc);
				AssertEquals(new DateTime(2013, 3, 24, 22, 0, 0), incident.IM_CloseTime);
				AssertEquals(ZDateTime.UtcNow, incident.IM_CloseTimeUtc);

				var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
				AssertEquals(incident.IM_ResolveTimeUtc, incidentResolvedLog.SL_EventTimeUtc);
				AssertEquals("Should match current utc time", ZDateTime.UtcNow, incidentResolvedLog.SL_PostedTimeUtc);
				var statusUpdatedLog = incident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First
				(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);
				AssertEquals("Should match current local time", ZDateTime.Now, statusUpdatedLog.SL_EventTime);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingIncidentsClosedOverExpireDays()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				var logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Expire Days - No notice logs", 0, logsCount);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage2));
				AssertEquals(true, messageList.Any(msg => msg.Body == "Support Incident Closed: No Response from Client"));
				AssertEquals("Should be in utc time", new DateTime(2013, 2, 22, 12, 0, 0), incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
				AssertEquals("Should be in local time", new DateTime(2013, 3, 24, 22, 0, 0), incident.IM_CloseTime);
			}
		}

		[TestDate(2023, 12, 1, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTask_ResolutionAndClosureBehaviour()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident1.IM_IncidentNumber = "CS00088801";
			incident1.IM_Description = "Incident 1";
			incident1.IM_OH_Client = client.PK;
			incident1.IM_OC_Contact = contact.PK;
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident2.IM_IncidentNumber = "CS00088802";
			incident2.IM_Description = "Incident 2";
			incident2.IM_OH_Client = client.PK;
			incident2.IM_OC_Contact = contact.PK;
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident2.IM_Product = ProductTypes.Codes.CargoWiseOne;
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			AssertEquals("Precondition: Should have the ALL criticalities and ALL products default fallback", 2, resolutionAndClosureBehaviourCollection.Count);
			var resolutionAndClosureBehaviourArray = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>();

			var defaultCriticalityBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID.IsEmpty);
			var defaultProductBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID == defaultCriticalityBehaviour.PK);

			defaultProductBehaviour.DaysPendingCustomerToClosed = 9;
			var defaultCriticalityENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			defaultCriticalityENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			defaultCriticalityENTBehaviour.ParentID = defaultCriticalityBehaviour.PK;
			defaultCriticalityENTBehaviour.DaysPendingCustomerToClosed = 11;

			var cr4Behaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4Behaviour.Code = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			resolutionAndClosureBehaviourCollection.AddSystemChildren(cr4Behaviour);
			AssertEquals("Precondition: Should have the ALL products fallback for cr4", 5, resolutionAndClosureBehaviourCollection.Count);

			var cr4ALLProductBehaviour = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>().FirstOrDefault(x => x.ParentID == cr4Behaviour.PK);
			cr4ALLProductBehaviour.DaysPendingCustomerToClosed = 13;

			var cr4ENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			cr4ENTBehaviour.ParentID = cr4Behaviour.PK;
			cr4ENTBehaviour.DaysPendingCustomerToClosed = 20;

			var cr4ZZZBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ZZZBehaviour.Code = "ZZZ";
			cr4ZZZBehaviour.ParentID = cr4Behaviour.PK;
			cr4ZZZBehaviour.DaysPendingCustomerToClosed = 23;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2023, 12, 1 + defaultProductBehaviour.DaysPendingCustomerToClosed - 6, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + defaultProductBehaviour.DaysPendingCustomerToClosed - 4, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + defaultProductBehaviour.DaysPendingCustomerToClosed, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should use defaultProductBehaviour and be updated to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + cr4ENTBehaviour.DaysPendingCustomerToClosed - 6, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + cr4ENTBehaviour.DaysPendingCustomerToClosed - 4, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + cr4ENTBehaviour.DaysPendingCustomerToClosed, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should use cr4ENTBehaviour and be updated to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingIncidentsClosed3DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Already 3 days left, but without first notice - One added to log", 1, logsCount);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage2));
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No further notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("More than expire Days and 3 days after 2nd notice - No new logs", 1, logsCount);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be in utc time", new DateTime(2013, 2, 22, 12, 0, 0), incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
				AssertEquals("Should be in local time", new DateTime(2013, 3, 25, 22, 0, 0), incident.IM_CloseTime);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingIncidentsClosed5DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4).AddHours(23);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("First notice is 5 days left and almost 3 days left - One added to log", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("3 days left but less than 48 hours after 1st notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1).AddHours(1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Less than 3 days left and 48 hours after 1nd notice - One added to log", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 2).AddHours(1).AddMinutes(5);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Over Expire Days and 3 days after 2nd notice - No new logs", 2, logsCount);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be in utc time", new DateTime(2013, 2, 22, 12, 0, 0), incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsClosedOverExpireDays()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				var logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Expire Days - No notice logs", 0, logsCount);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage2));
				AssertEquals(true, messageList.Any(msg => msg.Body == "Support Incident Closed: No Response from Client"));
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithNoValidReplyClosedOverExpireDays()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				var logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Expire Days - No notice logs", 0, logsCount);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage2));
				AssertEquals(true, messageList.Any(msg => msg.Body == "Support Incident Closed: No Response from Client"));
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithValidReplyClosedOverExpireDays()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				var logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Incident Not Expire - No notice logs", 0, logsCount);
				AssertEquals("Incident Not Expire - Incident is at CWR", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays * 2);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Expire Days - No notice logs", 0, logsCount);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Expire Days - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage2));
				AssertEquals(true, messageList.Any(msg => msg.Body == "Support Incident Closed: No Response from Client"));
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsClosed3DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Already 3 days left, but without first notice - One added to log", 1, logsCount);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage2));
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No further notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("More than expire Days and 3 days after 2nd notice - No new logs", 1, logsCount);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithNoValidReplyClosed3DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Already 3 days left, but without first notice - One added to log", 1, logsCount);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage2));
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No further notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("More than expire Days and 3 days after 2nd notice - No new logs", 1, logsCount);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithValidReplyClosed3DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2).AddMinutes(10);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("should be no notice at this time", 0, logsCount);

				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4 + expireDays);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Already 3 days left, but without first notice - One added to log", 1, logsCount);
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				string noticeBody = EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody;
				string noticeMessage1 = noticeBody.Replace("(*TimeLeft*)", "5 days");
				string noticeMessage2 = noticeBody.Replace("(*TimeLeft*)", "3 days");
				var messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == noticeMessage1));
				AssertEquals(true, messageList.Any(msg => msg.Body == noticeMessage2));
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4 + expireDays).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("No further notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 2 + expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + expireDays);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("More than expire Days and 3 days after 2nd notice - No new logs", 1, logsCount);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("More than expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsClosed5DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4).AddHours(23);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("First notice is 5 days left and almost 3 days left - One added to log", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("3 days left but less than 48 hours after 1st notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1).AddHours(1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Less than 3 days left and 48 hours after 1nd notice - One added to log", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 2).AddHours(1).AddMinutes(5);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Over Expire Days and 3 days after 2nd notice - No new logs", 2, logsCount);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithNoValidReplyClosed5DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5).AddMinutes(10);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4).AddHours(23);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("First notice is 5 days left and almost 3 days left - One added to log", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 3).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("3 days left but less than 48 hours after 1st notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 1).AddHours(1);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Less than 3 days left and 48 hours after 1nd notice - One added to log", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 2).AddHours(1).AddMinutes(5);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Over Expire Days and 3 days after 2nd notice - No new logs", 2, logsCount);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute_ExistingRevertToCWRIncidentsWithValidReplyClosed5DaysLeft()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var expireDays = 30;
			TestDateAttribute.Date = lastValidCWRTime = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Factory.Save();

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4);
			incident.RevertIncidentToAwaitingResponse();
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4).AddHours(23);
				process.RunTask();
				int logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("should be no notice at this time", 0, logsCount);

				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 9 + expireDays).AddHours(-12);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("First notice is 5 days left and almost 3 days left - One added to log", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 8 + expireDays).AddMinutes(30);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("3 days left but less than 48 hours after 1st notice - No new logs", 1, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 6 + expireDays).AddHours(13);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Less than 3 days left and 48 hours after 1nd notice - One added to log", 2, logsCount);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 5 + expireDays);
				process.RunTask();
				AssertEquals("Expire Days but less than 3 days after 2nd notice - do no close", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays + 2 + expireDays).AddHours(1).AddMinutes(5);
				process.RunTask();
				logsCount = incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Awaiting Response Notice Sent")).Length;
				AssertEquals("Over Expire Days and 3 days after 2nd notice - No new logs", 2, logsCount);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Over Expire Days and 3 days after 2nd notice - Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, incident.IM_ClosureResolution);
				AssertEquals("Should be lastValidCWRTime", lastValidCWRTime, incident.IM_ResolveTimeUtc);
				AssertEquals("Should be in utc time", ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			}
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTask_ExceptionShouldReportError()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysPendingCustomerToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2013, 2, 22, 12, 0, 0).AddDays(expireDays - 4).AddHours(23);
				ErrorReporter.Clear();
				process.ServiceLogger = new ServiceLoggerForExceptionTest();
				process.RunTask();
				AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("Failed to process Incident", ErrorReporter.LastKeyReported);
				AssertEquals(incident.IM_IncidentNumber + ' ' + ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override void SetUpCore()
		{
			base.SetUpCore();
			process = new IncidentAwaitingResponseServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
		}

		IncidentAwaitingResponseServiceTask process;
		TestServiceLogger logger;
	}
}
