using System.Collections.Generic;
using System.Text;

namespace Enterprise.DocumentWrappers.FormatTables
{
	[System.Diagnostics.DebuggerDisplay("Columns = {columns.Length}")]
	internal sealed class FormatTable : IFormatSectionComponent
	{
		public FormatTable(bool includeHeadingInBody, params FormatColumn[] columns)
		{
			this.includeHeadingInBody = includeHeadingInBody;
			this.columns = columns;
		}

		public bool IncludeHeadingInBody
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return includeHeadingInBody; }
		}

		public int CalculateTotalWidth()
		{
			int result = 0;

			foreach (FormatColumn column in columns)
			{
				result += column.OuterWidth;
			}

			return result;
		}

		public IEnumerable<string> Heading
		{
			get { return CombineAndEnumerate(true); }
		}

		public IEnumerable<string> Body
		{
			get { return CombineAndEnumerate(false); }
		}

		#region Implementation

		IEnumerable<string> CombineAndEnumerate(bool heading)
		{
			IEnumerator<string>[] enumerators = new IEnumerator<string>[columns.Length];
			IEnumerator<FormatSegment>[] currentRow = new IEnumerator<FormatSegment>[columns.Length];

			if (heading)
			{
				for (int i = 0; i < columns.Length; i++)
				{
					IEnumerable<string> values = new string[] { columns[i].Heading ?? string.Empty };
					enumerators[i] = columns[i].InnerWidth == 0 ? null : values.GetEnumerator();
				}
			}
			else
			{
				for (int i = 0; i < columns.Length; i++)
				{
					enumerators[i] = columns[i].InnerWidth == 0 ? null : columns[i].GetEnumerator();
				}
			}

			StringBuilder lineBuilder = new StringBuilder();
			bool enumeratorsEmpty;
			bool rowsEmpty;

			do
			{
				enumeratorsEmpty = true;

				for (int i = 0; i < enumerators.Length; i++)
				{
					if (enumerators[i] == null)
					{
						currentRow[i] = null;
					}
					else if (!enumerators[i].MoveNext())
					{
						currentRow[i] = null;
						enumerators[i].Dispose();
						enumerators[i] = null;
					}
					else
					{
						currentRow[i] = FormatSegment.EnumerateSegments(enumerators[i].Current, columns[i].InnerWidth);
						enumeratorsEmpty = false;
					}
				}

				if (enumeratorsEmpty)
				{
					break;
				}

				do
				{
					rowsEmpty = true;
					lineBuilder.Length = 0;

					for (int i = 0; i < currentRow.Length; i++)
					{
						FormatColumn column = columns[i];

						if (currentRow[i] == null)
						{
							lineBuilder.Append(' ', column.OuterWidth);
						}
						else if (!currentRow[i].MoveNext())
						{
							lineBuilder.Append(' ', column.OuterWidth);
							currentRow[i].Dispose();
							currentRow[i] = null;
						}
						else
						{
							rowsEmpty = false;
							AppendSegment(lineBuilder, column, currentRow[i].Current);
						}
					}

					if (rowsEmpty)
					{
						break;
					}

					yield return lineBuilder.ToString();
				} while (true);
			} while (true);
		}

		static void AppendSegment(StringBuilder lineBuilder, FormatColumn column, FormatSegment segment)
		{
			int gap = column.InnerWidth - segment.Length;
			int lPad;
			int rPad;

			switch (column.Options & FormatColumnOptions.AlignmentMask)
			{
				default:
				case FormatColumnOptions.LeftAlign:
					lPad = column.LeftPadding;
					rPad = column.RightPadding + gap;
					break;

				case FormatColumnOptions.RightAlign:
					lPad = column.LeftPadding + gap;
					rPad = column.RightPadding;
					break;

				case FormatColumnOptions.CenterAlign:
					int mid = gap >> 1;
					lPad = column.LeftPadding + mid;
					rPad = column.RightPadding + gap - mid;
					break;
			}

			lineBuilder.Append(' ', lPad);
			segment.AddTo(lineBuilder);
			lineBuilder.Append(' ', rPad);
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly bool includeHeadingInBody;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
		readonly FormatColumn[] columns;
	}
}
