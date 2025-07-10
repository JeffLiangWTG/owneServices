using CargoWise.Data;
using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity
{
	class SqlDbSecuritySynchroniserProvider : ISqlDbSecuritySynchroniserProvider
	{
		public IDifferenceSynchroniser GetServerSynchroniser(string proposedPrincipalsAndMemberships, string proposedPermissions, string[] likeFilters, string[] notLikeFilters, ILogger logger)
		{
			return new ServerSecuritySynchroniser(proposedPrincipalsAndMemberships, proposedPermissions, likeFilters, notLikeFilters, Db.DatabaseCaseSensitiveCollation, logger);
		}

		public IDifferenceSynchroniser GetDatabaseSyncrhoniser(string proposedPrincipalsAndMemberships, string proposedPermissions, string[] likeFilters, string[] notLikeFilters, ILogger logger)
		{
			return new DatabaseSecuritySynchroniser(proposedPrincipalsAndMemberships, proposedPermissions, likeFilters, notLikeFilters, Db.DatabaseCaseSensitiveCollation, logger);
		}
	}
}
