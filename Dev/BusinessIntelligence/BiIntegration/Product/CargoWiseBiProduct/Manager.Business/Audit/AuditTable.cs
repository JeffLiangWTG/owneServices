using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class AuditTable : NonPersistentBusinessObject
	{
		public AuditTable(string sourceTableName)
		{
			var source = sourceTableName.Split('.');
			if (source.Length != 2)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Source Table Name value ('{0}') is invalid.", sourceTableName));
			}

			Schema = source[0];
			Name = source[1];
		}

		public AuditTable(ZString schema, ZString name)
		{
			Schema = schema;
			Name = name;
		}

		public ZString Schema { get; set; }
		public ZString Name { get; set; }
		public ZString CurrentState { get; set; }
		public ZDateTime EarliestTransactionDate { get; set; }
		public ZDateTime LatestTransactionDate { get; set; }
		public ZLong RowCount { get; set; }
		public ZDecimal SizeUsed { get; set; }
		public ZDecimal SizeUnused { get; set; }
		public ZDecimal TotalSize { get; set; }
		public ZLong LoadRecordCount { get; set; }
		public ZLong LoadDuration { get; set; }
		public ZString SqlErrorMessage { get; set; }
		public ZDateTime SqlErrorDatetimeUTC { get; set; }
		public ZString IndexReorganizationSqlErrorMessage { get; set; }
	}
}
