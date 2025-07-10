using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.Test.IncidentManager.WorkItem
{
	public class EDIWorkItemFetchForViewTest : TestCaseWithFactory
	{
		public void TestFetchHintAreAddedOnWorkItemProcessHeadersInFetchForView()
		{
			// Arrange
			var collection = (IBusinessObjectCollection)GetTestDataAsCollection();

			// Act
			collection.FetchStrategy.FetchForView(collection.ToArray(), GetTableColumns());

			// Assert
			CombineAssertions(() =>
			{
				AssertTableHasActiveFetchHints(ProcessTasksSchema.Constants.TableName, 12);
				AssertTableHasActiveFetchHints(ProcessHeaderLinkSchema.Constants.TableName, 24);
			});
		}

		TableColumn[] GetTableColumns()
		{
			return new TableColumn[]
			{
				new (WorkItemSchema.Constants.TableName, WorkItemSchema.Constants.WKI_WorkItemNumber),
				new (WorkItemSchema.Constants.TableName, WorkItemSchema.Constants.WKI_Summary),
				new (WorkItemSchema.Constants.TableName, nameof(NewWorkItem.AssignedToCode)),
				new (WorkItemSchema.Constants.TableName, nameof(NewWorkItem.CurrentTask))
			};
		}

		void AssertTableHasActiveFetchHints(string tableName, int expected)
		{
			var actual = Factory.ActiveFetchHintsForTable(tableName);

			AssertEquals("Number of fetch hints should be equal to expected.", expected, actual);
		}

		EDIWorkItemCollection GetTestDataAsCollection()
		{
			// Required to add Workflows
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			var type = system.RelatedWorkflowTypes.AddNew();
			type.FSW_WorkflowType = "WKI";
			Factory.Save();

			var collection = new EDIWorkItemCollection(Factory);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var glbDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			for (var i = 0; i < 6; i++)
			{
				var workItem = collection.AddNew();

				workItem.WKI_ActivityType = workItem.Lookups.ActiveActivityTypes[0].Code;
				workItem.WKI_ActivitySubtype = workItem.Lookups.ActiveActivitySubtypes[0].Code;
				workItem.WKI_Priority = workItem.Lookups.AllPriorities[0].Code;
				workItem.WKI_WorkItemArea = workItem.Lookups.ActiveAreas[0].Code;
				workItem.WKI_PortOrCountry = "CAYHZ";
				workItem.WKI_GC_AssignedCompany = company.PK;
				workItem.WKI_GE_AssignedDepartment = glbDepartment.PK;

				var workflow = workItem.Workflows.AddNew();
				var workflow2 = workItem.Workflows.AddNew();

				var task1 = workItem.WorkflowItems.AddNew();
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				task1.P9_G4_RequiredCapability = capability.PK;
				task1.P9_Description = "AAA";
				task1.P9_FH_ProcessHeader = workflow2.PK;

				var task2 = workItem.WorkflowItems.AddNew();
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				task2.P9_Description = "BBB";
				task2.P9_FH_ProcessHeader = workflow.PK;
			}
			Factory.Save();

			return collection;
		}
	}
}
