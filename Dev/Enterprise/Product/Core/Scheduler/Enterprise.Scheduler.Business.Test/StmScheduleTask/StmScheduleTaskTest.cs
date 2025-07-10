using System;
using System.Data;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTask))]
	sealed class StmScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		#region BusinessObject Overrides

		[TestDate(2006, 1, 1)]
		public void TestSetDefaultValues()
		{
			AssertEquals("Default value for S5_GB must be currnt branch PK", GlbBranch.CurrentBranch.PK, ScheduleTask.S5_GB);
			AssertEquals("Default value for S5_IsActive must be true", true, ScheduleTask.S5_IsActive);
			AssertEquals("Default value for S5_StartDate must be now", ZDateTime.Today, ScheduleTask.S5_StartDate);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2024, 4, 22, 10, 0, 0, 0)]
		public void TestS5StartDateDefaultValueShouldBeUTC()
		{
			TestDateAttribute.UseUNLOCO = true;
			var stmScheduleTask = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			AssertEquals("Default value for S5_StartDate must be UTC time", new ZDateTime(2024, 4, 21, 14, 0, 0), stmScheduleTask.S5_StartDate);
		}

		public void TestDelete()
		{
			ScheduleTask.Recipients.AddNew();
			ScheduleTask.Recipients.AddNew();
			AssertEquals("Recipients number must be 2", 2, ScheduleTask.Recipients.Count);
			ScheduleTask.Delete();
			AssertEquals("ScheduleTask must be deleted", true, ScheduleTask.IsDeleted);
			AssertEquals("Recipients number must be 0 after deletion of ScheduleTask", 0, ScheduleTask.Recipients.Count);
		}

		public void TestIsAutoLogged()
		{
			AssertEquals(false, ScheduleTask.IsAutoLogged);
		}

		public void TestIsAutoLogOnlyEnabledForACT()
		{
			AssertEquals(true, ScheduleTask.IsAutoLogOnlyEnabledForACT);
		}

		#endregion

		#region Related Business Objects

		public void TestRecipients()
		{
			AssertNotNull("Recipients property should not be null", ScheduleTask.Recipients);
			AssertEquals("Recipients number is 0 by default", 0, ScheduleTask.Recipients.Count);
			ScheduleTask.Recipients.AddNew();
			AssertEquals("Recipients number must be 1 after adding first recipient", 1, ScheduleTask.Recipients.Count);
			ScheduleTask.Recipients.AddNew();
			AssertEquals("Recipients number must be 2 after adding second recipient", 2, ScheduleTask.Recipients.Count);
			ScheduleTask.Recipients.RemoveAll();
			AssertEquals("Recipients number must be 0 after removing all recipients", 0, ScheduleTask.Recipients.Count);
		}

		public void TestRecurrence()
		{
			AssertEquals("IsRegisteredEditableChildObject(Recurrence)", true, ScheduleTask.IsRegisteredEditableChildObject(scheduleTask.Recurrence));
		}

		public void TestGetReceipientsWithEmailAddressFromLogs()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "testuser@gmail.com";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PrepareLogData();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
			}

			AssertNotNull(Env.OutgoingMailManager.EmailsCreated[0].Recipients);
			AssertEquals("Email.Recipients.Count", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("Email.Recipients[0]", "testuser@gmail.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestNoNotificationsWithEmailAddressFromLogs_WhenLastEditingUserIsSystemAccount()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "testuser@gmail.com";
			newStaff.GS_IsSystemAccount = true;
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PrepareLogData();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
			}

			AssertEquals("No Email Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNoNotificationsWithEmailAddressFromLogs_WhenLastEditingUserEmailIsNotValid()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "testuser_email";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PrepareLogData();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
			}

			AssertEquals("No Email Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNoNotificationsWithEmailAddressFromLogs_WhenLastEditingUserIsInactive()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			newStaff.GS_EmailAddress = "testuser@gmail.com";
			newStaff.GS_IsActive = false;
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PrepareLogData();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
			}

			AssertEquals("No Email Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[ExpectNoExceptions]
		public void TestGetEmailOfLastEditingUserNoExceptionThrown()
		{
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_LoginName = "UserA";
			newStaff.GS_Code = "GSA";
			newStaff.GS_EmailAddress = "";
			Factory.Save();

			ScheduleTask.S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			ScheduleTask.S5_ParentID = Guid.NewGuid();
			ScheduleTask.S5_SystemLastEditUser = newStaff.GS_Code;
			ScheduleTask.S5_SystemLastEditTimeUtc = new ZDateTime();
			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
				AssertEquals("Pre-Condition: Should create no emails for notifications - No Email Address", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			newStaff.GS_EmailAddress = "test@wise-test.com";
			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
				AssertNotEquals("Should create some emails for notifications", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestNoAuditLogsGeneratedForScheduleTask()
		{
			ScheduleTask.S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			ScheduleTask.S5_ParentID = Guid.NewGuid();
			Factory.Save();

			ScheduleTask.S5_ParentID = Guid.NewGuid();
			Factory.Save();

			ScheduleTask.Delete();

			var query = new ZQuery(StmALogSchema.SL_Parent, ScheduleTask.PK);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("No audit logs should be generated for schedule task", 0, logs.Length);
		}
		#endregion

		#region Property Overrides

		public void TestS5_ScheduleDescriptionMultilingual()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			using (var mockData = Res.UseMockData())
			{
				var key = scheduleTask.CustomizableDataResourceStrings.Source.GetKey(null, "Data Import Service");
				mockData.Put(key, new ResourceStringData(key, "数据导入服务"));
				mockData.Put(scheduleTask.CustomizableDataResourceStrings.Source.GetKey(null, "Test Services"), new ResourceStringData(key, "测试服务"));

				scheduleTask.S5_ScheduleDescription = "Data Import Service";
				AssertEquals("数据导入服务", scheduleTask.S5_ScheduleDescriptionMultilingual);

				scheduleTask.S5_ScheduleDescription = "Test Services";
				AssertEquals("测试服务", scheduleTask.S5_ScheduleDescriptionMultilingual);
			}
		}

		public void TestHumanReadableServiceTaskIsActuallyHumanReadable()
		{
			AssertEquals("Human readble text should actually be human readable. Log description is a good example of readable.", ScheduleTask.HumanReadableName, ScheduleTask.DescriptionForLog);
		}

		public void TestScheduleStateClearedOnParentIDChange()
		{
			ScheduleTask.S5_ScheduleState = ZBlob.FromAscii("hello everyone");
			AssertEquals(false, ScheduleTask.S5_ScheduleState.IsEmpty);

			ScheduleTask.S5_ParentID = ZGuid.NewZGuid();
			AssertEquals(true, ScheduleTask.S5_ScheduleState.IsEmpty);
		}

		public void TestS5_IsPrivate()
		{
			AssertEquals(true, ScheduleTask.S5_IsPrivateInfo.ReadOnly);
		}

		public void TestCalcNextRunTimeLocal_ReadOnly()
		{
			ScheduleTask.S5_IsPrivate = true;
			AssertEquals(false, ScheduleTask.CalcNextRunTimeLocalInfo.ReadOnly);
			ScheduleTask.S5_IsPrivate = false;
			AssertEquals(false, ScheduleTask.CalcNextRunTimeLocalInfo.ReadOnly);
		}

		public void TestS5_DailyStartTime()
		{
			StmScheduleTask scheduleTask = Factory.New<StmScheduleTask>();

			AssertEquals("DailyStartTime Is Valid?", false, scheduleTask.S5_DailyStartTime.IsValid);
			AssertEquals("ScheduleTask has changes?", false, scheduleTask.HasChanges);

			ZDateTime anotherDateTime = new ZDateTime(2007, 11, 15, 2, 30, 50);
			scheduleTask.S5_DailyStartTime = anotherDateTime;
			AssertEquals("ScheduleTask has changes?", true, scheduleTask.HasChanges);

			AssertEquals("ScheduleTask has changes", true, scheduleTask.HasChanges);
			AssertEquals("DailyStartTime - Year", ZDateTime.MinSmallDateTimeValue.Year, scheduleTask.S5_DailyStartTime.Year);
			AssertEquals("DailyStartTime - Month", ZDateTime.MinSmallDateTimeValue.Month, scheduleTask.S5_DailyStartTime.Month);
			AssertEquals("DailyStartTime - Day", ZDateTime.MinSmallDateTimeValue.Day, scheduleTask.S5_DailyStartTime.Day);
			AssertEquals("DailyStartTime - Hour", anotherDateTime.Hour, scheduleTask.S5_DailyStartTime.Hour);
			AssertEquals("DailyStartTime - Minute", anotherDateTime.Minute, scheduleTask.S5_DailyStartTime.Minute);
			AssertEquals("DailyStartTime - Second", anotherDateTime.Second, scheduleTask.S5_DailyStartTime.Second);

			scheduleTask.ClearHasChanges();
			scheduleTask.S5_DailyStartTime = anotherDateTime;
			AssertEquals("ScheduleTask has changes?", false, scheduleTask.HasChanges);

			scheduleTask.S5_DailyStartTime = ZDateTime.Invalid;
			AssertEquals("DailyStartTime Is Valid?", false, scheduleTask.S5_DailyStartTime.IsValid);
			AssertEquals("ScheduleTask has changes?", true, scheduleTask.HasChanges);
		}

		public void TestS5_DailyEndTime()
		{
			StmScheduleTask scheduleTask = Factory.New<StmScheduleTask>();

			AssertEquals("DailyEndTime Is Valid?", false, scheduleTask.S5_DailyEndTime.IsValid);
			AssertEquals("ScheduleTask has changes?", false, scheduleTask.HasChanges);

			ZDateTime anotherDateTime = new ZDateTime(2007, 11, 15, 2, 30, 50);
			scheduleTask.S5_DailyEndTime = anotherDateTime;
			AssertEquals("ScheduleTask has changes?", true, scheduleTask.HasChanges);

			AssertEquals("ScheduleTask has changes", true, scheduleTask.HasChanges);
			AssertEquals("DailyEndTime - Year", ZDateTime.MinSmallDateTimeValue.Year, scheduleTask.S5_DailyEndTime.Year);
			AssertEquals("DailyEndTime - Month", ZDateTime.MinSmallDateTimeValue.Month, scheduleTask.S5_DailyEndTime.Month);
			AssertEquals("DailyEndTime - Day", ZDateTime.MinSmallDateTimeValue.Day, scheduleTask.S5_DailyEndTime.Day);
			AssertEquals("DailyEndTime - Hour", anotherDateTime.Hour, scheduleTask.S5_DailyEndTime.Hour);
			AssertEquals("DailyEndTime - Minute", anotherDateTime.Minute, scheduleTask.S5_DailyEndTime.Minute);
			AssertEquals("DailyEndTime - Second", anotherDateTime.Second, scheduleTask.S5_DailyEndTime.Second);

			scheduleTask.ClearHasChanges();
			scheduleTask.S5_DailyEndTime = anotherDateTime;
			AssertEquals("ScheduleTask has changes?", false, scheduleTask.HasChanges);

			scheduleTask.S5_DailyEndTime = ZDateTime.Invalid;
			AssertEquals("DailyEndTime Is Valid?", false, scheduleTask.S5_DailyEndTime.IsValid);
			AssertEquals("ScheduleTask has changes?", true, scheduleTask.HasChanges);
		}

		[TestDate(2016, 4, 29, 0, 0, 0)]
		public void TestCalcNextRunTimeLocal()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";
			scheduleTask.S5_GB = branch.PK;

			AssertEquals("[PRE-CONDITION] S5_NextScheduledPrintRunTimeUtc valid?", false, scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid);
			AssertEquals("[PRE-CONDITION] CalcNextRunTimeLocal valid?", false, scheduleTask.CalcNextRunTimeLocal.IsValid);

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 4, 22, 10, 0, 0);
			ZDateTime expectedLocalTime = Env.Time.GetLocalTimeFromUtc(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToDateTime());
			AssertEquals("CalcNextRunTimeLocal", expectedLocalTime, scheduleTask.CalcNextRunTimeLocal);

			// Set to an invalid date time
			scheduleTask.CalcNextRunTimeLocal = ZDateTime.Empty;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", new ZDateTime(2010, 4, 22, 10, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			// Set to a valid one
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2010, 4, 20, 19, 20, 11);
			expectedLocalTime = Env.Time.GetUtcFromLocalTime(scheduleTask.CalcNextRunTimeLocal.ToDateTime());
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", expectedLocalTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2024, 1, 1, 0, 0, 0)]
		public void TestCalcNextRunTimeInDifferentBranch()
		{
			TestDateAttribute.UseUNLOCO = true;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			branch.GB_RL_NKHomePort = "JPTKY";

			var branchForTest = company.Branches.AddNew();
			branchForTest.GB_RL_NKHomePort = "SGSIN";
			branchForTest.GB_Code = "ABD";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var scheduleTask = newFactory.NewWithValidTestData<StmScheduleTask>();
				scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
				scheduleTask.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 19, 0, 0);
				scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
				scheduleTask.Recurrence.WeekDaysOnly = false;
				scheduleTask.Recurrence.TaskPeriodCount = 1;
				scheduleTask.S5_IsActive = true;
				scheduleTask.S5_GB = branch.PK;
				newFactory.Save();

				AssertEquals(new ZDateTime(2024, 1, 1, 10, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				AssertEquals(new ZDateTime(2024, 1, 1, 19, 0, 0), scheduleTask.CalcNextRunTimeLocal);
				AssertEquals(new ZDateTime(1900, 1, 1, 19, 0, 0), scheduleTask.S5_DailyStartTime);
				AssertEquals(new ZDateTime(1900, 1, 1, 19, 0, 0), scheduleTask.CalcDailyStartTimeLocal);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchForTest.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var factoryInCABranch = new BusinessObjectFactory { RefreshEnabled = false };
					var scheduleTaskLoadInCABranch = factoryInCABranch.Load<StmScheduleTask>(scheduleTask.PK);

					AssertEquals(new ZDateTime(2024, 1, 1, 10, 0, 0), scheduleTaskLoadInCABranch.S5_NextScheduledPrintRunTimeUtc);
					AssertEquals(new ZDateTime(2024, 1, 1, 18, 0, 0), scheduleTaskLoadInCABranch.CalcNextRunTimeLocal);
					AssertEquals(new ZDateTime(1900, 1, 1, 19, 0, 0), scheduleTaskLoadInCABranch.S5_DailyStartTime);
					AssertEquals(new ZDateTime(1900, 1, 1, 18, 0, 0), scheduleTaskLoadInCABranch.CalcDailyStartTimeLocal);

					scheduleTaskLoadInCABranch.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 19, 1, 0);
					factoryInCABranch.Save();

					AssertEquals(new ZDateTime(2024, 1, 1, 10, 1, 0), scheduleTaskLoadInCABranch.S5_NextScheduledPrintRunTimeUtc);
					AssertEquals(new ZDateTime(2024, 1, 1, 18, 1, 0), scheduleTaskLoadInCABranch.CalcNextRunTimeLocal);
					AssertEquals(new ZDateTime(1900, 1, 1, 19, 1, 0), scheduleTaskLoadInCABranch.S5_DailyStartTime);
					AssertEquals(new ZDateTime(1900, 1, 1, 18, 1, 0), scheduleTaskLoadInCABranch.CalcDailyStartTimeLocal);
				}

				scheduleTask.Reload();

				AssertEquals(new ZDateTime(2024, 1, 1, 10, 1, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				AssertEquals(new ZDateTime(2024, 1, 1, 19, 1, 0), scheduleTask.CalcNextRunTimeLocal);
				AssertEquals(new ZDateTime(1900, 1, 1, 19, 1, 0), scheduleTask.S5_DailyStartTime);
				AssertEquals(new ZDateTime(1900, 1, 1, 19, 1, 0), scheduleTask.CalcDailyStartTimeLocal);
			}
		}

		[TestDate(2016, 4, 29, 0, 0, 0)]
		public void TestScheduleTaskCalculatesNextRunTimeLocalWithUtcOverride()
		{
			var scheduleTask = Factory.New<StmScheduleTaskForTest>();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";
			scheduleTask.S5_GB = branch.PK;

			AssertEquals("[PRE-CONDITION] S5_NextScheduledPrintRunTimeUtc valid?", false, scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid);
			AssertEquals("[PRE-CONDITION] CalcNextRunTimeLocal valid?", false, scheduleTask.CalcNextRunTimeLocal.IsValid);

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 4, 22, 10, 0, 0);
			ZDateTime expectedLocalTime = Env.Time.GetLocalTimeFromUtc(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToDateTime()).AddDays(2);
			AssertEquals("CalcNextRunTimeLocal", expectedLocalTime, scheduleTask.CalcNextRunTimeLocal);

			// Set to an invalid date time
			scheduleTask.CalcNextRunTimeLocal = ZDateTime.Empty;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", new ZDateTime(2010, 4, 22, 10, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			// Set to a valid one
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2010, 4, 20, 19, 20, 11);
			expectedLocalTime = Env.Time.GetUtcFromLocalTime(scheduleTask.CalcNextRunTimeLocal.ToDateTime()).AddDays(-2);
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", expectedLocalTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		class StmScheduleTaskForTest : StmScheduleTask
		{
			public StmScheduleTaskForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override TimeSpan? UtcOffsetOverride
			{
				get
				{
					return TimeSpan.FromDays(2);
				}
			}
		}

		[TestDate(2016, 4, 29, 0, 0, 0)]
		public void TestCalcDailyStartTimeLocal()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";
			scheduleTask.S5_GB = branch.PK;

			scheduleTask.S5_GB = Env.CurrentBranchPK;

			DateTime utcNow = DateTime.UtcNow;

			TimeSpan localUserOffset = Env.Time.GetUtcOffsetBasedOnUtc(utcNow);
			TimeSpan localScheduleTaskOffset = Env.Time.GetUtcOffsetBasedOnUtc("USFDW", utcNow);

			AssertEquals("[PRE-CONDITION] S5_DailyStartTime valid?", false, scheduleTask.S5_DailyStartTime.IsValid);
			AssertEquals("[PRE-CONDITION] CalcDailyStartTimeLocal valid?", false, scheduleTask.CalcDailyStartTimeLocal.IsValid);

			ZDateTime expectedCalcDailyStartTimeLocal = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)).Add(-localScheduleTaskOffset).Add(localUserOffset).TimeOfDay
				);

			ZDateTime expectedCalcDailyStartTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)).Add(-localUserOffset).TimeOfDay
				);

			scheduleTask.S5_DailyStartTime = new ZDateTime(2010, 4, 22, 10, 0, 0);
			AssertEquals("S5_DailyStartTime", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)), scheduleTask.S5_DailyStartTime);
			//S5_DailyStartTime is local to the schedule task branch. CalcDailyStartTimeLocal is local to the logged in user branch.
			AssertEquals("CalcDailyStartTimeLocal", expectedCalcDailyStartTimeLocal, scheduleTask.CalcDailyStartTimeLocal);
			AssertEquals("CalcDailyStartTimeUtc", expectedCalcDailyStartTimeUtc, scheduleTask.CalcDailyStartTimeUtc);

			// Set to an invalid date time
			scheduleTask.CalcDailyStartTimeLocal = ZDateTime.Invalid;
			AssertEquals("S5_DailyStartTime", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)), scheduleTask.S5_DailyStartTime);
			AssertEquals("CalcDailyStartTimeLocal", expectedCalcDailyStartTimeLocal, scheduleTask.CalcDailyStartTimeLocal);

			ZDateTime expectedS5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromMinutes(140)).Add(-localUserOffset).Add(localScheduleTaskOffset).TimeOfDay
				);

			// Set to a valid one
			scheduleTask.CalcDailyStartTimeLocal = new ZDateTime(2010, 4, 20, 2, 20, 0);
			AssertEquals("CalcDailyStartTimeLocal", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromMinutes(140)), scheduleTask.CalcDailyStartTimeLocal);
			AssertEquals("S5_DailyStartTime", expectedS5_DailyStartTime, scheduleTask.S5_DailyStartTime);
		}

		[TestDate(2016, 4, 29, 0, 0, 0)]
		public void TestCalcDailyEndTimeLocal()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";
			scheduleTask.S5_GB = branch.PK;

			scheduleTask.S5_GB = Env.CurrentBranchPK;

			DateTime utcNow = DateTime.UtcNow;

			TimeSpan localUserOffset = Env.Time.GetUtcOffsetBasedOnUtc(utcNow);
			TimeSpan localScheduleTaskOffset = Env.Time.GetUtcOffsetBasedOnUtc("USFDW", utcNow);

			AssertEquals("[PRE-CONDITION] S5_DailyEndTime valid?", false, scheduleTask.S5_DailyEndTime.IsValid);
			AssertEquals("[PRE-CONDITION] CalcDailyEndTimeLocal valid?", false, scheduleTask.CalcDailyEndTimeLocal.IsValid);

			ZDateTime expectedCalcDailyEndTimeLocal = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)).Add(-localScheduleTaskOffset).Add(localUserOffset).TimeOfDay
				);

			ZDateTime expectedCalcDailyEndTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)).Add(-localUserOffset).TimeOfDay
				);

			scheduleTask.S5_DailyEndTime = new ZDateTime(2010, 4, 22, 10, 0, 0);
			AssertEquals("S5_DailyEndTime", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)), scheduleTask.S5_DailyEndTime);
			//S5_DailyEndTime is local to the schedule task branch. CalcDailyEndTimeLocal is local to the logged in user branch.
			AssertEquals("CalcDailyEndTimeLocal", expectedCalcDailyEndTimeLocal, scheduleTask.CalcDailyEndTimeLocal);
			AssertEquals("CalcDailyEndTimeUtc", expectedCalcDailyEndTimeUtc, scheduleTask.CalcDailyEndTimeUtc);

			// Set to an invalid date time
			scheduleTask.CalcDailyEndTimeLocal = ZDateTime.Invalid;
			AssertEquals("S5_DailyEndTime", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(10)), scheduleTask.S5_DailyEndTime);
			AssertEquals("CalcDailyEndTimeLocal", expectedCalcDailyEndTimeLocal, scheduleTask.CalcDailyEndTimeLocal);

			ZDateTime expectedS5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.Add(
				ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromMinutes(140)).Add(-localUserOffset).Add(localScheduleTaskOffset).TimeOfDay
				);

			// Set to a valid one
			scheduleTask.CalcDailyEndTimeLocal = new ZDateTime(2010, 4, 20, 2, 20, 0);
			AssertEquals("CalcDailyEndTimeLocal", ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromMinutes(140)), scheduleTask.CalcDailyEndTimeLocal);
			AssertEquals("S5_DailyEndTime", expectedS5_DailyEndTime, scheduleTask.S5_DailyEndTime);
		}

		public void TestUtcOffsetOverride()
		{
			var task = Factory.NewWithValidTestData<StmScheduleTask>();
			AssertEquals("Should be null by default", null, task.UtcOffsetOverride);
		}

		public void TestCalcDailyStartTimeUtc_WithUtcOffsetOverriden()
		{
			var task = Factory.NewWithValidTestData<StmScheduleTaskWithUtcOffset>();
			task.SetUtcOffsetOverrideForTesting(new TimeSpan(6, 0, 0));

			task.CalcDailyStartTimeUtc = new ZDateTime(1900, 1, 1, 10, 0, 0);
			AssertEquals(new ZDateTime(1900, 1, 1, 16, 0, 0), task.S5_DailyStartTime);

			task.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 20, 0, 0);
			AssertEquals(new ZDateTime(1900, 1, 1, 14, 0, 0), task.CalcDailyStartTimeUtc);
		}

		public void TestCalcDailyStartTimeLocal_WithUtcOffsetOverriden()
		{
			var task = Factory.NewWithValidTestData<StmScheduleTaskWithUtcOffset>();
			task.SetUtcOffsetOverrideForTesting(new TimeSpan(6, 0, 0));

			task.CalcDailyStartTimeLocal = new ZDateTime(1900, 1, 1, 10, 0, 0);
			AssertEquals(new ZDateTime(1900, 1, 1, 6, 0, 0), task.S5_DailyStartTime);

			task.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 20, 0, 0);
			AssertEquals(new ZDateTime(1900, 1, 1, 0, 0, 0), task.CalcDailyStartTimeLocal);
		}

		#endregion

		#region Start / Next Date Calculation

		[TestTimeZoneUNLOCO("USDFW"), SnailTest]
		[TestDate]
		public void TestNextRunTime_vs_DSTEndingTheSameDay()
		{
			// http://www.timeanddate.com/worldclock/clockchange.html?n=64&year=2013

			//1) set unit test time to 2nd November 12:00 pm in the time zone CDT UTC-5h (= 02-Nov-17:00 pm UTC)
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2013, 11, 2, 17, 0, 0);

			//2) Create an StmScheduleTask with Next Run Time 03-Nov-00:00 in the time zone CDT UTC-5h (= 03-Nov-05:00 UTC)
			//... recurrence of daily, with S5_DailyStartTime of local 00:00 in the time zone CDT UTC-5h (= 05:00 UTC)
			//... S5_StartDate way in the past

			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2013, 11, 3, 5, 0, 0); //midnight of the 3rd in local time CDT, 5 pm in UTC)
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 0, 0, 0); //midnight in local time CST, 6 AM in UTC. notice that this is for when daylight savings is OVER
			scheduleTask.S5_StartDate = new ZDateTime(2011, 11, 1);
			scheduleTask.S5_IsActive = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "USDFW";
			branch.GB_GC = company.PK;
			scheduleTask.S5_GB = branch.PK;

			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			Factory.Save();

			//3) Advance unit test time to 3rd November 00:00 local. The time zone should still be CDT UTC-5h, but 2 hours from now...
			//...the clock will be rolled back to a second 1 am, and it will be CST UTC-6h.

			TestDateAttribute.Date = new DateTime(2013, 11, 3, 5, 4, 0); //4min past midnight of the 3rd in local time CDT, 5 pm in UTC)

			//4) Have the StmScheduleTask calculate the next run time. If it is in the past, fail unit test.
			scheduleTask.S5_ScheduleActualRunCount = 1;
			Factory.Save(); //UpdateNextScheduledDate() should have been called

			AssertEquals(String.Format("scheduleTask.S5_NextScheduledPrintRunTimeUtc {0} is in the future", scheduleTask.S5_NextScheduledPrintRunTimeUtc), 4, scheduleTask.S5_NextScheduledPrintRunTimeUtc.Day);
		}

		[TestDate]
		public void TestNextRunTime_vs_DSTStartingTheSameDay()
		{
			// http://www.timeanddate.com/worldclock/clockchange.html?n=64&year=2013

			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2013, 3, 9, 18, 0, 0);

			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2013, 3, 10, 5, 0, 0);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 23, 0, 0);
			scheduleTask.S5_StartDate = new ZDateTime(2011, 11, 1);
			scheduleTask.S5_IsActive = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "USDFW";
			branch.GB_GC = company.PK;
			scheduleTask.S5_GB = branch.PK;

			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 3, 10, 5, 4, 0);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				scheduleTask.S5_ScheduleActualRunCount = 1;
				Factory.Save(); //UpdateNextScheduledDate() should have been called
			}

			AssertEquals(String.Format("scheduleTask.S5_NextScheduledPrintRunTimeUtc {0} is correctly one day ahead", scheduleTask.S5_NextScheduledPrintRunTimeUtc), 11, scheduleTask.S5_NextScheduledPrintRunTimeUtc.Day);
		}

		[TestDate(2010, 8, 25, 0, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCalcNextRunTimeLocalInUTC10TimeZoneAndRecurringStartTimeLocal()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			scheduleTask.Recurrence.WeeklyRange = ZBool.True;
			scheduleTask.Recurrence.Friday = ZBool.True;
			scheduleTask.S5_StartDate = ZDateTime.Now;
			scheduleTask.Recurrence.RecurringStartTimeLocal = new ZDateTime(2010, 8, 25, 6, 0, 0);

			Factory.Save();

			AssertEquals("scheduleTask.CalcNextRunTimeLocal", new ZDateTime(2010, 8, 27, 6, 0, 0), scheduleTask.CalcNextRunTimeLocal);
		}

		public void TestDeleteRecurringStartTimeLocal()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			scheduleTask.Recurrence.WeeklyRange = ZBool.True;
			scheduleTask.Recurrence.Friday = ZBool.True;
			scheduleTask.S5_StartDate = ZDateTime.Now;
			scheduleTask.Recurrence.RecurringStartTimeLocal = new ZDateTime(2010, 8, 25, 6, 0, 0);
			AssertNotEquals("scheduleTask.CalcNextRunTimeLocal", ZDateTime.Empty, scheduleTask.Recurrence.RecurringStartTimeLocal);

			scheduleTask.Recurrence.RecurringStartTimeLocal = ZDateTime.Empty;
			AssertEquals("scheduleTask.CalcNextRunTimeLocal", ZDateTime.Empty, scheduleTask.Recurrence.RecurringStartTimeLocal);
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST()
		{
			using (new TemporaryUserContext { BranchPK = TempBranch.PK.ToGuid() }.Set())
			{
				StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
				scheduleTask.S5_GB = TempBranch.PK;

				ZDateTime dailyStartTimeUtc = ZDateTime.UtcNow;
				dailyStartTimeUtc = dailyStartTimeUtc.AddSeconds(-dailyStartTimeUtc.Second);
				dailyStartTimeUtc = dailyStartTimeUtc.AddMilliseconds(-dailyStartTimeUtc.Millisecond);
				ZDateTime dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc.ToDateTime());

				scheduleTask.S5_StartDate = dailyStartTimeUtc;
				scheduleTask.S5_DailyStartTime = dailyStartTimeLocal;
				scheduleTask.S5_TaskPeriod = "D";
				scheduleTask.S5_TaskPeriodCount = 1;
				scheduleTask.S5_WeekDaysOnly = ZBool.False;
				scheduleTask.S5_IsPrivate = ZBool.False;
				scheduleTask.S5_IsActive = ZBool.True;
				scheduleTask.S5_GB = TempBranch.PK;

				Factory.Save();

				AssertEquals(dailyStartTimeUtc, scheduleTask.S5_StartDate);
				AssertEquals(dailyStartTimeLocal, scheduleTask.CalcNextRunTimeLocal);
				AssertEquals(Env.Time.GetUtcFromLocalTime(dailyStartTimeLocal.ToDateTime()), scheduleTask.S5_NextScheduledPrintRunTimeUtc);

				scheduleTask.Run();

				//AssertEquals(dailyStartTimeLocal.AddDays(1), scheduleTask.CalcNextRunTimeLocal); //Could be a DST change!
				AssertEquals(Env.Time.GetUtcFromLocalTime(dailyStartTimeLocal.AddDays(1).ToDateTime()), scheduleTask.S5_NextScheduledPrintRunTimeUtc);

				scheduleTask.Run();

				//AssertEquals(dailyStartTimeLocal.AddDays(2), scheduleTask.CalcNextRunTimeLocal); //Could be a DST change!
				AssertEquals(Env.Time.GetUtcFromLocalTime(dailyStartTimeLocal.AddDays(2).ToDateTime()), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			}
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void TestCalcNextRunTimeLocal_InUSERI_LocaleWithDSTRealTime()
		{
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestCalcNextRunTimeLocal_InUSERI_LocaleWithDST()
		{
			// set unit test time to 1-November 2020 05:30 in USERI time zone (= 01-Nov 01:30 UTC) - DST Start at 2:00
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2020, 11, 01, 05, 30, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 5-November 2016 11:30 in USERI time zone, which could fail the test
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2016, 11, 5, 11, 30, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 29-November 19:00 in USERI time zone (= 30-Nov 00:00 UTC)
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2014, 11, 30, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 30-November 19:00 in USERI time zone (= 01-Dec 00:00 UTC)
			TestDateAttribute.Date = new DateTime(2014, 12, 1, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 01-December 19:00 in USERI time zone (= 02-Dec 00:00 UTC)
			TestDateAttribute.Date = new DateTime(2014, 12, 2, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 30-December 19:00 in USERI time zone (= 31-Dec 00:00 UTC)
			TestDateAttribute.Date = new DateTime(2014, 12, 31, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 31-December 19:00 in USERI time zone (= 01-Jan 00:00 UTC)
			TestDateAttribute.Date = new DateTime(2015, 1, 1, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();

			// set unit test time to 01-January 19:00 in USERI time zone (= 02-Jan 00:00 UTC)
			TestDateAttribute.Date = new DateTime(2015, 1, 2, 0, 0, 0);
			AssertCalcNextRunTimeLocal_InUSERI_LocaleWithDST();
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void TestCalcNextRunTimeLocal_InUSERI_LocaleWithDSTCrossDST_Days()
		{
			using (new TemporaryUserContext { BranchPK = TempBranch.PK.ToGuid() }.Set())
			{
				StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

				ZDateTime beginningOfNextYearUtc = Env.Time.GetUtcFromLocalTime(new ZDateTime(ZDateTime.Today.Year + 1, 1, 1).ToDateTime());
				ZDateTime dailyStartTimeUtc = beginningOfNextYearUtc;
				ZDateTime dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc.ToDateTime());

				scheduleTask.S5_StartDate = beginningOfNextYearUtc;
				scheduleTask.S5_DailyStartTime = dailyStartTimeLocal;
				scheduleTask.S5_TaskPeriod = "D";
				scheduleTask.S5_TaskPeriodCount = 150;
				scheduleTask.S5_WeekDaysOnly = ZBool.False;
				scheduleTask.S5_IsPrivate = ZBool.False;
				scheduleTask.S5_IsActive = ZBool.True;
				scheduleTask.Recurrence.RecurringStartTimeLocal = dailyStartTimeLocal;
				scheduleTask.S5_GB = TempBranch.PK;

				Factory.Save();

				AssertEquals(beginningOfNextYearUtc, scheduleTask.S5_StartDate);
				AssertEquals(dailyStartTimeLocal, scheduleTask.CalcNextRunTimeLocal);
				AssertEquals(dailyStartTimeUtc, scheduleTask.S5_NextScheduledPrintRunTimeUtc);

				scheduleTask.Run();
				AssertEquals(dailyStartTimeLocal.AddDays(150), Env.Time.GetLocalTimeFromUtc(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToDateTime()));

				scheduleTask.Run();
				AssertEquals(dailyStartTimeLocal.AddDays(300), Env.Time.GetLocalTimeFromUtc(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToDateTime()));

				scheduleTask.Run();
				AssertEquals(dailyStartTimeLocal.AddDays(450), Env.Time.GetLocalTimeFromUtc(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToDateTime()));
			}
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void TestCalcNextRunTimeLocal_InUSERI_LocaleWithDSTCrossDST_Months()
		{
			using (new TemporaryUserContext { BranchPK = TempBranch.PK.ToGuid() }.Set())
			{
				StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

				ZDateTime utcToday = Env.Time.GetUtcFromLocalTime(new ZDateTime(ZDateTime.Today.Year + 1, 6, 1).ToDateTime());
				ZDateTime dailyStartTimeUtc = utcToday.Add(new ZDateTime(1900, 1, 1, 0, 0, 0).TimeOfDay);
				ZDateTime dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc.ToDateTime());

				scheduleTask.S5_StartDate = utcToday;
				scheduleTask.S5_DailyStartTime = dailyStartTimeLocal;
				scheduleTask.S5_TaskPeriod = "M";
				scheduleTask.S5_TaskPeriodCount = 6;
				scheduleTask.S5_WeekDaysOnly = ZBool.False;
				scheduleTask.S5_IsPrivate = ZBool.False;
				scheduleTask.S5_IsActive = ZBool.True;
				scheduleTask.Recurrence.RecurringStartTimeLocal = dailyStartTimeLocal;
				scheduleTask.S5_GB = TempBranch.PK;

				Factory.Save();

				AssertEquals(utcToday, scheduleTask.S5_StartDate);
				AssertEquals(dailyStartTimeLocal, scheduleTask.CalcNextRunTimeLocal);
				AssertEquals(dailyStartTimeUtc, scheduleTask.S5_NextScheduledPrintRunTimeUtc);

				scheduleTask.Run();

				AssertEquals("Should be 6 months later. DST taken into account and local time of day is the same", dailyStartTimeLocal.AddMonths(6), scheduleTask.CalcNextRunTimeLocal);

				scheduleTask.Run();

				AssertEquals(dailyStartTimeLocal.AddMonths(12), scheduleTask.CalcNextRunTimeLocal);
			}
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void TestCalcNextRunTimeLocal_InUSERI_LocaleWithDSTCrossDST_Months_LastDayOccurence()
		{
			using (new TemporaryUserContext { BranchPK = TempBranch.PK.ToGuid() }.Set())
			{
				StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

				ZDateTime utcToday = Env.Time.GetUtcFromLocalTime(new ZDateTime(ZDateTime.Today.Year + 1, 5, 1).ToDateTime());
				ZDateTime dailyStartTimeUtc = utcToday.Add(new ZDateTime(1900, 1, 1, 0, 0, 0).TimeOfDay);
				ZDateTime dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc.ToDateTime());

				scheduleTask.S5_StartDate = utcToday;
				scheduleTask.S5_DailyStartTime = dailyStartTimeLocal;
				scheduleTask.S5_DayNumber = 99;
				scheduleTask.S5_TaskPeriod = "M";
				scheduleTask.S5_TaskPeriodCount = 6;
				scheduleTask.S5_WeekDaysOnly = ZBool.False;
				scheduleTask.S5_IsPrivate = ZBool.False;
				scheduleTask.S5_IsActive = ZBool.True;
				scheduleTask.Recurrence.RecurringStartTimeLocal = dailyStartTimeLocal;
				scheduleTask.S5_GB = TempBranch.PK;

				Factory.Save();

				var expectedStartDate = new ZDateTime(utcToday.Year, utcToday.Month, 31, utcToday.Hour, utcToday.Minute, utcToday.Second);
				AssertEquals(expectedStartDate, scheduleTask.S5_StartDate);

				var expectedRunTimeLocal = new ZDateTime(dailyStartTimeLocal.Year, dailyStartTimeLocal.Month, 31, dailyStartTimeLocal.Hour, dailyStartTimeLocal.Minute, dailyStartTimeLocal.Second);
				AssertEquals(expectedRunTimeLocal, scheduleTask.CalcNextRunTimeLocal);

				scheduleTask.Run();

				AssertEquals("Should be 6 months later. DST taken into account and local time of day is the same", expectedRunTimeLocal.AddMonths(6), scheduleTask.CalcNextRunTimeLocal);

				scheduleTask.Run();

				AssertEquals(expectedRunTimeLocal.AddMonths(12), scheduleTask.CalcNextRunTimeLocal);
			}
		}

		public void TestDatesAreNotChangedIfIsPrivate()
		{
			ZDateTime now = ZDateTime.Now;
			ScheduleTask.S5_IsPrivate = true;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = now;
			Factory.Save();
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", now, ScheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2010, 8, 25)]
		public void TestNextScheduledPrintRunTimeNOTinThePastIfIsPrivate()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_IsPrivate = true;

			DateTime scheduledStartTime = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day, 14, 40, 0);
			scheduleTask.S5_DailyStartTime = Env.Time.GetLocalTimeFromUtc(scheduledStartTime);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddYears(-1);

			Factory.Save();
			AssertEquals("S5_NextScheduledPrintRunTimeUtc", scheduledStartTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2006, 1, 1)]
		public void TestNextScheduledPrintRunTimeIsSetWithCorrectTimeForFirstRun()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			scheduleTask.S5_StartDate = ZDateTime.Today;
			DateTime scheduledStartTime = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day, 14, 40, 0);
			scheduleTask.S5_DailyStartTime = Env.Time.GetLocalTimeFromUtc(scheduledStartTime);

			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_WeekDaysOnly = ZBool.False;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();

			AssertEquals("ScheduleTask.S5_StartDate must be today", ZDateTime.Today, scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today", scheduledStartTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2006, 1, 1)]
		public void TestNextScheduledPrintRunTimeIsNotInPastWhenStartDateIsInPast()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();

			scheduleTask.S5_StartDate = ZDateTime.Today.AddYears(-1);
			DateTime scheduledStartTime = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day, 14, 40, 0);
			scheduleTask.S5_DailyStartTime = Env.Time.GetLocalTimeFromUtc(scheduledStartTime);

			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_WeekDaysOnly = ZBool.False;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();

			AssertEquals("ScheduleTask.S5_StartDate must be one year ago", ZDateTime.Today.AddYears(-1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today", scheduledStartTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2006, 1, 1)]
		public void TestDailyRecurrence()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_WeekDaysOnly = ZBool.False;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be today", ZDateTime.Today, scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today", ZDateTime.Today, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be today", ZDateTime.Today, scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be tommorrow", ZDateTime.Today.AddDays(1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.S5_WeekDaysOnly = ZBool.False;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be today", ZDateTime.Today, scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today", ZDateTime.Today, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be today", ZDateTime.Today, scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today +2 days", ZDateTime.Today.AddDays(2), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.S5_WeekDaysOnly = ZBool.True;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be work day", new ZDateTime(2006, 1, 2), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be work day", new ZDateTime(2006, 1, 2), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be work day", new ZDateTime(2006, 1, 2), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be next work day", new ZDateTime(2006, 1, 3), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();
		}

		[TestDate(2006, 1, 1)]
		public void TestWeeklyRecurrence()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "W";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_DayList = "YNNNNNN"; //each sunday
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be sunday", new ZDateTime(2006, 1, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc be sunday", new ZDateTime(2006, 1, 1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be sunday", new ZDateTime(2006, 1, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be next sunday", new ZDateTime(2006, 1, 8), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "W";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_DayList = "YNNYNNN"; //each sunday and wednesday
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be sunday or wednesday", new ZDateTime(2006, 1, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be sunday or wednesday", new ZDateTime(2006, 1, 1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be sunday or wednesday", new ZDateTime(2006, 1, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be next sunday or wednesday", new ZDateTime(2006, 1, 4), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "W";
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.S5_DayList = "NYNNYNN"; //each monday and thursday
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be first monday (2006-1-2)", new ZDateTime(2006, 1, 2), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be monday", new ZDateTime(2006, 1, 2), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask S5_StartDate must be first monday", new ZDateTime(2006, 1, 2), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be Thursday", new ZDateTime(2006, 1, 5), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask S5_StartDate must be first monday", new ZDateTime(2006, 1, 2), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be Monday in 2 weeks", new ZDateTime(2006, 1, 16), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.S5_DayList = "NNNNNYN";
			scheduleTask.S5_TaskPeriodCount = 1;
			Factory.Save();
			AssertEquals("ScheduleTask S5_StartDate must be first thursday after prev Start date", new ZDateTime(2006, 1, 6), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be first thursday after prev Start date", new ZDateTime(2006, 1, 6), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask S5_StartDate must be first thursday after prev Start date", new ZDateTime(2006, 1, 6), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be next thursday after prev Start date", new ZDateTime(2006, 1, 13), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 3", 3, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();
		}

		[TestDate(2006, 2, 1)]
		public void TestAccountingRecurrence()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			CreateAccPeriodTestData();
			Factory.Save();

			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.Recurrence.DayOfAccountingPeriod = 20;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 20th day of the nearest accounting period but more then StartDate", new ZDateTime(2006, 4, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 4, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2006, 4, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 20th day of next accounting period", new ZDateTime(2006, 7, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.Recurrence.DayOfAccountingPeriod = 40;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 40th day of the nearest accounting period but more then StartDate", new ZDateTime(2006, 2, 9), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 2, 9), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2006, 2, 9), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 40th day in 2 accounting periods ", new ZDateTime(2006, 8, 9), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_DayNumber = 99; //Last day
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be last day of Start Date accounting period", new ZDateTime(2006, 3, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 3, 31), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 3, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be last day of next accounting period", new ZDateTime(2006, 6, 30), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 3, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be last day of next accounting period", new ZDateTime(2006, 9, 30), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.S5_DayNumber = 4;
			scheduleTask.S5_WeekDayOccurrenceNumber = 2;
			Factory.Save();
			AssertEquals("ScheduleTask S5_StartDate must be second wednesday of accounting period after Start date", new ZDateTime(2006, 4, 12), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 4, 12), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 4, 12), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be second wednesday of next accounting period", new ZDateTime(2006, 7, 12), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 3", 3, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();
		}

		[TestDate(2007, 11, 1)]
		public void TestAccountingRecurrenceWhenNoFutureAccountingPeriod()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			CreateAccPeriodTestData();

			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "alexander.korotun2@cargowise.com";
			staff.GS_Code = "ZAC";
			Factory.Save();

			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.Recurrence.DayOfAccountingPeriod = 20;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();

			AssertEquals("precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEquals("ScheduleTask.S5_StartDate must be the 20th day of the nearest accounting period but more then StartDate", new ZDateTime(2007, 11, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2007, 11, 1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2007, 11, 1), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must remain same since there is no next accounting period", new ZDateTime(2007, 11, 1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);

			AssertEquals("ScheduleTask.S5_IsActive should be set to false when there is no next accounting period, this is to prevent executing of this scheduled task during each BatchProcessor loop because of unchanged S5_NextScheduledPrintRunTimeUtc", false, scheduleTask.S5_IsActive);
			AssertEquals("Notification email to PostMasters has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef emailDef = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email subject", $"A scheduled job has been disabled by {Core.Constants.ProductName}", emailDef.Subject);
			AssertEquals("Email body", GetExpected_NoNextAccPeriodNotificationEmail(scheduleTask.S5_ScheduleDescription), emailDef.Body);

			scheduleTask.Delete();
		}

		[TestDate(2006, 1, 1)]
		public void TestMonthlyRecurrence()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "M";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.Recurrence.DayOfMonth = 20;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 20th day of the nearest month but more then StartDate", new ZDateTime(2006, 1, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 1, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2006, 1, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 20th day of next month", new ZDateTime(2006, 2, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "M";
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.Recurrence.DayOfMonth = 15;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 15th day of the nearest month but more then StartDate", new ZDateTime(2006, 1, 15), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 1, 15), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2006, 1, 15), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 15th day in 2 month ", new ZDateTime(2006, 3, 15), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "M";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_DayNumber = 99; //Last day
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;

			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be last day of Start Date month", new ZDateTime(2006, 1, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 1, 31), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);

			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 1, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be last day of next month", new ZDateTime(2006, 2, 28), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);

			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 1, 31), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be last day of next month", new ZDateTime(2006, 3, 31), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);

			scheduleTask.S5_DayNumber = 4;
			scheduleTask.S5_WeekDayOccurrenceNumber = 2;
			scheduleTask.S5_StartDate = new ZDateTime(2006, 1, 22);
			Factory.Save();
			AssertEquals("ScheduleTask S5_StartDate must be second wednesday of month after Start date", new ZDateTime(2006, 2, 8), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 2, 8), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);

			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 2, 8), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be second wednesday of next month", new ZDateTime(2006, 3, 8), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 3", 3, scheduleTask.S5_ScheduleActualRunCount);
		}

		[TestDate(2006, 1, 1)]
		public void TestYearlyRecurrence()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "Y";
			scheduleTask.S5_TaskPeriodCount = 3;
			scheduleTask.S5_MonthNumber = 3;
			scheduleTask.Recurrence.DayOfMonth = 20;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 20th day of March but more then StartDate", new ZDateTime(2006, 3, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 3, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2006, 3, 20), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 20th day of next month", new ZDateTime(2007, 3, 20), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "Y";
			scheduleTask.S5_TaskPeriodCount = 2;
			scheduleTask.S5_StartDate = new ZDateTime(2003, 5, 5);
			scheduleTask.S5_MonthNumber = 2;
			scheduleTask.Recurrence.DayOfMonth = 29;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be the 29th day of February but more then StartDate", new ZDateTime(2004, 2, 29), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be today if S5_StartDate is in the past", ZDateTime.Today, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must not be changed", new ZDateTime(2004, 2, 29), scheduleTask.S5_StartDate);
			scheduleTask.S5_StartDate = new ZDateTime(2008, 2, 29);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be 28 of February of next month (because is not leap year)", new ZDateTime(2009, 2, 28), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();

			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			//ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Today;
			scheduleTask.S5_TaskPeriod = "Y";
			scheduleTask.S5_TaskPeriodCount = 1;
			scheduleTask.S5_DayNumber = 2;
			scheduleTask.S5_WeekDayOccurrenceNumber = 1;
			scheduleTask.S5_MonthNumber = 4;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			Factory.Save();
			AssertEquals("ScheduleTask.S5_StartDate must be first monday of April", new ZDateTime(2006, 4, 3), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2006, 4, 3), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 0", 0, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 4, 3), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be first monday of April in next Year", new ZDateTime(2007, 4, 2), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 1", 1, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2006, 4, 3), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be first monday of April in next Year", new ZDateTime(2008, 4, 7), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.S5_DayNumber = 4;
			scheduleTask.S5_WeekDayOccurrenceNumber = 2;
			scheduleTask.S5_MonthNumber = 3;
			scheduleTask.S5_StartDate = new ZDateTime(2007, 1, 22);
			Factory.Save();
			AssertEquals("ScheduleTask S5_StartDate must be second wednesday of March after Start date", new ZDateTime(2007, 3, 14), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be S5_StartDate", new ZDateTime(2007, 3, 14), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 2", 2, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Run();
			AssertEquals("ScheduleTask.S5_StartDate must be not be changed", new ZDateTime(2007, 3, 14), scheduleTask.S5_StartDate);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc must be second wednesday of March of next year", new ZDateTime(2008, 3, 12), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_ScheduleActualRunCount must be 3", 3, scheduleTask.S5_ScheduleActualRunCount);
			scheduleTask.Delete();
		}

		void CreateAccPeriodTestData()
		{
			ZGuid companyPK = GlbCompany.CurrentCompany.PK;
			AccPeriodManagement accPeriod;
			AccPeriodManagementCollection collection = new AccPeriodManagementCollection(Factory);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200601;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200604;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200607;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200610;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 12, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200701;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200704;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200707;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200710;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 12, 31);
		}

		string GetExpected_NoNextAccPeriodNotificationEmail(string scheduleDescription)
		{
			return "Scheduled Job '" + scheduleDescription + $"' has been set to Inactive by {Core.Constants.ProductName} running on database '" + Db.Connection.CurrentDatabase + "'. This job is setup to run per Accounting Period, but the current Accounting Period for your company ('" + Env.CurrentCompany.Code + @"') is the last period in the Accounting Year and the next Accounting Year has not been setup yet.

To re-enable this Scheduled Job:
1)  Setup your Accounting Periods for the next Accounting Year. You can do this by going into Manage > General Ledger > Period Management module and press Setup next accounting year.
2)  You can then re-enable this Scheduled Job by going into Maintain -> System -> Scheduled Reports module, use Description filter with value " + scheduleDescription + " to find this Scheduled Job and tick its Active flag.";
		}

		#endregion

		#region Cancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(StmScheduleTask)));
		}

		#endregion

		#region Run

		public void TestDummyTaskS5_DescriptionSerialisation()
		{
			Assert(!ScheduleTask.ThrowExceptionNotImplemented);
			Assert(!ScheduleTask.ThrowExceptionInvalidOp);
			Assert(!ScheduleTask.RunCalled);
			Assert(!ScheduleTask.NotifyScheduleRunError);
			AssertEquals(0, ScheduleTask.RunCount);

			ScheduleTask.ThrowExceptionInvalidOp = true;
			ScheduleTask.ThrowExceptionNotImplemented = true;
			ScheduleTask.RunCalled = true;
			ScheduleTask.RunCount = 8;
			ScheduleTask.NotifyScheduleRunError = true;

			Assert(ScheduleTask.ThrowExceptionNotImplemented);
			Assert(ScheduleTask.ThrowExceptionInvalidOp);
			Assert(ScheduleTask.RunCalled);
			Assert(ScheduleTask.NotifyScheduleRunError);
			AssertEquals(8, ScheduleTask.RunCount);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			DummyStmScheduleTask task2 = newFactory.Load<DummyStmScheduleTask>(ScheduleTask.PK);
			Assert("ThrowExceptionNotImplemented", task2.ThrowExceptionNotImplemented);
			Assert("ThrowExceptionInvalidOp", task2.ThrowExceptionInvalidOp);
			Assert("RunCalled", task2.RunCalled);
			Assert("NotifyScheduleRunError", task2.NotifyScheduleRunError);
			AssertEquals("RunCount", 8, task2.RunCount);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafeRetriesForCertainErrors()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.ThrowExceptionNotImplemented = true;
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			});

			ScheduleTask.Reload();
			AssertEquals("Should have retried 10 times ", 10, ScheduleTask.RunCount);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunInBackgroundDoesNotRetryForCertainErrors()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.ThrowExceptionInvalidOp = true;
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			try
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			}
			catch
			{
				ScheduleTask.Reload();
				AssertEquals("Should excuted only once before existing", 1, ScheduleTask.RunCount);
			}
		}

		[TestDate(2003, 10, 3, 10, 21, 40)]
		public void TestRunSendsErrorReport()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "gyv";
			newStaff.GS_EmailAddress = "staff@group.com";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				SetUpPostMasterGroup();

				ScheduleTask.NotifyScheduleRunError = true;
				ScheduleTask.Run();
			}

			AssertEquals("One error email should be created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email.Recipients.Count", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("Email.Recipients[0]", "staff@group.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
			AssertEquals("Email.Subject ( note: it's weird because we're using the description to do secret testing stuff )", "One or more errors were encountered for Scheduled Task - 0=True $4=True $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $ run on 03-Oct-03 10:21:40", Env.OutgoingMailManager.EmailsCreated[0].Subject);

			StringBuilder resultBuilder = new StringBuilder();
			resultBuilder.Append(": <br>");
			resultBuilder.Append("Machine Name : ");
			resultBuilder.Append(System.Environment.MachineName + "<br>");

			resultBuilder.Append(@"Database Server \ Instance : ");
			resultBuilder.Append(Db.Connection.ServerName + "<br>");

			resultBuilder.Append("Database Name : ");
			resultBuilder.Append(Db.Connection.CurrentDatabase + "<br>");

			resultBuilder.Append(Env.CurrentCompany.GetSummaryAndRegistrationText().Replace(System.Environment.NewLine, "<br>"));

			AssertEquals("Email content type", EmailContentTypes.HTML, Env.OutgoingMailManager.EmailsCreated[0].ContentType);
			Assert("Email.Body", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(resultBuilder.ToString()));
			AssertContains("Error message should keep format in email", "<pre>An error occurred while processing the task\r\n</pre>", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestRunSendsErrorReportWhenLastEditingUserHasNoEmailAddress()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "gyv";
			newStaff.GS_EmailAddress = "";

			ScheduleTask.S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			ScheduleTask.S5_ParentID = Guid.NewGuid();
			ScheduleTask.S5_SystemLastEditUser = newStaff.GS_Code;
			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				ScheduleTask.NotifyScheduleRunError = true;
				var notifications = new NotificationBuffer();
				ScheduleTask.Run(notifications);
				AssertEquals("An error occurred while processing the task\r\nError notification email could not be sent. Please make sure that the group specified in the Notification > Company Notification Group Registry setting has a member with an email address on their Staff profile, or the last editing user has an email address.", notifications.AsString.Trim());
			}
		}

		public void TestHandlingOfWarnings()
		{
			SetUpPostMasterGroup();

			ScheduleTask.NotifyScheduleRunWarning = true;
			NotificationBuffer notifications = new NotificationBuffer();
			ScheduleTask.Run(notifications);

			AssertEquals("No emails.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No errors.", false, notifications.HasErrors);
			AssertEquals("Warnings present.", true, notifications.HasWarnings);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafe()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
				AssertEquals("Current user should be unchanged at the end of the report", "ZZ", Env.CurrentUser.Initials);
				AssertEquals("Service task should have been run under WebUser.", User.WebUserName, DummyStmScheduleTask.LastUserLoginName);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafe_CanRunInAnyBranchServiceTaskContext()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranch.PK));
			AssertNotNull(branch);

			ScheduleTask.S5_GB = branch.PK;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			var initialBranch = Env.CurrentBranch.PK;

			using (EnvProxy.Instance.TemporaryServiceTaskContext("SRR", canRunInAnyBranch: true))
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			}

			AssertEquals("Branch should be initial after completion", initialBranch, Env.CurrentBranch.PK);
			AssertEquals("Branch should be changed to ScheduleTask.S5_GB during processing", branch.PK, DummyStmScheduleTask.LastBranch);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafe_CanRunInAnyBranchServiceTaskContext_NullBranch()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranch.PK));
			AssertNotNull(branch);

			ScheduleTask.S5_GB = ZGuid.Empty;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			var initialBranch = Env.CurrentBranch.PK;

			using (EnvProxy.Instance.TemporaryServiceTaskContext("SRR", canRunInAnyBranch: true))
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			}

			AssertEquals("Branch should be initial after completion", initialBranch, Env.CurrentBranch.PK);
			AssertNotEquals("Branch should be set during processing", ZGuid.Empty, DummyStmScheduleTask.LastBranch);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafe_CanRunInAnyBranchServiceTaskContext_NullBranch_NoActiveCompanies()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranch.PK));
			AssertNotNull(branch);

			ScheduleTask.S5_GB = ZGuid.Empty;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ForEach(c => c.GC_IsActive = false);
			Factory.Save();

			var initialBranch = Env.CurrentBranch.PK;

			using (EnvProxy.Instance.TemporaryServiceTaskContext("SRR", canRunInAnyBranch: true))
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			}

			AssertEquals("Branch should be initial after completion", initialBranch, Env.CurrentBranch.PK);
			AssertNotEquals("Branch should be set during processing", ZGuid.Empty, DummyStmScheduleTask.LastBranch);

			AssertEquals("Should have reported a silent error.", true, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
			ErrorReporter.Clear();
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafeWithErrorResult()
		{
			SetUpPostMasterGroup();

			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.NotifyScheduleRunError = true;
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			var notifications = new NotificationBuffer();

			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasErrors);
			var expectedNotification = @"An error occurred while processing the task
Error notification email was sent to the following email address.
staff@group.com";
			AssertEquals(expectedNotification, notifications.AsString.Trim());
		}

		[TestDate(2006, 1, 1)]
		public void TestRunInBackgroundWithWarningResult()
		{
			SetUpPostMasterGroup();

			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.NotifyScheduleRunWarning = true;
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			var notifications = new NotificationBuffer();
			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasWarnings);
			AssertEquals("Here is a warning", notifications.AsString.Trim());
		}

		[TestDate(2006, 1, 1)]
		[ExpectException(typeof(InvalidOperationException))]
		public void TestRunSafe_WithException()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.ThrowExceptionInvalidOp = true;
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			try
			{
				ScheduleTask.RunSafe(new NotificationBuffer());
			}
			finally
			{
				AssertEquals("Task is no longer active", false, ScheduleTask.S5_IsActive);
				AssertEquals("Task is saved", false, ScheduleTask.HasChanges);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestRunSafeSwitchesCompany()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranch.PK));
			AssertNotNull(branch);
			ScheduleTask.S5_GB = branch.PK;
			Factory.Save();

			Guid initialBranch = Env.CurrentBranch.PK;

			ScheduleTask.RunSafe(new NotificationBuffer());

			AssertEquals("Branch should be initial after completion", initialBranch, Env.CurrentBranch.PK);
			AssertEquals("Branch should be changed to ScheduleTask.S5_GB during processing", branch.PK, DummyStmScheduleTask.LastBranch);
		}

		[ExpectNoExceptions]
		public void TestRunAfterDeletingTask_ShouldNotThrow()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			newFactory.Load<StmScheduleTask>(ScheduleTask.PK).Delete();
			newFactory.Save();

			var notifications = new NotificationBuffer();
			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasErrors);
		}

		#region Test Send Error Notification Email

		public void TestRunSafeWithSendErrorNotificationEmailToGroupRole()
		{
			SetUpNotificationGroup(true);
			RunSafeWithNotifyScheduleRunError();

			var notifications = new NotificationBuffer();

			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasErrors);
			var expectedNotification = @"An error occurred while processing the task
Error notification email was sent to the following email address.
groupStaff@user.com";
			AssertEquals(expectedNotification, notifications.AsString.Trim());
			AssertErrorNotificationEmail("groupStaff@user.com");
		}

		public void TestRunSafeWithSendErrorNotificationEmailToStaffRole()
		{
			SetUpNotificationStaffRole("JTR");
			RunSafeWithNotifyScheduleRunError();

			var notifications = new NotificationBuffer();
			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasErrors);
			var expectedNotification = @"An error occurred while processing the task
Error notification email was sent to the following email address.
roleStaff@user.com";
			AssertEquals(expectedNotification, notifications.AsString.Trim());
			AssertErrorNotificationEmail("roleStaff@user.com");
		}

		public void TestRunSafeWithSendErrorNotificationEmailToDefaultStaffUserIfNotificationGroupHasNoStaffUser()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "CSF";
			newStaff.GS_EmailAddress = "currentStaff@user.com";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				SetUpNotificationGroup(false);
				RunSafeWithNotifyScheduleRunError();

				var notifications = new NotificationBuffer();
				ScheduleTask.RunSafe(notifications);
				Assert(notifications.HasErrors);
				var expectedNotification = @"An error occurred while processing the task
Error notification email was sent to the following email address.
currentStaff@user.com";
				AssertEquals(expectedNotification, notifications.AsString.Trim());
				AssertErrorNotificationEmail("currentStaff@user.com");
			}
		}

		public void TestRunSafeWithSendErrorNotificationEmailToGroupRoleIfNotificationStaffRoleHasNoStaffUser()
		{
			SetUpNotificationStaffRole("SAL");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "groupStaff@user.com";
			staff.GS_LoginName = "JerryTest";
			staff.GS_Code = "GJY";
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			RunSafeWithNotifyScheduleRunError();

			var notifications = new NotificationBuffer();
			ScheduleTask.RunSafe(notifications);
			Assert(notifications.HasErrors);
			var expectedNotification = @"An error occurred while processing the task
Error notification email was sent to the following email address.
groupStaff@user.com";
			AssertEquals(expectedNotification, notifications.AsString.Trim());
			AssertErrorNotificationEmail("groupStaff@user.com");
		}

		void RunSafeWithNotifyScheduleRunError()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			ScheduleTask.NotifyScheduleRunError = true;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();
		}

		void SetUpNotificationGroup(bool shouldAddUserToGroup)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			if (shouldAddUserToGroup)
			{
				var staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "groupStaff@user.com";
				staff.GS_LoginName = "JerryTest";
				staff.GS_Code = "JYT";
			}
			Factory.Save();

			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP);
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void SetUpNotificationStaffRole(string roleCode)
		{
			var staffRoles = new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = true, Code = "JTR" },
			};

			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staffRoles);
			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "RSF";
			newStaff.GS_EmailAddress = "roleStaff@user.com";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();

			var staffAssignments = orgHeader.StaffAssignments.AddNew();
			staffAssignments.O8_Role = roleCode;
			staffAssignments.O8_GS_NKPersonResponsible = newStaff.GS_Code;

			ScheduleTask.Recipients.RemoveAndDeleteAll();
			var recipient = ScheduleTask.Recipients.AddNew();
			recipient.S6_OH = orgHeader.PK;
			recipient.S6_OC = contact.PK;
		}

		void AssertErrorNotificationEmail(string exceptedEmailAddress)
		{
			AssertEquals("One error email should be created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email.Recipients.Count", 1, createdEmail.Recipients.Count);
			AssertEquals("Email.Recipients[0]", exceptedEmailAddress, createdEmail.Recipients[0]);
			AssertContains("One or more errors were encountered for Scheduled Task", createdEmail.Subject);

			var resultBuilder = new StringBuilder();
			resultBuilder.Append(": <br>");
			resultBuilder.Append("Machine Name : ");
			resultBuilder.Append(System.Environment.MachineName + "<br>");

			resultBuilder.Append(@"Database Server \ Instance : ");
			resultBuilder.Append(Db.Connection.ServerName + "<br>");

			resultBuilder.Append("Database Name : ");
			resultBuilder.Append(Db.Connection.CurrentDatabase + "<br>");

			resultBuilder.Append(Env.CurrentCompany.GetSummaryAndRegistrationText().Replace(System.Environment.NewLine, "<br>"));

			AssertEquals("Email content type", EmailContentTypes.HTML, createdEmail.ContentType);
			Assert("Email.Body", createdEmail.Body.Contains(resultBuilder.ToString()));
			AssertContains("Error message should keep format in email", "<pre>An error occurred while processing the task\r\n</pre>", createdEmail.Body);
		}

		#endregion

		#endregion

		#region TestClone

		public void TestClone()
		{
			StmScheduleTask scheduleTask;
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			ZDateTime testDateTime = new ZDateTime(2007, 5, 1);
			scheduleTask.S5_StartDate = testDateTime;
			scheduleTask.S5_ScheduleActualRunCount = 3;
			scheduleTask.S5_TaskPeriod = "D";
			scheduleTask.S5_WeekDaysOnly = ZBool.True;
			scheduleTask.S5_IsPrivate = ZBool.False;
			scheduleTask.S5_IsActive = ZBool.True;
			scheduleTask.Recipients.AddNew();
			Factory.Save();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2007, 5, 2);
			Factory.Save();
			StmScheduleTask clonedTask = (StmScheduleTask)scheduleTask.TemplateCopy();
			Factory.Save();
			AssertEquals(testDateTime, clonedTask.S5_StartDate);
			AssertNotEquals(ZDateTime.Today, clonedTask.S5_StartDate);
			AssertNotEquals(3, clonedTask.S5_ScheduleActualRunCount);
			AssertEquals(0, clonedTask.S5_ScheduleActualRunCount);
			AssertEquals("D", clonedTask.S5_TaskPeriod);
			AssertEquals(ZBool.True, clonedTask.S5_WeekDaysOnly);
			AssertEquals(ZBool.False, clonedTask.S5_IsPrivate);
			AssertEquals(ZBool.True, clonedTask.S5_IsActive);
			AssertEquals(scheduleTask.S5_NextScheduledPrintRunTimeUtc, clonedTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(1, clonedTask.Recipients.Count);
		}

		#endregion

		#region Test DST changes

		[TestUtcOffset(10, 0, 0)]
		[TestDate]
		public void TestCalculateNextRunTimeWithDSTChange1()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 0, 0, 0); // Midnight local time
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;

			AssertCalculateNextRunTimeWithDSTChange(
				scheduleTask,
				new DateTime(2013, 10, 1, 14, 0, 0), new DateTime(2013, 10, 10, 0, 0, 0),
				new DateTime(2013, 10, 4, 15, 0, 0), new TimeSpan(0, 10, 0, 0), new TimeSpan(0, 11, 0, 0), // 2013-10-5 1:00 local time clocks go 1 hour forward
				"1 14:00\r\n2 14:00\r\n3 14:00\r\n4 14:00\r\n5 13:00\r\n6 13:00\r\n7 13:00\r\n8 13:00\r\n9 13:00\r\n10 13:00");
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate]
		public void TestCalculateNextRunTimeWithDSTChange2()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 0, 0, 0); // Midnight local time
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;

			AssertCalculateNextRunTimeWithDSTChange(
				scheduleTask,
				new DateTime(2013, 10, 1, 13, 0, 0), new DateTime(2013, 10, 10, 0, 0, 0),
				new DateTime(2013, 10, 4, 15, 0, 0), new TimeSpan(0, 11, 0, 0), new TimeSpan(0, 10, 0, 0), // 2013-10-5 2:00 local time clocks go 1 hour back
				"1 13:00\r\n2 13:00\r\n3 13:00\r\n4 13:00\r\n5 14:00\r\n6 14:00\r\n7 14:00\r\n8 14:00\r\n9 14:00\r\n10 14:00");
		}

		void AssertCalculateNextRunTimeWithDSTChange(StmScheduleTask scheduleTask, DateTime utcFrom, DateTime utcTo, DateTime dstChangeUtcTime, TimeSpan initialUtcOffset, TimeSpan newUtcOffset, string expectedNextRunTimes)
		{
			Assert("Sanity", utcFrom < utcTo);

			var sb = new StringBuilder();

			TestDateAttribute.Date = utcFrom;

			TestUtcOffsetAttribute.Time = initialUtcOffset;
			TestUtcOffsetAttribute.IsDstEnabled = true;
			TestUtcOffsetAttribute.Time2 = newUtcOffset;
			TestUtcOffsetAttribute.DstChangeUtcDate = dstChangeUtcTime;

			scheduleTask.Factory.Save();
			sb.AppendLine(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToString("d HH:mm"));

			while (ZDateTime.UtcNow < utcTo)
			{
				if (scheduleTask.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow)
				{
					scheduleTask.UpdateNextScheduledDate();
					sb.AppendLine(scheduleTask.S5_NextScheduledPrintRunTimeUtc.ToString("d HH:mm"));
				}

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
			}

			AssertEquals(expectedNextRunTimes, sb.ToString().Trim());
		}

		#endregion

		#region Test Calculate Next Run Time With UtcOffsetOverride

		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestCalculateNextRunTimeWithUtcOverride()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTaskWithUtcOffset>();
			scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 8, 0, 0);
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;
			scheduleTask.SetUtcOffsetOverrideForTesting(TimeSpan.FromHours(5));

			scheduleTask.Factory.Save();
			AssertEquals(new ZDateTime(2016, 7, 10, 3, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			scheduleTask.UpdateNextScheduledDate();
			AssertEquals(new ZDateTime(2016, 7, 11, 3, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		#endregion

		public void TestPrintUserName()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "~test";
			staff.GS_Code = "~t";

			ScheduleTask.S5_GS_NKPrintUser = staff.GS_Code;
			AssertEquals("~test", ScheduleTask.PrintUserName);

			ScheduleTask.S5_GS_NKPrintUser = "~no";
			AssertNullOrEmpty(ScheduleTask.PrintUserName);
		}

		public void TestPriority()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			AssertEquals("Priority should be zero.", 0, scheduleTask.Priority);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEndDateLocalIsConsistentWithLocalTime()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_StartDate = new ZDateTime(2018, 1, 12);
			scheduleTask.S5_EndDate = new ZDateTime(2018, 1, 14, 13, 0, 0);
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;

			Factory.Save();

			AssertEquals("ScheduleTask CalEndDateLocal must be 15/01/2018 00:00:00", new ZDateTime(2018, 1, 15), scheduleTask.CalcEndDateLocal);
		}

		public void TestRunningReport()
		{
			AssertNull(ScheduleTask.RunningReport);

			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun.RRI_Status = Constants.StmReportRunState.Error;
			stmReportRun.RRI_S5_Schedule = ScheduleTask.PK;
			AssertNull(ScheduleTask.RunningReport);

			stmReportRun.RRI_Status = Constants.StmReportRunState.Running;
			AssertNotNull(ScheduleTask.RunningReport);
			AssertEquals(stmReportRun, ScheduleTask.RunningReport);
		}

		public void TestLastProcessedReport()
		{
			AssertNull(ScheduleTask.LastProcessedReport);

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_Status = Constants.StmReportRunState.Error;
			stmReportRun1.RRI_StartTimeUtc = new ZDateTime(2018, 03, 15);
			stmReportRun1.RRI_S5_Schedule = ScheduleTask.PK;
			AssertNull(ScheduleTask.LastProcessedReport);

			stmReportRun1.RRI_Status = Constants.StmReportRunState.Finished;
			AssertNotNull(ScheduleTask.LastProcessedReport);
			AssertEquals(stmReportRun1, ScheduleTask.LastProcessedReport);

			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_Status = Constants.StmReportRunState.Finished;
			stmReportRun2.RRI_StartTimeUtc = new ZDateTime(2018, 03, 16);
			stmReportRun2.RRI_S5_Schedule = ScheduleTask.PK;

			AssertEquals(stmReportRun2, ScheduleTask.LastProcessedReport);
			AssertNotEquals(stmReportRun1, ScheduleTask.LastProcessedReport);
		}

		#region Notifications

		[TestDate(2015, 12, 3, 15, 21, 30)]
		public void TestNotificationsLoggedImmediately()
		{
			var task = Factory.NewWithValidTestData<NotificationTaskForTest>();
			var notifications = new NotificationBufferForTest();
			task.Run(notifications);

			const string expectedLogs =
@"15:22 One
15:24 Two
15:27 Three
";

			AssertMultilineASCIIEquals("Notifications should be logged immediately", expectedLogs, notifications.ToString());
		}

		class NotificationTaskForTest : StmScheduleTask
		{
			public NotificationTaskForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void RunCore(INotifications notifications, CancellationToken token)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				notifications.Add(new InfoNotification("One"));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);
				notifications.Add(new InfoNotification("Two"));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
				notifications.Add(new InfoNotification("Three"));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(4);
			}
		}

		class NotificationBufferForTest : INotifications
		{
			public void Add(INotification notification)
			{
				sb.Append(ZDateTime.Now.ToShortTimeString()).Append(" ").AppendLine(notification.Message);
			}

			readonly StringBuilder sb = new StringBuilder();

			public override string ToString()
			{
				return sb.ToString();
			}
		}

		#endregion

		#region Implementation

		DummyStmScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.NewWithValidTestData<DummyStmScheduleTask>();
				}
				return scheduleTask;
			}
		}
		DummyStmScheduleTask scheduleTask;

		void SetUpPostMasterGroup()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "staff@group.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
		}

		void PrepareLogData()
		{
			ScheduleTask.S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			ScheduleTask.S5_ParentID = Guid.NewGuid();
			Factory.Save();

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "UserA";
			staff1.GS_Code = "GSA";
			staff1.GS_EmailAddress = "usera@hotmail.com";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "UserB";
			staff2.GS_Code = "GSB";
			staff2.GS_EmailAddress = "userb@hotmail.com";

			Factory.Save();
		}

		GlbBranch TempBranch
		{
			get
			{
				if (tempBranch == null)
				{
					SetUpBranch();
				}
				return tempBranch;
			}
		}
		GlbBranch tempBranch;

		void SetUpBranch()
		{
			tempBranch = Factory.New<GlbBranch>();
			tempBranch.GB_Code = "BZZ";
			tempBranch.GB_RL_NKHomePort = "USERI";
			tempBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}

		#endregion

		internal class StmScheduleTaskWithUtcOffset : StmScheduleTask
		{
			public StmScheduleTaskWithUtcOffset(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetUtcOffsetOverrideForTesting(TimeSpan? value)
			{
				utcOffsetOverrideForTesting = value;
			}
			TimeSpan? utcOffsetOverrideForTesting;

			public override TimeSpan? UtcOffsetOverride
			{
				get { return utcOffsetOverrideForTesting ?? base.UtcOffsetOverride; }
			}
		}
	}
}
