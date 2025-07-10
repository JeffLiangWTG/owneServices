using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.Client.EDI.Billing.Business
{
	sealed class DatabaseOwnerHelper
	{
		public static Dictionary<Guid, Guid> GetDatabaseOwner(IEnumerable<Guid> databasePks)
		{
			Dictionary<Guid, Guid> result = new Dictionary<Guid, Guid>();
			if (databasePks.Any())
			{
				var paramName = databasePks.Count() < 5 ? "@FewPKs" : "@ManyPKs";

				string sql = @"select LD_PK, LC_PK from dbo.EdiViewLicenceDatabaseOwner where LD_PK in (select Value from " + paramName + @")";
				using (var pkParam = new GuidTableValuedParameter(databasePks))
				using (var cmd = Db.Connection.Command(sql))
				{
					pkParam.AddTo(cmd, paramName);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(reader.GetGuid(0), reader.GetGuid(1));
						}
					}
				}
			}
			return result;
		}

		public static Guid GetDatabaseOwner(Guid databasePk)
		{
			var result = Guid.Empty;
			string sql = @"select LC_PK from dbo.EdiViewLicenceDatabaseOwner where LD_PK = @DbPk";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@DbPk", System.Data.SqlDbType.UniqueIdentifier, databasePk);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = reader.GetGuid(0);
					}
				}
			}
			return result;
		}
	}
}
