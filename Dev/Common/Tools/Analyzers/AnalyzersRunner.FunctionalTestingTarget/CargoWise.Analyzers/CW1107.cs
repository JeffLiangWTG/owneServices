// CW1107:Do Not Use DbConnection Methods Analyzer

using CargoWise.Data;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1107
	{
		public void WriteDebugMessages()
		{
			Db.Connection.BeginTransaction();
			Db.Connection.CommitTransaction();
			Db.Connection.RollbackTransaction();
			var cmd = Db.Connection.Command("DROP DATABASE;");
		}
	}
}
