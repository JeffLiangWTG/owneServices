using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(trgCusStatementHeader_Del))]
	class trgCusStatementLine_Del_Test : DbCreateScriptTest
	{
		public void TestDeletingCusStatementHeaderRaiseError()
		{
			var comapnyPK = Guid.NewGuid();
			var b2PK = Guid.NewGuid();

			const string insertQuery = @"
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES(@comapnyPK, 'DAN', 'US company', 'US', 'USD')
INSERT INTO dbo.CusStatementHeader(B2_PK, B2_GC) VALUES(@B2PK, @comapnyPK)
			";

			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@comapnyPK", SqlDbType.UniqueIdentifier, comapnyPK);
				command.AddParameter("@B2PK", SqlDbType.UniqueIdentifier, b2PK);

				command.ExecuteNonQuery();
			}

			var deleteQuery = string.Format("DELETE FROM dbo.CusStatementHeader WHERE B2_PK=@B2PK");
			using (var command = TestConnection.Command(deleteQuery))
			{
				command.AddParameter("@B2PK", SqlDbType.UniqueIdentifier, b2PK);
				var exceptionSql = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertContains(@"Delete operation NOT allowed on CusStatementHeader.", exceptionSql.Message);
			}
		}
	}
}

