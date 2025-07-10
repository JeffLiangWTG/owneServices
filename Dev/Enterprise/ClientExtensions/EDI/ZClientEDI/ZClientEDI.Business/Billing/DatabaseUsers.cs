using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IDatabaseUsers
	{
		int DatabaseMonthlyUserCount(ZGuid databasePk, ZDateTime firstDayOfMonth);
		IEnumerable<string> DatabaseMonthlyUserList(ZGuid databasePk, ZDateTime firstDayOfMonth);
	}

	/// <summary>
	/// Calculates the unique users of a database
	/// </summary>

	public class DatabaseUsers : IDatabaseUsers
	{
		public DatabaseUsers(bool forSingleOrg)
		{
			this.forSingleOrg = forSingleOrg;
		}

		readonly bool forSingleOrg;

		public int DatabaseMonthlyUserCount(ZGuid databasePk, ZDateTime firstDayOfMonth)
		{
			if (databasePk.IsEmpty)
			{
				return 0;
			}

			if (map == null)
			{
				map = new Dictionary<int, Dictionary<Guid, int>>();
			}

			Guid pk = databasePk.ToGuid();
			int period = BillingPeriodConverter.ToInt(firstDayOfMonth);

			Dictionary<Guid, int> databaseToCountMap;
			if (!map.TryGetValue(period, out databaseToCountMap))
			{
				databaseToCountMap = forSingleOrg ? new Dictionary<Guid, int>() : LoadAll(period);
				map.Add(period, databaseToCountMap);
			}

			int result = 0;
			if (!databaseToCountMap.TryGetValue(pk, out result) && forSingleOrg)
			{
				result = LoadOne(pk, period);
				databaseToCountMap.Add(pk, result);
			}
			return result;
		}

		public IEnumerable<string> DatabaseMonthlyUserList(ZGuid databasePk, ZDateTime firstDayOfMonth)
		{
			if (databasePk.IsEmpty)
			{
				return Enumerable.Empty<string>();
			}

			List<string> result = new List<string>();

			using (var cmd = Db.Connection.Command(sqlUserList + "ORDER BY LS_FullName, LS_Code"))
			{
				cmd.AddParameter("@Period", System.Data.SqlDbType.Int, BillingPeriodConverter.ToInt(firstDayOfMonth));
				cmd.AddParameter("@DatabasePk", System.Data.SqlDbType.UniqueIdentifier, databasePk.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string name = reader.GetString(0);
						string code = reader.GetString(1).Trim();
						if (!string.IsNullOrEmpty(code))
						{
							name += " (" + code + ')';
						}
						result.Add(name);
					}
				}
			}

			return result;
		}

		int LoadOne(Guid databasePk, int period)
		{
			int result = 0;
			var sql = sqlCountOne.Replace("@DatabasePk", "'" + databasePk.ToString() + "'")
				.Replace("@Period", period.ToString(CultureInfo.InvariantCulture));

			using (var cmd = Db.Connection.Command(sql))
			{
				const int TimeoutMinutes = 30;
				cmd.CommandTimeout = TimeoutMinutes * 60;

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = reader.GetInt32(0);
					}
				}
			}

			return result;
		}

		Dictionary<Guid, int> LoadAll(int period)
		{
			Dictionary<Guid, int> result = new Dictionary<Guid, int>();

			using (var cmd = Db.Connection.Command(sqlCountAll))
			{
				cmd.AddParameter("@Period", System.Data.SqlDbType.Int, period);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0), reader.GetInt32(1));
					}
				}
			}

			if (Globals.IsTest)
			{
				++CallsToLoadAll;
			}
			return result;
		}

		internal int CallsToLoadAll;

		Dictionary<int, Dictionary<Guid, int>> map;

		const string sqlCountOne =
@"SELECT CoreUsers = count(distinct case when LS_Code != '' then LS_Code else LS_FullName end)
FROM (" + sqlUserList + ") a";

		const string sqlCountAll =
@"SELECT
	LD_PK,
	CoreUsers = count(distinct case when LS_Code != '' then LS_Code else LS_FullName end)
FROM
	dbo.EdiViewBillableUsage
WHERE
	LX2_ModuleCode = 'COR'
	AND LX2_Period = @Period
	AND ISNULL(LA_IsActive, 1) = 1
	AND LD_IsActive = 1
GROUP BY
	LD_PK";

		const string sqlUserList =
@"SELECT
	LS_FullName,
	LS_Code
FROM
	dbo.EdiViewBillableUsage
WHERE
	LX2_ModuleCode = 'COR'
	AND LX2_Period = @Period
	AND ISNULL(LA_IsActive, 1) = 1
	AND LD_PK = @DatabasePk
GROUP BY
	LS_FullName, LS_Code
";
	}
}

