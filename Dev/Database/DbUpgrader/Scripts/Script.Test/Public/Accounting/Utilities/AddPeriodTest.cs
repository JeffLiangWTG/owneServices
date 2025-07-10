using System;
using System.Data;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(AddPeriod))]
	class AddPeriodTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const int NumberOfMonths = 15;
			const int NumberOfWeeks = 20;
			const int startingPeriod = 200605;

			Func<int, int, int> getNextPeriod = (p, numberOfPeriodsInYear) =>
			{
				var year = Math.DivRem(p, 100, out var interval);
				interval++;
				if (interval == numberOfPeriodsInYear + 1)
				{
					interval = 1;
					year++;
				}
				return year * 100 + interval;
			};

			Func<int, DateTime, string> getPeriodInsertLineSQL =
				(p, d) =>
					$"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '{p}', '{d.ToString("yyyy-MM-dd")}','{d.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd")}','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')";

			var period = startingPeriod;
			var startDate = new DateTime(2006, 5, 1);
			var sqlBuilder = new StringBuilder();

			for (var i = 0; i < NumberOfMonths; i++)
			{
				sqlBuilder.AppendLine(getPeriodInsertLineSQL(period, startDate));
				period = getNextPeriod(period, 12);
				startDate = startDate.AddMonths(1);
			}

			period = Math.DivRem(period, 100, out var notused) * 100 + 101;
			for (var i = 0; i < NumberOfWeeks; i++)
			{
				sqlBuilder.AppendLine(getPeriodInsertLineSQL(period, startDate));
				period = getNextPeriod(period, 52);
				startDate = startDate.AddMonths(1);
			}

			TestConnection.ExecuteNonQuery(sqlBuilder.ToString());

			var expectedResults = new[]
			{
				new { period = 200605, num = 1, expected = (object)200606 },
				new { period = 200605, num = 13, expected = (object)200706 },
				new { period = 200805, num = -10, expected = (object)200702 },
				new { period = 200605, num = NumberOfMonths + NumberOfWeeks, expected = (object)DBNull.Value },
				new { period = 200605, num = -1, expected = (object)DBNull.Value },
				new { period = 200607, num = 5, expected = (object)200612 },
				new { period = 200707, num = -7, expected = (object)200612 },
				new { period = 200605, num = 0, expected = (object)200605 },
				new { period = 200706, num = 17, expected = (object)200816 },
				new { period = 200803, num = -5, expected = (object)200705 },
			};

			using (var cmd = TestConnection.Command("SELECT [Value] FROM [dbo].[AddPeriod](@Period,@NumberOfPeriodsToAdd, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')"))
			{
				cmd.AddParameter("@Period", SqlDbType.Int, 0);
				cmd.AddParameter("@NumberOfPeriodsToAdd", SqlDbType.Int, 0);

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@Period", r.period);
					cmd.SetParameterValue("@NumberOfPeriodsToAdd", r.num);
					AssertEquals($"(period = {r.period}, numberOfPeriodsToAdd = {r.num})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

