using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class FilteredLogsView : BusinessObjectCollectionView<StmALog>
	{
		public FilteredLogsView(IStmALogParent parent, Predicate<StmALog> isLogPartOfTheCollection)
			: base(parent.Logs.GetAllLogs())
		{
			IsLogPartOfTheCollection = isLogPartOfTheCollection;
			Rebuild();
		}

		readonly Predicate<StmALog> IsLogPartOfTheCollection;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var log = element as StmALog;
			if (log != null && IsLogPartOfTheCollection != null)
			{
				return IsLogPartOfTheCollection(log);
			}

			return false;
		}
	}
}
