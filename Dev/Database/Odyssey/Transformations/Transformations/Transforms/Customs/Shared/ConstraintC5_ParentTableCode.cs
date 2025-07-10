using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintC5_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column C5_ParentTableCode";

		public TransformationIndexProvider IndexProvider => CreateIndexProvider();

		TransformationIndexProvider CreateIndexProvider()
		{
			var provider = new TransformationIndexProvider(this);

			provider.New(CusOutturnSchema.Instance)
				.Key(CusOutturnSchema.Constants.C5_ParentID)
				.Include(CusOutturnSchema.Constants.C5_SystemCreateTimeUtc)
				.Include(CusOutturnSchema.Constants.C5_SystemLastEditTimeUtc)
				.Where(@"[C5_ParentTableCode]<>'' AND [C5_ParentTableCode]<>'APA' AND [C5_ParentTableCode]<>'BD' AND [C5_ParentTableCode]<>'CG' AND [C5_ParentTableCode]<>'CJ' AND [C5_ParentTableCode]<>'CM' AND [C5_ParentTableCode]<>'CS' AND [C5_ParentTableCode]<>'CV' AND [C5_ParentTableCode]<>'CX' AND [C5_ParentTableCode]<>'JC' AND [C5_ParentTableCode]<>'JI' AND [C5_ParentTableCode]<>'JS'")
				.GetInfo();

			return provider;
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				DELETE FROM dbo.CusOutturn
				WHERE C5_ParentTableCode NOT IN (
					'', -- Blank
					'APA', -- AsycudaPack
					'BD', -- CusSeaManOBLDetail
					'CG', -- CusPartShip
					'CJ', -- CusSCADepotContainer
					'CM', -- CusMAWB
					'CS', -- CusHAWB
					'CV', -- CusSCAPivot
					'CX', -- CusSCADepotHouse
					'JC', -- JobContainer
					'JI', -- JobComInvoiceLine
					'JS'  -- JobShipment
				)");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
