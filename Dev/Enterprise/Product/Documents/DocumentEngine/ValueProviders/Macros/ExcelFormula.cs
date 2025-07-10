using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ExcelFormula : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ExcelFormula({stringContainingExcelFormulaToCalculate})>",
				ResString.GetMultilingualString("c179a260-a0fc-48e3-95ff-7b4f0b06668f", @"The string content inside the macro will be treated as excel formulas and will be calculated after replacing.
Note: This macro only support default excel formulas."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<ExcelFormula(\"= AVERAGE(3,1,5)\")>", 3.00),
					("<ExcelFormula(\"= (2 + 2)/2\")>", 2.00) }
				);
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string innerContent = match.Groups["stringContainingExcelFormulaToCalc"].Value.UnEscapeAngleBrackets();

			object result = string.Empty;
			try
			{
				if (report.WorkSheetCurrentlyBeingProcessed != null)
				{
					using (Culture.SetTemporarily(Culture.Default))
					{
						var activeSheet = report.WorkSheetCurrentlyBeingProcessed;
						using (activeSheet.ParentExcelInterface.SetCellValueTemporarily(report.Renderer.CurrentRow, report.Renderer.CurrentColumn, new TFormula(innerContent)))
						{
							result = activeSheet.RecalcCell(report.Renderer.CurrentRow, report.Renderer.CurrentColumn, false);
						}
					}
				}
			}
			catch (InvalidCastException ex)
			{
				var message = ex.Message;

				message = Res.GetString("01D7AD42-1CC8-45F9-B27D-2864F1656900", "Cannot evaluate {0}. {1}", macro, message.ShrinkToMaxLength(200));
				report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error, ex));
			}
			catch (FlexCelCoreException ex)
			{
				result = string.Empty;
				string message = ex.Message;

				message = Res.GetString("6a2e91ed-2959-484b-988e-a4ed9d4c7303", "Error Replacing Macros in [{0}] - {1}", macro, message.ShrinkToMaxLength(200));
				report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string RegexPattern = @"^<[\s]*ExcelFormula[\s]*\((""|\\"")(?<stringContainingExcelFormulaToCalc>[^\\]*)(""|\\"")\)[\s]*>$";
		static readonly Regex fRegex = new Regex(RegexPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}
		protected override bool ShouldReplaceNestedMacrosWhileIrrisponsibleCore(string macro, Passes currentPass)
		{
			return Regex.IsMatch(macro) && !IsResponsibleForReplacing(macro, currentPass);
		}
	}
}
