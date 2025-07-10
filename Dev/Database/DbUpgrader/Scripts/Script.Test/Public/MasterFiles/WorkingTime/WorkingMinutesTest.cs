using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(WorkingMinutes))]
	class BranchWorkingMinutesTest : DbCreateScriptTest
	{
		DateTime dateFrom;
		DateTime dateTo;
		int intervalWorkDays;
		Guid branch;
		Guid department;
		Guid holidaySource;
		Guid staff;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int MinutesPerDay = 24 * 60;

		protected override void SetUp()
		{
			base.SetUp();

			dateFrom = new DateTime(2017, 1, 2);
			dateTo = new DateTime(2017, 1, 6);
			intervalWorkDays = 4;
			branch = Guid.NewGuid();
			holidaySource = Guid.NewGuid();
			department = Guid.NewGuid();
			staff = Guid.NewGuid();
		}

		public void TestStaffEmploymentDateIsNull()
		{
			SetupStaff(staff, staffStartDate: null, staffDepartureDate: dateTo.AddYears(1));
			var expected = intervalWorkDays * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffDepartureDateIsNull()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddYears(-1), staffDepartureDate: null);
			var expected = intervalWorkDays * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffIsNull()
		{
			var expected = intervalWorkDays * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff: null);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateBeforeFromDate_AndDepartureDateBeforeFromDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddYears(-1), staffDepartureDate: dateFrom.AddYears(-1));
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(0, actual);
		}

		public void TestStaffEmploymentDateBeforeFromDate_AndDepartureDateEqualsFromDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddYears(-1), staffDepartureDate: dateFrom);
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(0, actual);
		}

		public void TestStaffEmploymentDateBeforeFromDate_AndDepartureDateBeforeToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddYears(-1), staffDepartureDate: dateTo.AddDays(-2));
			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateEqualsFromDate_AndDepartureDateBeforeToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom, staffDepartureDate: dateTo.AddDays(-2));
			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateAfterFromDate_AndDepartureDateBeforeToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddDays(1), staffDepartureDate: dateTo.AddDays(-2));
			var expected = (intervalWorkDays - 3) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateEqualsFromDate_AndDepartureDateEqualsToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			var expected = intervalWorkDays * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateAfterFromDate_AndDepartureDateEqualsToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddDays(2), staffDepartureDate: dateTo);
			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateAfterFromDate_AndDepartureDateAfterToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddDays(2), staffDepartureDate: dateTo.AddYears(1));
			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestStaffEmploymentDateEqualsToDate_AndDepartureDateAfterToDate()
		{
			SetupStaff(staff, staffStartDate: dateTo, staffDepartureDate: dateTo.AddYears(1));
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(0, actual);
		}

		public void TestStaffEmploymentDateAfterToDate_AndDepartureDateAfterToDate()
		{
			SetupStaff(staff, staffStartDate: dateTo.AddYears(1), staffDepartureDate: dateTo.AddYears(1));
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(0, actual);
		}

		public void TestStaffEmploymentDateBeforeFromDate_AndDepartureDateAfterToDate()
		{
			SetupStaff(staff, staffStartDate: dateFrom.AddYears(-1), staffDepartureDate: dateTo.AddYears(1));
			var expected = intervalWorkDays * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromHolidaySource_NoBranchHoliday()
		{
			SetupBranch(branch, "YYZ", "WTG", "AUS", "AUD");
			SetupStaff(staff, branch, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHolidaySource(holidaySource, "New South Wales", "NSW");
			SetupHoliday(holidaySource, "GHS", dateFrom);
			SetupHolidaySourceHistory(holidaySource, dateFrom, null);

			var expected = (intervalWorkDays - 1) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromHolidaySource_BranchHolidayExists()
		{
			SetupBranch(branch, "YYZ", "WTG", "AUS", "AUD");
			SetupStaff(staff, branch, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHolidaySource(holidaySource, "New South Wales", "NSW");
			SetupHoliday(holidaySource, "GHS", dateFrom);
			SetupHoliday(branch, "GB", dateFrom.AddDays(1));
			SetupHolidaySourceHistory(holidaySource, dateFrom, null);

			var expected = (intervalWorkDays - 1) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromHolidaySourceAndBranch()
		{
			SetupBranch(branch, "YYZ", "WTG", "AUS", "AUD");
			SetupStaff(staff, branch, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHolidaySource(holidaySource, "New South Wales", "NSW");
			SetupHoliday(holidaySource, "GHS", dateFrom.AddDays(3));
			SetupHoliday(branch, "GB", dateFrom.AddDays(1));
			SetupHolidaySourceHistory(holidaySource, dateFrom.AddDays(2), null);

			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromBranch_NoHolidaySource()
		{
			SetupBranch(branch, "YYZ", "WTG", "AUS", "AUD");
			SetupStaff(staff, branch, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHoliday(branch, "GB", dateFrom.AddDays(1));

			var expected = (intervalWorkDays - 1) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromHolidaySource_StaffChangesHolidaySourceBetweenHolidays()
		{
			SetupStaff(staff, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHolidaySource(holidaySource, "New South Wales", "NSW");
			SetupHoliday(holidaySource, "GHS", dateFrom);
			SetupHoliday(holidaySource, "GHS", dateFrom.AddDays(2));
			SetupHolidaySourceHistory(holidaySource, dateFrom, null);

			var newHolidaySource = Guid.NewGuid();
			SetupHolidaySource(newHolidaySource, "Victoria", "VIC");
			SetupHolidaySourceHistory(newHolidaySource, dateFrom.AddDays(1), null);

			var expected = (intervalWorkDays - 1) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		public void TestPublicHoliday_FromTwoHolidaySources_StaffChangesHolidaySourceBetweenHolidays()
		{
			SetupStaff(staff, staffStartDate: dateFrom, staffDepartureDate: dateTo);
			SetupHolidaySource(holidaySource, "New South Wales", "NSW");
			SetupHoliday(holidaySource, "GHS", dateFrom);
			SetupHolidaySourceHistory(holidaySource, dateFrom, null);

			var newHolidaySource = Guid.NewGuid();
			SetupHolidaySource(newHolidaySource, "Victoria", "VIC");
			SetupHoliday(newHolidaySource, "GHS", dateFrom.AddDays(2));
			SetupHolidaySourceHistory(newHolidaySource, dateFrom.AddDays(1), null);

			var expected = (intervalWorkDays - 2) * MinutesPerDay;
			var actual = GetWorkingMinutes(dateFrom, dateTo, branch, department, staff);
			AssertEquals(expected, actual);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int GetWorkingMinutes(DateTime dateFrom, DateTime dateTo, Guid branch, Guid department, Guid? staff)
		{
			using (var testCommand = TestConnection.Command("SELECT * FROM dbo.WorkingMinutes(@date_from_local, @date_to_local, @branch, @department, @staff)"))
			{
				testCommand.AddParameter("@date_from_local", SqlDbType.SmallDateTime, dateFrom);
				testCommand.AddParameter("@date_to_local", SqlDbType.SmallDateTime, dateTo);
				testCommand.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				testCommand.AddParameter("@department", SqlDbType.UniqueIdentifier, department);
				testCommand.AddParameter("@staff", SqlDbType.UniqueIdentifier, (object)staff ?? DBNull.Value);
				return Convert.ToInt32(testCommand.ExecuteScalar());
			}
		}

		void SetupStaff(Guid staff, DateTime? staffStartDate, DateTime? staffDepartureDate)
		{
			var personPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbStaffSchema.PK);
				command.ExecuteNonQuery();
			}

			using (var setupCommand = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_EmploymentDate, GS_DepartureDate, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
					VALUES (@staff, 'AAA', @employment_date, @departure_date, @personPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")))
			{
				setupCommand.AddParameter("@staff", SqlDbType.UniqueIdentifier, staff);
				setupCommand.AddParameter("@employment_date", SqlDbType.SmallDateTime, (object)staffStartDate ?? DBNull.Value);
				setupCommand.AddParameter("@departure_date", SqlDbType.SmallDateTime, (object)staffDepartureDate ?? DBNull.Value);
				setupCommand.AddParameter("@personPk", SqlDbType.UniqueIdentifier, personPk);
				setupCommand.ExecuteNonQuery();
			}
		}

		void SetupStaff(Guid staff, Guid branch, DateTime? staffStartDate, DateTime? staffDepartureDate)
		{
			var personPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbStaffSchema.PK);
				command.ExecuteNonQuery();
			}

			using (var setupCommand = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_GB_HomeBranch, GS_EmploymentDate, GS_DepartureDate, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
					VALUES (@staff, 'AAA', @branch, @employment_date, @departure_date, @personPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")))
			{
				setupCommand.AddParameter("@staff", SqlDbType.UniqueIdentifier, staff);
				setupCommand.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				setupCommand.AddParameter("@employment_date", SqlDbType.SmallDateTime, (object)staffStartDate ?? DBNull.Value);
				setupCommand.AddParameter("@departure_date", SqlDbType.SmallDateTime, (object)staffDepartureDate ?? DBNull.Value);
				setupCommand.AddParameter("@personPk", SqlDbType.UniqueIdentifier, personPk);
				setupCommand.ExecuteNonQuery();
			}
		}

		void SetupBranch(Guid branch, string branchCode, string companyCode, string countryCode, string currencyCode)
		{
			var company = Guid.NewGuid();
			using (var command = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
					VALUES (@company, @companyCode, 'AU company', @countryCode, @currencyNK)")))
			{
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.AddParameter("@companyCode", SqlDbType.VarChar, GlbCompanySchema.GC_Code.MaxLength, companyCode);
				command.AddParameter("@countryCode", SqlDbType.VarChar, GlbCompanySchema.GC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@currencyNK", SqlDbType.VarChar, GlbCompanySchema.GC_RX_NKLocalCurrency.MaxLength, currencyCode);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) values (@branch, @branchCode, @company)"))
			{
				command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				command.AddParameter("@branchCode", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, branchCode);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.ExecuteNonQuery();
			}
		}

		void SetupHolidaySource(Guid holidaySource, string name, string code)
		{
			using (var setupCommand = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbHolidaySource (GHS_PK, GHS_Name, GHS_Code, GHS_SystemCreateTimeUtc, GHS_SystemCreateUser, GHS_SystemLastEditTimeUtc, GHS_SystemLastEditUser)
					VALUES (@holidaySource, @name, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP');")))
			{
				setupCommand.AddParameter("@holidaySource", SqlDbType.UniqueIdentifier, holidaySource);
				setupCommand.AddParameter("@name", SqlDbType.NVarChar, GlbHolidaySourceSchema.GHS_Name.MaxLength, name);
				setupCommand.AddParameter("@code", SqlDbType.VarChar, GlbHolidaySourceSchema.GHS_Code.MaxLength, code);
				setupCommand.ExecuteNonQuery();
			}
		}

		void SetupHoliday(Guid parentID, string parentTableCode, DateTimeOffset date)
		{
			using (var setupCommand = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbHoliday (GH_PK, GH_HolidayName, GH_Date, GH_ParentID, GH_ParentTableCode)
					VALUES (NEWID(), @name, @date, @parentID, @parentTableCode);")))
			{
				setupCommand.AddParameter("@name", SqlDbType.VarChar, GlbHolidaySchema.GH_HolidayName.MaxLength, $"{parentTableCode} {date}");
				setupCommand.AddParameter("@date", SqlDbType.DateTimeOffset, date);
				setupCommand.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				setupCommand.AddParameter("@parentTableCode", SqlDbType.VarChar, GlbHolidaySchema.GH_ParentTableCode.MaxLength, parentTableCode);
				setupCommand.ExecuteNonQuery();
			}
		}

		void SetupHolidaySourceHistory(Guid holidaySource, DateTimeOffset effectiveDate, DateTimeOffset? effectiveEndDate)
		{
			using (var setupCommand = TestConnection.Command(string.Format(
				@"INSERT INTO dbo.GlbHolidaySourceHistory (GHH_PK, GHH_GS_Staff, GHH_EffectiveDate, GHH_AutoEffectiveEndDate, GHH_GHS_HolidaySource, GHH_SystemCreateTimeUtc, GHH_SystemCreateUser, GHH_SystemLastEditTimeUtc, GHH_SystemLastEditUser)
					VALUES (NEWID(), @staff, @effectiveDate, @effectiveEndDate, @holidaySource, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")))
			{
				setupCommand.AddParameter("@staff", SqlDbType.UniqueIdentifier, staff);
				setupCommand.AddParameter("@effectiveDate", SqlDbType.DateTimeOffset, effectiveDate);
				setupCommand.AddParameter("@effectiveEndDate", SqlDbType.DateTimeOffset, (object)effectiveEndDate ?? DBNull.Value);
				setupCommand.AddParameter("@holidaySource", SqlDbType.UniqueIdentifier, holidaySource);
				setupCommand.ExecuteNonQuery();
			}
		}
	}
}

