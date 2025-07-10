using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(IncidentResolvedServiceTask))]
	class IncidentResolvedServiceTaskTest : ServiceTaskTestCase<IncidentResolvedServiceTask>
	{
		[TestDate(2023, 12, 1, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestExecute()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			var cancelledIncident = Factory.NewWithValidTestData<SupportIncident>();
			cancelledIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			cancelledIncident.IM_IncidentNumber = "CS00088801";
			cancelledIncident.IM_Description = "Incident 1";
			cancelledIncident.IM_OH_Client = client.PK;
			cancelledIncident.IM_OC_Contact = contact.PK;
			cancelledIncident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			cancelledIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled;

			var resolvedIncident = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident.IM_IncidentNumber = "CS00088802";
			resolvedIncident.IM_Description = "Incident 2";
			resolvedIncident.IM_OH_Client = client.PK;
			resolvedIncident.IM_OC_Contact = contact.PK;
			resolvedIncident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			var resolutionAndClosureBehaviour = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var daysResolvedToClosed = ((ResolutionAndClosureBehaviour)resolutionAndClosureBehaviour).DaysResolvedToClosed;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2023, 12, 1 + daysResolvedToClosed - 1, 12, 0, 0);
			process.RunTask();
			AssertEquals("cancelled incident resolution code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, cancelledIncident.IM_ResolutionCode);
			AssertEquals("resolved incident resolution code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, resolvedIncident.IM_ResolutionCode);

			TestDateAttribute.Date = new DateTime(2023, 12, 1 + daysResolvedToClosed, 12, 0, 0);
			process.RunTask();
			AssertEquals("cancelled incident resolution code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, cancelledIncident.IM_ResolutionCode);
			AssertEquals("resolved incident resolution code should be changed to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident.IM_ResolutionCode);

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(resolvedIncident.PK);
			var loadedIncidentLogs = loadedIncident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(1, loadedIncidentLogs.Count(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode && x.SL_Reference.Contains("|NEW=CLS|OLD=SLV")));
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
			var resolvedIncident1 = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident1.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident1.IM_IncidentNumber = "CS00088801";
			resolvedIncident1.IM_Description = "Incident 1";
			resolvedIncident1.IM_OH_Client = client.PK;
			resolvedIncident1.IM_OC_Contact = contact.PK;
			resolvedIncident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			resolvedIncident1.IM_Product = ProductTypes.Codes.Enterprise;
			resolvedIncident1.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			var resolvedIncident2 = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident2.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident2.IM_IncidentNumber = "CS00088802";
			resolvedIncident2.IM_Description = "Incident 2";
			resolvedIncident2.IM_OH_Client = client.PK;
			resolvedIncident2.IM_OC_Contact = contact.PK;
			resolvedIncident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			resolvedIncident2.IM_Product = ProductTypes.Codes.CargoWiseOne;
			resolvedIncident2.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			AssertEquals("Precondition: Should have the ALL criticalities and ALL products default fallback", 2, resolutionAndClosureBehaviourCollection.Count);
			var resolutionAndClosureBehaviourArray = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>();

			var defaultCriticalityBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID.IsEmpty);
			var defaultProductBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID == defaultCriticalityBehaviour.PK);

			defaultProductBehaviour.DaysResolvedToClosed = 5;
			var defaultCriticalityENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			defaultCriticalityENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			defaultCriticalityENTBehaviour.ParentID = defaultCriticalityBehaviour.PK;
			defaultCriticalityENTBehaviour.DaysResolvedToClosed = 7;

			var cr4Behaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4Behaviour.Code = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			resolutionAndClosureBehaviourCollection.AddSystemChildren(cr4Behaviour);
			AssertEquals("Precondition: Should have the ALL products fallback for cr4", 5, resolutionAndClosureBehaviourCollection.Count);

			var cr4ALLProductBehaviour = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>().FirstOrDefault(x => x.ParentID == cr4Behaviour.PK);
			cr4ALLProductBehaviour.DaysResolvedToClosed = 9;

			var cr4ENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			cr4ENTBehaviour.ParentID = cr4Behaviour.PK;
			cr4ENTBehaviour.DaysResolvedToClosed = 11;

			var cr4ZZZBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ZZZBehaviour.Code = "ZZZ";
			cr4ZZZBehaviour.ParentID = cr4Behaviour.PK;
			cr4ZZZBehaviour.DaysResolvedToClosed = 13;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				TestDateAttribute.Date = new DateTime(2023, 12, 1 + defaultProductBehaviour.DaysResolvedToClosed - 1, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, resolvedIncident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, resolvedIncident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + defaultProductBehaviour.DaysResolvedToClosed, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, resolvedIncident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should use defaultProductBehaviour and be updated to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + cr4ENTBehaviour.DaysResolvedToClosed - 1, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, resolvedIncident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident2.IM_ResolutionCode);

				TestDateAttribute.Date = new DateTime(2023, 12, 1 + cr4ENTBehaviour.DaysResolvedToClosed, 12, 0, 0);
				process.RunTask();
				AssertEquals("incident1 Resolution Code should use cr4ENTBehaviour and be updated to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident1.IM_ResolutionCode);
				AssertEquals("incident2 Resolution Code should not be changed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident2.IM_ResolutionCode);
			}
		}

		[TestDate(2023, 12, 1, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTask_ExceptionShouldReportError()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2023, 12, 8, 12, 0, 0);
			ErrorReporter.Clear();
			process.ServiceLogger = new ServiceLoggerForExceptionTest();
			process.RunTask();
			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Failed to process Incident", ErrorReporter.LastKeyReported);
			AssertEquals(incident.IM_IncidentNumber + ' ' + ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2013, 2, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";
			var cancelledIncident = Factory.NewWithValidTestData<SupportIncident>();
			cancelledIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			cancelledIncident.IM_IncidentNumber = "CS00088801";
			cancelledIncident.IM_Description = "Incident 1";
			cancelledIncident.IM_OH_Client = client.PK;
			cancelledIncident.IM_OC_Contact = contact.PK;
			cancelledIncident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			cancelledIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled;

			var resolvedIncident = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident.IM_IncidentNumber = "CS00088802";
			resolvedIncident.IM_Description = "Incident 2";
			resolvedIncident.IM_OH_Client = client.PK;
			resolvedIncident.IM_OC_Contact = contact.PK;
			resolvedIncident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			var resolutionAndClosureBehaviour = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var daysResolvedToClosed = ((ResolutionAndClosureBehaviour)resolutionAndClosureBehaviour).DaysResolvedToClosed;

			Factory.Save();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
			{
				TestDateAttribute.Date = new DateTime(2023, 12, 1 + daysResolvedToClosed, 12, 0, 0);
				AssertNoExceptionThrown(() => process.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestDate(2023, 12, 1, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIncidentEscalateContentDevelopment_WhenIncidentClosedByIRP()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";

			var resolvedIncident = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident.IM_IncidentNumber = "CS00088802";
			resolvedIncident.IM_Description = "Incident 2";
			resolvedIncident.IM_OH_Client = client.PK;
			resolvedIncident.IM_OC_Contact = contact.PK;
			resolvedIncident.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			resolvedIncident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			resolvedIncident.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment;

			var resolutionAndClosureBehaviour = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var daysResolvedToClosed = ((ResolutionAndClosureBehaviour)resolutionAndClosureBehaviour).DaysResolvedToClosed;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2023, 12, 1 + daysResolvedToClosed, 12, 0, 0);
			process.RunTask();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(resolvedIncident.PK);
			var loadedIncidentLogs = loadedIncident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(1, loadedIncidentLogs.Count(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode && x.SL_Reference.Contains("|NEW=CLS|OLD=SLV")));
			AssertEquals(SupportIncidentCategoriesList.Codes.ContentDevelopment, loadedIncident.IM_Category);
			AssertEquals("resolved incident resolution code should be changed to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
		}

		public void TestIncidentEscalateContentDevelopment_WhenIncidentManuallyClosed()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.IM_IncidentNumber = "CS00088802";
			incident.IM_Description = "Incident 2";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			incident.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment;

			Factory.Save();

			process.RunTask();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentCategoriesList.Codes.ContentDevelopment, loadedIncident.IM_Category);
		}

		public void TestNoExceptionWhenIncidentWithNoResolvedTime()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";

			var resolvedIncident = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident.IM_IncidentNumber = "CS00088802";
			resolvedIncident.IM_Description = "Test Incident";
			resolvedIncident.IM_OH_Client = client.PK;
			resolvedIncident.IM_OC_Contact = contact.PK;
			resolvedIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			Factory.Save();

			// This query simulates bad data entering the DB. There's currently no legitimate way a SupportIncident can have a null resolve time while in a resolved state.
			var query = $"UPDATE dbo.IncidentMain SET IM_ResolveTimeUtc = NULL WHERE IM_PK = '{resolvedIncident.PK}'";
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}

			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			AssertNoExceptionThrown(() => process.RunTask());
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestDate(2023, 12, 1, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTask_ShouldNotOverrideProductArea()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "archie.contact@abc.org";
			client.Contacts.Add(contact);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Archie Tester";
			var resolvedIncident1 = Factory.NewWithValidTestData<SupportIncident>();
			resolvedIncident1.IM_GS_NKCustServiceContact = staff.GS_Code;
			resolvedIncident1.IM_IncidentNumber = "CS00088801";
			resolvedIncident1.IM_Description = "Incident 1";
			resolvedIncident1.IM_OH_Client = client.PK;
			resolvedIncident1.IM_OC_Contact = contact.PK;
			resolvedIncident1.IM_Priority = "CR8";
			resolvedIncident1.IM_Product = ProductTypes.Codes.Enterprise;
			resolvedIncident1.IM_Module = "AAA";
			resolvedIncident1.IM_ResolveTimeUtc = TestDateAttribute.Date;
			resolvedIncident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			var resolutionAndClosureBehaviour = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var daysResolvedToClosed = ((ResolutionAndClosureBehaviour)resolutionAndClosureBehaviour).DaysResolvedToClosed;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2023, 12, 1 + daysResolvedToClosed, 12, 0, 0);
			resolvedIncident1.ProductArea = "ZZZ";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var recalculatedProductArea = resolvedIncident1.GetRecalculatedProductArea();
			AssertEquals("if recalculate, the ProductArea should be ARC", ProductAreaList.Codes.ARC, recalculatedProductArea);

			process.RunTask();

			AssertEquals("resolved incident resolution code should be changed to CLS", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedIncident1.IM_ResolutionCode);
			AssertEquals("product area should not be changed after processing", "ZZZ", resolvedIncident1.ProductArea);
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

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			process = new IncidentResolvedServiceTask();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
		}

		IncidentResolvedServiceTask process;
		TestServiceLogger logger;
	}
}
