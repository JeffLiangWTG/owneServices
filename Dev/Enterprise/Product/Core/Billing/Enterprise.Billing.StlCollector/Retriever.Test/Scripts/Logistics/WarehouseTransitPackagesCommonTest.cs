using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	sealed class WarehouseTransitPackagesCommonTest : TestCaseWithFactory
	{
		public static void AssertRowSet(IEnumerable<IStlTransaction> transactions, int line, string companyCode, string branchCode, string userCode, int itemCount, string reference01, string reference02, DateTimeOffset transactionDateUtc, string reference03 = "", string reference04 = "")
		{
			var transaction = transactions.Single(t => t.Reference1 == reference01 && t.Reference2 == reference02);
			AssertEquals("[T" + line + "] CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals("[T" + line + "] BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals("[T" + line + "] UserCode", userCode, transaction.ClientStaffCode);
			AssertEquals("[T" + line + "] ItemCount", itemCount, transaction.BillableCount);
			AssertEquals("[T" + line + "] TransactionReference02", reference02, transaction.Reference2);
			if (!string.IsNullOrEmpty(reference03))
			{
				AssertEquals("[T" + line + "] TransactionReference03", reference03, transaction.Reference3);
			}
			if (!string.IsNullOrEmpty(reference04))
			{
				AssertEquals("[T" + line + "] TransactionReference04", reference04, transaction.Reference4);
			}

			AssertEquals("[T" + line + "] TransactionDateUtc", transactionDateUtc, new DateTimeOffset(transaction.ServiceOccuredUTC));
		}

		/// <summary>
		/// If [WhsItemPackageState].[WPS_KP_Package] is not unique, packages will be counted more than once.
		/// </summary>
		public static void AssertTransitPackageStateFkToPackageIsUnique(DbConnection connection)
		{
			string sqlText = @"
				DECLARE @WwPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @WrcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @KjPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @KpiPkID1 UNIQUEIDENTIFIER = newid();
				DECLARE @KpPk01 UNIQUEIDENTIFIER = '47a221dc-c87d-4b21-b4b6-c361a16c9fce';
				INSERT dbo.WhsWarehouse (WW_PK, WW_WarehouseCode, WW_WarehouseType, WW_OA_WarehouseAddress, WW_GB_RelatedCompanyBranch, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WwPk01, 'WHS', 'TRW', (SELECT TOP 1 OA_PK FROM dbo.OrgAddress), (SELECT TOP 1 GB_PK FROM dbo.GlbBranch), '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_JobID, WRC_SystemCreateUser, WRC_SystemLastEditUser) VALUES
					(@WrcPk01, @WwPk01, 'WRC01', SYSDATETIME(), SYSDATETIME(), 'RC001', '~BP', '~BP');
				INSERT dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
					(@KjPk01, @WrcPk01, 'WRC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT INTO dbo.PkgPackageHeader(KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES
					(@KpiPkID1, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob,KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KpPk01, @KpiPkID1, @KjPk01, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_WRC_TransitReceiveConsignment, WPS_Status, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser) VALUES
					(newid(), @KpPk01, @WwPk01, @WrcPk01, 'BKD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KpPk01, @WwPk01, @WrcPk01, 'BKD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			AssertExceptionThrown(
				"[WhsItemPackageState].[WPS_KP_Package] is not unique, packages will be counted more than once",
				typeof(SqlException),
				"Cannot insert duplicate key row in object 'dbo.WhsItemPackageState' with unique index 'FK_UX__WPS_KP_Package'. The duplicate key value is (47a221dc-c87d-4b21-b4b6-c361a16c9fce).\r\nThe statement has been terminated.",
				() => connection.ExecuteNonQuery(sqlText));
		}

		public static void AssertScriptShouldUseDateTimeOffsetParameters(IStlScript script)
		{
			var parameters = script.GetInputParameters(AusydMonthRange.New(2017, 9));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.DateTimeOffset));
		}
	}
}
