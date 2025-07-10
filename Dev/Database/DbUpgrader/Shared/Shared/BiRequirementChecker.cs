using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

#if !DEBUG
using Enterprise.ChangeDataCapture.Common;
#endif

namespace Enterprise.DbUpgrader.Shared
{
	public class BiRequirementChecker
	{
		public static BiRequirementChecker Instance
		{
			get
			{
				return instance ?? (instance = new BiRequirementChecker());
			}
		}
		[ThreadSafe]
		static BiRequirementChecker instance;

		public bool IsBiDatabaseUpgradeRequired(DbConnection mainDbConnection, DbConnection biConnection)
		{
			return mainDbConnection != null
				&& biConnection != null
#if !DEBUG
				&& CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName)
#endif
				;
		}
	}
}
