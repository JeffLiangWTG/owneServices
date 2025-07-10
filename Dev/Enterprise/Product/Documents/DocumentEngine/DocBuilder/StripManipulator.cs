using CargoWise.Common;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class StripManipulator
	{
		internal StripManipulator(ExcelWorkSheet destinationWorkSheet)
		{
			Argument.NotNull(destinationWorkSheet, "destinationWorkSheet");

			this.currentRow = 0;
			this.destinationWorkSheet = destinationWorkSheet;
		}

		int currentRow;
		readonly ExcelWorkSheet destinationWorkSheet;

		internal void InsertStrip(IDocumentConfigItem documentConfigItem, ExcelWorkSheet sourceWorkSheet, TemplateSection sourceTemplateSection)
		{
			foreach (var genericAreaID in GetGenericAreaIDs(documentConfigItem))
			{
				var isDummyArea = false;

				if (!string.IsNullOrEmpty(genericAreaID))
				{
					isDummyArea = genericAreaID.StartsWith("-");

					destinationWorkSheet.InsertRows(currentRow, 1);
					destinationWorkSheet[currentRow, 0] = isDummyArea ? genericAreaID.Substring(1) : genericAreaID;
					currentRow++;
				}

				if (!isDummyArea)
				{
					var sourceRowIndex = sourceTemplateSection.StartingRowNumber;
					var sourceRowCount = sourceTemplateSection.RowCount;

					CopyAndInsertRows(sourceWorkSheet, sourceRowIndex, sourceRowCount, sourceTemplateSection);
				}
			}
		}

		internal void CopyAndInsertRows(ExcelWorkSheet sourceWorkSheet, int sourceRowIndex, int sourceRowCount, TemplateSection section)
		{
			if (sourceRowCount > 0)
			{
				if (sourceWorkSheet == destinationWorkSheet)
				{
					sourceRowIndex += currentRow;
				}

				destinationWorkSheet.CopyAndInsertRows(sourceWorkSheet, sourceRowIndex, sourceRowCount, currentRow);

				TranslateStrip(section, sourceRowCount);

				currentRow += sourceRowCount;
			}
		}

		void TranslateStrip(TemplateSection section, int sourceRowCount)
		{
			if (section != null && section.Language == Res.CurrentLanguage)
			{
				return;
			}

			for (var row = currentRow; row < currentRow + sourceRowCount; row++)
			{
				for (var column = 1; column < destinationWorkSheet.ColumnCount; column++)
				{
					var cell = destinationWorkSheet.GetCell(row, column);
					var analyzer = new ExcelCellAnalyzer(cell);

					if (!cell.IsEmpty)
					{
						var allTranslatable = analyzer.GetTextToBeTranslated();
						if (allTranslatable.Length > 0)
						{
							foreach (var translatable in allTranslatable)
							{
								translatable.TranslatableText = DocBuilderResourceStrings.GetString(section, translatable.TranslatableText);
							}
							analyzer.SetTranslation();
						}
					}
				}
			}

			destinationWorkSheet.ReCalc();
		}

		string[] GetGenericAreaIDs(IDocumentConfigItem documentConfigItem)
		{
			string[] result = null;

			switch (documentConfigItem.SectionType)
			{
				case GenericSectionUsageList.Codes.PageHeaderAll:
					result = new string[] { (NoResString)"#DocumentHeader", (NoResString)"#PageHeader:StartFromSecondPage" };
					break;

				case GenericSectionUsageList.Codes.PageHeaderFirstPageOnly:
					result = new string[] { (NoResString)"#DocumentHeader" };
					break;

				case GenericSectionUsageList.Codes.PageHeaderStartFromSecondPage:
					result = new string[] { (NoResString)"#PageHeader:StartFromSecondPage" };
					break;

				case GenericSectionUsageList.Codes.BodySection:
					result = new string[] { (NoResString)"#SectionBody" };
					break;

				case GenericSectionUsageList.Codes.BodySectionExpanding:
					result = new string[] { string.Empty };
					break;

				case GenericSectionUsageList.Codes.PageFooterFirstPageOnly:
					result = new string[] { (NoResString)"#FirstPageFooter", (NoResString)"#OnlyOnePageFooter" };
					break;

				case GenericSectionUsageList.Codes.PageFooterAllExceptLastPage:
					result = new string[] { (NoResString)"#FirstPageFooter", (NoResString)"#PageFooter", (NoResString)"-#LastPageFooter" };
					break;

				case GenericSectionUsageList.Codes.PageFooterLastPage:
					result = new string[] { (NoResString)"#LastPageFooter", (NoResString)"#OnlyOnePageFooter" };
					break;

				case GenericSectionUsageList.Codes.PageFooterAll:
					result = new string[] { (NoResString)"#PageFooter", (NoResString)"#FirstPageFooter", (NoResString)"#OnlyOnePageFooter", (NoResString)"#LastPageFooter" };
					break;

				case GenericSectionUsageList.Codes.PageFooterFallbackDefault:
					result = new string[] { (NoResString)"#PageFooter" };
					break;

				case GenericSectionUsageList.Codes.FirstPageFooterUnlessOnlyOnePage:
					result = new string[] { (NoResString)"#FirstPageFooter" };
					break;

				case GenericSectionUsageList.Codes.FirstPageFooterWhenOnlyOnePage:
					result = new string[] { (NoResString)"#OnlyOnePageFooter" };
					break;

				case GenericSectionUsageList.Codes.BackPage:
					result = new string[] { (NoResString)"#BackPage" };
					break;

				case GenericSectionUsageList.Codes.BackPageFirstPageOnly:
					result = new string[] { (NoResString)"#BackPage:FirstPageOnly" };
					break;

				default:
					result = new string[] { string.Empty };
					break;
			}

			return result;
		}

		internal void RemoveUnnecessaryRows()
		{
			destinationWorkSheet.RemoveRows(currentRow, destinationWorkSheet.RowCount + 1);
		}
	}
}
