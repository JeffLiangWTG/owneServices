using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public sealed class UpdateJobCO2eStatusFromTEUStatus : DataTransformation
	{
		public override string UserDescription => "Migrate JobCO2e TEUStatus to Status";

		const string UpdateShipmentSQL = @"
UPDATE
  dbo.JobCO2e
SET
  JCO_Status = JCO_TEUStatus,
  JCO_SystemLastEditTimeUtc = GETUTCDATE(),
  JCO_SystemLastEditUser = '~BP'
WHERE
  JCO_ParentTableCode = 'JS'
  AND JCO_ParentID IN (
    SELECT
      JS.JS_PK
    FROM
      dbo.JobShipment JS
      JOIN dbo.JobConShipLink JN ON JS.JS_PK = JN.JN_JS
      JOIN dbo.JobConsol JK ON JN.JN_JK = JK.JK_PK
    WHERE
      EXISTS (
        -- check container TEU count > 0
        SELECT
          1
        FROM
          dbo.JobPackLines JL
          JOIN dbo.JobContainerPackPivot J6 ON JL.JL_PK = J6.J6_JL
          JOIN dbo.JobContainer JC ON J6.J6_JC = JC.JC_PK
          JOIN dbo.RefContainer RC ON JC.JC_RC = RC.RC_PK
        WHERE
          JL.JL_JS = JS.JS_PK
          AND JC.JC_JK = JK.JK_PK
          AND JL.JL_ActualWeight > 0
          AND RC.RC_TEU > 0
      )
      AND NOT EXISTS (
        -- check shipment packlines are all packed
        SELECT
          1
        FROM
          dbo.JobPackLines JL
          LEFT JOIN dbo.JobConShipLink JN ON JN.JN_JS = JL.JL_JS
          LEFT JOIN dbo.JobConsol JK ON JN.JN_JK = JK.JK_PK
          LEFT JOIN dbo.JobContainer JC ON JC.JC_JK = JK.JK_PK
          LEFT JOIN dbo.JobContainerPackPivot J6 ON J6.J6_JC = JC.JC_PK
          AND J6.J6_JL = JL.JL_PK
        WHERE
          JL.JL_JS = JS.JS_PK
        GROUP BY
          JL.JL_PK,
          JK.JK_PK
        HAVING
          COUNT(J6.J6_PK) = 0
      )
      AND JS.JS_IsForwardRegistered = 1
      AND JS.JS_TransportMode = 'SEA'
      AND JS.JS_PackingMode IN ('FCL', 'SCN', 'BCN')
    UNION ALL
    SELECT
      JS.JS_PK
    FROM
      dbo.JobShipment JS
      JOIN dbo.JobConShipLink JN ON JS.JS_PK = JN.JN_JS
      JOIN dbo.JobConsol JK ON JN.JN_JK = JK.JK_PK
      JOIN dbo.JobContainer JC ON JC.JC_JK = JK.JK_PK
    WHERE
      JS.JS_TransportMode IN ('ROA', 'RAI')
      AND JS.JS_PackingMode = 'FCL'
      AND JS.JS_ActualWeight = 0
  );
";

		const string UpdateConsolSQL = @"
UPDATE
  dbo.JobCO2e
SET
  JCO_Status = JCO_TEUStatus,
  JCO_SystemLastEditTimeUtc = GETUTCDATE(),
  JCO_SystemLastEditUser = '~BP'
WHERE
  JCO_ParentTableCode = 'JK'
  AND JCO_ParentID IN (
    SELECT
      JK.JK_PK
    FROM
      dbo.JobConsol JK
      JOIN dbo.JobContainer JC ON JK.JK_PK = JC.JC_JK
      JOIN dbo.RefContainer RC ON JC.JC_RC = RC.RC_PK
    WHERE
      -- check Consol has container and TEUCount > 0
      RC.RC_TEU > 0
      AND JK.JK_TransportMode = 'SEA'
      AND JK.JK_ConsolMode IN ('FCL', 'GRP', 'BCN')
    UNION ALL
    SELECT
      JK.JK_PK
    FROM
      dbo.JobConsol JK
      JOIN dbo.JobContainer JC ON JK.JK_PK = JC.JC_JK
      LEFT JOIN dbo.JobConShipLink JN ON JK.JK_PK = JN.JN_JK
      LEFT JOIN dbo.JobShipment JS ON JN.JN_JS = JS.JS_PK
    WHERE
      JK.JK_TransportMode IN ('ROA', 'RAI')
      AND JK.JK_ConsolMode = 'FCL'
    GROUP BY
      JK.JK_PK,
      JK.JK_TotalShipmentActWeightCheck
    HAVING
      -- check Consol Weight = 0 and Consol TotalShipmentWeight = 0
      JK.JK_TotalShipmentActWeightCheck = 0
      AND (
        COUNT(JS.JS_PK) = 0
        OR SUM(JS.JS_ActualWeight) = 0
      )
  );
";

		const string UpdateOneOffQuoteSQL = @"
UPDATE
  dbo.JobCO2e
SET
  JCO_Status = JCO_TEUStatus,
  JCO_SystemLastEditTimeUtc = GETUTCDATE(),
  JCO_SystemLastEditUser = '~BP'
WHERE
  JCO_ParentTableCode = 'VB'
  AND JCO_ParentID IN (
    SELECT
      TH.TH_PK
    FROM
      dbo.RatingHeader TH
      LEFT JOIN dbo.JobShipment JS ON TH.TH_PK = JS.JS_TH_OneTimeQuote
      JOIN dbo.RateOneOffShipment TT ON TH.TH_PK = TT.TT_TH
      JOIN dbo.RateOneOffContainers TC ON TT.TT_PK = TC.TC_TT
      JOIN dbo.RefContainer RC ON TC.TC_RC = RC.RC_PK
    WHERE
      -- check OOQ has container and TEUCount > 0
      TT.TT_ContainerMode IN ('FCL', 'FRO')
      AND TT.TT_TransportMode = 'SEA'
      AND JS.JS_TH_OneTimeQuote IS NULL
      AND RC.RC_TEU > 0
    UNION ALL
    SELECT
      TH.TH_PK
    FROM
      dbo.RatingHeader TH
      LEFT JOIN dbo.JobShipment JS ON TH.TH_PK = JS.JS_TH_OneTimeQuote
      JOIN dbo.RateOneOffShipment TT ON TH.TH_PK = TT.TT_TH
      JOIN dbo.RateOneOffContainers TC ON TT.TT_PK = TC.TC_TT
    WHERE
      -- check OOQ has container and weight = 0
      TT.TT_ContainerMode IN ('FCL', 'FRO')
      AND TT.TT_TransportMode IN ('ROA', 'RAI')
      AND JS.JS_TH_OneTimeQuote IS NULL
      AND TC.TC_ContainerCount > 0
      AND TT.TT_ActualWeight = 0
  );
";

		const string UpdateQuotedBookingSQL = @"
UPDATE
  dbo.JobCO2e
SET
  JCO_Status = JCO_TEUStatus,
  JCO_SystemLastEditTimeUtc = GETUTCDATE(),
  JCO_SystemLastEditUser = '~BP'
WHERE
  JCO_ParentTableCode = 'VB'
  AND JCO_ParentID IN (
    SELECT
      ISNULL(TH.TH_PK, JS.JS_PK)
    FROM
      dbo.JobShipment JS
      JOIN dbo.JobContainer JC ON JC.JC_JS_FCLBookingOnlyLink = JS.JS_PK
      JOIN dbo.RefContainer RC ON JC.JC_RC = RC.RC_PK
      LEFT JOIN dbo.RatingHeader TH ON TH.TH_PK = JS.JS_TH_OneTimeQuote
    WHERE
      -- check QB has container and TEUCount > 0
      JS.JS_IsBooking = 1
      AND JS.JS_IsForwardRegistered = 0
      AND JS.JS_PackingMode = 'FCL'
      AND JS.JS_TransportMode = 'SEA'
      AND RC.RC_TEU > 0
    UNION ALL
    SELECT
      ISNULL(TH.TH_PK, JS.JS_PK)
    FROM
      dbo.JobShipment JS
      JOIN dbo.JobContainer JC ON JC.JC_JS_FCLBookingOnlyLink = JS.JS_PK
      JOIN dbo.RefContainer RC ON JC.JC_RC = RC.RC_PK
      LEFT JOIN dbo.RatingHeader TH ON TH.TH_PK = JS.JS_TH_OneTimeQuote
    WHERE
      -- check QB has container and weight = 0
      JS_IsBooking = 1
      AND JS_IsForwardRegistered = 0
      AND JS_PackingMode = 'FCL'
      AND JS_TransportMode IN ('ROA', 'RAI')
      AND JS_ActualWeight = 0
  );
";

		const string UpdateLegSQL = @"
UPDATE
  dbo.JobCO2e
SET
  JCO_Status = CASE
    WHEN JCO_Status IN ('NCU', 'PEN', 'REJ')
    OR JCO_TEUStatus IN ('NCU', 'PEN', 'REJ') THEN 'NCU'
    WHEN (
      JCO_Status = 'CUR'
      AND JCO_TEUStatus = 'NON'
    )
    OR (
      JCO_Status = 'NON'
      AND JCO_TEUStatus = 'CUR'
    ) THEN 'CUR'
    ELSE JCO_Status
  END,
  JCO_SystemLastEditTimeUtc = GETUTCDATE(),
  JCO_SystemLastEditUser = '~BP'
WHERE
  JCO_ParentTableCode IN ('JX', 'JW');
";

		protected override void OfflinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, JobCO2eSchema.Constants.TableName)
				|| !DbObjectCreator.ColumnExists(Db.Connection, JobCO2eSchema.Constants.TableName, "JCO_TEUStatus")
				|| !DbObjectCreator.ColumnExists(Db.Connection, JobCO2eSchema.Constants.TableName, "JCO_Status")
				|| !DbObjectCreator.ColumnExists(Db.Connection, JobCO2eSchema.Constants.TableName, "JCO_ParentID")
				|| !DbObjectCreator.ColumnExists(Db.Connection, JobCO2eSchema.Constants.TableName, "JCO_ParentTableCode"))
			{
				return;
			}

			Db.Connection.ExecuteNonQuery(UpdateShipmentSQL);
			Db.Connection.ExecuteNonQuery(UpdateConsolSQL);
			Db.Connection.ExecuteNonQuery(UpdateOneOffQuoteSQL);
			Db.Connection.ExecuteNonQuery(UpdateQuotedBookingSQL);
			Db.Connection.ExecuteNonQuery(UpdateLegSQL);
		}
	}
}
