using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecFetchStrategy : EU.Business.CusTempStorage.CusTempStorageDecFetchStrategy
	{
		public CusTempStorageDecFetchStrategy(CusTempStorageDec cusTempStorageDec) : base(cusTempStorageDec)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var storageLine = (CusTempStorageDec)BusinessObject;
			foreach (TableColumn column in columns)
			{
				switch (column.ColumnName)
				{
					case CusTempStorageDec.Schema.ReferenceNumber:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, storageLine.PK);
						break;
				}
			}
		}
	}
}
