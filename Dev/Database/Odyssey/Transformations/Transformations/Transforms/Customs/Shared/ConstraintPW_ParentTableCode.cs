using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintPW_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column PW_ParentTableCode";

		public TransformationIndexProvider IndexProvider => CreateIndexProvider();

		TransformationIndexProvider CreateIndexProvider()
		{
			var provider = new TransformationIndexProvider(this);

			provider.New(CusBondDetailSchema.Instance)
				.Key(CusBondDetailSchema.Constants.PW_ParentID)
				.Include(CusBondDetailSchema.Constants.PW_SystemCreateTimeUtc)
				.Include(CusBondDetailSchema.Constants.PW_SystemLastEditTimeUtc)
				.Where(@"[PW_ParentTableCode]<>'ABL' AND [PW_ParentTableCode]<>'AMA' AND [PW_ParentTableCode]<>'BH' AND [PW_ParentTableCode]<>'BM' AND [PW_ParentTableCode]<>'CEI' AND [PW_ParentTableCode]<>'JE' AND [PW_ParentTableCode]<>'OH' AND [PW_ParentTableCode]<>'SRH'")
				.GetInfo();

			return provider;
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				DELETE FROM dbo.CusBondDetail
				WHERE PW_ParentTableCode NOT IN (
					'ABL', -- AsycudaBill
					'AMA', -- AsycudaManifestHeader
					'CEI', -- CusEntryInstruction
					'BH', -- CusInBondHeader
					'BM', -- CusInBondMoveHeader
					'SRH', -- CusTempStorageRegHeader
					'JE', -- JobDeclaration
					'OH' -- OrgHeader
				)");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
