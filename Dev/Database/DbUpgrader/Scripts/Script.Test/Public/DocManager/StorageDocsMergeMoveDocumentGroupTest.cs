using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.DocManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.DocManager
{
	[TestedType(typeof(StorageDocsMergeMoveDocumentGroup))]
	sealed class StorageDocsMergeMoveDocumentGroupTest : DbCreateScriptTest
	{
		/// <summary>
		/// Sample Call Test Only.
		/// More comprehensive tests on StorageDocs Merge Module.
		/// </summary>
		public void TestSampleCall()
		{
			var storageMainPk = Guid.NewGuid();
			var sqlText = $"INSERT dbo.StorageMain (SM_PK, SM_DB) VALUES ('{storageMainPk}', 5)";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("DB Number - Before", 5, GetStorageMainDbNumber(storageMainPk));

			using (var cmd = TestConnection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SourceDbNumber", SqlDbType.Int, 5);
				cmd.AddParameter("@DestDbNumber", SqlDbType.Int, 1);
				cmd.ExecuteNonQuery();
			}

			AssertEquals("DB Number - After", 1, GetStorageMainDbNumber(storageMainPk));
		}

		int GetStorageMainDbNumber(Guid storageMainPk)
		{
			var sqlText = $"SELECT SM_DB FROM dbo.StorageMain WHERE SM_PK = '{storageMainPk}'";
			var result = (int)TestConnection.ExecuteScalar(sqlText);

			return result;
		}
	}

	sealed class StorageDocsMergeMoveDocumentGroupNonTransactionalTest : TestCase
	{
		/// <summary>
		/// If the StorageDocs schema changes, 
		/// the column list in this stored procedere MUST change accordingly.
		/// </summary>
		public void TestStorageDocsColumnListIsUpToDate()
		{
			var sqlText = @"
				DECLARE @ColList varchar(1000); SET @ColList = ''

				SELECT
					@ColList = @ColList + col.name + ', '
				FROM
					sys.columns col
					INNER JOIN sys.tables tab ON tab.object_id = col.object_id
				WHERE
					tab.name = 'StorageDocs'
					and col.is_computed = 0
				ORDER BY
					CASE col.name
						WHEN 'SC_PK' THEN 0
						ELSE 1
					END,
					col.name
					
				SET @ColList = rtrim(@ColList)
				SELECT left(@ColList, len(@ColList) - 1)";

			var actualColumnList = Db.Connection.ExecuteScalar(sqlText).ToString();
			var testScript = new StorageDocsMergeMoveDocumentGroup();
			var columnListRegex = new Regex(@"(?<=\bSET @StorageDocsFieldList = ')([^'])+(?=')", RegexOptions.IgnoreCase);
			var expectedColumnList = columnListRegex.Match(testScript.Text).Value;

			AssertEquals("StorageDocs CSV column list", expectedColumnList, actualColumnList);
		}

		public void TestStorageDocsColumnList_DoesNotExceedDeclaredVariableLength()
		{
			var testScript = new StorageDocsMergeMoveDocumentGroup();
			var storageDocsFieldLengthRegex = new Regex(@"(?<=\bDECLARE @StorageDocsFieldList varchar\()(\d+)(?=\))", RegexOptions.IgnoreCase);
			var storageDocsFieldLengthMatch = storageDocsFieldLengthRegex.Match(testScript.Text);

			AssertEquals("Extract defined maximum length for @StorageDocsFieldList", true, storageDocsFieldLengthMatch.Success);

			var destDbNameLengthRegex = new Regex(@"(?<=\bDECLARE @DestDbName varchar\()(\d+)(?=\))", RegexOptions.IgnoreCase);
			var destDbNameLengthMatch = destDbNameLengthRegex.Match(testScript.Text);

			AssertEquals("Extract defined maximum length for @DestDbName", true, destDbNameLengthMatch.Success);

			var sourceDbNameLengthRegex = new Regex(@"(?<=\bDECLARE @SourceDbName varchar\()(\d+)(?=\))", RegexOptions.IgnoreCase);
			var sourceDbNameLengthMatch = sourceDbNameLengthRegex.Match(testScript.Text);

			AssertEquals("Extract defined maximum length for @SourceDbName", true, sourceDbNameLengthMatch.Success);

			var sqlTextLengthRegex = new Regex(@"(?<=\bDECLARE @SqlText nvarchar\()(\d+)(?=\))", RegexOptions.IgnoreCase);
			var sqlTextLengthMatch = sqlTextLengthRegex.Match(testScript.Text);

			AssertEquals("Extract defined maximum length for @SqlText", true, sqlTextLengthMatch.Success);

			var storageDocsFieldLength = int.Parse(storageDocsFieldLengthMatch.Value);
			var columnListRegex = new Regex(@"(?<=\bSET @StorageDocsFieldList = ')([^'])+(?=')", RegexOptions.IgnoreCase);
			var expectedColumnList = columnListRegex.Match(testScript.Text).Value;

			AssertLessThan("The length of existing column lists should be less than defined maximum length", expectedColumnList.Length, storageDocsFieldLength);

			var start = testScript.Text.IndexOf("-- INSERT DEST COMMAND", StringComparison.OrdinalIgnoreCase);
			var end = testScript.Text.IndexOf("EXEC", start, StringComparison.OrdinalIgnoreCase);
			var substringLength = end - start;

			AssertGreaterThan("Length of substring should be greater than 0", substringLength, 0);

			var insertSqlText = testScript.Text.Substring(start, substringLength);
			var literalStringMatchCollection = Regex.Matches(insertSqlText, @"'([^']+)'");
			var literalStringTotalLength = literalStringMatchCollection.Cast<Match>().Sum(match => match.Groups[1].Length);
			var destDbNameLength = int.Parse(destDbNameLengthMatch.Value);
			var sourceDbNameLength = int.Parse(sourceDbNameLengthMatch.Value);
			var sqlTextLength = int.Parse(sqlTextLengthMatch.Value);

			AssertLessThan("The length of sql text should be less than defined maximum length", destDbNameLength + sourceDbNameLength + 2 * storageDocsFieldLength + literalStringTotalLength, sqlTextLength);
		}
	}
}

