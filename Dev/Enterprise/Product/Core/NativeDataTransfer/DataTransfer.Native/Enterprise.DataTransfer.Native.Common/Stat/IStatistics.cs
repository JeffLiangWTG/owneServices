using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.Common.Stat
{
	public interface IStatistics
	{
		long GetCount(DBEntity.DbAction action);
		string[] EntityNames { get; }
		IEnumerable<DBEntity> EntityAffected { get; }
		DBEntityStatistics GetEntityStatistics(string name);
		void Clear();
	}

	public static class StatisticsExtension
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public static IEnumerable<string> Summary(this IStatistics statistics)
		{
			var entityNames = statistics.EntityNames;

			if (entityNames.Length == 0)
			{
				yield return "No insert/update action performed.";
			}
			else
			{
				foreach (var entityName in entityNames)
				{
					var entityStat = statistics.GetEntityStatistics(entityName);
					yield return entityStat.ToString();
				}
			}
		}
	}
}
