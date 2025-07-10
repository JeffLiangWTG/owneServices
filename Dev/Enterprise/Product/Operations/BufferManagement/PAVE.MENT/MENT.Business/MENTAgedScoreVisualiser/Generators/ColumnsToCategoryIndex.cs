using System.Collections.Generic;

namespace Enterprise.PAVE.MENT.Business
{
	public class ColumnsToCategoryIndex
	{
		public ColumnsToCategoryIndex(IEnumerable<string> columnsToMap, int index, string categoryLabel)
		{
			this.ColumnsToMap = columnsToMap;
			this.CategoryLabel = categoryLabel;
			this.Index = index;
		}

		public IEnumerable<string> ColumnsToMap { get; private set; }

		public string CategoryLabel { get; private set; }

		public int Index { get; private set; }
	}
}
