using System.Collections.Generic;
using System.Text;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	public class DocumentStringBuilder
	{
		public DocumentStringBuilder(IDocument document)
		{
			this.document = document;
		}

		readonly IDocument document;

		public override string ToString()
		{
			var builder = new StringBuilder();
			var rowCells = new List<string>();

			var pageNumber = 0;

			var processedCells = new HashSet<IRange>(new RangeEqualityComparer());

			var columns = new List<string>();

			foreach (var column in document.Columns)
			{
				columns.Add(string.Format("{{{0}}}", GetColumnName(column.Number)));
			}

			if (columns.Count > 0)
			{
				builder.AppendLine("\t" + string.Join("\t", columns));
			}

			foreach (var page in document.Pages)
			{
				pageNumber++;

				builder.AppendLine(string.Format("--- Page {0} ---", pageNumber));

				for (int row = page.Range.TopRow; row <= page.Range.BottomRow; row++)
				{
					builder.Append(string.Format("{{{0}}}", row));

					rowCells.Clear();

					for (int column = page.Range.LeftColumn; column <= page.Range.RightColumn; column++)
					{
						var cell = document.GetCell(row, column);

						if (processedCells.Add(cell) && cell.HasValue())
						{
							rowCells.Add(string.Format("{{{0}}} - {1}", GetColumnName(column), cell.Value));
						}
					}

					if (rowCells.Count > 0)
					{
						var header = row < 10 ? new string('\t', 2) : "\t";

						builder.AppendLine(header + string.Join("\t", rowCells));
					}
					else
					{
						builder.AppendLine();
					}
				}
			}

			return builder.ToString();
		}

		string GetColumnName(int index)
		{
			const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			index--;

			var value = "";

			if (index >= letters.Length)
			{
				value += letters[index / letters.Length - 1];
			}

			value += letters[index % letters.Length];

			return value;
		}
	}
}