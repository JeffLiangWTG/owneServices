using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(Report_CustomsEntriesbyBrokerUS))]
	class Report_CustomsEntriesbyBrokerUSTest : DbCreateScriptTest
	{
		public void TestInvoiceLines()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "012345678", "ENS", "CUS", "US");

			var invoice1PK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, 1, invoiceNumber: "INV001");
			TestDataCreator.CreateJobComInvoiceLine(invoice1PK, 1);

			var reportSql = $"SELECT JE_DeclarationReference, InvoiceLines FROM Report_CustomsEntriesbyBrokerUS('CCC', '', '', '{companyPK}')";
			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("JE_DeclarationReference", "B00001", (string)reader["JE_DeclarationReference"]);
					AssertEquals("InvoiceLines", 1, (int)reader["InvoiceLines"]);
				}
			);

			var invoice2PK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, 1, invoiceNumber: "INV002");
			TestDataCreator.CreateJobComInvoiceLine(invoice2PK, 1);
			TestDataCreator.CreateJobComInvoiceLine(invoice2PK, 1);
			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("JE_DeclarationReference", "B00001", (string)reader["JE_DeclarationReference"]);
					AssertEquals("InvoiceLines", 3, (int)reader["InvoiceLines"]);
				}
			);
		}

		public void TestReleaseDateTime()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "012345678", "ENS", "CUS", "US");
			var reportSql = $"SELECT JE_DeclarationReference, ReleaseDateTime FROM Report_CustomsEntriesbyBrokerUS('CCC', '', '', '{companyPK}')";
			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("JE_DeclarationReference", "B00001", (string)reader["JE_DeclarationReference"]);
					AssertEquals("ReleaseDateTime", DBNull.Value, reader["ReleaseDateTime"]);
				}
			);

			var updateDeclarationSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_EntryAuthorisationDate= '{0}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{1}'";
			Db.Connection.Command(string.Format(updateDeclarationSQL, "2019-02-28", declarationPK)).ExecuteNonQuery();
			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("JE_DeclarationReference", "B00001", (string)reader["JE_DeclarationReference"]);
					AssertEquals("ReleaseDateTime", "2019-02-28 00:00:00", (string)reader["ReleaseDateTime"]);
				}
			);

			Db.Connection.Command(string.Format(updateDeclarationSQL, "2019-03-01", declarationPK)).ExecuteNonQuery();
			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("JE_DeclarationReference", "B00001", (string)reader["JE_DeclarationReference"]);
					AssertEquals("ReleaseDateTime", "2019-03-01 00:00:00", (string)reader["ReleaseDateTime"]);
				}
			);
		}

		public void TestReturnActualEventTime()
		{
			var now = DateTime.Now;
			now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

			var dateEstimate = now.AddMonths(-2);
			var dateActual = now.AddMonths(2);

			string sql = string.Format(@"
								declare @Comp uniqueIdentifier
								declare @Branch uniqueIdentifier, @CountryCode VARCHAR(2)
								SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @Branch = GB_PK, @Comp = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;

								declare @RN_Code  varchar(2)
								set @RN_Code = (select top 1 rn_code from dbo.refcountry)

								declare @je_PK  uniqueIdentifier
								set @je_PK = NEWID()

								declare @jz_pk  uniqueIdentifier
								set @jz_pk = NEWID()

								insert into dbo.JobDeclaration
								(
									JE_PK, JE_DataModel, jE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_MessageType, JE_ClusterKey
								)
								values
								(
									@je_PK, @CountryCode, @Branch, @Comp, getdate(), 'B0001000', 'IMP', 1
								)

								insert into dbo.JobComInvoiceHeader 
								(
									JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey
								)
								values
								(
									@jz_pk, @CountryCode, @je_PK, @Branch, 1
								)

								insert into dbo.JobComInvoiceLine
								(
									JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey)
								values
								(
									NEWID(), @CountryCode, @jz_pk, 1
								)

								insert into dbo.JobComInvoiceLine
								(
									JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey)
								values
								(
									NEWID(), @CountryCode, @jz_pk, 1
								)

								insert into dbo.CusEntryHeader
								(
									CH_PK, CH_DataModel, CH_JE, CH_EntrySubmittedDate, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser
								)
								values
								(NEWID(), @CountryCode, @je_PK, DATEADD(DAY, 1, '{0}'), 'ENS', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
								(NEWID(), @CountryCode, @je_PK, '{0}', 'SE', 1, getutcdate(), '~BP', getutcdate(), '~BP')

								insert into dbo.CusEntryNum
								(
									CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser
								)
								values
								(
									NEWID(), 'ENS', @je_PK, 'US', 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP'
								)

								insert into dbo.StmALog
								(
									SL_PK, SL_EventTime, SL_SE_NKEvent, SL_Parent, SL_Table
								)
								values
								(
									NEWID(), '{1}', 'CLR', @je_PK, 'JobDeclaration'
								)

								select * from Report_CustomsEntriesbyBrokerUS('CCC', dateadd(m, -6, GetDate()), 	dateadd(m, 6, GetDate()), @Comp)
								order by CE_EntryNum", dateEstimate.ToSqlFormat(), dateActual.ToSqlFormat());

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);

					var resultDataTime = (DateTime)reader["SL_EventTimeCCC"];
					var expectedDataTime = dateEstimate;

					AssertEquals("CCC date returned is actual date.", expectedDataTime, resultDataTime);
				}
			}
		}
	}
}
