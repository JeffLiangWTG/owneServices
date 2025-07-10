using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using static Enterprise.Core.Constants.CustomerService;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementLink))]
	public class IncidentManagementLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRelatedIncidentManagementGroup()
		{
			var link = GetNewLink();
			var group = Factory.New<IncidentManagementGroup>();
			var group1 = Factory.New<IncidentManagementGroup>();

			link.INL_ING_Group = ZGuid.Empty;
			AssertNull("If INL_ING_Group is Empty, RelatedIncidentManagementGroup should return null", link.IncidentManagementGroup);

			link.INL_ING_Group = group.PK;
			AssertNotNull("If INL_ING_Group is pointed to a group, RelatedIncidentManagementGroup should not return null", link.IncidentManagementGroup);
			AssertEquals("If INL_ING_Group is pointed to a group, RelatedIncidentManagementGroup should be equal to origin", group, link.IncidentManagementGroup);

			link.INL_ING_Group = group1.PK;
			AssertNotEquals("If INL_ING_Group is changed, RelatedIncidentManagementGroup should not be equal to origin", group, link.IncidentManagementGroup);
			AssertEquals("If INL_ING_Group is changed, RelatedIncidentManagementGroup should be equal to new", group1, link.IncidentManagementGroup);
		}

		public void TestRelatedIncident()
		{
			var link = GetNewLink();
			var incident = Factory.New<SupportIncident>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();

			link.INL_IM_Incident = ZGuid.Empty;
			AssertNull("If INL_IM_Incident is Empty, RelatedIncident should return null", link.SupportIncident);

			link.INL_IM_Incident = incident.PK;
			AssertNotNull("If INL_IM_Incident is pointed to a incident, RelatedIncident should not return null", link.SupportIncident);
			AssertEquals("If INL_IM_Incident is pointed to a incident, RelatedIncident should be equal to origin", incident, link.SupportIncident);

			link.INL_IM_Incident = incident1.PK;
			AssertNotEquals("If INL_IM_Incident is changed, RelatedIncident should not be equal to origin", incident, link.SupportIncident);
			AssertEquals("If INL_IM_Incident is changed, RelatedIncident should be equal to new", incident1, link.SupportIncident);
		}

		#region SetIncidentToControlled

		public void TestSetIncidentToControlled()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "INC000002";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000003";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = GetNewLink();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;
			task1.P9_TaskID = "T00000001";

			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Sequence = 1;
			task2.P9_TaskID = "T00000002";
			Factory.Save();

			link1.INL_IsGroupControlled = true;
			link2.INL_IsGroupControlled = true;

			Factory.Save();

			var lockReference1 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var lockReference2 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var lockReference3 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{incident2.IM_IncidentNumber}")));
			var lockReference1OnIncident = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var lockReference2OnIncident = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var lockReference3OnIncident = incident2.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{incident2.IM_IncidentNumber}")));
			var expectedLockReference1 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));
			var expectedLockReference2 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task2.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));
			var expectedLockReference3 = ConstructLCKExpectedReferences(incident2.IM_IncidentNumber, null, null, null
				, FormattableString.Invariant($"Control enabled for incident {incident2.IM_IncidentNumber}"));

			var cancelledReference1 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var cancelledReference2 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var expectedCancelledReference1 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));
			var expectedCancelledReference2 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task2.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));

			CombineAssertions(() =>
			{
				AssertReference(expectedLockReference1, lockReference1.SL_Reference);
				AssertReference(expectedLockReference2, lockReference2.SL_Reference);
				AssertReference(expectedLockReference3, lockReference3.SL_Reference);
				AssertReference(expectedLockReference1, lockReference1OnIncident.SL_Reference);
				AssertReference(expectedLockReference2, lockReference2OnIncident.SL_Reference);
				AssertReference(expectedLockReference3, lockReference3OnIncident.SL_Reference);
				AssertReference(expectedCancelledReference1, cancelledReference1.SL_Reference);
				AssertReference(expectedCancelledReference2, cancelledReference2.SL_Reference);
				AssertEquals("Should not have any cancellation events on incident without open tasks", false, incident2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode));

				AssertEquals(IncidentMainLookups.Status.Closed, incident1.IM_Status);
				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
				AssertEquals(DispositionList.Constants.Closed.ClosedByIncidentGroup, incident1.IM_ResolutionCode);
				AssertEquals(IncidentMainLookups.Status.Closed, incident2.IM_Status);
				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident2.IM_Category);
				AssertEquals(DispositionList.Constants.Closed.ClosedByIncidentGroup, incident2.IM_ResolutionCode);
			});

			AssertEquals("Emails should not be sent for group managed incident updates", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSetIncidentToControlled_NotInDatabase()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;

			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Sequence = 1;
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000003";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			Assert("Precondition", link1.INL_IsGroupControlled);

			Factory.Save();

			var lockReference1 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var lockReference2 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var expectedLockReference1 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));
			var expectedLockReference2 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task2.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));

			var cancelledReference1 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var cancelledReference2 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var expectedCancelledReference1 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));
			var expectedCancelledReference2 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task2.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));

			CombineAssertions(() =>
			{
				AssertReference(expectedLockReference1, lockReference1.SL_Reference);
				AssertReference(expectedLockReference2, lockReference2.SL_Reference);
				AssertReference(expectedCancelledReference1, cancelledReference1.SL_Reference);
				AssertReference(expectedCancelledReference2, cancelledReference2.SL_Reference);

				AssertEquals(IncidentMainLookups.Status.Closed, incident1.IM_Status);
				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
				AssertEquals(DispositionList.Constants.Closed.ClosedByIncidentGroup, incident1.IM_ResolutionCode);
			});

			AssertEquals("Emails should not be sent for group managed incident updates", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSetIncidentToControlled_LCKAndCNC_ArgumentShouldMatch()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var groupReloaded = newFactory.Load<IncidentManagementGroup>(group.PK);

			groupReloaded.Stages.RemoveAll();
			groupReloaded.Stages.AddNew("INV", "Investigation", "x", controlIncidents: true, incidentCompleted: true);
			groupReloaded.ING_Status = "INV";
			AssertEquals(true, groupReloaded.ControlIncidents);

			var linkReloaded = groupReloaded.LinkedIncidents.Cast<IncidentManagementLink>().First();
			linkReloaded.INL_IsGroupControlled = true;
			newFactory.Save();

			var lockForEditLog = groupReloaded.Logs.MostRecentLogByEventTime(Events.LockForEdit);
			var cancelledTaskLog = linkReloaded.SupportIncident.Logs.MostRecentLogByEventTime(Events.Cancelled);

			var lockForEditLogFreeTextBits = new List<string>();
			var lockForEditLogParamBits = new List<(string Key, string Value)>();
			var cancelledTaskLogFreeTextBits = new List<string>();
			var cancelledTaskLogParamBits = new List<(string Key, string Value)>();

			var logRefBuilder = EventLogReferenceBuilder.New();
			logRefBuilder.ParseReference(lockForEditLog.SL_Reference, (text) => lockForEditLogFreeTextBits.Add(text), (key, value) => lockForEditLogParamBits.Add((key, value)));
			logRefBuilder.ParseReference(cancelledTaskLog.SL_Reference, (text) => cancelledTaskLogFreeTextBits.Add(text), (key, value) => cancelledTaskLogParamBits.Add((key, value)));
			AssertEquals("Cancellation Key stored against ARG should match",
				cancelledTaskLogParamBits.First(x => x.Key == Constants.EventReferenceParameters.Codes.Argument).Value,
				lockForEditLogParamBits.First(x => x.Key == Constants.EventReferenceParameters.Codes.Argument).Value);
		}

		public void TestSetIncidentToControlled_ClosedTasksShouldNotBeCancelled()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_TaskID = "T00000001";

			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_TaskID = "T00000002";
			Factory.Save();

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			var lockReference1 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var expectedLockReference1 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));

			var cancelledReference1 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var expectedCancelledReference1 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Working
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));

			CombineAssertions(() =>
			{
				AssertReference(expectedLockReference1, lockReference1.SL_Reference);
				AssertReference(expectedCancelledReference1, cancelledReference1.SL_Reference);
				AssertEquals("Should not have any lock events for closed task", false, group.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}"))));
				AssertEquals("Should not have any cancellation events for closed task", false, incident1.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}"))));

				AssertEquals("Should be closed", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				AssertEquals("Should remain unchanged", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			});
		}

		public void TestSetIncidentToControlled_OnlyCurrentTasksShouldHaveLCKLogs()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;
			task1.P9_TaskID = "T00000001";

			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Sequence = 2;
			task2.P9_TaskID = "T00000002";
			Factory.Save();

			link1.INL_IsGroupControlled = true;
			Factory.Save();
			var lockReference1 = group.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var expectedLockReference1 = ConstructLCKExpectedReferences(incident1.IM_IncidentNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Control enabled for incident {incident1.IM_IncidentNumber}"));

			var cancelledReference1 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task1.P9_TaskID}")));
			var cancelledReference2 = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.CancelledCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}")));
			var expectedCancelledReference1 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task1.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));
			var expectedCancelledReference2 = ConstructCNCExpectedReferences(group.ING_IncidentGroupNumber, GlbStaff.CurrentUser.GS_Code, task2.P9_TaskID, ProcessTaskStatusCodeList.Codes.Assigned
				, FormattableString.Invariant($"Task cancelled by {group.ING_IncidentGroupNumber}"));

			CombineAssertions(() =>
			{
				AssertReference(expectedLockReference1, lockReference1.SL_Reference);
				AssertReference(expectedCancelledReference1, cancelledReference1.SL_Reference);
				AssertReference(expectedCancelledReference2, cancelledReference2.SL_Reference);
				AssertEquals("Should not have any lock events for task with higher sequence", false, group.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.LockForEditCode && x.SL_Reference.Contains(FormattableString.Invariant($"{task2.P9_TaskID}"))));

				AssertEquals("Should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
				AssertEquals("Should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			});
		}

		public void TestSetIncidentToControlled_IncidentIncomplete_NoWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident1.IM_ResolutionCode);
		}

		public void TestSetIncidentToControlled_IncidentIncomplete_WorkingWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var task2 = workItem.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Sequence = 1;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentIncomplete_AssignedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentIncomplete_OpenWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = string.Empty;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentIncomplete_ClosedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentComplete_NoWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(IncidentMainLookups.Status.Closed, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
			AssertEquals(DispositionList.Constants.Closed.ClosedByIncidentGroup, incident1.IM_ResolutionCode);
		}

		public void TestSetIncidentToControlled_IncidentComplete_WorkingWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var task2 = workItem.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentComplete_AssignedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentComplete_OpenWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = string.Empty;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_IncidentComplete_ClosedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestSetIncidentToControlled_ReleasingControlBeforeSaveShouldNotAddLog()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link = GetNewLink();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;
			link.INL_IsGroupControlled = false;
			Factory.Save();

			link.INL_IsGroupControlled = true;
			link.INL_IsGroupControlled = false;
			Factory.Save();

			var controlEnabledLogPredicate = new Func<StmALog, bool>(x => x.SL_SE_NKEvent == Events.LockForEditCode);
			AssertEquals("Should not add log if control is released prior to save", false, group.Logs.GetAllLogs().Cast<StmALog>().Any(controlEnabledLogPredicate));

			link.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals("Should add log", true, group.Logs.GetAllLogs().Cast<StmALog>().Any(controlEnabledLogPredicate));
		}

		public void TestSetIncidentToControlled_ShouldNotAddLogIfAlreadyControlled()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link = GetNewLink();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var groupReloaded = newFactory.Load<IncidentManagementGroup>(group.PK);

/*			groupReloaded.Stages.RemoveAll();
			groupReloaded.Stages.AddNew("INV", "Investigation", "x", controlIncidents: true, incidentCompleted: true);*/
			groupReloaded.ING_Status = "INV";
			AssertEquals(true, groupReloaded.ControlIncidents);

			var linkReloaded = groupReloaded.LinkedIncidents.Cast<IncidentManagementLink>().First();
			linkReloaded.INL_IsGroupControlled = true;
			newFactory.Save();

			var controlEnabledLogPredicate = new Func<StmALog, bool>(x => x.SL_SE_NKEvent == Events.LockForEditCode);
			AssertEquals("Precondition: Should add log", 1, groupReloaded.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate));

			linkReloaded.INL_IsGroupControlled = false;
			linkReloaded.INL_IsGroupControlled = true;
			newFactory.Save();

			AssertEquals("Should not add any new logs since control was reinstated before save", 1, groupReloaded.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate));

			linkReloaded.INL_IsGroupControlled = false;
			newFactory.Save();
			linkReloaded.INL_IsGroupControlled = true;
			newFactory.Save();

			AssertEquals("Should add a new log since control was reinstated after save", 2, groupReloaded.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate));
		}

		#region UpdateStatusOnIncidentIfRequired

		public void TestUpdateStatusOnIncidentIfRequired_IncidentIncomplete_NoWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			AssertEquals("Precondition", 0, group.AutoCascadeRelatedItems.Count);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident1.IM_ResolutionCode);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentIncomplete_WorkingWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var task2 = workItem.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Sequence = 1;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentIncomplete_AssignedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentIncomplete_OpenWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = string.Empty;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentIncomplete_ClosedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			group.ING_Status = "INV";
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentComplete_NoWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			AssertEquals("Precondition", 0, group.AutoCascadeRelatedItems.Count);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(IncidentMainLookups.Status.Closed, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
			AssertEquals(DispositionList.Constants.Closed.ClosedByIncidentGroup, incident1.IM_ResolutionCode);

			var expectedMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident1.Lookups.StageList.GetDescriptionFromCode(SupportIncidentCategoriesList.Codes.Support));
			Assert(!incident1.EConversation.Conversation.Messages.Any(x => x.Body.Equals(expectedMessage)));
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentComplete_WorkingWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var task2 = workItem.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
			var expectedMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident1.Lookups.StageList.GetDescriptionFromCode(SupportIncidentCategoriesList.Codes.Defect));
			Assert(incident1.EConversation.Conversation.Messages.Any(x => x.Body.Equals(expectedMessage)));
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentComplete_AssignedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentComplete_OpenWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = string.Empty;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestUpdateStatusOnIncidentIfRequired_IncidentComplete_ClosedWorkItem()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			Assert("Precondition", incident1.RelatedWorkItems.Contains(workItem));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			link1.UpdateStatusOnIncidentIfRequired();

			AssertEquals(IncidentMainLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		#endregion

		#endregion

		#region Assert Log References

		void AssertReference(Dictionary<string, string> expectedReferenceParameters, string reference)
		{
			var freeTextBits = new List<string>();
			var paramBits = new List<(string Key, string Value)>();
			EventLogReferenceBuilder.New().ParseReference(reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));
			foreach (var referencePair in expectedReferenceParameters)
			{
				AssertEquals(FormattableString.Invariant($"Value should match for key {referencePair.Key}"), referencePair.Value, paramBits.FirstOrDefault(x => x.Key == referencePair.Key).Value);
			}
		}

		Dictionary<string, string> ConstructLCKExpectedReferences(string referenceNumber, string assigned, string taskCode, string status, string description)
		{
			var expectedReferences = new Dictionary<string, string>();
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.ReferenceNumber, referenceNumber);
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.Description, description);

			if (!string.IsNullOrEmpty(assigned))
			{
				expectedReferences.Add(Constants.EventReferenceParameters.Codes.Assigned, assigned);
			}

			if (!string.IsNullOrEmpty(taskCode))
			{
				expectedReferences.Add(Constants.EventReferenceParameters.Codes.TaskCode, taskCode);
			}

			if (!string.IsNullOrEmpty(status))
			{
				expectedReferences.Add(Constants.EventReferenceParameters.Codes.Status, status);
			}

			return expectedReferences;
		}

		Dictionary<string, string> ConstructCNCExpectedReferences(string jobNumber, string assigned, string taskCode, string status, string description)
		{
			var expectedReferences = new Dictionary<string, string>();
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.JobNumber, jobNumber);
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.Description, description);
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.Assigned, assigned);
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.TaskCode, taskCode);
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.Type, "Task");
			expectedReferences.Add(Constants.EventReferenceParameters.Codes.Status, status);

			return expectedReferences;
		}

		#endregion

		public void TestCascadeChangesWithIncidentAttached()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			group.ING_Priority = "CR4";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";
			group.ING_IMT_Triage = triage.PK;

			Factory.Save();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			group.LinkedIncidents.Reload(false);
			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident1.IM_IMT_Triage, group.ING_IMT_Triage);
		}

		public void TestCascadeChangesWithIncidentGroupChanged()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();

			group.ING_Priority = "CR4";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";
			group.ING_IMT_Triage = triage1.PK;

			Factory.Save();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			link1.INL_IsGroupControlled = false;
			link2.INL_IsGroupControlled = false;
			link3.INL_IsGroupControlled = false;

			group.LinkedIncidents.Reload(false);
			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident1.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident2.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident3.IM_Priority, group.ING_Priority);
			AssertEquals(incident3.IM_Product, group.ING_Product);
			AssertEquals(incident3.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident3.IM_Module, group.ING_Module);
			AssertEquals(incident3.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident3.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident3.IM_IMT_Triage, group.ING_IMT_Triage);

			group.ING_Priority = "CR3";
			group.ING_Product = "SHP";
			group.ING_ProductArea = "ARF";
			group.ING_Module = "AIR";
			group.ING_ServiceType = "TET";
			group.ING_SourceModuleId = "Change";
			group.ING_IMT_Triage = triage2.PK;

			link1.INL_IsGroupControlled = true;
			link2.INL_IsGroupControlled = false;
			link3.INL_IsGroupControlled = true;

			Factory.Save();
			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident1.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident2.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident3.IM_Priority, group.ING_Priority);
			AssertEquals(incident3.IM_Product, group.ING_Product);
			AssertEquals(incident3.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident3.IM_Module, group.ING_Module);
			AssertEquals(incident3.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident3.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident3.IM_IMT_Triage, group.ING_IMT_Triage);
		}

		public void TestCascadeChangesWithIncidentGroupChanged_NewFactory()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();

			group.ING_Priority = "CR4";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";
			group.ING_IMT_Triage = triage1.PK;

			Factory.Save();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			group.LinkedIncidents.Add(link1);
			group.LinkedIncidents.Add(link2);
			group.LinkedIncidents.Add(link3);

			group.LinkedIncidents.Reload(false);
			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident1.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident2.IM_IMT_Triage, group.ING_IMT_Triage);

			AssertEquals(incident3.IM_Priority, group.ING_Priority);
			AssertEquals(incident3.IM_Product, group.ING_Product);
			AssertEquals(incident3.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident3.IM_Module, group.ING_Module);
			AssertEquals(incident3.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident3.IM_SourceModuleId, group.ING_SourceModuleId);
			AssertEquals(incident3.IM_IMT_Triage, group.ING_IMT_Triage);

			var newFactory = new BusinessObjectFactory();
			var groupInNewFactory = newFactory.Load<IncidentManagementGroup>(group.PK);
			groupInNewFactory.ING_Status = "INV";

			groupInNewFactory.ING_Priority = "CR3";
			groupInNewFactory.ING_Product = "SHP";
			groupInNewFactory.ING_ProductArea = "ARF";
			groupInNewFactory.ING_Module = "AIR";
			groupInNewFactory.ING_ServiceType = "TET";
			groupInNewFactory.ING_SourceModuleId = "Change";
			groupInNewFactory.ING_IMT_Triage = triage2.PK;

			newFactory.Save();

			var incident1InNewFactory = newFactory.Load<SupportIncident>(incident1.PK);
			var incident2InNewFactory = newFactory.Load<SupportIncident>(incident2.PK);
			var incident3InNewFactory = newFactory.Load<SupportIncident>(incident3.PK);

			AssertEquals(incident1InNewFactory.IM_Priority, groupInNewFactory.ING_Priority);
			AssertEquals(incident1InNewFactory.IM_Product, groupInNewFactory.ING_Product);
			AssertEquals(incident1InNewFactory.IM_ProgramArea, groupInNewFactory.ING_ProductArea);
			AssertEquals(incident1InNewFactory.IM_Module, groupInNewFactory.ING_Module);
			AssertEquals(incident1InNewFactory.IM_ServiceType, groupInNewFactory.ING_ServiceType);
			AssertEquals(incident1InNewFactory.IM_SourceModuleId, groupInNewFactory.ING_SourceModuleId);
			AssertEquals(incident1InNewFactory.IM_IMT_Triage, groupInNewFactory.ING_IMT_Triage);

			AssertEquals(incident2InNewFactory.IM_Priority, groupInNewFactory.ING_Priority);
			AssertEquals(incident2InNewFactory.IM_Product, groupInNewFactory.ING_Product);
			AssertEquals(incident2InNewFactory.IM_ProgramArea, groupInNewFactory.ING_ProductArea);
			AssertEquals(incident2InNewFactory.IM_Module, groupInNewFactory.ING_Module);
			AssertEquals(incident2InNewFactory.IM_ServiceType, groupInNewFactory.ING_ServiceType);
			AssertEquals(incident2InNewFactory.IM_SourceModuleId, groupInNewFactory.ING_SourceModuleId);
			AssertEquals(incident2InNewFactory.IM_IMT_Triage, groupInNewFactory.ING_IMT_Triage);

			AssertEquals(incident3InNewFactory.IM_Priority, groupInNewFactory.ING_Priority);
			AssertEquals(incident3InNewFactory.IM_Product, groupInNewFactory.ING_Product);
			AssertEquals(incident3InNewFactory.IM_ProgramArea, groupInNewFactory.ING_ProductArea);
			AssertEquals(incident3InNewFactory.IM_Module, groupInNewFactory.ING_Module);
			AssertEquals(incident3InNewFactory.IM_ServiceType, groupInNewFactory.ING_ServiceType);
			AssertEquals(incident3InNewFactory.IM_SourceModuleId, groupInNewFactory.ING_SourceModuleId);
			AssertEquals(incident3InNewFactory.IM_IMT_Triage, groupInNewFactory.ING_IMT_Triage);
		}

		public void TestCascadeChangesWithSuppressIncidentEvent()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Priority = "CR4";
			incident.IM_Product = "FLY";
			incident.IM_ProgramArea = "SUN";
			incident.IM_Module = "SKY";
			incident.IM_ServiceType = "TOP";
			incident.IM_SourceModuleId = "INC";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			group.ING_Priority = "CR5";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";
			group.ING_IMT_Triage = triage.PK;

			Factory.Save();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			AssertEquals(incident.IncidentEventFactoryDisabled, false);
			link.CascadeGroupParameters();
			Factory.Save();
			var lastEventLog = SupportIncidentEvent.GetLastEventLog(incident);
			AssertNull(lastEventLog);

			var expectedMessage = string.Format(IncidentConstants.CriticalityChangedMessageTemplate, "CR4", "CR5");
			Assert(incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals(expectedMessage)));
		}

		public void TestIsControlled_IsGroupControlledTrue_ControlIncidentsTrue_ShouldReturnTrue()
		{
			var link = GetNewLink();
			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;

			link.INL_ING_Group = incidentManagementGroup.PK;

			link.INL_IsGroupControlled = true;
			link.IncidentManagementGroup.NowStage.ControlIncidents = true;

			AssertEquals(true, link.IsControlled);
		}

		public void TestIsControlled_IsGroupControlledFalse_Or_ControlIncidentsFalse_ShouldReturnFalse()
		{
			var link = GetNewLink();
			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;

			link.INL_ING_Group = incidentManagementGroup.PK;

			link.INL_IsGroupControlled = false;
			incidentManagementGroup.NowStage.ControlIncidents = true;

			AssertEquals(false, link.IsControlled);

			link.INL_IsGroupControlled = true;
			incidentManagementGroup.NowStage.ControlIncidents = false;

			AssertEquals(false, link.IsControlled);

			link.INL_IsGroupControlled = false;
			incidentManagementGroup.NowStage.ControlIncidents = false;

			AssertEquals(false, link.IsControlled);
		}

		[TestDate]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLastCustomerMessageReceived()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var link = GetNewLink();

			var conversation = link.SupportIncident.EConversation.Conversation;
			var customerParticipant = conversation.Participants.GetOrAdd(customer);
			var staffParticipant = conversation.Participants.GetOrAdd(staff);

			var conversationMessage1 = conversation.Messages.AddNew();
			conversationMessage1.JCM_Body = "Hello World :)";
			conversationMessage1.JCM_IsInternal = false;
			conversationMessage1.JCM_IsLocal = false;
			conversationMessage1.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 01);

			conversationMessage1.JCM_JCP_Participant = customerParticipant.PK;

			var conversationMessage2 = conversation.Messages.AddNew();
			conversationMessage2.JCM_Body = "Hello World :)";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 02);

			conversationMessage2.JCM_JCP_Participant = customerParticipant.PK;

			var conversationMessage3 = conversation.Messages.AddNew();
			conversationMessage3.JCM_Body = "Hello World :)";
			conversationMessage3.JCM_IsInternal = false;
			conversationMessage3.JCM_IsLocal = false;
			conversationMessage3.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 03);

			conversationMessage3.JCM_JCP_Participant = staffParticipant.PK;

			AssertEquals("Should be the latest message sent from a contact", conversationMessage2, link.LastCustomerMessageReceived);
		}

		public void TestFlagged()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var link = GetNewLink();
			link.IncidentManagementGroup.ING_IsAutoReplyUnflagsCommunication = true;

			AssertEquals("Flagged's default value should be false", false, link.Flagged);
			AssertEquals("For new link, Flagger's logs should not contain any logs about status changed", 0, link.IncidentManagementGroup.Logs.Find(link.UnflaggedSearchQuery).Length);

			var conversation = link.SupportIncident.EConversation.Conversation;
			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = "Hello World :)";
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;

			var participant = conversation.Participants.GetOrAdd(customer);
			conversationMessage.JCM_JCP_Participant = participant.PK;

			Factory.Save();

			AssertEquals("Flagged's default value should be true, because the customer added a message to conversation", true, link.Flagged);
			AssertEquals("Flagger's logs should not contain the logs about status changed", 0, link.IncidentManagementGroup.Logs.Find(link.UnflaggedSearchQuery).Length);
			AssertNotNull("There should have 1 message from customer in the conversation", link.LastCustomerMessageReceived);

			link.IncidentManagementGroup.SendBroadcastMessage(link);

			Thread.Sleep(100);

			link.CreateOrUpdateUnflaggedEvent();

			Factory.Save();

			AssertEquals("Should be false as the event is newer than the last message", false, link.Flagged);
		}

		public void TestIncidentStatusAndDispositionShouldBeUpdated_WhenFlaggedStatusIsRemovedOrClientResponseIsReceived()
		{
			var link = GetNewLink();
			var group = link.IncidentManagementGroup;
			var incident = link.SupportIncident;

			var conversation = incident.EConversation.Conversation;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var participant = conversation.Participants.GetOrAdd(customer);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.GroupCompleted = true;
			group.NowStage.ControlIncidents = true;
			Factory.Save();

			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Completed;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			CreateNewConversationMessage(conversation, participant);
			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			var previousStatusLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
			var previousStatus = previousStatusLog.SL_Reference.Substring(previousStatusLog.SL_Reference.Length - 3, 3);
			var previousResolutionCodeLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));
			var previousResolutionCode = previousResolutionCodeLog.SL_Reference.Substring(previousResolutionCodeLog.SL_Reference.Length - 3, 3);

			AssertEquals("Incident's Status should be WRK when client response is received", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Incident's Disposition should be AUC when client response is received", DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);
			AssertEquals("previousStatus should be CLS", IncidentMainLookups.Status.Closed, previousStatus);
			AssertEquals("previousResolutionCode should be COM", DispositionList.Constants.Closed.Completed, previousResolutionCode);

			link.Flagged = false;
			AssertEquals("Incident's Status should be reverted to CLS when Flagged status is removed manually", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Incident's Disposition should be reverted to COM when Flagged status is removed manually", DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);

			CreateNewConversationMessage(conversation, participant);
			Factory.Save();
			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			AssertEquals("Precondition", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Precondition", DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			group.AddIncidentMessageSentEvent(incident);
			Factory.Save();

			AssertEquals("Incident's Status should be reverted to CLS when reply to eConversation", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Incident's Disposition should be reverted to COM when reply to eConversation", DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
		}

		public void TestIncidentStatusAndDispositionShouldNotBeUpdated_WhenIncidentIsNotControlled()
		{
			var link = GetNewLink();
			var group = link.IncidentManagementGroup;
			var incident = link.SupportIncident;

			var conversation = incident.EConversation.Conversation;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var participant = conversation.Participants.GetOrAdd(customer);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = false;
			group.NowStage.GroupCompleted = true;
			Factory.Save();

			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Completed;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			CreateNewConversationMessage(conversation, participant);
			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			var previousStatusLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
			var previousResolutionCodeLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));

			AssertEquals("Incident's Status should not be changed when incident is not controlled", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Incident's Disposition should not be changed when incident is not controlled", DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
			AssertNull("previousStatusLog should be null", previousStatusLog);
			AssertNull("previousResolutionCodeLog should be null", previousResolutionCodeLog);
		}

		public void TestIncidentStatusAndDispositionShouldNotBeUpdated_WhenIncidentIsNotCompleted()
		{
			var link = GetNewLink();
			var group = link.IncidentManagementGroup;
			var incident = link.SupportIncident;

			var conversation = incident.EConversation.Conversation;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var participant = conversation.Participants.GetOrAdd(customer);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;
			group.NowStage.GroupCompleted = false;
			Factory.Save();

			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_ResolutionCode = DispositionList.Constants.Working.Assigned;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			CreateNewConversationMessage(conversation, participant);
			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			var previousStatusLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
			var previousResolutionCodeLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));

			AssertEquals("Incident's Status should not be changed when incident is not completed", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("Incident's Disposition should not be changed when incident is not completed", DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);
			AssertNull("previousStatusLog should be null", previousStatusLog);
			AssertNull("previousResolutionCodeLog should be null", previousResolutionCodeLog);
		}

		public void TestIncidentStatusAndDispositionShouldNotBeUpdated_WhenIncidentGroupIsNotCompleted()
		{
			var link = GetNewLink();
			var group = link.IncidentManagementGroup;
			var incident = link.SupportIncident;

			var conversation = incident.EConversation.Conversation;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var participant = conversation.Participants.GetOrAdd(customer);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;
			group.NowStage.GroupCompleted = false;
			Factory.Save();

			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Completed;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			CreateNewConversationMessage(conversation, participant);
			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			var previousStatusLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
			var previousResolutionCodeLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));

			AssertEquals("Incident's Status should not be changed when incident group is not completed", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Incident's Disposition should not be changed when incident group is not completed", DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
			AssertNull("previousStatusLog should be null", previousStatusLog);
			AssertNull("previousResolutionCodeLog should be null", previousResolutionCodeLog);
		}

		public void TestIncidentStatusAndDispositionShouldNotBeUpdated_WhenIncidentHasAnActiveWI()
		{
			var link = GetNewLink();
			var group = link.IncidentManagementGroup;
			var incident = link.SupportIncident;

			var conversation = incident.EConversation.Conversation;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var participant = conversation.Participants.GetOrAdd(customer);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;
			group.NowStage.GroupCompleted = true;
			Factory.Save();

			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Completed;
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WorkflowItems.Tasks.AddNew();
			incident.RelatedWorkItems.Add(workItem);
			CreateNewConversationMessage(conversation, participant);
			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			var previousStatusLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
			var previousResolutionCodeLog = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));

			AssertEquals("Incident's Status should not be changed when incident has an active WI", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Incident's Disposition should not be changed when incident has an active WI", DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
			AssertNull("previousStatusLog should be null", previousStatusLog);
			AssertNull("previousResolutionCodeLog should be null", previousResolutionCodeLog);
		}

		void CreateNewConversationMessage(EConversation.Business.JobConversation conversation, EConversation.Business.JobConversationParticipant participant)
		{
			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = "Hello World :)";
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;
			conversationMessage.JCM_JCP_Participant = participant.PK;
		}

		public void TestUpdateLastUnflaggedEvent()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var link = GetNewLink();

			var conversation = link.SupportIncident.EConversation.Conversation;
			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = "Hello World :)";
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;

			var participant = conversation.Participants.GetOrAdd(customer);
			conversationMessage.JCM_JCP_Participant = participant.PK;

			AssertNull(link.LastUnflaggedEventForTest);
			Assert(link.Flagged);

			link.Flagged = false;
			AssertNotNull(link.LastUnflaggedEventForTest);
			AssertEquals(1, link.Logs.Find(link.UnflaggedSearchQuery).Length);
			AssertEquals(1, link.IncidentManagementGroup.Logs.Find(link.UnflaggedSearchQuery).Length);

			var linkEvent = link.Logs.Find(link.UnflaggedSearchQuery)[0];
			var groupEvent = link.LastUnflaggedEventForTest;

			var eventTimeForLink = linkEvent.SL_EventTime;
			var eventTimeForGroup = groupEvent.SL_EventTime;

			System.Threading.Thread.Sleep(5000);

			link.CreateOrUpdateUnflaggedEvent();

			Assert("the link's event object should not be changed", link.Logs.Find(link.UnflaggedSearchQuery)[0].Equals(linkEvent));
			Assert("the group's event object should not be changed", link.LastUnflaggedEventForTest.Equals(groupEvent));

			Assert("link's event time doesn't update", eventTimeForLink < linkEvent.SL_EventTime);
			Assert("group's event time doesn't update", eventTimeForGroup < groupEvent.SL_EventTime);

			AssertNoExceptionThrown(() => { Factory.Save(); });

			Assert(linkEvent.IsInDatabase);
			Assert(groupEvent.IsInDatabase);

			link.CreateOrUpdateUnflaggedEvent();
			var linkSearchResults = link.Logs.Find(link.UnflaggedSearchQuery);
			var groupSearchResults = link.IncidentManagementGroup.Logs.Find(link.UnflaggedSearchQuery);

			AssertEquals("the number of link's event should be 1", 1, linkSearchResults.Length);
			AssertEquals("the number of group's event should be 1", 1, groupSearchResults.Length);
		}

		public void TestCustomerWaitingTime_NoCustomerMessages()
		{
			var link = GetNewLink();

			AssertNull("Precondition", link.LastCustomerMessageReceived);
			AssertEquals("Should be false if there's no customer message", false, link.IsCustomerWaitingForResponse);
			AssertEquals(string.Empty, link.CustomerWaitingTime);
		}

		[TestDate(2022, 01, 05)]
		public void TestCustomerWaitingTime_CustomerMessage()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var link = GetNewLink();

			var conversation = link.SupportIncident.EConversation.Conversation;
			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = "Hello World :)";
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;
			conversationMessage.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 03, 22, 30, 25);

			var participant = conversation.Participants.GetOrAdd(customer);
			conversationMessage.JCM_JCP_Participant = participant.PK;

			AssertNotNull("Precondition", link.LastCustomerMessageReceived);
			AssertEquals("Precondition", new ZDateTime(2022, 01, 03, 22, 30, 25), link.LastCustomerMessageReceived.JCM_PostedTimeUtc);
			AssertEquals("Should be true if there's a customer message and no unflag event", true, link.IsCustomerWaitingForResponse);
			AssertEquals("Should be the time since message was received", "1Day(s) 1Hour(s) 29Min", link.CustomerWaitingTime);
		}

		[TestDate(2022, 01, 05)]
		public void TestCustomerWaitingTime_UnflaggedCustomerMessage()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var link = GetNewLink();

			var conversation = link.SupportIncident.EConversation.Conversation;
			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = "Hello World :)";
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;
			conversationMessage.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 03, 22, 30, 25);

			var participant = conversation.Participants.GetOrAdd(customer);
			conversationMessage.JCM_JCP_Participant = participant.PK;

			AssertNotNull("Precondition", link.LastCustomerMessageReceived);
			Factory.Save();

			link.Flagged = false;

			AssertNotNull("Should now have an unflagged event", link.LastUnflaggedEventForTest);
			AssertLessThan("Precondition: Latest message should be less recent than unflagged event", link.LastCustomerMessageReceived.SendLocalDateTime, link.LastUnflaggedEventForTest.SL_EventTime);
			AssertEquals("Should be false if the unflag event is more recent than the customer message", false, link.IsCustomerWaitingForResponse);
			AssertEquals("Should be empty since the unflagged event is more recent than the message", string.Empty, link.CustomerWaitingTime);
		}

		[TestDate(2022, 01, 05)]
		public void TestCustomerWaitingTime_UnflaggedPriorToLastCustomerMessage()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var link = GetNewLink();

			var conversation = link.SupportIncident.EConversation.Conversation;
			var conversationMessage1 = conversation.Messages.AddNew();
			conversationMessage1.JCM_Body = "Hello World :)";
			conversationMessage1.JCM_IsInternal = false;
			conversationMessage1.JCM_IsLocal = false;
			conversationMessage1.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 03, 22, 30, 25);

			var participant = conversation.Participants.GetOrAdd(customer);
			conversationMessage1.JCM_JCP_Participant = participant.PK;

			var conversationMessage2 = conversation.Messages.AddNew();
			conversationMessage2.JCM_Body = "Hello World :)";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06);

			conversationMessage2.JCM_JCP_Participant = participant.PK;

			AssertNotNull("Precondition", link.LastCustomerMessageReceived);
			Factory.Save();

			link.Flagged = false;

			Factory.Save();

			TestDateAttribute.AddDays(2);

			AssertNotNull("Should now have an unflagged event", link.LastUnflaggedEventForTest);

			AssertGreaterThan("Precondition: Latest message should be more recent than unflagged event", link.LastCustomerMessageReceived.JCM_PostedTimeUtc, link.LastUnflaggedEventForTest.SL_PostedTimeUtc);
			AssertEquals("Should be true if there's a customer message more recently received than unflag event", true, link.IsCustomerWaitingForResponse);
			AssertEquals("Should be time since the second conversation message", "1Day(s) 0Hour(s) 0Min", link.CustomerWaitingTime);
		}

		[TestDate]
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestUtcOffset(11, 0, 0)]
		public void TestCustomerWaitingTime_WhenResponderInOtherTimeZone()
		{
			var date = DateTime.UtcNow;
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_RN_NKCountryCode = "PH";

			var link = GetNewLink();
			link.IncidentManagementGroup.ING_IsAutoReplyUnflagsCommunication = true;
			var conversation = link.SupportIncident.EConversation.Conversation;
			var customerParticipant = conversation.Participants.GetOrAdd(customer);
			var staffParticipant = conversation.Participants.GetOrAdd(staff);

			var conversationMessage1 = conversation.Messages.AddNew();
			conversationMessage1.JCM_Body = "Hello World1 :)";
			conversationMessage1.JCM_IsInternal = false;
			conversationMessage1.JCM_IsLocal = false;
			conversationMessage1.JCM_PostedTimeUtc = date.AddHours(-2);

			conversationMessage1.JCM_JCP_Participant = customerParticipant.PK;

			var conversationMessage2 = conversation.Messages.AddNew();
			conversationMessage2.JCM_Body = "Hello World2 :)";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = date.AddHours(-1);

			conversationMessage2.JCM_JCP_Participant = customerParticipant.PK;

			var conversationMessage3 = conversation.Messages.AddNew();
			conversationMessage3.JCM_Body = "Hello World3 :)";
			conversationMessage3.JCM_IsInternal = false;
			conversationMessage3.JCM_IsLocal = false;
			conversationMessage3.JCM_PostedTimeUtc = date;

			conversationMessage3.JCM_JCP_Participant = staffParticipant.PK;

			AssertNotNull("Precondition", link.LastCustomerMessageReceived);
			Factory.Save();

			link.Flagged = false;
			AssertNotNull("Should now have an unflagged event", link.LastUnflaggedEventForTest);
			TestTimeZoneUNLOCOAttribute.UNLOCO = "PHCPL";
			TestUtcOffsetAttribute.Time = new TimeSpan(2, 0, 0);
			Factory.Save();

			AssertEquals("Should be false if the unflagged event is more recent than the customer message", false, link.IsCustomerWaitingForResponse);
			AssertEquals("Should be empty since the unflagged event is more recent than the message", string.Empty, link.CustomerWaitingTime);
		}

		public void TestOnSaving_AttachNewIncident_ShouldSendClosingBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			var messagePublishGroupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage);
			AssertNotNull(messagePublishGroupLog);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Closing);
			Assert(link.CanReceiveBroadcastMessages);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);
			AssertEquals(User.ServiceUserCode, newMessage.SenderCode);

			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

			var messageType = message.IGM_Type;
			var description = FormattableString.Invariant(
				$"{messageType} Broadcast Message sent to {incident.Number}"
				);

			var pair1 = new KeyValuePair<string, string>("MST", messageType);
			var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
			var pair3 = new KeyValuePair<string, string>("DES", description);

			Assert(incidentLogParameters.Contains(pair1));
			Assert(incidentLogParameters.Contains(pair2));
			Assert(incidentLogParameters.Contains(pair3));

			var groupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage);
			var groupLogParameters = groupLog.Parameters;
			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			AssertNotEquals("New broadcast message was sent which should be logged on the group", messagePublishGroupLog.PK, groupLog.PK);
		}

		public void TestAttachLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.Attached).Parameters;
			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.Attached).Parameters;

			var desc = FormattableString.Invariant(
						$"Incident {incident.Number} added to Incident Group {group.Number}"
					   );
			var pair1 = new KeyValuePair<string, string>("DES", desc);
			var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
			var pair3 = new KeyValuePair<string, string>("JOB", group.Number);
			var pair4 = new KeyValuePair<string, string>("TYP", group.Number);

			Assert(incidentLogParameters.Contains(pair1));
			Assert(incidentLogParameters.Contains(pair2));
			Assert(incidentLogParameters.Contains(pair3));
			Assert(incidentLogParameters.Contains(pair4));

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair4));
		}

		public void TestOnSaving_NotInDatabase_ShouldCascadeWI()
		{
			var wi1 = Factory.NewWithValidTestData<WorkItem>();
			wi1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			wi1.WKI_WorkItemNumber = "WI00NTZ01";
			var task1 = wi1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org.PK;
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.AutoCascadeRelatedItems.Add(wi1);
			group.NowStage.ControlIncidents = true;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, wi1.WKI_Status);

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = true;

			Factory.Save();

			AssertEquals("Precondition: Should add related WI", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Precondition: Should add related WI", wi1.PK, incident1.RelatedWorkItems[0].PK);

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = true;

			Factory.Save();

			AssertEquals("Should not add the same WI twice", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Precondition: Should be the same WI", wi1.PK, incident1.RelatedWorkItems[0].PK);
			AssertEquals("Should add related WI", 1, incident2.RelatedWorkItems.Count);
			AssertEquals("Related WI should be the cascaded one", wi1.PK, incident2.RelatedWorkItems[0].PK);
		}

		public void TestOnSaving_ShouldOnlyCascadeWIForDefects()
		{
			var wi1 = Factory.NewWithValidTestData<WorkItem>();
			wi1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			wi1.WKI_WorkItemNumber = "WI00NTZ01";
			var task1 = wi1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4a = Factory.NewWithValidTestData<SupportIncident>();
			incident4a.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			var incident4b = Factory.NewWithValidTestData<SupportIncident>();
			incident4b.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			var incident5 = Factory.NewWithValidTestData<SupportIncident>();
			var incident6 = Factory.NewWithValidTestData<SupportIncident>();

			incident1.IM_OH_Client = incident2.IM_OH_Client = incident3.IM_OH_Client = incident4a.IM_OH_Client
				= incident4b.IM_OH_Client = incident5.IM_OH_Client = incident6.IM_OH_Client = org.PK;

			Factory.Save();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group3.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			var group4 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group4.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			var group5 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group5.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
			var group6 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group6.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;

			group1.ING_Type = group2.ING_Type = group3.ING_Type = group4.ING_Type = group5.ING_Type = group6.ING_Type
				= IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group1.AutoCascadeRelatedItems.Add(wi1);
			group2.AutoCascadeRelatedItems.Add(wi1);
			group3.AutoCascadeRelatedItems.Add(wi1);
			group4.AutoCascadeRelatedItems.Add(wi1);
			group5.AutoCascadeRelatedItems.Add(wi1);
			group6.AutoCascadeRelatedItems.Add(wi1);
			group1.NowStage.CascadeProductDetails = group2.NowStage.CascadeProductDetails = group3.NowStage.CascadeProductDetails
				= group5.NowStage.CascadeProductDetails = group6.NowStage.CascadeProductDetails = true;
			group4.NowStage.CascadeProductDetails = false;
			group1.NowStage.ControlIncidents = group2.NowStage.ControlIncidents = group3.NowStage.ControlIncidents
				= group4.NowStage.ControlIncidents = group5.NowStage.ControlIncidents = group6.NowStage.ControlIncidents = true;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, wi1.WKI_Status);

			Factory.Save();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group1.PK;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group2.PK;
			var link3 = Factory.New<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group3.PK;
			var link4a = Factory.New<IncidentManagementLink>();
			link4a.INL_IM_Incident = incident4a.PK;
			link4a.INL_ING_Group = group4.PK;
			var link4b = Factory.New<IncidentManagementLink>();
			link4b.INL_IM_Incident = incident4b.PK;
			link4b.INL_ING_Group = group4.PK;
			var link5 = Factory.New<IncidentManagementLink>();
			link5.INL_IM_Incident = incident5.PK;
			link5.INL_ING_Group = group5.PK;
			var link6 = Factory.New<IncidentManagementLink>();
			link6.INL_IM_Incident = incident6.PK;
			link6.INL_ING_Group = group6.PK;

			link1.INL_IsGroupControlled = link2.INL_IsGroupControlled = link3.INL_IsGroupControlled = link4a.INL_IsGroupControlled
				= link4b.INL_IsGroupControlled = link5.INL_IsGroupControlled = link6.INL_IsGroupControlled = true;

			//Product details need to be cascaded before incident can be checked for IsDefect

			Factory.Save();

			AssertEquals("Precondition: Priority (criticality) should match group", group1.ING_Priority, incident1.IM_Priority);
			AssertEquals("Precondition: Priority (criticality) should match group", group2.ING_Priority, incident2.IM_Priority);
			AssertEquals("Precondition: Priority (criticality) should match group", group3.ING_Priority, incident3.IM_Priority);
			AssertEquals("Precondition", Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident4a.IM_Priority);
			AssertEquals("Precondition", Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest, incident4b.IM_Priority);
			AssertEquals("Precondition: Priority (criticality) should match group", group5.ING_Priority, incident5.IM_Priority);
			AssertEquals("Precondition: Priority (criticality) should match group", group6.ING_Priority, incident6.IM_Priority);

			AssertEquals("Should cascade if incident priority (criticality) is defect", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Should cascade if incident priority (criticality) is defect", 1, incident2.RelatedWorkItems.Count);
			AssertEquals("Should cascade if incident priority (criticality) is defect", 1, incident3.RelatedWorkItems.Count);
			AssertEquals("Should cascade if incident priority (criticality) is defect", 1, incident4a.RelatedWorkItems.Count);
			AssertEquals("Should not cascade if incident priority (criticality) is not defect", 0, incident4b.RelatedWorkItems.Count);
			AssertEquals("Should not cascade if incident priority (criticality) is not defect", 0, incident5.RelatedWorkItems.Count);
			AssertEquals("Should not cascade if incident priority (criticality) is not defect", 0, incident6.RelatedWorkItems.Count);
		}

		public void TestOnSaving_ShouldOnlyCascadeWIToIsControlled()
		{
			var wi1 = Factory.NewWithValidTestData<WorkItem>();
			wi1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			wi1.WKI_WorkItemNumber = "WI00NTZ01";
			var task1 = wi1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident1.IM_OH_Client = incident2.IM_OH_Client = incident3.IM_OH_Client = org.PK;

			Factory.Save();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group3.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;

			group1.AutoCascadeRelatedItems.Add(wi1);
			group2.AutoCascadeRelatedItems.Add(wi1);
			group3.AutoCascadeRelatedItems.Add(wi1);
			group1.ING_Type = group2.ING_Type = group3.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group1.NowStage.ControlIncidents = true;
			group2.NowStage.ControlIncidents = true;
			group3.NowStage.ControlIncidents = false;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, wi1.WKI_Status);
			AssertEquals("Precondition", true, group1.NowStage.ControlIncidents);
			AssertEquals("Precondition", true, group2.NowStage.ControlIncidents);
			AssertEquals("Precondition", false, group3.NowStage.ControlIncidents);

			Factory.Save();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group1.PK;
			link1.INL_IsGroupControlled = true;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group2.PK;
			link2.INL_IsGroupControlled = false;
			var link3 = Factory.New<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group3.PK;
			link3.INL_IsGroupControlled = true;

			Factory.Save();

			AssertEquals("Precondition", true, link1.IsControlled);
			AssertEquals("Precondition", false, link2.IsControlled);
			AssertEquals("Precondition", false, link3.IsControlled);
			AssertEquals("Should cascade if controlled", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Should not cascade if not group controlled", 0, incident2.RelatedWorkItems.Count);
			AssertEquals("Should not cascade if not in control stage", 0, incident3.RelatedWorkItems.Count);
		}

		public void TestSetIncidentToControlled_IncidentComplete_AssignedWorkItem_ShouldCascadeWorkItemAndUpdateCategory()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var link1 = GetNewLink();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			group.ING_Status = "INV";
			group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			group.AutoCascadeRelatedItems.Add(workItem);
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.INL_IsGroupControlled);

			Factory.Save();

			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			link1.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
		}

		public void TestCascadeGroupParametersShouldNotOverwriteStatusChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_ResolutionCode = DispositionList.Constants.Working.Assigned;
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;
			group.NowStage.IncidentCompleted = false;
			group.NowStage.CascadeProductDetails = true;

			Factory.Save();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			AssertEquals("Precondition", true, link.IsControlled);
			AssertEquals("Precondition", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Precondition", DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);
			link.CancelTasksAddLogsAndUpdateStatusOnIncident();

			AssertEquals("Precondition", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals("Precondition", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			link.CascadeGroupParameters();
			AssertEquals("Should not revert to original values", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should not revert to original values", SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals("Should not revert to original values", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
		}

		[TestDate(2023, 01, 01)]
		public void TestLastCommunication()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence;
			groupStatusConfig1.ControlIncidents = false;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.Stages.Add(groupStatusConfig1);
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			AssertEquals("Should return empty if there are no message events", string.Empty, link.LastCommunication);

			Factory.Save();

			TestDateAttribute.AddSeconds(1);
			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			AssertEquals("Last communication should be message received event", AutoEvents.MessageReceived.Description, link.LastCommunication);

			TestDateAttribute.AddSeconds(1);
			group.AddIncidentMessageSentEvent(incident);
			Factory.Save();

			AssertEquals("Latest message should now be message sent", $"{AutoEvents.MessageSent.Description} - Manual", link.LastCommunication);

			TestDateAttribute.AddSeconds(1);
			group.SendBroadcastMessage(link);
			Factory.Save();

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals("Precondition: Should have sent broadcast message", newMessage.JCM_Body, message.IGM_Message);
			AssertEquals("Latest message should now be broadcast message", $"{AutoEvents.BroadcastMessage.Description} - {message.IGM_Type}", link.LastCommunication);

			TestDateAttribute.AddSeconds(1);
			group.AddIncidentMessageSentEvent(incident);
			Factory.Save();

			AssertEquals("Latest message should now be message sent", $"{AutoEvents.MessageSent.Description} - Manual", link.LastCommunication);

			TestDateAttribute.AddSeconds(1);
			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			AssertEquals("Latest message should now be message received", AutoEvents.MessageReceived.Description, link.LastCommunication);
		}

		public void TestReopenIncidentTasksClosedByGroup_NoCancellationKey()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();
			var task3 = incident.WorkflowItems.Tasks.AddNew();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link = Factory.NewWithValidTestData<IncidentManagementLinkForTest>();
			link.INL_ING_Group = group.PK;
			link.INL_IM_Incident = incident.PK;
			link.INL_IsGroupControlled = true;

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.NowStage.ControlIncidents = true;
			Assert(link.IsControlled);

			Factory.Save();

			AssertEquals("Task 1 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals("Task 2 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);

			var lckSearchingQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.LockForEdit.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, incident.Number).
				AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var lckEvents = group.Logs.Find(lckSearchingQuery);
			Assert("The LCK events should be found in group's logs", lckEvents.Length != 0);
			lckEvents.ForEach(x => x.Cancel());

			var cncSearchingQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Cancelled.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, group.Number).
				AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var cncEvents = incident.Logs.Find(cncSearchingQuery);

			AssertEquals("2 cnc events should be found", 2, cncEvents?.Length);
			AssertEquals("The cnc event that contains ASN should be found", 1, cncEvents.Count(x => x.SL_Reference.Contains($"{Constants.EventReferenceParameters.Codes.Status}={ProcessTaskStatusCodeList.Codes.Assigned}")));
			AssertEquals("The cnc event that contains OPN should be found", 1, cncEvents.Count(x => x.SL_Reference.Contains($"{Constants.EventReferenceParameters.Codes.Status}={ProcessTaskStatusCodeList.Codes.Open}")));

			var appendNote = "123456789";
			link.ReopenIncidentTasksClosedByGroup(appendNote);
			AssertEquals("Task 1 should be recovered to ASN", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Task 2 should be recovered to OPN", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);
			AssertEquals("Task 3 should keep closing", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);

			AssertContains("The append note should be found in task 1", appendNote, task1.P9_NotesAsString);
			AssertContains("The append note should be found in task 2", appendNote, task2.P9_NotesAsString);
			Assert("appended text should not be found in task 3", !task3.P9_NotesAsString.Contains(appendNote));

			AssertEquals("CNC events should be cancelled", 0, cncEvents.Count(x => !x.IsCancelled));
		}

		public void TestReopenIncidentTasksClosedByGroup()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();
			var task3 = incident.WorkflowItems.Tasks.AddNew();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link = Factory.NewWithValidTestData<IncidentManagementLinkForTest>();
			link.INL_ING_Group = group.PK;
			link.INL_IM_Incident = incident.PK;
			link.INL_IsGroupControlled = true;

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.NowStage.ControlIncidents = true;
			Assert(link.IsControlled);

			Factory.Save();
			link.INL_IsGroupControlled = false;
			Factory.Save();
			link.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals("Task 1 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals("Task 2 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);

			var lckSearchingQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.LockForEdit.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, incident.Number).
				AddToFilter(StmALogSchema.SL_IsCancelled, false);
			lckSearchingQuery.OrderBy = $"{StmALogSchema.SL_PostedTimeUtc.Name} DESC";
			var lckEvents = group.Logs.Find(lckSearchingQuery);
			Assert("The LCK events should be found in group's logs", lckEvents.Length > 1);

			var cncSearchingQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Cancelled.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, group.Number).
				AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var cncEvents = incident.Logs.Find(cncSearchingQuery);

			var cancellationKey = ZGuid.Invalid;
			AssertNoExceptionThrown(() => { cancellationKey = new ZGuid(StmALog.GetParametersFromReference(cncEvents[0].SL_Reference)[Constants.EventReferenceParameters.Codes.Argument]); });
			Assert("Cancellation key should be valid", cancellationKey.IsValid);

			AssertEquals("2 cnc events should be found", 2, cncEvents?.Length);
			AssertEquals("The cnc event that contains ASN should be found", 1, cncEvents.Count(x => x.SL_Reference.Contains($"{Constants.EventReferenceParameters.Codes.Status}={ProcessTaskStatusCodeList.Codes.Assigned}")));
			AssertEquals("The cnc event that contains OPN should be found", 1, cncEvents.Count(x => x.SL_Reference.Contains($"{Constants.EventReferenceParameters.Codes.Status}={ProcessTaskStatusCodeList.Codes.Open}")));

			var appendNote = "123456789";
			link.ReopenIncidentTasksClosedByGroup(appendNote);
			AssertEquals("Task 1 should be recovered to ASN", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Task 2 should be recovered to OPN", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);
			AssertEquals("Task 3 should keep closing", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);

			AssertContains("The append note should be found in task 1", appendNote, task1.P9_NotesAsString);
			AssertContains("The append note should be found in task 2", appendNote, task2.P9_NotesAsString);
			Assert("Appended text should not be found in task 3", !task3.P9_NotesAsString.Contains(appendNote));

			AssertEquals("CNC events should be cancelled", 0, cncEvents.Count(x => !x.IsCancelled));

			link.INL_IsGroupControlled = false;
			Factory.Save();

			Assert("Task should not be closed if related incident is not be controlled", task1.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			link.INL_IsGroupControlled = true;
			Factory.Save();
			Assert(task2.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled);

			cncEvents = incident.Logs.Find(cncSearchingQuery);
			AssertEquals("Only one active CNC event should be found", 1, cncEvents.Length);
			AssertContains("This CNC event should belong to Task 2", task2.P9_TaskID, cncEvents.FirstOrDefault().SL_Reference);

			link.ReopenIncidentTasksClosedByGroup();
			Factory.Save();
			AssertEquals("Task 1 remains in the cancelled state because it is manually operated", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertNotEquals("Task 2 should be recover", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
		}

		[TestDate(2023, 01, 01)]
		public void TestLastCommunication_HasAutoReplyMessage()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence;
			groupStatusConfig1.ControlIncidents = false;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.Stages.Add(groupStatusConfig1);
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			AssertEquals("Should return empty if there are no message events", string.Empty, link.LastCommunication);

			Factory.Save();

			TestDateAttribute.AddSeconds(1);
			group.ProcessIncidentMessageReceived(incident);
			Factory.Save();

			AssertEquals("Should show the last message event sent, no auto-reply message event was sent", AutoEvents.MessageReceived.Description, link.LastCommunication);

			TestDateAttribute.AddSeconds(1);
			link.AddAutoReplyEvent();
			Factory.Save();

			var description = $"{AutoEvents.MessageSent.Description} - {IncidentManagementGroupMessageTypePairList.Codes.AutoReply}";
			AssertEquals("Latest message event should be auto-reply message", description, link.LastCommunication);
		}

		public void TestTrySendGroupAutoReply()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			var autoreplyMessage = "Test auto reply message";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_IsAutoReply = true;
			group.NowStage.ControlIncidents = true;
			var groupMessages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			autoReplay.IGM_Message = autoreplyMessage;
			autoReplay.IGM_IsPublished = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			AssertEquals(1, incident.EConversation.JobConversationForTest.Messages.Count);

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			var messages = incident.EConversation.JobConversationForTest.Messages;

			AssertEquals(2, messages.Count);
			AssertEquals(true, messages[0].JCM_Body.Contains(autoreplyMessage));

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			AssertEquals("Should not duplicate auto-reply message", 2, messages.Count);
		}

		public void TestTrySendGroupAutoReply_NoPublishedMessage()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			var autoreplyMessage = "Test auto reply message";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_IsAutoReply = true;
			group.NowStage.ControlIncidents = true;
			var groupMessages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			autoReplay.IGM_Message = autoreplyMessage;
			autoReplay.IGM_IsPublished = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			AssertEquals(1, incident.EConversation.JobConversationForTest.Messages.Count);

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			AssertEquals("Should not add auto-reply message", 1, incident.EConversation.JobConversationForTest.Messages.Count);

			autoReplay.IGM_IsPublished = true;

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			var messages = incident.EConversation.JobConversationForTest.Messages;

			AssertEquals(2, messages.Count);
			AssertEquals(true, messages[0].JCM_Body.Contains(autoreplyMessage));
		}

		public void TestTrySendGroupAutoReply_NoSendUncontrolled()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			var autoreplyMessage = "Test auto reply message";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_IsAutoReply = true;
			group.NowStage.ControlIncidents = false;
			var groupMessages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			autoReplay.IGM_Message = autoreplyMessage;
			autoReplay.IGM_IsPublished = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			AssertEquals(1, incident.EConversation.JobConversationForTest.Messages.Count);

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			AssertEquals("Should not add auto-reply message", 1, incident.EConversation.JobConversationForTest.Messages.Count);

			autoReplay.IGM_IsPublished = true;

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			var messages = incident.EConversation.JobConversationForTest.Messages;

			AssertEquals(1, messages.Count);
		}

		public void TestTrySendGroupAutoReply_IsAutoReply()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			var autoreplyMessage = "Test auto reply message";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_IsAutoReply = false;
			group.NowStage.ControlIncidents = true;
			var groupMessages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			autoReplay.IGM_Message = autoreplyMessage;
			autoReplay.IGM_IsPublished = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			AssertEquals(1, incident.EConversation.JobConversationForTest.Messages.Count);

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			AssertEquals("Should not add auto-reply message", 1, incident.EConversation.JobConversationForTest.Messages.Count);

			group.ING_IsAutoReply = true;

			TestDateAttribute.AddSeconds(1);
			link.TrySendGroupAutoReply();
			Factory.Save();

			var messages = incident.EConversation.JobConversationForTest.Messages;

			AssertEquals(2, messages.Count);
			AssertEquals(true, messages[0].JCM_Body.Contains(autoreplyMessage));
		}

		public void TestOnSaving_AttachNewIncident_ShouldSendOpeningBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var messageInterim = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			messageInterim.IGM_ING_Group = group.PK;
			messageInterim.IGM_IsPublished = true;
			messageInterim.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			messageInterim.IGM_Message = "Test for Interim";

			var messageOpening = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			messageOpening.IGM_ING_Group = group.PK;
			messageOpening.IGM_IsPublished = true;
			messageOpening.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			messageOpening.IGM_Message = "Test for Opening";

			Factory.Save();

			var messagePublishGroupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage);
			AssertNotNull(messagePublishGroupLog);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			Factory.Save();

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, messageOpening.IGM_Message);
			AssertEquals(User.ServiceUserCode, newMessage.SenderCode);
		}

		IncidentManagementLinkForTest GetNewLink()
		{
			return Factory.NewWithValidTestData<IncidentManagementLinkForTest>();
		}
	}

	class IncidentManagementLinkForTest : IncidentManagementLink
	{
		public IncidentManagementLinkForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public StmALog LastUnflaggedEventForTest => LastUnflaggedEvent;

		public ZQuery UnflaggedSearchQuery => UnflaggedEventSearchQuery;

		public string UnflaggedEventReference => GetUnFlaggedEventReferenceText(SupportIncident);
	}
}
