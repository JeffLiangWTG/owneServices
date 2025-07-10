using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.StlAnalysis.Load.Testing
{
	sealed class EtlScriptsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllFactTransactionEtlScriptsAreCurerent()
		{
			var fullErrorMessage = new StringBuilder();
			string rootFactTransactionEtlFolder = Path.Combine(rootEtlFolderPath, "FactTransactions");
			var etlDir = new DirectoryInfo(rootFactTransactionEtlFolder);

			foreach (var fileInfo in etlDir.GetFiles("*.sql", SearchOption.AllDirectories))
			{
				string error = ExecuteScriptIfNotExternalDataSourceAndReturnErrors(fileInfo);

				if (error != null)
				{
					fullErrorMessage.AppendFormat("[{0}] {1}\r\n", fileInfo.FullName.Replace(etlDir.FullName, ""), error);
				}
			}

			Assert("Found problems with the following scripts:\r\n" + fullErrorMessage.ToString(), fullErrorMessage.Length == 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFactActiveUserEtlScriptIsCurerent()
		{
			string factActiveUserScriptPath = Path.Combine(rootEtlFolderPath, "FactActiveUsers", "MonthlyActiveUsers.sql");
			var sqlFile = new FileInfo(factActiveUserScriptPath);
			string sqlScript = GetSqlFileContents(sqlFile) + "\r\nOPTION (RECOMPILE)";
			AssertNoExceptionThrown("MonthlyActiveUsers script", () => Db.Connection.ExecuteNonQuery(sqlScript));
		}

		string ExecuteScriptIfNotExternalDataSourceAndReturnErrors(FileInfo sqlFile)
		{
			string sqlScript = GetSqlFileContents(sqlFile);
			var scriptSourceMatch = scriptSourceRegex.Match(sqlScript);

			return (scriptSourceMatch.Success && scriptSourceMatch.Groups["SOURCE"].Value != nameof(SourceLocation.CLIENT))
				? null
				: ExecuteScriptAndReturnErrors(sqlScript + "\r\nOPTION (RECOMPILE)");
		}

		static readonly Regex scriptSourceRegex = new Regex(@"^-- \[Source\]\s*(?<SOURCE>\w+)\b", RegexOptions.Compiled);

		string ExecuteScriptAndReturnErrors(string sqlScript)
		{
			string errorMessage = null;
			var unexpectedFieldList = new List<string>();
			var fieldNotFoundList = new List<string>();

			try
			{
				using (var cmd = Db.Connection.Command(sqlScript))
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();

					for (int i = 0; i < Math.Max(reader.FieldCount, 3); i++)
					{
						if (i > 2)
						{
							unexpectedFieldList.Add(reader.GetName(i));
						}
						else if (reader.FieldCount < i + 1 || reader.GetName(i) != expectedFields[i])
						{
							fieldNotFoundList.Add(expectedFields[i]);
						}
					}
				}
			}
			catch (SqlException ex)
			{
				errorMessage = "Script failed: " + ex.Message;
			}

			if (errorMessage == null && (unexpectedFieldList.Any() || fieldNotFoundList.Any()))
			{
				errorMessage =
					"Unexpected fields: (" + String.Join(", ", unexpectedFieldList) + ")" +
					" / Expected fields not found: (" + String.Join(", ", fieldNotFoundList) + ")";
			}

			return errorMessage;
		}

		string GetSqlFileContents(FileInfo sqlFile)
		{
			using (var sr = sqlFile.OpenText())
			{
				return sr.ReadToEnd();
			}
		}

		readonly string rootEtlFolderPath = Path.Combine(TestCase.BaseSourcePath, "BusinessIntelligence", "BiIntegration", "Development", "StlAnalysis", "StlAnalysisLoad", "StlAnalysisLoad", "Scripts", "ETL");
		readonly string[] expectedFields = new string[] { "CompanyCode", "TransactionDate", "TransactionCount" };
	}
}
