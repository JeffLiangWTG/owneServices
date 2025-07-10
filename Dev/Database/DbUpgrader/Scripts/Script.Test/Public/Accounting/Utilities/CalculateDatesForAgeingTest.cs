using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(CalculateDatesForAgeing))]
	class CalculateDatesForAgeingTest : DbCreateScriptTest
	{
		//CREATE FUNCTION CalculateDatesForAgeing(@Company uniqueidentifier, @Period int, @AgeingOption char(3), @Day1 int, @Day2 int, @Day3 int, @Day4 int, @CurrentDateTime datetime)
		//RETURNS @Dates TABLE (	
		//ReportDate smalldatetime, 
		//P1PlusStart smalldatetime, 
		//P2PlusStart smalldatetime, 
		//P3PlusStart smalldatetime,
		//PCurrentStart smalldatetime, 
		//PCurrentEnd smalldatetime, 
		//P1Start smalldatetime, 
		//P2Start smalldatetime, 
		//P3Start smalldatetime,
		//P4Start smalldatetime 

		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201201', 'Jan 01 2012  0:00:00:000AM','Jan 31 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201202', 'Feb 01 2012  0:00:00:000AM','Feb 28 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201203', 'Mar 01 2012  0:00:00:000AM','Mar 31 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201204', 'Apr 01 2012  0:00:00:000AM','Apr 30 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201205', 'May 01 2012  0:00:00:000AM','May 31 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201206', 'Jun 01 2012  0:00:00:000AM','Jun 30 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201207', 'Jul 01 2012  0:00:00:000AM','Jul 31 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201208', 'Aug 01 2012  0:00:00:000AM','Aug 31 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201209', 'Sep 01 2012  0:00:00:000AM','Sep 30 2012  11:59:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'PER', 0, 0, 0, 0, 'May 31 2012  11:59:00:000PM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 5, 31, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 6, 1), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 7, 1), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 8, 1), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 5, 1), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 5, 31, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 4, 1), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 3, 1), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 2, 1), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 1, 1), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 0, 0, 0, 0, 'Jun 15 2012  10:00:00:000AM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 5, 31, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 6, 1), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 6, 1), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 6, 1), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 5, 31, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 1, 2, 3, 4, 'Jun 15 2012  10:00:00:000AM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 5, 31, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 6, 2), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 6, 3), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 6, 4), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 5, 31), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 6, 1, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 5, 30), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 5, 29), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 5, 28), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 5, 27), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 0, 0, 0, 0, 'May 15 2012  10:00:00:000AM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 5, 15, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 5, 16), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 5, 16), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 5, 16), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 5, 15, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 1, 2, 3, 4, 'May 15 2012  10:00:00:000AM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 5, 15, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 5, 17), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 5, 18), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 5, 19), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 5, 15), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 5, 16, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 5, 14), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 5, 13), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 5, 12), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 5, 11), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 0, 0, 0, 0, 'Apr 30 2012  11:59:00:000PM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 4, 30, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 5, 1), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 5, 1), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 5, 1), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 4, 30, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["P4Start"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from CalculateDatesForAgeing('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 201205, 'DAY', 1, 2, 3, 4, 'Apr 30 2012  11:59:00:000PM')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("ReportDate", new DateTime(2012, 4, 30, 23, 59, 0), (DateTime)result.Rows[0]["ReportDate"]);
			AssertEquals("P1PlusStart", new DateTime(2012, 5, 2), (DateTime)result.Rows[0]["P1PlusStart"]);
			AssertEquals("P2PlusStart", new DateTime(2012, 5, 3), (DateTime)result.Rows[0]["P2PlusStart"]);
			AssertEquals("P3PlusStart", new DateTime(2012, 5, 4), (DateTime)result.Rows[0]["P3PlusStart"]);
			AssertEquals("PCurrentStart", new DateTime(2012, 4, 30), (DateTime)result.Rows[0]["PCurrentStart"]);
			AssertEquals("PCurrentEnd", new DateTime(2012, 5, 1, 23, 59, 0), (DateTime)result.Rows[0]["PCurrentEnd"]);
			AssertEquals("P1Start", new DateTime(2012, 4, 29), (DateTime)result.Rows[0]["P1Start"]);
			AssertEquals("P2Start", new DateTime(2012, 4, 28), (DateTime)result.Rows[0]["P2Start"]);
			AssertEquals("P3Start", new DateTime(2012, 4, 27), (DateTime)result.Rows[0]["P3Start"]);
			AssertEquals("P4Start", new DateTime(2012, 4, 26), (DateTime)result.Rows[0]["P4Start"]);
		}
	}
}

