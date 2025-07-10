using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("Count {Count}, Cached {columns.Count}")]
	sealed class FlexCelColumnCollection : IReadOnlyList<IColumn>
	{
		public FlexCelColumnCollection(FlexCelWorksheet worksheet)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
		}

		readonly FlexCelWorksheet worksheet;
		Dictionary<int, IColumn> columns;

		IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator()
		{
			for (int i = 1; i <= worksheet.ColCount; i++)
			{
				yield return GetAt(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerable<IColumn> enumerable = this;
			return enumerable.GetEnumerator();
		}

		public IColumn this[int index]
		{
			get { return GetAt(index + 1); }
		}

		// uses 1 based index
		IColumn GetAt(int columnNumber)
		{
			IColumn result = null;

			if (columns == null || !columns.ContainsKey(columnNumber))
			{
				result = CreateColumn(columnNumber);

				if (columns == null)
				{
					columns = new Dictionary<int, IColumn>();
				}

				columns[columnNumber] = result;
			}
			else
			{
				result = columns[columnNumber];
			}

			return result;
		}

		IColumn CreateColumn(int columnNumber)
		{
			var isOutOfBounds = columnNumber <= 0 || columnNumber > worksheet.ColCount;

			if (isOutOfBounds)
			{
				return new Column(columnNumber, 0d);
			}

			return new FlexCelColumn(worksheet, columnNumber);
		}

		public int Count
		{
			get { return worksheet.ColCount; }
		}
	}
}