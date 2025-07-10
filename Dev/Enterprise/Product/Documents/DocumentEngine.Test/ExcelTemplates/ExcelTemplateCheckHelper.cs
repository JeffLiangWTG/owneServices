using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing
{
	public static class ExcelTemplateCheckHelper
	{
		public static HashSet<string> CheckDateRangeFilterInSQLQuery(Report report)
		{
			var result = new HashSet<string>();

			report.PrepareForRender();
			var testReportAnalyser = report.Analyser;
			var dateFields = report.FilterCollection.Where(filter => filter is DateRangeField or DateField).ToList();

			foreach (var source in testReportAnalyser.ReportSQLSources)
			{
				Match parameters = ReportDataProvider.adsRegex.Match(source.TableNameAndSelectStatement);
				var originalQuery = parameters.Groups["originalQuery"].Value;

				foreach (var field in dateFields)
				{
					#region Check if is DateRangeField

					if (field is DateRangeField dateRangeField && !(dateRangeField.SubstituteMaxDateForNullTo || dateRangeField.SubstituteMinDateForNullFrom))
					{
						if (CheckSQLSourceIsInvalidOfDateRangeField(originalQuery, dateRangeField.DisplayName))
						{
							result.Add(originalQuery);
						}
					}

					#endregion

					#region Check if is DateField

					if (field is DateField dateField)
					{
						if (CheckSQLSourceIsInvalidOfDateField(originalQuery, dateField.DisplayName))
						{
							result.Add(originalQuery);
						}
					}

					#endregion
				}
			}
			return result;
		}

		static bool CheckSQLSourceIsInvalidOfDateRangeField(string sqlSource, string filterName)
		{
			sqlSource = sqlSource.Replace($"<{filterName}.FromDateForSQLParameter>", "");
			sqlSource = sqlSource.Replace($"<{filterName}.ToDateForSQLParameter>", "");
			sqlSource = sqlSource.Replace($"<{filterName}.ToNextDateForSQLParameter>", "");
			sqlSource = sqlSource.Replace($"<{filterName}.FromDateUtcForSQLParameter>", "");
			sqlSource = sqlSource.Replace($"<{filterName}.ToNextDateUtcForSQLParameter>", "");
			return new Regex(@$"<{filterName}\.[\w]+>").IsMatch(sqlSource);
		}

		static bool CheckSQLSourceIsInvalidOfDateField(string sqlSource, string filterName)
		{
			sqlSource = sqlSource.Replace($"<{filterName}.ValueForSQLParameter>", "");
			sqlSource = sqlSource.Replace($"<{filterName}.ToNextDateForSQLParameter>", "");
			return new Regex(@$"<{filterName}\.[\w]+>").IsMatch(sqlSource);
		}
	}
}
