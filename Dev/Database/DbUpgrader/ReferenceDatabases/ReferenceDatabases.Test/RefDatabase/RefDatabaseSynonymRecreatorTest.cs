using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.Testing;
using CargoWise.Database.Shared;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;
namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class RefDatabaseSynonymRecreatorTest : TransactionedTestCase
	{
		public void TestEnsureSynonymIsPointingToValudeObject()
		{
			const string synonymPrefix = RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix;
			var incorrectMapping = new ZStringBuilder();
			foreach (var mapping in RefDatabaseVersionMapHelper.CombinedViewMappings)
			{
				var synonymName = $"{synonymPrefix}_{mapping.Key}";
				if (!IsSynonymValid(synonymName))
				{
					incorrectMapping.Append($"Synonym '{synonymName}' refers to an invalid object '{mapping.Value}'.");
				}
			}
			AssertEquals("Invalid synonym mapping", string.Empty, incorrectMapping.ToStringWithNewLineBetweenAppends());
		}

		bool IsSynonymValid(string synonym)
		{
			const string script = "SELECT CASE WHEN EXISTS(SELECT NULL FROM sys.synonyms WHERE name = @SynonymName AND OBJECT_ID(base_object_name) IS NOT NULL) THEN 1 ELSE 0 END";
			using (var cmd = TestConnection.Command(script))
			{
				cmd.AddParameter("@SynonymName", SqlDbType.VarChar, synonym);
				return ((int)cmd.ExecuteScalar()) == 1;
			}
		}

		public void TestRecreateSynonymOnlyWhenNotExist()
		{
			//Arrange
			const string synonymName = "RefDatabase_RefDataGrouping";
			TestConnection.ExecuteScalar($"DROP SYNONYM [dbo].[{synonymName}]");

			//Act
			//Assert
			AssertSynonymsExist(synonymName, isExist: false);
			AssertRecreatesSynonym(synonymName, shouldRecreate: true);
			AssertSynonymsExist(synonymName, isExist: true);
			AssertRecreatesSynonym(synonymName, shouldRecreate: false);
		}

		void AssertRecreatesSynonym(string synonymName, bool shouldRecreate)
		{
			var executedScripts = new List<string>();
			var creator = new Mock<RefDatabaseSynonymRecreator>(TestConnection) { CallBase = true };
			creator.Protected()
				.Setup("ExecuteScript", ItExpr.IsAny<string>())
				.Callback<string>(executedScripts.Add)
				.CallBase();

			creator.Object.RecreateSynonym();
			AssertEquals($"Should {(shouldRecreate ? "not" : "")} have recreated synonym: {synonymName}, executedCommands: {(string.Join(Environment.NewLine, executedScripts))}", executedScripts.Any(x => x.Contains($"CREATE SYNONYM [dbo].[{synonymName}]")),shouldRecreate);
		}

		public void TestRecreateSynonym()
		{
			TestConnection.ExecuteScalar("DROP TABLE IF EXISTS [RefAccTaxRate_ForSRDbOffline]");
			TestConnection.ExecuteScalar("CREATE SYNONYM [RefDatabase_RateView123] FOR [CW-RefDatabase].[dbo].[RateView123_V1]");
			
			var creator = new RefDatabaseSynonymRecreator(TestConnection);
			creator.RecreateSynonym();

			AssertSynonymsExist("RefDatabase_RateView123", isExist: false);
			AssertSynonymsExist("RefDatabase_RateView");
			AssertSynonymsExist("RefDatabase_RefAccTaxRate");

			AssertSRDbTestingTableNotExist("RefAccTaxRate_ForSRDbOffline");
		}

		void AssertSynonymsExist(string synonymName, bool isExist = true)
		{
			string sqlText = "IF EXISTS (SELECT NULL FROM sys.synonyms WHERE name = '" + synonymName + "') SELECT 1 ELSE SELECT 0";
			AssertEquals(synonymName + " synonym exist", isExist, Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
		}

		void AssertSRDbTestingTableNotExist(string tableName)
		{
			var sql = $"IF EXISTS (SELECT 1 FROM sys.tables WHERE name='{tableName}') SELECT 1 ELSE SELECT 0";
			AssertEquals(false, Convert.ToBoolean(TestConnection.ExecuteScalar(sql)));
		}

		public void TestRecreateSynonymDoesNotReportIssueIfTimedOut()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6522, byte.MaxValue, byte.MinValue, TestConnection.ServerName, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0);
			var sqlException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(error), new Win32Exception("The wait operation timed out"));
			var param = new Mock<IDbDataParameter>();
			param.SetupSet(x => x.ParameterName = It.IsAny<string>());
			param.SetupSet(x => x.Value = It.IsAny<string>());
			var command = new Mock<IDbCommand>();
			var collection = new Mock<IDataParameterCollection>();
			collection.Setup(x => x.Add(It.IsAny<object>()));
			command.Setup(x => x.CreateParameter()).Returns(param.Object);
			command.SetupGet(x => x.Parameters).Returns(collection.Object);
			command.Setup(x => x.ExecuteNonQuery()).Throws(sqlException);
			var factory = new Mock<IDataProviderFactory>();
			var dbConnection = new Mock<DbConnection>(factory.Object, "abc", "abc", null, null);
			dbConnection.As<IDbConnectionWithSettings>().Setup(x => x.CreateCommand(It.IsAny<string>())).Returns(command.Object);

			var creator = new Mock<RefDatabaseSynonymRecreator>(dbConnection.Object) { CallBase = true };
			creator.Setup(x => x.GetExistingRefDbSynonyms(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
				.Returns((new () { "ASynonym" }, new () { "ASynonym" }));

			var refAppException = AssertExceptionThrown<RefApplicationException>(() => creator.Object.RecreateSynonym());
			AssertEquals(false, refAppException.ReportIssue);
		}
	}
}
