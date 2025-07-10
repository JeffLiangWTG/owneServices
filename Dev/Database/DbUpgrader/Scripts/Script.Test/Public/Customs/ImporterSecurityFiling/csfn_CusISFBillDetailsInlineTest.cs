using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ImporterSecurityFiling
{
	[TestedType(typeof(csfn_CusISFBillDetailsInline))]
	class csfn_CusISFBillDetailsInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_CusISFBillDetailsInline()
		{
			var bFPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES(@CompanyPK, 'US', 'USD', 'USC', 'US company')
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, @CompanyPK, 'USB', 'US')
INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference)
VALUES
 (@BF_PK, 'CT', '1', @BranchPK, '01', '11', '2023-05-10', 'ISF000001')

INSERT INTO dbo.CusISFBill(BB_PK, BB_BF, BB_BillNum, BB_BillType)
VALUES
 (NEWID(), @BF_PK, 'DLFFGYESAN010746', 'OB'),
 (NEWID(), @BF_PK, 'DLFFGYESAN010747', 'OB'),
 (NEWID(), @BF_PK, 'DLFFGYESAN010748', 'OB'),
 (NEWID(), @BF_PK, 'DLFFGYESAN010749', 'OB'),
 (NEWID(), @BF_PK, 'DLFFGYESAN010750', 'OB')
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@BF_PK", SqlDbType.UniqueIdentifier, bFPK);
				command.ExecuteNonQuery();
			}

			var value = "";
			Db.Connection.ExecuteReader($@"SELECT Value FROM csfn_CusISFBillDetailsInline('{bFPK}', 'OB', 3)", reader =>
			{
				value = reader.GetString(0);
			});

			AssertEquals("Value", "DLFFGYESAN010746, DLFFGYESAN010747, DLFFGYESAN010748, THERE MORE DATA", value);
		}
	}
}
