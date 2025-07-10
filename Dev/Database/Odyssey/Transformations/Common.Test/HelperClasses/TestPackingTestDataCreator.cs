using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	public static class TestPackingTestDataCreator
	{
		#region CreateDummyParent

		public static Guid CreateDummyParent(DbConnection connection)
		{
			var pk = Guid.NewGuid();

			using (var command = connection.Command("INSERT INTO dbo.DummyBizo (Z0_PK) VALUES (@PK)"))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, DummyBizoSchema.PK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreatePackageJob

		public static Guid CreatePackageJob(DbConnection connection, string packageJobID, Guid? parent = null, string parentTable = DummyBizoSchema.Constants.Prefix)
		{
			var pk = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_JobID, KJ_ParentID, KJ_ParentTableCode)
VALUES (@PK, @JobId, @ParentID, @ParentTableCode)
";
			parent = parent ?? CreateDummyParent(connection);

			using (var command = connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, PkgPackageJobSchema.PK);
				command.AddParameterBasedOnDbColumn("@JobId", packageJobID, PkgPackageJobSchema.KJ_JobID);
				command.AddParameterBasedOnDbColumn("@ParentID", parent.Value, PkgPackageJobSchema.KJ_ParentID);
				command.AddParameterBasedOnDbColumn("@ParentTableCode", parentTable, PkgPackageJobSchema.KJ_ParentTableCode);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreatePackage

		public static Guid CreatePackage(DbConnection connection, Guid packageJob, string packageID, Guid? parentPackage = null, int packQty = 1, string packType = "PLT")
		{
			var pk = Guid.NewGuid();
			var packageIdPK = string.IsNullOrEmpty(packageID) ? Guid.Empty : Guid.NewGuid();
			if (!string.IsNullOrEmpty(packageID))
			{
				var sqlPackageID = "INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser) VALUES (@KPH_PK, @KPH_PackageID, GetUtcDate(), '~BP')";
				using (var command = connection.Command(sqlPackageID))
				{
					command.AddParameterBasedOnDbColumn("@KPH_PK", packageIdPK, PkgPackageHeaderSchema.PK);
					command.AddParameterBasedOnDbColumn("@KPH_PackageID", packageID, PkgPackageHeaderSchema.KPH_PackageID);
					command.ExecuteNonQuery();
				}
			}

			var sqlPackage = @"
INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_KPH_PackageHeader, KP_KP_ParentPackage, KP_PackageQty, KP_F3_NKPackType, KP_Sequence)
VALUES (@PK, @PackageJob, @PackageID, @ParentPackage, @PackageQty, @PackType, 1)";

			using (var command = connection.Command(sqlPackage))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, PkgPackageSchema.PK);
				command.AddParameterBasedOnDbColumn("@PackageJob", packageJob == Guid.Empty ? DBNull.Value : packageJob, PkgPackageSchema.KP_KJ_ParentPackageJob);
				command.AddParameterBasedOnDbColumn("@PackageID", packageIdPK == Guid.Empty ? DBNull.Value : packageIdPK, PkgPackageSchema.KP_KPH_PackageHeader);
				command.AddParameterBasedOnDbColumn("@ParentPackage", (object)parentPackage ?? DBNull.Value, PkgPackageSchema.KP_KP_ParentPackage);
				command.AddParameterBasedOnDbColumn("@PackageQty", packQty, PkgPackageSchema.KP_PackageQty);
				command.AddParameterBasedOnDbColumn("@PackType", packType, PkgPackageSchema.KP_F3_NKPackType);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreatePackageWithNoPackageJob(DbConnection connection, Guid parentPackage = new Guid())
		{
			return CreatePackage(connection, Guid.Empty, "", parentPackage: parentPackage);
		}

		#endregion

		#region CreatePackageContainer

		public static Guid CreatePackageContainer(DbConnection connection, Guid pkgPackage)
		{
			var pk = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.PkgPackageContainer (K0_PK, K0_KP_Package, K0_RC_ContainerType)
VALUES (@PK, @PkgPackage, @ContainerType)";

			using (var command = connection.Command(sql))
			{
				var containerType = connection.ExecuteScalar("SELECT TOP 1 RC_PK FROM dbo.RefContainer");
				command.AddParameterBasedOnDbColumn("@PK", pk, PkgPackageContainerSchema.PK);
				command.AddParameterBasedOnDbColumn("@PkgPackage", pkgPackage, PkgPackageContainerSchema.K0_KP_Package);
				command.AddParameterBasedOnDbColumn("@ContainerType", containerType, PkgPackageContainerSchema.K0_RC_ContainerType);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreatePackageItemDivot

		public static Guid CreatePackageItemDivot(DbConnection connection, Guid package, Guid packedItem, string packedItemTableCode, decimal packedQty)
		{
			var pk = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.PkgPackageItemDivot (KI_PK, KI_KP_Package, KI_ParentID, KI_ParentTableCode, KI_PackedQty)
VALUES (@PK, @Package, @PackedItem, @PackedItemTableCode, @PackedQty)";

			using (var command = connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, PkgPackageItemDivotSchema.PK);
				command.AddParameterBasedOnDbColumn("@PackedQty", packedQty, PkgPackageItemDivotSchema.KI_PackedQty);
				command.AddParameterBasedOnDbColumn("@Package", package, PkgPackageItemDivotSchema.KI_KP_Package);
				command.AddParameterBasedOnDbColumn("@PackedItem", packedItem, PkgPackageItemDivotSchema.KI_ParentID);
				command.AddParameterBasedOnDbColumn("@PackedItemTableCode", packedItemTableCode, PkgPackageItemDivotSchema.KI_ParentTableCode);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion
	}
}
