using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.DevTools.ServiceClient.Assess;
using ZClientEDI.Business.Test;
using static Enterprise.Client.EDI.Test.ProcessedShelfsServiceTaskTest;

namespace ZClientEDI.Test.ServiceTasks
{
	[TestedType(typeof(ProcessedShelfsServiceTask))]
	class WiseTechAcademyAspectTests : ServiceTaskTestCase<ProcessedShelfsServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new ProcessedShelfsServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
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

		public void TestNonCompletedSkillsInEmail()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,LCD,ASN,3");

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			Factory.Save();

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", "WI00011111", "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,LCD,ASN,3");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", "WI00011111", "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid(),
			};

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(staff, aspectPK1, false)
				.SetupHasCompletedLearningUnit(staff, aspectPK2, false);

			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "AspectName");
			var aspectReview2 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK2, "AspectName");

			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";

			testHelper.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertWorkItemTasks(3);
			var tasksBefore = testHelperWithAspectDat.WorkItemTasks;

			tasksBefore[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			tasksBefore[1].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			tasksBefore[2].P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && WorkItemProcessTask.IsReviewTypeTask(t.P9_Type));
			reviewTask.P9_Description = "12345";

			Factory.Save();

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var item = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Failure: WI00011111 has been rejected", item.Subject);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{shelvesetWithAspectReview.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelvesetWithAspectReview.UserHeaderPK}</a> for details", item.Body);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + shelvesetWithAspectReview.RelatedProcessTask.Parent.PK, item.Body);
			AssertStringInRtf("We have detected the following areas of knowledge relevant to your submission. We suggest you complete these courses by following the links below:", item.Body);
			AssertStringInRtf(FormattableString.Invariant($@"<a href=""https://nothing/assess/5004"">Advanced Data Analysis and Methods of Psychological Inquiry</a><br />"), item.Body);
			AssertStringInRtf(FormattableString.Invariant($@"<a href=""https://nothing/assess/5005"">Behavioural Neuroscience</a><br />"), item.Body);
		}

		public void TestWTASubjectAspect_ShelfPass()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(1);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN", topWorkflow.PK);

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, LearningTaskType, "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestWTASubjectAspect_LearningTaskNotCreatedForSkilledCoder()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,1", "CBC,JMK,ASN,2");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");

			AssertEquals(1, reviewTask.SkillsPivots.Count);
		}

		public void TestWTASubjectAspect_LearningTaskCreatedForPartiallySkilledCoder()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,1", "CBC,JMK,ASN,2");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1, aspectPK2 });

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK2, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(2, LearningTaskType, "ASN");

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();
			AssertEquals("(Optional) " + LearningUnitName2, learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(2, reviewTask.SkillsPivots.Count);
		}

		public void TestWTASubjectAspect_BothAspectAndSkill()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,10", "CBC,JMK,CLS,3", "CDF,A.G,CLS,1");

			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};

			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "AspectAssess");
			var aspectReview2 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectCapability");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			Factory.Save();

			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(10, "CH0", "ASN");

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(9, WorkItemProcessTask.AspectReviewTaskType, "ASN");
			testHelperWithAspectDat.AssertTaskExists(9, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(10, "CH0", "SUS");

			AssertEquals("Review is closed so no skills added", 0, reviewTask.SkillsPivots.Count);
		}

		public void TestWTASubjectAspect_ShelfRejected()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(1);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN", topWorkflow.PK);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, LearningTaskType, "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "SUS", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestWTASubjectAspect_ShelfPassWithReviewAndCheckin()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,REV,ASN,3", "CHK,A.G,ASN,4");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(4, "CHK", "ASN");

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(3, LearningTaskType, "ASN");
			testHelperWithAspectDat.AssertTaskExists(4, "CHK", "ASN");

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(reviewTask.P9_Sequence, learningTasks[0].P9_Sequence);
		}

		public void TestWTASubjectAspect_NoLearningTaskCreatedForSuccessfulCheckin()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,4", "CDF,A.G,CLS,1", "SH0,A.G,CLS,2", "CBC,REV,CLS,3");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.CheckedIn, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CH0", "ASN");

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CH0", "CLS");
		}

		public void TestWTASubjectAspect_CheckinRejectedNoReviewStillCreatesLearningTask()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,5", "CDU,A.G,CLS,1", "CDF,A.G,CLS,2", "SH0,A.G,CLS,3", "CBC,REV,CLS,4");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDU", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(5, "CH0", "ASN");
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(7);
			testHelperWithAspectDat.AssertTaskExists(1, "CDU", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, LearningTaskType, "ASN");
			testHelperWithAspectDat.AssertTaskExists(4, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(5, "CH0", "SUS");

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			AssertEquals(1, learningTasks.Length);
			AssertEquals("no skills should be added to the previously closed review task", 0, reviewTask.SkillsPivots.Count);

			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			const string learningTaskNotes = @"DAT has detected an ASSESS course relevant to your submission. Please consider taking this course using the link below to help improve your knowledge in this area. It is optional, and you may cancel this task if you do not wish to complete the course now.
DAT requires that code reviewers complete all ASSESS courses relevant to the submission, so taking the course will allow you to review code like this in the future.

Advanced Data Analysis and Methods of Psychological Inquiry
https://nothing/assess/5004";

			AssertEquals(learningTaskNotes, learningTasks[0].P9_NotesAsString);
		}

		public void TestWTASubjectAspect_CreateLearningTask_LongDescription()
		{
			var longestSubjectDescription = new string('x', ProcessTasksSchema.P9_Description.MaxLength);
			assessServiceClientMock.SetupGetLearningUnitName(aspectPK2, longestSubjectDescription);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,A.G,ASN,3");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,A.G,ASN,3");
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1, aspectPK2 });

			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(2, "SH0", "ASN");
			testHelper.AssertTaskExists(3, "CBC", "ASN");
			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN");

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);
			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK2, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(2, "SH0", "SUS");
			testHelper.AssertTaskExists(3, "CBC", "ASN");
			var reviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == WorkItemProcessTask.CodeReviewTaskType);
			AssertEquals(0, reviewTask.SkillsPivots.Count);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "SUS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN");
			var reviewTaskAssess = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == WorkItemProcessTask.CodeReviewTaskType);
			AssertEquals(2, reviewTaskAssess.SkillsPivots.Count);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();
			AssertEquals(2, learningTasks.Length);
			AssertEquals(1, learningTasks[0].P9_Sequence);
			AssertEquals(1, learningTasks[1].P9_Sequence);

			var longTaskDescription = $"(Optional) {longestSubjectDescription}";
			longTaskDescription = longTaskDescription.Substring(0, ProcessTasksSchema.P9_Description.MaxLength);
			AssertContainsExactElementsInAnyOrder(new[] { "(Optional) Advanced Data Analysis and Methods of P", longTaskDescription }, learningTasks.Select(t => t.P9_Description));
		}

		public void TestWTASubjectAspect_PassedAspectOnlyBuild()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1 });

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(5, "AS0", "ASN");
			testHelper.AssertTaskExists(20, "CHK", "ASN");

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(5, "AS0", "CLS");
			testHelper.AssertTaskExists(5, LearningTaskType, "ASN");
			testHelper.AssertTaskExists(20, "CHK", "ASN");
		}

		public void TestWTASubjectAspect_FailedAspectOnlyBuildWithoutQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(5, "AS0", "ASN");
			testHelper.AssertTaskExists(20, "CHK", "ASN");

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(4, LearningTaskType, "ASN");
			testHelper.AssertTaskExists(5, "AS0", "SUS");
			testHelper.AssertTaskExists(20, "CHK", "ASN");
		}

		public void TestWTASubjectAspect_FailedAspectOnlyBuildWithQualityIteration()
		{
			//A rejected aspect only build probably means a build failure (but what if release build fails and debug build records aspect data?
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(3, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(5, "AS0", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(20, "CHK", "ASN", topWorkflow.PK);

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(7);
			var newCoding = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;

			testHelper.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(3, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(5, "AS0", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(5, LearningTaskType, "ASN", iterationHeader);
			testHelper.AssertTaskExists(6, "COD", "ASN", iterationHeader);
			testHelper.AssertTaskExists(7, "ASP", "ASN", iterationHeader);
			testHelper.AssertTaskExists(22, "CHK", "ASN", topWorkflow.PK);
		}

		public void TestWTASubjectAspect_CheckinRejectedWithReviewInIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "CH0,A.G,ASN,50", "CDU,A.G,CLS,10", "CDF,A.G,CLS,20", "SH0,A.G,CLS,30", "CBC,REV,CLS,40");
			testHelper.ShelfTask.P9_Description = "Shelf Checkin Task";
			var isQCBTask = testHelper.ShelfTask.IsQualityContainmentBarrierTask();
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.RejectedForPendingAspectData, new[] { aspectPK1 });

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(10, "CDU", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(20, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(30, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(40, "CBC", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(50, "CH0", "ASN", topWorkflow.PK);
			var oldReviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(7);

			testHelper.AssertTaskExists(10, "CDU", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(20, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(30, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(40, "CBC", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(48, LearningTaskType, "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(49, "CBC", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(50, "CH0", "SUS", topWorkflow.PK);

			var learningTasks = testHelper.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();
			AssertEquals(1, learningTasks.Length);
			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);

			AssertEquals("no skills should be added to the previously closed review task", 0, oldReviewTask.SkillsPivots.Count);
			var newReviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType) && t.P9_Status == "ASN");
			AssertEquals(1, newReviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, newReviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestWTASubjectAspect_ShelfRejectedWithReviewAndCheckin_Iteration()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var workflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,REV,ASN,3");
			testHelperWithAspectDat.ShelfTask.P9_Description = "This Shelf Will Be REJ";

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN", workflow.PK);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			reviewTask.P9_Description = "Code Review Of Functionality";

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;
			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, LearningTaskType, "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(4, "SHV", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", workflow.PK);

			AssertNotNull(reviewTask);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
		}

		public void TestWTASubjectAspect_CheckinRejectedWithNewCodeReviewInIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			var workflow = testHelper.AddWorkflow("YES", null, "CH0,A.G,ASN,3", "CDF,A.G,CLS,1", "CBC,REV,CLS,2");
			testHelper.ShelfTask.P9_Description = "This checkin will be rejected";

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, EDIShelvesetInfo.ActionTypes.ShelfCheckin, new[] { aspectPK1 });

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists(1, "CDF", "CLS", workflow.PK);
			testHelper.AssertTaskExists(2, "CBC", "CLS", workflow.PK);
			testHelper.AssertTaskExists(3, "CH0", "ASN", workflow.PK);

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(6);
			var newCodingTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "CDF" && t.IsOpen);
			var iterationHeader = newCodingTask.P9_FH_ProcessHeader;

			testHelper.AssertTaskExists(1, "CDF", "CLS", workflow.PK);
			testHelper.AssertTaskExists(2, "CBC", "CLS", workflow.PK);
			testHelper.AssertTaskExists(3, "CH0", "CAN", workflow.PK);
			testHelper.AssertTaskExists(4, "CDF", "ASN", iterationHeader);
			testHelper.AssertTaskExists(5, "CBC", "ASN", iterationHeader);
			testHelper.AssertTaskExists(6, "CHK", "ASN", iterationHeader);

			var codeReviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType) && t.IsOpen);

			AssertNotNull(codeReviewTask);
			AssertEquals(1, codeReviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, codeReviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestWTASubjectAspect_ShelfPassedInsideOfIteration()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,JMK,ASN,3");
			Factory.Save();

			testHelperWithAspectDat.CreateQualityIteration();

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV");
			var iterationWorkflow = newShelf.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(4, "SHV", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", topWorkflow.PK);

			newShelf.P9_Type = "SH0";
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(newShelf, ShelfStatuses.Passed, new[] { aspectPK1 });
			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(4, "SH0", "CLS", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(5, LearningTaskType, "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();
			AssertEquals(1, learningTasks.Length);
			AssertNotNull(reviewTask);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(true, learningTasks[0].IsCurrent);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);
			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
		}

		public void TestWTASubjectAspect_ShelfRejectedInsideOfIteration()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,REV,ASN,3");
			Factory.Save();

			testHelperWithAspectDat.CreateQualityIteration();

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;
			newCoding.P9_Status = "CLS";
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV");
			testHelperWithAspectDat.ShelfTask = newShelf;
			newShelf.P9_Type = "SH0";
			newShelf.P9_Description = "This Shelf Will Be REJ";

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(newShelf, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "CLS", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(4, "SH0", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", topWorkflow.PK);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(8);

			var codingInSecondIteration = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var secondIterationHeader = codingInSecondIteration.P9_FH_ProcessHeader;

			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "CLS", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(4, "SH0", "CAN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(4, LearningTaskType, "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "COD", "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(6, "SHV", "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(7, "CBC", "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();

			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);
			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
		}

		public void TestWTASubjectAspect_LearningTaskShouldNotBeCopiedInIteration()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,3", "CDF,A.G,CLS,1", LearningTaskType + ",A.G,CLS,2", "CBC,REV,ASN,4");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, LearningTaskType, "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(4, "COD", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "SHV", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(6, "CBC", "ASN", topWorkflow.PK);
		}

		public void TestWTASubjectAspect_LearningTaskShouldNotBeCopiedInIteration2()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "SH0,A.G,ASN,3", "CDF,A.G,CLS,1", LearningTaskType + ",A.G,CLS,2", "CBC,REV,ASN,4");
			testHelper.ShelfTask.P9_Description = "This Shelf Will Be REJ";
			Factory.Save();
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			var newCoding = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;

			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(2, LearningTaskType, "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(3, "SH0", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(4, "COD", "ASN", firstIterationHeader);
			testHelper.AssertTaskExists(5, "SHV", "ASN", firstIterationHeader);
			testHelper.AssertTaskExists(6, "CBC", "ASN", topWorkflow.PK);
		}

		public void TestWTASubjectAspect_LearningTaskShouldNotBeCopiedInIterationForFailedCheckin()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,3", "CDF,A.G,CLS,1", LearningTaskType + ",A.G,CLS,2");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "CDF" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, LearningTaskType, "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "CH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(4, "CDF", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "CHK", "ASN", firstIterationHeader);
		}

		public void TestWTASubjectAspect_CancelledReviewAndFailedCheckin()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelper.AddWorkflow(
				"YES",
				null,
				shelfTask: "CH1,A.G,ASN,500",
				"CDF,A.G,CLS,220",
				"SHV,A.G,CAN,230",
				"PRV,A.G,CAN,300",
				"CBC,A.G,CAN,310",
				"CBF,A.G,CAN,400"
				);
			testHelper.ShelfTask.P9_Description = "This Shelf Will Be REJ";
			Factory.Save();
			assessServiceClientMock
				.SetupHasCompletedLearningUnit(Staff, aspectPK1, true)
				.SetupEnsureEnrolled(Staff, aspectPK1);

			SkillAspect_CancelledReviewAndFailedCheckin_Core(testHelper, topWorkflow, aspectPK1);

			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			SkillAspect_CancelledReviewAndFailedCheckin_Core(testHelper, topWorkflow, aspectPK1);
		}

		void SkillAspect_CancelledReviewAndFailedCheckin_Core(ProcessedShelfsServiceTaskTestHelper testHelper, IProcessHeader topWorkflow, Guid aspectPK)
		{
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.RejectedForPendingAspectData, EDIShelvesetInfo.ActionTypes.ShelfCheckin, [aspectPK]);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();

			testHelper.AssertWorkItemTasks(7);
			testHelper.AssertTaskExists(220, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(230, "SHV", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(300, "PRV", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(310, "CBC", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(400, "CBF", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(499, "CBC", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(500, "CH1", "SUS", topWorkflow.PK);

			var reviewTask = testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 499);
			AssertEquals(aspectPK, reviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestWTASubjectAspect_ChangedSkillShouldUpdateReviewTask()
		{
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,20", "CDF,A.G,CLS,10", "CBC,REV,ASN,30", "CHK,A.G,ASN,40");

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, new[] { aspectPK1 });

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, "SH0", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(30, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(40, "CHK", "ASN", topWorkflow.PK);

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, false);
			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK2, false);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			Factory.Save();
			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(7);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, LearningTaskType, "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(21, "COD", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(22, "SHV", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(32, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(42, "CHK", "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType).ToArray();
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("(Optional) Advanced Data Analysis and Methods of P", learningTasks[0].P9_Description);
			Assert(learningTasks[0].IsCurrent);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			learningTasks[0].P9_Status = "CAN";
			newCoding.P9_Status = "CLS";
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV" && t.P9_Status == "ASN");
			newShelf.P9_Type = "SH0";
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(newShelf, ShelfStatuses.Passed, new[] { aspectPK2 });

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(8);
			testHelperWithAspectDat.AssertTaskExists(10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, LearningTaskType, "CAN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(21, "COD", "CLS", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(22, "SH0", "CLS", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(32, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(32, LearningTaskType, "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(42, "CHK", "ASN", topWorkflow.PK);

			learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == LearningTaskType && t.P9_Status == "ASN").ToArray();
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("(Optional) Behavioural Neuroscience", learningTasks[0].P9_Description);
			Assert(learningTasks[0].IsCurrent);

			testHelperWithAspectDat.RefreshWorkItemTasks();
			var updatedReviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			var anotherFactory = Factory.CreateNewFactory();
			var anotherFactoryWorkItemTasks = anotherFactory.Load<WorkItemProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, SQLComparisonOperator.Equal, testHelperWithAspectDat.WorkItem.PK));
			var anotherFactoryReviewTask = anotherFactoryWorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			AssertEquals(1, anotherFactoryReviewTask.SkillsPivots.Count);
			AssertEquals("Review skill updated to new skill", aspectPK2, reviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestWTASubjectAspect_FailedCheckInWithDeletedProcessTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "CH0,A.G,ASN,3", "CDF,A.G,CLS,1", "CBC,REV,CLS,2");

			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, EDIShelvesetInfo.ActionTypes.ShelfCheckin, processTask: null)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid(),
				NotificationEmail = "lee@wtg.com",
			};
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(shelvesetWithAspectReview, new[] { aspectPK1 });
			testHelper.RefreshWorkItemTasks();

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(3);
			AssertEquals(ShelfStatuses.RejectedAndNotified, ServiceTask.shelvesets[0].Status);
		}

		#region ASSESS Review task creation

		public void TestWTAAspect_ReviewerHasCompletedCourse_ShouldNotCreateAssessReview()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer, aspectPK1, true);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 3, tasks.Length);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[2].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerHasCompletedCourse_ReviewInPrerequisiteWorkflow_ShouldNotCreateAssessReview()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Review");
			var workflow3 = bmTestHelper.CreateWorkflow(jobHeader, "Checkin");
			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow2, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow3, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30); // Checkin task is in dependent workflow, but should pick up code review for same PR in prerequisite

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer, aspectPK1, true);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 3, tasks.Length);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[2].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerNotCompletedCourse_ShouldCreateAssessReview()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 4, tasks.Length);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[2].P9_Description);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", ReviewerStaffCode, tasks[2].P9_GS_NKAssignedStaffMember);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[3].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerNotCompletedCourse_ShouldCreateAssessReview_UsingDetailsFromLaterCodeReviewTask()
		{
			const string anotherReviewer = "DEA";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer1 = MasterFilesTestHelper.CreateStaff(Factory, anotherReviewer, "Code Reviewer 1");
			var reviewer2 = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer 2");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: anotherReviewer, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 21);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer1, aspectPK1, false);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer2, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer1, aspectPK1);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer2, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 5, tasks.Length);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[3].P9_Description);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", ReviewerStaffCode, tasks[3].P9_GS_NKAssignedStaffMember);
				AssertEquals("Checkin task P9_Type", "CH0", tasks[4].P9_Type);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[4].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerNotCompletedCourse_AnotherReviewInUnrelatedWorkflowWhoseReviewerHasCompletedCourse_ShouldCreateAssessReview()
		{
			const string reviewerStaffCodeWhoHasNotCompletedCourse = "DEA";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer1 = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer 1");
			var reviewer2 = MasterFilesTestHelper.CreateStaff(Factory, reviewerStaffCodeWhoHasNotCompletedCourse, "Code Reviewer 2");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review 1");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review 2");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 30);

			bmTestHelper.CreateTask(workflow2, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 110);
			bmTestHelper.CreateTask(workflow2, staffCode: reviewerStaffCodeWhoHasNotCompletedCourse, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 120);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow2, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 130);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer1, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer2, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer2, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 7, tasks.Length);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[5].P9_Description);
				AssertEquals("ASSESS review task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[5].P9_Status);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", reviewerStaffCodeWhoHasNotCompletedCourse, tasks[5].P9_GS_NKAssignedStaffMember);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[6].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerNotCompletedCourse_AnotherReviewInUnrelatedWorkflowWhoseReviewerHasCompletedCourse_WhenQualityIterationCreated_ShouldNotCreateAssessReview()
		{
			const string reviewerStaffCodeWhoHasNotCompletedCourse = "DEA";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations();

			var reviewer1 = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer 1");
			var reviewer2 = MasterFilesTestHelper.CreateStaff(Factory, reviewerStaffCodeWhoHasNotCompletedCourse, "Code Reviewer 2");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review 1");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review 2");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 30);

			bmTestHelper.CreateTask(workflow2, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 110);
			bmTestHelper.CreateTask(workflow2, staffCode: reviewerStaffCodeWhoHasNotCompletedCourse, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 120, description: "Regular code review");
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow2, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 130);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer1, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer2, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer2, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 9, tasks.Length);

				AssertEquals("First checkin task P9_Type", EDITaskTypes.TaskActiveCheckin, tasks[5].P9_Type);
				AssertEquals("First checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Cancelled, tasks[5].P9_Status);

				AssertEquals("QI coding task P9_Type", "CDF", tasks[6].P9_Type);
				AssertEquals("QI coding task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[6].P9_Status);

				AssertEquals("QI code review task P9_Type", WorkItemProcessTask.CodeReviewTaskType, tasks[7].P9_Type);
				AssertEquals("QI code review task P9_Description", "Regular code review", tasks[7].P9_Description);
				AssertEquals("QI code review task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[7].P9_Status);
				AssertEquals("QI code review task SkillsPivots", 1, tasks[7].SkillsPivots.Count);

				AssertEquals("QI checkin task P9_Type", EDITaskTypes.TaskCheckin, tasks[8].P9_Type);
				AssertEquals("QI checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[8].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerNotCompletedCourse_ReviewInPrerequisiteWorkflow_ShouldCreateAssessReview()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Review");
			var workflow3 = bmTestHelper.CreateWorkflow(jobHeader, "Checkin");
			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow2, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow3, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30); // Checkin task is in dependent workflow, but should pick up code review for same PR in prerequisite

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 4, tasks.Length);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[2].P9_Description);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", ReviewerStaffCode, tasks[2].P9_GS_NKAssignedStaffMember);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[3].P9_Status);
			});
		}

		public void TestWTAAspect_ReviewerHasCompletedCourse_ReviewInPrerequisiteWorkflowOfAnotherWorkItem_ShouldCreateAssessReview()
		{
			const string reviewerStaffCodeWhoHasNotCompletedCourse = "DEA";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer1 = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer 1");
			var reviewer2 = MasterFilesTestHelper.CreateStaff(Factory, reviewerStaffCodeWhoHasNotCompletedCourse, "Code Reviewer 2");

			var prerequisiteWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var prerequisiteJobHeader = bmTestHelper.GetJobHeaderForParent(prerequisiteWorkItem, Factory, false);
			var prerequisiteWorkflow = bmTestHelper.CreateWorkflow(prerequisiteJobHeader, "Prereq");
			bmTestHelper.CreateTask(prerequisiteWorkflow, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(prerequisiteWorkflow, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding");
			var workflow2 = bmTestHelper.CreateWorkflow(jobHeader, "Review");
			var workflow3 = bmTestHelper.CreateWorkflow(jobHeader, "Checkin");
			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			prerequisiteWorkflow.GetOrCreateDependencyLink(workflow2);

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow2, staffCode: reviewerStaffCodeWhoHasNotCompletedCourse, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow3, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30); // Checkin task is in dependent workflow, but should pick up code review for same PR in prerequisite

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer1, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer2, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer2, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 4, tasks.Length);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[2].P9_Description);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", reviewerStaffCodeWhoHasNotCompletedCourse, tasks[2].P9_GS_NKAssignedStaffMember);
				AssertEquals("Checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Suspended, tasks[3].P9_Status);
			});
		}

		public void TestWTAAspect_AssessReviewAlreadyExists_ShouldNotCreateAssessReviewInQualityIteration()
		{
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations();

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Code Reviewer");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: WorkItemProcessTask.CodeReviewTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20, description: "Regular code review");
			bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: WorkItemProcessTask.CodeReviewTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 29, description: "ASSESS Review");
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			assessServiceClientMock.SetupHasCompletedLearningUnit(Staff, aspectPK1, true);
			assessServiceClientMock.SetupHasCompletedLearningUnit(reviewer, aspectPK1, false);
			assessServiceClientMock.SetupEnsureEnrolled(reviewer, aspectPK1);

			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(checkinTask, ShelfStatuses.Rejected, new[] { aspectPK1 });
			InitialiseAndRunTaskSchedule(ServiceTask);

			var newFactory = Factory.CreateNewFactory();
			var tasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, workItem.PK) { OrderBy = ProcessTasksSchema.P9_Sequence.Name });
			var taskDetails = ProcessedShelfsServiceTaskTestHelper.GetTaskDetails(tasks);

			CombineAssertions(taskDetails, () =>
			{
				AssertEquals("Total number of tasks", 7, tasks.Length);

				AssertEquals("ASSESS review task P9_Type", WorkItemProcessTask.CodeReviewTaskType, tasks[2].P9_Type);
				AssertEquals("ASSESS review task P9_Description", "ASSESS Review", tasks[2].P9_Description);
				AssertEquals("ASSESS review task P9_GS_NKAssignedStaffMember", ReviewerStaffCode, tasks[2].P9_GS_NKAssignedStaffMember);

				AssertEquals("First checkin task P9_Type", EDITaskTypes.TaskActiveCheckin, tasks[3].P9_Type);
				AssertEquals("First checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Cancelled, tasks[3].P9_Status);

				AssertEquals("QI coding task P9_Type", "CDF", tasks[4].P9_Type);
				AssertEquals("QI coding task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[4].P9_Status);

				AssertEquals("QI code review task P9_Type", WorkItemProcessTask.CodeReviewTaskType, tasks[5].P9_Type);
				AssertEquals("QI code review task P9_Description", "Regular code review", tasks[5].P9_Description);
				AssertEquals("QI code review task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[5].P9_Status);

				AssertEquals("QI checkin task P9_Type", EDITaskTypes.TaskCheckin, tasks[6].P9_Type);
				AssertEquals("QI checkin task P9_Status", ProcessTaskStatusCodeList.Codes.Assigned, tasks[6].P9_Status);
			});
		}

		#endregion

		#region Submission result email for RPA rejections

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourse()
		{
			RunSubmissionForReviewRequirementsRejected(true, false, null, aspectPK1, @"Your submission was rejected because there are ASSESS requirements, and the code reviewer (REV - Reviewer Name) has not passed the corresponding ASSESS courses.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourseAndAspectReview()
		{
			RunSubmissionForReviewRequirementsRejected(true, false, "CAP", aspectPK1, @"Your submission was rejected because there are pending Aspect reviews and ASSESS requirements, and the code reviewer (REV - Reviewer Name) has not passed the corresponding ASSESS courses.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourse_WhenMultipleReviewersPresent()
		{
			RunSubmissionForReviewRequirementsRejected(true, true, null, aspectPK1, @"Your submission was rejected because there are ASSESS requirements, and the code reviewers (REV - Reviewer Name; RV2 - Second Reviewer) have not collectively passed the corresponding ASSESS courses.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourseAndAspectReview_WhenMultipleReviewersPresent()
		{
			RunSubmissionForReviewRequirementsRejected(true, true, "CAP", aspectPK1, @"Your submission was rejected because there are pending Aspect reviews and ASSESS requirements, and the code reviewers (REV - Reviewer Name; RV2 - Second Reviewer) have not collectively passed the corresponding ASSESS courses.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourse_WhenNoReviewerPresent()
		{
			RunSubmissionForReviewRequirementsRejected(false, false, null, aspectPK1, @"Your submission was rejected because there are ASSESS requirements, but DAT could not find a completed code review task for this submission. This task must be present in the same workflow as the submission task, or in a prerequisite workflow.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAssessCourseAndAspectReview_WhenNoReviewerPresent()
		{
			RunSubmissionForReviewRequirementsRejected(false, false, "CAP", aspectPK1, @"Your submission was rejected because there are pending Aspect reviews and ASSESS requirements, but DAT could not find a completed code review task for this submission. This task must be present in the same workflow as the submission task, or in a prerequisite workflow.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAspectReview()
		{
			RunSubmissionForReviewRequirementsRejected(true, false, "CAP", null, @"Your submission was rejected because there are pending Aspect reviews.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		public void TestEmailForRejectionDueToReviewRequirements_ForAspectReview_WhenNoReviewerPresent()
		{
			RunSubmissionForReviewRequirementsRejected(false, false, "CAP", null, @"Your submission was rejected because there are pending Aspect reviews.<br /><br />See <a href=""http://crikey.wtg.zone/TestResults/");
		}

		void RunSubmissionForReviewRequirementsRejected(bool shouldCreateCodeReview, bool shouldCreateMultipleCodeReviews, string capability, Guid? aspectPK, string emailBody)
		{
			if (capability != null)
			{
				MasterFilesTestHelper.CreateCapability(Factory, capability, "Description");
			}

			Staff.GS_EmailAddress = "me@something.com";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var serviceTaskTestHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			serviceTaskTestHelper.SetUpForQualityIterations(defaultSettingForReleaseGroupsNotSpecified: false);

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, ReviewerStaffCode, "Reviewer Name");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = bmTestHelper.GetJobHeaderForParent(workItem, Factory, false);
			var workflow1 = bmTestHelper.CreateWorkflow(jobHeader, "Coding and Review");

			bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			if (shouldCreateCodeReview)
			{
				bmTestHelper.CreateTask(workflow1, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
			}
			if (shouldCreateMultipleCodeReviews)
			{
				const string secondReviewerStaffCode = "RV2";
				var reviewer2 = MasterFilesTestHelper.CreateStaff(Factory, secondReviewerStaffCode, "Second Reviewer");
				Factory.Save();
				bmTestHelper.CreateTask(workflow1, staffCode: secondReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20);
				assessServiceClientMock
					.SetupHasCompletedLearningUnit(reviewer2, aspectPK.Value, false)
					.SetupEnsureEnrolled(reviewer2, aspectPK.Value);
			}
			var checkinTask = (WorkItemProcessTask)bmTestHelper.CreateTask(workflow1, staffCode: CoderStaffCode, taskType: "CH0", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 30);

			Factory.Save();

			var submissionInfo = CreateSubmisionInfo(checkinTask, EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.RejectedForPendingAspectData);
			if (capability != null)
			{
				var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), capability, "Aspect");
				submissionInfo.AspectReviews.Add(aspectReview);
			}
			if (aspectPK != null)
			{
				assessServiceClientMock
					.SetupHasCompletedLearningUnit(Staff, aspectPK.Value, true)
					.SetupHasCompletedLearningUnit(reviewer, aspectPK.Value, false)
					.SetupEnsureEnrolled(reviewer, aspectPK.Value);

				var assessReview = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK.Value, "Aspect");
				submissionInfo.AspectReviews.Add(assessReview);
			}
			ServiceTask.shelvesets.Add(submissionInfo);

			InitialiseAndRunTaskSchedule(ServiceTask);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			CombineAssertions("Email details", () =>
			{
				AssertEquals("email.Subject", "Failure: Your submission to DAT has been rejected due to pending review requirements", email.Subject);
				AssertStartsWith("email.Body", emailBody, email.Body);
			});
		}

		#endregion

		#region Implementation

		EDIShelvesetInfo CreateSubmisionInfo(WorkItemProcessTask submissionTask, string submissionType, string submissionStatus)
		{
			return new EDIShelvesetInfo(@"CORP\Test.User", submissionTask.P9_Description, submissionType, submissionTask)
			{
				Status = submissionStatus,
				UserHeaderPK = Guid.NewGuid(),
			};
		}

		void AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(WorkItemProcessTask newShelf, string status, Guid[] aspectPKs)
		{
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(newShelf, status, EDIShelvesetInfo.ActionTypes.ShelfsetTest, aspectPKs);
		}

		void AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(WorkItemProcessTask submissionTask, string submissionStatus, string submissionType, Guid[] aspectPKs)
		{
			var submission = CreateSubmisionInfo(submissionTask, submissionType, submissionStatus);
			AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(submission, aspectPKs);
		}

		void AddShelfWithAssessWTASubjectToServiceTaskShelvsetList(EDIShelvesetInfo shelvesetInfo, Guid[] aspectPKs)
		{
			foreach (var aspectPK in aspectPKs)
			{
				var aspectReview = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK, "Assess Aspect " + aspectPK);
				shelvesetInfo.AspectReviews.Add(aspectReview);
			}
			ServiceTask.shelvesets.Add(shelvesetInfo);
			Factory.Save();
		}

		ProcessedShelfsServiceTaskForTest ServiceTask => serviceTask ?? (serviceTask = new ProcessedShelfsServiceTaskForTest(Factory));
		ProcessedShelfsServiceTaskForTest serviceTask;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			WorkItemProcessTaskTestHelper.SetupLearningTaskRegistry(LearningTaskType);
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Staff.GS_Code = CoderStaffCode;
			Factory.Save();

			aspectPK1 = Guid.NewGuid();
			aspectPK2 = Guid.NewGuid();

			assessServiceClientMock = new Mock<IAssessServiceClient>()
				.SetupGetLearningUnitName(aspectPK1, LearningUnitName1)
				.SetupGetLearningUnitName(aspectPK2, LearningUnitName2)
				.SetupGetLearningUnitUrl(aspectPK1, "https://nothing/assess/5004")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/5005");
			disposables = new DisposableList(1);
			disposables.Add(ObjectFactory.Substitute(assessServiceClientMock.Object));
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			disposables?.Dispose();
		}

		Mock<IAssessServiceClient> assessServiceClientMock;
		DisposableList disposables;
		Guid aspectPK1;
		Guid aspectPK2;

		const string LearningTaskType = "LRN";
		const string LearningUnitName1 = "Advanced Data Analysis and Methods of Psychological Inquiry";
		const string LearningUnitName2 = "Behavioural Neuroscience";

		const string CoderStaffCode = "A.G";
		const string ReviewerStaffCode = "REV";

		GlbStaff Staff;

		#endregion
	}
}
