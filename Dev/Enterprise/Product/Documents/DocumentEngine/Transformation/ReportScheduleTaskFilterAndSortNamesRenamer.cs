using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.Transformation
{
	[WTG.StaticAnalysis.Annotation.CodeAlive(@"This is used by reflection in:
		RenameTypeToMethodForScheduledCommuncationReporting.cs,
		ReplaceScheduledLegacyColdCallReportWithInquiryReport.cs,
		ReplaceScheduledLegacyColdCallReportWithInquiryReport.cs,
		UpdateSchduledInquiryReport.cs,
		UpdateScheduledCallReportingFilterAndSortNames.cs")]
	public class ReportScheduleTaskFilterAndSortNamesRenamer
	{
		public ReportScheduleTaskFilterAndSortNamesRenamer(DbConnection connection)
		{
			this.factory = new BusinessObjectFactory(connection);
		}

		readonly BusinessObjectFactory factory;

		public void Rename(
				IEnumerable<Tuple<SchemaColumn, object>> reportScheduleTasksQueryColumnFilters,
				string reportRename,
				Dictionary<string, string> filterDisplayNameRenames,
				Dictionary<string, string> filterFieldNameChanges,
				Dictionary<string, string> sortOrderDisplayNameRenames,
				Dictionary<string, string> sortOrderFieldListChanges,
				Dictionary<string, string> groupByDisplayNameRenames,
				Dictionary<string, string> groupByFieldListChanges,
				Dictionary<string, string> worksheetRenames,
				Dictionary<string, string> columnHeadingDisplayLabelRenames,
				Dictionary<string, string> columnHeadingDescriptionRenames,
				Dictionary<string, string> columnHeadingTextRenames)
		{
			var reportScheduleTasksQuery = new ZQuery();
			foreach (var columnFilter in reportScheduleTasksQueryColumnFilters)
			{
				reportScheduleTasksQuery.AddToFilter(columnFilter.Item1, columnFilter.Item2);
			}

			foreach (var reportScheduleTask in factory.Load<ReportScheduleTask>(reportScheduleTasksQuery))
			{
				ReportSerializationInfo deserialisedReportInfo;
				try
				{
					deserialisedReportInfo = reportScheduleTask.CreateReportFromTask();
				}
				catch (System.Runtime.Serialization.SerializationException)
				{
					continue;
				}
				catch (Exception ex)
				{
					if (ex.InnerException is System.Runtime.Serialization.SerializationException)
					{
						continue;
					}
					else
					{
						throw;
					}
				}

				if (deserialisedReportInfo != null)
				{
					var report = deserialisedReportInfo.Report;

					if (reportRename != null)
					{ report.Name = reportRename; }

					RenameFilterDisplayNames(report, filterDisplayNameRenames);
					UpdateFilterFieldNames(report, filterFieldNameChanges);

					RenameSortOrdersDisplayNames(report, sortOrderDisplayNameRenames);
					UpdateSortOrdersFieldLists(report, sortOrderFieldListChanges);

					RenameGroupByDisplayNames(report, groupByDisplayNameRenames);
					UpdateGroupByFieldLists(report, groupByFieldListChanges);

					UpdateColumnSettings(report, worksheetRenames, columnHeadingDisplayLabelRenames, columnHeadingDescriptionRenames, columnHeadingTextRenames);

					var json = JsonConverterHelper.Serialize(deserialisedReportInfo);
					reportScheduleTask.S5_ScheduleState = new ZBlob(Encoding.UTF8.GetBytes(json));
				}
			}

			factory.Save();
		}

		#region Rename / Update Filters

		void RenameFilterDisplayNames(Report report, Dictionary<string, string> filterDisplayNameRenames)
		{
			if (filterDisplayNameRenames != null)
			{
				foreach (FilterField filter in report.FilterCollection)
				{
					string newDisplayName;
					if (filterDisplayNameRenames.TryGetValue(filter.DisplayName, out newDisplayName))
					{
						filter.DisplayName = newDisplayName;
					}
				}
			}
		}

		void UpdateFilterFieldNames(Report report, Dictionary<string, string> filterFieldNameChanges)
		{
			if (filterFieldNameChanges != null)
			{
				foreach (FilterField filter in report.FilterCollection)
				{
					string newFieldName;
					if (filterFieldNameChanges.TryGetValue(filter.FieldName, out newFieldName))
					{
						filter.FieldName = newFieldName;
					}
				}
			}
		}

		#endregion

		#region Rename / Update Sort Orders

		void RenameSortOrdersDisplayNames(Report report, Dictionary<string, string> sortOrderDisplayNameRenames)
		{
			if (sortOrderDisplayNameRenames != null)
			{
				foreach (RuntimeOptions.SortOrder sortOrder in report.SortOrderCollection)
				{
					string newDisplayName;
					if (sortOrderDisplayNameRenames.TryGetValue(sortOrder.DisplayName, out newDisplayName))
					{
						sortOrder.DisplayName = newDisplayName;
					}
				}
			}
		}

		void UpdateSortOrdersFieldLists(Report report, Dictionary<string, string> sortOrderFieldListChanges)
		{
			if (sortOrderFieldListChanges != null)
			{
				foreach (RuntimeOptions.SortOrder sortOrder in report.SortOrderCollection)
				{
					string newFieldList;
					if (sortOrderFieldListChanges.TryGetValue(sortOrder.FieldList, out newFieldList))
					{
						sortOrder.FieldList = newFieldList;
					}
				}
			}
		}

		#endregion

		#region Rename / Update Group Bys

		void RenameGroupByDisplayNames(Report report, Dictionary<string, string> groupByDisplayNameRenames)
		{
			if (groupByDisplayNameRenames != null)
			{
				foreach (GroupBy groupBy in report.GroupByCollection)
				{
					string newDisplayName;
					if (groupByDisplayNameRenames.TryGetValue(groupBy.DisplayName, out newDisplayName))
					{
						groupBy.DisplayName = newDisplayName;
					}
				}
			}
		}

		void UpdateGroupByFieldLists(Report report, Dictionary<string, string> groupByFieldListChanges)
		{
			if (groupByFieldListChanges != null)
			{
				foreach (GroupBy groupBy in report.GroupByCollection)
				{
					string newFieldList;
					if (groupByFieldListChanges.TryGetValue(groupBy.FieldList, out newFieldList))
					{
						groupBy.FieldList = newFieldList;
					}
				}
			}
		}

		#endregion

		void UpdateColumnSettings(Report report,
			Dictionary<string, string> worksheetRenames,
			Dictionary<string, string> displayLabelRenames,
			Dictionary<string, string> descriptionRenames,
			Dictionary<string, string> headingTextRenames)
		{
			var newHeadingsXml = report.ColumnHeadingManager.headingsxml;
			newHeadingsXml = RegexReplaceUsingDictionary(newHeadingsXml, new Regex(@"<Name>([\w\s]+)</Name>"), worksheetRenames);
			newHeadingsXml = RegexReplaceUsingDictionary(newHeadingsXml, new Regex(@"<DisplayLabel>([\w\s]+)</DisplayLabel>"), displayLabelRenames);
			newHeadingsXml = RegexReplaceUsingDictionary(newHeadingsXml, new Regex(@"<Description>([\w\s]+)</Description>"), descriptionRenames);
			newHeadingsXml = RegexReplaceUsingDictionary(newHeadingsXml, new Regex(@"<HeadingText>([\w\s]+)</HeadingText>"), headingTextRenames);

			report.ColumnHeadingManager.OverrideHeadingsSerialization(newHeadingsXml);
		}

		string RegexReplaceUsingDictionary(string str, Regex regex, Dictionary<string, string> dic)
		{
			if (dic == null)
			{
				return str;
			}

			return regex.Replace(str, match =>
				{
					Capture capture = match.Groups[1];
					string key = capture.Value;
					string value;
					return (dic.TryGetValue(key, out value)) ? match.Value.Remove(capture.Index - match.Index, capture.Length).Insert(capture.Index - match.Index, value) : match.Value;
				});
		}
	}
}
