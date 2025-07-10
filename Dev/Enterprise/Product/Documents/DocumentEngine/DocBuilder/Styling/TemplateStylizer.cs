using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	public class TemplateStylizer
	{
		public const int MaximumColumnsToStylize = 72;

		public TemplateStylizer(ExcelInterface excelInterface, DocBuilderTheme theme)
		{
			this.excelInterface = excelInterface;
			this.sheetCellManagers = new Dictionary<ExcelWorkSheet, StylizerCellManager[,]>();
			this.Theme = theme;
		}

		readonly ExcelInterface excelInterface;
		readonly Dictionary<ExcelWorkSheet, StylizerCellManager[,]> sheetCellManagers;

		internal readonly DocBuilderTheme Theme;

		public void Stylize()
		{
			if (!excelInterface.HasBeenStyled)
			{
				var cellStylizerProvider = new CellStylizerProvider(this);

				var workSheets = excelInterface.WorkSheets.ToList();
				foreach (var workSheet in workSheets)
				{
					var stylizeCommands = new List<StylizeCommand>();
					var sheet = workSheet.WorkSheetNumber;
					var rowCount = excelInterface.Xls.GetRowCount(sheet);
					for (var row = 0; row < rowCount; row++)
					{
						for (var column = 0; column < MaximumColumnsToStylize; column++)
						{
							var cellManager = GetCellManager(workSheet, row, column);

							var cellStylizer = cellStylizerProvider.Get(cellManager);
							if (cellStylizer != null)
							{
								stylizeCommands.Add(new StylizeCommand(cellManager, cellStylizer));
							}
						}
					}

					stylizeCommands.Sort(StylizeCommandComparisonByThemeBorderBrightness);

					foreach (var stylizeCommand in stylizeCommands)
					{
						stylizeCommand.Stylize();
					}

					if (sheetCellManagers.ContainsKey(workSheet))
					{
						foreach (var cellManager in sheetCellManagers[workSheet])
						{
							if (cellManager != null && cellManager.ShouldApply)
							{
								cellManager.Apply();
							}
						}
					}
				}

				excelInterface.HasBeenStyled = true;
			}
		}

		internal StylizerCellManager GetCellManager(ExcelWorkSheet workSheet, int row, int column)
		{
			if (!sheetCellManagers.ContainsKey(workSheet))
			{
				var rowCount = workSheet.ParentExcelInterface.Xls.GetRowCount(workSheet.WorkSheetNumber);
				sheetCellManagers[workSheet] = new StylizerCellManager[rowCount, MaximumColumnsToStylize];
			}

			var result = sheetCellManagers[workSheet][row, column] ?? (sheetCellManagers[workSheet][row, column] = new StylizerCellManager(workSheet, row, column));

			return result;
		}

		int StylizeCommandComparisonByThemeBorderBrightness(StylizeCommand x, StylizeCommand y)
		{
			return y.CellStylizer.ThemeBorderBrightness.CompareTo(x.CellStylizer.ThemeBorderBrightness);
		}

		class StylizeCommand
		{
			internal StylizeCommand(StylizerCellManager cellManager, RegionCellStylizer cellStylizer)
			{
				this.cellManager = cellManager;
				this.CellStylizer = cellStylizer;
			}

			readonly StylizerCellManager cellManager;
			readonly internal RegionCellStylizer CellStylizer;

			internal void Stylize()
			{
				CellStylizer.Stylize(cellManager);
			}

			internal void Apply()
			{
				cellManager.Apply();
			}
		}

		class CellStylizerProvider
		{
			internal CellStylizerProvider(TemplateStylizer templateStylizer)
			{
				this.availableCellStylizers = GetAvailableCellStylizers(templateStylizer);
			}

			readonly RegionCellStylizer[] availableCellStylizers;

			internal RegionCellStylizer Get(StylizerCellManager cellManager)
			{
				RegionCellStylizer result = null;

				foreach (var cellStylizer in availableCellStylizers)
				{
					if (cellStylizer.CanStylize(cellManager))
					{
						result = cellStylizer;
						break;
					}
				}

				return result;
			}

			RegionCellStylizer[] GetAvailableCellStylizers(TemplateStylizer templateStylizer)
			{
				return new RegionCellStylizer[]
				{
					new SecondaryBodyCellStylizer(templateStylizer),
					new PrimaryBodyCellStylizer(templateStylizer),
					new SecondaryHeadingCellStylizer(templateStylizer),
					new PrimaryHeadingCellStylizer(templateStylizer),
					new DocumentHeadingCellStylizer(templateStylizer),
					new PageNumberHeadingCellStylizer(templateStylizer),
				};
			}
		}
	}
}
