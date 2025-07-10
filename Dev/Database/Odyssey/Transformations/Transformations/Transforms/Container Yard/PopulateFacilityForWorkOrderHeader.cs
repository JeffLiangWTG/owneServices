using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateFacilityForWorkOrderHeader : DataTransformation
	{
		public override string UserDescription => "Populate warehouse for MNRWorkOrderHeader";

		protected override void OfflinePreUpgradeTransform()
		{
			if (TableExists(Db.Connection, MNRWorkOrderHeaderSchema.Constants.TableName) && !DbObjectCreator.ColumnExists(Db.Connection, MNRWorkOrderHeaderSchema.Constants.TableName, "MWO_WW_Facility"))
			{
				var addFacilityColSql1 = @"
						ALTER TABLE dbo.[MNRWorkOrderHeader]
						ADD [MWO_WW_Facility] UNIQUEIDENTIFIER;
						";
				Db.Connection.ExecuteNonQuery(addFacilityColSql1);

				var populateFacilityYardCol = @"
						DECLARE @CurrentUtcDate DATETIME;
						SET @CurrentUtcDate = GetUtcDate();

						UPDATE dbo.MNRWorkOrderHeader
						SET
							MWO_WW_Facility = WW.WW_PK,
							MWO_SystemLastEditTimeUtc = @CurrentUtcDate,
							MWO_SystemLastEditUser = '~BP'
						FROM dbo.MNRWorkOrderHeader
							JOIN dbo.CYDYardUnitState YUS ON YUS.YUS_PK = dbo.MNRWorkOrderHeader.MWO_ParentId
							JOIN dbo.CYDReceiveAdviceLine YRL ON YRL.YRL_PK = YUS.YUS_YRL_ReceiveLine
							JOIN dbo.CYDReceiveAdvice YRA ON YRA.YRA_PK = YRL.YRL_YRA_ReceiveAdvice
							JOIN dbo.WhsWarehouse WW ON WW.WW_PK = YRA.YRA_WW_Yard;
						";
				Db.Connection.ExecuteNonQuery(populateFacilityYardCol);
			}
		}
	}
}
