using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("Count {Count}, Cached {rows.Count}")]
	sealed class FlexCelRowCollection : IReadOnlyList<IRow>
	{
		public FlexCelRowCollection(FlexCelWorksheet worksheet)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
		}

		readonly FlexCelWorksheet worksheet;
		Dictionary<int, IRow> rows;

		IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator()
		{
			for (int i = 1; i <= Count; i++)
			{
				yield return GetAt(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerable<IRow> enumerable = this;
			return enumerable.GetEnumerator();
		}

		public IRow this[int index]
		{
			get { return GetAt(index + 1); }
		}

		// uses 1 based index
		IRow GetAt(int rowNumber)
		{
			IRow result = null;

			if (rows == null || !rows.ContainsKey(rowNumber))
			{
				result = CreateRow(rowNumber);

				if (rows == null)
				{
					rows = new Dictionary<int, IRow>();
				}

				rows[rowNumber] = result;
			}
			else
			{
				result = rows[rowNumber];
			}

			return result;
		}

		IRow CreateRow(int rowNumber)
		{
			var isOutOfBounds = rowNumber <= 0 || rowNumber > worksheet.RowCount;

			if (isOutOfBounds)
			{
				return new Row(rowNumber, 0d);
			}

			return new FlexCelRow(worksheet, rowNumber);
		}

		public int Count
		{
			get { return worksheet.RowCount; }
		}
	}
}