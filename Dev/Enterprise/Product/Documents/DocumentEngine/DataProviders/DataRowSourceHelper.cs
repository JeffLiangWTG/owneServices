using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.DataProviders
{
	class DataRowSourceHelper
	{
		public static IEnumerable<T> GetRowsFromIndexes<T>(int[] indexes, T[] dataSource)
		{
			return indexes.Where(index => index > 0 && index <= dataSource.Length).Select(index => dataSource[index - 1]);
		}
	}
}
