using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_APProfile))]
	class Report_APProfileTest : DbCreateScriptTest
	{
		public void TestAPProfile_APPaymentMethod()
		{
			CreateOrganizationWithAccounts();

			var sql = string.Format("SELECT AccountName, CompanyPK FROM Report_APProfile('DEF',null,'') WHERE CompanyPK = '{0}' ORDER BY AccountName", CompanyPK.ToString());
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals(5, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"AP-CHQ-Acc",
					"AP-DDR-Acc",
					"AP-DEF-Acc",
					"AP-END-Acc",
					"AP-EPO-Acc"
				},
				result.AsEnumerable().Select(row => row["AccountName"].ToString()));
		}

		public void TestAPProfile_IncludeOrganizationWithoutAccount()
		{
			CreateOrganizationWithAccounts();
			var organisationWithoutAccount = TestDataCreator.CreateOrganisation("2ndCarDiv", "2nd Carrier Division");
			CreateOrgCompanyData(organisationWithoutAccount);

			var sql = string.Format("SELECT AccountName, CompanyPK FROM Report_APProfile('DEF',null,'') WHERE CompanyPK = '{0}' ORDER BY AccountName", CompanyPK.ToString());
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(6, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"AP-CHQ-Acc",
					"AP-DDR-Acc",
					"AP-DEF-Acc",
					"AP-END-Acc",
					"AP-EPO-Acc",
					""
				},
				result.AsEnumerable().Select(row => row["AccountName"].ToString()));
		}

		void CreateOrganizationWithAccounts()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("1stCarDiv", "1st Carrier Division");
			var orgCompanyDataPK = CreateOrgCompanyData(organisationPK);

			AddAccAccountDetails(orgCompanyDataPK, "AR-TAX-Acc", "AR-TAX-Bank", "AR-TAX-BankAcc", "TAX");
			AddAccAccountDetails(orgCompanyDataPK, "AR-CRQ-Acc", "AR-CRQ-Bank", "AR-CRQ-BankAcc", "CRQ");
			AddAccAccountDetails(orgCompanyDataPK, "AP-DEF-Acc", "AP-DEF-Bank", "AP-DEF-BankAcc", "DEF");
			AddAccAccountDetails(orgCompanyDataPK, "AP-CHQ-Acc", "AP-CHQ-Bank", "AP-CHQ-BankAcc", "CHQ");
			AddAccAccountDetails(orgCompanyDataPK, "AP-DDR-Acc", "AP-DDR-Bank", "AP-DDR-BankAcc", "DDR");
			AddAccAccountDetails(orgCompanyDataPK, "AP-END-Acc", "AP-END-Bank", "AP-END-BankAcc", "END");
			AddAccAccountDetails(orgCompanyDataPK, "AP-EPO-Acc", "AP-EPO-Bank", "AP-EPO-BankAcc", "EPO");
		}

		Guid CreateOrgCompanyData(Guid organisationPK)
		{
			var result = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgCompanyData(OB_PK, OB_OH, OB_GC, OB_IsCreditor, OB_ARExternalDebtorCode)
VALUES (@OB_PK, @OB_OH, @OB_GC, @OB_IsCreditor, @OB_ARExternalDebtorCode)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@OB_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@OB_OH", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@OB_GC", SqlDbType.UniqueIdentifier, CompanyPK);
				command.AddParameter("@OB_IsCreditor", SqlDbType.Bit, true);
				command.AddParameter("@OB_ARExternalDebtorCode", SqlDbType.VarChar, string.Empty);
				command.ExecuteNonQuery();
			}

			return result;
		}

		void AddAccAccountDetails(Guid orgCompanyDataPK, string accountName, string bankName, string bankAccount, string paymentMethod)
		{
			var sql = @"
INSERT INTO dbo.AccAPAccountDetails(A1_PK, A1_RX_NKAccountCurrency, A1_AccountName, A1_BankName, A1_BankAccount, A1_PaymentMethod, A1_OB, A1_BankBsb, A1_SystemCreateTimeUtc, A1_SystemCreateUser, A1_SystemLastEditTimeUtc, A1_SystemLastEditUser)
VALUES (@A1_PK, 'AUD', @A1_AccountName, @A1_BankName, @A1_BankAccount, @A1_PaymentMethod, @A1_OB, 'BankBsb', '2022-06-16 00:00:00', 'E', '2022-06-16 00:00:00', 'E')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@A1_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@A1_AccountName", SqlDbType.VarChar, accountName);
				command.AddParameter("@A1_BankName", SqlDbType.VarChar, bankName);
				command.AddParameter("@A1_BankAccount", SqlDbType.VarChar, bankAccount);
				command.AddParameter("@A1_PaymentMethod", SqlDbType.VarChar, paymentMethod);
				command.AddParameter("@A1_OB", SqlDbType.UniqueIdentifier, orgCompanyDataPK);
				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = TestDataCreator.CreateCompany("COR", "AU", "AUD");
		}
		Guid CompanyPK;
	}
}

