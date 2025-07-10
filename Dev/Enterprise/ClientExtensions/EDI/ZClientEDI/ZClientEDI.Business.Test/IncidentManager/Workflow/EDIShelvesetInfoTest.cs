using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.Registry.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EDIShelvesetInfoTest : TransactionedTestCase
	{
		public void TestNextCodeReviewTask()
		{
			var processTask = Factory.New<WorkItemProcessTask>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();

			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Silly Hats Only");

			processTask.P9_ParentID = workItem.PK;
			processTask.P9_Sequence = 1;
			processTask.P9_FH_ProcessHeader = workflow.PK;

			var closedReview = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			closedReview.P9_Sequence = 2;
			closedReview.P9_Type = "CBC";
			closedReview.P9_Status = "CLS";
			closedReview.P9_FH_ProcessHeader = workflow.PK;

			var uatReview = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			uatReview.P9_Sequence = 3;
			uatReview.P9_Type = "CBF";
			uatReview.P9_FH_ProcessHeader = workflow.PK;

			var codeReview = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			codeReview.P9_Sequence = 4;
			codeReview.P9_Type = "CBC";
			codeReview.P9_FH_ProcessHeader = workflow.PK;

			EDIShelvesetInfo shelf = new EDIShelvesetInfo("Alex", "Never mind", "SCH", processTask);

			AssertEquals(codeReview, shelf.NextCodeReviewTask);
		}

		public void TestSchedule()
		{
			WorkItemProcessTask processTask = Factory.New<WorkItemProcessTask>();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			processTask.P9_ParentID = workItem.PK;

			EDIShelvesetInfo shelf = new EDIShelvesetInfo("Alex", "Never mind", "SCH", processTask);
			AssertNull("Precondition: NO shelf in DB for processTask yet.", ShelvesetTools.LoadShelfByProcessTask(processTask));

			using (DbConnection connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				connection.BeginTransaction();
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				try
				{
					shelf.Schedule(crikeyDataAccess);
					var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					Assert("Shelf is in DB now", scheduledShelf != null);
					AssertEquals("Shelf name", "Never mind", scheduledShelf.Name);
					AssertEquals("Shelf owner", "Alex", scheduledShelf.Owner);

					EDIShelvesetInfo shelf2 = new EDIShelvesetInfo("The man who sold the world", "Nothing else matters", "SCH", processTask);

					shelf2.Schedule(crikeyDataAccess);
					var updatedShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					Assert("Shelf is still in DB", updatedShelf != null);
					AssertEquals("Shelf name", "Nothing else matters", updatedShelf.Name);
					AssertEquals("Shelf owner", "The man who sold the world", updatedShelf.Owner);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestPropertiesCopiedByWorkItem()
		{
			var newWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var processTask1 = Factory.New<WorkItemProcessTask>();
			processTask1.P9_ParentID = newWorkItem.PK;
			var processTask2 = Factory.New<WorkItemProcessTask>();
			processTask2.P9_ParentID = newWorkItem.PK;

			using (DbConnection connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

				var shelf = new EDIShelvesetInfo("Someone", "Whatever", "SCH", processTask1);
				shelf.Schedule(crikeyDataAccess);
				var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask1);
				AssertNotNull("Shelf is in DB now", scheduledShelf);
				var submitted = GetFirstSumittedDate(connection, scheduledShelf.UserHeaderPK);
				AssertEquals(string.Empty, GetPriority(connection, scheduledShelf.UserHeaderPK));
				SetPriority(connection, scheduledShelf.UserHeaderPK, ":-)");
				crikeyDataAccess.UpdateStatus(scheduledShelf, "REJ");

				shelf = new EDIShelvesetInfo("Anywone", "Something Else", "SCH", processTask2);
				shelf.Schedule(crikeyDataAccess);
				scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask2);
				AssertNotNull("Shelf is in DB now", scheduledShelf);
				AssertEquals(submitted, GetFirstSumittedDate(connection, scheduledShelf.UserHeaderPK));
				AssertEquals(":-)", GetPriority(connection, scheduledShelf.UserHeaderPK));
			}
		}

		public void TestRemoveFromSchedule()
		{
			WorkItemProcessTask processTask = Factory.New<WorkItemProcessTask>();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			processTask.P9_ParentID = workItem.PK;

			EDIShelvesetInfo shelf = new EDIShelvesetInfo("Alex", "Never mind", "SCH", processTask);
			AssertNull("Precondition: NO shelf in DB for processTask yet.", ShelvesetTools.LoadShelfByProcessTask(processTask));

			using (DbConnection connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				connection.BeginTransaction();
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				try
				{
					shelf.Schedule(crikeyDataAccess);
					var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					Assert("Shelf is in DB now", scheduledShelf != null);

					crikeyDataAccess.RemoveFromSchedule(shelf);
					shelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					Assert(shelf == null || shelf.Status == "CAN");
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestHasBeenBuilt()
		{
			WorkItemProcessTask processTask = Factory.New<WorkItemProcessTask>();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			processTask.P9_ParentID = workItem.PK;

			EDIShelvesetInfo shelf = new EDIShelvesetInfo("Alex", "Never mind", "SCH", processTask);
			AssertNull("Precondition: NO shelf in DB for processTask yet.", ShelvesetTools.LoadShelfByProcessTask(processTask));

			using (DbConnection connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				connection.BeginTransaction();
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				try
				{
					shelf.Schedule(crikeyDataAccess);

					var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					Assert("Precondition: Shelf is in DB now", scheduledShelf != null);

					AssertEquals("Shelf has NOT been built", false, scheduledShelf.HasBeenBuilt);
					crikeyDataAccess.UpdateStatus(scheduledShelf, "RNF");
					AssertEquals("Shelf has been built", true, scheduledShelf.HasBeenBuilt);
					AssertEquals("Shelf has been built", true, crikeyDataAccess.LoadShelfByProcessTask(processTask).HasBeenBuilt);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestUpdateStatus()
		{
			WorkItemProcessTask processTask = Factory.New<WorkItemProcessTask>();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			processTask.P9_ParentID = workItem.PK;

			using (DbConnection connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				EDIShelvesetInfo shelf = new EDIShelvesetInfo("Alex", "Never mind", "SCH", processTask);
				AssertNull("Precondition: NO shelf in DB for processTask yet.", crikeyDataAccess.LoadShelfByProcessTask(processTask));

				connection.BeginTransaction();
				try
				{
					shelf.Schedule(crikeyDataAccess);

					var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					AssertEquals("Shelf Status", ShelfStatuses.QueuedForBranchDetection, scheduledShelf.Status);

					crikeyDataAccess.UpdateStatus(scheduledShelf, "XXX");

					var loadedShelf = crikeyDataAccess.LoadShelfByProcessTask(processTask);
					AssertEquals("Shelf Status", "XXX", loadedShelf.Status);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestTaskCommentsContainTestRigOrigin()
		{
			var factory = new BusinessObjectFactory();

			var workItemNumber = "WI00761176";
			var workItem = factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_WorkItemNumber = workItemNumber;
			var processTask = workItem.WorkflowItems.AddNew();

			var expectedComments = string.Format(CultureInfo.InvariantCulture, @"TestRigOrigin: {0}
TestRigRestoreFromBackup: SuperCoolBackup.bak

https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/247986",
			workItemNumber);

			processTask.P9_NotesAsString = @"TestRigRestoreFromBackup: SuperCoolBackup.bak

https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/247986";

			var shelf = new EDIShelvesetInfo("CORP\\Mr.Rose", null, EDIShelvesetInfo.ActionTypes.UATBuild, processTask);

			var actualComments = shelf.Comments;

			AssertEquals(expectedComments, actualComments);
		}

		public DateTime GetFirstSumittedDate(DbConnection connection, Guid userTestPK)
		{
			using (var command = connection.Command("select UH_Submitted from UserTestHeader where UH_PK = @pk"))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, userTestPK);
				return (DateTime)command.ExecuteScalar();
			}
		}

		public string GetPriority(DbConnection connection, Guid userTestPK)
		{
			using (var command = connection.Command("select coalesce(UH_Criticality, '') from UserTestHeader where UH_PK = @pk"))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, userTestPK);
				return (string)command.ExecuteScalar();
			}
		}

		public void SetPriority(DbConnection connection, Guid userTestPK, string priority)
		{
			using (var command = connection.Command("update UserTestHeader set UH_Criticality = @priority where UH_PK = @pk"))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, userTestPK);
				command.AddParameter("priority", SqlDbType.VarChar, priority);
				command.ExecuteNonQuery();
			}
		}

		#region Test Rig Options

		EDIShelvesetInfo CreateInfoFromWorkItemAndTaskDetails(string product, string productArea, string module, string changeType, string taskNotes)
		{
			var workItem = Factory.New<NewWorkItem>();
			workItem.WKI_WorkItemType = product;
			workItem.WKI_WorkItemArea = productArea;
			workItem.WKI_ActivityType = module;
			workItem.WKI_ActivitySubtype = changeType;
			var task = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();

			if (taskNotes != null)
			{
				task.P9_Notes = ORtfTextUtil.TextToRtfBytes(taskNotes);
			}

			return new EDIShelvesetInfo("adam.durkee", "there's no way of knowing", "shalala", task);
		}

		public void TestRegistryConfigurationMatching()
		{
			TestRigRegistryTestHelper.AddWorkItemTypes();
			TestRigRegistryTestHelper.AddTestRigOptions("1AA", "2AA", null, null, @"\\server\1.bak");
			TestRigRegistryTestHelper.AddTestRigOptions("1AA", "2AA", "3AA", null, @"\\server\2.bak");
			TestRigRegistryTestHelper.AddTestRigOptions("1AA", "2AA", null, "4AA", @"\\server\3.bak");
			TestRigRegistryTestHelper.AddTestRigOptions("1AA", "2AA", "3AA", "4AA", @"\\server\4.bak");

			var info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", null, null, null);
			AssertShelfComments("The correct registry item should have been selected based on WI criteria, and yet...", @"
TestRigRestoreFromBackup: <\\server\1.bak>", info);

			info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", "3AA", null, null);
			AssertShelfComments("The correct registry item should have been selected based on WI criteria, and yet...", @"
TestRigRestoreFromBackup: <\\server\2.bak>", info);

			info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", null, "4AA", null);
			AssertShelfComments("The correct registry item should have been selected based on WI criteria, and yet...", @"
TestRigRestoreFromBackup: <\\server\3.bak>", info);

			info = CreateInfoFromWorkItemAndTaskDetails(null, null, null, "4AA", null);
			AssertShelfComments("There is no matching registry item so the notes should not be populated with anything, and yet...",
				string.Empty, info);

			info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", "3AA", "4AA", null);
			AssertShelfComments("The correct registry item should have been selected based on WI criteria, and yet...", @"
TestRigRestoreFromBackup: <\\server\4.bak>", info);
		}

		public void TestOptions_ShouldUseDetailsFromRegistryAsFallback()
		{
			TestRigRegistryTestHelper.AddWorkItemTypes();
			TestRigRegistryTestHelper.AddTestRigOptions("1AA", "2AA", null, null, @"\\server\1.bak", @"
TestRigSqlServer: fromRegistry.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Registry
TestRigSqlServerLogFilePath: G:\Logs\From\Registry
TestRigDatabaseName: RegistryDatabase
TestRigIconName: RegistryIcon
TestRigCreateIcon: true
TestRigIncludeSystemPackage: true
TestRigWebSites: OneWebsite,TheOtherWebsite
TestRigWebServer: RegistryServer
TestRigWebDomain: RegistryDomain
TestRigClientCode: RegistryClient
TestRigRegister: false
TestRigRegistrationEnterpriseCode: REG
TestRigRegistrationSqlServer: fromRegistryServer.com");

			// Details in notes AND a matching registry item
			var info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", "3AA", "4AA", @"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false
TestRigWebSites: This, That
TestRigWebServer: NotesServer
TestRigWebDomain: NotesDomain
TestRigClientCode: NotesClient
TestRigRegister: true
TestRigRegistrationEnterpriseCode: NOT
TestRigRegistrationSqlServer: fromNotesServer.com");

			AssertShelfComments("There were both notes and a matching registry item, so the notes should take precedence, and yet...",
				@"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false
TestRigWebSites: This, That
TestRigWebServer: NotesServer
TestRigWebDomain: NotesDomain
TestRigClientCode: NotesClient
TestRigRegister: true
TestRigRegistrationEnterpriseCode: NOT
TestRigRegistrationSqlServer: fromNotesServer.com", info);

			// Matching registry item but no notes
			info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", "3AA", "4AA", null);
			AssertShelfComments("There was a registry item but no notes, so the values from the registry should be used, and yet...",
				@"
TestRigRestoreFromBackup: <\\server\1.bak>
TestRigSqlServer: fromRegistry.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Registry
TestRigSqlServerLogFilePath: G:\Logs\From\Registry
TestRigDatabaseName: RegistryDatabase
TestRigIconName: RegistryIcon
TestRigCreateIcon: true
TestRigIncludeSystemPackage: true
TestRigWebSites: OneWebsite,TheOtherWebsite
TestRigWebServer: RegistryServer
TestRigWebDomain: RegistryDomain
TestRigClientCode: RegistryClient
TestRigRegister: false
TestRigRegistrationEnterpriseCode: REG
TestRigRegistrationSqlServer: fromRegistryServer.com", info);

			// Notes, but no matching registry item
			info = CreateInfoFromWorkItemAndTaskDetails("1BB", "2AA", "3AA", "4AA", @"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false
TestRigWebSites: This, That
TestRigWebServer: NotesServer
TestRigWebDomain: NotesDomain
TestRigClientCode: NotesClient
TestRigRegister: true
TestRigRegistrationEnterpriseCode: NOT
TestRigRegistrationSqlServer: fromNotesServer.com");

			AssertShelfComments("There were notes and nothing in the registry, so the notes should be used, and yet...",
				@"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false
TestRigWebSites: This, That
TestRigWebServer: NotesServer
TestRigWebDomain: NotesDomain
TestRigClientCode: NotesClient
TestRigRegister: true
TestRigRegistrationEnterpriseCode: NOT
TestRigRegistrationSqlServer: fromNotesServer.com", info);

			// No registry item and no notes
			info = CreateInfoFromWorkItemAndTaskDetails("1BB", "2AA", "3AA", "4AA", null);
			AssertShelfComments("There was neither notes nor registry item so the comments should be empty, and yet...", string.Empty, info);

			// Some notes and a full registry
			info = CreateInfoFromWorkItemAndTaskDetails("1AA", "2AA", "3AA", "4AA", @"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false");

			AssertShelfComments("There were notes for some of the settings, so those ones should come from the notes while the rest come from the registry item, and yet...",
				@"
TestRigRestoreFromBackup: <\\server\fromNotes.bak >
TestRigSqlServer: fromNotes.wtg.zone
TestRigSqlServerDataFilePath: G:\From\Notes
TestRigSqlServerLogFilePath: G:\Logs\From\Notes
TestRigDatabaseName: NotesDatabase
TestRigIconName: NotesIcon
TestRigCreateIcon: false
TestRigIncludeSystemPackage: false
TestRigWebSites: OneWebsite,TheOtherWebsite
TestRigWebServer: RegistryServer
TestRigWebDomain: RegistryDomain
TestRigClientCode: RegistryClient
TestRigRegister: false
TestRigRegistrationEnterpriseCode: REG
TestRigRegistrationSqlServer: fromRegistryServer.com", info);
		}

		#endregion

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;

		void AssertShelfComments(string message, string expectedComments, EDIShelvesetInfo shelf)
		{
			var comments = GetCommentsWithOrigin(expectedComments, shelf.RelatedProcessTask);

			AssertMultilineASCIIEquals(message, comments, shelf.Comments);
		}

		string GetCommentsWithOrigin(string comments, WorkItemProcessTask task)
		{
			if (task.Parent == null)
			{
				return comments;
			}

			return @$"TestRigOrigin: {task.Parent.WKI_WorkItemNumber}
{comments}";
		}
	}
}
