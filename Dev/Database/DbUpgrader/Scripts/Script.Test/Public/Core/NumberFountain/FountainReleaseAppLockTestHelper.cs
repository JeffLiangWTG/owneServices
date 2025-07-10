using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	public static class FountainReleaseAppLockTestHelper
	{
		internal static void Run(DbConnection connection, string name, Guid owner, int fountainId, string resource)
		{
			connection.ExecuteNonQuery(
				nameof(FountainReleaseAppLock),
				(cmd) =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Name", SqlDbType.VarChar, name);
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
					cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
					cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);
				});
		}
	}
}
