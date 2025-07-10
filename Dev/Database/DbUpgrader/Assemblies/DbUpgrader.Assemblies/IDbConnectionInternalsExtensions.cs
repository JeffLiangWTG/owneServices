using System.Data;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Assemblies
{
	static class IDbConnectionInternalsExtensions
	{
		public static IDbConnection GetIDbConnection(this IDbConnectionInternals internals) => internals.InternalDbConnection;
		public static IDbTransaction GetIDbTransaction(this IDbConnectionInternals internals) => internals.InternalDbTransaction;
	}
}
