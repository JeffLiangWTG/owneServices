using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroup))]
	public class IncidentManagementGroupTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2025, 1, 1, 0, 0, 0)]
		public void TestUpdateAuditFieldsWhenSaving()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TST";
			staff.GS_PersonalEDIMailBox = "123@123.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Test User2";
			staff2.GS_Code = "TS2";
			staff2.GS_LoginName = "TST2";
			staff2.GS_PersonalEDIMailBox = "1232@123.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Assert(group.IsAutoAdminBusinessObjectLoggerEnabled);
			Factory.Save();
			AssertEquals(new ZDateTime(2025, 1, 1, 0, 0, 0), group.ING_SystemCreateTimeUtc);
			AssertNotEquals(staff.GS_Code, group.ING_SystemCreateUser);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Guid.Empty, Guid.Empty))
			{
				TestDateAttribute.AddHours(1);
				var link = group.LinkedIncidents.AddNew();
				link.INL_IM_Incident = incident.PK;
				link.INL_IsGroupControlled = true;
				link.INL_ING_Group = group.PK;
				Assert(group.HasChanges);
				Factory.Save();

				CombineAssertions("Update link", () =>
				{
					AssertEquals(staff.GS_Code, group.ING_SystemLastEditUser);
					AssertEquals(new ZDateTime(2025, 1, 1, 1, 0, 0), group.ING_SystemLastEditTimeUtc);
				});
			}

			using (Env.SetTemporaryUserContext(staff2.GS_LoginName, Guid.Empty, Guid.Empty))
			{
				TestDateAttribute.AddHours(1);
				group.RootCauseText = "Root Cause";
				Assert(group.HasChanges);
				Factory.Save();
				CombineAssertions("Update Notes", () =>
				{
					AssertEquals(staff2.GS_Code, group.ING_SystemLastEditUser);
					AssertEquals(new ZDateTime(2025, 1, 1, 2, 0, 0), group.ING_SystemLastEditTimeUtc);
				});
			}

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Guid.Empty, Guid.Empty))
			{
				TestDateAttribute.AddHours(1);
				group.ING_Description = "New Description";
				Assert(group.HasChanges);
				Factory.Save();
				CombineAssertions("Update Self", () =>
				{
					AssertEquals(staff.GS_Code, group.ING_SystemLastEditUser);
					AssertEquals(new ZDateTime(2025, 1, 1, 3, 0, 0), group.ING_SystemLastEditTimeUtc);
				});
			}
		}

		public void TestSetING_TypeShouldResetDefaultStatus()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals("Precondition", string.Empty, group.ING_Type);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertEquals("Status should have been set", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, group.ING_Status);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			group.ING_Type = "OTH";
			AssertEquals("Status should have been reset", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, group.ING_Status);
		}

		public void TestShouldSetIncidentNumberOnSave()
		{
			var group = Factory.New<IncidentManagementGroup>();
			AssertEquals("Precondition", string.Empty, group.ING_IncidentGroupNumber);

			Factory.Save();

			AssertNotEquals(string.Empty, group.ING_IncidentGroupNumber);
		}

		public void TestDefaultServiceOutage()
		{
			var group = Factory.New<IncidentManagementGroup>();
			AssertEquals("Should default to Investigating", IncidentManagementGroupConstants.ServiceOutageCodes.Investigating, group.ING_ServiceOutage);
		}

		public void TestStages()
		{
			var code1 = "OZZ";
			var code2 = "ODE";
			var disabledCode = "POP";
			var description1 = "num	1";
			var description2 = "num 2";
			var disabledDescription = "disabled";
			var otherIncidentTypeCode = "OTH";
			var code3 = "OZY";
			var description3 = "num 3";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(disabledCode, disabledDescription, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, enabled: false);
			var otherGroup = registryValue.AddNew(otherIncidentTypeCode, "Other Management group type");
			otherGroup.IncidentGroupStatusConfigurations.AddNew(code3, description3, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.New<IncidentManagementGroup>();
			AssertEquals(0, group.Stages.Count);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;

			AssertEquals(7, group.Stages.Count);
			var originalStages = group.Stages;
			var stages = group.Stages.Cast<IncidentGroupStatusConfiguration>();

			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code1)));
			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code2)));
			Assert(!stages.Any(x => x.Code.EqualsIgnoringCase(disabledCode)));
			Assert(stages.Any(x => x.DescriptionOnGroup.EqualsIgnoringCase(description1)));
			Assert(stages.Any(x => x.DescriptionOnGroup.EqualsIgnoringCase(description2)));
			Assert(!stages.Any(x => x.DescriptionOnGroup.EqualsIgnoringCase(disabledDescription)));

			group.ING_Type = otherIncidentTypeCode;

			AssertEquals(6, group.Stages.Count);
			AssertEquals("Collection object should be the same object as earlier (required for grid binding)", group.Stages, originalStages);
			stages = group.Stages.Cast<IncidentGroupStatusConfiguration>();
			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code3)));
			Assert(stages.Any(x => x.DescriptionOnGroup.EqualsIgnoringCase(description3)));
		}

		public void TestStage_LinkedIncidents()
		{
			var code1 = "OZZ";
			var code2 = "ODE";
			var midCode = "MID";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, "num1", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			var config2 = registryValue[0].IncidentGroupStatusConfigurations.AddNew(code2, "num2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			config2.IncidentCompleted = true;
			var midStage = registryValue[0].IncidentGroupStatusConfigurations.AddNew(midCode, "middie", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			midStage.Sequence = 35;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.New<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING000011";

			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_IncidentNumber = "WI00665511";
			incident1.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident1.RelatedWorkItems.Add(Factory.New<WorkItem>());
			var link1 = group.LinkedIncidents.AddNew();
			link1.INL_ING_Group = group.PK;
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_IsGroupControlled = true;

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_IncidentNumber = "WI00665522";
			incident2.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			var link2 = group.LinkedIncidents.AddNew();
			link2.INL_ING_Group = group.PK;
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_IsGroupControlled = true;

			var incident3 = Factory.New<SupportIncident>();
			incident3.IM_IncidentNumber = "WI00665533";
			incident3.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident3.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident3.RelatedWorkItems.Add(Factory.New<WorkItem>());
			var link3 = group.LinkedIncidents.AddNew();
			link3.INL_ING_Group = group.PK;
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_IsGroupControlled = true;

			var incident4 = Factory.New<SupportIncident>();
			incident4.IM_IncidentNumber = "WI00665544";
			incident4.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident4.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			var link4 = group.LinkedIncidents.AddNew();
			link4.INL_ING_Group = group.PK;
			link4.INL_IM_Incident = incident4.PK;
			link4.INL_IsGroupControlled = false;

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = code1;

			Factory.Save();

			AssertEquals("No changes should be made in the Incident if IncidentCompleted is false", false, group.NowStage.IncidentCompleted);
			AssertEquals(IncidentConstants.IncidentStatus.AllOpen, incident1.IM_Status);
			AssertEquals(IncidentConstants.IncidentStatus.AllOpen, incident2.IM_Status);
			AssertEquals(IncidentConstants.IncidentStatus.AllOpen, incident3.IM_Status);
			AssertEquals(IncidentConstants.IncidentStatus.AllOpen, incident4.IM_Status);
			Assert(!ConstainsNewLog(incident1));
			Assert(!ConstainsNewLog(incident2));
			Assert(!ConstainsNewLog(incident3));
			Assert(!ConstainsNewLog(incident4));

			group.ING_Status = code2;

			Factory.Save();

			AssertEquals("Should apply changes to controlled incidents if IncidentCompleted is true", true, group.NowStage.IncidentCompleted);
			AssertEquals("Should not change status, just add log", IncidentConstants.IncidentStatus.AllOpen, incident1.IM_Status);
			AssertEquals("Should change incident status", SupportIncidentLookups.Status.Closed, incident2.IM_Status);
			AssertEquals("Should change incident resolution code", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			AssertEquals("Should change incident closure resolution", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, incident2.IM_ClosureResolution);
			AssertNotEquals("Should set closure time on closing", ZDateTime.Empty, incident2.IM_CloseTimeUtc);
			AssertEquals("Should not change status, just add log", IncidentConstants.IncidentStatus.AllOpen, incident3.IM_Status);
			AssertEquals("No changes should be applied because it is not Controlled", IncidentConstants.IncidentStatus.AllOpen, incident4.IM_Status);
			Assert("No changes should be applied because it is not Controlled", !ConstainsNewLog(incident4));
			Assert("Should have the new log", ConstainsNewLog(incident1));
			Assert("Should have the new log", ConstainsNewLog(incident2));
			Assert("Should have the new log", ConstainsNewLog(incident3));

			bool ConstainsNewLog(SupportIncident incident)
			{
				return incident.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
															l.SL_Reference.Contains("INC=" + incident.IM_IncidentNumber) &&
															l.SL_Reference.Contains("ING=" + group.ING_IncidentGroupNumber) &&
															l.Event.SE_Code == Events.IncidentClosedCode);
			}
		}

		public void TestDefaultResponder_LinkedIncidents()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "TOM";
			staff1.GS_Code = "TOM";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Test";
			staff2.GS_Code = "TTT";
			var group = Factory.New<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff1.GS_Code;

			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident1.RelatedWorkItems.Add(Factory.New<WorkItem>());
			var link1 = group.LinkedIncidents.AddNew();
			link1.INL_ING_Group = group.PK;
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_IsGroupControlled = true;

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Status = IncidentConstants.IncidentStatus.AllOpen;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			var link2 = group.LinkedIncidents.AddNew();
			link2.INL_ING_Group = group.PK;
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_IsGroupControlled = true;
			link2.INL_GS_NKResponder = staff2.GS_Code;
			Factory.Save();

			AssertEquals(staff1.GS_FullName, link1.Responder.GS_FullName);
			AssertEquals(staff2.GS_FullName, link2.Responder.GS_FullName);
		}

		public void TestGetPreviousAndNextStatus()
		{
			var code1 = "OZZ";
			var code2 = "ODE";
			var midCode = "MID";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, "num1", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code2, "num2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			var midStage = registryValue[0].IncidentGroupStatusConfigurations.AddNew(midCode, "middie", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			midStage.Sequence = 35;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.New<IncidentManagementGroup>();

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertEquals("Precondition", 8, group.Stages.Count);
			AssertEquals("Precondition: INV should be the first stage", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Sequence, group.Stages[0].Sequence);
			AssertEquals("Precondition: INV should be the first stage", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, group.Stages[0].Code);
			AssertEquals("Precondition: ESC should be the second stage", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Sequence, group.Stages[1].Sequence);
			AssertEquals("Precondition: ESC should be the second stage", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, group.Stages[1].Code);
			AssertEquals("Precondition", midStage.Sequence, group.Stages[4].Sequence);
			AssertEquals("Precondition", midCode, group.Stages[4].Code);
			AssertEquals("Precondition: RSV should be between mid and new stages", IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Sequence, group.Stages[5].Sequence);
			AssertEquals("Precondition: RSV should be between mid and new stages", IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, group.Stages[5].Code);
			AssertEquals("Precondition", 50, group.Stages[6].Sequence);
			AssertEquals("Precondition", code1, group.Stages[6].Code);
			AssertEquals("Precondition", 60, group.Stages[7].Sequence);
			AssertEquals("Precondition", code2, group.Stages[7].Code);

			group.ING_Status = code1;

			group.Stages.Sort("DescriptionOnGroup");

			AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, group.GetPreviousStatus().Code);
			AssertEquals(code2, group.GetNextStatus().Code);

			group.ING_Status = code2;

			AssertEquals(code1, group.GetPreviousStatus().Code);
			AssertEquals(null, group.GetNextStatus());

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertEquals(null, group.GetPreviousStatus());
			AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, group.GetNextStatus().Code);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			AssertEquals(midCode, group.GetPreviousStatus().Code);
			AssertEquals(code1, group.GetNextStatus().Code);

			group.ING_Status = "ZYX";
			AssertEquals(null, group.GetPreviousStatus());
			AssertEquals(null, group.GetNextStatus());
		}

		public void TestING_Type_ReadOnly()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Assert("Should not be readonly until saved", !group.ING_TypeInfo.ReadOnly);

			Factory.Save();

			Assert("Should be readonly once saved", group.ING_TypeInfo.ReadOnly);
		}

		#region SourceModule

		public void TestIsSourceModuleOverriden()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals(false, group.IsSourceModuleOverriden);

			group.ING_SourceModuleId = "DummySourceModule";
			AssertEquals("Can not 'override' if group hasn't been saved yet", false, group.IsSourceModuleOverriden);
			Factory.Save();
			AssertEquals(false, Factory.Load<IncidentManagementGroup>(group.PK).IsSourceModuleOverriden);

			group.ING_SourceModuleId = "DummySourceModule2";
			AssertEquals("Should now be overriden", true, group.IsSourceModuleOverriden);
			Factory.Save();
			AssertEquals(true, Factory.Load<IncidentManagementGroup>(group.PK).IsSourceModuleOverriden);
		}

		public void TestSourceModuleWithPath()
		{
			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("Dummy1", "Dummy 1", "Dummies >", ModuleListType.MenuSection, "DUM", true, true, "ENT");
			sourceModules.AddNew("COR", "ediCore", "[Licence]", ModuleListType.MenuSection, "", false, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var group = Factory.New<IncidentManagementGroup>();
			group.ING_Product = ProductTypes.Codes.Enterprise;

			group.ING_SourceModuleId = "";
			AssertEquals("<Not Available>", group.SourceModuleWithPath);

			group.ING_SourceModuleId = IncidentApproval.NotAvailableActiveModuleID;
			AssertEquals("<Not Available>", group.SourceModuleWithPath);

			group.ING_SourceModuleId = ModuleListBuilder.Codes.All;
			AssertEquals(ModuleListBuilder.Descriptions.All, group.SourceModuleWithPath);

			group.ING_SourceModuleId = "Dummy1";
			AssertEquals("Dummies > Dummy 1", group.SourceModuleWithPath);

			group.ING_SourceModuleId = "COR";
			AssertEquals("[Licence] ediCore", group.SourceModuleWithPath);

			group.ING_Product = "XXX";
			AssertEquals("COR", group.SourceModuleWithPath);
		}

		#endregion

		public void TestIncidentManagementGroupMessages()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var message1 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message1.IGM_ING_Group = group.PK;
			var message2 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var message3 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message3.IGM_ING_Group = group.PK;
			Factory.Save();

			AssertCollectionContains(message1, group.IncidentManagementGroupMessages);
			AssertCollectionNotContains(message2, group.IncidentManagementGroupMessages);
			AssertCollectionContains(message3, group.IncidentManagementGroupMessages);
		}

		public void TestFilteredRelatedItems()
		{
			var defect = Factory.NewWithValidTestData<IncidentManagementGroup>();
			defect.ING_Type = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value[0].GroupType;
			defect.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			var workitem = Factory.NewWithValidTestData<NewWorkItem>();
			workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			var project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var incident = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incident.ING_Type = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value[0].GroupType;
			incident.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			incident.RelatedItems.Add(defect);
			incident.RelatedItems.Add(issue);
			incident.RelatedItems.Add(workitem);
			incident.RelatedItems.Add(project);

			AssertEquals(4, incident.FilteredRelatedItems.Count);
			AssertCollectionContains(defect, incident.FilteredRelatedItems);
			AssertCollectionContains(issue, incident.FilteredRelatedItems);
			AssertCollectionContains(workitem, incident.FilteredRelatedItems);
			AssertCollectionContains(project, incident.FilteredRelatedItems);
		}

		public void TestChildrenParentRelatedItems()
		{
			var incidentParent = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentParent.ING_Description = "Parent Incident";
			var incident = incidentParent.RelatedItems.AddNew(typeof(IncidentManagementGroup)) as IncidentManagementGroup;

			incident.RelatedItems.AddNew(typeof(WorkItem));
			incident.RelatedItems.AddNew(typeof(WorkItem));

			var model = new WorkTaskRelatedItemsTreeModel(incident);
			var mainNode = model.RootNodes;
			AssertEquals(1, mainNode.Count);

			var rootNodes = mainNode[0].ChildNodes.ToList();
			AssertEquals(2, rootNodes.Count);

			incident.RelatedItems.Load();
			AssertEquals("Should load all related items", 3, incident.RelatedItems.Count);
			incident.ChildrenOnlyRelatedItems.Load();
			AssertEquals("Should load just the children", 2, incident.ChildrenOnlyRelatedItems.Count);
			incident.ParentsOnlyRelatedItems.Load();
			AssertEquals("Should load just the parent", 1, incident.ParentsOnlyRelatedItems.Count);
			AssertEquals("Parent Incident", incident.ParentsOnlyRelatedItems.Cast<IncidentManagementGroup>().First().ING_Description);
		}

		public void TestStageChangeShouldTriggerWorkItemCascade()
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
			incident1.IM_OH_Client = org.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident2.IM_OH_Client = org.PK;

			Factory.Save();

			var otherIncidentTypeCode = "OTH";
			var nonControlCode = "NCN";
			var nonControlDescription = "Non-controlled";
			var controlCode = "CON";
			var controlDescription = "Controlled";
			var cascadeProductDetailsCode = "CPD";
			var cascadeProductDetailsDescription = "Cascade product details";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var otherGroup = registryValue.AddNew(otherIncidentTypeCode, "Other Management group type");
			otherGroup.IncidentGroupStatusConfigurations.AddNew(nonControlCode, nonControlDescription, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: false, cascadeProductDetails: false);
			otherGroup.IncidentGroupStatusConfigurations.AddNew(controlCode, controlDescription, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true, cascadeProductDetails: false);
			otherGroup.IncidentGroupStatusConfigurations.AddNew(cascadeProductDetailsCode, cascadeProductDetailsDescription, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true, cascadeProductDetails: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;

			group1.ING_Type = otherIncidentTypeCode;
			group1.ING_Status = nonControlCode;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, wi1.WKI_Status);
			AssertEquals("Precondition", false, group1.NowStage.ControlIncidents);
			AssertEquals("Precondition", false, group1.NowStage.CascadeProductDetails);

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group1.PK;
			link1.INL_IsGroupControlled = true;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group1.PK;
			link2.INL_IsGroupControlled = true;
			group1.AutoCascadeRelatedItems.Add(wi1);

			Factory.Save();

			AssertEquals("Precondition: Should cascade WI regardless of whether group is in control stage", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Precondition: Should not cascade since criticality is not defect", 0, incident2.RelatedWorkItems.Count);

			group1.ING_Status = controlCode;
			Factory.Save();
			//When WI00556006 is checked in, saving after updating ING_Status will cancel tasks and update IM_Status

			AssertEquals("Precondition", true, link1.IsControlled);
			AssertEquals("Precondition", true, link2.IsControlled);
			AssertEquals("Should cascade if controlled", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Should set category to defect since WI is attached", SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
			AssertEquals("Should not cascade since criticality is not defect", 0, incident2.RelatedWorkItems.Count);

			group1.ING_Status = cascadeProductDetailsCode;
			Factory.Save();

			AssertEquals("Precondition", true, link1.IsControlled);
			AssertEquals("Precondition", true, link2.IsControlled);
			AssertEquals("Should remain cascaded if controlled", 1, incident1.RelatedWorkItems.Count);
			AssertEquals("Should cascade since criticality is updated to defect by CascadeProductDetails", 1, incident2.RelatedWorkItems.Count);
			AssertEquals("Should set category to defect since WI is attached", SupportIncidentCategoriesList.Codes.Defect, incident2.IM_Category);
		}

		public void TestShouldNotCascadeWorkItemToIncidentIfTheWorkItemIsAlreadyAttached()
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
			incident1.IM_OH_Client = org.PK;

			Factory.Save();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			group1.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group1.NowStage.ControlIncidents = true;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, wi1.WKI_Status);
			AssertEquals("Precondition", true, group1.NowStage.ControlIncidents);

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group1.PK;
			link1.INL_IsGroupControlled = true;

			incident1.RelatedWorkItems.Add(wi1);

			Factory.Save();

			group1.AutoCascadeRelatedItems.Add(wi1);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Precondition", true, link1.IsControlled);
			AssertEquals("Should not re-cascade if controlled", 1, incident1.RelatedWorkItems.Count);
		}

		public void TestSaveFactory_ShouldNotReApplyWorkflowTemplates()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			var system = bmTestHelper.CreateSystem(Factory, "ING");
			Factory.Save();

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "ING");
			var templateWorkflow = bmTestHelper.CreateWorkflow(template);
			var templateTask1 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Marshmellow");
			var templateTask2 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Happier");

			templateTask1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask1.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";
			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			Factory.Save();
			AssertHasCorrectTasks("Tasks should have been applied from the template on initial save");

			group.ING_Description = "I want you to be happier";

			Factory.Save();
			AssertHasCorrectTasks("Saving again shouldn't create more tasks. If we were to apply Workflow in the standard way (in OnFactorySaving), then the places where incidents also manually apply templates would cause duplication.");

			void AssertHasCorrectTasks(string assertionMessage)
			{
				AssertSequencesEqual(assertionMessage, new[] { "Marshmellow", "Happier" }, group.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_Description.ToString()));
			}
		}

		[TestDate(2022, 01, 03)]
		public void TestOutageDuration_NoMilestones()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.ING_ServiceOutage = ServiceOutageCodes.Active;
			AssertEquals("No milestone", string.Empty, group.OutageDuration);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)task).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
			group.Milestones.Add(task);

			AssertEquals("Milestone has no actual date", string.Empty, group.OutageDuration);
		}

		[TestDate(2022, 01, 03)]
		public void TestOutageDuration()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)task).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
			task.SetMilestoneActualDateForTest(new DateTime(2022, 01, 01, 22, 30, 20));
			group.Milestones.Add(task);

			group.ING_ServiceOutage = ServiceOutageCodes.Active;
			AssertEquals("Time should be calculated between now and the milestone date", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
			group.ING_ServiceOutage = ServiceOutageCodes.Downgraded;
			AssertEquals("Time should be calculated between now and the milestone date", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
		}

		[TestDate(2022, 01, 03)]
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestUtcOffset(0, 0, 0)]
		public void TestOutageDuration_DifferentTimeZone()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)task).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
			task.SetMilestoneActualDateForTest(new DateTime(2022, 01, 01, 22, 30, 20));
			group.Milestones.Add(task);

			group.ING_ServiceOutage = ServiceOutageCodes.Active;
			AssertEquals("Time should be calculated between now and the milestone date", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
			group.ING_ServiceOutage = ServiceOutageCodes.Downgraded;
			AssertEquals("Time should be calculated between now and the milestone date", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);

			AssertEquals("Precondition: Before change time zone", "2022-01-03 00:00:00", ZDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture));
			TestTimeZoneUNLOCOAttribute.UNLOCO = "PHCPL";
			TestUtcOffsetAttribute.Time = new TimeSpan(2, 0, 0);
			AssertEquals("Precondition: After change time zone", "2022-01-03 02:00:00", ZDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture));

			group.ING_ServiceOutage = ServiceOutageCodes.Active;
			AssertEquals("Time should be calculated between now and the milestone date in another time zone", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
			group.ING_ServiceOutage = ServiceOutageCodes.Downgraded;
			AssertEquals("Time should be calculated between now and the milestone date in another time zone", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
		}

		[TestDate(2022, 01, 05)]
		public void TestOutageDuration_ServiceRestored_NoMilestone()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var serviceSuspendedTask = Factory.NewWithValidTestData<ProcessTask>();
			serviceSuspendedTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)serviceSuspendedTask).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
			serviceSuspendedTask.SetMilestoneActualDateForTest(new DateTime(2022, 01, 01, 22, 30, 20));
			group.Milestones.Add(serviceSuspendedTask);

			group.ING_ServiceOutage = ServiceOutageCodes.Restored;
			AssertEquals("Should show error since milestone does not exist", "Milestone Setup Error", group.OutageDuration);

			var serviceRestoredTask = Factory.NewWithValidTestData<ProcessTask>();
			serviceRestoredTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)serviceRestoredTask).TriggerEventCode = AutoEvents.ServiceCommencedCode;
			group.Milestones.Add(serviceRestoredTask);

			AssertEquals("Should show error since milestone has no actual date", "Milestone Setup Error", group.OutageDuration);

			serviceRestoredTask.SetMilestoneActualDateForTest(new DateTime(2022, 01, 03));
			AssertEquals("Time should be calculated between the service suspended and commenced milestone dates", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
		}

		[TestDate(2022, 01, 05)]
		public void TestOutageDuration_ServiceRestored()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var serviceSuspendedTask = Factory.NewWithValidTestData<ProcessTask>();
			serviceSuspendedTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)serviceSuspendedTask).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
			serviceSuspendedTask.SetMilestoneActualDateForTest(new DateTime(2022, 01, 01, 22, 30, 20));
			group.Milestones.Add(serviceSuspendedTask);
			var serviceRestoredTask = Factory.NewWithValidTestData<ProcessTask>();
			serviceRestoredTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			((ITriggerConditions)serviceRestoredTask).TriggerEventCode = AutoEvents.ServiceCommencedCode;
			serviceRestoredTask.SetMilestoneActualDateForTest(new DateTime(2022, 01, 03));
			group.Milestones.Add(serviceRestoredTask);

			group.ING_ServiceOutage = ServiceOutageCodes.Restored;
			AssertEquals("Time should be calculated between the service suspended and commenced milestone dates", "1Day(s) 1Hour(s) 29Min", group.OutageDuration);
		}

		public void TestCreateOneMessageForEachType()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var messages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>();
			AssertEquals("Should have one of each message", 4, group.IncidentManagementGroupMessages.Count);
			AssertEquals(1, messages.Count(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply));
			AssertEquals(1, messages.Count(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Closing));
			AssertEquals(1, messages.Count(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Interim));
			AssertEquals(1, messages.Count(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening));

			AssertEquals("Default messages don't need to post NEW event", 0, messages.Count(m => m.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage)));
		}

		[TestDate(2022, 01, 05)]
		public void TestIncidentsPendingResponseCount()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link4 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_ING_Group = group.PK;
			link2.INL_ING_Group = group.PK;
			link3.INL_ING_Group = group.PK;
			link4.INL_ING_Group = group.PK;

			var conversation1 = link1.SupportIncident.EConversation.Conversation;
			var conversationMessage1 = conversation1.Messages.AddNew();
			conversationMessage1.JCM_Body = "Hello World :)";
			conversationMessage1.JCM_IsInternal = false;
			conversationMessage1.JCM_IsLocal = false;
			conversationMessage1.JCM_SystemCreateTimeUtc = new ZDateTime(2022, 01, 04);

			var participant1 = conversation1.Participants.GetOrAdd(customer);
			conversationMessage1.JCM_JCP_Participant = participant1.PK;

			link1.Flagged = false;
			link2.Flagged = false;
			link3.Flagged = false;
			Factory.Save();

			TestDateAttribute.AddDays(1);

			var conversation2 = link2.SupportIncident.EConversation.Conversation;
			var conversationMessage2 = conversation2.Messages.AddNew();
			conversationMessage2.JCM_Body = "Hello World :)";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06);

			var participant2 = conversation2.Participants.GetOrAdd(customer);
			conversationMessage2.JCM_JCP_Participant = participant2.PK;

			var conversation3 = link3.SupportIncident.EConversation.Conversation;
			var conversationMessage3 = conversation3.Messages.AddNew();
			conversationMessage3.JCM_Body = "Hello World :)";
			conversationMessage3.JCM_IsInternal = false;
			conversationMessage3.JCM_IsLocal = false;
			conversationMessage3.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06);

			var participant3 = conversation3.Participants.GetOrAdd(customer);
			conversationMessage3.JCM_JCP_Participant = participant3.PK;
			Factory.Save();

			AssertEquals("Precondition", false, link1.Flagged);
			AssertEquals("Precondition", true, link2.Flagged);
			AssertEquals("Precondition", true, link3.Flagged);
			AssertEquals("Precondition", false, link4.Flagged);

			AssertEquals("Should count the flagged incidents", 2, group.IncidentsPendingResponseCount);
		}

		[TestDate(2022, 01, 06)]
		public void TestLongestWaitingTime()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link4 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_ING_Group = group.PK;
			link2.INL_ING_Group = group.PK;
			link3.INL_ING_Group = group.PK;
			link4.INL_ING_Group = group.PK;

			var conversation1 = link1.SupportIncident.EConversation.Conversation;
			var conversationMessage1 = conversation1.Messages.AddNew();
			conversationMessage1.JCM_Body = "Hello World :)";
			conversationMessage1.JCM_IsInternal = false;
			conversationMessage1.JCM_IsLocal = false;
			conversationMessage1.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 04);

			var participant1 = conversation1.Participants.GetOrAdd(customer);
			conversationMessage1.JCM_JCP_Participant = participant1.PK;

			link1.Flagged = false;
			link2.Flagged = false;
			link3.Flagged = false;
			Factory.Save();

			TestDateAttribute.AddDays(2);

			var conversation2 = link2.SupportIncident.EConversation.Conversation;
			var conversationMessage2 = conversation2.Messages.AddNew();
			conversationMessage2.JCM_Body = "Hello World :)";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06, 22, 30, 25);

			var participant2 = conversation2.Participants.GetOrAdd(customer);
			conversationMessage2.JCM_JCP_Participant = participant2.PK;

			var conversation3 = link3.SupportIncident.EConversation.Conversation;
			var conversationMessage3 = conversation3.Messages.AddNew();
			conversationMessage3.JCM_Body = "Hello World :)";
			conversationMessage3.JCM_IsInternal = false;
			conversationMessage3.JCM_IsLocal = false;
			conversationMessage3.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06, 23, 20, 33);

			var participant3 = conversation3.Participants.GetOrAdd(customer);
			conversationMessage3.JCM_JCP_Participant = participant3.PK;
			Factory.Save();

			AssertEquals("Precondition1", false, link1.Flagged);
			AssertEquals("Precondition2", true, link2.Flagged);
			AssertEquals("Precondition3", true, link3.Flagged);
			AssertEquals("Precondition4", false, link4.Flagged);

			AssertEquals("Should count time since conversationMessage2 as it's the oldest flagged", "1Day(s) 1Hour(s) 29Min", group.LongestWaitingTime);
		}

		public void TestSetING_AutoReply_ShouldSetAutoReplyDescription()
		{
			var incidentManagementGroupGetterTestObj = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals("precondition", false, incidentManagementGroupGetterTestObj.ING_IsAutoReply);

			var expectedDescription = IncidentManagementGroupConstants.AutoReplyDescriptions.Manual;
			AssertEquals(expectedDescription, incidentManagementGroupGetterTestObj.AutoReplyDescription);

			incidentManagementGroupGetterTestObj.ING_IsAutoReply = true;
			expectedDescription = IncidentManagementGroupConstants.AutoReplyDescriptions.AutoReplyOnce;
			AssertEquals(expectedDescription, incidentManagementGroupGetterTestObj.AutoReplyDescription);
		}

		public void TestSetAutoReplyDescription_ShouldSetING_AutoReply()
		{
			var incidentManagementGroupSetterTestObj = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals("precondition", false, incidentManagementGroupSetterTestObj.ING_IsAutoReply);
			AssertEquals("precondition", IncidentManagementGroupConstants.AutoReplyDescriptions.Manual, incidentManagementGroupSetterTestObj.AutoReplyDescription);

			incidentManagementGroupSetterTestObj.AutoReplyDescription = IncidentManagementGroupConstants.AutoReplyDescriptions.AutoReplyOnce;
			AssertEquals(true, incidentManagementGroupSetterTestObj.ING_IsAutoReply);

			incidentManagementGroupSetterTestObj.AutoReplyDescription = IncidentManagementGroupConstants.AutoReplyDescriptions.Manual;
			AssertEquals(false, incidentManagementGroupSetterTestObj.ING_IsAutoReply);
		}

		public void TestIsIncidentCompleted()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertNotNull(group.NowStage);

			group.NowStage.IncidentCompleted = true;
			AssertEquals("Precondition", true, group.NowStage.IncidentCompleted);
			AssertEquals(true, group.IncidentCompleted);

			group.NowStage.IncidentCompleted = false;
			AssertEquals("Precondition", false, group.NowStage.IncidentCompleted);
			AssertEquals(false, group.IncidentCompleted);

			group.Stages.RemoveAll();
			AssertNull(group.NowStage);
			AssertEquals(false, group.IncidentCompleted);
		}

		public void TestControlIncidents()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertNotNull(group.NowStage);

			group.NowStage.ControlIncidents = true;
			AssertEquals("Precondition", true, group.NowStage.ControlIncidents);
			AssertEquals(true, group.ControlIncidents);

			group.NowStage.ControlIncidents = false;
			AssertEquals("Precondition", false, group.NowStage.ControlIncidents);
			AssertEquals(false, group.ControlIncidents);

			group.Stages.RemoveAll();
			AssertNull(group.NowStage);
			AssertEquals(false, group.ControlIncidents);
		}

		#region Broadcast Message

		public void TestOnSaving_NonActiveIncidentStatus_ShouldNotSendOpeningBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Opening);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
			Assert(link.CanReceiveBroadcastMessages);
		}

		public void TestOnSaving_NonPostIncidentStatus_ShouldNotSendClosingBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Closing);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
			Assert(link.CanReceiveBroadcastMessages);
		}

		public void TestOnSaving_DecreasingStatusSequenceToActiveIncident_ShouldNotSendOpeningBroadcastMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Sequence;
			groupStatusConfig1.ControlIncidents = true;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.Stages.Add(groupStatusConfig1);
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			var groupStatusConfig2 = new IncidentGroupStatusConfiguration();
			groupStatusConfig2.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig2.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence;
			groupStatusConfig2.ControlIncidents = true;

			group.Stages.Add(groupStatusConfig2);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Opening);
			AssertEquals(groupStatusConfig2.Code, IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code);
			Assert(groupStatusConfig1.Sequence > groupStatusConfig2.Sequence);
			Assert(link.CanReceiveBroadcastMessages);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
		}

		public void TestOnSaving_DecreasingStatusSequenceToPostIncident_ShouldNotSendClosingBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var rsvStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "RSV");
			rsvStage.ControlIncidents = true;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			group.ING_IsBroadcastToControlledOnly = false;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			Assert(rsvStage.Sequence > psiStage.Sequence);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Closing);
			Assert(rsvStage.Sequence > psiStage.Sequence);
			Assert(link.CanReceiveBroadcastMessages);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
		}

		public void TestOnSaving_IncreasingStatusSequenceToActiveIncident_ShouldSendOpeningBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = false;
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Opening);
			Assert(invStage.Sequence < aciStage.Sequence);
			Assert(link.CanReceiveBroadcastMessages);

			AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);
			AssertEquals(newMessage.SenderCode, "TOM");

			var messageType = message.IGM_Type;
			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			AssertNull("Should not add MessageSent event for broadcast", incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add MessageSent event for broadcast", group.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));

			var description = FormattableString.Invariant(
				$"{messageType} Broadcast Message sent to {incident.Number}"
				);

			var pair1 = new KeyValuePair<string, string>("MST", messageType);
			var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
			var pair3 = new KeyValuePair<string, string>("DES", description);

			Assert(incidentLogParameters.Contains(pair1));
			Assert(incidentLogParameters.Contains(pair2));
			Assert(incidentLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
		}

		public void TestOnSaving_IncreasingStatusSequenceToPostIncident_ShouldSendClosingBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = false;
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			Assert(invStage.Sequence < psiStage.Sequence);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Closing);
			Assert(link.CanReceiveBroadcastMessages);

			AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);
			AssertEquals(newMessage.SenderCode, "TOM");

			var messageType = message.IGM_Type;
			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			AssertNull("Should not add MessageSent event for broadcast", incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add MessageSent event for broadcast", group.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));

			var description = FormattableString.Invariant(
				$"{messageType} Broadcast Message sent to {incident.Number}"
				);

			var pair1 = new KeyValuePair<string, string>("MST", messageType);
			var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
			var pair3 = new KeyValuePair<string, string>("DES", description);

			Assert(incidentLogParameters.Contains(pair1));
			Assert(incidentLogParameters.Contains(pair2));
			Assert(incidentLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
		}

		public void TestOnSaving_NoControlIncident_ShouldNotSendOpeningBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = false;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";
			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Opening);
			Assert(!link.CanReceiveBroadcastMessages);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
		}

		public void TestOnSaving_NoControlIncident_ShouldNotSendClosingBroadcastMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = false;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";
			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;

			Factory.Save();

			AssertEquals(message.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.Closing);
			Assert(!link.CanReceiveBroadcastMessages);
			AssertEquals(incident.EConversation.Conversation.Messages.Count, 0);
		}

		public void TestSendBroadcastMessage_IsSupportSenderTrue_ShouldUseSupportSender()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			var isSupportSender = true;

			Assert(isSupportSender);

			group.SendBroadcastMessage(link, isSupportSender);

			Factory.Save();

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);
			AssertEquals(User.ServiceUserCode, newMessage.SenderCode);
		}

		public void TestSendBroadcastMessage_IsSupportSenderFalse_ShouldUseCurrentSender()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			AssertEquals(1, incident.EConversation.Conversation.Messages.Count);

			var isSupportSender = false;

			Assert(!isSupportSender);

			group.SendBroadcastMessage(link, isSupportSender);

			Factory.Save();

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);

			AssertEquals("TOM", newMessage.SenderCode);
		}

		public void TestOnSaving_CascadeProductDetailsFromFalseToTrue_ShouldTriggerCascadeProductCriticality()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = false;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Status = "INV";
			group.ING_Type = "MIM";

			group.ING_Priority = "CR4";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";

			Factory.Save();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			group.LinkedIncidents.Add(link1);
			group.LinkedIncidents.Add(link2);

			Factory.Save();

			AssertNotEquals(incident1.IM_Priority, group.ING_Priority);
			AssertNotEquals(incident1.IM_Product, group.ING_Product);
			AssertNotEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertNotEquals(incident1.IM_Module, group.ING_Module);
			AssertNotEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertNotEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);

			AssertNotEquals(incident2.IM_Priority, group.ING_Priority);
			AssertNotEquals(incident2.IM_Product, group.ING_Product);
			AssertNotEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertNotEquals(incident2.IM_Module, group.ING_Module);
			AssertNotEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertNotEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);

			group.ING_Status = "ESC";

			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);
		}

		public void TestOnSaving_CascadeProductDetailsFromTrueToFalseThenToTrue_ShouldTriggerCascadeProductCriticality()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.CascadeProductDetails = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.CascadeProductDetails = false;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.CascadeProductDetails = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";

			group.ING_Status = "INV";

			group.ING_Priority = "CR3";
			group.ING_Product = "CAR";
			group.ING_ProductArea = "EUP";
			group.ING_Module = "OCN";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "test";

			Factory.Save();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			group.LinkedIncidents.Add(link1);
			group.LinkedIncidents.Add(link2);

			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);

			group.ING_Status = "ESC";

			group.ING_Priority = "CR4";
			group.ING_Product = "BNN";
			group.ING_ProductArea = "ARE";
			group.ING_Module = "SKY";
			group.ING_ServiceType = "TME";
			group.ING_SourceModuleId = "test2";

			Factory.Save();

			AssertNotEquals(incident1.IM_Priority, group.ING_Priority);
			AssertNotEquals(incident1.IM_Product, group.ING_Product);
			AssertNotEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertNotEquals(incident1.IM_Module, group.ING_Module);
			AssertNotEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertNotEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);

			AssertNotEquals(incident2.IM_Priority, group.ING_Priority);
			AssertNotEquals(incident2.IM_Product, group.ING_Product);
			AssertNotEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertNotEquals(incident2.IM_Module, group.ING_Module);
			AssertNotEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertNotEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);

			group.ING_Status = "ACI";

			Factory.Save();

			AssertEquals(incident1.IM_Priority, group.ING_Priority);
			AssertEquals(incident1.IM_Product, group.ING_Product);
			AssertEquals(incident1.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident1.IM_Module, group.ING_Module);
			AssertEquals(incident1.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident1.IM_SourceModuleId, group.ING_SourceModuleId);

			AssertEquals(incident2.IM_Priority, group.ING_Priority);
			AssertEquals(incident2.IM_Product, group.ING_Product);
			AssertEquals(incident2.IM_ProgramArea, group.ING_ProductArea);
			AssertEquals(incident2.IM_Module, group.ING_Module);
			AssertEquals(incident2.IM_ServiceType, group.ING_ServiceType);
			AssertEquals(incident2.IM_SourceModuleId, group.ING_SourceModuleId);
		}

		public void TestOnFactorySaving_CascadeGroupParameters()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			AssertEquals(1, incident.EConversation.Conversation.Messages.Count);

			var isSupportSender = true;

			group.SendBroadcastMessage(link, isSupportSender);

			Factory.Save();

			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			AssertNull("Should not add MessageSent event for broadcast", incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add MessageSent event for broadcast", group.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));

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

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
		}

		public void TestSendBroadcastMessage_ShouldSendUpdateEmail()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var testTemplate = new NotificationEmailTemplate();
			testTemplate.EmailBody = "ABCDEFG **/// (*eConversationMessages*) /// $$ HIJKLM";
			testTemplate.EmailSubject = "AWR Subject";

			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTemplate);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new IncidentCustomerNotifierForTest(incident);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			AssertEquals(1, incident.EConversation.Conversation.Messages.Count);

			group.SendBroadcastMessage(link);

			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var expectedSubject = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value.EmailSubject;

			AssertEquals(expectedSubject, email.Subject);
			AssertContains(message.IGM_Message, email.Body);
			AssertContains("ABCDEFG **///", email.Body);
		}

		public void TestSendBroadcastMessage_WithoutMessage()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			Assert(
				(message.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening) ||
				(message.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Closing)
				);
			message.IGM_Message = "Test for Broadcast";

			Factory.Save();

			group.SendBroadcastMessage(link);

			Factory.Save();

			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

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

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
		}

		public void TestSendBroadcastMessage_InterimMessageFallbackOpeningMessage()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "DongTest1";
			staff1.GS_Code = "DT1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "DongTest2";
			staff2.GS_Code = "DT2";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Type = "MIM";
			group1.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group1.ING_IsBroadcastToControlledOnly = false;
			group1.ING_GS_NKGroupOwner = staff1.GS_Code;

			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_Type = "MIM";
			group2.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group2.ING_IsBroadcastToControlledOnly = false;
			group2.ING_GS_NKGroupOwner = staff2.GS_Code;

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group1.PK;
			link1.INL_IsGroupControlled = true;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group2.PK;
			link2.INL_IsGroupControlled = true;

			var openingMessage1 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			openingMessage1.IGM_ING_Group = group1.PK;
			openingMessage1.IGM_IsPublished = true;
			openingMessage1.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			openingMessage1.IGM_Message = "Open message 1";

			var interimMessage1 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			interimMessage1.IGM_ING_Group = group1.PK;
			interimMessage1.IGM_IsPublished = true;
			interimMessage1.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			interimMessage1.IGM_Message = "Interim message 1";

			var openingMessage2 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			openingMessage2.IGM_ING_Group = group2.PK;
			openingMessage2.IGM_IsPublished = true;
			openingMessage2.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			openingMessage2.IGM_Message = "Open message 2";

			var interimMessage2 = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			interimMessage2.IGM_ING_Group = group2.PK;
			interimMessage2.IGM_IsPublished = false;
			interimMessage2.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			interimMessage2.IGM_Message = "Interim message 2";

			Factory.Save();

			group1.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group2.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			Factory.Save();

			var newMessage1 = incident1.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage1.JCM_Body, interimMessage1.IGM_Message);

			var newMessage2 = incident2.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage2.JCM_Body, openingMessage2.IGM_Message);
		}

		public void TestSendBroadcastMessage_InterimStatus()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = true;
			var tmpStage = registryValue[0].IncidentGroupStatusConfigurations.AddNew();
			tmpStage.Code = "TMP";
			tmpStage.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence + 1;
			tmpStage.ControlIncidents = true;
			tmpStage.Enabled = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "TMP";
			group.ING_IsBroadcastToControlledOnly = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Interim message for broadcast";

			var messageOpening = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			messageOpening.IGM_ING_Group = group.PK;
			messageOpening.IGM_IsPublished = true;
			messageOpening.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			messageOpening.IGM_Message = "Opening message for broadcast";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			Factory.Save();

			AssertEquals(1, incident.EConversation.Conversation.Messages.Count);
			var eConvMessage1 = incident.EConversation.Conversation.Messages[0];
			AssertEquals("Newly added incident should receive Opening message", "Opening message for broadcast", eConvMessage1.Body);

			group.SendBroadcastMessage(link);

			AssertEquals(2, incident.EConversation.Conversation.Messages.Count);
			var eConvMessage2 = incident.EConversation.Conversation.Messages[0];
			AssertEquals("Interim message for broadcast", eConvMessage2.Body);
		}

		public void TestSendBroadcastMessage_StatusChangeAndNewIncidentLink_NoDuplicateMessages()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.ControlIncidents = true;
			var tmpStage = registryValue[0].IncidentGroupStatusConfigurations.AddNew();
			tmpStage.Code = "TMP";
			tmpStage.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence + 1;
			tmpStage.ControlIncidents = true;
			tmpStage.Enabled = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_Status = "ACI";
			group.ING_IsBroadcastToControlledOnly = false;
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Interim message for broadcast";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_ING_Group = group.PK;
			link.INL_IM_Incident = incident.PK;
			link.INL_IsGroupControlled = true;

			group.LinkedIncidents.Add(link);
			group.ING_Status = "TMP";

			Factory.Save();

			AssertEquals("Should have only one broadcast message", 1, incident.EConversation.Conversation.Messages.Count);
			var eConvMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals("Interim message for broadcast", eConvMessage.Body);
		}

		#endregion Broadcast Message

		public void TestLogStatusChange_StageChanged_ShouldLogEvent()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.IncidentCompleted = true;
			invStage.GroupCompleted = true;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.IncidentCompleted = false;
			escStage.GroupCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			Factory.Save();

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var hasStageChanges = group.ING_StatusInfo.HasChanges;
			Assert(hasStageChanges);

			Factory.Save();

			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.StageChange).Parameters;

			var expectedOldINGStatusPair = new KeyValuePair<string, string>(StageChangeEventLogCodes.OldStatus, IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code);
			var expectedNewINGStatusPair = new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStatus, IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code);
			var expectedStageIncidentCompletedPair = new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStageIncidentCompleted, escStage.IncidentCompleted.ToString());
			var expectedGroupCompletedPair = new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStageGroupCompleted, escStage.GroupCompleted.ToString());

			Assert(groupLogParameters.Contains(expectedOldINGStatusPair));
			Assert(groupLogParameters.Contains(expectedNewINGStatusPair));
			Assert(groupLogParameters.Contains(expectedStageIncidentCompletedPair));
			Assert(groupLogParameters.Contains(expectedGroupCompletedPair));
		}

		public void TestLogStatusChange_StageNotChanged_ShouldNotLogEvent()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence;
			groupStatusConfig1.IncidentCompleted = true;
			groupStatusConfig1.GroupCompleted = false;

			group.Stages.Add(groupStatusConfig1);
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			Factory.Save();

			var groupStatusConfig2 = new IncidentGroupStatusConfiguration();
			groupStatusConfig2.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig2.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence + 1;
			groupStatusConfig2.IncidentCompleted = true;
			groupStatusConfig2.GroupCompleted = false;

			group.Stages.Add(groupStatusConfig2);
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			var hasStageChanges = group.ING_StatusInfo.HasChanges;
			Assert(!hasStageChanges);

			Factory.Save();

			var groupHasStageLogs = group.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.StageChange.Code);
			Assert(!groupHasStageLogs);
		}

		public void TestWorkItemCascade_CascadeWorkitemToIncidents()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			incident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";
			link1.INL_IsGroupControlled = true;

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_GS_NKResponder = "~UK";
			link2.INL_IsGroupControlled = false;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			AssertCollectionNotContains("Precondition", workItemCascade, link1.SupportIncident.RelatedItems);
			AssertCollectionNotContains("Precondition", workItemCascade, link2.SupportIncident.RelatedItems);

			group.AutoCascadeRelatedItems.Add(workItemCascade);

			Factory.Save();

			AssertCollectionContains("Should add WI to controlled incident", workItemCascade, link1.SupportIncident.RelatedItems);
			AssertCollectionContains("Should add WI to not controlled incident", workItemCascade, link2.SupportIncident.RelatedItems);

			var groupLog = group.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l =>
														l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber) &&
														l.SL_Reference.Contains("RFN=" + link1.SupportIncident.IM_IncidentNumber) &&
														l.SL_Reference.Contains("JOB=" + group.ING_IncidentGroupNumber));
			AssertNotNull(groupLog);

			var incidentLog = link1.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l =>
														l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber) &&
														l.SL_Reference.Contains("RFN=" + link1.SupportIncident.IM_IncidentNumber) &&
														l.SL_Reference.Contains("JOB=" + group.ING_IncidentGroupNumber));
			AssertNotNull(incidentLog);
		}

		public void TestWorkItemCascade_ShouldNotReopenTasksNorAddMSNAndESCEvent()
		{
			var ibmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			ibmTestHelper.EnableBMSInRegistry();
			var system = ibmTestHelper.CreateSystem(Factory, "INC");

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			var workflowItem = template.WorkflowItems.AddNew();
			workflowItem.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var header = template.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "ESC AAA";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_GS_NKResponder = "~UK";
			link.INL_IsGroupControlled = true;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00PYN01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var msnEventLog = link.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "MSN");
			var escEventLog = link.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "STC" && l.SL_Reference == "Event - ESC");
			var opnOrAsnTasks = link.SupportIncident.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(t => t.P9_Status == "OPN" || t.P9_Status == "ASN");
			AssertNull(msnEventLog);
			AssertNull(escEventLog);
			AssertNull(opnOrAsnTasks);
		}

		public void TestCheckTaskStatusWhenFinishAddingIncident()
		{
			var ibmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			ibmTestHelper.EnableBMSInRegistry();
			var system = ibmTestHelper.CreateSystem(Factory, "INC");

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			var workflowItem = template.WorkflowItems.AddNew();
			workflowItem.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var header = template.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "ESC AAA";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = incident.WorkflowItems.Tasks.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task3 = incident.WorkflowItems.Tasks.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task4 = incident.WorkflowItems.Tasks.AddNew();
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_GS_NKResponder = "~UK";
			link.INL_IsGroupControlled = true;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00PYN01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			foreach (var task in link.SupportIncident.WorkflowItems.Tasks.Cast<ProcessTask>())
			{
				if (task.PK == task1.PK || task.PK == task2.PK)
				{
					AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
				}
				else if (task.PK == task3.PK || task.PK == task4.PK)
				{
					AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
				}
			}
		}

		public void TestWorkItemCascade_DontAddMoreThanOnce()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";
			link1.INL_IsGroupControlled = true;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			group.AutoCascadeRelatedItems.Add(workItemCascade);

			Factory.Save();

			AssertCollectionContains("Should add WI in incident", workItemCascade, link1.SupportIncident.RelatedItems);
			var groupLogCount = group.Logs.GetAllLogs().Cast<StmALog>().Count(l => !l.IsCancelled && l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber));
			AssertEquals(groupLogCount, 1);
			var incidentLogCount = link1.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().Count(l => !l.IsCancelled && l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber));
			AssertEquals(incidentLogCount, 1);

			group.SetWorkItemsCascadeToIncident(link1);

			var workitemCount = link1.SupportIncident.RelatedItems.Count(w => w.PK == workItemCascade.PK);
			AssertEquals(workitemCount, 1);
			groupLogCount = group.Logs.GetAllLogs().Cast<StmALog>().Count(l => !l.IsCancelled && l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber));
			AssertEquals(groupLogCount, 1);
			incidentLogCount = link1.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().Count(l => !l.IsCancelled && l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber));
			AssertEquals(incidentLogCount, 1);
		}

		public void TestWorkItemCascade_ChangeIncidentStage()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.NowStage.ControlIncidents = true;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";
			link1.INL_IsGroupControlled = true;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("DES", $"Work Item {workItemCascade.WKI_WorkItemNumber} added to Incident Group")
			};
			group.Logs.AddNew(AutoEvents.Attached, parameters.ToArray());
			group.AutoCascadeRelatedItems.Add(workItemCascade);

			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident1.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident1.IM_ResolutionCode);
		}

		public void TestWorkItemCascade_ShouldCascadeOnFirstSave()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.NowStage.ControlIncidents = true;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";
			link1.INL_IsGroupControlled = true;

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("DES", $"Work Item {workItemCascade.WKI_WorkItemNumber} added to Incident Group")
			};
			group.Logs.AddNew(AutoEvents.Attached, parameters.ToArray());
			group.AutoCascadeRelatedItems.Add(workItemCascade);

			Factory.Save();

			AssertEquals("Should attach work item to incident", 1, incident1.RelatedWorkItems.Count);
		}

		public void TestOnSave_FirstSave_ShouldPostQUCEvent()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroupForTest>();
			group.ING_Product = ProductTypes.Codes.Enterprise;
			group.ING_ProductArea = "XRM";
			group.ING_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			group.ING_SourceModuleId = "Dummy";

			Factory.Save();

			var groupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.QueueChanged);

			AssertNotNull(groupLog);

			var desc = "Product details initially set";
			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc);
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Product, group.ING_Product);
			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ProductArea, group.ING_ProductArea);
			var pair4 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Module, group.ING_Module);
			var pair5 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.SourceModuleId, group.ING_SourceModuleId);
			var groupLogParameters = groupLog.Parameters;

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair4));
			Assert(groupLogParameters.Contains(pair5));
		}

		public void TestOnSave_ProductDetailsChanged_ShouldPostQUCEvent()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroupForTest>();
			group.ING_Product = ProductTypes.Codes.Enterprise;
			group.ING_ProductArea = "XRM";
			group.ING_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			group.ING_SourceModuleId = "Dummy";

			Factory.Save();

			group.ING_Product = ProductTypes.Codes.CargoWiseOne;

			Assert(group.HasProductDetailsChangedForPostWorkflowForTest);

			Factory.Save();

			var groupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.QueueChanged);

			AssertNotNull(groupLog);

			var desc = "Product details were changed by user";
			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc);
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Product, group.ING_Product);
			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ProductArea, group.ING_ProductArea);
			var pair4 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Module, group.ING_Module);
			var pair5 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.SourceModuleId, group.ING_SourceModuleId);
			var groupLogParameters = groupLog.Parameters;

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair4));
			Assert(groupLogParameters.Contains(pair5));
		}

		public void TestOnSave_ProductDetailsChangedAndApplyNewWorkflowTemplate_ShouldPostCNCEevent()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			var system = bmTestHelper.CreateSystem(Factory, "ING");
			Factory.Save();

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "ING", null, ProductTypes.Codes.CargoWiseOne);

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroupForTest>();
			group.ING_Product = ProductTypes.Codes.Enterprise;

			Factory.Save();

			group.ING_Product = ProductTypes.Codes.CargoWiseOne;

			Assert(group.IsProductDetailsChangedCauseWorkflowTemplateBeingAppliedForTest);

			Factory.Save();

			var groupLog = group.Logs.MostRecentLogByEventTime(AutoEvents.Cancelled);

			AssertNotNull(groupLog);

			var desc = "Task cancel trigger event due to product details change";
			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc);
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Product, group.ING_Product);
			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ProductArea, group.ING_ProductArea);
			var pair4 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Module, group.ING_Module);
			var pair5 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.SourceModuleId, group.ING_SourceModuleId);
			var groupLogParameters = groupLog.Parameters;

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			Assert(groupLogParameters.Contains(pair4));
			Assert(groupLogParameters.Contains(pair5));
		}

		public void TestControlIncidentChangeToTrue_EnactControlOnLinkedIncidentsOnSaving()
		{
			//Arrange: Incidents
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "INC000002";

			//Arrange: Tasks
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

			//Arrange: Group
			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCriticalities = incidentApprovalLookups.CriticalityList.GetAllCodes();
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var validModule = "CAT";
			product.ModuleMappings.AddNew(validModule, "Category Module", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = validCriticalities[0];
			group.ING_Module = validModule;

			//Arrange: Stages
			var testStageCode1 = "TS1";
			var testStageCode2 = "TS2";
			var description1 = "TestStage 1";
			var description2 = "TestStage 2";

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			//Arrange: Links
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			Factory.Save();

			//Arrange: Group
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = testStageCode1;

			Factory.Save();

			AssertEquals("Precondition: Group Control Incident is false", false, group.ControlIncidents);
			AssertEquals("Precondition: Group Linked Incidents are linked", 2, group.LinkedIncidents.Count);
			var controlEnabledLogPredicate = new Func<StmALog, bool>(x => x.SL_SE_NKEvent == Events.LockForEditCode);
			var originalControlEnabledLogsCount = group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate);

			group.ING_Status = testStageCode2;
			Factory.Save();

			AssertEquals("Precondition: Group stage changed", testStageCode2, group.NowStage.Code);
			AssertGreaterThan("Should add new control enabled logs", group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate), originalControlEnabledLogsCount);
		}

		public void TestControlIncidentChangeToFalse_ShouldNotEnactControlOnLinkedIncidentsOnSaving()
		{
			//Arrange: Incidents
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "INC000002";

			//Arrange: Tasks
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

			//Arrange: Stages
			var testStageCode1 = "TS1";
			var testStageCode2 = "TS2";
			var description1 = "TestStage 1";
			var description2 = "TestStage 2";

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			//Arrange: Group
			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCriticalities = incidentApprovalLookups.CriticalityList.GetAllCodes();
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var validModule = "CAT";
			product.ModuleMappings.AddNew(validModule, "Category Module", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = validCriticalities[0];
			group.ING_Module = validModule;

			//Arrange: Links
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			Factory.Save();

			//Arrange: Group
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = testStageCode1;

			Factory.Save();
			AssertEquals("Precondition: Group Control Incident is true", true, group.ControlIncidents);
			AssertEquals("Precondition: Group Linked Incidents are linked", 2, group.LinkedIncidents.Count);
			var controlEnabledLogPredicate = new Func<StmALog, bool>(x => x.SL_SE_NKEvent == Events.LockForEditCode);
			var originalControlEnabledLogsCount = group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate);

			group.ING_Status = testStageCode2;
			Factory.Save();

			AssertEquals("Precondition: Group stage changed", testStageCode2, group.NowStage.Code);
			AssertEquals("Should not add new control enabled logs", originalControlEnabledLogsCount, group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate));
		}

		public void TestControlIncidentUnchanged_ShouldNotEnactControlOnLinkedIncidentsOnSaving()
		{
			//Arrange: Incidents
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "INC000002";

			//Arrange: Tasks
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

			//Arrange: Stages
			var testStageCode1 = "TS1";
			var testStageCode2 = "TS2";
			var description1 = "TestStage 1";
			var description2 = "TestStage 2";

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(testStageCode2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			//Arrange: Group
			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCriticalities = incidentApprovalLookups.CriticalityList.GetAllCodes();
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var validModule = "CAT";
			product.ModuleMappings.AddNew(validModule, "Category Module", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = validCriticalities[0];
			group.ING_Module = validModule;

			//Arrange: Links
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			Factory.Save();

			//Arrange: Group
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = testStageCode1;

			Factory.Save();
			AssertEquals("Precondition: Group Control Incident is true", true, group.ControlIncidents);
			AssertEquals("Precondition: Group Linked Incidents are linked", 2, group.LinkedIncidents.Count);
			var controlEnabledLogPredicate = new Func<StmALog, bool>(x => x.SL_SE_NKEvent == Events.LockForEditCode);
			var originalControlEnabledLogsCount = group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate);

			group.ING_Status = testStageCode2;
			Factory.Save();

			AssertEquals("Precondition: Group stage changed", testStageCode2, group.NowStage.Code);
			AssertEquals("Should not add new control enabled logs", originalControlEnabledLogsCount, group.Logs.GetAllLogs().Cast<StmALog>().Count(controlEnabledLogPredicate));
		}

		public void TestAddIncidentMessageReceivedEvent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);

			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.MessageReceived).Parameters;

			var desc = FormattableString.Invariant($"Message(s) received for {incident.Number}");
			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc);
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, incident.Number);

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
		}

		public void TestAddIncidentMessageReceivedEvent_DifferentGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group1.PK;

			Factory.Save();

			group2.ProcessIncidentMessageReceived(incident);
			AssertNull("Should not add log since the group used is not connected to the incident", group1.Logs.MostRecentLogByEventTime(AutoEvents.MessageReceived));
			AssertNull("Should not add log since the group used is not connected to the incident", group2.Logs.MostRecentLogByEventTime(AutoEvents.MessageReceived));
		}

		public void TestAddIncidentMessageReceivedEvent_NoGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			Factory.Save();

			group.ProcessIncidentMessageReceived(incident);
			AssertNull("Should not add log since the incident is not connected to any groups", group.Logs.MostRecentLogByEventTime(AutoEvents.MessageReceived));
		}

		public void TestAddIncidentMessageSentEvent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var participant = incident.EConversation.Conversation.Participants.AddNew();
			participant.JCP_JCC_Conversation = incident.EConversation.Conversation.PK;
			participant.JCP_ParticipantTableCode = OrgContactSchema.Constants.Prefix;
			participant.JCP_ParticipantID = contact.PK;

			incident.EConversation.Conversation.Messages.AddNew(participant, "123456");

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.Flagged = true;

			Factory.Save();
			Assert(link.Flagged);

			var messageType = "Manual";
			var desc = FormattableString.Invariant($"Message(s) sent for {incident.Number}");
			group.AddIncidentMessageSentEvent(incident, messageType, desc);

			var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent).Parameters;
			var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent).Parameters;

			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc);
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, incident.IM_IncidentNumber);
			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, "Manual");

			Assert(groupLogParameters.Contains(pair1));
			Assert(groupLogParameters.Contains(pair2));
			Assert(groupLogParameters.Contains(pair3));
			Assert(incidentLogParameters.Contains(pair1));
			Assert(incidentLogParameters.Contains(pair2));
			Assert(incidentLogParameters.Contains(pair3));

			var unflaggedEventSearchQuery = new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(link.SupportIncident));
			Assert("Link should have been unflagged", !link.Flagged);
			AssertEquals(1, group.Logs.Find(unflaggedEventSearchQuery).Length);
			AssertEquals(1, link.Logs.Find(unflaggedEventSearchQuery).Length);

			Factory.Save();
			Thread.Sleep(100); //Make sure the 'NEXT' log has a greater timestamp. 

			incident.EConversation.Conversation.Messages.AddNew(participant, "7890");
			Assert(link.Flagged);

			messageType = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			desc = "XXXXX";
			group.ING_IsAutoReplyUnflagsCommunication = false;
			group.AddIncidentMessageSentEvent(incident, messageType, desc);

			Assert("Link should have not been unflagged if ING_IsAutoReplyUnflagsCommunication is false and message type is AutoReply.", link.Flagged);
			AssertEquals(1, group.Logs.Find(unflaggedEventSearchQuery).Length);
			AssertEquals(1, link.Logs.Find(unflaggedEventSearchQuery).Length);

			group.ING_IsAutoReplyUnflagsCommunication = true;
			group.AddIncidentMessageSentEvent(incident, messageType, desc);
			Assert("Link should have been unflagged", !link.Flagged);
			AssertEquals(2, group.Logs.Find(unflaggedEventSearchQuery).Length);
			AssertEquals(2, link.Logs.Find(unflaggedEventSearchQuery).Length);
		}

		public void TestAddIncidentMessageSentEvent_DifferentGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group1.PK;

			Factory.Save();

			group2.AddIncidentMessageSentEvent(incident);
			AssertNull("Should not add log since the group used is not connected to the incident", group1.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add log since the group used is not connected to the incident", group2.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add log since the group used is not connected to the incident", incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));

			AssertEquals("The event sending time of unflagged should be the same as that of MSN event", 0, group1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(link.SupportIncident))).Length);
			AssertEquals("The event sending time of unflagged should be the same as that of MSN event", 0, link.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(link.SupportIncident))).Length);
		}

		public void TestAddIncidentMessageSentEvent_NoGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			Factory.Save();

			group.AddIncidentMessageSentEvent(incident);
			AssertNull("Should not add log since the incident is not connected to any groups", group.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));
			AssertNull("Should not add log since the incident is not connected to any groups", incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent));

			AssertEquals("The event sending time of unflagged should be the same as that of MSN event", 0, group.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(incident))).Length);
		}

		public void TestSendBroadcastMessageBasicTest()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.ING_IsBroadcastToControlledOnly = true;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = false;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			group.SendBroadcastMessage(link, message);

			Factory.Save();

			var newMessage = incident.EConversation.Conversation.Messages[0];
			AssertEquals(newMessage.JCM_Body, message.IGM_Message);
			AssertEquals(User.ServiceUserCode, newMessage.SenderCode);
		}

		public void TestControlIncidentChangeToFalse_ShouldLogUCKEvent()
		{
			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCriticalities = incidentApprovalLookups.CriticalityList.GetAllCodes();
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var validModule = "CAT";
			product.ModuleMappings.AddNew(validModule, "Category Module", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Priority = validCriticalities[0];
			group.ING_Module = validModule;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org.PK;

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var newTypeConfig = registryValue.AddNew();
			newTypeConfig.GroupType = "ZZZ";
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = "AAA";
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence;
			groupStatusConfig1.ControlIncidents = true;
			groupStatusConfig1.IncidentCompleted = false;
			groupStatusConfig1.GroupCompleted = false;
			newTypeConfig.IncidentGroupStatusConfigurations.Add(groupStatusConfig1);

			var groupStatusConfig2 = new IncidentGroupStatusConfiguration();
			groupStatusConfig2.Code = "BBB";
			groupStatusConfig2.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Sequence;
			groupStatusConfig2.ControlIncidents = false;
			groupStatusConfig2.IncidentCompleted = false;
			groupStatusConfig2.GroupCompleted = false;
			newTypeConfig.IncidentGroupStatusConfigurations.Add(groupStatusConfig2);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			group.ING_Type = newTypeConfig.GroupType;
			group.ING_Status = groupStatusConfig1.Code;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = true;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = true;
			var link3 = Factory.New<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;
			link3.INL_IsGroupControlled = false;

			Factory.Save();

			AssertEquals("Precondition", true, link1.IsControlled);
			AssertEquals("Precondition", true, link2.IsControlled);
			AssertEquals("Precondition", false, link3.IsControlled);

			group.ING_Status = groupStatusConfig2.Code;
			Factory.Save();

			AssertEquals("Precondition: Control should now be disabled", false, link1.IsControlled);

			var incident1UCKLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit, log => log.SL_Reference.Contains(incident1.Number)).Parameters;
			var incident2UCKLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit, log => log.SL_Reference.Contains(incident2.Number)).Parameters;
			AssertNull("Should not add log for incident which was not controlled", group.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit, log => log.SL_Reference.Contains(incident3.Number)));

			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Control disabled for incident {incident1.Number}"));
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, group.Number);
			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, incident1.Number);

			var pair4 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Control disabled for incident {incident2.Number}"));
			var pair5 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, incident2.Number);

			Assert(incident1UCKLogParameters.Contains(pair1));
			Assert(incident1UCKLogParameters.Contains(pair2));
			Assert(incident1UCKLogParameters.Contains(pair3));

			Assert(incident2UCKLogParameters.Contains(pair4));
			Assert(incident2UCKLogParameters.Contains(pair2));
			Assert(incident2UCKLogParameters.Contains(pair5));
		}

		public void TestGenerateBroadcastMessageLog_ShouldPostUnflaggedEvent()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();

			var link1 = Factory.New<IncidentManagementLink>();
			var link2 = Factory.New<IncidentManagementLink>();
			var link3 = Factory.New<IncidentManagementLink>();
			var link4 = Factory.New<IncidentManagementLink>();

			link1.INL_IM_Incident = incident1.PK;
			link2.INL_IM_Incident = incident2.PK;
			link3.INL_IM_Incident = incident3.PK;
			link4.INL_IM_Incident = incident4.PK;

			link1.INL_ING_Group = group.PK;
			link2.INL_ING_Group = group.PK;
			link3.INL_ING_Group = group.PK;
			link4.INL_ING_Group = group.PK;

			Factory.Save();

			group.Reload();
			AssertEquals(4, group.LinkedIncidents.Count);

			void AssertUnflaggedEvent(string reason, IncidentManagementLink targetLink, int expectedEventNumber)
			{
				AssertEquals($"{expectedEventNumber} unflagged event should be generated, because {reason}(Group)", expectedEventNumber, group.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(targetLink.SupportIncident))).Length);
				AssertEquals($"{expectedEventNumber} unflagged event should be generated, because {reason}(Link)", expectedEventNumber, targetLink.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, IncidentManagementLink.GetUnFlaggedEventReferenceText(targetLink.SupportIncident))).Length);
			}

			var autoReplyMessage = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var interimMessage = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var openningMessage = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var closingMessage = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();

			autoReplyMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			interimMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			openningMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			closingMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;

			var generateBroadcastMessageMethod = typeof(IncidentManagementGroup).GetMethod("GenerateBroadcastMessageLog", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull("The method GenerateBroadcastMessageLog should be found", generateBroadcastMessageMethod);

			//Auto-reply Broadcast Message
			group.ING_IsAutoReplyUnflagsCommunication = false;
			generateBroadcastMessageMethod.Invoke(group, new object[] { link1, autoReplyMessage });
			AssertUnflaggedEvent("ING_IsAutoReplyUnflagsCommunication is false", link1, 0);

			group.ING_IsAutoReplyUnflagsCommunication = true;
			generateBroadcastMessageMethod.Invoke(group, new object[] { link1, autoReplyMessage });
			AssertUnflaggedEvent("Sending manually", link1, 1);

			//Interim Broadcast Message
			group.ING_IsInterimBroadcastUnflagsCommunication = false;
			generateBroadcastMessageMethod.Invoke(group, new object[] { link2, interimMessage });
			AssertUnflaggedEvent("ING_IsInterimBroadcastUnflagsCommunication is false", link2, 0);

			group.ING_IsInterimBroadcastUnflagsCommunication = true;
			generateBroadcastMessageMethod.Invoke(group, new object[] { link2, interimMessage });
			AssertUnflaggedEvent("ING_IsInterimBroadcastUnflagsCommunication is true", link2, 1);

			//Openning Broadcast Message
			generateBroadcastMessageMethod.Invoke(group, new object[] { link3, openningMessage });
			AssertUnflaggedEvent("ING_IsInterimBroadcastUnflagsCommunication is true", link3, 1);

			//Closing Broadcast Message
			generateBroadcastMessageMethod.Invoke(group, new object[] { link4, closingMessage });
			AssertUnflaggedEvent("ING_IsInterimBroadcastUnflagsCommunication is true", link4, 1);
		}

		public void TestSupportedRelatedItemModules()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Assert(group.SupportedRelatedItemModules.Any(x => x.ControllerID == ControllerIDs.WorkItem));
			Assert(group.SupportedRelatedItemModules.Any(x => x.ControllerID == ControllerIDs.Project));
			Assert(group.SupportedRelatedItemModules.Any(x => x.ControllerID == ClientControllerRegistration.SupportIncident));
		}

		public void TestShowModuleGridData()
		{
			#region Assigned to Name/Code

			var basegroup = Factory.New<IncidentManagementGroup>();
			basegroup.ING_BusinessImpact = "XXX";
			basegroup.ING_ServiceOutage = "XXX";
			basegroup.ING_Urgency = "XXX";
			basegroup.ING_Product = "XXX";
			basegroup.ING_ProductArea = "XXX";

			var task = basegroup.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_GG_AssignedGroupCode = "ALL";
			task.P9_G4_RequiredCapability = ZGuid.Empty;

			AssertNull(task.AssignedStaffMember);

			AssertNullOrEmpty("BusinessImpact", basegroup.BusinessImpact);
			AssertNullOrEmpty("ServiceOutageStatusDescription", basegroup.ServiceOutageStatusDescription);
			AssertNullOrEmpty("CapabilityDescription", basegroup.CapabilityDescription);
			AssertNullOrEmpty("CapabilityCode", basegroup.CapabilityCode);
			AssertNullOrEmpty("AssignedToUserName", basegroup.AssignedToUserName);
			AssertNullOrEmpty("UrgencyDescription", basegroup.UrgencyDescription);
			AssertNullOrEmpty("ProductDescription", basegroup.ProductDescription);
			AssertNullOrEmpty("ProductAreaDescription", basegroup.ProductAreaDescription);
			AssertEquals("basegroup.HasAutoReply", false, basegroup.HasAutoReply);

			#endregion
		}

		public void TestSetModuleShouldRecalculateProductArea()
		{
			#region Test Data

			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("CFA", "Category Fruits Area A");
			productAreasList.AddPair("CFB", "Category Fruits Area B");
			productAreasList.AddPair("8FA", "CR8 Fruits Area A");
			productAreasList.AddPair("8FB", "CR8 Fruits Area B");
			productAreasList.AddPair("9FA", "CR9 Fruits Area A");
			productAreasList.AddPair("9FB", "CR9 Fruits Area B");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			products.ModuleMappings.AddNew("APP", "Apples", "CFA", false);
			products.ModuleMappings.AddNew("TOM", "Tomatoes", "CFB", false);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			products.ModuleMappings.AddNew("APP", "Apples", "8FA", false);
			products.ModuleMappings.AddNew("BAN", "Bannanas", "8FB", false);

			var nonEDIproduct = collection.AddNew();
			nonEDIproduct.Code = "VEG";
			nonEDIproduct.Description = (NoResString)"Vegetables";
			var nonEDImodule = nonEDIproduct.ModuleMappings.AddNew();
			nonEDImodule.ModuleCode = "TOM";
			nonEDImodule.ModuleDescription = (NoResString)"Tomatoes";
			nonEDImodule.ProductArea = "CFA";

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			products.ModuleMappings.AddNew("APP", "Apples", "9FA", false);
			products.ModuleMappings.AddNew("PEA", "Pears", "9FB", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			#endregion

			var group = Factory.New<IncidentManagementGroup>();
			group.ING_Product = "ENT";
			group.ING_Priority = "CR4";
			group.ING_Module = "APP";
			var productAreaList = group.Lookups.ProductAreaList;
			AssertEquals("CFA", group.GetRecalculatedProductArea());
			AssertContainsExactElementsInAnyOrder(new[] { "CFA", "CFB" }, productAreaList.GetAllCodes());

			group.ING_Priority = "CR8";
			group.ING_Module = "APP";
			productAreaList = group.Lookups.ProductAreaList;
			AssertEquals("8FA", group.GetRecalculatedProductArea());
			AssertContainsExactElementsInAnyOrder(new[] { "8FA", "8FB" }, productAreaList.GetAllCodes());

			group.ING_Priority = "CR9";
			group.ING_Module = "APP";
			productAreaList = group.Lookups.ProductAreaList;
			AssertEquals("9FA", group.GetRecalculatedProductArea());
			AssertContainsExactElementsInAnyOrder(new[] { "9FA", "9FB" }, productAreaList.GetAllCodes());

			group.ING_Product = "VEG";
			group.ING_Module = "TOM";
			productAreaList = group.Lookups.ProductAreaList;
			AssertEquals("", group.GetRecalculatedProductArea());
			AssertEquals(0, productAreaList.Count);

			group.ING_Priority = "CR8";
			productAreaList = group.Lookups.ProductAreaList;
			AssertEquals("CFA", group.GetRecalculatedProductArea());
			AssertContainsExactElementsInAnyOrder(new[] { "CFA" }, productAreaList.GetAllCodes());
		}

		#region Notes

		public void TestNoteTypes()
		{
			var incident = (IncidentManagementGroup)GetNewBusinessObject();
			var expectedNoteTypes = GetExpectedNoteTypes();
			AssertEquals(expectedNoteTypes.Length, incident.NoteTypes.Count);
			foreach (var expectedNoteType in expectedNoteTypes)
			{
				AssertCollectionContains(expectedNoteType, incident.NoteTypes);
			}
		}

		PredefinedNoteType[] GetExpectedNoteTypes()
		{
			return new PredefinedNoteType[]
			{
				EDIPredefinedNoteTypes.Instance.IncidentManagementGroupInitialSymptoms,
				EDIPredefinedNoteTypes.Instance.IncidentManagementGroupBusinessImpactDescription,
				EDIPredefinedNoteTypes.Instance.IncidentManagementGroupRootCause
			};
		}

		#endregion

		public void TestEConversationEmailNotification()
		{
			//Internal			
			var staff = Factory.New<GlbStaff>();
			var staffGG1 = Factory.New<GlbStaff>();
			var staffGG2 = Factory.New<GlbStaff>();
			var staffUnsubscribed = Factory.New<GlbStaff>();

			//External
			var externalEmailAddress = "external@123.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var contactUnsubscribed = Factory.NewWithValidTestData<OrgContact>();

			staff.GS_Code = "S11";
			staffGG1.GS_Code = "GG1";
			staffGG2.GS_Code = "GG2";
			staffUnsubscribed.GS_Code = "SU1";

			staff.GS_LoginName = "S11";
			staffGG1.GS_LoginName = "GG1";
			staffGG2.GS_LoginName = "GG2";
			staffUnsubscribed.GS_LoginName = "SU1";

			staff.GS_EmailAddress = "staff@123.com";
			staffGG1.GS_EmailAddress = "GG1@123.com";
			staffGG2.GS_EmailAddress = "GG2@123.com";
			staffUnsubscribed.GS_EmailAddress = "staffUnsubscribed@123.com";

			contact.OC_Email = "contact@123.com";
			contact.OC_Email = "contactUnSubscribed@123.com";

			var staffGroup = Factory.New<GlbGroup>();
			staffGroup.GG_Code = "AAA";
			staffGroup.GG_Desc = "AAAAAAA";
			staffGroup.Staff.Add(staffGG1);
			staffGroup.Staff.Add(staffGG2);

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.EConversation.Conversation.Participants.AddNewParticipant(contactUnsubscribed).JCP_IsSubscribed = false;
			group.EConversation.Conversation.Participants.AddNewParticipant(staffUnsubscribed).JCP_IsSubscribed = false;
			group.EConversation.Conversation.Participants.AddNewParticipant(staffGroup).JCP_IsSubscribed = true;
			group.EConversation.Conversation.Participants.AddNewParticipant(contact).JCP_IsSubscribed = true;
			group.EConversation.Conversation.Participants.AddNewParticipant(staff).JCP_IsSubscribed = true;
			var externalEmailParticipant = group.EConversation.Conversation.Participants.AddNew();
			externalEmailParticipant.EmailAddress = externalEmailAddress;
			externalEmailParticipant.JCP_IsSubscribed = true;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			group.EConversation.AddMessageFromCurrentUser("PublicMessage", false, false);
			Factory.Save();
			var recipients = Env.OutgoingMailManager.EmailsCreated.SelectMany(x => x.Recipients.ToList<RecipientDef>().Select(y => y.Email)).Distinct();

			AssertEquals(5, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(5, recipients.Count());
			Assert("All emails should contain the new message", Env.OutgoingMailManager.EmailsCreated.All(x => x.Body.Contains("PublicMessage")));
			AssertEquals("All subscribed should receive the notification", 0, recipients.Except(new string[]
			{
				staff.GS_EmailAddress ,
				staffGG1.GS_EmailAddress ,
				staffGG2.GS_EmailAddress ,
				externalEmailAddress,
				contact.OC_Email
			}).Count());

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			group.EConversation.AddMessageFromCurrentUser("InternalMessage", true, false);
			Factory.Save();
			recipients = Env.OutgoingMailManager.EmailsCreated.SelectMany(x => x.Recipients.ToList<RecipientDef>().Select(y => y.Email)).Distinct();

			AssertEquals(3, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(3, recipients.Count());
			Assert("All emails should contain the new message", Env.OutgoingMailManager.EmailsCreated.All(x => x.Body.Contains("InternalMessage")));
			AssertEquals("Only staff should receive the notification", 0, recipients.Except(new string[]
			{
				staff.GS_EmailAddress,
				staffGG1.GS_EmailAddress,
				staffGG2.GS_EmailAddress
			}).Count());
		}

		public void TestShouldHaveErrorWhenSaveIncidentManagementGroupWithInvalidStage()
		{
			var group = Factory.New<IncidentManagementGroup>();
			group.ING_Status = "INV";
			group.ING_Type = "MIM";
			Factory.Save();

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadedGroup = factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));
			loadedGroup.ING_Description = "test";
			loadedGroup.Validation.ValidateING_Status();
			AssertHasError(loadedGroup.ING_StatusInfo, "The current status is invalid. You should reload the form to select a valid status before continuing.");
		}

		public void TestAutoLogDisabled()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();

			incidentManagementGroup.ING_ProductArea = "ENT";
			Factory.Save();

			var loadedObject = (new BusinessObjectFactory() { RefreshEnabled = false }).Load<IncidentManagementGroup>(incidentManagementGroup.PK);

			AssertEquals(0, loadedObject.Logs.Find(x => x.SL_SE_NKEvent == "ADD").Count());
			AssertEquals(0, loadedObject.Logs.Find(x => x.SL_SE_NKEvent == "EDT").Count());
		}

		#region Custom Fields

		[TestedType(typeof(IncidentManagementGroup))]
		class IncidentManagementGroupCustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestCustomFields()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			var system = bmTestHelper.CreateSystem(Factory, "ING");
			Factory.Save();

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "ING", null, ProductTypes.Codes.Enterprise);

			var custom1 = template.GenCustomColumnDefinitions.AddNew();
			custom1.XC_Name = "Service Type";
			custom1.XC_Type = AddOnColumnDataType.Codes.String;

			var custom2 = template.GenCustomColumnDefinitions.AddNew();
			custom2.XC_Name = "My Bool";
			custom2.XC_Type = AddOnColumnDataType.Codes.Boolean;

			Factory.Save();

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Product = ProductTypes.Codes.Enterprise;

			Factory.Save();

			var customFields = (group as ICustomFieldProvider).GetCustomBusinessObject();
			AssertNotNull(customFields["__SERVICE TYPE__prop__ZString"]);
			AssertNotNull(customFields["__MY BOOL__prop__ZBool"]);
		}

		#endregion
	}

	class IncidentManagementGroupForTest : IncidentManagementGroup
	{
		public IncidentManagementGroupForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsProductDetailsChangedCauseWorkflowTemplateBeingAppliedForTest
		{
			get
			{
				return IsProductDetailsChangedCauseWorkflowTemplateBeingApplied;
			}
		}

		public ZBool HasProductDetailsChangedForPostWorkflowForTest
		{
			get
			{
				return base.HasProductDetailsChangedForPostWorkflow;
			}
		}
	}

	[TestedType(typeof(IncidentManagementGroup))]
	class IncidentManagementGroupWorkTaskRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(IncidentManagementGroup))]
	class IncidentManagementGroupWorkTaskRelatedItemTest : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => FormattableString.Invariant($"{EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value[0].GroupType} - {EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value[0].Description}");

		protected override string ExpectedSelectionCriterion2 => "INA";

		protected override string ExpectedSelectionCriterion3 => "INP";

		protected override string ExpectedSelectionCriterion4 => string.Empty;

		protected override string ExpectedSelectionCriterion5 => string.Empty;

		protected override Type ExpectedPivotCollectionType => typeof(GenPivotCollection);

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var bizo = Factory.New<IncidentManagementGroup>();
			bizo.ING_Type = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value[0].GroupType;
			bizo.ING_ProductArea = "INA";
			bizo.ING_Product = "INP";
			return bizo;
		}
	}

	class IncidentCustomerNotifierForTest : IncidentCustomerNotifier
	{
		public IncidentCustomerNotifierForTest(SupportIncident incident)
			: base(incident, new WebRequestNotificationSender())
		{
		}

		protected override SupportIncidentEmail CreateStandardEmailIfApplicable()
		{
			return null;
		}
	}
}
