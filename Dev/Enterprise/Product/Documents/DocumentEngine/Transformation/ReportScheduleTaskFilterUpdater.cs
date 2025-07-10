using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Transformation
{
	[WTG.StaticAnalysis.Annotation.CodeAlive(@"This is used by reflection in:
		ReplaceScheduledLegacyColdCallReportWithInquiryReport.cs,
		UpdateSchduledInquiryReport.cs")]
	class ReportScheduleTaskFilterUpdater
	{
		public ReportScheduleTaskFilterUpdater(DbConnection connection)
		{
			this.factory = new BusinessObjectFactory(connection);
		}

		readonly BusinessObjectFactory factory;

		public void UpdateFilters(
			IEnumerable<Tuple<SchemaColumn, object>> reportScheduleTasksQueryColumnFilters,
			IEnumerable<Tuple<string, string[]>> filtersToAdd,
			Dictionary<string, Tuple<string, string>> filtersToReplace,
			HashSet<string> filtersToRemove)
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

					AddFilters(report, filtersToAdd);
					ReplaceFilters(report, filtersToReplace);
					RemoveFilters(report, filtersToRemove);

					//To avoid empty ReportID exception when serializing
					report.ColumnHeadingManager.OverrideHeadingsSerialization(report.ColumnHeadingManager.headingsxml);
					var json = JsonConverterHelper.Serialize(deserialisedReportInfo);
					reportScheduleTask.S5_ScheduleState = new ZBlob(Encoding.UTF8.GetBytes(json));
				}
			}

			factory.Save();
		}

		void AddFilters(Report report, IEnumerable<Tuple<string, string[]>> filtersToAdd)
		{
			if (filtersToAdd != null)
			{
				foreach (var filterToAdd in filtersToAdd)
				{
					AddFilterDelegateLookup[filterToAdd.Item1](report, filterToAdd.Item2);
				}
			}
		}

		void ReplaceFilters(Report report, Dictionary<string, Tuple<string, string>> filtersToReplace)
		{
			if (filtersToReplace != null)
			{
				for (int i = 0; i < report.FilterCollection.Count; i++)
				{
					var filter = report.FilterCollection[i] as FilterField;
					if (filter != null && filtersToReplace.ContainsKey(filter.DisplayName))
					{
						var filterReplaceTypes = filtersToReplace[filter.DisplayName];
						ReplaceFilterDelegateLookup[filterReplaceTypes](report, i);
					}
				}
			}
		}

		void RemoveFilters(Report report, HashSet<string> filtersToRemove)
		{
			if (filtersToRemove != null)
			{
				foreach (var filter in report.FilterCollection.ToArray())
				{
					FilterField filterField = filter as FilterField;
					if (filterField != null && filtersToRemove.Contains(filterField.DisplayName))
					{
						report.FilterCollection.Remove(filterField);
					}
				}
			}
		}

		#region Add Filter Delegates

		delegate void AddFilterDelegate(Report report, string[] filterValues);

		Dictionary<string, AddFilterDelegate> AddFilterDelegateLookup
		{
			get
			{
				if (addFilterDelegateLookup == null)
				{
					addFilterDelegateLookup = new Dictionary<string, AddFilterDelegate>();
					addFilterDelegateLookup.Add("CodeList", AddCodeListFilter);
				}
				return addFilterDelegateLookup;
			}
		}
		Dictionary<string, AddFilterDelegate> addFilterDelegateLookup;

		void AddCodeListFilter(Report report, string[] filterValues)
		{
			if (filterValues.Length == 3)
			{
				var newFilter = new CodeListMultipleChoice(factory);
				newFilter.DisplayName = filterValues[0];
				newFilter.FieldName = filterValues[1];
				newFilter.Value = filterValues[2];
				report.FilterCollection.Add(newFilter);
			}
		}

		#endregion

		#region Replace Filter Delegates

		delegate void ReplaceFilterDelegate(Report report, int originalFilterIndex);

		Dictionary<Tuple<string, string>, ReplaceFilterDelegate> ReplaceFilterDelegateLookup
		{
			get
			{
				if (replaceFilterDelegateLookup == null)
				{
					replaceFilterDelegateLookup = new Dictionary<Tuple<string, string>, ReplaceFilterDelegate>();
					replaceFilterDelegateLookup.Add(new Tuple<string, string>("MultipleChoice", "CheckBox"), ReplaceMultipleChoiceFilterWithOptionGroupFilter);
					replaceFilterDelegateLookup.Add(new Tuple<string, string>((NoResString)"Lookup", "MultipleSelectionLookup"), ReplaceLookupFilterWithMultipleSelectionLookupFilter);
				}
				return replaceFilterDelegateLookup;
			}
		}
		Dictionary<Tuple<string, string>, ReplaceFilterDelegate> replaceFilterDelegateLookup;

		void ReplaceMultipleChoiceFilterWithOptionGroupFilter(Report report, int originalFilterIndex)
		{
			MultipleChoice originalFilter = report.FilterCollection[originalFilterIndex] as MultipleChoice;

			if (originalFilter != null)
			{
				var newFilter = new OptionGroup(factory);
				newFilter.DisplayName = originalFilter.DisplayName;
				newFilter.FieldName = originalFilter.FieldName;

				List<OptionGroup.Item> options = new List<OptionGroup.Item>();
				foreach (CodeDescriptionPair choice in originalFilter.List)
				{
					options.Add(new OptionGroup.Item() { Code = choice.Code, Desc = choice.Description, Value = (choice.Code == (string)originalFilter.Value) });
				}
				newFilter.AddAllOptions(options);

				report.FilterCollection[originalFilterIndex] = newFilter;
			}
		}

		void ReplaceLookupFilterWithMultipleSelectionLookupFilter(Report report, int originalFilterIndex)
		{
			LookupField originalFilter = report.FilterCollection[originalFilterIndex] as LookupField;

			if (originalFilter != null)
			{
				var newFilter = new MultipleSelectionLookup(factory);
				newFilter.DisplayName = originalFilter.DisplayName;
				newFilter.FieldName = originalFilter.FieldName;
				newFilter.SetCollectionProvider(originalFilter.CollectionProvider);

				Type bizoType = originalFilter.CollectionProvider.Collection.TypeOfElements;
				var bizo = factory.Load(bizoType, originalFilter.Value);
				string codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(bizoType);
				if (bizo != null && !string.IsNullOrEmpty(codePropertyName))
				{
					newFilter.ValueAsStringForSerialisation = bizo[codePropertyName].ToString();
				}

				report.FilterCollection[originalFilterIndex] = newFilter;
			}
		}

		#endregion
	}
}
