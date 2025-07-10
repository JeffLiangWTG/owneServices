using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Asycuda
{
	static class ClearCountryCodeFromABCCusEntryNum
	{
		const string UserDescription = "Clear CE_RN_CountryCode for ABCEntryNum";

		public static void Transform(IUpgradeManager manager)
		{
			manager.ShowInfoMessage("\t\t" + UserDescription);

			Db.Connection.ExecuteNonQuery(sqlUpdateQuery);

			manager.ShowInfoMessage("\t\t\tCompleted: " + UserDescription);
		}

		const string sqlUpdateQuery = @$"
UPDATE dbo.CusEntryNum
SET
    CE_RN_NKCountryCode = '',
    CE_SystemLastEditTimeUtc = GETUTCDATE(),
    CE_SystemLastEditUser = '~BP'
FROM dbo.CusEntryNum
WHERE 
    CE_ParentTable = 'AsycudaBillCountry' AND CE_RN_NKCountryCode <> '';";
	}
}
