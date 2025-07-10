using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.LandTransport
{
	class DeduplicateJobDocAddressForDtbConsignmentAddresses : DataTransformation
	{
		public override string UserDescription => "Removing duplicate JobDocAddresses for DtbConsignmentAddresses.";

		const string TriggerName = "PreventDuplicateJobDocAddressesForDtbConsignmentAddress";

		protected override void OnlinePreUpgradeTransform()
		{
			base.OnlinePreUpgradeTransform();

			if (!DbObjectCreator.TableExists(Db.Connection, "DtbConsignmentAddress") ||
				!DbObjectCreator.TableExists(Db.Connection, "JobDocAddress"))
			{
				return;
			}

			var hasConsignmentAddressRows = Db.Connection.ExecuteScalar("SELECT TOP 1 LTS_PK FROM DtbConsignmentAddress") != null;

			if (!hasConsignmentAddressRows)
			{
				return;
			}

			Db.Connection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT object_id FROM sys.triggers WHERE type = 'TR' AND name = '{TriggerName}')
exec
('
	CREATE TRIGGER dbo.{TriggerName}
	ON dbo.JobDocAddress AFTER INSERT, UPDATE AS
	IF EXISTS
	(
		SELECT TOP 1 inserted.E2_PK
		FROM	dbo.JobDocAddress jda
		JOIN	inserted
		ON		inserted.E2_ParentId = jda.E2_ParentId
		AND		inserted.E2_PK <> jda.E2_PK
		WHERE	inserted.E2_ParentTableCode = ''LTS''
	)
	THROW 77020, ''Unable to add more than one JobDocAddress for a DtbConsignmentAddress.'', 1;
');

SELECT E2_PK INTO #tempDuplicateJDA
FROM
(
	SELECT E2_PK,
	ROW_NUMBER() OVER
	(
		PARTITION BY	E2_ParentID
		ORDER BY		E2_SystemCreateTimeUtc DESC,
						E2_SystemLastEditTimeUtc DESC,
						E2_PK
	) as PositionWithinParentID
	FROM  dbo.JobDocAddress
	WHERE E2_ParentTableCode = 'LTS'
) duplicates
WHERE PositionWithinParentID > 1;

DELETE FROM dbo.JobDocAddressNumber WHERE E2N_E2 IN (SELECT E2_PK FROM #tempDuplicateJDA);

DELETE FROM dbo.JobDocumentExclusion WHERE JDE_E2_Address IN (SELECT E2_PK FROM #tempDuplicateJDA);

DELETE FROM dbo.QuarantineExDocEstablishmentAndTime WHERE EE_E2_Address IN (SELECT E2_PK FROM #tempDuplicateJDA);

DELETE FROM dbo.JobDocAddress WHERE E2_PK IN (SELECT E2_PK FROM #tempDuplicateJDA);

DROP TABLE #tempDuplicateJDA;
");
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			Db.Connection.ExecuteNonQuery($@"
IF EXISTS (SELECT object_id FROM sys.triggers WHERE type = 'TR' AND name = '{TriggerName}')
DROP TRIGGER dbo.{TriggerName};");
		}
	}
}
