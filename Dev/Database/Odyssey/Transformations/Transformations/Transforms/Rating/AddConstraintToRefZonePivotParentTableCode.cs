using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class AddConstraintToRefZonePivotParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column F2_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				UPDATE rzp
				SET rzp.F2_ParentTableCode = 'RL', F2_SystemLastEditTimeUtc=GetUtcDate(), F2_SystemLastEditUser='~BP'
				FROM dbo.RefZonePivot rzp
				INNER JOIN dbo.RefUNLOCO rfu
				ON rzp.F2_ParentTableCode NOT IN ('RL', 'RN') and rzp.F2_ParentID = rfu.RL_PK;

				UPDATE rzp
				SET rzp.F2_ParentTableCode = 'RN', F2_SystemLastEditTimeUtc=GetUtcDate(), F2_SystemLastEditUser='~BP'
				FROM dbo.RefZonePivot rzp
				INNER JOIN dbo.RefCountry rfc
				ON rzp.F2_ParentTableCode NOT IN ('RL', 'RN') and rzp.F2_ParentID = rfc.RN_PK;

				DELETE FROM dbo.RefZonePivot
				WHERE F2_ParentTableCode NOT IN ('RL', 'RN');
			");

			Db.Connection.ExecuteNonQuery(sql);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(RefZonePivotSchema.Instance)
					.Key(RefZonePivotSchema.Constants.F2_ParentID)
					.Where("[F2_ParentTableCode]<>'RL' AND [F2_ParentTableCode]<>'RN'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
