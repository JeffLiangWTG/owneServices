using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime
{
	static class WorkingTimeTestHelper
	{
		static class Days
		{
			public const string Monday = "MON";
			public const string Tuesday = "TUE";
			public const string Wednesday = "WED";
			public const string Thursday = "THU";
			public const string Friday = "FRI";
			public const string Saturday = "SAT";
			public const string Sunday = "SUN";
		}

		public static readonly Guid dummyStaffPk = new Guid("ab60a25c-1057-4465-8893-0cfa1afb43f6"); // Arbitrary constant guid
		public static readonly Guid dummyDepartmentPk = new Guid("cf59d65f-91a5-43b7-8aa3-5f9572ecaf82"); // Arbitrary constant guid

		public static readonly DateTime earliestActiveStartTime = new DateTime(1900, 1, 1);

		public static void CreateWorkWeek(DbConnection conn, string tablePrefix, Guid parentPk, string monday = "", string tuesday = "", string wednesday = "", string thursday = "", string friday = "")
		{
			var command = new StringBuilder();

			var values = new[] { monday, tuesday, wednesday, thursday, friday };
			var days = new[] { Days.Monday, Days.Tuesday, Days.Wednesday, Days.Thursday, Days.Friday };
			foreach (var (day, val) in days.Zip(values, (d, v) => (d, v)))
			{
				if (!string.IsNullOrEmpty(val))
				{
					command.AppendLine(CreateWorkDayInsertStatement(tablePrefix, parentPk, day, val));
				}
			}

			conn.ExecuteNonQuery(command.ToString());
		}

		static string CreateWorkDayInsertStatement(string parentTablePrefix, Guid parentPk, string day, string intervals)
		{
			var formattedParameters = ExtractIntervals(intervals)
				.Select(interval => FormattableString.Invariant($"(NEWID(), 1, '{parentTablePrefix}', '{parentPk}', '{day}', {FormatDateTime(interval.start)}, {FormatDateTime(interval.end)}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"));

			return "INSERT INTO dbo.GlbWorkTime (GW_PK, GW_IsValid, GW_ParentTableCode, GW_ParentID, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES " + string.Join(", ", formattedParameters);

			string FormatDateTime(DateTime? value)
					=> value == null ? "NULL" : ("'" + value.Value.ToString("yyyy/MM/dd HH:mm:ss") + "'");
		}

		static IEnumerable<(DateTime start, DateTime end)> ExtractIntervals(string intervals)
		{
			// Tip: Intervals can be checked in wolfram alpha, like "(09:00 to 13:00) + (14:00 to 17:00)"
			foreach (var interval in intervals.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var match = Regex.Match(interval, @"^\s*([012]\d):([0-5]\d) to ([012]\d):([0-5]\d)\s*$");
				if (!match.Success)
				{
					throw new ArgumentException(FormattableString.Invariant($"Invalid interval: `{interval}`. Intervals must be in the form HH:MM to HH:MM, eg \"08:30 to 13:00, 14:00 to 16:20\"."));
				}

				var intervalNumbers = Enumerable.Range(1, 4)
					.Select(i => int.Parse(match.Groups[i].Value, CultureInfo.InvariantCulture))
					.ToArray();

				var startTime = new DateTime(1900, 1, 1, intervalNumbers[0], intervalNumbers[1], 0);
				var endTime = new DateTime(1900, 1, 1, intervalNumbers[2], intervalNumbers[3], 0);

				yield return (startTime, endTime);
			}
		}
	}
}

