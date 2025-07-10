using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	class DBOrBOValueProvider : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<{tablename}.{columnname}>",
				ResString.GetMultilingualString("4cf02043-dafc-421e-a603-714cf3d25225",
				@"Will return the value of the {0} specified in the table {1}. Functionality primarily designed for use in Reports. Is the way most Reports specify a {2}.",
				"{columnname}", "{tablename}", "{fieldname}"),
				new List<(string example, object expectedResult)> { ("<Header.Number>", "Header1                                 ") });
		}

		#region GetReplacementCore

		protected override object GetReplacementCore(string macro, Report report)
		{
			try
			{
				macro = macro.Substring(1);
				macro = macro.Substring(0, macro.Length - 1);
				macro = macro.Trim();
				object fieldValue = GetFieldValue(report, macro);

				if (report.IsReportForTextMacroProcessor && fieldValue == null)
				{
					string message = Res.GetString("e8072235-4cf9-4bd1-b7c6-e2ed9eecdf1e", "Macro evaluated to null: {0}", macro);
					report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning));
					return string.Empty;
				}
				else
				{
					return (fieldValue != null) ? ProcessValue(fieldValue, report) : "";
				}
			}
			catch (TargetInvocationException ex1)
			{
				throw new DocumentEngineException("Report name: " + report.Name, ex1);
			}
			catch (InvalidOperationException ex)
			{
				var innerEx = ex.GetFirstOccurrenceOfException<InvalidDocumentWrapperParameterException>();
				if (innerEx != null)
				{
					ProcessParameterEx(innerEx, report);
					return string.Empty;
				}

				var conversionEx = ex.GetFirstOccurrenceOfException<DocumentTypeConversionFailedException>();
				if (conversionEx != null)
				{
					ProcessConversionEx(conversionEx, report);
					return string.Empty;
				}

				throw;
			}
			catch (BODocDataProviderCollectionFormatException ex)
			{
				ProcessFormatEx(ex, report);
				return string.Empty;
			}
			catch (DocumentTypeConversionFailedException ex1)
			{
				ProcessConversionEx(ex1, report);
				return string.Empty;
			}
			catch (BODocDataProviderCollectionFindException ex1)
			{
				ProcessFindEx(ex1, report);
				return string.Empty;
			}
		}

		static void ProcessConversionEx(DocumentTypeConversionFailedException ex, Report report)
		{
			string message = Res.GetString("faccc643-ae54-403f-abb0-1de3542746dc", "Conversion Error: {0}", ex.Message);
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
		}

		static void ProcessParameterEx(InvalidDocumentWrapperParameterException ex, Report report)
		{
			string message = Res.GetString("36005dda-9e1e-4df6-b09d-748d8cd7c924", "Document Creation Error: {0}", ex.Message);
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
		}

		static void ProcessFormatEx(BODocDataProviderCollectionFormatException ex, Report report)
		{
			string message = Res.GetString("71a828de-ff22-4454-98c5-5995efdf3997", "Collection Formatting Error: {0}", ex.Message);
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
		}

		static void ProcessFindEx(BODocDataProviderCollectionFindException ex, Report report)
		{
			string message = Res.GetString("46777e9a-73e1-4823-8b0f-2af47130a925", "Find Evaluation Error: {0}", ex.Message);
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
		}

		protected object GetFieldValue(Report report, string macro)
		{
			object result = null;

			if (report.Renderer != null && report.Renderer.CurrentAreaToProcess != null)
			{
				result = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, macro);
			}
			else
			{
				result = report.DataProvider.GetColumnValue(null, 0, macro);
			}

			return result;
		}

		protected object ProcessValue(object value, Report report)
		{
			if (value is byte[])
			{
				return ProcessBytes((byte[])value, report);
			}

			if (value is ZBlob)
			{
				return ProcessBytes((ZBlob)value, report);
			}

			return value;
		}

		static object ProcessBytes(byte[] bytes, Report report)
		{
			if (Compressor.IsCompressed(bytes))
			{
				bytes = Compressor.Uncompress(bytes);
			}

			try
			{
				if (Enterprise.ZArchitecture.Core.ORtfTextUtil.IsRtf(bytes))
				{
					return Enterprise.ZArchitecture.Core.ORtfTextUtil.RtfToText(bytes).Replace("\r", "");
				}
			}
			catch (InvalidOperationException ex)
			{
				ProcessMissingContentEx(ex, report);
				return string.Empty;
			}

			return new System.Text.UTF8Encoding().GetString(bytes).Replace("\r", "");
		}

		static void ProcessMissingContentEx(InvalidOperationException ex, Report report)
		{
			var message = Res.GetString("819f1a0f-75b4-4d07-879e-2ce132c4fad3", "RTF text parsing error, please check that the relevant data is defined correctly");
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error, ex));
		}

		#endregion

		public bool IsResponsibleForReplacingFromDataBaseOrBusinessObject(string macro, Report report)
		{
			if (IsResponsibleForReplacing(macro, Passes.FirstPass))// is in <Table.Column> format
			{
				return true;
			}

			if (report != null && report.DataProvider != null)
			{
				if (report.DataProvider is BusinessObjectDataProvider)
				{
					var isMacroWithAngleBrackets = macro.StartsWith("<") && macro.EndsWith(">");
					return (isMacroWithAngleBrackets && ((BusinessObjectDataProvider)report.DataProvider).DoesColumnExist(macro.Substring(1, macro.Length - 2)));
				}

				if (report.DataProvider is EmptyDataProvider)
				{
					return true;
				}
			}

			return false;
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		public override Regex Regex
		{
			get { return AtleastOneDotTableAndColumnNameInAngleBracketsRegex; }
		}
		static readonly Regex AtleastOneDotTableAndColumnNameInAngleBracketsRegex = new Regex(@"^<(?:[\s]*)([^>\. ()]+)(?:[\s]*)\.(?:[\s]*)([^> ()]+)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
