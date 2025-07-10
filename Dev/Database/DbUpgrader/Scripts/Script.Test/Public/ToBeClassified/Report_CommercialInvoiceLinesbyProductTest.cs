using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(Report_CommercialInvoiceLinesbyProduct))]
	class Report_CommercialInvoiceLinesbyProductTest : DbCreateScriptTest
	{
		public void TestCountryOfOrigin()
		{
			PrepareTestData();

			DataTable resultUS1 = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JI_CountryOfOrigin FROM Report_CommercialInvoiceLinesbyProduct('{USCompanyPK}', 'US', '') WHERE JI_CountryOfOrigin = 'IT' AND JE_MessageType='IMP'");
			DataTable resultUS2 = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JI_CountryOfOrigin FROM Report_CommercialInvoiceLinesbyProduct('{USCompanyPK}', 'US', '') WHERE JI_CountryOfOrigin = 'CN' AND JE_MessageType='EXP'");
			DataTable resultUS3 = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JI_CountryOfOrigin FROM Report_CommercialInvoiceLinesbyProduct('{USCompanyPK}', 'US', '') WHERE JI_CountryOfOrigin = 'KR' AND JE_MessageType='FTZ'");
			DataTable resultCA = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JI_CountryOfOrigin FROM Report_CommercialInvoiceLinesbyProduct('{CACompanyPK}', 'CA', '') WHERE JI_CountryOfOrigin = 'PR'");
			DataTable resultTW = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JI_CountryOfOrigin FROM Report_CommercialInvoiceLinesbyProduct('{TWCompanyPK}', 'TW', '') WHERE JI_CountryOfOrigin = 'JP'");
			AssertEquals("Result should have 1 row", 1, resultUS1.Rows.Count);
			AssertEquals("Result should have 1 row", 1, resultUS2.Rows.Count);
			AssertEquals("Result should have 1 row", 1, resultUS3.Rows.Count);
			AssertEquals("Result should have 1 row", 1, resultCA.Rows.Count);
			AssertEquals("Result should have 1 row", 1, resultTW.Rows.Count);
		}

		readonly Guid USCompanyPK = Guid.NewGuid();
		readonly Guid CACompanyPK = Guid.NewGuid();
		readonly Guid TWCompanyPK = Guid.NewGuid();
		public void PrepareTestData()
		{
			var sql = $@"
			DECLARE @USBranchPK			UNIQUEIDENTIFIER = NEWID(),
					@CABranchPK			UNIQUEIDENTIFIER = NEWID(),
					@TWBranchPK			UNIQUEIDENTIFIER = NEWID(),
					@USDeclaration1PK	UNIQUEIDENTIFIER = NEWID(),
					@USDeclaration2PK	UNIQUEIDENTIFIER = NEWID(),
					@USDeclaration3PK	UNIQUEIDENTIFIER = NEWID(),
					@CADeclarationPK	UNIQUEIDENTIFIER = NEWID(),
					@TWDeclarationPK	UNIQUEIDENTIFIER = NEWID(),
					@USInvoice1PK		UNIQUEIDENTIFIER = NEWID(),
					@USInvoice2PK		UNIQUEIDENTIFIER = NEWID(),
					@USInvoice3PK		UNIQUEIDENTIFIER = NEWID(),
					@CAInvoicePK		UNIQUEIDENTIFIER = NEWID(),
					@TWInvoicePK		UNIQUEIDENTIFIER = NEWID(),
					@USInvoiceLine1PK	UNIQUEIDENTIFIER = NEWID(),
					@USInvoiceLine2PK	UNIQUEIDENTIFIER = NEWID(),
					@USInvoiceLine3PK	UNIQUEIDENTIFIER = NEWID(),
					@CAInvoiceLinePK	UNIQUEIDENTIFIER = NEWID(),
					@TWInvoiceLinePK	UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES(@USCompanyPK, 'USC', 'US company', 'US', 'USD')
INSERT INTO dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@USBranchPK, 'USB', @USCompanyPK)

INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES(@CACompanyPK, 'CAC', 'CA company', 'CA', 'CAD')
INSERT INTO dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@CABranchPK, 'CAB', @CACompanyPK)

INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES(@TWCompanyPK, 'TWC', 'TW company', 'TW', 'TWD')
INSERT INTO dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@TWBranchPK, 'TWB', @TWCompanyPK)

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ClusterKey) VALUES (@USDeclaration1PK, 'US', @USBranchPK, @USCompanyPK, 'US00001', 'IMP', 1)
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_JE, JZ_ClusterKey) VALUES (@USInvoice1PK, 'US', 'INVUS1', @USDeclaration1PK, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_AddInfo, JI_CountryOfOrigin, JI_JZ, JI_ClusterKey) VALUES (@USInvoiceLine1PK, 'US', 'UC_NKCountryOfOrigin=IT', '', @USInvoice1PK, 1)

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ClusterKey) VALUES (@USDeclaration2PK, 'US', @USBranchPK, @USCompanyPK, 'US00002', 'EXP', 2)
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_JE, JZ_ClusterKey) VALUES (@USInvoice2PK, 'US', 'INVUS2', @USDeclaration2PK, 2)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_AddInfo, JI_CountryOfOrigin, JI_JZ, JI_ClusterKey) VALUES (@USInvoiceLine2PK, 'US', '', 'CN', @USInvoice2PK, 2)

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_AddInfo, JE_DeclarationReference, JE_MessageType, JE_ClusterKey) VALUES (@USDeclaration3PK, 'US', @USBranchPK, @USCompanyPK, 'UC_NKCountryOfOrigin=KR', 'US00003', 'FTZ', 3)
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_JE, JZ_ClusterKey) VALUES (@USInvoice3PK, 'US', 'INVUS3', @USDeclaration3PK, 3)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_AddInfo, JI_CountryOfOrigin, JI_JZ, JI_ClusterKey) VALUES (@USInvoiceLine3PK, 'US', '', 'CN', @USInvoice3PK, 3)

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ClusterKey) VALUES(@CADeclarationPK, 'CA', @CABranchPK, @CACompanyPK, 'CA00001', 'IMP', 4)
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_JE, JZ_ClusterKey) VALUES(@CAInvoicePK, 'CA', 'INVCA1', @CADeclarationPK, 4)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_AddInfo, JI_CountryOfOrigin, JI_JZ, JI_ClusterKey) VALUES(@CAInvoiceLinePK, 'CA', '', 'PR', @CAInvoicePK, 4)

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ClusterKey) VALUES(@TWDeclarationPK, 'TW', @TWBranchPK, @TWCompanyPK, 'TW00001', 'IMP', 5)
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_JE, JZ_ClusterKey) VALUES(@TWInvoicePK, 'TW', 'INVTW1', @TWDeclarationPK, 5)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_AddInfo, JI_CountryOfOrigin, JI_JZ, JI_ClusterKey) VALUES(@TWInvoiceLinePK, 'TW', '', 'JP', @TWInvoicePK, 5)
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@USCompanyPK", SqlDbType.UniqueIdentifier, USCompanyPK);
				cmd.AddParameter("@CACompanyPK", SqlDbType.UniqueIdentifier, CACompanyPK);
				cmd.AddParameter("@TWCompanyPK", SqlDbType.UniqueIdentifier, TWCompanyPK);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
