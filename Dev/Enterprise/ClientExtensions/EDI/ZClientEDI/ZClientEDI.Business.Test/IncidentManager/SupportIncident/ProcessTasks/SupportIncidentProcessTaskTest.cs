using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentProcessTask))]
	public class SupportIncidentProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentType()
		{
			CachedTask.P9_ParentID = Factory.New<SupportIncident>().PK;
			AssertEquals(typeof(SupportIncident), CachedTask.Parent.GetType());
		}

		public void TestParentControllerID()
		{
			AssertEquals(ClientControllerRegistration.SupportIncident, CachedTask.ParentControllerID);
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		[TestDate(2014, 7, 18, 14, 0, 21)]
		public void TestEstimateDefaultedFromDate_TimeOfTaskCopiedFromTemplate()
		{
			var incident = Factory.New<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Sequence = 10;
			task.P9_EstimatedDefaultedFrom = SupportIncidentEstimateDefaultedFromList.Codes.TimeOfTaskCopiedFromTemplate;
			task.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.Now.Year, 1, 1, 6, 0, 0);
			Factory.Save();
			Assert(!task.P9_ScheduledDate.IsEmpty);

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			AssertEquals("Scheduled date from db should match value in memory", task.P9_ScheduledDate, loadedTask.P9_ScheduledDate);

			loadedTask.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddHours(10));
			var scheduledDate = loadedTask.P9_ScheduledDate;
			loadedTask.Factory.Save();
			AssertEquals("Scheduled date should not re-default if value exists", scheduledDate, loadedTask.P9_ScheduledDate);

			loadedTask.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Empty);
			loadedTask.Factory.Save();
			AssertEquals("Scheduled date should not re-default if value exists", ZDateTime.Empty, loadedTask.P9_ScheduledDate.ToZDateTime());
		}

		public void TestStatusChange()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			ZString currentOrNextTaskStatus = ZString.Empty;
			incident.OnCurrentOrNextTaskStatusChange += (s, e) => { currentOrNextTaskStatus = incident.CurrentOrNextTask.P9_Status; };

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, currentOrNextTaskStatus);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedTask = anotherFactory.Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			anotherFactory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, currentOrNextTaskStatus);
		}

		public void TestShouldNotAutoSaveParent_WhenDeletingTaskAndParentIsUnsaved()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			Assert(!incident.IsInDatabase);

			incident.OnCloseIncident += (s, e) =>
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			};

			task.Delete();
			Assert(!incident.IsInDatabase);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			return incident.WorkflowItems.AddNew();
		}

		ProcessTask CachedTask
		{
			get { return (ProcessTask)CachedBusinessObject; }
		}
	}
}
