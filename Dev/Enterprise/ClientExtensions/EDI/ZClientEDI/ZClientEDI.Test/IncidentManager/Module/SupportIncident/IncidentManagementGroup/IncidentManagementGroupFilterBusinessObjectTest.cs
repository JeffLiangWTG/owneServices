using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentManagementGroupFilterBusinessObject))]
	public class IncidentManagementGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IncidentManagementGroupFilterBusinessObject();
		}

		(IncidentManagementGroup, IncidentManagementGroup, IncidentManagementGroup) CreateGroupsForTest()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FMT";

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Product = ProductTypes.Codes.CargoWiseOne;
			group1.ING_ProductArea = "XRM";
			group1.ING_Description = "Group Desc 1";
			group1.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group1.ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.Significant;
			group1.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.Medium;
			group1.ING_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;

			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_Product = ProductTypes.Codes.CargoWiseOne;
			group2.ING_Description = "Group Desc 2";
			group2.ING_IsAutoReply = true;
			group2.ING_ServiceOutage = ServiceOutageCodes.Downgraded;
			group2.ING_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			group2.ING_RN_NKCountry = "AU";
			group2.ING_Module = "BUF";

			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group3.ING_Product = ProductTypes.Codes.Enterprise;
			group3.ING_Description = "Group Desc 3";
			group3.ING_GS_NKGroupOwner = staff.GS_Code;
			group3.ING_SourceModuleId = "MAA";
			group3.ING_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;

			Factory.Save();

			return (group1, group2, group3);
		}

		public void TestGroupNumberFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Group Number"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = group1.ING_IncidentGroupNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = group1.ING_IncidentGroupNumber;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestDescriptionFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Description"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "Group Desc 2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "Group Desc 2";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestGroupTypeFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Group Type"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestStageFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Stage"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestHasAutoReplyFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			// Call it to create default messages
			var messages1 = group1.IncidentManagementGroupMessages;
			var messages2 = group2.IncidentManagementGroupMessages;
			var messages3 = group3.IncidentManagementGroupMessages;

			group2.ING_IsAutoReply = false;
			var group2AutoReply = messages2.Cast<IncidentManagementGroupMessage>().First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			group2AutoReply.IGM_IsPublished = true;

			group3.ING_IsAutoReply = true;
			var group3AutoReply = messages3.Cast<IncidentManagementGroupMessage>().First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			group3AutoReply.IGM_IsPublished = true;

			Factory.Save();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Has Auto-Reply"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "DFT";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "PUB";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "ENB";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestRelatedWorkItemsFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleGuidFilter)incidentManagementGroupFilter["Related Work Items"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			var relatedWorkItem0 = Factory.New<WorkItem>();
			group3.AutoCascadeRelatedItems.Add(relatedWorkItem0);
			Factory.Save();

			group3.RelatedItems.Add(relatedWorkItem0);
			filter.Property = relatedWorkItem0.PK;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group3", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group3", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group3.PK));

			var relatedWorkItem = Factory.New<WorkItem>();
			group1.RelatedItems.Add(relatedWorkItem);
			Factory.Save();

			filter.Property = relatedWorkItem.PK;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));
		}

		public void TestBusinessImpactFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Business Impact"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = IncidentManagementGroupConstants.BusinessImpactCodes.Significant;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = IncidentManagementGroupConstants.BusinessImpactCodes.Significant;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestServiceOutageStatusFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Service Outage Status"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = ServiceOutageCodes.Downgraded;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = ServiceOutageCodes.Downgraded;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestOwnerFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleNkFilter)incidentManagementGroupFilter["Owner"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "FMT";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group3", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group3", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group3.PK));

			filter.Property = "FMT";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group3", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group3", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group3.PK));
		}

		public void TestUrgencyCodeFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Urgency Code"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = IncidentManagementGroupConstants.UrgencyCodes.Medium;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = IncidentManagementGroupConstants.UrgencyCodes.Medium;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestCriticalityFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Criticality"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestProductFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Product"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = ProductTypes.Codes.Enterprise;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group3", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group3", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group3.PK));

			filter.Property = ProductTypes.Codes.Enterprise;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group3", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group3", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group3.PK));
		}

		public void TestProductAreaFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Product Area"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "XRM";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group1", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "XRM";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group1", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestMenuItemFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Menu Item"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "MAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group3", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group3", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group3.PK));

			filter.Property = "MAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group3", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group3", incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group3.PK));
		}

		public void TestMenuSectionFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Menu Section"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "BUF";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "BUF";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		public void TestCountryFilter()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Country/Region"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "AU";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group2", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group2", !incidentManagementGroup.Contains(group3.PK));

			filter.Property = "AU";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group2", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group2", incidentManagementGroup.Contains(group3.PK));
		}

		[TestDate(2022, 01, 06)]
		public void TestHasCommunicationFilter()
		{
			var customer = Factory.NewWithValidTestData<OrgContact>();
			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group4 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link4 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_ING_Group = group1.PK;
			link2.INL_ING_Group = group2.PK;
			link3.INL_ING_Group = group3.PK;
			link4.INL_ING_Group = group4.PK;

			var conversation1 = link1.SupportIncident.EConversation.Conversation;
			var conversationMessage1 = conversation1.Messages.AddNew();
			conversationMessage1.JCM_Body = "Message body 1";
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
			conversationMessage2.JCM_Body = "Message body 2";
			conversationMessage2.JCM_IsInternal = false;
			conversationMessage2.JCM_IsLocal = false;
			conversationMessage2.JCM_PostedTimeUtc = new ZDateTime(2022, 01, 06, 22, 30, 25);

			var participant2 = conversation2.Participants.GetOrAdd(customer);
			conversationMessage2.JCM_JCP_Participant = participant2.PK;

			var conversation3 = link3.SupportIncident.EConversation.Conversation;
			var conversationMessage3 = conversation3.Messages.AddNew();
			conversationMessage3.JCM_Body = "Message body 3";
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

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentManagementGroupFilter["Has Flagged Incident"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should only contain group3", !incidentManagementGroup.Contains(group1.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group2.PK));
			Assert("Should only contain group3", incidentManagementGroup.Contains(group3.PK));
			Assert("Should only contain group3", !incidentManagementGroup.Contains(group4.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group3", incidentManagementGroup.Contains(group1.PK));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group2.PK));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group3.PK));
			Assert("Should not contain group3", incidentManagementGroup.Contains(group4.PK));
		}

		public void TestGroupCompleted()
		{
			var customGroupType = "TMP";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue.AddNew(customGroupType, "Custom group type");

			var mimGroupType = registryValue.Cast<IncidentGroupType>().FirstOrDefault(f => f.GroupType == IncidentGroupStatusConfigurationConstants.MajorIncidentCode);
			var inv = mimGroupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(f => f.Code == "INV");
			inv.GroupCompleted = true;
			var psi = mimGroupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(f => f.Code == "PSI");
			psi.GroupCompleted = true;

			var tmpGroupType = registryValue.Cast<IncidentGroupType>().FirstOrDefault(f => f.GroupType == customGroupType);
			var esc = tmpGroupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(f => f.Code == "ESC");
			esc.GroupCompleted = true;
			var aci = tmpGroupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(f => f.Code == "ACI");
			aci.GroupCompleted = true;

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group1.ING_Status = "INV";
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group2.ING_Status = "ESC";
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group3.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group3.ING_Status = "PSI";
			var group4 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group4.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group4.ING_Status = "ACI";

			var group5 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group5.ING_Type = customGroupType;
			group5.ING_Status = "INV";
			var group6 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group6.ING_Type = customGroupType;
			group6.ING_Status = "ESC";
			var group7 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group7.ING_Type = customGroupType;
			group7.ING_Status = "PSI";
			var group8 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group8.ING_Type = customGroupType;
			group8.ING_Status = "ACI";

			Factory.Save();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleFlagsFilter)incidentManagementGroupFilter["Group Completed"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property0 = true;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("MIM group type: Should contain group1", incidentManagementGroup.Contains(group1.PK));
			Assert("MIM group type: Should not contain group2", !incidentManagementGroup.Contains(group2.PK));
			Assert("MIM group type: Should contain group3", incidentManagementGroup.Contains(group3.PK));
			Assert("MIM group type: Should not contain group4", !incidentManagementGroup.Contains(group4.PK));

			Assert("TMP group type: Should not contain group5", !incidentManagementGroup.Contains(group5.PK));
			Assert("TMP group type: Should contain group6", incidentManagementGroup.Contains(group6.PK));
			Assert("TMP group type: Should not contain group7", !incidentManagementGroup.Contains(group7.PK));
			Assert("TMP group type: Should contain group8", incidentManagementGroup.Contains(group8.PK));

			filter.Property0 = false;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("MIM group type: Should not contain group1", !incidentManagementGroup.Contains(group1.PK));
			Assert("MIM group type: Should contain group2", incidentManagementGroup.Contains(group2.PK));
			Assert("MIM group type: Should not contain group3", !incidentManagementGroup.Contains(group3.PK));
			Assert("MIM group type: Should contain group4", incidentManagementGroup.Contains(group4.PK));

			Assert("TMP group type: Should contain group5", incidentManagementGroup.Contains(group5.PK));
			Assert("TMP group type: Should not contain group6", !incidentManagementGroup.Contains(group6.PK));
			Assert("TMP group type: Should contain group7", incidentManagementGroup.Contains(group7.PK));
			Assert("TMP group type: Should not contain group8", !incidentManagementGroup.Contains(group8.PK));
		}

		public void TestAddWorkflowCustomFieldFilters()
		{
			var (group1, group2, group3) = CreateGroupsForTest();

			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "ING";
			GenCustomColumnDefinition def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "CustomField1";
			def1.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "ING";
			GenCustomColumnDefinition def2 = template1.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "CustomField2";
			def2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			
			ModuleFilterCollection filterCollection = new IncidentManagementGroupFilterBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["CustomField1"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["CustomField2"].GetType());

			group1.SetPossiblyCustomProperty("__CUSTOMFIELD1__prop__ZString", new ZString("Hello"));
			group2.SetPossiblyCustomProperty("__CUSTOMFIELD1__prop__ZString", new ZString("Hello"));
			group3.SetPossiblyCustomProperty("__CUSTOMFIELD1__prop__ZString", new ZString("Goodbye"));
			group1.SetPossiblyCustomProperty("__CUSTOMFIELD2__prop__ZInt", new ZInt(15));
			group2.SetPossiblyCustomProperty("__CUSTOMFIELD2__prop__ZInt", new ZInt(50));
			group3.SetPossiblyCustomProperty("__CUSTOMFIELD2__prop__ZInt", new ZInt(50));
			Factory.Save();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["CustomField1"];
			var incidentManagementGroup = new IncidentManagementGroupCollection(Factory);

			filter.Property = "Hello";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should contain group1", incidentManagementGroup.Contains(group1));
			Assert("Should contain group2", incidentManagementGroup.Contains(group2));
			Assert("Should not contain group3", !incidentManagementGroup.Contains(group3));

			var filter2 = (ModuleNumberRangeFilter)incidentManagementGroupFilter["CustomField2"];
			filter2.Property1 = 50;
			filter.IsActive = false;
			filter2.IsActive = true;
			incidentManagementGroup.Load(incidentManagementGroupFilter.Filter);

			Assert("Should not contain group1", !incidentManagementGroup.Contains(group1));
			Assert("Should contain group2", incidentManagementGroup.Contains(group2));
			Assert("Should contain group3", incidentManagementGroup.Contains(group3));
		}

		#region Participant Filters

		public void TestContactParticipantOfIncidentManagementGroupFilter()
		{
			var filterObj = new IncidentManagementGroupFilterBusinessObject();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "Contact 1";
			group1.EConversation.Conversation.RelatedParties.AddNewParticipant(orgContact);
			Factory.Save();

			var filterResults = new IncidentManagementGroupCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Contact Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Name");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "Contact 1";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));
		}

		public void TestStaffParticipantOfIncidentManagementGroupFilter()
		{
			var filterObj = new IncidentManagementGroupFilterBusinessObject();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CG";
			group1.EConversation.Conversation.RelatedParties.AddNewParticipant(staff);
			Factory.Save();

			var filterResults = new IncidentManagementGroupCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Staff Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "CG";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));
		}

		public void TestOrganizationParticipantOfIncidentManagementGroupFilter()
		{
			var filterObj = new IncidentManagementGroupFilterBusinessObject();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DEMOSYD";
			var participant = group1.EConversation.Conversation.RelatedParties.AddNewParticipant(orgHeader);
			participant.RelatedPartyTypeName = "Organization";
			AssertEquals(false, participant.JCP_IsSubscribed);
			Factory.Save();

			var filterResults = new IncidentManagementGroupCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Organization Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "DEMOSYD";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));
		}

		public void TestGroupParticipantOfIncidentManagementGroupFilter()
		{
			var filterObj = new IncidentManagementGroupFilterBusinessObject();

			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();
			glbGroup.GG_Code = "PMG55";
			group1.EConversation.Conversation.RelatedParties.AddNewParticipant(glbGroup);
			Factory.Save();

			var filterResults = new IncidentManagementGroupCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Group Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "PMG55";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));
		}

		public void TestHasEmailParticipantOfIncidentManagementGroupFilter()
		{
			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group1.EConversation.Conversation.RelatedParties.AddNewParticipant("417666@hh.com");
			Factory.Save();

			var incidentManagementGroupFilter = new IncidentManagementGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentManagementGroupFilter["Has Email Participant"];
			var filterResults = new IncidentManagementGroupCollection(Factory);

			filter.Property = "417666@hh.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterResults.Load(incidentManagementGroupFilter.Filter);

			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));

			filter.Property = "417666";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filterResults.Load(incidentManagementGroupFilter.Filter);

			Assert("Filter should return group1", filterResults.Contains(group1));
			Assert("Filter should not return group2", !filterResults.Contains(group2));
			Assert("Filter should not return group3", !filterResults.Contains(group3));
		}

		#endregion
	}
}
