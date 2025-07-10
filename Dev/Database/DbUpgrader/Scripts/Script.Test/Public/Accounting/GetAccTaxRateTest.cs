using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetAccTaxRate))]
	class GetAccTaxRateTest : DbCreateScriptTest
	{
		public void TestAccTaxRateView()
		{
			PrepareTestData();
			AssertViewData();
		}

		void PrepareTestData()
		{
			helper.InsertTaxRate("CAPMST", true, "Standard Rated - Capital Items", "CAP", string.Empty, "DE", 1, "STD", string.Empty);
			helper.InsertTaxRate("EXEMPT", true, "Exempt Rated", "EXT", string.Empty, "DE", 1, "EMPTY", string.Empty);
			helper.InsertTaxRate("NOTREPORT", true, "Not Reportable", "NOT", string.Empty, "DE", 1, "NOT", string.Empty);
			helper.InsertTaxRate("MST", true, "Standard Rated", "CAP", "QCT", "DE", 1, "STD", "QCT");
			helper.InsertTaxRate("LOWMST", true, "Reduced Rate", "RAT", "SER", "DE", 1, "MID", "SER");

			var sql = $@"
INSERT INTO RefDatabase_RefAccTaxRate
	(ZAT_PK,ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate,ZAT_RateNumerator,ZAT_RateDenominator)
VALUES
	('{Guid.NewGuid()}','DE','CSTD','2018/01/01','2018/12/31',30,10),
	('{Guid.NewGuid()}','DE','SER','2018/01/01','2018/12/31',1234567890,1000000000),
	('{Guid.NewGuid()}','DE','LOW','2018/01/01','2018/12/31',25,10),
	('{Guid.NewGuid()}','DE','MID','2018/01/01','2018/12/31',6666,7),
	('{Guid.NewGuid()}','DE','STD','2018/01/01','2018/12/31',9,1),
	('{Guid.NewGuid()}','DE','STD','2017/01/01','2017/12/31',23,21),
	('{Guid.NewGuid()}','DE','QCT','2017/01/01','2018/12/31',85,5)";
			ExecuteCommand();

			void ExecuteCommand()
			{
				using (var command = TestConnection.Command(sql))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		void AssertViewData()
		{
			const string sql1 = "SELECT * FROM GetAccTaxRate('2018/06/01') WHERE ATV_RN_NKCountry = 'DE'";
			var rows = DataUtils.GetDataTableFromQuery(TestConnection, sql1).AsEnumerable().ToArray();

			AssertEquals(3, rows.Length);
			AssertCAPMST1(rows);
			AssertMST1(rows);
			AssertLOWMST1(rows);

			const string sql2 = "SELECT * FROM GetAccTaxRate('2017/06/01') WHERE ATV_RN_NKCountry = 'DE'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql2).AsEnumerable().ToArray();

			AssertEquals(2, rows.Length);
			AssertCAPMST2(rows);
			AssertMST2(rows);
		}

		void AssertCAPMST1(DataRow[] rows)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>(ATV_Code) == "CAPMST"
						&& x.Field<decimal>(ATV_Rate) == 9M
						&& x.Field<DateTime>(ATV_RateStartDate) == new DateTime(2018, 01, 01)
						&& x.Field<DateTime>(ATV_RateEndDate) == new DateTime(2018, 12, 31));
			AssertNotNull(row);

			AssertEquals("CAPMST", row.Field<string>(ATV_Code));
			AssertEquals("Standard Rated - Capital Items", row.Field<string>(ATV_Description));
			AssertEquals(null, row.Field<decimal?>(ATV_ExtraRate));
			AssertEquals(true, DBNull.Value.Equals(row[ATV_ExtraRateEndDate]));
			AssertEquals(true, DBNull.Value.Equals(row[ATV_ExtraRateStartDate]));
			AssertEquals(string.Empty, row.Field<string>(ATV_ExtraTaxRateType));
			AssertEquals(true, row.Field<bool>(ATV_IsActive));
			AssertEquals((short)1, row.Field<short>(ATV_PostingGroupId));
			AssertEquals("DE", row.Field<string>(ATV_RN_NKCountry));
			AssertEquals("CAP", row.Field<string>(ATV_Type));
		}

		void AssertCAPMST2(DataRow[] rows)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>(ATV_Code) == "CAPMST"
						&& x.Field<decimal>(ATV_Rate) == 1.095238095238095238M
						&& x.Field<DateTime>(ATV_RateStartDate) == new DateTime(2017, 01, 01)
						&& x.Field<DateTime>(ATV_RateEndDate) == new DateTime(2017, 12, 31));
			AssertNotNull(row);

			AssertEquals("CAPMST", row.Field<string>(ATV_Code));
			AssertEquals("Standard Rated - Capital Items", row.Field<string>(ATV_Description));
			AssertEquals(null, row.Field<decimal?>(ATV_ExtraRate));
			AssertEquals(true, DBNull.Value.Equals(row[ATV_ExtraRateEndDate]));
			AssertEquals(true, DBNull.Value.Equals(row[ATV_ExtraRateStartDate]));
			AssertEquals(string.Empty, row.Field<string>(ATV_ExtraTaxRateType));
			AssertEquals(true, row.Field<bool>(ATV_IsActive));
			AssertEquals((short)1, row.Field<short>(ATV_PostingGroupId));
			AssertEquals("DE", row.Field<string>(ATV_RN_NKCountry));
			AssertEquals("CAP", row.Field<string>(ATV_Type));
		}

		void AssertMST1(DataRow[] rows)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>(ATV_Code) == "MST"
						&& x.Field<decimal>(ATV_Rate) == 9M
						&& x.Field<DateTime>(ATV_RateStartDate) == new DateTime(2018, 01, 01)
						&& x.Field<DateTime>(ATV_RateEndDate) == new DateTime(2018, 12, 31));
			AssertNotNull(row);

			AssertEquals("MST", row.Field<string>(ATV_Code));
			AssertEquals("Standard Rated", row.Field<string>(ATV_Description));
			AssertEquals(17M, row.Field<decimal>(ATV_ExtraRate));
			AssertEquals(new DateTime(2018, 12, 31).Date, row.Field<DateTime>(ATV_ExtraRateEndDate));
			AssertEquals(new DateTime(2017, 1, 1).Date, row.Field<DateTime>(ATV_ExtraRateStartDate));
			AssertEquals("QCT", row.Field<string>(ATV_ExtraTaxRateType));
			AssertEquals(true, row.Field<bool>(ATV_IsActive));
			AssertEquals((short)1, row.Field<short>(ATV_PostingGroupId));
			AssertEquals("DE", row.Field<string>(ATV_RN_NKCountry));
			AssertEquals("CAP", row.Field<string>(ATV_Type));
		}

		void AssertMST2(DataRow[] rows)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>(ATV_Code) == "MST"
						&& x.Field<decimal>(ATV_Rate) == 1.095238095238095238M
						&& x.Field<DateTime>(ATV_RateStartDate) == new DateTime(2017, 01, 01)
						&& x.Field<DateTime>(ATV_RateEndDate) == new DateTime(2017, 12, 31));
			AssertNotNull(row);

			AssertEquals("MST", row.Field<string>(ATV_Code));
			AssertEquals("Standard Rated", row.Field<string>(ATV_Description));
			AssertEquals(17M, row.Field<decimal>(ATV_ExtraRate));
			AssertEquals(new DateTime(2018, 12, 31).Date, row.Field<DateTime>(ATV_ExtraRateEndDate));
			AssertEquals(new DateTime(2017, 1, 1).Date, row.Field<DateTime>(ATV_ExtraRateStartDate));
			AssertEquals("QCT", row.Field<string>(ATV_ExtraTaxRateType));
			AssertEquals(true, row.Field<bool>(ATV_IsActive));
			AssertEquals((short)1, row.Field<short>(ATV_PostingGroupId));
			AssertEquals("DE", row.Field<string>(ATV_RN_NKCountry));
			AssertEquals("CAP", row.Field<string>(ATV_Type));
		}

		void AssertLOWMST1(DataRow[] rows)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>(ATV_Code) == "LOWMST");

			AssertEquals("LOWMST", row.Field<string>(ATV_Code));
			AssertEquals("Reduced Rate", row.Field<string>(ATV_Description));
			AssertEquals(1.23456789M, row.Field<decimal>(ATV_ExtraRate));
			AssertEquals(new DateTime(2018, 12, 31).Date, row.Field<DateTime>(ATV_ExtraRateEndDate));
			AssertEquals(new DateTime(2018, 1, 1).Date, row.Field<DateTime>(ATV_ExtraRateStartDate));
			AssertEquals("SER", row.Field<string>(ATV_ExtraTaxRateType));
			AssertEquals(true, row.Field<bool>(ATV_IsActive));
			AssertEquals((short)1, row.Field<short>(ATV_PostingGroupId));
			AssertEquals(952.285714285714285714M, row.Field<decimal>(ATV_Rate));
			AssertEquals(new DateTime(2018, 12, 31), row.Field<DateTime>(ATV_RateEndDate));
			AssertEquals(new DateTime(2018, 01, 01), row.Field<DateTime>(ATV_RateStartDate));
			AssertEquals("DE", row.Field<string>(ATV_RN_NKCountry));
			AssertEquals("RAT", row.Field<string>(ATV_Type));
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestDbHelper(TestConnection);
		}

		TestDbHelper helper;
		const string ATV_Code = "ATV_Code";
		const string ATV_Description = "ATV_Description";
		const string ATV_ExtraRate = "ATV_ExtraRate";
		const string ATV_ExtraRateEndDate = "ATV_ExtraRateEndDate";
		const string ATV_ExtraRateStartDate = "ATV_ExtraRateStartDate";
		const string ATV_ExtraTaxRateType = "ATV_ExtraTaxRateType";
		const string ATV_IsActive = "ATV_IsActive";
		const string ATV_PostingGroupId = "ATV_PostingGroupId";
		const string ATV_Rate = "ATV_Rate";
		const string ATV_RateEndDate = "ATV_RateEndDate";
		const string ATV_RateStartDate = "ATV_RateStartDate";
		const string ATV_RN_NKCountry = "ATV_RN_NKCountry";
		const string ATV_Type = "ATV_Type";
	}
}

