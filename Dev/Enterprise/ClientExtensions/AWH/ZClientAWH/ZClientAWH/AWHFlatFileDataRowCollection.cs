using System;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.AWH
{
	class AWHFlatFileDataRowCollection : FlatFileDataRowCollection
	{
		public override void Add(FlatFileDataRow row)
		{
			bool shouldAddNew = true;
			foreach (FlatFileDataRow existingRow in this)
			{
				if (row[AWHFlatFileDataRow.Schema.Invoice.Name] == existingRow[AWHFlatFileDataRow.Schema.Invoice.Name]
					&& row[AWHFlatFileDataRow.Schema.GLAccount.Name] == existingRow[AWHFlatFileDataRow.Schema.GLAccount.Name]
					&& row[AWHFlatFileDataRow.Schema.TaxCode.Name] == existingRow[AWHFlatFileDataRow.Schema.TaxCode.Name])
				{
					UpdateAmount(existingRow, row);
					shouldAddNew = false;
				}
			}

			if (shouldAddNew)
			{
				base.Add(row);
			}
		}

		void UpdateAmount(FlatFileDataRow existingRow, FlatFileDataRow row)
		{
			existingRow[AWHFlatFileDataRow.Schema.Amount.Name] = (Convert.ToDecimal(existingRow[AWHFlatFileDataRow.Schema.Amount.Name]) + Convert.ToDecimal(row[AWHFlatFileDataRow.Schema.Amount.Name])).ToString();
		}
	}
}
