using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(IsCAPGAIndicated))]
	class IsCAPGAIndicatedTest : DbCreateScriptTest
	{
		public void TestIsCAPGAIndicated_YIndicatorOverruleOtherIndicators()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=Y'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=Y')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", "Y"));
		}

		public void TestIsCAPGAIndicated_NIndicatorOverruleBlankAndNA()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", "N"));
		}

		public void TestIsCAPGAIndicated_NAIndicator()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A'),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=N/A')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", "N/A"));
		}

		public void TestIsCAPGAIndicated_BlankIndicator()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd='),
	(NEWID(), @LinePK, 'JI', 'CHC', 'HDRProgramInd=')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", "N/A"));
		}

		public void TestIsCAPGAIndicated_NoIndicatorMatchData()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES (NEWID(), @LinePK, 'JI', 'CHC', 'ZZZProgramInd=')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", "N/A"));
		}

		public void TestIsCAPGAIndicated_NullIndicator()
		{
			var linePK = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

IF OBJECT_ID('Constraint_B7_Type', 'C') IS NOT NULL ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@LinePK, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK, 'JI', 'C12', 'HDRProgramInd=Y'),
	(NEWID(), @LinePK, 'JI', 'CEC', '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.ExecuteNonQuery();
			}

			AssertData("LinePK", linePK, ("HCHDR", DBNull.Value));
		}

		public void TestHCAPI()
		{
			AssertIsCAPGAIndicated("CHC", "API", "HCAPI");
		}

		public void TestHCBBC()
		{
			AssertIsCAPGAIndicated("CHC", "BBC", "HCBBC");
		}

		public void TestHCCTO()
		{
			AssertIsCAPGAIndicated("CHC", "CTO", "HCCTO");
		}

		public void TestHCCPR()
		{
			AssertIsCAPGAIndicated("CHC", "CPR", "HCCPR");
		}

		public void TestHCDSE()
		{
			AssertIsCAPGAIndicated("CHC", "DSE", "HCDSE");
		}

		public void TestHCHDR()
		{
			AssertIsCAPGAIndicated("CHC", "HDR", "HCHDR");
		}

		public void TestHCOCS()
		{
			AssertIsCAPGAIndicated("CHC", "OCS", "HCOCS");
		}

		public void TestHCMDE()
		{
			AssertIsCAPGAIndicated("CHC", "MDE", "HCMDE");
		}

		public void TestHCNHP()
		{
			AssertIsCAPGAIndicated("CHC", "NHP", "HCNHP");
		}

		public void TestHCPES()
		{
			AssertIsCAPGAIndicated("CHC", "PES", "HCPES");
		}

		public void TestHCRED()
		{
			AssertIsCAPGAIndicated("CHC", "RED", "HCRED");
		}

		public void TestHCVET()
		{
			AssertIsCAPGAIndicated("CHC", "VET", "HCVET");
		}

		public void TestTCTPR()
		{
			AssertIsCAPGAIndicated("CTC", "TPR", "TCTPR");
		}

		public void TestTCVPR()
		{
			AssertIsCAPGAIndicated("CTC", "VPR", "TCVPR");
		}

		public void TestECCCWRM()
		{
			AssertIsCAPGAIndicated("CEC", "WRM", "ECCCWRM");
		}

		public void TestECCCODS()
		{
			AssertIsCAPGAIndicated("CEC", "ODS", "ECCCODS");
		}

		public void TestECCCWEN()
		{
			AssertIsCAPGAIndicated("CEC", "WEN", "ECCCWEN");
		}

		public void TestNRCanEEF()
		{
			AssertIsCAPGAIndicated("CNR", "EEF", "NRCanEEF");
		}

		public void TestNRCanEXP()
		{
			AssertIsCAPGAIndicated("CNR", "EXP", "NRCanEXP");
		}

		public void TestNRCanRDA()
		{
			AssertIsCAPGAIndicated("CNR", "RDA", "NRCanRDA");
		}

		public void TestDFOABI()
		{
			AssertIsCAPGAIndicated("CFO", "ABI", "DFOABI");
		}

		public void TestDFOAIS()
		{
			AssertIsCAPGAIndicated("CFO", "AIS", "DFOAIS");
		}

		public void TestDFOTTP()
		{
			AssertIsCAPGAIndicated("CFO", "TTP", "DFOTTP");
		}

		public void TestCNSCALL()
		{
			AssertIsCAPGAIndicated("CCN", "ALL", "CNSCALL");
		}

		public void TestGACALL()
		{
			AssertIsCAPGAIndicated("CGA", "ALL", "GACALL");
		}

		public void TestCFIAALL()
		{
			AssertIsCAPGAIndicated("CCF", "ALL", "CFIAALL");
		}

		public void TestPHACHAP()
		{
			AssertIsCAPGAIndicated("CPH", "HAP", "PHACHAP");
		}

		void AssertIsCAPGAIndicated(string type, string programIndType, string columnName)
		{
			var linePK01 = Guid.NewGuid();
			var linePK02 = Guid.NewGuid();
			var linePK03 = Guid.NewGuid();

			var sql = @"
DECLARE	@InvoicePK UNIQUEIDENTIFIER = NEWID();
DECLARE	@CompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE	@BranchPK UNIQUEIDENTIFIER = NEWID()

INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (@CompanyPK, 'CA', 'CAD', 'DCA', 'CA company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES (@BranchPK, @CompanyPK, 'BLO', 'CA')
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB) VALUES (@InvoicePK, 'CA', 1, @BranchPK)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey)
VALUES
	(@LinePK01, 'CA', @InvoicePK, 1),
	(@LinePK02, 'CA', @InvoicePK, 1),
	(@LinePK03, 'CA', @InvoicePK, 1)

INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData)
VALUES
	(NEWID(), @LinePK01, 'JI', @Type, @ProgramType + 'ProgramInd=Y'),
	(NEWID(), @LinePK02, 'JI', @Type, @ProgramType + 'ProgramInd=N'),
	(NEWID(), @LinePK03, 'JI', @Type, 'ZZZProgramInd=Y')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK01", SqlDbType.UniqueIdentifier, linePK01);
				command.AddParameter("@LinePK02", SqlDbType.UniqueIdentifier, linePK02);
				command.AddParameter("@LinePK03", SqlDbType.UniqueIdentifier, linePK03);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddParameter("@ProgramType", SqlDbType.VarChar, programIndType);
				command.ExecuteNonQuery();
			}

			AssertData("Y Indicator", linePK01, (columnName, "Y"));
			AssertData("N Indicator", linePK02, (columnName, "N"));
			AssertData("N/A Indicator", linePK03, (columnName, "N/A"));
		}

		void AssertData(string messsage, Guid linePK, params (string columnName, object value)[] pgaValues)
		{
			var sql = @"SELECT * FROM dbo.IsCAPGAIndicated(@LinePK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(messsage, () =>
					{
						Assert(reader.Read());
						foreach ((string columnName, object value) in pgaValues)
						{
							AssertEquals("columnName", value, reader[columnName]);
						}
						Assert(!reader.Read());
					});
				}
			}
		}
	}
}
