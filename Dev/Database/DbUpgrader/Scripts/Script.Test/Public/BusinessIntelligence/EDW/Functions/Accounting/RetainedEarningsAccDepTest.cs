using System;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(RetainedEarningsAccDep))]
	class RetainedEarningsAccDepTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestTransactionCategoryGroup()
		{
			Helper.InsertGLAccount(1, "111.222.01", "BSH");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertBASAccount(1);
			Helper.InsertPeriodForInputYear(2019);
			Helper.InsertStmData(new DateTime(2019, 03, 01));

			var categoryGroupBuilder = new StringBuilder();
			for (int index = 0; index < 26; index++)
			{
				var category = $"A{index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				Helper.InsertGLAggregate(100, 202001, 1, category, 100);
				categoryGroupBuilder.Append($"{category},");
			}
			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].RawAggregateAccDep(202001,'CBH',202002,1,'{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("singe category", 100m, result.Rows[0]["CurrentAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroupBuilder.ToString().TrimEnd(',')));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("maximum group is 25", 2500m, result.Rows[0]["CurrentAmount"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.InsertGLAccount(1, "111.222.01", "BSH");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertBASAccount(1);
			Helper.InsertPeriodForInputYear(2019);
			Helper.InsertStmData(new DateTime(2019, 03, 01));

			var categoryGroupBuilder = new StringBuilder();
			for (int index = 0; index < 10; index++)
			{
				var category = $"A{index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				Helper.InsertBASGLAggregate(100, 201901, 1, presentationCategory: category);
				categoryGroupBuilder.Append($"{category},");
			}

			for (int index = 10; index < 20; index++)
			{
				var category = $"A{index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				Helper.InsertGLAggregate(100, 202001, 1, category, 100);
				categoryGroupBuilder.Append($"{category},");
			}

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].RawAggregateAccDep(201901,'CBH',202002,1,'{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("singe category", 100m, result.Rows[0]["CurrentAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroupBuilder.ToString().TrimEnd(',')));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("maximum group is 20", 2000m, result.Rows[0]["CurrentAmount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
