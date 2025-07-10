using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Testing
{
	[TestedType(typeof(GetCommissionAgreementPrimaryKeys))]
	class GetCommissionAgreementPrimaryKeysTest : DbCreateScriptTest
	{
		public void TestGetKeys()
		{
			SetupTestData();
			CreateEffectiveDateCacheTable();

			var pks = CallFunction(string.Empty, new DateTime(2023, 2, 2));

			var expectedAgreements = new Guid[] { agreement1Pk, agreement3Pk, agreement4Pk };
			AssertContainsExactElementsInAnyOrder(expectedAgreements, pks);

			pks = CallFunction(string.Empty, new DateTime(2023, 3, 2));

			expectedAgreements = new Guid[] { agreement1Pk, agreement4Pk };
			AssertContainsExactElementsInAnyOrder(expectedAgreements, pks);

			pks = CallFunction(null, new DateTime(2021, 3, 2));

			expectedAgreements = new Guid[] { agreement1Pk, agreement3Pk };
			AssertContainsExactElementsInAnyOrder(expectedAgreements, pks);

			pks = CallFunction("AAA", new DateTime(2023, 1, 1));

			expectedAgreements = new Guid[] { agreement2Pk };
			AssertContainsExactElementsInAnyOrder(expectedAgreements, pks);
		}

		List<Guid> CallFunction(string stream, DateTime commissionDate)
		{
			var pks = new List<Guid>();

			using (var command = TestConnection.Command("SELECT VCA_PK FROM dbo.GetCommissionAgreementPrimaryKeys(@OpportunityClientPk, @CustomerPk, @CommissionStream, @CommissionDate, @EffectiveDateCacheTable)")) // Avoid to load all BOs
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@OpportunityClientPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CustomerPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@CommissionStream", SqlDbType.VarChar, 3, stream == null ? DBNull.Value : stream);
				command.AddParameter("@CommissionDate", SqlDbType.DateTime, commissionDate);
				command.AddTableValuedParameter("@EffectiveDateCacheTable", "dbo.TVP_TriggerTypeEffectiveDate", effectiveDateCacheTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						pks.Add((Guid)reader["VCA_PK"]);
					}
				}
			}
			return pks;
		}

		protected void CreateEffectiveDateCacheTable()
		{
			effectiveDateCacheTable = new DataTable();
			effectiveDateCacheTable.Columns.Add("TriggerType", typeof(string));
			effectiveDateCacheTable.Columns.Add("CustomerPk", typeof(Guid));
			effectiveDateCacheTable.Columns.Add("EffectiveDate", typeof(DateTime));

			var dataRow = effectiveDateCacheTable.NewRow();
			dataRow["TriggerType"] = "ERR";
			dataRow["CustomerPk"] = orgPk;
			dataRow["EffectiveDate"] = new DateTime(2021, 1, 1);
			effectiveDateCacheTable.Rows.Add(dataRow);

			dataRow = effectiveDateCacheTable.NewRow();
			dataRow["TriggerType"] = "1AR";
			dataRow["CustomerPk"] = orgPk;
			dataRow["EffectiveDate"] = new DateTime(2022, 1, 1);
			effectiveDateCacheTable.Rows.Add(dataRow);
		}

		void SetupTestData()
		{
			var insertSql = $@"
DECLARE @CompanyPk UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'TES', 'AU company', 'AU', 'AUD')

DECLARE @OrgPk UNIQUEIDENTIFIER = '{orgPk}';
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort)
VALUES (@OrgPk, 'TESTORGAAA', 'Test Organisation AAA', 'NZAKL')

DECLARE @OpportunityPk UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_EstimatedCloseDate, P8_ClosedDate, P8_EstimatedValue, P8_RX_NKEstimatedValueCurrency, P8_LostReason, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser, P8_Outcome, P8_PackageType, P8_Source, P8_SourceDetails, P8_DiscountAmount, P8_RentalMultiplier)
VALUES (@OpportunityPk, 'O01009999', @OrgPk, @CompanyPk, 'XXX', 'A1', 'WON', 'Test Opportunity 1', 'SS1', '2020-07-15', '2020-08-26', 2000, 'AUD', 'PRO', '2019-10-15 11:10', 'E', '2019-10-15 13:20', 'E', 'OUT', 'AAA', 'OTH', 'Test Details', 19, 83)

DECLARE @OrgMiscServPk UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_CMClientCommenced) VALUES (@OrgMiscServPk, @OrgPk, '2021-01-01')

DECLARE @Agreement1Pk UNIQUEIDENTIFIER = '{agreement1Pk}';
DECLARE @Agreement2Pk UNIQUEIDENTIFIER = '{agreement2Pk}';
DECLARE @Agreement3Pk UNIQUEIDENTIFIER = '{agreement3Pk}';
DECLARE @Agreement4Pk UNIQUEIDENTIFIER = '{agreement4Pk}';
DECLARE @Agreement5Pk UNIQUEIDENTIFIER = '{agreement5Pk}';

INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement1Pk, '#1', @OpportunityPk, @OrgPk, 'ERR', 'PRF', '2020-01-11 17:43', NULL, NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', '')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement2Pk, '#2', @OpportunityPk, @OrgPk, '1AR', 'REV', '2020-02-02 07:11', NULL, NULL, '2020-02-02 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement3Pk, '#3', @OpportunityPk, @OrgPk, 'CCD', 'PRF', '2020-01-11 17:43', '2023-02-17', NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', '')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement4Pk, '#4', @OpportunityPk, @OrgPk, 'MAN', '2022-01-01', 'PRF', '2020-01-11 17:43', '2023-03-17', NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', '')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement5Pk, '#5', @OpportunityPk, @OrgPk, 'ERR', 'PRF', NULL, NULL, NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', '')
";

			TestConnection.ExecuteNonQuery(insertSql);
		}

		readonly Guid orgPk = Guid.NewGuid();
		readonly Guid agreement1Pk = Guid.NewGuid();
		readonly Guid agreement2Pk = Guid.NewGuid();
		readonly Guid agreement3Pk = Guid.NewGuid();
		readonly Guid agreement4Pk = Guid.NewGuid();
		readonly Guid agreement5Pk = Guid.NewGuid();
		DataTable effectiveDateCacheTable;
	}
}
