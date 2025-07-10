using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public abstract class AccTransactionHeaderAuthorisationRecordWithGenAddOnColumns : AccTransactionHeaderAuthorisationRecord
	{
		protected AccTransactionHeaderAuthorisationRecordWithGenAddOnColumns(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected GenAddOnColumnCollection AddOnColumns => addOnColumns ?? (addOnColumns = new GenAddOnColumnCollection(this));
		GenAddOnColumnCollection addOnColumns;

		protected ZString? GetAddOnColumnValue(string columnName) => AddOnColumns.Find(columnName)?.XA_Data;

		protected void SetAddOnColumnValue(string columnName, ZString value)
		{
			var column = AddOnColumns.Find(columnName);
			if (column != null && value.IsEmpty)
			{
				AddOnColumns.Delete(column);
			}
			else if (!value.IsEmpty)
			{
				if (column == null)
				{
					column = AddOnColumns.AddNew();
					column.XA_Name = columnName;
				}
				column.XA_Data = value;
			}
		}
	}
}