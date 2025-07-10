using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(AccTransactionLines))]
	class AccTransactionLinesEDWTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 100, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			});
		}
		void AssertRowValues(DataTable resultTable, int? jh, double? headeramountspostdate_al_lineamount, double? headeramountspostdate_wipamount, double? headeramountspostdate_acramount, double? headeramountsreversedate_al_lineamount, double? headeramountsreversedate_wipamount, double? headeramountsreversedate_acramount, double? headeramountsreversedate_cstamount, double? headeramountsreversedate_revamount, double? headeramountsrevcst_al_lineamount, double? headeramountsrevcst_unrecogcstamount, double? headeramountsrevcst_unrecogrevamount)
		{
			var selectqry = string.Format("JH {0} AND HeaderAmountsPostDate_AL_LineAmount {1} AND HeaderAmountsPostDate_WIPAmount {2} AND HeaderAmountsPostDate_ACRAmount {3} AND HeaderAmountsReverseDate_AL_LineAmount {4} AND HeaderAmountsReverseDate_WIPAmount {5} AND HeaderAmountsReverseDate_ACRAmount {6} AND HeaderAmountsReverseDate_CSTAmount {7} AND HeaderAmountsReverseDate_REVAmount {8} AND HeaderAmountsREVCST_AL_LineAmount {9} AND HeaderAmountsREVCST_UnRecogCSTAmount {10} AND HeaderAmountsREVCST_UnRecogREVAmount {11}",
				jh == null ? "IS NULL" : "=" + jh
				, headeramountspostdate_al_lineamount == null ? "IS NULL" : "=" + headeramountspostdate_al_lineamount
				, headeramountspostdate_wipamount == null ? "IS NULL" : "=" + headeramountspostdate_wipamount
				, headeramountspostdate_acramount == null ? "IS NULL" : "=" + headeramountspostdate_acramount
				, headeramountsreversedate_al_lineamount == null ? "IS NULL" : "=" + headeramountsreversedate_al_lineamount
				, headeramountsreversedate_wipamount == null ? "IS NULL" : "=" + headeramountsreversedate_wipamount
				, headeramountsreversedate_acramount == null ? "IS NULL" : "=" + headeramountsreversedate_acramount
				, headeramountsreversedate_cstamount == null ? "IS NULL" : "=" + headeramountsreversedate_cstamount
				, headeramountsreversedate_revamount == null ? "IS NULL" : "=" + headeramountsreversedate_revamount
				, headeramountsrevcst_al_lineamount == null ? "IS NULL" : "=" + headeramountsrevcst_al_lineamount
				, headeramountsrevcst_unrecogcstamount == null ? "IS NULL" : "=" + headeramountsrevcst_unrecogcstamount
				, headeramountsrevcst_unrecogrevamount == null ? "IS NULL" : "=" + headeramountsrevcst_unrecogrevamount
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}
		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM {0}.dbo.AccTransactionLines('2010-08-13 14:17:00', '2010-10-13 14:17:00')",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GLTransactionLine](JobHeaderKey, LocalAmount, LineType, TransactionDate, GLTransactionLineID, GLTransactionLineKey, TransactionDateCheck)
															VALUES(1, 100.0000, 'WIP', '2010-08-13 14:17:00', '52E5080A-3670-4480-B8D2-C7E3FA23DBF8', -1, 'PostDate')",
			ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
