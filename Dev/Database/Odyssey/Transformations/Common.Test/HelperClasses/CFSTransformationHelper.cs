#if DEBUG

using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformations.Testing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	[CodeAlive("Even though the underlying transformation has been removed due to our cleanup process, we don't need another dev spending time recreating this for future transforms only to delete when that gets removed and then potentially recreate again.")]
	public class CFSTransformationHelper
	{
		public Guid CreateCFSOrder(string tableName, Guid warehousePK, string jobNumber, string purpose = "STO", string transportMode = "SEA")
		{
			var cfsOrderPk = Guid.NewGuid();

			var sql = string.Format(CultureInfo.InvariantCulture, @"
						INSERT INTO {0} (FRO_PK, FRO_WW_Warehouse, FRO_JobNumber, FRO_Purpose, FRO_TransportMode, FRO_IsCancelled, FRO_SystemCreateTimeUtc, FRO_SystemCreateUser, FRO_SystemLastEditTimeUtc, FRO_SystemLastEditUser)
						VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', 0, '20160107', 'AAA', '20160107', 'BBB')",
						tableName, cfsOrderPk, warehousePK, jobNumber, purpose, transportMode);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			return cfsOrderPk;
		}

		public Guid CreatePackageContainer(string jobNumber)
		{
			var packageJob = TestPackingTestDataCreator.CreatePackageJob(Db.Connection, jobNumber);
			var packageOnPackageJob = TestPackingTestDataCreator.CreatePackage(Db.Connection, packageJob, "", packType: "CNT");
			var packageContainerPk = TestPackingTestDataCreator.CreatePackageContainer(Db.Connection, packageOnPackageJob);

			return packageContainerPk;
		}

		public Guid CreateCompany(string code, string name, string countryCode = "AU", string currencyCode = "AUD")
		{
			var companyPk = Guid.NewGuid();

			var sql = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
					VALUES ('{0}', '{1}', '{2}', '{3}', {4})",
					companyPk, code, name, countryCode, currencyCode);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			return companyPk;
		}

		public Guid CreateBranch(string code, string name, Guid company)
		{
			var branckPk = Guid.NewGuid();

			var sql = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC)
					VALUES ('{0}', '{1}', '{2}', '{3}')",
					branckPk, code, name, company);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			return branckPk;
		}

		public class OhAndOa
		{
			public readonly Guid Oa;
			public readonly Guid Oh;

			public OhAndOa(Guid orgHeaderPk, Guid addressPK)
			{
				this.Oh = orgHeaderPk;
				this.Oa = addressPK;
			}
		}

		public OhAndOa CreateOrgHeaderAndAddress(string orgHeaderCode, string orgHeaderPort, string addressPort, string addressCity)
		{
			var addressPK = Guid.NewGuid();
			var orgHeaderPk = Guid.NewGuid();

			var sql1 = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_RL_NKClosestPort)
					VALUES ('{0}', '{1}', '{2}')",
					orgHeaderPk, orgHeaderCode, orgHeaderPort);

			using (var cmd = Db.Connection.Command(sql1))
			{
				cmd.ExecuteNonQuery();
			}

			var sql2 = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_RL_NKRelatedPortCode, OA_City, OA_Address1)
					VALUES ('{0}', '{1}', '{2}', '{3}', 'Address 1')",
					addressPK, orgHeaderPk, addressPort, addressCity);

			using (var cmd = Db.Connection.Command(sql2))
			{
				cmd.ExecuteNonQuery();
			}
			return new OhAndOa(orgHeaderPk, addressPK);
		}

		public Guid CreateWarehouse(Guid branchPK, Guid addressPK, string warehouseCode)
		{
			var warehousePK = Guid.NewGuid();

			var sql = string.Format(CultureInfo.InvariantCulture, @"
					INSERT INTO dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_DefaultInboundDockDoor, WW_DefaultOutboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
					VALUES ('{0}', '{1}', '{2}', '{3}', NEWID(), NEWID(), (select top 1 WLT_PK from dbo.WhsLocationType), GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
					warehousePK, branchPK, addressPK, warehouseCode);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			return warehousePK;
		}
	}
}

#endif
