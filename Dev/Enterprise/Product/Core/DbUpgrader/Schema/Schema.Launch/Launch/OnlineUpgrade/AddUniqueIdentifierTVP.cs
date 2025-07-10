using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	class AddUniqueIdentifierTVP
	{
		public AddUniqueIdentifierTVP(IUpgradeManager manager)
		{
			this.manager = manager;
		}

		readonly IUpgradeManager manager;

		public void Run()
		{
			manager.StartTask(string.Format(Culture.Invariant, "Creating Table-Valued Parameter {0} if it does not exist.", TVPHelper.TVP_uniqueidentifier));

			string sql = string.Format(Culture.Invariant, @"
IF NOT EXISTS (SELECT name FROM sys.types WHERE user_type_id = TYPE_ID('{0}') AND is_user_defined = 1 AND is_table_type = 1)
BEGIN
	CREATE TYPE {0} AS TABLE (Value uniqueidentifier NOT NULL PRIMARY KEY CLUSTERED);
END", TVPHelper.TVP_uniqueidentifier);

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
