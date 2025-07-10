using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings.Testing
{
	[TestedType(typeof(RenameDtbBookingQueueServiceTaskDescription))]
	class RenameDtbBookingQueueServiceTaskDescriptionTest : DataTransformationTestCase
	{
		Guid S5_PK_ServiceTaskToBeModified = new Guid("aff4298e-eb25-486f-a9bf-5d11272f48e7");
		Guid S5_PK_DescriptionAlreadyUpdated = new Guid("F125B437-EC71-43B0-96C1-67833EFEBCA4");
		Guid S5_PK_ServiceTaskWithDifferentTypeOfDocument = new Guid("75D0A9E8-DFD2-48F8-B8C9-F6B22A7996AE");
		Guid S5_PK_ServiceTaskWithDifferentScheduleType = new Guid("68ea69c5-2480-46bc-b4d3-826366f2f7cf");

		protected override void AssertTransformationResults()
		{
			CombineAssertions("Should rename the description and audit details of DtbBookingQueueServiceTask", () =>
			{
				AssertDescription(S5_PK_ServiceTaskToBeModified, "KMQ", "DOM", "Transport Job Generator", "~BP", DateTime.UtcNow);
				AssertDescription(S5_PK_DescriptionAlreadyUpdated, "KMQ", "DOM", "Transport Job Generator", "AAA", new DateTime(2023, 5, 9, 12, 38, 0));
				AssertDescription(S5_PK_ServiceTaskWithDifferentTypeOfDocument, "KMQ", "ZZZ", "Unrelated task with same S5_ScheduleType but different S5_TypeOfDocument", "AAA", new DateTime(2023, 5, 9, 12, 38, 0));
				AssertDescription(S5_PK_ServiceTaskWithDifferentScheduleType, "ZZZ", "DOM", "Unrelated service task", "AAA", new DateTime(2023, 5, 9, 12, 38, 0));
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameDtbBookingQueueServiceTaskDescription();
		}

		protected override void PrepareTestData()
		{
			string sql = @$"insert [dbo].[StmScheduleTask] ([S5_PK],[S5_ScheduleType],[S5_TypeOfDocument],[S5_ScheduleDescription],[S5_TaskPeriod],[S5_WeekDaysOnly],[S5_TaskPeriodCount],[S5_DayNumber],[S5_DayList],[S5_MonthNumber],[S5_WeekDayOccurrenceNumber],[S5_StartDate],[S5_EndAfterCount],[S5_EndDate],[S5_ScheduleActualRunCount],[S5_AccountingPeriodScheduleFrstRun],[S5_DateScheduleFirstRun],[S5_NextScheduledPrintRunTimeUtc],[S5_CurrentPrintRunTime],[S5_IsActive],[S5_IsPrivate],[S5_ScheduleState],[S5_DailyStartTime],[S5_DailyEndTime],[S5_ParentTableCode],[S5_ParentID],[S5_GB],[S5_RunTimeInMinutes],[S5_GS_NKPrintUser],[S5_SystemCreateTimeUtc],[S5_SystemCreateUser],[S5_SystemLastEditTimeUtc],[S5_SystemLastEditUser],[S5_OverdueDurationInSeconds])
				select '{S5_PK_ServiceTaskToBeModified}','KMQ','DOM','Transport Booking Queue Service Task','T',0,1,0,'NNNNNNN',0,0,'2021-08-05 00:00:00.000',0,NULL,0,0,NULL,'2023-05-09 14:23:50.113',NULL,1,1,NULL,NULL,NULL,'SH',NULL,NULL,0,'','2021-08-05 01:41:00','','2023-05-09 12:38:00','AAA',0 UNION ALL
				select '{S5_PK_DescriptionAlreadyUpdated}','KMQ','DOM','Transport Job Generator','T',0,1,0,'NNNNNNN',0,0,'2021-08-05 00:00:00.000',0,NULL,0,0,NULL,'2023-05-09 14:23:50.113',NULL,1,1,NULL,NULL,NULL,'SH',NULL,NULL,0,'','2021-08-05 01:41:00','','2023-05-09 12:38:00','AAA',0 UNION ALL
				select '{S5_PK_ServiceTaskWithDifferentTypeOfDocument}','KMQ','ZZZ','Unrelated task with same S5_ScheduleType but different S5_TypeOfDocument','T',0,1,0,'NNNNNNN',0,0,'2021-08-05 00:00:00.000',0,NULL,0,0,NULL,'2023-05-09 14:23:50.113',NULL,1,1,NULL,NULL,NULL,'SH',NULL,NULL,0,'','2021-08-05 01:41:00','','2023-05-09 12:38:00','AAA',0 UNION ALL
				select '{S5_PK_ServiceTaskWithDifferentScheduleType}','ZZZ','DOM','Unrelated service task','T',0,1,0,'NNNNNNN',0,0,'2021-08-05 00:00:00.000',0,NULL,0,0,NULL,'2023-05-09 14:23:54.430',NULL,1,1,NULL,NULL,NULL,'SH',NULL,NULL,0,'','2021-08-05 03:41:00','','2023-05-09 12:38:00','AAA',0;";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void AssertDescription(Guid s5_PK, string code, string typeOfDocument, string expectedDescription, string expectedSystemLastEditUser, DateTime expectedSystemLastEditTimeUtc)
		{
			var sqlText = $"SELECT S5_PK, S5_ScheduleDescription, S5_SystemLastEditUser, S5_SystemLastEditTimeUtc FROM [dbo].[StmScheduleTask] WHERE S5_PK = '{s5_PK}' AND S5_ScheduleType = '{code}' AND S5_TypeOfDocument = '{typeOfDocument}'";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				var dt = new DataTable("StmScheduleTask");
				dt.Load(reader);
				dt.PrimaryKey = new[] { dt.Columns["S5_PK"] };

				AssertEquals(1, dt.Rows.Count);

				var actualDescription = dt.Rows.Find(s5_PK).Field<string>("S5_ScheduleDescription");
				var actualSystemLastEditUser = dt.Rows.Find(s5_PK).Field<string>("S5_SystemLastEditUser");
				var actualSystemLastEditTimeUtc = dt.Rows.Find(s5_PK).Field<DateTime>("S5_SystemLastEditTimeUtc");

				AssertEquals($"Description should be correct for code: {code}, type of document: {typeOfDocument}", expectedDescription, actualDescription);
				AssertEquals($"S5_SystemLastEditUser should be ~BP if edited, otherwise remain AAA for code: {code}, type of document: {typeOfDocument}", expectedSystemLastEditUser, actualSystemLastEditUser);
				AssertEquals($"S5_SystemLastEditTimeUtc should be updated if edited, otherwise remain unchanged for code: {code}, type of document: {typeOfDocument}", expectedSystemLastEditTimeUtc.ToShortDateString(), actualSystemLastEditTimeUtc.ToShortDateString());
			}
		}
	}
}
