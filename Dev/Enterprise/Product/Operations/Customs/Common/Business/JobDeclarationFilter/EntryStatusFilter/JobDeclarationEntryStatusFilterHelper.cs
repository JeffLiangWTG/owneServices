using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class JobDeclarationEntryStatusFilterHelper
	{
		public JobDeclarationEntryStatusFilterHelper(Func<SQLComparisonOperator, ZString, ZQuery> getEntryStatusQueryForAny, Func<SQLComparisonOperator, ZString, ZQuery> getEntryStatusQueryForAll)
		{
			Argument.NotNull(getEntryStatusQueryForAny, nameof(getEntryStatusQueryForAny));
			Argument.NotNull(getEntryStatusQueryForAll, nameof(getEntryStatusQueryForAll));
			this.getEntryStatusQueryForAny = getEntryStatusQueryForAny;
			this.getEntryStatusQueryForAll = getEntryStatusQueryForAll;
		}

		public ZQuery GetEntryStatusFilter(SQLComparisonOperator @operator, ZString type, ZString entryStatus)
		{
			if (type == EntryStatusFilterTypeList.Codes.All)
			{
				return getEntryStatusQueryForAll.Invoke(@operator, entryStatus);
			}
			else
			{
				return getEntryStatusQueryForAny.Invoke(@operator, entryStatus);
			}
		}

		readonly Func<SQLComparisonOperator, ZString, ZQuery> getEntryStatusQueryForAny;
		readonly Func<SQLComparisonOperator, ZString, ZQuery> getEntryStatusQueryForAll;
	}
}
