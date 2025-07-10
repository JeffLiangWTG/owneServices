using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class AddAuditColumnsToEdiTokenAuthOnBoardingDataTransformation : DataTransformation
	{
		public override string UserDescription => "Add Audit columns for EdiTokenAuthOnBoardingData";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, TableName))
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, TableName, "TOD_SystemCreateTimeUtc", "smalldatetime", DefaultTableCreateTime);
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, TableName, "TOD_SystemCreateUser", "VARCHAR(3)", "'~BP'");
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, TableName, "TOD_SystemLastEditTimeUtc", "smalldatetime", DefaultTableCreateTime);
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, TableName, "TOD_SystemLastEditUser", "VARCHAR(3)", "'~BP'");

				var sqlText = $@"
UPDATE OnBoardingData
SET
	TOD_SystemCreateTimeUtc   = ISNULL(Logs.CreateTime, TOD_SystemCreateTimeUtc),
	TOD_SystemCreateUser      = ISNULL(Logs.CreateUser, '~BP'),
	TOD_SystemLastEditTimeUtc = ISNULL(Logs.LastEditTime, TOD_SystemLastEditTimeUtc),
	TOD_SystemLastEditUser    = ISNULL(Logs.LastEditUser, '~BP')
FROM
	EdiTokenAuthOnBoardingData AS OnBoardingData
	OUTER APPLY
	(
		SELECT TOP (1)
			CreateTime   = FIRST_VALUE(IIF(SL_SE_NKEvent = 'ADD', SL_PostedTimeUtc, NULL)) OVER (ORDER BY SL_PostedTimeUtc),
			CreateUser   = FIRST_VALUE(IIF(SL_SE_NKEvent = 'ADD', SL_GS_NKUser    , NULL)) OVER (ORDER BY SL_PostedTimeUtc),
			LastEditTime = FIRST_VALUE(SL_PostedTimeUtc) OVER (ORDER BY SL_PostedTimeUtc DESC),
			LastEditUser = FIRST_VALUE(SL_GS_NKUser    ) OVER (ORDER BY SL_PostedTimeUtc DESC)
		FROM
			StmALog WITH (FORCESEEK, INDEX([NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime]))
		WHERE
			SL_Parent = OnBoardingData.TOD_PK
	) AS Logs
WHERE
	OnBoardingData.TOD_SystemLastEditTimeUtc = {DefaultTableCreateTime}
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

		const string TableName = "EdiTokenAuthOnBoardingData";

		const string DefaultTableCreateTime = "'2023-04-21 00:00:00'";
	}
}
