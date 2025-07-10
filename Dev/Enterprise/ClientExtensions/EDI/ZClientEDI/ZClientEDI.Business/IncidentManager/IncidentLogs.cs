using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentLogs : Logs
	{
		public IncidentLogs(AutoIncidentMain parent)
			: base(parent)
		{
		}

		public StmALog AddLog(Event @event, string reference = "")
		{
			StmALog result = AddNew(@event, reference);
			AddedLogs.Add(result);
			return result;
		}

		public void RemoveAddedLogs()
		{
			foreach (BusinessObject log in AddedLogs)
			{
				if (!log.IsInDatabase)
				{
					RemoveAndDeleteWithoutLoading(log);
				}
			}
			AddedLogs.Clear();
		}

		public void RemoveAddedLogs(Predicate<StmALog> logPredicate)
		{
			foreach (var log in AddedLogs.ToArray())
			{
				if (logPredicate(log))
				{
					RemoveAndDeleteWithoutLoading(log);
					AddedLogs.Remove(log);
				}
			}
		}

		public void StartAddingLogs()
		{
			AddedLogs.Clear();
		}

		#region Added Logs

		public List<StmALog> AddedLogs
		{
			get
			{
				if (fAddedLogs == null)
				{
					fAddedLogs = new List<StmALog>();
				}
				return fAddedLogs;
			}
		}

		List<StmALog> fAddedLogs;

		#endregion

#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage()]
		public System.Collections.ArrayList ElementsNotInDBForTest => ElementsNotInDB;
#endif
	}
}