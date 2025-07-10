using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateYardUnitDetailsForMNRWorkOrderHeader : DataTransformation
	{
		public override string UserDescription => "Add and yard unit details for M&R Work Order";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, MNRWorkOrderHeaderSchema.Constants.TableName)
				&& !DbObjectCreator.ColumnExists(Db.Connection, MNRWorkOrderHeaderSchema.Constants.TableName, MNRWorkOrderHeaderSchema.Constants.MWO_ParentID)
				)
			{
				var addColumnsSql = @"
ALTER TABLE dbo.[MNRWorkOrderHeader]
ADD 
    MWO_ParentID UNIQUEIDENTIFIER 
";

				Db.Connection.ExecuteNonQuery(addColumnsSql);

	var addDataSql = @"
DECLARE @CurrentUtcDate DATETIME;
SET @CurrentUtcDate = GetUtcDate();

DECLARE @YardUnitPK UNIQUEIDENTIFIER;
SELECT TOP 1 @YardUnitPK = YUS_PK FROM dbo.CYDYardUnitState
Where YUS_YRL_ReceiveLine is not null;

UPDATE dbo.MNRWorkOrderHeader
SET
    MWO_ParentID = @YardUnitPK,
    MWO_ParentTableCode = 'YUS',
    MWO_SystemLastEditTimeUtc = @CurrentUtcDate,
    MWO_SystemLastEditUser = '~BP'
FROM dbo.MNRWorkOrderHeader
WHERE MWO_ParentID IS NULL;
";

				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}
