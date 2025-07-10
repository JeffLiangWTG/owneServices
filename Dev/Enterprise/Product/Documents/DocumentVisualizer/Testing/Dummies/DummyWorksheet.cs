using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	[DebuggerDisplay("Rows={rows.Last} Columns={columns.Last}")]
	public class DummyWorksheet : IWorksheet
	{
		IReadOnlyList<IColumn> IWorksheet.Columns => columns;

		public List<IColumn> Columns => columns;

		readonly List<IColumn> columns = new List<IColumn>();

		IReadOnlyList<IRow> IWorksheet.Rows => rows;

		public List<IRow> Rows => rows;

		readonly List<IRow> rows = new List<IRow>();

		Margins IWorksheet.Margins => margins ?? (margins = DummyMarginsFactory.Get());
		Margins margins;

		PageDimensions IWorksheet.PageDimensions => pageDimensions ?? (pageDimensions = DummyPageDimensionsFactory.Get(DummyPageDimensionsFactory.PaperType.Letter));
		PageDimensions pageDimensions;

		public bool PrintContentCenteredHorizontally { get; set; }

		public string Name { get; set; } = "dummy";

		public ICell GetCell(int row, int column)
		{
			var location = new Point(row, column);

			ICell result;

			if (cells.ContainsKey(location))
			{
				result = cells[location];
			}
			else
			{
				result = new DummyCell
				{
					BottomRow = row,
					TopRow = row,
					LeftColumn = column,
					RightColumn = column
				};

				AddCell(result);
			}

			return result;
		}

		protected readonly Dictionary<Point, ICell> cells = new Dictionary<Point, ICell>();

		public void AddCell(ICell cell)
		{
			for (int row = cell.TopRow; row <= cell.BottomRow; row++)
			{
				for (int column = cell.LeftColumn; column <= cell.RightColumn; column++)
				{
					var location = new Point(row, column);
					cells[location] = cell;
				}
			}
		}

		public IEnumerable<int> PageBreaks => Enumerable.Empty<int>();

		public static DummyWorksheet Parse(string content, double rowHeight = 20.2d, double columnWidth = 70.1d)
		{
			var worksheet = new DummyWorksheet();

			var rows = content.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);

			int rowNumber = 0;

			foreach (var row in rows)
			{
				rowNumber++;

				worksheet.rows.Add(rowHeight);

				var columns = row.Split('\t');

				int columnNumber = 0;

				foreach (var column in columns)
				{
					columnNumber++;

					if (worksheet.columns.Count < columnNumber)
					{
						worksheet.columns.Add(columnWidth);
					}

					if (!string.IsNullOrEmpty(column))
					{
						var location = new Point(rowNumber, columnNumber);

						var cell = new DummyCell
						{
							TopRow = location.X,
							LeftColumn = location.Y,
							BottomRow = location.X,
							RightColumn = location.Y,
							Height = rowHeight,
							Width = columnWidth,
							Value = column
						};

						cell.Format = new Format
						{
							Borders = Borders.Empty,
							Font = new Core.Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, Color.Empty, 0)
						};

						worksheet.cells.Add(location, cell);
					}
				}
			}

			return worksheet;
		}

		#region IResourceProvider members

		IReadOnlyDictionary<string, Func<object>> IResourceProvider.Resources => resources ?? (resources = new Dictionary<string, Func<object>>());
		IReadOnlyDictionary<string, Func<object>> resources;

		#endregion
	}
}
