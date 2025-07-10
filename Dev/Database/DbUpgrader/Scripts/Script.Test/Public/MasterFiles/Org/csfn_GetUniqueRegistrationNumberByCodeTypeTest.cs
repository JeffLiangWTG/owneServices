using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(csfn_GetUniqueRegistrationNumberByCodeType))]
	class csfn_GetUniqueRegistrationNumberByCodeTypeTest : DbCreateScriptTest
	{
		public void Testcsfn_GetUniqueRegistrationNumber()
		{
			var insertSql = @"
			DECLARE @Org1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @Org2 UNIQUEIDENTIFIER = 'BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Org1, 'TESTORG1');

			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'ABN1234', 'ABN', 'AU', @Org1)
			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'ABN1234', 'ABN', 'NZ', @Org1)

			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'GST1234', 'GST', 'NZ', @Org1)
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqlQuery = "SELECT * FROM csfn_GetUniqueRegistrationNumberByCodeType(@OrgPK, @CodeType, @CountryCode)";

			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6")); //Org2
			command.AddParameter("@CodeType", SqlDbType.Char, "ABN");
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "");

			DataTable result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No record found as no cuscode is present for Org2", 0, result.Rows.Count);

			//without optional parameter CountryCode
			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392")); //Org1
			command.AddParameter("@CodeType", SqlDbType.Char, "ABN");
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "");

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("One record is found, even though 2 records match the filters", 1, result.Rows.Count);
			AssertEquals("ABN1234", result.Rows[0]["OK_CustomsRegNo"]);
			Assert("The result should contain column 'OK_RN_NKCodeCountry'", result.Columns.Contains("OK_RN_NKCodeCountry"));
			Assert("The result should contain column 'OK_CodeType'", result.Columns.Contains("OK_CodeType"));

			//with optional parameter CountryCode
			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392")); //Org1
			command.AddParameter("@CodeType", SqlDbType.Char, "ABN");
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "NZ");

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("One record found should be found - optional parameter countryCode was provided", 1, result.Rows.Count);
			AssertEquals("ABN1234", result.Rows[0]["OK_CustomsRegNo"]);
			Assert("The result should contain column 'OK_RN_NKCodeCountry'", result.Columns.Contains("OK_RN_NKCodeCountry"));
			Assert("The result should contain column 'OK_CodeType'", result.Columns.Contains("OK_CodeType"));

			//with optional parameter - no result
			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392")); //Org1
			command.AddParameter("@CodeType", SqlDbType.Char, "GST");
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "AU");

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No record found should be found as country Code does not match", 0, result.Rows.Count);
		}
	}
}

