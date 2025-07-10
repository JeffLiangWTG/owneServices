using System;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	public class LargeTestDataGenerationTest : TestCaseWithFactory
	{
		// This unit test has the purpose to store a SQL script that can be used to create a transaction with a large number of positions to be used for load tests
		// To use the script on your SAND database, copy it from "SQLScripts\CreateLargeTransactionForTesting.sql" in this solution and paste it into your SQL Studio
		// Execute the script to create a stored procedure that inserts the test data
		// An example call for the stored procedure can be found below in: const string runProcedureStatement
		// Note that the stored procedure may take several minutes to insert a transaction with one million positions!
		public void TestCreateLargeTransactionForTesting()
		{
			// create the stored procedure in the database
			var procedureSourceCode = GetEmbeddedResourceAsZString("CreateLargeTransactionForTesting.sql");
			Db.Connection.ExecuteNonQuery(procedureSourceCode);

			// run the procedure
			const string runProcedureStatement = "EXEC CreateLargeTransactionForTesting 10, 'EDI', 'SYD', 'BRN', 'Test Transaction', '2023-01-15'";
			var transactionHeaderPK = (Guid)Db.Connection.ExecuteScalar(runProcedureStatement);
			AssertNotNull("Procedure call was successful", transactionHeaderPK);

			var transaction = Factory.Load<AccTransactionHeader>(transactionHeaderPK);
			AssertNotNull("Transaction was found in the database", transaction);

			var lines = new AccTransactionLinesCollection(Factory, new ZQuery(AccTransactionLinesSchema.AL_AH, transactionHeaderPK));
			lines.Load();
			AssertEquals("Number of transaction lines", 10, lines.Count);
		}

		#region Implementation

		ZString GetEmbeddedResourceAsZString(string filename)
		{
			var filecontent = ZString.Empty;
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.Testing.ComplianceReport.SQLScripts." + filename))
			{
				using (var sr = new StreamReader(stream))
				{
					filecontent = sr.ReadToEnd();
				}
			}
			return filecontent;
		}

		#endregion
	}
}
