using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class ShelvesetToolsTest : TestCaseWithFactory
	{
		public void TestGetScheduledShelvesByStatusWithName()
		{
			var processTask = Factory.New<WorkItemProcessTask>();
			var shelf = new EDIShelvesetInfo("Someone", "Something", "SCH", processTask);
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				shelf.Schedule(crikeyDataAccess);
				connection.ExecuteNonQuery("UPDATE UserTestHeader SET UH_Status = 'CIN', UH_ShelfName = '1234'");
				AssertEquals("1234", ShelvesetTools.GetScheduledShelvesByStatus("CIN", Factory).First().Name);
			}
		}

		public void TestGetScheduledShelvesByStatusWithAspectReviewData()
		{
			var processTask = Factory.New<WorkItemProcessTask>();
			var shelf = new EDIShelvesetInfo("Someone", "Something", "SCH", processTask);
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				shelf.Schedule(crikeyDataAccess);
				connection.ExecuteNonQuery(@"UPDATE UserTestHeader SET UH_Status = 'REJ'");

				var shelfFromDB = ShelvesetTools.GetScheduledShelvesByStatus("REJ", Factory).SingleOrDefault();
				AssertEquals("REJ", shelfFromDB.Status);
				AssertEquals(false, shelfFromDB.AspectReviews.Any());

				UpdateShelfResults(shelfFromDB.UserHeaderPK, "REJ", "/$Branch", "Pending aspect data found\r\netc");
				CreateAspectWithReviewAndData("PEN", ShelvesetTools.LoadShelfByProcessTask(processTask).UserHeaderPK);

				shelfFromDB = ShelvesetTools.GetScheduledShelvesByStatus("REJ", Factory).SingleOrDefault();
				AssertEquals("REJ", shelfFromDB.Status);
				AssertEquals(1, shelfFromDB.AspectReviews.Count);
			}
		}

		public void TestGetScheduledShelvesByStatusWithAspectReviewDataOnlyReturnPendingData()
		{
			var processTaskAcc = Factory.New<WorkItemProcessTask>();
			var processTaskPen = Factory.New<WorkItemProcessTask>();
			var processTaskRej = Factory.New<WorkItemProcessTask>();

			var shelfAcc = new EDIShelvesetInfo("Someone", "Something", "SCH", processTaskAcc);
			var shelfPen = new EDIShelvesetInfo("Sometwo", "Something too", "SCH", processTaskPen);
			var shelfRej = new EDIShelvesetInfo("Somethree", "Something else", "SCH", processTaskRej);
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				shelfAcc.Schedule(crikeyDataAccess);
				shelfPen.Schedule(crikeyDataAccess);
				shelfRej.Schedule(crikeyDataAccess);

				var userTestHeaderPKAcc = ShelvesetTools.LoadShelfByProcessTask(processTaskAcc).UserHeaderPK;
				var userTestHeaderPKPen = ShelvesetTools.LoadShelfByProcessTask(processTaskPen).UserHeaderPK;
				var userTestHeaderPKRej = ShelvesetTools.LoadShelfByProcessTask(processTaskRej).UserHeaderPK;

				UpdateShelfResults(userTestHeaderPKAcc, "REJ", "/$Branch", "Rejected for another reason");
				UpdateShelfResults(userTestHeaderPKPen, "REJ", "/$Branch", "Pending aspect data found\r\netc");
				UpdateShelfResults(userTestHeaderPKRej, "REJ", "/$Branch", "Rejected aspect data found\r\netc");

				CreateAspectWithReviewAndData("ACC", userTestHeaderPKAcc);
				CreateAspectWithReviewAndData("PEN", userTestHeaderPKPen);
				CreateAspectWithReviewAndData("REJ", userTestHeaderPKRej);

				var shelvesFromDB = ShelvesetTools.GetScheduledShelvesByStatus("REJ", Factory);
				AssertEquals("We shousle have 3 shelves", 3, shelvesFromDB.Count);

				var shelfFromDbAcc = shelvesFromDB.SingleOrDefault(s => s.RelatedProcessTask.PK == processTaskAcc.PK);
				AssertNotNull("Looking for the shelf with accepted aspect data", shelfFromDbAcc);
				AssertEquals("REJ", shelfFromDbAcc.Status);
				AssertEquals("Should have No review details", 0, shelfFromDbAcc.AspectReviews.Count);

				var shelfFromDbPen = shelvesFromDB.SingleOrDefault(s => s.RelatedProcessTask.PK == processTaskPen.PK);
				AssertNotNull("Looking for the shelf with pending aspect data", shelfFromDbPen);
				AssertEquals("REJ", shelfFromDbPen.Status);
				AssertEquals("Should have a Single review details", 1, shelfFromDbPen.AspectReviews.Count);

				var shelfFromDbRej = shelvesFromDB.SingleOrDefault(s => s.RelatedProcessTask.PK == processTaskRej.PK);
				AssertNotNull("Looking for the shelf with rejected aspect data", shelfFromDbRej);
				AssertEquals("REJ", shelfFromDbRej.Status);
				AssertEquals("Should have No review details", 0, shelfFromDbRej.AspectReviews.Count);
			}
		}

		public void TestGetScheduledShelvesByStatusWithMultipleAspectsOfDifferentStatus()
		{
			var processTask = Factory.New<WorkItemProcessTask>();
			var shelf = new EDIShelvesetInfo("Someone", "Something", "SCH", processTask);
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				shelf.Schedule(crikeyDataAccess);
				var userTestHeaderPK = ShelvesetTools.LoadShelfByProcessTask(processTask).UserHeaderPK;

				UpdateShelfResults(userTestHeaderPK, "REJ", "/$Branch", "Rejected for another reason");

				var shelfinfo = ShelvesetTools.LoadShelfByProcessTask(processTask);

				UpdateShelfResults(userTestHeaderPK, "REJ", "/$Branch1", "Rejected for another reason");
				UpdateShelfResults(userTestHeaderPK, "REJ", "/$Branch2", "Pending aspect data found\r\netc");
				UpdateShelfResults(userTestHeaderPK, "REJ", "/$Branch3", "Rejected aspect data found\r\netc");

				var reviewWithAccPK = CreateAspectWithReviewAndData("ACC", shelfinfo.UserHeaderPK);
				var reviewWithRejPK = CreateAspectWithReviewAndData("REJ", shelfinfo.UserHeaderPK);
				var reviewWithPenPK = CreateAspectWithReviewAndData("PEN", shelfinfo.UserHeaderPK);

				var shelfFromDB = ShelvesetTools.GetScheduledShelvesByStatus("REJ", Factory).SingleOrDefault();
				AssertNotNull(shelfFromDB);
				AssertEquals("REJ", shelfFromDB.Status);
				AssertEquals(0, shelfFromDB.AspectReviews.Count(ar => ar.PK == reviewWithRejPK));
				AssertEquals(1, shelfFromDB.AspectReviews.Count(ar => ar.PK == reviewWithPenPK));
				AssertEquals(0, shelfFromDB.AspectReviews.Count(ar => ar.PK == reviewWithAccPK));
				AssertEquals(1, shelfFromDB.AspectReviews.Count);
			}
		}

		void UpdateShelfResults(Guid userTestHeaderPK, string testStatus, string branch, string branchProcessingError)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"
					UPDATE UserTestHeader SET UH_Status = 'REJ'
					INSERT INTO UserTest (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) 
						VALUES (NEWID(), @userTestHeaderPK, 'whatever', @branch, @testStatus, @branchProcessingError)
					"))
			{
				cmd.AddParameter("@branchProcessingError", SqlDbType.VarChar, branchProcessingError);
				cmd.AddParameter("@userTestHeaderPK", SqlDbType.UniqueIdentifier, userTestHeaderPK);
				cmd.AddParameter("@testStatus", SqlDbType.VarChar, 3, testStatus);
				cmd.AddParameter("@branch", SqlDbType.VarChar, branch);
				cmd.ExecuteNonQuery();
			}
		}

		Guid CreateAspectWithReviewAndData(string aspectDataStatus, Guid userTestHeaderPK)
		{
			var reviewPK = Guid.NewGuid();
			var aspectCapability = aspectDataStatus;
			var aspectName = $"Aspect With capability: {aspectCapability}";
			var aspectKey = $"Aspect Key {aspectCapability}";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"
					DECLARE @AspectPK AS UNIQUEIDENTIFIER = NEWID()
					DECLARE @AspectKeyPK AS UNIQUEIDENTIFIER = NEWID()
					DECLARE @AspectDataPK AS UNIQUEIDENTIFIER = NEWID()
					INSERT INTO Aspect (AS_PK, AS_Name, AS_Capability, AS_DataExtratorType, AS_IsActive, AS_IsFromXML, AS_HasBaseline)
								VALUES (@AspectPK, @AspectName, @AspectCapability, '', 1, 0, 1)
					INSERT INTO AspectKey (AK_PK, AK_AS, AK_Key) 
								VALUES (@AspectKeyPK, @AspectPK, @AspectKey)
					INSERT INTO AspectData (AD_PK, AD_AK, AD_Value, AD_Status)
								VALUES (@AspectDataPK, @AspectKeyPK, 'Value', @AspectDataStatus)
					INSERT INTO AspectReview (AR_PK, AR_AS, AR_UH, AR_Capability) 
								VALUES (@ReviewPK, @AspectPK, @UserTestHeaderPK, @AspectCapability)
					INSERT INTO AspectReviewData (ARD_AR, ARD_AD) 
								VALUES (@ReviewPK, @AspectDataPK)
					"))
			{
				cmd.AddParameter("@ReviewPK", SqlDbType.UniqueIdentifier, reviewPK);
				cmd.AddParameter("@AspectName", SqlDbType.VarChar, aspectName);
				cmd.AddParameter("@AspectKey", SqlDbType.VarChar, aspectKey);
				cmd.AddParameter("@AspectCapability", SqlDbType.VarChar, 3, aspectCapability);
				cmd.AddParameter("@AspectDataStatus", SqlDbType.VarChar, 3, aspectDataStatus);
				cmd.AddParameter("@UserTestHeaderPK", SqlDbType.UniqueIdentifier, userTestHeaderPK);
				cmd.ExecuteNonQuery();
				return reviewPK;
			}
		}

		public static void AddCheckInTaskTypes()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes parent = collection.AddNew();
			parent.Code = JobInvoicingConsumerTypes.WorkItem.Code;
			foreach (string taskType in ReleaseRingsLookup.CheckInTaskTypes)
			{
				parent.TaskTypes.AddNew().Code = taskType;
			}
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AddCheckInTaskTypes();
		}

		//TODO uncomment when Form moved to gui
		//[GuiTest]
		//public void TestGetScheduledShelvesByStatus()
		//{
		//	var tfsContextForTest = new TfsRestContextForTest();
		//	using (ObjectFactory.Substitute<ITfsRestContext>(tfsContextForTest))
		//	{
		//		var staff = Factory.New<GlbStaff>();
		//		staff.GS_Code = "TST";
		//		staff.GS_LoginName = "test";
		//		staff.GS_FullName = "Test Test";

		//		tfsContextForTest.AddShelves(TfsRestContext.DomainNames[0] + "\\" + staff.GS_LoginName, "shelf for test");

		//		NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
		//		WorkItemProcessTask task = (WorkItemProcessTask)workItem.WorkflowItems.AddNew();

		//		task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
		//		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		//		task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
		//		task.P9_Description = "shelf for test";

		//		EDIShelvesetInfo shelf = ShelvesetTools.LoadShelfByProcessTask(task);
		//		Assert("Precondition: shelf should NOT exist in DB yet.", shelf == null);

		//		using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
		//		using (NewWorkItemForm form = new NewWorkItemForm(workItem))
		//		{
		//			var crkeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

		//			form.FireSaveButton();
		//			AssertNoErrors(workItem);

		//			shelf = ShelvesetTools.LoadShelfByProcessTask(task);
		//			Assert("Precondition: shelf should exist in DB.", shelf != null);

		//			crkeyDataAccess.UpdateStatus(shelf, "YYY");
		//			AssertEquals("Shelf should NOT be found.", 0, ShelvesetTools.GetScheduledShelvesByStatus("XXX", Factory).Count);
		//			crkeyDataAccess.UpdateStatus(shelf, "XXX");
		//			AssertEquals("Shelf should be found.", 1, ShelvesetTools.GetScheduledShelvesByStatus("XXX", Factory).Count);
		//			AssertEquals("Shelf name", task.P9_Description, shelf.Name);

		//			crkeyDataAccess.UpdateStatus(shelf, "YYY");
		//			AssertEquals("Shelf should NOT be found.", 0, ShelvesetTools.GetScheduledShelvesByStatus("XXX", Factory).Count);
		//		}
		//	}
		//}

		//[GuiTest]
		//public void TestLoadShelfByProcessTask()
		//{
		//	var tfsContextForTest = new TfsRestContextForTest();
		//	using (ObjectFactory.Substitute<ITfsRestContext>(tfsContextForTest))
		//	{
		//		var staff = Factory.New<GlbStaff>();
		//		staff.GS_Code = "TST";
		//		staff.GS_LoginName = "test";
		//		staff.GS_FullName = "Test Test";

		//		tfsContextForTest.AddShelves(TfsRestContext.DomainNames[0] + "\\" + staff.GS_LoginName, "shelf for test");

		//		NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
		//		WorkItemProcessTask task = (WorkItemProcessTask)workItem.WorkflowItems.AddNew();

		//		task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
		//		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		//		task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
		//		task.P9_Description = "shelf for test";

		//		using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
		//		{
		//			var crkeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

		//			EDIShelvesetInfo shelfInfo = new EDIShelvesetInfo("", "", "", task);
		//			AssertEquals("Shelf for this ProcessTask should NOT be scheduled yet", false, crkeyDataAccess.IsShelfScheduled(shelfInfo));

		//			using (NewWorkItemForm form = new NewWorkItemForm(workItem))
		//			{
		//				form.FireSaveButton();
		//				AssertNoErrors(workItem);

		//				AssertEquals("Shelf for this ProcessTask should be scheduled already", true, crkeyDataAccess.IsShelfScheduled(shelfInfo));

		//				EDIShelvesetInfo shelf = ShelvesetTools.LoadShelfByProcessTask(task);

		//				Assert("Shelf has been loaded", shelf != null);
		//				AssertEquals("Shelf Name", task.P9_Description, shelf.Name);
		//				AssertEquals("Shelf Type", "SCH", shelf.ActionType);
		//				AssertEquals("Related ProcessTask PK", task.PK, shelf.RelatedProcessTask.PK);
		//			}
		//		}
		//	}
		//}

		public void TestGetScheduledShelvesByStatusPopulatesNotificationEmail()
		{
			var processTask = Factory.New<WorkItemProcessTask>();
			var shelf = new EDIShelvesetInfo("Someone", "Something", "SCH", processTask);
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				shelf.Schedule(crikeyDataAccess);
				connection.ExecuteNonQuery("UPDATE UserTestHeader SET UH_NotificationEmail = 'someone@wisetechglobal.com', UH_Status = 'CIN', UH_ChangesetId = 1234");
				AssertEquals("someone@wisetechglobal.com", ShelvesetTools.GetScheduledShelvesByStatus("CIN", Factory).First().NotificationEmail);
			}
		}
	}
}
