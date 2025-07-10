using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.Common.Stat
{
	public class DBEntityStatistics
	{
		readonly List<DBEntity> entityList;

		public DBEntityStatistics(List<DBEntity> entityList)
		{
			this.entityList = entityList;
		}

		public string EntityName
		{
			get { return entityList.Take(1).Select(e => e.Name).FirstOrDefault(); }
		}

		public long GetCount(DBEntity.DbAction action)
		{
			return entityList.Count(e => e.Action == action);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override string ToString()
		{
			return string.Format("{0} - {1} inserts, {2} updates, {3} deletes", EntityName, GetCount(DBEntity.DbAction.Insert), GetCount(DBEntity.DbAction.Update), GetCount(DBEntity.DbAction.Delete));
		}

		public List<DBEntity> Entities
		{
			get { return entityList; }
		}
	}
}
