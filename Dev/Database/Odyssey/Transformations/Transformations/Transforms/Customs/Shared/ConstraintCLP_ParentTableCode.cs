using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintCLP_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CLP_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DELETE FROM dbo.CusCALPCO
WHERE CLP_ParentTableCode NOT IN ('B7', 'JE')");

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusCALPCOSchema.Instance)
				.Key(CusCALPCOSchema.Constants.CLP_ParentID)
				.Include(CusCALPCOSchema.Constants.CLP_OA_Applicant)
				.Include(CusCALPCOSchema.Constants.CLP_OA_Holder)
				.Include(CusCALPCOSchema.Constants.CLP_SystemCreateTimeUtc)
				.Include(CusCALPCOSchema.Constants.CLP_SystemLastEditTimeUtc)
				.Where(@"[CLP_ParentTableCode]<>'B7' AND [CLP_ParentTableCode]<>'JE'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
