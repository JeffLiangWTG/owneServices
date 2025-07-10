using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class SelectList : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<{ReportDataSourceName}.SelectList>",
				ResString.GetMultilingualString("ebcdaad8-b665-4fc4-bd7c-41bfc6ce6737", "Returns a syntactically correct SQL Select statement for all the columns actually shown or referenced in the Excel template for a given Report Data Source. The syntax of the macro is <{ReportDataSourceName}.SelectList>"),
				new List<(string example, object expectedResult)> { ("<ReportData.SelectList>", "[AL_LINEAMOUNT], [AL_LINETYPE], [AL_EXCHANGERATE], [AL_POSTPERIOD], [AL_UNITQTY], [AL_UNITPRICE], [JH_A_JOP], [JH_PK], [JH_OA_LOCALCHARGESADDR], [JH_ISPROFITSHAREPOSTED], [JH_OA_AGENTCOLLECTADDR], [AL_POSTTOGL], [JH_JOBNUM], [AL_JH], [AL_OSAMOUNT]") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			if (match.Success)
			{
				string result = null;

				string tableName = match.Groups[1].Value.ToUpperInvariant();
				string selectListName = report.WorkSheetCurrentlyBeingProcessed.SheetName + "!" + tableName;
				if (!selectListsCache.TryGetValue(selectListName, out result))
				{
					result = GetSelectList(tableName, report);
					if (string.IsNullOrWhiteSpace(result))
					{
						result = "*";
					}
					selectListsCache[selectListName] = result;
				}
				return result;
			}
			else
			{
				throw new DocumentEngineException(string.Format("Data source not correctly defined on select list macro: {0}", macro));
			}
		}
		readonly Dictionary<string, string> selectListsCache = new Dictionary<string, string>();

		protected override void ResetCore()
		{
			selectListsCache.Clear();
		}

		string GetSelectList(string tableName, Report report)
		{
			var fields = new List<string>();
			var removedAreas = (report.Renderer as SingleSectionBodyDataAreaReportRenderer)?.RemovedAreasWithFields;
			var areas = removedAreas == null ? report.Analyser.Areas : report.Analyser.Areas.Union(removedAreas);
			foreach (var area in areas)
			{
				List<string> usedDataFileds;
				if (area.AllDatafields.TryGetValue(tableName, out usedDataFileds))
				{
					foreach (var field in usedDataFileds)
					{
						if (!fields.Contains(field.ToUpperInvariant()))
						{
							fields.Add(field.ToUpperInvariant());
						}
					}
				}
			}
			var result = string.Join("], [", fields.ToArray());
			return string.IsNullOrEmpty(result) ? result : "[" + result + "]";
		}

		public override System.Text.RegularExpressions.Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex("^" + RegexProvider.SelectListMacroRegex.ToString() + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
