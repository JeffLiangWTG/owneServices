using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class AddConstraintToRateLinesParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column TL_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				UPDATE dbo.RateLines
				SET TL_ParentTableCode='', TL_ParentID=null, TL_SystemLastEditTimeUtc=GetUtcDate(), TL_SystemLastEditUser='~BP'
				WHERE TL_ParentTableCode NOT IN ('', 'OP');
			");

			Db.Connection.ExecuteNonQuery(sql);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(RateLinesSchema.Instance)
					.Key(RateLinesSchema.Constants.TL_ParentID)
					.Where("[TL_ParentTableCode]<>'' AND [TL_ParentTableCode]<>'OP'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
