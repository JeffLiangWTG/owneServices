using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateYardUnitDetailsForMNRSurvey : DataTransformation
	{
		public override string UserDescription => "Add yard unit details for M&R Survey";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, MNRSurveySchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, MNRSurveySchema.Constants.TableName, "MRS_YUS_YardUnitState")
				&& !DbObjectCreator.ColumnExists(Db.Connection, MNRSurveySchema.Constants.TableName, MNRSurveySchema.Constants.MRS_ParentID)
				&& !DbObjectCreator.ColumnExists(Db.Connection, MNRSurveySchema.Constants.TableName, MNRSurveySchema.Constants.MRS_WW_Facility)
				)
			{
				var addColumnsSql = @"
ALTER TABLE dbo.[MNRSurvey]
ADD 
    MRS_ParentID UNIQUEIDENTIFIER, 
    MRS_WW_Facility UNIQUEIDENTIFIER;
";

				Db.Connection.ExecuteNonQuery(addColumnsSql);

				var addDataSql = @"
DECLARE @CurrentUtcDate DATETIME;
SET @CurrentUtcDate = GetUtcDate();

UPDATE dbo.MNRSurvey
SET
    MRS_ParentID = YUS_PK,
    MRS_WW_Facility = YRA.YRA_WW_Yard,
    MRS_SystemLastEditTimeUtc = @CurrentUtcDate,
    MRS_SystemLastEditUser = '~BP'
FROM dbo.MNRSurvey MRS
	JOIN dbo.CYDYardUnitState YUS ON YUS.YUS_PK = MRS.MRS_YUS_YardUnitState
	JOIN dbo.CYDReceiveAdviceLine YRL ON YRL.YRL_PK = YUS.YUS_YRL_ReceiveLine
	JOIN dbo.CYDReceiveAdvice YRA ON YRA.YRA_PK = YRL.YRL_YRA_ReceiveAdvice
WHERE MRS_ParentID IS NULL;
";

				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}
