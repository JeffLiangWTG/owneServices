using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Testing.Public.Recruiter.TalentDataHelpers;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(TG_HRJobApplicationLogStageCompletedTrigger))]
	class TG_HRJobApplicationLogStageCompletedTriggerTest : DBCreateTriggerScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		public void TestNewAppActivityInserted_DoesNothing()
		{
			var activity = CreateTalActivity(TestConnection, "Generic activity", "CPF");

			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Generic stage", 1, "CMF");
			_ = CreateTalDefaultActivity(TestConnection, activity, defaultStage);

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Generic stage", 1, "CMF");
			var appActivity = CreateTalApplicationActivity(TestConnection, activity, stage);

			_ = CreateTalApplicationActivity(TestConnection, CreateTalActivity(TestConnection, "Generic activity 2", "RND"), stage);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(0, results.Count());
			}
		}

		public void TestOtherFieldUpdated_DoesNothing()
		{
			var activity = CreateTalActivity(TestConnection, "Generic activity", "CPF");

			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Generic stage", 1, "CMF");
			_ = CreateTalDefaultActivity(TestConnection, activity, defaultStage);

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Generic stage", 1, "CMF");
			var appActivity = CreateTalApplicationActivity(TestConnection, activity, stage);

			UpdateTalApplicationActivityURL(appActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(0, results.Count());
			}
		}

		public void TestCompletedUtcSet_OtherAppActivitiesNotComplete_DoesNothing()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var activity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var appActivity = CreateTalApplicationActivity(TestConnection, activity, stage);

			var otherActivity = CreateTalActivity(TestConnection, "Personality test", "TST", "TAL");
			var otherAppActivity = CreateTalApplicationActivity(TestConnection, otherActivity, stage);

			UpdateTalApplicationActivityCompletedUtc(appActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(0, results.Count());
			}
		}

		public void TestCompletedUtcSet_NoOtherAppActivities_HasNextStage_CreatesCompletedAndReachedLogs()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var activity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var appActivity = CreateTalApplicationActivity(TestConnection, activity, stage);

			var otherStage = CreateTalApplicationStage(TestConnection, application, "Development testing", 2, "DVT");
			var otherActivity = CreateTalActivity(TestConnection, "Coding test", "TST", "HAK");
			var otherAppActivity = CreateTalApplicationActivity(TestConnection, otherActivity, otherStage);

			UpdateTalApplicationActivityCompletedUtc(appActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(2, results.Count());

				var expected = ExpectedLogs(
					application,
					new EventAndReference { Reference = "STA=COMPLETED", Event = "SDT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "DVT" });

				AssertContainsExactElementsInAnyOrder(comparator, expected, results);
			}
		}

		public void TestCompletedUtcSet_OtherAppActivitiesComplete_HasNextStage_CreatesCompletedAndReachedLogs()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var iqTestActivity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var iqAppActivity = CreateTalApplicationActivity(TestConnection, iqTestActivity, stage);

			var personalityTestActivity = CreateTalActivity(TestConnection, "Personality test", "TST", "TAL");
			var personalityAppActivity = CreateTalApplicationActivity(TestConnection, personalityTestActivity, stage);

			var otherStage = CreateTalApplicationStage(TestConnection, application, "Development testing", 2, "DVT");
			var otherActivity = CreateTalActivity(TestConnection, "Coding test", "TST", "HAK");
			var otherAppActivity = CreateTalApplicationActivity(TestConnection, otherActivity, otherStage);

			UpdateTalApplicationActivityCompletedUtc(iqAppActivity);
			UpdateTalApplicationActivityCompletedUtc(personalityAppActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(2, results.Count());

				var expected = ExpectedLogs(
					application,
					new EventAndReference { Reference = "STA=COMPLETED", Event = "SDT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "DVT" });

				AssertContainsExactElementsInAnyOrder(comparator, expected, results);
			}
		}

		public void TestCompletedUtcSet_NoOtherAppActivities_IsLastStage_CreatesCompletedLogOnly()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var iqTestActivity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var iqAppActivity = CreateTalApplicationActivity(TestConnection, iqTestActivity, stage);

			var personalityTestActivity = CreateTalActivity(TestConnection, "Personality test", "TST", "TAL");
			var personalityAppActivity = CreateTalApplicationActivity(TestConnection, personalityTestActivity, stage);

			var otherStage = CreateTalApplicationStage(TestConnection, application, "Development testing", 2, "DVT");
			var otherActivity = CreateTalActivity(TestConnection, "Coding test", "TST", "HAK");
			var otherAppActivity = CreateTalApplicationActivity(TestConnection, otherActivity, otherStage);

			UpdateTalApplicationActivityCompletedUtc(iqAppActivity);
			UpdateTalApplicationActivityCompletedUtc(personalityAppActivity);

			UpdateTalApplicationActivityCompletedUtc(otherAppActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(3, results.Count());

				var expected = ExpectedLogs(
					application,
					new EventAndReference { Reference = "STA=COMPLETED", Event = "SDT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "DVT" },
					new EventAndReference { Reference = "STA=COMPLETED", Event = "DVT" });

				AssertContainsExactElementsInAnyOrder(comparator, expected, results);
			}
		}

		public void TestCompletedUtcSet_OtherAppActivitiesComplete_IsLastStage_CreatesCompletedLogOnly()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var iqTestActivity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var iqAppActivity = CreateTalApplicationActivity(TestConnection, iqTestActivity, stage);

			var codingStage = CreateTalApplicationStage(TestConnection, application, "Development testing", 2, "DVT");
			var codingActivity = CreateTalActivity(TestConnection, "Coding test", "TST", "HAK");
			var codingAppActivity = CreateTalApplicationActivity(TestConnection, codingActivity, codingStage);

			var harderCodingActivity = CreateTalActivity(TestConnection, "Challenge problem", "TST", "HAK");
			var harderCodingAppActivity = CreateTalApplicationActivity(TestConnection, harderCodingActivity, codingStage);

			UpdateTalApplicationActivityCompletedUtc(iqAppActivity);
			UpdateTalApplicationActivityCompletedUtc(codingAppActivity);
			UpdateTalApplicationActivityCompletedUtc(harderCodingAppActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(3, results.Count());

				var expected = ExpectedLogs(
					application,
					new EventAndReference { Reference = "STA=COMPLETED", Event = "SDT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "DVT" },
					new EventAndReference { Reference = "STA=COMPLETED", Event = "DVT" });

				AssertContainsExactElementsInAnyOrder(comparator, expected, results);
			}
		}

		public void TestCompletedUtcSet_MultipleStages_CreatesCorrectLogs()
		{
			var campaign = CreateJobCampaign(TestConnection);
			var defaultStage = CreateTalDefaultStage(TestConnection, campaign, "Standard testing", 1, "SDT");

			var application = CreateJobApplication(TestConnection, campaign, CreateJobApplicant(TestConnection));
			var stage = CreateTalApplicationStage(TestConnection, application, "Standard testing", 1, "SDT");

			var iqTestActivity = CreateTalActivity(TestConnection, "IQ test", "TST", "TAL");
			var iqAppActivity = CreateTalApplicationActivity(TestConnection, iqTestActivity, stage);

			var codingStage = CreateTalApplicationStage(TestConnection, application, "Development testing", 2, "DVT");
			var codingActivity = CreateTalActivity(TestConnection, "Coding test", "TST", "HAK");
			var codingAppActivity = CreateTalApplicationActivity(TestConnection, codingActivity, codingStage);

			var interviewStage = CreateTalApplicationStage(TestConnection, application, "Interviews", 3, "INT");
			var scheduleActivity = CreateTalActivity(TestConnection, "Schedule interview", "SCH");
			var scheduleAppActivity = CreateTalApplicationActivity(TestConnection, scheduleActivity, interviewStage);

			var offerStage = CreateTalApplicationStage(TestConnection, application, "Offer", 4, "OFF");
			var awaitActivity = CreateTalActivity(TestConnection, "Await offer", "AWT");
			var awaitAppActivity = CreateTalApplicationActivity(TestConnection, awaitActivity, offerStage);

			UpdateTalApplicationActivityCompletedUtc(iqAppActivity);
			UpdateTalApplicationActivityCompletedUtc(codingAppActivity);

			var query = string.Format(@"
SELECT SL_Parent, SL_Table, SL_Reference, SL_GS_NKUser, SL_SE_NKEvent, SL_DataSource
FROM dbo.StmALog WHERE SL_Parent = '{0}'", application.ToString());

			using (var cmd = TestConnection.Command(query))
			{
				var results = DataUtils.GetDataTableFromCommand(cmd).AsEnumerable();
				AssertEquals(4, results.Count());

				var expected = ExpectedLogs(
					application,
					new EventAndReference { Reference = "STA=COMPLETED", Event = "SDT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "DVT" },
					new EventAndReference { Reference = "STA=COMPLETED", Event = "DVT" },
					new EventAndReference { Reference = "STA=REACHED", Event = "INT" });

				AssertContainsExactElementsInAnyOrder(comparator, expected, results);
			}
		}

		void UpdateTalApplicationActivityURL(Guid pk)
		{
			var sqlText = @"
UPDATE dbo.TalApplicationActivity
SET TPA_URL = 'https://http.cat/', TPA_SystemLastEditTimeUtc = GETUTCDATE(), TPA_SystemLastEditUser = 'E'
WHERE TPA_PK = @PK
";

			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateTalApplicationActivityCompletedUtc(Guid pk)
		{
			var sqlText = @"
UPDATE dbo.TalApplicationActivity
SET TPA_CompletedUtc = GETUTCDATE(), TPA_SystemLastEditTimeUtc = GETUTCDATE(), TPA_SystemLastEditUser = 'E'
WHERE TPA_PK = @PK
";

			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		IEnumerable<DataRow> ExpectedLogs(Guid parent, params EventAndReference[] eventAndReferences)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("SL_Parent", typeof(Guid)));
			expected.Columns.Add(new DataColumn("SL_Table", typeof(string)));
			expected.Columns.Add(new DataColumn("SL_Reference", typeof(string)));
			expected.Columns.Add(new DataColumn("SL_GS_NKUser", typeof(string)));
			expected.Columns.Add(new DataColumn("SL_SE_NKEvent", typeof(string)));
			expected.Columns.Add(new DataColumn("SL_DataSource", typeof(string)));

			foreach (var eventAndReference in eventAndReferences)
			{
				var row = expected.NewRow();
				row["SL_Parent"] = parent;
				row["SL_Table"] = "HRJobApplication";
				row["SL_Reference"] = eventAndReference.Reference;
				row["SL_GS_NKUser"] = "E  ";
				row["SL_SE_NKEvent"] = eventAndReference.Event;
				row["SL_DataSource"] = "G";
				expected.Rows.Add(row);
			}

			return expected.AsEnumerable();
		}
	}

	struct EventAndReference
	{
		public string Reference { get; set; }
		public string Event { get; set; }
	}
}
