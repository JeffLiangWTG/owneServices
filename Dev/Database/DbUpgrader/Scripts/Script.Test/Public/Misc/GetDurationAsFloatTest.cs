using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetDurationAsFloat))]
	class GetDurationAsFloatTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			AssertEquals("Min smalldatetime value", 0D, GetDurationAsFloat(new DateTime(1900, 1, 1)));
			AssertEquals("First moment of the year", 0D, GetDurationAsFloat(new DateTime(2009, 1, 1)));
			AssertEquals("2009-09-16 00:00", 258D, GetDurationAsFloat(new DateTime(2009, 9, 16)));
			AssertEquals("2006-09-16 10:09:07.003", 258.42299772376282D, GetDurationAsFloat(new DateTime(2006, 9, 16, 10, 9, 7, 3)));
			AssertEquals("1985-11-05 01:00", 308.04166666666788D, GetDurationAsFloat(new DateTime(1985, 11, 5, 1, 0, 0)));
			AssertEquals("Last moment of a leap year (1996-12-31 23:59:59.997)", 365.99999996142287D, GetDurationAsFloat(new DateTime(1996, 12, 31, 23, 59, 59, 997)));
			AssertEquals("Always less or equal than the last moment of the year", true, GetDurationAsFloat(DateTime.UtcNow) <= 365.99999996142287D);
		}

		double GetDurationAsFloat(DateTime duration)
		{
			double result;
			string sqlText = string.Format("SELECT * FROM dbo.{0}(@DateTimeDuration)", ScriptToTest.Name);

			using (DbCommand command = TestConnection.Command(sqlText))
			{
				command.AddParameter("@DateTimeDuration", SqlDbType.DateTime, duration);
				result = (double)command.ExecuteScalar();
			}

			return result;
		}
	}
}

