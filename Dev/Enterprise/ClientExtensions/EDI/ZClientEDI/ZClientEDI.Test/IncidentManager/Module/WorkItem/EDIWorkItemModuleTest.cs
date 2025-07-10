using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(EDIWorkItemModule))]
	public class EDIWorkItemModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkItem;
		}

		public void TestRecentItemsModule()
		{
			using (var module = new EDIWorkItemModule())
			{
				AssertEquals("Use WorkItem module ID for recent items", ModuleIDs.WorkItem.Name, module.RecentItemsModuleID.Name);
			}
		}

		protected override void SetupDataForFetchHintsTest()
		{
			var factory = new BusinessObjectFactory();
			var capability = factory.NewWithValidTestData<GlbCapability>();
			var company = factory.NewWithValidTestData<GlbCompany>();
			var glbDepartment = factory.NewWithValidTestData<GlbDepartment>();
			capability.G4_Description = "AAA";

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var system = factory.NewWithValidTestData<BMSystem>();
			var type = system.RelatedWorkflowTypes.AddNew();
			type.FSW_WorkflowType = "WKI";
			factory.Save();

			for (int i = 0; i < 6; i++)
			{
				var workItem = factory.NewWithValidTestData<NewWorkItem>();
				var task1 = workItem.WorkflowItems.AddNew();
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				task1.P9_G4_RequiredCapability = capability.PK;
				task1.P9_Description = "AAA";
				workItem.WKI_ActivityType = workItem.Lookups.ActiveActivityTypes[0].Code;
				workItem.WKI_ActivitySubtype = workItem.Lookups.ActiveActivitySubtypes[0].Code;
				workItem.WKI_Priority = workItem.Lookups.AllPriorities[0].Code;
				workItem.WKI_WorkItemArea = workItem.Lookups.ActiveAreas[0].Code;
				workItem.WKI_PortOrCountry = "CAYHZ";
				workItem.WKI_GC_AssignedCompany = company.PK;
				workItem.WKI_GE_AssignedDepartment = glbDepartment.PK;

				var processHeader = workItem.Workflows.AddNew();
				workItem.JobWorkflow.AgreedDeliveryDateLocal = new ZDateTime(2018, 7, 3, 11, 0, 0);
				processHeader.AgreedDeliveryDateLocal = new ZDateTime(2018, 7, 3, 11, 0, 0);

				var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "PM1");
				if (staff == null)
				{
					staff = factory.New<GlbStaff>();
					staff.GS_Code = "PM1";
					staff.GS_FullName = "Mr. " + "PM1";
				}

				var project1 = factory.New<EDIProject>();
				project1.WKP_ProjectNumber = $"PRJ1{i}";
				project1.WKP_GS_NKProjectManager = staff.GS_Code;

				var issue1 = factory.New<EdiHelpErrorLog>();
				workItem.RelatedItems.Add(issue1);

				var request1 = factory.New<SupportIncident>();
				request1.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

				project1.RelatedItems.Add(request1);
				request1.RelatedItems.Add(workItem);

				var client = factory.NewWithValidTestData<OrgHeader>();
				client.OH_FullName = $"Name{i}";
				client.OH_Code = $"Client1{i}";

				var relatedItem = factory.NewWithValidTestData<SupportIncident>();
				relatedItem.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				relatedItem.IM_OH_Client = client.PK;

				var incident1 = factory.New<SupportIncident>();
				incident1.IM_Priority = "CR6";
				incident1.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

				relatedItem.RelatedItems.Add(workItem);
				incident1.RelatedItems.Add(workItem);
			}
			factory.Save();
		}

		protected override ZFilterModule CreateModuleForFetchHintsTest()
		{
			return new EDIWorkItemModule();
		}
	}
}
