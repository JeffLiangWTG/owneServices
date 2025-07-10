using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class ConstraintB5_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column B5_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = """
				DELETE pack FROM dbo.CusInvPack pack
				LEFT JOIN dbo.CusInvPack parentPack ON pack.B5_B5_ParentPackage = parentPack.B5_PK
				WHERE
					pack.B5_ParentTableCode NOT IN (
						'B7', -- CusAddInfo
						'B9', -- CusInBondMoveDetail
						'BY', -- CusInBondCargoDesc
						'CXI', -- CusExitItem
						'JE', -- JobDeclaration
						'JI', -- JobComInvoiceLine
						'JZ' -- JobComInvoiceHeader
					) OR parentPack.B5_ParentTableCode NOT IN (
						'B7', -- CusAddInfo
						'B9', -- CusInBondMoveDetail
						'BY', -- CusInBondCargoDesc
						'CXI', -- CusExitItem
						'JE', -- JobDeclaration
						'JI', -- JobComInvoiceLine
						'JZ' -- JobComInvoiceHeader
					)
				""";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusInvPackSchema.Instance)
					.Key(CusInvPackSchema.Constants.B5_ParentID)
					.Include(CusInvPackSchema.Constants.B5_SystemCreateTimeUtc)
					.Include(CusInvPackSchema.Constants.B5_SystemLastEditTimeUtc)
					.Where("[B5_ParentTableCode]<>'B7' AND [B5_ParentTableCode]<>'B9' AND [B5_ParentTableCode]<>'BY' AND [B5_ParentTableCode]<>'CXI' AND [B5_ParentTableCode]<>'JE' AND [B5_ParentTableCode]<>'JI' AND [B5_ParentTableCode]<>'JZ'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
