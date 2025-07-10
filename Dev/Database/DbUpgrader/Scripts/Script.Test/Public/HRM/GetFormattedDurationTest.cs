using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(GetFormattedDuration))]

	class GetFormattedDurationTest : DbCreateScriptTest
	{
		public void TestEmpty() => AssertEquals(("0:00", 0), CallFormatDate(new DateTime(1900, 1, 1)));
		public void TestMinutes() => AssertEquals(("0:05", 5), CallFormatDate(new DateTime(1900, 1, 1, 0, 5, 0)));
		public void TestHours() => AssertEquals(("24:00", 24 * 60), CallFormatDate(new DateTime(1900, 1, 2)));
		public void TestHoursWithLeftoverMinutes() => AssertEquals(("12:23", 12 * 60 + 23), CallFormatDate(new DateTime(1900, 1, 1, 12, 23, 0)));
		public void TestMoreThanOneDay() => AssertEquals(("60:23", 2 * 24 * 60 + 12 * 60 + 23), CallFormatDate(new DateTime(1900, 1, 3, 12, 23, 0)));

		(string duration, int totalMinutes) CallFormatDate(DateTime date)
		{
			(string, int) result = default;

			TestConnection.ExecuteReader("SELECT * FROM GetFormattedDuration(@date)",
				param => param.AddParameter("@date", SqlDbType.SmallDateTime, date),
				reader => result = (reader.GetString(0), reader.GetInt32(1)));

			return result;
		}
	}
}
