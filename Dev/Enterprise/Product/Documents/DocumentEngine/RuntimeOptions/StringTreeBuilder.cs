using System;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class StringTreeBuilder
	{
		public StringTreeBuilder(ExcelWorkSheet sheet)
		{
			this.Sheet = sheet;
		}

		public StringTreeNode GetTree()
		{
			return GetTree(true);
		}

		public StringTreeNode GetTree(bool trim)
		{
			Root = new StringTreeNode();
			if (Sheet != null)
			{
				Root.Value = Sheet.SheetName;
				Root.CellReference = new CellReference(Root.Value, "");
				FindEndMarker();
				ReadTemplateIntoTree(trim);
			}

			return Root;
		}

		protected ExcelWorkSheet Sheet;
		protected StringTreeNode Root;
		protected static readonly Regex EndMarker = new Regex(@"^\s*#end\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		protected int EndRow;
		protected int CurrentRow;

		protected void FindEndMarker()
		{
			var maxRowCount = Sheet.ParentExcelInterface.MaxRowCountSupportedByCurrentExcelFile;
			for (int row = 0; row < maxRowCount; row++)
			{
				if (EndMarker.IsMatch(Sheet[row, 0].ToString()))
				{
					EndRow = row;
					return;
				}
			}

			throw new TemplateDefinitionException(String.Format(@"There was no #End marker in the sheet ""{0}""", Root.Value), Root.CellReference);
		}

		protected void ReadTemplateIntoTree(bool trim)
		{
			var maxColCount = Sheet.ParentExcelInterface.MaxColCountSupportedByCurrentExcelFile;

			while (CurrentRow < EndRow)
			{
				var seenDataOnThisRow = false;

				for (var col = 0; col < maxColCount; col++)
				{
					var currentCell = Sheet[CurrentRow, col].ToString();

					if (trim)
					{
						currentCell = currentCell.Trim();
					}

					if (string.IsNullOrEmpty(currentCell))
					{
						if (seenDataOnThisRow)
						{
							break;
						}

						continue;
					}

					seenDataOnThisRow = true;

					var currentNode = new StringTreeNode
					{
						Value = currentCell,
						CellReference =
						{
							Cell = Sheet.ParentExcelInterface.GetCellReference(CurrentRow, col),
							SheetName = Root.CellReference.SheetName
						}
					};

					Root.LastCollectionAtLevel(col).Add(currentNode);
				}

				CurrentRow++;
			}
		}
	}
}
