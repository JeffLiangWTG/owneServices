using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Total : ValueProvider, ITFormulaProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Total {fieldname}>",
				ResString.GetMultilingualString("e4696d97-8587-44a8-b084-760866b457bd", @"Returns the total value of a given field in the group footer where that field name was shown in the body of a section. 
Inserts a {0} Excel formula in its place summing the field specified across the range of rows in the body section in the final output.",
"+SUM(cell:cell)"),
				new List<(string example, object expectedResult)> { ((NoResString)"<Total InvoiceLines.Amount>", new TFormula("=SUM(B3:B7)", 0)) });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			object formula = 0;

			try
			{
				formula = DoReplacement(macro, report);
			}
			catch (FormulaProviderException ex)
			{
				var severity = report.IsReportForSectionPreview ? ReportProcessingErrorSeverity.WarningWithoutErrorReport : ReportProcessingErrorSeverity.Warning;
				ReportMacroError(report, ex.Message, severity);
			}
			catch (FormulaProviderNotReadyException ex)
			{
				ReportMacroError(report, ex.Message);
			}

			return formula;
		}

		protected virtual object DoReplacement(string macro, Report report)
		{
			object formula = 0;
			var match = Regex.Match(macro);
			var totalString = match.Groups[1].Value;

			if (report.Renderer.CurrentAreaToProcess != null)
			{
				var formulaProvider = GetFirstParentFormulaProvider(totalString, report.Renderer.CurrentAreaToProcess);
				if (formulaProvider != null)
				{
					formula = formulaProvider.GetFormula(totalString, report.Renderer.CurrentAreaToProcess.Parents, report.FileFormat);

					if (new Currency().Regex.IsMatch("<" + totalString + ">"))
					{
						formula = !(report.MacroTranslator.GetValue("<" + totalString + ">", Passes.SecondPass) is FormattedCellValue totalFormat) ? null : new FormattedCellValue((TFormula)formula, totalFormat.Format);
					}
					else if (new FormatNumber().Regex.IsMatch("<" + totalString + ">"))
					{
						formula = report.MacroTranslator.GetValue("<" + totalString + ">", Passes.SecondPass);
					}
				}
				else
				{
					var isFieldExisted = false;
					foreach (var area in report.Analyser.Areas)
					{
						if (area.FormulaProvider != null && area.FormulaProvider.ContainsColumn(totalString))
						{
							isFieldExisted = true;
							break;
						}
					}

					if (!isFieldExisted)
					{
						var formularProviderWithAllFields = new FormulaProvider(report.WorkSheetCurrentlyBeingProcessed);
						foreach (var section in report.Analyser.Sections)
						{
							foreach (var area in section.SectionBodyAndGroupByAreas)
							{
								section.GetFormularProviderWithAllFields(report, area, formularProviderWithAllFields);
							}

							section.GetFormularProviderWithAllFields(report, section.SectionFooter, formularProviderWithAllFields);
						}

						if (formularProviderWithAllFields.ContainsColumn(totalString))
						{
							isFieldExisted = true;
						}
					}

					if (!isFieldExisted)
					{
						throw new FormulaProviderException("Field " + totalString + " does not exist in the template!");
					}
				}
			}
			else
			{
				return null;
			}

			return formula;
		}

		FormulaProvider GetFirstParentFormulaProvider(string columnName, Area areaToCalculate)
		{
			FormulaProvider result = null;
			var formulaProviders = new FormulaProviderList();
			AddFormulaProviders(formulaProviders, columnName, areaToCalculate.Parents);

			if (formulaProviders.Count > 0)
			{
				result = formulaProviders[0];
			}

			return result;
		}

		void AddFormulaProviders(FormulaProviderList formulaProviders, string columnName, List<Area> areas)
		{
			if (areas != null)
			{
				foreach (var area in areas)
				{
					if (area.FormulaProvider != null && area.FormulaProvider.ContainsColumn(columnName))
					{
						formulaProviders.Add(area.FormulaProvider);
					}
					else
					{
						AddFormulaProviders(formulaProviders, columnName, area.Parents);
					}
				}
			}
		}
		#endregion

		public override Passes PassToStartReplacingOn => Passes.SecondPass;

		public override Regex Regex => RegexProvider.TotalMacroRegex;
	}
}
