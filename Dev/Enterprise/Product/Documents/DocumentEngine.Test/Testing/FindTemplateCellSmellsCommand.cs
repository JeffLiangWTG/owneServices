using System.Collections.Generic;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FindTemplateCellSmellsCommand
	{
		internal FindTemplateCellSmellsCommand(StmTemplateBase template, IList<string> smells, params ITemplateCellSmellFinder[] smellFinders)
		{
			this.template = template;
			this.smellFinders = smellFinders;
			this.smells = smells;
		}

		readonly StmTemplateBase template;
		readonly ITemplateCellSmellFinder[] smellFinders;
		readonly IList<string> smells;

		internal void Execute()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				foreach (var workSheet in excelInterface.WorkSheets)
				{
					var section = "#Config";

					for (int row = 0; row < workSheet.RowCount; row++)
					{
						var cellContent = workSheet[row, 0].ToString();
						if (cellContent.StartsWith("#ConfigurableSection:", System.StringComparison.InvariantCultureIgnoreCase))
						{
							section = cellContent;
						}

						for (int column = 0; column < workSheet.ColumnCount; column++)
						{
							var cell = workSheet.GetCell(row, column);

							foreach (var smellFinder in smellFinders)
							{
								if (smellFinder.MatchesSmell(cell))
								{
									if (smells.Count >= 1000)
									{
										return;
									}

									var smell = string.Format(
@"{0}
Reference: {1}
{2}
",
										section,
										cell.Reference,
										GetCellFormatString(cell));

									smells.Add(smell);
								}
							}
						}
					}
				}
			}
		}

		string GetCellFormatString(ExcelCell cell)
		{
			var formatter = new CellFormatterExposingFormatting(cell.WorkSheet.ParentExcelInterface.Xls);
			return formatter.Format(cell.WorkSheet, cell.Row, cell.Column);
		}
	}
}
