using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(vw_USLatestDispositionDatesInline))]
	class vw_USLatestDispositionDatesInlineTest : DbCreateScriptTest
	{
		public void TestGetDateFromSourceSO()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();
			var importerPK = TestDataCreator.CreateOrganisation("oh1", "OrgH1");
			const string declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK1, 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK2, 'IMP', @branchPK, @companyPK, 'JOB2', 'ACS', 0, 2);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1);
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1);
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK2, 2);
			const string billAddInfo = "UI_NKBillIssuerSCAC=APLU";
			var billPK = TestDataCreator.CreateCusDecHouseBill(true, billAddInfo, declarationPK1, 1);

			const string cusAddInfoSql = @"
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'UDP', 'Code=54*DispositionDate=2021-04-01 00:38:02.000*MessageSequenceNumber=2*Order=2*Source=CQ', 'CU', @billCUPK1, 1)
				INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_IsValid)
				VALUES(NEWID(), 'UDP', 'Code=54*DispositionDate=2021-03-30 00:37:07.000*MessageSequenceNumber=1*Order=1*Source=SO', 'CU', @billCUPK1, 1)";
			using (var command = Db.Connection.Command(cusAddInfoSql))
			{
				command.AddParameter("@billCUPK1", SqlDbType.UniqueIdentifier, billPK);
				command.ExecuteNonQuery();
			}

			const string reportSql = @"SELECT CONVERT(varchar, Disposition54Date, 22) as DispositionDate, DeclarationClusterKey FROM dbo.vw_USLatestDispositionDatesInline";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Disposition54Date from SO", "03/30/21 12:37:00 AM", reader["DispositionDate"].ToString());
					AssertEquals("One Job Cluster Key", 1, reader.GetInt32(1));
				}
			}
		}
	}
}
