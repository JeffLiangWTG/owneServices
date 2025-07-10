using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BE
{
	public class ChangeBE_CE_ParentTableAndCE_ParentIDFromHeaderToMovementHeaderNCTS : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Link CusEntryNum to CusInBondMoveHeader where CE_EntryType is CID or REG. Only for BE.";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(nctsEntryNumTransformSql);
		}

		const string nctsEntryNumTransformSql = @"UPDATE cusEntryNum
			SET CE_ParentTable = 'CusInBondMoveHeader', CE_ParentID = cusInBondMoveHeader.BM_PK, CE_SystemLastEditTimeUtc = GetUtcDate(), CE_SystemLastEditUser = '~BP' 
			FROM dbo.CusEntryNum cusEntryNum
			INNER JOIN dbo.CusInBondMoveHeader cusInBondMoveHeader
			ON cusInBondMoveHeader.BM_BH = cusEntryNum.CE_ParentID
			WHERE CE_ParentTable = 'CusInBondHeader'
			AND CE_Category = 'CUS'
			AND (CE_EntryType = 'REG' OR CE_EntryType = 'CID')
			AND CE_RN_NKCountryCode = 'BE';";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryNumSchema.Instance)
					.Key(CusEntryNumSchema.Constants.CE_ParentTable, CusEntryNumSchema.Constants.CE_Category, CusEntryNumSchema.Constants.CE_EntryType, CusEntryNumSchema.Constants.CE_RN_NKCountryCode)
					.Include(CusEntryNumSchema.Constants.CE_EntryStatus, CusEntryNumSchema.Constants.CE_SystemLastEditTimeUtc)
					.Where("([CE_EntryType] IN ('REG', 'CID')) AND [CE_ParentTable]='CusInBondHeader' AND [CE_Category]='CUS' AND [CE_RN_NKCountryCode]='BE'")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
