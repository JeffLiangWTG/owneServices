using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(csfn_GetDebtorExporterExemptionDocumentDetails))]
	class csfn_GetDebtorExporterExemptionDocumentDetailsTest : DbCreateScriptTest
	{
		[TestDate(2018, 2, 10)]
		public void Testcsfn_GetDebtorExporterExemptionDocumentDetailsTest()
		{
			var referenceDate = new DateTime(2018, 2, 10);

			var insertSql = @"
			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @OrgPK2 UNIQUEIDENTIFIER = 'BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK2, 'TESTORG2');

			DECLARE @JobReq1 UNIQUEIDENTIFIER = '7803F853-B05A-4091-B603-985567C2D373';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (@JobReq1, '1234A', '2018-02-01', '2018-02-09', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)

			DECLARE @JobReq2 UNIQUEIDENTIFIER = '73B57E80-57F5-4614-BFB8-95976815A927';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (@JobReq2, '1234B', '2018-02-09', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)

			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (newid(), '2345', '2018-02-10', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'DBT', 'NZ', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (newid(), '3456', '2018-02-10', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'CRT', 'AU', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (newid(), '4567', '2018-02-10', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (newid(), '5678', '2018-02-10', '2018-02-19', 'Special Notes', 'DEC', 'PER', 'DBT', 'AU', 'OH', @OrgPK1, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK1)
			
			DECLARE @JobReq6 UNIQUEIDENTIFIER = '817BF967-3D22-41D5-B9B6-176CA1F670F1';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID, EQ_DocCategory, EQ_CreditControlDoc, EQ_OriginalDocRequired, EQ_RcvFromCustomsBroker, EQ_ReturnToShipper, EQ_SntToCustomsBroker, EQ_OH_DocumentOwner)
				VALUES (@JobReq6, '6789', '2018-02-10', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK2, 'CSR', 0, 0, '2018-02-10', '2018-02-11', '2018-02-12', @OrgPK2)
";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqlQuery = "SELECT * FROM csfn_GetDebtorExporterExemptionDocumentDetails(@OrgPK, @CountryCode, @Date, @CompanyCode)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392")); //Org1
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "AU");
			command.AddParameter("@Date", SqlDbType.SmallDateTime, referenceDate.AddDays(-9));
			command.AddParameter("@CompanyCode", SqlDbType.VarChar, DBNull.Value);

			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("number of rows for Transaction 1", 1, result.Rows.Count);
			AssertEquals("1234A", result.Rows[0]["EQ_DocNumber"]);
			AssertEquals(new DateTimeOffset(new DateTime(2018, 2, 01), TimeSpan.Zero), (DateTimeOffset)result.Rows[0]["EQ_DateReceived"]);
			AssertEquals("Special Notes", result.Rows[0]["EQ_DocumentNotes"]);
			AssertEquals(new DateTime(2018, 2, 09), (DateTime)result.Rows[0]["EQ_ValidToDate"]);
			AssertEquals("EXV", result.Rows[0]["EQ_DocType"]);
			AssertEquals("PER", result.Rows[0]["EQ_DocPeriod"]);
			AssertEquals("DBT", result.Rows[0]["EQ_DocUsage"]);
			AssertEquals("AU", result.Rows[0]["EQ_RN_NKRelatedCountry"]);
			AssertEquals("CSR", result.Rows[0]["EQ_DocCategory"]);
			AssertEquals(false, result.Rows[0]["EQ_CreditControlDoc"]);
			AssertEquals(false, result.Rows[0]["EQ_OriginalDocRequired"]);
			AssertEquals(new DateTime(2018, 2, 10), (DateTime)result.Rows[0]["EQ_RcvFromCustomsBroker"]);
			AssertEquals(new DateTime(2018, 2, 11), (DateTime)result.Rows[0]["EQ_ReturnToShipper"]);
			AssertEquals(new DateTime(2018, 2, 12), (DateTime)result.Rows[0]["EQ_SntToCustomsBroker"]);
			AssertEquals(new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392"), (Guid)result.Rows[0]["EQ_OH_DocumentOwner"]);

			sqlQuery = "SELECT * FROM csfn_GetDebtorExporterExemptionDocumentDetails(@OrgPK, @CountryCode, @Date, @CompanyCode)";
			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392")); //Org1
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "AU");
			command.AddParameter("@Date", SqlDbType.SmallDateTime, referenceDate);
			command.AddParameter("@CompanyCode", SqlDbType.VarChar, DBNull.Value);

			result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("number of rows for Transaction 2", 2, result.Rows.Count);
			AssertEquals("1234B", result.Rows[0]["EQ_DocNumber"]);
			AssertEquals(new DateTimeOffset(new DateTime(2018, 2, 09), TimeSpan.Zero), (DateTimeOffset)result.Rows[0]["EQ_DateReceived"]);
			AssertEquals("Special Notes", result.Rows[0]["EQ_DocumentNotes"]);
			AssertEquals(new DateTime(2018, 2, 19), (DateTime)result.Rows[0]["EQ_ValidToDate"]);
			AssertEquals("EXV", result.Rows[0]["EQ_DocType"]);
			AssertEquals("PER", result.Rows[0]["EQ_DocPeriod"]);
			AssertEquals("DBT", result.Rows[0]["EQ_DocUsage"]);
			AssertEquals("AU", result.Rows[0]["EQ_RN_NKRelatedCountry"]);
			AssertEquals("CSR", result.Rows[0]["EQ_DocCategory"]);
			AssertEquals(false, result.Rows[0]["EQ_CreditControlDoc"]);
			AssertEquals(false, result.Rows[0]["EQ_OriginalDocRequired"]);
			AssertEquals(new DateTime(2018, 2, 10), (DateTime)result.Rows[0]["EQ_RcvFromCustomsBroker"]);
			AssertEquals(new DateTime(2018, 2, 11), (DateTime)result.Rows[0]["EQ_ReturnToShipper"]);
			AssertEquals(new DateTime(2018, 2, 12), (DateTime)result.Rows[0]["EQ_SntToCustomsBroker"]);
			AssertEquals(new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392"), (Guid)result.Rows[0]["EQ_OH_DocumentOwner"]);

			AssertEquals("4567", result.Rows[1]["EQ_DocNumber"]);
			AssertEquals(new DateTimeOffset(new DateTime(2018, 2, 10), TimeSpan.Zero), (DateTimeOffset)result.Rows[1]["EQ_DateReceived"]);
			AssertEquals("Special Notes", result.Rows[1]["EQ_DocumentNotes"]);
			AssertEquals(new DateTime(2018, 2, 19), (DateTime)result.Rows[1]["EQ_ValidToDate"]);
			AssertEquals("EXV", result.Rows[1]["EQ_DocType"]);
			AssertEquals("PER", result.Rows[1]["EQ_DocPeriod"]);
			AssertEquals("DBT", result.Rows[1]["EQ_DocUsage"]);
			AssertEquals("AU", result.Rows[1]["EQ_RN_NKRelatedCountry"]);
			AssertEquals("CSR", result.Rows[1]["EQ_DocCategory"]);
			AssertEquals(false, result.Rows[1]["EQ_CreditControlDoc"]);
			AssertEquals(false, result.Rows[1]["EQ_OriginalDocRequired"]);
			AssertEquals(new DateTime(2018, 2, 10), (DateTime)result.Rows[1]["EQ_RcvFromCustomsBroker"]);
			AssertEquals(new DateTime(2018, 2, 11), (DateTime)result.Rows[1]["EQ_ReturnToShipper"]);
			AssertEquals(new DateTime(2018, 2, 12), (DateTime)result.Rows[1]["EQ_SntToCustomsBroker"]);
			AssertEquals(new Guid("4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392"), (Guid)result.Rows[1]["EQ_OH_DocumentOwner"]);

			sqlQuery = "SELECT * FROM csfn_GetDebtorExporterExemptionDocumentDetails(@OrgPK, @CountryCode, @Date, @CompanyCode)";
			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, new Guid("BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6")); //Org2
			command.AddParameter("@CountryCode", SqlDbType.VarChar, "AU");
			command.AddParameter("@Date", SqlDbType.SmallDateTime, referenceDate);
			command.AddParameter("@CompanyCode", SqlDbType.VarChar, DBNull.Value);

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("number of rows for org TESTORG2", 1, result.Rows.Count);
			AssertEquals("6789", result.Rows[0]["EQ_DocNumber"]);
			AssertEquals(new DateTimeOffset(new DateTime(2018, 2, 10), TimeSpan.Zero), (DateTimeOffset)result.Rows[0]["EQ_DateReceived"]);
			AssertEquals("Special Notes", result.Rows[0]["EQ_DocumentNotes"]);
			AssertEquals(new DateTime(2018, 2, 19), (DateTime)result.Rows[0]["EQ_ValidToDate"]);
			AssertEquals("EXV", result.Rows[0]["EQ_DocType"]);
			AssertEquals("PER", result.Rows[0]["EQ_DocPeriod"]);
			AssertEquals("DBT", result.Rows[0]["EQ_DocUsage"]);
			AssertEquals("AU", result.Rows[0]["EQ_RN_NKRelatedCountry"]);
			AssertEquals("CSR", result.Rows[0]["EQ_DocCategory"]);
			AssertEquals(false, result.Rows[0]["EQ_CreditControlDoc"]);
			AssertEquals(false, result.Rows[0]["EQ_OriginalDocRequired"]);
			AssertEquals(new DateTime(2018, 2, 10), (DateTime)result.Rows[0]["EQ_RcvFromCustomsBroker"]);
			AssertEquals(new DateTime(2018, 2, 11), (DateTime)result.Rows[0]["EQ_ReturnToShipper"]);
			AssertEquals(new DateTime(2018, 2, 12), (DateTime)result.Rows[0]["EQ_SntToCustomsBroker"]);
			AssertEquals(new Guid("BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6"), (Guid)result.Rows[0]["EQ_OH_DocumentOwner"]);
		}
	}
}

