using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	abstract class DataArea : Area
	{
		protected DataArea(int start, int end, Report report, string parameterText)
			: base(start, end, report, parameterText)
		{
		}

		protected DataArea() { }

		internal string TableName
		{
			get { return tableName ?? (tableName = GetTableName()); }
		}
		string tableName;

		public override bool IsDataArea
		{
			get { return true; }
		}

		protected virtual string GetTableName()
		{
			return Parameters[1].Substring(Parameters[1].IndexOf("DATA=", StringComparison.OrdinalIgnoreCase) + "DATA=".Length);
		}

		internal List<string> UsedDatafields
		{
			get
			{
				if (usedDatafields == null)
				{
					if (!ReadAllDataSourceFields().TryGetValue(TableName.ToUpperInvariant().Trim(), out usedDatafields))
					{
						usedDatafields = new List<string>();
					}
				}
				return usedDatafields;
			}
		}
		List<string> usedDatafields;
	}
}
