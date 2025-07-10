using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.Testing
{
	[TestedType(typeof(USImportEntryLineFee))]
	class USImportEntryLineFeeTest : DbCreateScriptTest
	{
		public void TestEndToEnd()
		{
			var clusterKey = 1;
			var usEntryLinePK = CreateCusEntryLine("US", clusterKey);
			var usEntryLineFee1PK = CreateCusEntryLineFee(usEntryLinePK, "K99", 150m, false, clusterKey);
			var usEntryLineFee2PK = CreateCusEntryLineFee(usEntryLinePK, "K01", 300m, true, clusterKey);
			clusterKey = 2;
			var auEntryLinePK = CreateCusEntryLine("AU", clusterKey);
			var auEntryLineFeePK = CreateCusEntryLineFee(auEntryLinePK, "K99", 200m, true, clusterKey);
			var sql = @"SELECT * FROM dbo.USImportEntryLineFee WHERE USF_ChargeType LIKE 'K%'";
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				var actualColumns = USImportEntryLineFeeSchema.All.OfType<SchemaColumn>().Select(x => x.Name).ToList();
				AssertEquals("reader.FieldCount", actualColumns.Count, reader.FieldCount);
				var columns = new[]
				{
					USImportEntryLineFeeSchema.Constants.PK, USImportEntryLineFeeSchema.Constants.USF_ChargeAmount,
					USImportEntryLineFeeSchema.Constants.USF_ChargeType, USImportEntryLineFeeSchema.Constants.USF_CL,
					USImportEntryLineFeeSchema.Constants.USF_IsLandedCostOnly
				};
				AssertEquals("columns.Length", actualColumns.Count, columns.Length);
				foreach (var column in columns)
				{
					Assert($"Removing column {column}", actualColumns.Remove(column));
				}
				if (actualColumns.Count > 0)
				{
					Fail("These columns are not tested:\r\n" + string.Join("\r\n", actualColumns));
				}
				var fees = new Dictionary<Guid, (Guid USF_CL, string USF_ChargeType, decimal USF_ChargeAmount, bool USF_IsLandedCostOnly)>();
				while (reader.Read())
				{
					fees.Add((Guid)reader["USF_PK"], ((Guid)reader["USF_CL"], (string)reader["USF_ChargeType"], (decimal)reader["USF_ChargeAmount"], (bool)reader["USF_IsLandedCostOnly"]));
				}
				AssertEquals(3, fees.Count);
				AssertCusEntryLineFee(fees[usEntryLineFee1PK], usEntryLinePK, "K99", 150m, false);
				AssertCusEntryLineFee(fees[usEntryLineFee2PK], usEntryLinePK, "K01", 300m, true);
				AssertCusEntryLineFee(fees[auEntryLineFeePK], auEntryLinePK, "K99", 200m, true);
			}
		}

		void AssertCusEntryLineFee((Guid USF_CL, string USF_ChargeType, decimal USF_ChargeAmount, bool USF_IsLandedCostOnly) fee, Guid entryLinePK, string chargeType, decimal chargeAmount, bool isLandedCostOnly)
		{
			AssertEquals("USF_CL", entryLinePK, fee.USF_CL);
			AssertEquals("USF_ChargeType", chargeType, fee.USF_ChargeType);
			AssertEquals("USF_ChargeAmount", chargeAmount, fee.USF_ChargeAmount);
			AssertEquals("USF_IsLandedCostOnly", isLandedCostOnly, fee.USF_IsLandedCostOnly);
		}

		Guid CreateCusEntryLineFee(Guid entryLinePK, string chargeType, decimal chargeAmount, bool isLandedCostOnly, int clusterKey)
		{
			var entryLineFeePK = Guid.NewGuid();
			var entryLineFeeSql = @"
INSERT dbo.CusEntryLineFee (CF_PK, CF_CL, CF_ChargeType, CF_ChargeAmount, CF_IsLandedCostOnly, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser)
VALUES(@EntryLineFeePK, @EntryLinePK, @ChargeType, @ChargeAmount, @IsLandedCostOnly, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";

			using (var command = Db.Connection.Command(entryLineFeeSql))
			{
				command.AddParameter("@EntryLineFeePK", SqlDbType.UniqueIdentifier, entryLineFeePK);
				command.AddParameter("@EntryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@ChargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@ChargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@IsLandedCostOnly", SqlDbType.Bit, isLandedCostOnly);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryLineFeePK;
		}

		Guid CreateCusEntryLine(string countryCode, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var entryLineSql = @"
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) 
VALUES (@CompanyPK, @CountryCode + 'C', 'US company', @CountryCode, @CountryCode + 'D')

DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC)
VALUES (@BranchPK, @CountryCode + 'B', @CompanyPK)

DECLARE @DeclarationPK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
VALUES (@DeclarationPK, 'US', @CountryCode + 'DEC00001', @BranchPK, @CompanyPK, @ClusterKey)

DECLARE @EntryPK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@EntryPK, 'US', @DeclarationPK, 'ENS', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLinePK, 'US', @EntryPK, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";

			using (var command = Db.Connection.Command(entryLineSql))
			{
				command.AddParameter("@EntryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryLinePK;
		}
	}
}
