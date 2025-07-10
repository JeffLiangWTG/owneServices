using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.Accounting.Business.Testing.StabilityCheck
{
	internal class DbCheckConstraintReader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public List<DbCheckConstraint> ReadAllCheckConstraints()
		{
			string sql = @"
select
	schema_name(T.schema_id) as schema_name,
	object_name(T.object_id) as table_name,
	C.name as constraint_name,
	C.definition
from SYS.CHECK_CONSTRAINTS C
inner join SYS.TABLES T on T.object_id=C.parent_object_id
";
			List<DbCheckConstraint> res = new List<DbCheckConstraint>();
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string schemaName = reader["schema_name"].ToString();
						string tableName = reader["table_name"].ToString();
						string constraintName = reader["constraint_name"].ToString();
						string expression = reader["definition"].ToString();

						res.Add(new DbCheckConstraint(schemaName, tableName, constraintName, expression));
					}
				}
			}

			return res;
		}
	}
}
