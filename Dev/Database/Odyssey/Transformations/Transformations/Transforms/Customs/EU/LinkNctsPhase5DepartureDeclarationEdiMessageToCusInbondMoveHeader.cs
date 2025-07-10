using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	abstract class LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : DataTransformation
	{
		protected abstract string CountryCode { get; }

		public override string UserDescription => $"Move {CountryCode} EDI Messages for Departure Declaration from the CusInBondHeader to the CusInBondMoveHeader";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists($@"FROM [dbo].[GlbCompany] WHERE [GC_RN_NKCountryCode] = '{CountryCode}'"))
			{
				var sql = $@"
UPDATE e SET
	e.[EM_LinkTable] = 'CusInBondMoveHeader',
	e.[EM_LinkUniqueID] = m.[BM_PK],
	e.[EM_SystemLastEditUser] = '~BP',
	e.[EM_SystemLastEditTimeUtc] = GETUTCDATE()
FROM [dbo].[EDIMessage] e
	INNER JOIN [dbo].[CusInBondHeader] h ON h.[BH_PK] = e.[EM_LinkUniqueID]
	INNER JOIN [dbo].[CusInBondMoveHeader] m ON m.[BM_BH] = h.[BH_PK]
	INNER JOIN [dbo].[GlbBranch] b ON b.[GB_PK] = e.[EM_GB]
	INNER JOIN [dbo].[GlbCompany] c ON c.[GC_PK] = b.[GB_GC]
WHERE c.[GC_RN_NKCountryCode] = '{CountryCode}' AND e.[EM_LinkTable] = 'CusInBondHeader' AND h.[BH_ApplicationCode] = 'NC5' AND m.[BM_SubApplicationCode] = 'D';
	";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
