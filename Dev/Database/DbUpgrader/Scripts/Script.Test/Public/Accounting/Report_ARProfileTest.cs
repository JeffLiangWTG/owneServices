using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_ARProfile))]
	class Report_ARProfileTest : DbCreateScriptTest
	{
		public void TestExternalDebtorCodeFallback()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("1stCarDiv", "1st Carrier Division");
			var orgCompanyDataPK = CreateOrgCompanyData(organisationPK);
			var sql = string.Format("Exec Report_ARProfile '{0}', N'Active', N'', N'1stCarDiv', NULL, NULL, NULL, NULL, NULL, N''", CompanyPK);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Number of results", 1, result.Rows.Count);
			AssertEquals("ExDebtorCode - empty EDR in Config & empty ARExternalDebtorCode", DBNull.Value, result.Rows[0]["ExDebtorCode"]);

			AddOK_CustomsRegNo_ExternalDebtorAccount(organisationPK, "wrongfallbackno1", "VN");
			AddOK_CustomsRegNo_ExternalDebtorAccount(organisationPK, "fallbackno");
			AddOK_CustomsRegNo_ExternalDebtorAccount(organisationPK, "wrongfallbackno2", "SG");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Number of results", 1, result.Rows.Count);
			AssertEquals("ExDebtorCode - should fallback to EDR in Config", "fallbackno", result.Rows[0]["ExDebtorCode"]);

			ChangeOB_ARExternalDebtorCode(orgCompanyDataPK, "APExternalCode");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Number of results", 1, result.Rows.Count);
			AssertEquals("ExDebtorCode - should use ARExternalDebtorCode", "APExternalCode", result.Rows[0]["ExDebtorCode"]);
		}

		Guid CreateOrgCompanyData(Guid organisationPK)
		{
			var result = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgCompanyData(OB_PK, OB_OH, OB_GC, OB_ISDebtor, OB_ARExternalDebtorCode)
VALUES (@OB_PK, @OB_OH, @OB_GC, @OB_ISDebtor, @OB_ARExternalDebtorCode)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@OB_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@OB_OH", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@OB_GC", SqlDbType.UniqueIdentifier, CompanyPK);
				command.AddParameter("@OB_ISDebtor", SqlDbType.Bit, true);
				command.AddParameter("@OB_ARExternalDebtorCode", SqlDbType.VarChar, string.Empty);
				command.ExecuteNonQuery();
			}

			return result;
		}

		void AddOK_CustomsRegNo_ExternalDebtorAccount(Guid organisationPK, string regNo)
		{
			AddOK_CustomsRegNo_ExternalDebtorAccount(organisationPK, regNo, CompanyCountryCode);
		}

		void AddOK_CustomsRegNo_ExternalDebtorAccount(Guid organisationPK, string regNo, string countryCode)
		{
			var sql = @"
INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH)
VALUES (newid(), @OK_CustomsRegNo, @OK_CodeType, @OK_RN_NKCodeCountry, @OK_OH)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@OK_CustomsRegNo", SqlDbType.VarChar, regNo);
				command.AddParameter("@OK_CodeType", SqlDbType.VarChar, "EDR");
				command.AddParameter("@OK_RN_NKCodeCountry", SqlDbType.VarChar, countryCode);
				command.AddParameter("@OK_OH", SqlDbType.UniqueIdentifier, organisationPK);
				command.ExecuteNonQuery();
			}
		}

		void ChangeOB_ARExternalDebtorCode(Guid pk, string code)
		{
			var sql = @"
UPDATE dbo.OrgCompanyData
SET OB_ARExternalDebtorCode = @OB_ARExternalDebtorCode
WHERE OB_PK = @OB_PK";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@OB_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@OB_ARExternalDebtorCode", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CompanyCountryCode = "AU";
			CompanyPK = TestDataCreator.CreateCompany("COR", CompanyCountryCode, "AUD");
		}
		Guid CompanyPK;
		string CompanyCountryCode;
	}
}

