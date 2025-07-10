using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity
{
	interface ISqlDbSecuritySynchroniserProvider
	{
		IDifferenceSynchroniser GetServerSynchroniser(string proposedPrincipalsAndMemberships, string proposedPermissions, string[] likeFilters, string[] notLikeFilters, ILogger logger);
		IDifferenceSynchroniser GetDatabaseSyncrhoniser(string proposedPrincipalsAndMemberships, string proposedPermissions, string[] likeFilters, string[] notLikeFilters, ILogger logger);
	}
}
