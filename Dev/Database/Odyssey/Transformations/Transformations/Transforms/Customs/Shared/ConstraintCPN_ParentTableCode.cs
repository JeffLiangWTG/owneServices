using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	sealed class ConstraintCPN_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CPN_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sqlText = FormattableString.Invariant($@"
DROP TABLE IF EXISTS #CusPersonsToDelete
CREATE TABLE #CusPersonsToDelete (CPN_PK UNIQUEIDENTIFIER)

INSERT INTO #CusPersonsToDelete
SELECT CPN_PK
FROM dbo.CusPerson
WHERE CPN_ParentTableCode NOT IN ('AMA', 'JE')

DELETE FROM dbo.CusPersonCountry
WHERE CPC_CPN_Person IN (SELECT CPN_PK FROM #CusPersonsToDelete)

DELETE FROM dbo.CusPerson
WHERE CPN_PK IN (SELECT CPN_PK FROM #CusPersonsToDelete)

DROP TABLE #CusPersonsToDelete
");

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusPersonSchema.Instance)
					.Key(CusPersonSchema.Constants.CPN_ParentID)
					.Include(CusPersonSchema.Constants.CPN_SystemCreateTimeUtc)
					.Include(CusPersonSchema.Constants.CPN_SystemLastEditTimeUtc)
					.Where("[CPN_ParentTableCode]<>'AMA' AND [CPN_ParentTableCode]<>'JE'")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
