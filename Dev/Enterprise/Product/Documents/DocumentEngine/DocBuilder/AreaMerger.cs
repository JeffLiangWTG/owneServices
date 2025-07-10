using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class AreaMerger
	{
		internal void Merge(ExcelInterface excelInterface)
		{
			var workSheets = excelInterface.WorkSheets.ToList();
			foreach (var workSheet in workSheets)
			{
				MergeWorkSheet(workSheet);
			}
		}

		void MergeWorkSheet(ExcelWorkSheet workSheet)
		{
			var areas = GetAreas(workSheet);
			areas.Sort(CompareByTypeOrderThenRowIndex);

			AreaType lastAreaType = null;
			int currentRow = 0;
			foreach (var area in areas)
			{
				if (lastAreaType == area.Type && area.Type.CanOnlyHaveOne)
				{
					workSheet.RemoveRow(area.FirstRow);
					area.RowCount--;
					OffsetAreas(areas, area.FirstRow, workSheet.RowCount, -1);
				}

				lastAreaType = area.Type;

				if (area.RowCount > 0 && area.FirstRow != currentRow)
				{
					workSheet.MoveRows(area.FirstRow, area.LastRow, currentRow);
					OffsetAreas(areas, currentRow - 1, area.FirstRow, area.RowCount);
				}

				currentRow += area.RowCount;
			}
			workSheet.RefreshRowCount();
		}

		void OffsetAreas(List<TemplateArea> areas, int firstRowIndex, int lastRowIndex, int offset)
		{
			foreach (var area in areas)
			{
				if (area.FirstRow > firstRowIndex && area.FirstRow < lastRowIndex)
				{
					area.FirstRow += offset;
				}
			}
		}

		List<TemplateArea> GetAreas(ExcelWorkSheet workSheet)
		{
			var result = new List<TemplateArea>();
			TemplateArea currentArea = null;
			for (var rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
			{
				var cellContent = workSheet[rowIndex, 0].ToString();
				if (cellContent.StartsWith("#"))
				{
					AreaType areaType = AreaTypes.Find(item => cellContent.StartsWith(item.Identifier, System.StringComparison.InvariantCultureIgnoreCase));
					if (areaType != null)
					{
						currentArea = new TemplateArea(rowIndex, 0, areaType);
						result.Add(currentArea);
					}
				}

				if (currentArea != null)
				{
					currentArea.RowCount++;
				}
			}

			return result;
		}

		int CompareByTypeOrderThenRowIndex(TemplateArea x, TemplateArea y)
		{
			int result = x.Type.Order.CompareTo(y.Type.Order);
			if (result == 0)
			{
				result = x.FirstRow.CompareTo(y.FirstRow);
			}
			return result;
		}

		class AreaType
		{
			internal string Identifier;
			internal int Order;
			internal bool CanOnlyHaveOne;
		}

		readonly List<AreaType> AreaTypes = new List<AreaType>()
		{
			new AreaType() { Identifier = (NoResString)"#CONFIG", Order = 0, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#DOCUMENTHEADER", Order = 1, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#PAGEHEADER", Order = 2, CanOnlyHaveOne = true },

			new AreaType() { Identifier = (NoResString)"#SECTIONHEADER", Order = 3, CanOnlyHaveOne = false },
			new AreaType() { Identifier = (NoResString)"#SECTIONPAGEHEADER", Order = 3, CanOnlyHaveOne = false },
			new AreaType() { Identifier = (NoResString)"#SECTIONBODY", Order = 3, CanOnlyHaveOne = false },
			new AreaType() { Identifier = (NoResString)"#GROUPBY", Order = 3, CanOnlyHaveOne = false },
			new AreaType() { Identifier = (NoResString)"#SECTIONPAGEFOOTER", Order = 3, CanOnlyHaveOne = false },
			new AreaType() { Identifier = (NoResString)"#SECTIONFOOTER", Order = 3, CanOnlyHaveOne = false },

			new AreaType() { Identifier = (NoResString)"#FIRSTPAGEFOOTER", Order = 4, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#PAGEFOOTER", Order = 5, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#ONLYONEPAGEFOOTER", Order = 6, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#LASTPAGEFOOTER", Order = 7, CanOnlyHaveOne = true },

			new AreaType() { Identifier = (NoResString)"#DOCUMENTFOOTER", Order = 8, CanOnlyHaveOne = true },

			new AreaType() { Identifier = (NoResString)"#BACKPAGE", Order = 9, CanOnlyHaveOne = true },
			new AreaType() { Identifier = (NoResString)"#ENDOFREPORT", Order = 10, CanOnlyHaveOne = true },
		};

		class TemplateArea
		{
			internal TemplateArea(int firstRow, int rowCount, AreaType type)
			{
				this.FirstRow = firstRow;
				this.RowCount = rowCount;
				this.Type = type;
			}

			internal int FirstRow;
			internal int RowCount;
			internal AreaType Type;

			internal int LastRow
			{
				get { return FirstRow + RowCount - 1; }
			}
		}
	}
}
