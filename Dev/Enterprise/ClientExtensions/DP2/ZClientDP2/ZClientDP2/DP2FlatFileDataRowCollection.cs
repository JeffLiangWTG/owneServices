using System;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DP2
{
	class DP2FlatFileDataRowCollection : FlatFileDataRowCollection
	{
		public override void Add(FlatFileDataRow row)
		{
			bool shouldAddNew = true;
			foreach (FlatFileDataRow existingRow in this)
			{
				if (row[DP2FlatFileDataRow.Schema.Invoice.Name] == existingRow[DP2FlatFileDataRow.Schema.Invoice.Name]
					&& row[DP2FlatFileDataRow.Schema.GLAccount.Name] == existingRow[DP2FlatFileDataRow.Schema.GLAccount.Name]
					&& row[DP2FlatFileDataRow.Schema.TaxCode.Name] == existingRow[DP2FlatFileDataRow.Schema.TaxCode.Name])
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
			existingRow[DP2FlatFileDataRow.Schema.Amount.Name] = (Convert.ToDecimal(existingRow[DP2FlatFileDataRow.Schema.Amount.Name]) + Convert.ToDecimal(row[DP2FlatFileDataRow.Schema.Amount.Name])).ToString();
		}
	}
}
