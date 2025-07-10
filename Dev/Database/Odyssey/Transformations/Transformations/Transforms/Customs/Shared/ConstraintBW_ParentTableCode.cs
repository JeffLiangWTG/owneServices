using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class ConstraintBW_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column BW_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
DECLARE @DeleteBW_PKs TABLE
(
	BW_PK uniqueidentifier NOT NULL
)

INSERT INTO @DeleteBW_PKs
SELECT BW_PK FROM dbo.CusCAeMHHouse WHERE BW_ParentTableCode NOT IN (
	'',
	'HVC', -- HVLVConsignment
	'JS' -- JobShipment
)

IF NOT EXISTS (SELECT * FROM @DeleteBW_PKs)
	RETURN

DELETE FROM dbo.CusCAeMHHouseContainerPivot
WHERE BPA_BW_House IN (SELECT BW_PK FROM @DeleteBW_PKs)

DELETE FROM dbo.CusCAeMHItem
WHERE BX_BW_House IN (SELECT BW_PK FROM @DeleteBW_PKs)

DELETE FROM dbo.CusCAeMHHouse
WHERE BW_PK IN (SELECT BW_PK FROM @DeleteBW_PKs)";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusCAeMHHouseSchema.Instance)
					.Key(CusCAeMHHouseSchema.Constants.BW_ParentID)
					.Include(CusCAeMHHouseSchema.Constants.BW_SystemCreateTimeUtc)
					.Include(CusCAeMHHouseSchema.Constants.BW_SystemLastEditTimeUtc)
					.Where("[BW_ParentTableCode]<>'' AND [BW_ParentTableCode]<>'HVC' AND [BW_ParentTableCode]<>'JS'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
