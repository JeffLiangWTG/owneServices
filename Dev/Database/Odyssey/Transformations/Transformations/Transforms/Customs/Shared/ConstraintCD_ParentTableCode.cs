using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintCD_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CD_ParentTableCode";

		public TransformationIndexProvider IndexProvider => CreateIndexProvider();

		TransformationIndexProvider CreateIndexProvider()
		{
			var provider = new TransformationIndexProvider(this);

			provider.New(CusUSClassificationSchema.Instance)
				.Key(CusUSClassificationSchema.Constants.CD_ParentID)
				.Include(CusUSClassificationSchema.Constants.CD_SystemCreateTimeUtc)
				.Include(CusUSClassificationSchema.Constants.CD_SystemLastEditTimeUtc)
				.Where(@"[CD_ParentTableCode]<>'CI'")
				.GetInfo();

			return provider;
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				DELETE FROM dbo.CusUSClassification
				WHERE CD_ParentTableCode NOT IN (
					'CI' -- CusClassPartPivot
				)");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
