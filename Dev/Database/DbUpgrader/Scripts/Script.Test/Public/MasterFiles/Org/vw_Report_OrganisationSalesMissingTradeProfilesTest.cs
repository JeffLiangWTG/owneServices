using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org
{
	[TestedType(typeof(vw_Report_OrganisationSalesMissingTradeProfiles))]
	class vw_Report_OrganisationSalesMissingTradeProfilesTest : DbCreateScriptTest
	{
		public void TestViewShouldReturnOrganisationsWithMissingTradeProfiles()
		{
			var companyPk = InsertTestCompany();
			InsertTestOrganisation(companyPk, "SALEHAS", true, true, true);
			InsertTestOrganisation(companyPk, "SALEMISS1", true, true, false);
			InsertTestOrganisation(companyPk, "SALEMISS2", true, true, false);
			InsertTestOrganisation(companyPk, "NONSALE", true, false, false);

			var orgsWithMissingTradeProfiles = GetOrganisationsWithMissingTradeProfiles(companyPk);

			AssertMultilineASCIIEquals("Query should return organisations with missing trade profiles",
@"SALEMISS1
SALEMISS2",
				string.Join(System.Environment.NewLine, orgsWithMissingTradeProfiles));
		}

		Guid InsertTestCompany()
		{
			var companyPk = Guid.NewGuid();

			using (var sqlCmd = Db.Connection.Command(@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'DAN', 'AU company', 'AU', 'AUD')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GC_PK", companyPk, GlbCompanySchema.PK);
				sqlCmd.ExecuteNonQuery();
			}

			return companyPk;
		}

		void InsertTestOrganisation(Guid companyPk, string code, bool isActive, bool isSalesLead, bool hasTradeProfile)
		{
			var oH_PK = Guid.NewGuid();
			using (var sqlCmd = Db.Connection.Command(@"INSERT INTO dbo.OrgHeader ([OH_PK] ,[OH_Code] ,[OH_IsActive] ,[OH_IsSalesLead] ,[OH_FullName])
VALUES (@OH_PK, @OH_Code, @OH_IsActive, @OH_IsSalesLead, 'TestOrganisation')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@OH_PK", oH_PK, OrgHeaderSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				sqlCmd.AddParameterBasedOnDbColumn("@OH_IsActive", isActive, OrgHeaderSchema.OH_IsActive);
				sqlCmd.AddParameterBasedOnDbColumn("@OH_IsSalesLead", isSalesLead, OrgHeaderSchema.OH_IsSalesLead);
				sqlCmd.ExecuteNonQuery();
			}

			using (var sqlCmd = Db.Connection.Command(@"INSERT INTO dbo.OrgCompanyData (OB_PK, OB_OH, OB_GC, OB_ARPreviousChequeDrawer) VALUES (@OB_PK, @OB_OH, @OB_GC, @OB_ARPreviousChequeDrawer)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@OB_PK", Guid.NewGuid(), OrgCompanyDataSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@OB_OH", oH_PK, OrgCompanyDataSchema.OB_OH);
				sqlCmd.AddParameterBasedOnDbColumn("@OB_GC", companyPk, OrgCompanyDataSchema.OB_GC);
				sqlCmd.AddParameterBasedOnDbColumn("@OB_ARPreviousChequeDrawer", "TestOrgCompanData", OrgCompanyDataSchema.OB_ARPreviousChequeDrawer);
				sqlCmd.ExecuteNonQuery();
			}

			if (hasTradeProfile)
			{
				var originPk = Guid.Empty;
				using (var sqlCmd = Db.Connection.Command("SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = 'BABBR'"))
				{
					originPk = (Guid)sqlCmd.ExecuteScalar();
				}

				var destinationPk = Guid.Empty;
				using (var sqlCmd = Db.Connection.Command("SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = 'CAAAC'"))
				{
					destinationPk = (Guid)sqlCmd.ExecuteScalar();
				}

				using (var sqlCmd = Db.Connection.Command(@"INSERT INTO dbo.OrgSales ([OW_PK] ,[OW_OH_Supplier] ,[OW_OH_Buyer] ,[OW_OriginID] ,[OW_OriginTableCode] ,[OW_DestinationID] ,[OW_DestinationTableCode] ,[OW_SystemCreateTimeUtc] ,[OW_SystemCreateUser] ,[OW_SystemLastEditTimeUtc] ,[OW_SystemLastEditUser])
VALUES (@OW_PK, @OW_OH_Supplier, NULL, @OW_OriginID, 'RL', @OW_DestinationID, 'RL', GetUtcDate(), 'E', GetUtcDate(), 'E')"))
				{
					sqlCmd.AddParameterBasedOnDbColumn("@OW_PK", Guid.NewGuid(), OrgSalesSchema.PK);
					sqlCmd.AddParameterBasedOnDbColumn("@OW_OH_Supplier", oH_PK, OrgSalesSchema.OW_OH_Supplier);
					sqlCmd.AddParameterBasedOnDbColumn("@OW_OriginID", originPk, OrgSalesSchema.OW_OriginID);
					sqlCmd.AddParameterBasedOnDbColumn("@OW_DestinationID", destinationPk, OrgSalesSchema.OW_DestinationID);
					sqlCmd.ExecuteNonQuery();
				}
			}
		}

		IList<string> GetOrganisationsWithMissingTradeProfiles(Guid companyPk)
		{
			var result = new List<string>();

			using (var command = Db.Connection.Command(
@"SELECT OrgCode
FROM dbo.vw_Report_OrganisationSalesMissingTradeProfiles
WHERE CompanyPK = @CompanyPK
ORDER BY OrgCode"))
			{
				command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, companyPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader["OrgCode"].ToString());
					}
				}
			}

			return result;
		}
	}
}

