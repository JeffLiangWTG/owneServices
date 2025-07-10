using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintBP_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column BP_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DROP TABLE IF EXISTS #CusCAeMHMasterToDelete;
DROP TABLE IF EXISTS #CusCAeMHContainerToDelete;
DROP TABLE IF EXISTS #CusCAeMHHouseToDelete;

SELECT BP_PK
INTO #CusCAeMHMasterToDelete
FROM dbo.CusCAeMHMaster
WHERE BP_ParentTableCode NOT IN ('','JK');

SELECT BQ_PK
INTO #CusCAeMHContainerToDelete
FROM dbo.CusCAeMHContainer
WHERE
	BQ_BP_Master IN (SELECT BP_PK FROM #CusCAeMHMasterToDelete);

SELECT BW_PK
INTO #CusCAeMHHouseToDelete
FROM dbo.CusCAeMHHouse
WHERE
	BW_BP_Master IN (SELECT BP_PK FROM #CusCAeMHMasterToDelete);

DELETE FROM dbo.CusCAeMHHouseContainerPivot
WHERE
	BPA_BW_House IN (SELECT BW_PK FROM #CusCAeMHHouseToDelete)

DELETE FROM dbo.CusCAeMHHouseContainerPivot
WHERE
	BPA_BQ_Container IN (SELECT BQ_PK FROM #CusCAeMHContainerToDelete);

DELETE FROM dbo.CusCAeMHItem
WHERE
	BX_BW_House IN (SELECT BW_PK FROM #CusCAeMHHouseToDelete);

DELETE FROM dbo.CusCAeMHContainer
WHERE
	BQ_PK IN (SELECT BQ_PK FROM #CusCAeMHContainerToDelete);

DELETE FROM dbo.CusCAeMHHouse
WHERE
	BW_PK IN (SELECT BW_PK FROM #CusCAeMHHouseToDelete);

DELETE FROM dbo.CusCAeMHMaster
WHERE BP_PK IN (SELECT BP_PK FROM #CusCAeMHMasterToDelete);
");

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusCAeMHMasterSchema.Instance)
				.Key(CusCAeMHMasterSchema.Constants.BP_ParentID)
				.Include(CusCAeMHMasterSchema.Constants.PK)
				.Include(CusCAeMHMasterSchema.Constants.BP_SystemCreateTimeUtc)
				.Include(CusCAeMHMasterSchema.Constants.BP_SystemLastEditTimeUtc)
				.Where(@"[BP_ParentTableCode]<>'' AND [BP_ParentTableCode]<>'JK'")
				.GetInfo();

				indexProvider.New(CusCAeMHHouseContainerPivotSchema.Instance)
				.Key(CusCAeMHHouseContainerPivotSchema.Constants.BPA_BW_House)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_BQ_Container)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_SystemCreateTimeUtc)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_SystemLastEditTimeUtc)
				.GetInfo();

				indexProvider.New(CusCAeMHHouseContainerPivotSchema.Instance)
				.Key(CusCAeMHHouseContainerPivotSchema.Constants.BPA_BQ_Container)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_BW_House)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_SystemCreateTimeUtc)
				.Include(CusCAeMHHouseContainerPivotSchema.Constants.BPA_SystemLastEditTimeUtc)
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
