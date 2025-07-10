using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Count : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Count([{tablename}, {logicalExpression}] )>",
				ResString.GetMultilingualString("ec6745f3-a434-4efa-890a-0bc0f8f44c9c", @"Returns the number of rows in the specified table.
Optional Parameters:
{0} is the table name you want to filter by.
{1} is a Logical expression which allows you to count the number of records with a specific condition.

If both parameters are not specified, the <Count()> macro will return:
  If inside a Group-by Area - the number of items in the current Group-by Area.
  If inside other types of Area that linked to a data source Area - the number of items in the data source of that Area",
"{tablename}", "{logicalExpression}"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<Count(Lines)>", 160),
					((NoResString)"<Count(Lines, <GST>==5 && (<GSTRate>==5))>", 4) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			try
			{
				var parameters = Regex.Split(macro).Where(s => !string.IsNullOrEmpty(s)).ToList();
				string tableName = parameters.Count > 0 ? parameters[0].Trim() : string.Empty;
				if (!string.IsNullOrEmpty(tableName))
				{
					IDataRowSource dataRowSource = null;

					try
					{
						dataRowSource = report.DataProvider.GetDataRowSource(tableName);
						if (dataRowSource != null)
						{
							if (parameters.Count == 2)
							{
								ZString filterExpression = parameters[1];
								if (!filterExpression.IsEmpty)
								{
									dataRowSource = dataRowSource.Filter(filterExpression);
								}
							}
							return dataRowSource != null ? dataRowSource.RowCount : 0;
						}
					}
					catch (FieldNotFoundException ex)
					{
						ReportMissingTable(macro, report, ex);
						return 0;
					}
				}
				else if (report.Renderer?.CurrentAreaToProcess is GroupByArea)
				{
					return ((GroupByArea)report.Renderer.CurrentAreaToProcess).RowCount;
				}
				else if (report.Renderer?.CurrentAreaToProcess?.DataSourceArea != null)
				{
					return report.Renderer.CurrentAreaToProcess.DataSourceArea.RowCount;
				}
			}
			catch (DataProviderException ex)
			{
				ReportMissingTable(macro, report, ex);
			}
			return 0;
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			return false;
		}

		public override bool EvaluateAllInnerMacrosWhenGettingReplacement => true;

		void ReportMissingTable(string macro, Report report, Exception ex)
		{
			ReportMacroError(report, Res.GetString("45e96f4b-2ab2-4ac8-9488-3668eef637a2", "Evaluating {0} :- {1}", macro, ex.Message));
		}

		public override Regex Regex => fRegex;
		static readonly Regex fRegex = new Regex(@"^<\s*Count\s*\(\s*([^\s,]+)?\s*(?:,([^,.]+))?\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
