using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintBH_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column BH_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DELETE FROM dbo.CusInBondHeader
WHERE BH_ParentTableCode NOT IN ('', 'CEI', 'JE', 'JK', 'JS', 'JX')");

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusInBondHeaderSchema.Instance)
				.Key(CusInBondHeaderSchema.Constants.BH_ParentID)
				.Include(CusInBondHeaderSchema.Constants.BH_SystemCreateTimeUtc)
				.Include(CusInBondHeaderSchema.Constants.BH_SystemLastEditTimeUtc)
				.Where(@"[BH_ParentTableCode]<>'' AND [BH_ParentTableCode]<>'CEI' AND [BH_ParentTableCode]<>'JE' AND [BH_ParentTableCode]<>'JK' AND [BH_ParentTableCode]<>'JS' AND [BH_ParentTableCode]<>'JX'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
