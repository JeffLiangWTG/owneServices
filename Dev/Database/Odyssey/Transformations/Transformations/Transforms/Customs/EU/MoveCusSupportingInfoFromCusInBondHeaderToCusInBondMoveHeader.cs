using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	public class MoveCusSupportingInfoFromCusInBondHeaderToCusInBondMoveHeader : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Change CSI_ParentTableCode 'BH' to 'BM' and update CSI_ParentID with BM_PK (where BH_ApplicationCode = 'NC5').";
		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(nctsCusSupportingInfoTransformation);
		}

		const string nctsCusSupportingInfoTransformation = @"
			UPDATE SupportingInfo
			SET SupportingInfo.CSI_ParentID = MoveHeader.BM_PK, SupportingInfo.CSI_ParentTableCode = 'BM', SupportingInfo.CSI_SystemLastEditTimeUtc = GetUtcDate(), SupportingInfo.CSI_SystemLastEditUser = 'E'
			FROM dbo.CusSupportingInfo AS SupportingInfo
			INNER JOIN dbo.CusInBondMoveHeader AS MoveHeader ON MoveHeader.BM_BH = SupportingInfo.CSI_ParentID
			INNER JOIN dbo.CusInBondHeader AS NctsHeader ON NctsHeader.BH_PK = SupportingInfo.CSI_ParentID
			WHERE SupportingInfo.CSI_ParentTableCode = 'BH' AND SupportingInfo.CSI_Type = 'SUP' AND NctsHeader.BH_ApplicationCode = 'NC5' AND NctsHeader.BH_HeaderType = 'D'";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(CusSupportingInfoSchema.Instance)
					.Key(CusSupportingInfoSchema.Constants.CSI_ParentID)
					.Include(CusSupportingInfoSchema.Constants.CSI_ParentTableCode, CusSupportingInfoSchema.Constants.CSI_CSI_SupportingInfo, CusSupportingInfoSchema.Constants.CSI_SystemCreateTimeUtc, CusSupportingInfoSchema.Constants.CSI_SystemLastEditTimeUtc, CusSupportingInfoSchema.Constants.CSI_Type)
					.Where("[CSI_Type]='SUP' AND [CSI_ParentTableCode]='BH'")
					.GetInfo();

				indexProvider.New(CusInBondHeaderSchema.Instance)
					.Key(CusInBondHeaderSchema.Constants.PK)
					.Where("[BH_ApplicationCode]='NC5' AND [BH_HeaderType]='D'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
