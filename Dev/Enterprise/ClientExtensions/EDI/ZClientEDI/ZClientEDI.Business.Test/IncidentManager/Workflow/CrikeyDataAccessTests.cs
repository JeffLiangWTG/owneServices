using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using WTG.DevTools.Common;
using WTG.DevTools.SourceControl;
using static System.FormattableString;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class CrikeyDataAccessTests : TestCase
	{
		public void TestGetScheduledShelvesGitPullsCheckedin()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var gitCheckinCIN = AddDatData(
					conn,
					"SCH",
					ShelfStatuses.CheckedIn,
					processTask.PK,
					"WI00262787",
					new List<(string gitPull, string title)> { ("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview", "WI00262787 Add .vs to gitignore") });

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.CheckedIn, factory);

				CombineAssertions("Git pull checked in", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);
					var shelfSetInfo = shelvesetInfos[0];

					AssertEquals(gitCheckinCIN, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(1, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals(string.Empty, shelfSetInfo.Name);
					AssertEquals("CIN", shelfSetInfo.Status);
					AssertEquals("master", shelfSetInfo.PrimaryBranch);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497", shelfSetInfo.DatGitPullRequests[0].OverviewUri.ToString());
					AssertEquals(1497, shelfSetInfo.DatGitPullRequests[0].PullRequestId);
					AssertEquals("CIN", shelfSetInfo.DatGitPullRequests[0].Status);
					AssertEquals("WI00262787 Add .vs to gitignore", shelfSetInfo.DatGitPullRequests[0].Title);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick", shelfSetInfo.DatGitPullRequests[0].TargetRepository);
				});
			}
		}

		public void TestGetScheduledShelvesGitPullFromAutoPatcher()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var gitCheckinCIN = AddDatData(
					conn,
					"SCH",
					ShelfStatuses.CheckedIn,
					processTask.PK,
					"WI00345134",
					new List<(string gitPull, string title)> { ("https://devops.wisetechglobal.com/wtg/Glow/_git/Glow/pullrequest/118?_a=overview", "WI00345134 d293d71 DPR Auto") },
					shelfName: null);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.CheckedIn, factory);

				CombineAssertions("Git pull checked in", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);
					var shelfSetInfo = shelvesetInfos[0];

					AssertEquals(gitCheckinCIN, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(1, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals(string.Empty, shelfSetInfo.Name);
					AssertEquals("CIN", shelfSetInfo.Status);
					AssertEquals("master", shelfSetInfo.PrimaryBranch);
					AssertEquals("https://devops.wisetechglobal.com/wtg/Glow/_git/Glow/pullrequest/118", shelfSetInfo.DatGitPullRequests[0].OverviewUri.ToString());
					AssertEquals(118, shelfSetInfo.DatGitPullRequests[0].PullRequestId);
					AssertEquals("CIN", shelfSetInfo.DatGitPullRequests[0].Status);
					AssertEquals("WI00345134 d293d71 DPR Auto", shelfSetInfo.DatGitPullRequests[0].Title);
					AssertEquals("https://devops.wisetechglobal.com/wtg/Glow/_git/Glow", shelfSetInfo.DatGitPullRequests[0].TargetRepository);
				});
			}
		}

		public void TestGetScheduledShelvesTfsPassed()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var tfsShelfPAS = AddDatData(
					conn,
					"SHV",
					ShelfStatuses.Passed,
					processTask.PK,
					"WI00282994",
					null,
					"WI00282994-tfs-only");

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Passed, factory);

				CombineAssertions("Tfs only passed", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);

					var shelfSetInfo = shelvesetInfos[0];
					AssertEquals(tfsShelfPAS, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(0, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals("WI00282994-tfs-only", shelfSetInfo.Name);
					AssertEquals("PAS", shelfSetInfo.Status);
					AssertEquals("$/Branch1", shelfSetInfo.PrimaryBranch);
				});
			}
		}

		public void TestGetScheduledShelvesTfsAndGitRejected()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var tfsandGitREJ = AddDatData(
					conn,
					"SHV",
					ShelfStatuses.Rejected,
					processTask.PK,
					"WI00282994",
					new List<(string gitPull, string title)> { ("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1539", "Make long-lasting DB connections more reliable") },
					"WI00282994-v1");

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				CombineAssertions("Tfs and Git pull passed", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);
					var shelfSetInfo = shelvesetInfos[0];

					AssertEquals(tfsandGitREJ, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(1, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals("WI00282994-v1", shelfSetInfo.Name);
					AssertEquals("REJ", shelfSetInfo.Status);
					AssertEquals("$/Branch1", shelfSetInfo.PrimaryBranch);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1539", shelfSetInfo.DatGitPullRequests[0].OverviewUri.ToString());
					AssertEquals(1539, shelfSetInfo.DatGitPullRequests[0].PullRequestId);
					AssertEquals("REJ", shelfSetInfo.DatGitPullRequests[0].Status);
					AssertEquals("Make long-lasting DB connections more reliable", shelfSetInfo.DatGitPullRequests[0].Title);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService", shelfSetInfo.DatGitPullRequests[0].TargetRepository);
				});
			}
		}

		public void TestGetScheduledShelvesTfsAndMultipleGitRejected()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var tfsandMultipleGitREJ = AddDatData(
					conn,
					"SHV",
					ShelfStatuses.Rejected,
					processTask.PK,
					"WI00262737",
					new List<(string gitPull, string title)>
					{
						("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/BulkAnalysisRunner/pullrequest/1496?_a=overview", ""),
						("http://tfs.wtg.zone:8080/tfs/cargowise/InternalTools/_git/GitTest/pullrequest/1344?_a=overview", "Initial Check-In for Vessel Speed Service"),
					},
					"WI00262737-UnitTest");

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				CombineAssertions("Tfs and Git pull passed", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);
					var shelfSetInfo = shelvesetInfos[0];

					AssertEquals(tfsandMultipleGitREJ, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(2, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals("WI00262737-UnitTest", shelfSetInfo.Name);
					AssertEquals("REJ", shelfSetInfo.Status);
					AssertEquals("$/Branch1", shelfSetInfo.PrimaryBranch);

					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/BulkAnalysisRunner/pullrequest/1496", shelfSetInfo.DatGitPullRequests[0].OverviewUri.ToString());
					AssertEquals(1496, shelfSetInfo.DatGitPullRequests[0].PullRequestId);
					AssertEquals("REJ", shelfSetInfo.DatGitPullRequests[0].Status);
					AssertEquals("WI00262737", shelfSetInfo.DatGitPullRequests[0].Title);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/BulkAnalysisRunner", shelfSetInfo.DatGitPullRequests[0].TargetRepository);

					AssertEquals("http://tfs.wtg.zone:8080/tfs/cargowise/InternalTools/_git/GitTest/pullrequest/1344", shelfSetInfo.DatGitPullRequests[1].OverviewUri.ToString());
					AssertEquals(1344, shelfSetInfo.DatGitPullRequests[1].PullRequestId);
					AssertEquals("REJ", shelfSetInfo.DatGitPullRequests[1].Status);
					AssertEquals("Initial Check-In for Vessel Speed Service", shelfSetInfo.DatGitPullRequests[1].Title);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/cargowise/InternalTools/_git/GitTest", shelfSetInfo.DatGitPullRequests[1].TargetRepository);
				});
			}
		}

		public void TestGetScheduledShelvesTfsAndGitCheckedInDeploymentFailed()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var tfsAndGitCheckinDJF = AddDatData(
					conn,
					"SCH",
					ShelfStatuses.DeploymentJobFailed,
					processTask.PK,
					"WI00280667",
					new List<(string gitPull, string title)> { ("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1539?_a=overview", "For auto deployment") },
					"WI00280667",
					ShelfStatuses.CheckedIn);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus(ShelfStatuses.DeploymentJobFailed, factory);

				CombineAssertions("Tfs and Git pull passed", () =>
				{
					AssertEquals(1, shelvesetInfos.Count);
					var shelfSetInfo = shelvesetInfos[0];

					AssertEquals(tfsAndGitCheckinDJF, shelfSetInfo.UserHeaderPK);
					AssertEquals(0, shelfSetInfo.AspectReviews.Count);
					AssertEquals(1, shelfSetInfo.DatGitPullRequests.Count);
					AssertEquals("WI00280667", shelfSetInfo.Name);
					AssertEquals("DJF", shelfSetInfo.Status);
					AssertEquals("$/Branch1", shelfSetInfo.PrimaryBranch);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1539", shelfSetInfo.DatGitPullRequests[0].OverviewUri.ToString());
					AssertEquals(1539, shelfSetInfo.DatGitPullRequests[0].PullRequestId);
					AssertEquals("CIN", shelfSetInfo.DatGitPullRequests[0].Status);
					AssertEquals("For auto deployment", shelfSetInfo.DatGitPullRequests[0].Title);
					AssertEquals("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService", shelfSetInfo.DatGitPullRequests[0].TargetRepository);
				});
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806", Justification = "Testing the constructor")]
		public void TestConstructorThrowsExceptionWhenConnectionIsNull()
		{
			var exception = AssertExceptionThrown<ArgumentNullException>(() => new CrikeyDataAccess(null));
			AssertEquals("crikeyConnection", exception.ParamName);
		}

		public void TestGetScheduledShelvesByStatusThrowsExceptionWhenFactoryIsNull()
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikey = new CrikeyDataAccess(connection);
				var exception = AssertExceptionThrown<ArgumentNullException>(() => crikey.GetScheduledShelvesByStatus("abc", null));
				AssertEquals("factory", exception.ParamName);
			}
		}

		public void TestGetScheduledShelvesByStatusNoTypeCastingExceptions()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = FormattableString.Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('42a2c0bb-70bf-41c6-9be1-237fba677eee', 'u01');

INSERT INTO [UserTestHeader](UH_PK, UH_U1, UH_Submitted, UH_Type, UH_Status, UH_P9, UH_ShelfName) VALUES ('6f490a7a-fc9a-4685-b53e-a045fb10e706', 
'42a2c0bb-70bf-41c6-9be1-237fba677eee', '2020-1-1', 'CH0', 'PAS', '{processTask.PK}', null);
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfos = crikey.GetScheduledShelvesByStatus("PAS", factory);
				AssertEquals(1, shelvesetInfos.Count);
			}
		}

		public void TestLoadShelfByProcessTaskNoExceptions()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var processTask = new BusinessObjectFactory(conn).NewWithValidTestData<WorkItemProcessTask>();

				var sql = FormattableString.Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('42a2c0bb-70bf-41c6-9be1-237fba677eee', 'u01');

INSERT INTO [UserTestHeader](UH_PK, UH_U1, UH_Submitted, UH_Type, UH_Status, UH_P9, UH_ShelfName) VALUES ('6f490a7a-fc9a-4685-b53e-a045fb10e706', 
'42a2c0bb-70bf-41c6-9be1-237fba677eee', '2018-1-1', 'CH0', 'CLS', '{processTask.PK}', 'WI123');
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfo = crikey.LoadShelfByProcessTask(processTask);

				AssertEquals("WI123", shelvesetInfo.Name);
			}
		}

		public void TestAspectInfoOnlyReturnedForFailedCheckinWithPendingAspectData()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTaskCIN = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskPAS = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskPAS_Assess = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskREJPendingAspect = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskREJPendingAspect_Assess = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskREJRejectedAspect = factory.NewWithValidTestData<WorkItemProcessTask>();
				var processTaskREJTestFail = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('54107f0b-635c-4c40-a4a5-4ece532f100a', 'u01');

INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('ab644524-5062-41f8-891d-dca489d41dc9', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskCIN.PK}', 'CIN', 'Checkin Pass With Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('1718aac3-5352-457c-a67d-c9e0ed5a7610', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SHV', '{processTaskPAS.PK}', 'PAS', 'Shelf With Pending Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('22A006A5-E972-41B9-9039-F6FD65C68703', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SHV', '{processTaskPAS_Assess.PK}', 'PAS', 'Shelf With Pending ASSESS Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('851390ec-c548-4246-bca1-a073ff303436', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskREJPendingAspect.PK}', 'REJ', 'Checkin With Pending Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('951EC195-DE01-424C-A982-7A79A588CA82', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskREJPendingAspect_Assess.PK}', 'REJ', 'Checkin With Pending ASSESS Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('8937b54c-2c7f-4c61-8449-ee25116c0b49', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskREJRejectedAspect.PK}', 'REJ', 'Checkin With Rejected Aspect Data')
INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('1048c6f6-8365-4867-aa07-47b909ee8298', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskREJTestFail.PK}', 'REJ', 'Checkin Fail Test and With Pending Aspect Data')

INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('6e38b00c-e4f6-4221-bc37-b4dc9f0e4ca4', 'ab644524-5062-41f8-891d-dca489d41dc9', 'Checkin Pass With Aspect Data', '$/Branch', 'CIN', NULL)
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('7053254d-2ee2-4cb6-812d-c405bddf08ac', '1718aac3-5352-457c-a67d-c9e0ed5a7610', 'Shelf With Pending Aspect Data', '$/Branch', 'PAS', NULL)
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('B6B57A83-C252-4571-972A-6BEFF79E21D9', '22A006A5-E972-41B9-9039-F6FD65C68703', 'Shelf With Pending ASSESS Aspect Data', '$/Branch', 'PAS', NULL)
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('a050fa3a-6e82-484f-a9f8-4bc58c244165', '851390ec-c548-4246-bca1-a073ff303436', 'Checkin With Pending Aspect Data', '$/Branch', 'REJ', 'Pending aspect data found\r\nAspect: AspectName, Key: [Pending Aspect Key], Value: [Pending Aspect Value]\r\n')
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status) VALUES ('4B6DAF54-ED65-48A9-BCB8-7DCC3A95F5E1', '951EC195-DE01-424C-A982-7A79A588CA82', 'Checkin With Pending ASSESS Aspect Data', '$/Branch', 'REJ')
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('4007a85c-ed5a-418c-bbce-f6c2b61b35e7', '8937b54c-2c7f-4c61-8449-ee25116c0b49', 'Checkin With Rejected Aspect Data', '$/Branch', 'REJ', 'Rejected aspect data found\r\nAspect: AspectName, Key: [Rejected Aspect Key], Value: [Rejected Aspect Value]\r\n')
INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('d1c3679b-9ac6-4793-95b3-fedbc1fd23ee', '1048c6f6-8365-4867-aa07-47b909ee8298', 'Checkin Fail Test and With Pending Aspect Data', '$/Branch', 'PAS', NULL)

INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-5f3a5d2289fa', 'AspectName', '$/Branch', 'Type', 'CAP', 1, 1, 1)

INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-b945c15e5f4d', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Accepted Aspect Key')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('3ada0829-5920-424c-94c0-5309aa003150', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Pending Aspect Key')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('4f88f87f-d675-4aca-9c01-19e8a4c202dd', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Rejected Aspect Key')

INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability)						VALUES ('a50374ce-06b3-4b60-99cb-c1c6ccc3c2a1', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'ab644524-5062-41f8-891d-dca489d41dc9', 'CAP')			--ACC
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability)						VALUES ('c32e1067-ad85-4b77-98f1-e134d8ab0e0e', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '8937b54c-2c7f-4c61-8449-ee25116c0b49', 'CAP')			--REJ SCH
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability)						VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '851390ec-c548-4246-bca1-a073ff303436', 'CAP')			--PEN SCH
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_LMSCode, AR_LMSLearningUnitID)	VALUES ('13F1B1EA-F673-4385-861C-65963693C2F8', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '951EC195-DE01-424C-A982-7A79A588CA82', 'WTA', '123')	--PEN SCH ASSESS
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability)						VALUES ('a3070acd-1d06-46a0-9f4d-10ebde3f05f4', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '1718aac3-5352-457c-a67d-c9e0ed5a7610', 'CAP')			--PEN SHV
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_LMSCode, AR_LMSLearningUnitID)	VALUES ('A469DBDD-C62E-476C-A1D1-D463FC20C0D9', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '22A006A5-E972-41B9-9039-F6FD65C68703', 'WTA', '123')	--PEN SHV ASSESS
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability)						VALUES ('09EC99B0-0739-46AC-98E0-D1F4D0076049', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '1048c6f6-8365-4867-aa07-47b909ee8298', 'CAP')			--PEN SCH failed test

INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-10a4fa2152f7', '51a32a48-63f8-4860-b766-b945c15e5f4d', 'Accepted Aspect Value', '$/Branch/File1.cs', 1, 2, 1, 'ACC', '54107f0b-635c-4c40-a4a5-4ece532f100a', 'Approved by User', '2018-10-31')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('ceec16ba-481d-4ed0-978f-5d8f71d7dbd3', '3ada0829-5920-424c-94c0-5309aa003150', 'Pending Aspect Value', '$/Branch/File12.cs', 1, 2, 0, 'PEN', NULL, '',  '2018-10-31')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('aafe425e-3c3c-42a3-a5c5-9504881bf698', '4f88f87f-d675-4aca-9c01-19e8a4c202dd', 'Rejected Aspect Value', '$/Branch/File2.cs', 1, 2, 0, 'REJ', '54107f0b-635c-4c40-a4a5-4ece532f100a', 'Rejected by User', '2018-10-31')

INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('a50374ce-06b3-4b60-99cb-c1c6ccc3c2a1', 'b7b07ca5-6cfd-4c54-8360-10a4fa2152f7') --ACC
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('c32e1067-ad85-4b77-98f1-e134d8ab0e0e', 'aafe425e-3c3c-42a3-a5c5-9504881bf698') --REJ
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('a3070acd-1d06-46a0-9f4d-10ebde3f05f4', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('13F1B1EA-F673-4385-861C-65963693C2F8', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('09EC99B0-0739-46AC-98E0-D1F4D0076049', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('A469DBDD-C62E-476C-A1D1-D463FC20C0D9', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoCIN = crikey.GetScheduledShelvesByStatus(ShelfStatuses.CheckedIn, factory);
				var shelvesetInfoPAS = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Passed, factory);
				var shelvesetInfoREJ = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				AssertEquals("There should be one checked in shelf in the results", 1, shelvesetInfoCIN.Count);
				AssertEquals("There should be two passed shelf in the results", 2, shelvesetInfoPAS.Count);
				AssertEquals("There should be three rejected shelves in the results", 4, shelvesetInfoREJ.Count);

				AssertEquals("The checkin should have no aspect review data attached", 0, shelvesetInfoCIN[0].AspectReviews.Count);
				AssertEquals("The passed shelf should have no aspect review data attached", 0, shelvesetInfoPAS[0].AspectReviews.Count);
				AssertEquals("The passed shelf should have no aspect review data attached", 1, shelvesetInfoPAS[1].AspectReviews.Count);
				AssertEquals("The rejected checkin should have no aspect review data as it has a rejected aspect", 0, shelvesetInfoREJ.Single(s => s.RelatedProcessTask.PK == processTaskREJRejectedAspect.PK).AspectReviews.Count);
				AssertEquals("The rejected checkin should have no aspect review data as it failed a test", 0, shelvesetInfoREJ.Single(s => s.RelatedProcessTask.PK == processTaskREJTestFail.PK).AspectReviews.Count);

				var shelvesetInfoWithAspectReviewData = shelvesetInfoREJ.Single(s => s.RelatedProcessTask.PK == processTaskREJPendingAspect.PK);
				AssertEquals("This shelf should contain aspect review data", 1, shelvesetInfoWithAspectReviewData.AspectReviews.Count);
				AssertEquals("AspectName", shelvesetInfoWithAspectReviewData.AspectReviews[0].AspectName);
				AssertEquals("CAP", shelvesetInfoWithAspectReviewData.AspectReviews[0].Capability);
				AssertEquals(Guid.Parse("bb2a8f3e-43e8-44f9-88d9-33958643cdd5"), shelvesetInfoWithAspectReviewData.AspectReviews[0].PK);

				var shelvesetInfoWithAspectReviewData_Assess = shelvesetInfoREJ.Single(s => s.RelatedProcessTask.PK == processTaskREJPendingAspect_Assess.PK);
				AssertEquals("This shelf should contain aspect review data", 1, shelvesetInfoWithAspectReviewData_Assess.AspectReviews.Count);
				AssertEquals("AspectName", shelvesetInfoWithAspectReviewData_Assess.AspectReviews[0].AspectName);
				AssertEquals(null, shelvesetInfoWithAspectReviewData_Assess.AspectReviews[0].Capability);
				AssertEquals(Guid.Parse("202db324-73a2-48dd-9d88-5f3a5d2289fa"), shelvesetInfoWithAspectReviewData_Assess.AspectReviews[0].AspectPK);
				AssertEquals(Guid.Parse("13F1B1EA-F673-4385-861C-65963693C2F8"), shelvesetInfoWithAspectReviewData_Assess.AspectReviews[0].PK);

				var shelvesetInfoPASData_Assess = shelvesetInfoPAS.Single(s => s.RelatedProcessTask.PK == processTaskPAS_Assess.PK);
				AssertEquals("This shelf should contain aspect review data", 1, shelvesetInfoPASData_Assess.AspectReviews.Count);
				AssertEquals("AspectName", shelvesetInfoPASData_Assess.AspectReviews[0].AspectName);
				AssertEquals(null, shelvesetInfoPASData_Assess.AspectReviews[0].Capability);
				AssertEquals(Guid.Parse("202db324-73a2-48dd-9d88-5f3a5d2289fa"), shelvesetInfoPASData_Assess.AspectReviews[0].AspectPK);
				AssertEquals(Guid.Parse("a469dbdd-c62e-476c-a1d1-d463fc20c0d9"), shelvesetInfoPASData_Assess.AspectReviews[0].PK);
			}
		}

		public void TestAspectInfoForFailedCheckinWithMultiplePendingAspectDataPoints()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTaskREJPendingAspect = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('54107f0b-635c-4c40-a4a5-4ece532f100a', 'u01');

INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('851390ec-c548-4246-bca1-a073ff303436', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTaskREJPendingAspect.PK}', 'REJ', 'Checkin With Pending Aspect Data')

INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('a050fa3a-6e82-484f-a9f8-4bc58c244165', '851390ec-c548-4246-bca1-a073ff303436', 'Checkin With Pending Aspect Data', '$/Branch', 'REJ', 'Pending aspect data found\r\nAspect: AspectName, Key: [Pending Aspect Key], Value: [Pending Aspect Value]\r\n')

INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-5f3a5d2289fa', 'AspectName', '$/Branch', 'Type', 'CAP', 1, 1, 1)

INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-b945c15e5f4d', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Pending Aspect Key1')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('3ada0829-5920-424c-94c0-5309aa003150', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Pending Aspect Key2')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('4f88f87f-d675-4aca-9c01-19e8a4c202dd', '202db324-73a2-48dd-9d88-5f3a5d2289fa', 'Pending Aspect Key3')

INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability) VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', '202db324-73a2-48dd-9d88-5f3a5d2289fa', '851390ec-c548-4246-bca1-a073ff303436', 'CAP') --PEN SCH

INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-10a4fa2152f7', '51a32a48-63f8-4860-b766-b945c15e5f4d', 'Pending Aspect Value 1', '$/Branch/File1.cs', 1, 2, 1, 'PEN', NULL, '', '2020-06-15')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('ceec16ba-481d-4ed0-978f-5d8f71d7dbd3', '3ada0829-5920-424c-94c0-5309aa003150', 'Pending Aspect Value 2', '$/Branch/File2.cs', 1, 2, 0, 'PEN', NULL, '', '2020-06-15')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('aafe425e-3c3c-42a3-a5c5-9504881bf698', '4f88f87f-d675-4aca-9c01-19e8a4c202dd', 'Pending Aspect Value 3', '$/Branch/File3.cs', 1, 2, 0, 'PEN', NULL, '', '2020-06-15')

INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', 'b7b07ca5-6cfd-4c54-8360-10a4fa2152f7') --PEN1
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', 'ceec16ba-481d-4ed0-978f-5d8f71d7dbd3') --PEN2
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-33958643cdd5', 'aafe425e-3c3c-42a3-a5c5-9504881bf698') --PEN3
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoREJ = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				AssertEquals("There should be one rejected shelf in the results", 1, shelvesetInfoREJ.Count);

				var shelvesetInfoWithAspectReviewData = shelvesetInfoREJ.Single(s => s.RelatedProcessTask.PK == processTaskREJPendingAspect.PK);
				AssertEquals("This shelf should contain aspect review data", 1, shelvesetInfoWithAspectReviewData.AspectReviews.Count);
				AssertEquals("AspectName", shelvesetInfoWithAspectReviewData.AspectReviews[0].AspectName);
				AssertEquals("CAP", shelvesetInfoWithAspectReviewData.AspectReviews[0].Capability);
				AssertEquals(Guid.Parse("bb2a8f3e-43e8-44f9-88d9-33958643cdd5"), shelvesetInfoWithAspectReviewData.AspectReviews[0].PK);
			}
		}

		public void TestAspectInfoForMultipleAspects()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('54107f0b-635c-4c40-a4a5-4ece532f100a', 'u01');

INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('851390ec-c548-4246-bca1-a073ff303436', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTask.PK}', 'REJ', 'Checkin With Pending Aspect Data')

INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('a050fa3a-6e82-484f-a9f8-4bc58c244165', '851390ec-c548-4246-bca1-a073ff303436', 'Checkin With Pending Aspect Data', '$/Branch', 'REJ', 'Pending aspect data found\r\nAspect: AspectName, Key: [Pending Aspect Key], Value: [Pending Aspect Value]\r\n')

INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-000000000001', 'Aspect1', '$/Branch', 'Type', 'CP1', 1, 1, 1)
INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-000000000002', 'Aspect2', '$/Branch', 'Type', 'CP2', 1, 1, 1)

INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-000000000001', '202db324-73a2-48dd-9d88-000000000001', 'Pending Aspect Key1')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-000000000002', '202db324-73a2-48dd-9d88-000000000002', 'Pending Aspect Key2')

INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000001', '202db324-73a2-48dd-9d88-000000000001', '851390ec-c548-4246-bca1-a073ff303436', 'CP1')
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000002', '202db324-73a2-48dd-9d88-000000000002', '851390ec-c548-4246-bca1-a073ff303436', 'CP2')

INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-000000000001', '51a32a48-63f8-4860-b766-000000000001', 'Pending Aspect Value1', '$/Branch/File1.cs', 1, 2, 0, 'PEN', NULL, '',  '2018-10-31')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-000000000002', '51a32a48-63f8-4860-b766-000000000002', 'Pending Aspect Value2', '$/Branch/File12.cs', 1, 2, 0, 'PEN', NULL, '',  '2018-10-31')

INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000001', 'b7b07ca5-6cfd-4c54-8360-000000000001') --PEN
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000002', 'b7b07ca5-6cfd-4c54-8360-000000000002') --PEN
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoREJ = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				AssertEquals("There should be one rejected shelf in the results", 1, shelvesetInfoREJ.Count);

				AssertEquals("This shelf should contain aspect review data for both aspects", 2, shelvesetInfoREJ[0].AspectReviews.Count);
			}
		}

		public void TestAspectInfoOnlyReturnedForPendingAspects()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User](U1_PK, U1_Name) VALUES ('54107f0b-635c-4c40-a4a5-4ece532f100a', 'u01');

INSERT INTO [UserTestHeader] (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName) VALUES ('851390ec-c548-4246-bca1-a073ff303436', '54107f0b-635c-4c40-a4a5-4ece532f100a', '2018-10-31', 'SCH', '{processTask.PK}', 'REJ', 'Checkin With Pending Aspect Data')

INSERT INTO [UserTest] (UT_PK, UT_UH, UT_Title, UT_Branch, UT_Status, UT_ProcessingError) VALUES ('a050fa3a-6e82-484f-a9f8-4bc58c244165', '851390ec-c548-4246-bca1-a073ff303436', 'Checkin With Pending Aspect Data', '$/Branch', 'REJ', 'Pending aspect data found\r\nAspect: AspectName, Key: [Pending Aspect Key], Value: [Pending Aspect Value]\r\n')

INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-accaccaccacc', 'Aspect Accepted', '$/Branch', 'Type', 'ACC', 1, 1, 1)
INSERT INTO [Aspect] (AS_PK, AS_Name, AS_Branch, AS_DataExtratorType, AS_Capability, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES ('202db324-73a2-48dd-9d88-000000000000', 'Aspect Pending', '$/Branch', 'Type', 'PEN', 1, 1, 1)

INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-accaccaccacc', '202db324-73a2-48dd-9d88-accaccaccacc', 'Accepted Aspect Key')
INSERT INTO [AspectKey] (AK_PK, AK_AS, AK_Key) VALUES ('51a32a48-63f8-4860-b766-000000000000', '202db324-73a2-48dd-9d88-000000000000', 'Pending Aspect Key')

INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability) VALUES ('bb2a8f3e-43e8-44f9-88d9-accaccaccacc', '202db324-73a2-48dd-9d88-accaccaccacc', '851390ec-c548-4246-bca1-a073ff303436', 'ACC')
INSERT INTO [AspectReview] (AR_PK, AR_AS, AR_UH, AR_Capability) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000000', '202db324-73a2-48dd-9d88-000000000000', '851390ec-c548-4246-bca1-a073ff303436', 'PEN')

INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-accaccaccacc', '51a32a48-63f8-4860-b766-accaccaccacc', 'Accepted Aspect Value', '$/Branch/File1.cs', 1, 2, 0, 'ACC', '54107f0b-635c-4c40-a4a5-4ece532f100a', 'Approved by User', '2018-10-31')
INSERT INTO [AspectData] ([AD_PK], [AD_AK], [AD_Value], [AD_DataSource], [AD_Line], [AD_Col], [AD_CurrentCheckedin], [AD_Status], [AD_U1], [AD_Comments], [AD_LastUpdateTime]) VALUES ('b7b07ca5-6cfd-4c54-8360-000000000000', '51a32a48-63f8-4860-b766-000000000000', 'Pending Aspect Value', '$/Branch/File12.cs', 1, 2, 0, 'PEN', NULL, '',  '2018-10-31')

INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-accaccaccacc', 'b7b07ca5-6cfd-4c54-8360-accaccaccacc') --ACC
INSERT INTO [AspectReviewData] (ARD_AR, ARD_AD) VALUES ('bb2a8f3e-43e8-44f9-88d9-000000000000', 'b7b07ca5-6cfd-4c54-8360-000000000000') --PEN
");
				conn.ExecuteNonQuery(sql);

				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoREJ = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				AssertEquals("There should be one rejected shelf in the results", 1, shelvesetInfoREJ.Count);

				AssertEquals("This shelf should contain aspect review data", 1, shelvesetInfoREJ[0].AspectReviews.Count);
				AssertEquals("Aspect Pending", shelvesetInfoREJ[0].AspectReviews[0].AspectName);
				AssertEquals("PEN", shelvesetInfoREJ[0].AspectReviews[0].Capability);
				AssertEquals(Guid.Parse("bb2a8f3e-43e8-44f9-88d9-000000000000"), shelvesetInfoREJ[0].AspectReviews[0].PK);
			}
		}

		public void TestUatCombinedBuildIncludedInShelvesByStatus()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User]
			(U1_PK, U1_Name) 
	VALUES	('CA48E6C9-908E-45D2-92F9-10BDFCABA70A', 'CORP\s_datservice'),
			('E628E24E-707B-498B-AB0C-43F1BA89D66B', 'CORP\Lee.Coady');

INSERT INTO [UserTestHeader] 
			(UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_ShelfName, UH_Status, UH_NotificationEmail) 
	VALUES	('7AAD24A5-F478-4B27-9942-2E6CEB7C6490', 'E628E24E-707B-498B-AB0C-43F1BA89D66B', '2019-02-01', 'UCC', NULL, 'REJ UAT Combined Child', 'REJ', NULL),
			('54C3FDE2-F8D5-437B-9CC0-8451449C6597', 'E628E24E-707B-498B-AB0C-43F1BA89D66B', '2019-02-01', 'UCC', NULL, 'PAS UAT Combined Child', 'PAS', NULL),
			('909C385F-0C2E-4E6A-A7C0-B17106FA88AC', 'CA48E6C9-908E-45D2-92F9-10BDFCABA70A', '2019-02-01', 'UCB', NULL, 'PAS UAT Combined Build', 'PAS', 'lee.coady@wisetechglobal.com'),
			('F6F74917-FC1F-4ACA-AA9F-D31E98755E54', 'CA48E6C9-908E-45D2-92F9-10BDFCABA70A', '2019-02-01', 'UCB', NULL, 'REJ UAT Combined Build', 'REJ', 'lee.coady@wisetechglobal.com');

INSERT INTO [UserTest]
			(UT_PK, UT_Title, UT_Branch, UT_Status, UT_UH, UT_ProcessingError)	
	VALUES	('24456B12-8F7D-441C-8E5D-29859E138252', 'REJ UAT Combined Child', '$/Dev', 'REJ', '7AAD24A5-F478-4B27-9942-2E6CEB7C6490', 'Cannot combine file.cs conflicts with another shelf that has already been included in this combined build'),
			('ACEF21FB-CAE9-4BAF-9802-C4A4D5ED3E6E', 'PAS UAT Combined Child', '$/Dev', 'PAS', '54C3FDE2-F8D5-437B-9CC0-8451449C6597', NULL),
			('93F1872E-0155-422A-A98D-1124D5BB4E48', 'PAS UAT Combined Build', '$/Dev', 'PAS', '909C385F-0C2E-4E6A-A7C0-B17106FA88AC', NULL),
			('4495D39A-9506-4D02-8E67-C8AA4B36E94F', 'REJ UAT Combined Build', '$/Dev', 'PAS', 'F6F74917-FC1F-4ACA-AA9F-D31E98755E54', NULL);
			");
				conn.ExecuteNonQuery(sql);
				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoPAS = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Passed, factory);
				var shelvesetInfoREJ = crikey.GetScheduledShelvesByStatus(ShelfStatuses.Rejected, factory);

				AssertEquals("There should be two rejected shelves in the results", 2, shelvesetInfoREJ.Count);
				AssertEquals("There should be two accepted shelves in the results", 2, shelvesetInfoPAS.Count);
			}
		}

		public void TestMultipleGitUserTestsWithTheSamePullRequest()
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sql = Invariant($@"
INSERT INTO [User]
			(U1_PK, U1_Name) 
	VALUES	('a0d9119f-976c-4448-8958-0ead704958e9', 'CORP\bret.ehlert');

INSERT INTO [UserTestHeader] 
			(UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_ShelfName, UH_Status, UH_NotificationEmail) 
	VALUES	('387b407f-035c-4cd8-8f2c-99f129d32d16', 'a0d9119f-976c-4448-8958-0ead704958e9', '2020-08-22', 'SCH', '{processTask.PK}', '', 'CIN', NULL);

INSERT INTO [UserTest]
			(UT_PK, UT_Title, UT_Branch, UT_Status, UT_UH, UT_IsBranchInShelfChanges, UT_TargetRepository, UT_PullRequestId, UT_Path)	
	VALUES	('cf5644a5-bb6b-4aba-9027-ec02a2b03c1f', 'WI00341858 Build.xml Aspects', '$/CWShared/CW1Deployment', 'PAS', '387b407f-035c-4cd8-8f2c-99f129d32d16', 0, null, null, null),
			('1c12c211-42dd-4015-88cc-a00c491a32d9', 'WI00341858 Build.xml Aspects', 'master', 'CIN', '387b407f-035c-4cd8-8f2c-99f129d32d16', 1, 'https://devops.wisetechglobal.com/wtg/Glow/_git/Glow', 21, '/CargoWiseOne/Configuration'),
			('d70a093b-1d8b-48ac-bc2e-cb1bf74d590d', 'WI00341858 Build.xml Aspects', 'master', 'CIN', '387b407f-035c-4cd8-8f2c-99f129d32d16', 1, 'https://devops.wisetechglobal.com/wtg/Glow/_git/Glow', 21, '/DotNet'),
			('b8c9b761-93be-4c76-a7ee-696f30b616a2', 'WI00341858 Build.xml Aspects', 'master', 'CIN', '387b407f-035c-4cd8-8f2c-99f129d32d16', 1, 'https://devops.wisetechglobal.com/wtg/Glow/_git/Glow', 21, '/ConfigurationModel');
			");
				conn.ExecuteNonQuery(sql);
				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfoCIN = crikey.GetScheduledShelvesByStatus(ShelfStatuses.CheckedIn, factory);

				AssertEquals("There should only be one shelf in the results", 1, shelvesetInfoCIN.Count);
				AssertEquals("There should only be 1 Git Pull Request in the results", 1, shelvesetInfoCIN[0].DatGitPullRequests.Count);
				AssertEquals("TargetRepository should match", "https://devops.wisetechglobal.com/wtg/Glow/_git/Glow", shelvesetInfoCIN[0].DatGitPullRequests[0].TargetRepository);
				AssertEquals("PullRequestId should match", 21, shelvesetInfoCIN[0].DatGitPullRequests[0].PullRequestId);
				AssertEquals("Title should match", "WI00341858 Build.xml Aspects", shelvesetInfoCIN[0].DatGitPullRequests[0].Title);
				AssertEquals("Status should match", "CIN", shelvesetInfoCIN[0].DatGitPullRequests[0].Status);
			}
		}

		public void TestMultipleGitUserTestsWithDifferentStatusShvRejPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.Shelveset, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusShvRejCan()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.Shelveset, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusShvRejCanPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.Shelveset, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusSchCinPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.ShelfCheckIn, ShelfStatuses.CheckedIn, ShelfStatuses.CheckedIn, ShelfStatuses.CheckedIn, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusSchCinRej()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.ShelfCheckIn, ShelfStatuses.DeploymentJobFailed, ShelfStatuses.CheckedIn, ShelfStatuses.CheckedIn, ShelfStatuses.Rejected);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusSchRejPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.ShelfCheckIn, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusSchRejCan()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.ShelfCheckIn, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusSchRejCanPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.ShelfCheckIn, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusAsbCanPasRej()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.AspectOnlyBuild, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled, ShelfStatuses.Passed, ShelfStatuses.Rejected);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusUabCanRejPas()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.UATBuild, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Cancelled, ShelfStatuses.Rejected, ShelfStatuses.Passed);
		}

		public void TestMultipleGitUserTestsWithDifferentStatusUccPasCanRej()
		{
			MultipleGitUserTestsWithDifferentStatus(UserTestTypes.UATCombinedChild, ShelfStatuses.Rejected, ShelfStatuses.Rejected, ShelfStatuses.Passed, ShelfStatuses.Cancelled, ShelfStatuses.Rejected);
		}

		public void MultipleGitUserTestsWithDifferentStatus(string type, string headerStatus, string expectedStatus, params string[] testStatuses)
		{
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var processTask = factory.NewWithValidTestData<WorkItemProcessTask>();

				var sqlBuilder = new StringBuilder(Invariant($@"
INSERT INTO [User]
			(U1_PK, U1_Name) 
	VALUES	('a0d9119f-976c-4448-8958-0ead704958e9', 'CORP\bret.ehlert');

INSERT INTO [UserTestHeader] 
			(UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_ShelfName, UH_Status, UH_NotificationEmail) 
	VALUES	('387b407f-035c-4cd8-8f2c-99f129d32d16', 'a0d9119f-976c-4448-8958-0ead704958e9', '2020-08-22', '{type}', '{processTask.PK}', '', '{headerStatus}', NULL);

INSERT INTO [UserTest]
			(UT_PK, UT_Title, UT_Branch, UT_Status, UT_UH, UT_IsBranchInShelfChanges, UT_TargetRepository, UT_PullRequestId, UT_Path)	
	VALUES	-- "));
				foreach (var testStatus in testStatuses)
				{
					sqlBuilder.AppendLine(",");
					sqlBuilder.AppendLine(Invariant($"('{Guid.NewGuid()}', 'UT Title', 'master', '{testStatus}', '387b407f-035c-4cd8-8f2c-99f129d32d16', 1, 'http://Server.com/Something/Coll/_git/Proj', 1, 'Path/{testStatus}')"));
				}
				sqlBuilder.AppendLine(";");
				conn.ExecuteNonQuery(sqlBuilder.ToString());
				var crikey = new CrikeyDataAccess(conn);
				var shelvesetInfo = crikey.GetScheduledShelvesByStatus(headerStatus, factory);

				CombineAssertions("", () =>
				{
					AssertEquals("There should only be one shelf in the results", 1, shelvesetInfo.Count);
					AssertEquals("There should only be 1 Git Pull Request in the results", 1, shelvesetInfo[0].DatGitPullRequests.Count);
					AssertEquals("TargetRepository should match", "http://Server.com/Something/Coll/_git/Proj", shelvesetInfo[0].DatGitPullRequests[0].TargetRepository);
					AssertEquals("PullRequestId should match", 1, shelvesetInfo[0].DatGitPullRequests[0].PullRequestId);
					AssertEquals("Title should match", "UT Title", shelvesetInfo[0].DatGitPullRequests[0].Title);
					AssertEquals("Status should match", expectedStatus, shelvesetInfo[0].DatGitPullRequests[0].Status);
				});
			}
		}

		#region Helpers

		Guid AddDatData(DbConnection conn, string type, string uhStatus, ZGuid p9Pk, string workItem, List<(string gitPull, string title)> gitPulls = null, string shelfName = "", string utStaus = null)
		{
			var uhComments = string.Empty;
			if (gitPulls != null)
			{
				uhComments = string.Join(System.Environment.NewLine, gitPulls.Select(x => x.gitPull));
			}

			var uhpk = AddUserTestHeader(conn, type, p9Pk.ToGuid(), uhStatus, shelfName, uhComments);

			if (!string.IsNullOrEmpty(shelfName))
			{
				AddUserTest(conn, uhpk, "$/Branch1", utStaus ?? uhStatus, Guid.NewGuid());
			}

			if (gitPulls != null)
			{
				foreach (var gitPull in gitPulls)
				{
					var pullUrl = PullRequestUrl.ParseMany(gitPull.gitPull).FirstOrDefault();

					string sourceBranch = workItem;
					string targetRepository = pullUrl?.RepositoryUrl;
					int? gitPullRequestId = pullUrl?.PullRequestId;
					string path = "/";
					string title = string.IsNullOrEmpty(gitPull.title) ? workItem : gitPull.title;
					string status = utStaus ?? uhStatus;

					Guid? sourceCommit = Guid.NewGuid();
					Guid? targetCommit = status == ShelfStatuses.CheckedIn ? Guid.NewGuid() : new Guid?();

					AddUserTest(conn, uhpk, "master", status, Guid.NewGuid(), sourceBranch, targetRepository, gitPullRequestId, path, title, sourceCommit, targetCommit);
				}
			}

			return uhpk;
		}

		Guid AddUserTestHeader(DbConnection connection, string type, Guid p9Pk, string status, string shelfName, string comments)
		{
			var uhPk = Guid.NewGuid();
			using (var command = connection.Command(
				@"
INSERT INTO UserTestHeader
(UH_PK, UH_U1, UH_Submitted, UH_Type, UH_P9, UH_Status, UH_ShelfName, UH_DateRecordAdded, UH_Comments)
VALUES
(@uhPk, (select top 1 U1_PK from [User]), getdate(), @type, @p9Pk, @status, @shelfName, getdate(), @comments)
"))
			{
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, uhPk);
				command.AddParameter("type", SqlDbType.VarChar, 3, type);
				command.AddParameter("p9Pk", SqlDbType.UniqueIdentifier, p9Pk);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.AddParameter("shelfName", SqlDbType.VarChar, 64, shelfName ?? (object)DBNull.Value);
				command.AddParameter("comments", SqlDbType.VarChar, -1, comments);
				command.ExecuteNonQuery();
			}
			return uhPk;
		}

		void AddUserTest(
			DbConnection connection,
			Guid uhPk,
			string branch,
			string status,
			Guid utPk,
			string sourceBranch = null,
			string targetRepository = null,
			int? gitPullRequestId = null,
			string path = null,
			string title = null,
			Guid? sourceCommit = null,
			Guid? targetCommit = null)
		{
			using (var command = connection.Command(
				@"
INSERT INTO UserTest
(UT_PK, UT_UH, UT_Branch, UT_Status, UT_IsBranchInShelfChanges, UT_IsDeployOnlyChain, UT_TargetRepository, UT_PullRequestId, UT_Path, UT_Title, UT_SourceBranch, UT_SourceCommit, UT_TargetCommit)
VALUES
(@utPk, @uhPk, @branch, @status, 1, 0, @targetRepository, @requestId, @path, @title, @sourceBranch, @sourceCommit, @targetCommit)
"))
			{
				command.AddParameter("utPk", SqlDbType.UniqueIdentifier, utPk);
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, uhPk);
				command.AddParameter("branch", SqlDbType.VarChar, 128, branch);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.AddParameter("targetRepository", SqlDbType.VarChar, 256, targetRepository ?? (object)DBNull.Value);
				command.AddParameter("requestId", SqlDbType.Int, gitPullRequestId ?? (object)DBNull.Value);
				command.AddParameter("path", SqlDbType.VarChar, 256, path ?? (object)DBNull.Value);
				command.AddParameter("title", SqlDbType.VarChar, 128, title ?? string.Empty);
				command.AddParameter("sourceBranch", SqlDbType.VarChar, 256, sourceBranch ?? string.Empty);
				command.AddParameter("sourceCommit", SqlDbType.UniqueIdentifier, sourceCommit ?? (object)DBNull.Value);
				command.AddParameter("targetCommit", SqlDbType.UniqueIdentifier, targetCommit ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}
