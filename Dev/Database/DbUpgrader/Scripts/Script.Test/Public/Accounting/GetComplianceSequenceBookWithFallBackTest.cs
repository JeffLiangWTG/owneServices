using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetComplianceSequenceBookWithFallBack))]
	class GetComplianceSequenceBookWithFallBackTest : DbCreateScriptTest
	{
		readonly List<ComplianceBook> complianceBooks = new List<ComplianceBook>();
		readonly List<TestReference> testReferences = new List<TestReference>();
		Guid companyPK;

		class ComplianceBook
		{
			public string Code;
			public string Prefix;
			public int StartNumber;
			public int EndNumber;
			public int MaximumNumberDigits;
		}

		class TestReference
		{
			public string Reference;
			public string ExpectCode;
			public bool CanFind;
		}

		[ExpectNoExceptions("No exception should be thrown during executing SQL query command")]
		public void TestShouldFindComplianceSequenceBookByReference()
		{
			companyPK = TestDataCreator.CreateCompany("TES", "AU", "AUD");
			CreateComplianceBooksAndTestReferences();
			InsertComplianceBooksToDB();

			var querySql = $@"SELECT * FROM GetComplianceSequenceBookWithFallBack('{companyPK}', 'NTI', @Reference, NULL)";
			foreach (var testReference in testReferences)
			{
				using (var command = TestConnection.Command(querySql))
				{
					command.AddParameter("@Reference", System.Data.SqlDbType.VarChar, testReference.Reference);
					using (var result = (SqlDataReader)command.ExecuteReader())
					{
						result.Read();
						if (testReference.CanFind)
						{
							AssertEquals($"Result of Code: {testReference.ExpectCode} and Reference: {testReference.Reference} should has rows", true, result.HasRows);
							AssertEquals($"Compliance book of Test Data with Code: {testReference.ExpectCode} and Reference: {testReference.Reference} should be found.", testReference.ExpectCode, result["Code"]);
						}
						else
						{
							AssertEquals($"Result of Code: {testReference.ExpectCode} and Reference: {testReference.Reference} should not has rows", false, result.HasRows);
						}
					}
				}
			}
		}

		public void TestGetComplianceSequenceBookWithFallBackWithTransactionComplianceBook()
		{
			companyPK = TestDataCreator.CreateCompany("TES", "AU", "AUD");
			var complianceBook = new ComplianceBook() { Code = "CO6", Prefix = "", StartNumber = 1, EndNumber = 9, MaximumNumberDigits = 1 };
			complianceBooks.Add(complianceBook);
			InsertComplianceBooksToDB();

			var querySql = $@"SELECT XD_PK FROM dbo.AccComplianceSequence WHERE XD_Code = 'CO6'";
			string complianceBookPK;
			using (var command = TestConnection.Command(querySql))
			{
				using (var result = (SqlDataReader)command.ExecuteReader())
				{
					result.Read();
					complianceBookPK = result["XD_PK"].ToString();
				}
			}

			querySql = $@"SELECT * FROM GetComplianceSequenceBookWithFallBack(NULL, '', '', '{complianceBookPK}')";

			using (var command = TestConnection.Command(querySql))
			{
				using (var result = (SqlDataReader)command.ExecuteReader())
				{
					result.Read();
					Assert(result.HasRows);
					AssertEquals("CO6", result["Code"]);
				}
			}
		}

		void InsertComplianceBooksToDB()
		{
			var insertSql = $@"INSERT INTO {AccComplianceSequenceSchema.Constants.SqlSchemaName}.{AccComplianceSequenceSchema.Constants.TableName}
										({AccComplianceSequenceSchema.Constants.PK},
										{AccComplianceSequenceSchema.Constants.XD_Code},
										{AccComplianceSequenceSchema.Constants.XD_Description},
										{AccComplianceSequenceSchema.Constants.XD_Prefix},
										{AccComplianceSequenceSchema.Constants.XD_MaxChargesPerTransaction},
										{AccComplianceSequenceSchema.Constants.XD_RollupBehaviourWhenMaxExceeded},
										{AccComplianceSequenceSchema.Constants.XD_SequenceClass},
										{AccComplianceSequenceSchema.Constants.XD_GC_Company},
										{AccComplianceSequenceSchema.Constants.XD_StartNumber},
										{AccComplianceSequenceSchema.Constants.XD_EndNumber},
										{AccComplianceSequenceSchema.Constants.XD_NextNumber},
										{AccComplianceSequenceSchema.Constants.XD_MaximumNumberDigits},
										{AccComplianceSequenceSchema.Constants.XD_IsActive},
										{AccComplianceSequenceSchema.Constants.XD_SystemCreateTimeUtc},
										{AccComplianceSequenceSchema.Constants.XD_SystemCreateUser},
										{AccComplianceSequenceSchema.Constants.XD_SystemLastEditTimeUtc},
										{AccComplianceSequenceSchema.Constants.XD_SystemLastEditUser})
								VALUES
										(@PK,
										@XD_Code,
										@XD_Description,
										@XD_Prefix,
										@XD_MaxChargesPerTransaction,
										@XD_RollupBehaviourWhenMaxExceeded,
										@XD_SequenceClass,
										@XD_GC_Company,
										@XD_StartNumber,
										@XD_EndNumber,
										@XD_NextNumber,
										@XD_MaximumNumberDigits,
										@XD_IsActive,
										GetUtcDate(),
										'~BP',
										GetUtcDate(),
										'~BP')";

			foreach (var complianceBook in complianceBooks)
			{
				using (var command = TestConnection.Command(insertSql))
				{
					command.AddParameterBasedOnDbColumn("@PK", Guid.NewGuid(), AccComplianceSequenceSchema.PK);
					command.AddParameterBasedOnDbColumn("@XD_Code", complianceBook.Code, AccComplianceSequenceSchema.XD_Code);
					command.AddParameterBasedOnDbColumn("@XD_Prefix", complianceBook.Prefix, AccComplianceSequenceSchema.XD_Prefix);
					command.AddParameterBasedOnDbColumn("@XD_StartNumber", complianceBook.StartNumber, AccComplianceSequenceSchema.XD_StartNumber);
					command.AddParameterBasedOnDbColumn("@XD_EndNumber", complianceBook.EndNumber, AccComplianceSequenceSchema.XD_EndNumber);
					command.AddParameterBasedOnDbColumn("@XD_MaximumNumberDigits", complianceBook.MaximumNumberDigits, AccComplianceSequenceSchema.XD_MaximumNumberDigits);
					command.AddParameterBasedOnDbColumn("@XD_Description", "TXI", AccComplianceSequenceSchema.XD_Description);
					command.AddParameterBasedOnDbColumn("@XD_MaxChargesPerTransaction", 40, AccComplianceSequenceSchema.XD_MaxChargesPerTransaction);
					command.AddParameterBasedOnDbColumn("@XD_RollupBehaviourWhenMaxExceeded", "SSM", AccComplianceSequenceSchema.XD_RollupBehaviourWhenMaxExceeded);
					command.AddParameterBasedOnDbColumn("@XD_SequenceClass", "NTI", AccComplianceSequenceSchema.XD_SequenceClass);
					command.AddParameterBasedOnDbColumn("@XD_GC_Company", companyPK, AccComplianceSequenceSchema.XD_GC_Company);
					command.AddParameterBasedOnDbColumn("@XD_NextNumber", "17", AccComplianceSequenceSchema.XD_NextNumber);
					command.AddParameterBasedOnDbColumn("@XD_IsActive", 1, AccComplianceSequenceSchema.XD_IsActive);

					command.ExecuteNonQuery();
				}
			}
		}

		void CreateComplianceBooksAndTestReferences()
		{
			complianceBooks.Add(new ComplianceBook() { Code = "CO1", Prefix = "", StartNumber = 1, EndNumber = 9, MaximumNumberDigits = 1 });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "0", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "1", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "5", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "9", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "10", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "f1", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO1", Reference = "f", CanFind = false });
			complianceBooks.Add(new ComplianceBook() { Code = "CO2", Prefix = "", StartNumber = 50, EndNumber = 99, MaximumNumberDigits = 2 });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "49", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "50", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "75", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "99", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "100", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO2", Reference = "a50", CanFind = false });
			complianceBooks.Add(new ComplianceBook() { Code = "CO3", Prefix = "NTI", StartNumber = 1, EndNumber = 999999999, MaximumNumberDigits = 9 });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI000000001", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI005555555", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI999999999", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI1000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTI00000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO3", Reference = "NTC005555555", CanFind = false });
			complianceBooks.Add(new ComplianceBook() { Code = "CO4", Prefix = "12", StartNumber = 1, EndNumber = 999999999, MaximumNumberDigits = 9 });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "12000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "12000000001", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "12000000020", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "12999999999", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "121000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "1200000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "13000000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO4", Reference = "11000000020", CanFind = false });
			complianceBooks.Add(new ComplianceBook() { Code = "CO5", Prefix = "TCD1", StartNumber = 1, EndNumber = 999999999, MaximumNumberDigits = 9 });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD1000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD1000000001", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD1000000020", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD1999999999", CanFind = true });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD10000000000", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD100000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD0000000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TCD2000000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "TC1000000020", CanFind = false });
			testReferences.Add(new TestReference() { ExpectCode = "CO5", Reference = "12345678901234567890", CanFind = false });
		}
	}
}

