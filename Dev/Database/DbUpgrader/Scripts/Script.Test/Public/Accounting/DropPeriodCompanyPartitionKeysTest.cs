using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(DropPeriodCompanyPartitionKeys))]
	class DropPeriodCompanyPartitionKeysTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var companyPK = GetDemoCompanyPK();

			CreatePartitions();
			CreatePartitionData(companyPK);

			foreach (var key in partitionKeys)
			{
				using (var command = TestConnection.Command(string.Format("EXEC CreatePeriodCompanyPartitionKeys '{0}'", key)))
				{
					command.ExecuteNonQuery();
				}
			}

			var actualPartitionKeys = GetPartitionKeys();
			partitionKeys.Add("99999900000000-0000-0000-0000-000000000000");
			AssertContainsExactElementsInAnyOrder(partitionKeys, actualPartitionKeys);

			var keyToRemove = actualPartitionKeys.First();
			using (var command = TestConnection.Command(string.Format("EXEC DropPeriodCompanyPartitionKeys '{0}'", keyToRemove)))
			{
				command.ExecuteNonQuery();
			}

			partitionKeys.Remove(keyToRemove);
			actualPartitionKeys = GetPartitionKeys();
			AssertContainsExactElementsInAnyOrder(partitionKeys, actualPartitionKeys);

			List<string> GetPartitionKeys()
			{
				var keys = new List<string>();
				var sql = @"	SELECT	CAST(value as char(42)) as PeriodKey
							FROM	sys.partition_functions AS pf
									INNER JOIN sys.partition_range_values AS prv ON prv.function_id = pf.function_id
							WHERE	pf.name = 'PF_AccountingPeriodCompany'";

				var dt = DataUtils.GetDataTableFromQuery(TestConnection, sql);
				if (dt != null)
				{
					foreach (DataRow row in dt.Rows)
					{
						keys.Add(Convert.ToString(row[0], CultureInfo.InvariantCulture).ToUpperInvariant());
					}
				}
				return keys;
			}
		}

		void CreatePartitionData(Guid companyPK)
		{
			partitionKeys = new List<string>();
			var year = DateTime.Today.Year;
			for (int month = 1; month <= 12; month++)
			{
				var dateTime = new DateTime(year, month, 1);
				var period = year * 100 + month;
				AddAccountingPeriod(dateTime, dateTime.AddDays(DateTime.DaysInMonth(dateTime.Year, dateTime.Month)), period, year, companyPK);
				partitionKeys.Add(string.Format("{0}{1}", period, companyPK).ToUpperInvariant());
			}
		}

		void AddAccountingPeriod(DateTime startDate, DateTime endDate, int period, int year, Guid company)
		{
			string sQL = "INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_StartDate, AM_EndDate, AM_Period, AM_Year, AM_GC_Company) VALUES (NEWID(), @StartDate, @EndDate, @Period, @Year, @Company)";
			using (DbCommand cmd = TestConnection.Command(sQL))
			{
				cmd.AddParameter("@StartDate", SqlDbType.DateTime, startDate);
				cmd.AddParameter("@EndDate", SqlDbType.DateTime, endDate);
				cmd.AddParameter("@Period", SqlDbType.Int, period);
				cmd.AddParameter("@Year", SqlDbType.Int, year);
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, company);
				cmd.ExecuteNonQuery();
			}
		}

		Guid GetDemoCompanyPK()
		{
			string sQL = @"SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = @DemoCompanyCode";
			using (DbCommand cmd = TestConnection.Command(sQL))
			{
				cmd.AddParameter("@DemoCompanyCode", SqlDbType.Char, "DEM");

				Guid result = Guid.Empty;
				object queryResult = cmd.ExecuteScalar();
				if (queryResult is Guid)
				{
					result = (Guid)queryResult;
				}
				return result;
			}
		}

		void CreatePartitions()
		{
			var sql = @"IF NOT EXISTS(SELECT null FROM sys.partition_functions WHERE  NAME = 'PF_AccountingPeriodCompany')
						BEGIN
							DECLARE @sqlcmd nvarchar(max), @ids varchar(max);
							SELECT @ids = coalesce(@ids + ', ', '') +  a.AM_Key  FROM (SELECT '''' + convert(char(6), AM_Period) + convert(char(36), AM_GC_Company) + '''' as AM_Key from dbo.AccPeriodManagement) a

							SET @ids = '''99999900000000-0000-0000-0000-000000000000''' +  IIF(@ids IS NULL, '', ', ' + @ids) ;

							SET @sqlcmd = N'CREATE PARTITION FUNCTION PF_AccountingPeriodCompany(char(42)) AS RANGE LEFT FOR VALUES (' + @ids + N')' ;

							EXEC SP_EXECUTESQL @sqlcmd;

							CREATE PARTITION SCHEME PS_AccountingPeriodCompany AS PARTITION PF_AccountingPeriodCompany ALL TO ([REPORTGROUP]);
						END

						EXEC CreateJobCostingDataTableAndCCI 0";

			TestConnection.ExecuteNonQuery(sql);
		}

		List<string> partitionKeys;
	}
}

