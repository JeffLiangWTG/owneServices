using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocBuilderTemplateDataTest : TestCaseWithFactory
	{
		public void TestUpperFormulaShouldNotBeUsedForHeadings()
		{
			var groupedErrors = new GroupedErrorList();
			var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);

			var regex = new Regex(@"(?<!<)UPPER\(.*\)(?!>)");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				var workSheet = excelInterface.WorkSheets[0];

				for (var row = 0; row < workSheet.RowCount; row++)
				{
					for (var column = 0; column < workSheet.ColumnCount; column++)
					{
						var cell = workSheet.GetCell(row, column);

						if (cell.IsFormula)
						{
							var cellFormat = workSheet.GetCellFormat(row, column);

							if (!cellFormat.IsBackgroundColorAutomatic && cellFormat.BackgroundColor.ToArgb() != Color.White.ToArgb())
							{
								if (regex.IsMatch(cell.ValueSourceText))
								{
									var error = string.Format("{0}:{1}", CellReference.GetCellRef(row, column), cell.ValueSourceText);
									groupedErrors.Add(template.SO_Name, error);
								}
							}
						}
					}
				}
			}

			groupedErrors.Assert(@"
Please replace usages of the =UPPER() Excel function with the <Upper()> macro. This is to avoid the pattern of
using the =UPPER() function on a cell reference pointing to another cell where the desired result is actually
rendered. If you use the <Upper()> macro instead, you keep the cell content in that one cell.

For example:
{C}-F[=UPPER(BU834)]   {BS}-[<CompanyName>]

Should be:
{C}-[<Upper(""<CompanyName>"")>]
");
		}

		public void TestAllDocBuilderTemplatesHaveDocumentWorkSheet()
		{
			foreach (var template in GetAllDocBuilderTemplates())
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(template.SO_Template);

					var workSheetName = SectionRepository.DocumentWorkSheetName;
					var message = string.Format("DocBuilder template [{0}] should have a [{1}] work sheet.", template.SO_Name, workSheetName);
					AssertNotNull(message, excelInterface.WorkSheets.Find(workSheetName));
				}
			}
		}

		#region Implementation

		StmTemplateBase[] GetAllDocBuilderTemplates()
		{
			var result = new List<StmTemplateBase>();

			var query = new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.System);
			query.AddToFilter(JoinCondition.Or, StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.User);

			foreach (var template in Factory.Load<StmTemplateBase>(query))
			{
				if (template.IsDocBuilderStyle)
				{
					result.Add(template);
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}
