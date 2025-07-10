using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocBuilder.Styling;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class SectionRepositoryTemplateTest : TestCaseWithFactory
	{
		public void TestAutoHeightIsNotNestedInAnotherMacro()
		{
			AssertTemplateCellsDoNotSmell(
				"AutoHeight macros should not be nested within another macro, because some macros require nested macros to be evaulated, and the AutoHeight will be replaced with an empty string.",
				new AutoHeightIsNotNestedInAnotherMacro());
		}

		public void TestAutoHeightIsNotUsedInIfFormulas()
		{
			AssertTemplateCellsDoNotSmell(
				"AutoHeight macros should not be used in if formulas, because if formula requires contents to be evaluated, and the AutoHeight will be replaced with an empty string.",
				new AutoHeightIsNotUsedInIfFormulas());
		}

		public void TestAllCellBackgroundAreColorizable()
		{
			var builder = new StringBuilder();

			builder.AppendLine("Cell background colors should use one of the following colours for colorization:\n");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var templateStylizer = new TemplateStylizer(excelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
				var cellStylizers = new List<RegionCellStylizer>()
				{
					new DocumentHeadingCellStylizer(templateStylizer),
					new PageNumberHeadingCellStylizer(templateStylizer),
					new PrimaryHeadingCellStylizer(templateStylizer),
					new SecondaryHeadingCellStylizer(templateStylizer),
					new PrimaryBodyCellStylizer(templateStylizer),
					new SecondaryBodyCellStylizer(templateStylizer)
				};

				foreach (var cellStylizer in cellStylizers)
				{
					builder.AppendLine(string.Format("  - R:{0} G:{1} B:{2} ({3})", cellStylizer.RegionIdentifyingBackgroundColor.R, cellStylizer.RegionIdentifyingBackgroundColor.G, cellStylizer.RegionIdentifyingBackgroundColor.B, cellStylizer.GetType().ToString()));
				}
			}

			AssertTemplateCellsDoNotSmell(
				builder.ToString(),
				new CellBackgroundColorSmellFinder());
		}

		public void TestAllBordersHaveAnAdjacentCellWithABackgroundColorOtherThanWhite()
		{
			AssertTemplateCellsDoNotSmell(
				@"Cell borders should have an adjacent cell with a background color other than white.
If your cell is part of a merged range and does not seem to have a white background, unmerge the range, apply the proper color background to your cell then re-do the merge.",
				new BordersHaveAnAdjacentCellWithABackgroundColorOtherThanWhite());
		}

		public void TestDocumentAndPageNumberHeadingsHaveBordersOnAllSides()
		{
			AssertTemplateCellsDoNotSmell(
				"Document and page number headings should have borders on all sides.",
				new DocumentHeadingsHaveBordersOnAllSides(),
				new PageNumberHeadingsHaveBordersOnAllSides());
		}

		public void TestExcelDateFormatingIsNotUsed()
		{
			AssertTemplateCellsDoNotSmell(
				"Do not use Excel date formatting, use <DateTimeAsString()> instead, otherwise dates will not be properly localized.",
				new DateFormatSmellFinder());
		}

		#region Smell Finders

		class AutoHeightIsNotNestedInAnotherMacro : ITemplateCellSmellFinder
		{
			readonly Regex regex = new Regex(@"^<[\s]*Auto[\s]*Height[\s]*(|\([\s]*(?<minimumRows>[[0-9].]+)[\s]*\))[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

			public bool MatchesSmell(ExcelCell cell)
			{
				var value = cell.Value;
				if (value != null)
				{
					var text = value.ToString();

					if (AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(text))
					{
						foreach (Match match in RegexProvider.OutermostMacroRegex.Matches(text))
						{
							if (regex.IsMatch(match.Value))
							{
								return false;
							}
						}

						return true;
					}
				}

				return false;
			}
		}

		class AutoHeightIsNotUsedInIfFormulas : ITemplateCellSmellFinder
		{
			public bool MatchesSmell(ExcelCell cell)
			{
				if (cell.IsFormula && cell.ValueSourceText.StartsWith("=if", System.StringComparison.InvariantCultureIgnoreCase))
				{
					var value = cell.Value;
					if (value != null)
					{
						var text = value.ToString();

						if (AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(text))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		class DocumentHeadingsHaveBordersOnAllSides : ITemplateCellSmellFinder
		{
			internal DocumentHeadingsHaveBordersOnAllSides()
			{
				using (var excelInterface = new ExcelInterface())
				{
					var templateStylizer = new TemplateStylizer(excelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
					var cellStylizer = new DocumentHeadingCellStylizer(templateStylizer);

					colorIdArgb = cellStylizer.RegionIdentifyingBackgroundColor.ToArgb();
				}
			}

			readonly int colorIdArgb;

			public bool MatchesSmell(ExcelCell cell)
			{
				var row = cell.Row;
				var column = cell.Column;
				var cellFormat = cell.WorkSheet.GetCellFormat(row, column);

				if (cellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb))
				{
					var rightCellFormat = cell.WorkSheet.GetCellFormat(row, column + 1);
					if (!rightCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Right.BorderStyle == CellBorderStyle.None)
					{
						return true;
					}

					var bottomCellFormat = cell.WorkSheet.GetCellFormat(row + 1, column);
					if (!bottomCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Bottom.BorderStyle == CellBorderStyle.None)
					{
						return true;
					}

					if (column > 0)
					{
						var leftCellFormat = cell.WorkSheet.GetCellFormat(row, column - 1);
						if (!leftCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Left.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
					else
					{
						if (cellFormat.Borders.Left.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}

					if (row > 0)
					{
						var topCellFormat = cell.WorkSheet.GetCellFormat(row - 1, column);
						if (!topCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Top.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
					else
					{
						if (cellFormat.Borders.Top.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		class PageNumberHeadingsHaveBordersOnAllSides : ITemplateCellSmellFinder
		{
			internal PageNumberHeadingsHaveBordersOnAllSides()
			{
				using (var excelInterface = new ExcelInterface())
				{
					var templateStylizer = new TemplateStylizer(excelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
					var cellStylizer = new PageNumberHeadingCellStylizer(templateStylizer);

					colorIdArgb = cellStylizer.RegionIdentifyingBackgroundColor.ToArgb();
				}
			}

			readonly int colorIdArgb;

			public bool MatchesSmell(ExcelCell cell)
			{
				var row = cell.Row;
				var column = cell.Column;
				var cellFormat = cell.WorkSheet.GetCellFormat(row, column);

				if (cellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb))
				{
					var rightCellFormat = cell.WorkSheet.GetCellFormat(row, column + 1);
					if (!rightCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Right.BorderStyle == CellBorderStyle.None)
					{
						return true;
					}

					var bottomCellFormat = cell.WorkSheet.GetCellFormat(row + 1, column);
					if (!bottomCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Bottom.BorderStyle == CellBorderStyle.None)
					{
						return true;
					}

					if (column > 0)
					{
						var leftCellFormat = cell.WorkSheet.GetCellFormat(row, column - 1);
						if (!leftCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Left.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
					else
					{
						if (cellFormat.Borders.Left.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}

					if (row > 0)
					{
						var topCellFormat = cell.WorkSheet.GetCellFormat(row - 1, column);
						if (!topCellFormat.BackgroundColor.ToArgb().Equals(colorIdArgb) && cellFormat.Borders.Top.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
					else
					{
						if (cellFormat.Borders.Top.BorderStyle == CellBorderStyle.None)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		class BordersHaveAnAdjacentCellWithABackgroundColorOtherThanWhite : ITemplateCellSmellFinder
		{
			public bool MatchesSmell(ExcelCell cell)
			{
				var row = cell.Row;
				var column = cell.Column;
				var cellFormat = cell.WorkSheet.GetCellFormat(row, column);

				if (cellFormat.IsBackgroundColorAutomatic || cellFormat.BackgroundColor.ToArgb().Equals(Color.White.ToArgb()))
				{
					if (cellFormat.Borders.Right.BorderStyle != CellBorderStyle.None)
					{
						var rightCellFormat = cell.WorkSheet.GetCellFormat(row, column + 1);
						if (rightCellFormat.IsBackgroundColorAutomatic || rightCellFormat.BackgroundColor.ToArgb().Equals(Color.White.ToArgb()))
						{
							return true;
						}
					}

					if (cellFormat.Borders.Bottom.BorderStyle != CellBorderStyle.None)
					{
						var bottomCellFormat = cell.WorkSheet.GetCellFormat(row + 1, column);
						if (bottomCellFormat.IsBackgroundColorAutomatic || bottomCellFormat.BackgroundColor.ToArgb().Equals(Color.White.ToArgb()))
						{
							return true;
						}
					}

					if (column > 0 && cellFormat.Borders.Left.BorderStyle != CellBorderStyle.None)
					{
						var leftCellFormat = cell.WorkSheet.GetCellFormat(row, column - 1);
						if (leftCellFormat.IsBackgroundColorAutomatic || leftCellFormat.BackgroundColor.ToArgb().Equals(Color.White.ToArgb()))
						{
							return true;
						}
					}

					if (row > 0 && cellFormat.Borders.Top.BorderStyle != CellBorderStyle.None)
					{
						var topCellFormat = cell.WorkSheet.GetCellFormat(row - 1, column);
						if (topCellFormat.IsBackgroundColorAutomatic || topCellFormat.BackgroundColor.ToArgb().Equals(Color.White.ToArgb()))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		class CellBackgroundColorSmellFinder : ITemplateCellSmellFinder
		{
			internal CellBackgroundColorSmellFinder()
			{
				using (var excelInterface = new ExcelInterface())
				{
					var templateStylizer = new TemplateStylizer(excelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
					var cellStylizers = new List<RegionCellStylizer>()
					{
						new DocumentHeadingCellStylizer(templateStylizer),
						new PageNumberHeadingCellStylizer(templateStylizer),
						new PrimaryHeadingCellStylizer(templateStylizer),
						new SecondaryHeadingCellStylizer(templateStylizer),
						new PrimaryBodyCellStylizer(templateStylizer),
						new SecondaryBodyCellStylizer(templateStylizer)
					};

					allowedBackgroundColorArgbs = new List<int>();
					foreach (var cellStylizer in cellStylizers)
					{
						allowedBackgroundColorArgbs.Add(cellStylizer.RegionIdentifyingBackgroundColor.ToArgb());
					}
				}
			}

			readonly List<int> allowedBackgroundColorArgbs;

			public bool MatchesSmell(ExcelCell cell)
			{
				var result = false;
				var cellFormat = cell.WorkSheet.GetCellFormat(cell.Row, cell.Column);

				if (!cellFormat.IsBackgroundColorAutomatic)
				{
					var backgroundColorArgb = cellFormat.BackgroundColor.ToArgb();

					result = !allowedBackgroundColorArgbs.Contains(backgroundColorArgb);
				}

				return result;
			}
		}

		class DateFormatSmellFinder : ITemplateCellSmellFinder
		{
			readonly List<string> dateFormats = new List<string>() { "dd/mm/YYYY", "d/m/yy;@", "[$-F800]dddd\\,\\ mmmm\\ dd\\,\\ yyyy", "d/m/yyyy;@", "d/mm/yyyy;@", "dd/mm/yy;@", "d/mm/yy;@", "dd/mm/yyyy;@", "[$-C09]dd\\-mmm\\-yy;@", "yyyy/mm/dd;@", "[$-C09]dd\\-mmmm\\-yyyy;@", "[$-C09]dddd\\,\\ d\\ mmmm\\ yyyy;@", "yyyy\\-mm\\-dd;@", "[$-C09]d\\ mmmm\\ yyyy;@", "yy/mm/dd;@" };

			public bool MatchesSmell(ExcelCell cell)
			{
				return !string.IsNullOrEmpty(cell.Format.FormatPattern) && dateFormats.Contains(cell.Format.FormatPattern);
			}
		}

		#endregion

		#region Implementation

		void AssertTemplateCellsDoNotSmell(string message, StmTemplateBase[] templates, params ITemplateCellSmellFinder[] smellFinders)
		{
			var groupedErrors = new GroupedErrorList();

			foreach (var template in templates)
			{
				var group = template.SO_Name;
				var smells = new List<string>();
				var command = new FindTemplateCellSmellsCommand(template, smells, smellFinders);

				command.Execute();
				groupedErrors.Add(group, smells.ToArray());
			}

			groupedErrors.Assert(message);
		}

		void AssertTemplateCellsDoNotSmell(string message, params ITemplateCellSmellFinder[] smellFinders)
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.User);
			query.AddToFilter(JoinCondition.Or, StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.System);

			var templates = Factory.Load<StmTemplateBase>(query);

			AssertTemplateCellsDoNotSmell(message, templates, smellFinders);
		}

		#endregion
	}
}
