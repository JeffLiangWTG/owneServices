using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
{
	public CusEntryHeaderFetchStrategy(Customs.Business.CusEntryHeader entryHeader)
		: base(entryHeader)
	{
	}

	protected override void FetchForViewCore(TableColumn[] columns)
	{
		foreach (var column in columns)
		{
			var columnName = column.ColumnName;
			switch (columnName)
			{
				case CusEntryHeader.Schema.SelectionResult:
				case CusEntryHeader.Schema.SelectionResultDescription:
				case CusEntryHeader.Schema.AccessCode:
					Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
					break;
			}
		}

		base.FetchForViewCore(columns);
	}
}
