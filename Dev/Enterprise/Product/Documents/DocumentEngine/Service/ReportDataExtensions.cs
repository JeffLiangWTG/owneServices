using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	static class ReportDataExtensions
	{
		public static ColumnConfigurationManager FillAndReturnReportConfiguration(this Report report, SelectedValueConfigurationData configurationData, ReportRunningError runningError)
		{
			var allConfigurations = report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations;
			var configuration = allConfigurations.FirstOrDefault(c => c.UniqueDescription == configurationData.UniqueDescription);
			if (configuration == null)
			{
				if (configurationData.LinkPk != Guid.Empty)
				{
					report.ColumnHeadingManager.LinkedLookupField.Value = configurationData.LinkPk;
				}

				var newConfiguration = new NewConfiguration(report.ColumnHeadingManager) { NewName = configurationData.UniqueDescription };
				if (newConfiguration.HasValidationErrors(runningError))
				{
					return null;
				}

				configuration = newConfiguration.NewManager;
			}

			configurationData.WorkSheets.CopyToWorkSheets(report.ColumnHeadingManager.CurrentConfiguration.Worksheets);
			report.FilterCollection.OfType<FilterField>().ForEach(f => f.SetFilterValue(configurationData.FilterData));
			report.FillReportGroupBy(configurationData.GroupBy);
			report.FillReportSortOrder(configurationData.SortOrder);
			report.Orientation = configurationData.Orientation;
			report.Parent.Language = configurationData.PrintLanguage;

			return configuration;
		}

		public static List<ConfigurationData> GetConfigurations(this ReportCommand reportCommand)
		{
			using var report = reportCommand.GetReport();
			return report.GetConfigurations();
		}

		public static List<ConfigurationData> GetConfigurations(this Report report)
		{
			var result = new List<ConfigurationData>();
			report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations.ForEach(c =>
			{
				var configurationData = new ConfigurationData();
				c.CopyToConfiguration(report, configurationData);
				result.Add(configurationData);
			});

			return result;
		}

		static void CopyToConfiguration(this ColumnConfigurationManager columnConfigurationManager, Report report, ConfigurationData configurationData)
		{
			columnConfigurationManager.Load(report);
			configurationData.ReportId = columnConfigurationManager.ReportID.ToGuid();
			configurationData.Description = columnConfigurationManager.Description;
			configurationData.UniqueDescription = columnConfigurationManager.UniqueDescription;
			configurationData.CanSaveAndDelete = columnConfigurationManager.CanSaveAndDelete;
			report.ColumnHeadingManager.CurrentConfiguration.Worksheets.CopyToWorkSheets(configurationData.WorkSheets);
			report.FilterCollection.OfType<FilterField>().ForEach(f => f.FillFilterData(configurationData.FilterData));
			if (columnConfigurationManager.LinkPK.IsValid)
			{
				configurationData.LinkPk = columnConfigurationManager.LinkPK.ToGuid();
			}

			configurationData.GroupBy = report.GroupByCollection.SelectedGroupBy?.DisplayName;
			configurationData.SortOrder = report.SortOrderCollection.SelectedOrder?.DisplayName;
			configurationData.Orientation = report.Orientation;
			configurationData.PrintLanguage = report.Parent.Language;

			if (columnConfigurationManager is DefaultTemplateConfigurationManager)
			{
				configurationData.Type = ConfigurationType.Default;
			}
			else if (columnConfigurationManager is CompanyDefaultConfigurationManager)
			{
				configurationData.Type = ConfigurationType.CompanyDefault;
			}
			else if (columnConfigurationManager is CombinedConfigurationManager)
			{
				configurationData.Type = ConfigurationType.Combined;
			}

			configurationData.IsReportTitleChangeable = columnConfigurationManager.HeadingManager.IsReportTitleChangeable;
			configurationData.BindTextInGui = columnConfigurationManager.ToString();
		}

		public static void FillReportData(this Report report, SelectedValueReportData reportData)
		{
			if (reportData.GroupBy != null)
			{
				report.FillReportGroupBy(reportData.GroupBy.GroupBy, reportData.GroupBy.BreakPage);
			}

			report.FillReportSortOrder(reportData.SortOrder);
			report.FillReportOptionalTemplates(reportData.OptionalTemplates);

			if (!report.ContainsAnyCustomisation)
			{
				report.Parent.DeliveryInstructions.Language = reportData.PrintLanguage;
			}

			if (report.Analyser.Config.PageStyle == PageStyles.Continuous)
			{
				report.Orientation = reportData.Orientation;
			}

			report.FilterCollection.OfType<FilterField>().ForEach(d => d.SetFilterValue(reportData.FilterData));
			reportData.WorkSheets.CopyToWorkSheets(report.ColumnHeadingManager.CurrentConfiguration.Worksheets);
		}

		static void FillReportGroupBy(this Report report, string groupBy, bool? breakPage = null)
		{
			if (breakPage != null)
			{
				report.GroupByCollection.BreakPageOverride = breakPage.Value;
			}

			if (!string.IsNullOrEmpty(groupBy))
			{
				foreach (GroupBy item in report.GroupByCollection)
				{
					item.Selected = item.DisplayName == groupBy;
				}
			}
		}

		static void FillReportSortOrder(this Report report, string sortOrder)
		{
			if (!string.IsNullOrEmpty(sortOrder))
			{
				foreach (RuntimeOptions.SortOrder item in report.SortOrderCollection)
				{
					item.Selected = item.DisplayName == sortOrder;
				}
			}
		}

		static void FillReportOptionalTemplates(this Report report, List<string> optionalTemplates)
		{
			foreach (OptionalTemplateSheet item in report.OptionalTemplateSheetCollection)
			{
				if (optionalTemplates.Any(o => o == item.DisplayName))
				{
					item.Selected = true;
				}
			}
		}

		public static ReportData GetReportData(this ReportCommand reportCommand, ReportRunningError runningError = null)
		{
			using var report = reportCommand.GetReport();
			if (report.HasProcessingErrors(runningError))
			{
				return null;
			}
			var result = new ReportData
			{
				Id = reportCommand.PK.ToGuid(),
				ReportName = reportCommand.SU_MenuName,
				GroupBys = new GroupByData { BreakPage = report.GroupByCollection.BreakPageOverride }
			};
			report.GroupByCollection.ToList().OfType<GroupBy>().Select(g => g.DisplayName).ForEach(result.GroupBys.GroupByCollection.Add);
			result.SortOrderCollection = report.SortOrderCollection.ToList().OfType<RuntimeOptions.SortOrder>().Select(s => s.DisplayName).ToArray();
			result.OptionalTemplateCollection = report.OptionalTemplateSheetCollection.ToList().OfType<OptionalTemplateSheet>().Select(o => o.DisplayName).ToArray();

			if (report.ContainsAnyCustomisation)
			{
				result.Language.HiddenInGui = true;
			}
			else
			{
				result.Language.PrintLanguage = report.Language;
				report.Parent.DeliveryInstructions.Languages.CopyToCodeDescriptionList(result.Language.LanguageList);
				result.Language.HiddenInGui = false;
			}

			if (report.Analyser.Config.PageStyle == PageStyles.Continuous)
			{
				result.Orientation.Orientation = report.Orientation;
				report.OrientationList.CopyToCodeDescriptionList(result.Orientation.OrientationList);
				result.Orientation.HiddenInGui = false;
			}
			else
			{
				result.Orientation.HiddenInGui = true;
			}

			report.FilterCollection.OfType<FilterField>().ForEach(d => d.FillFilterData(result.FilterData));
			result.LinkedLookupFilter = result.FilterData.LookupFilterCollection.FirstOrDefault(f => f.DisplayName == report.LinkedLookupField?.DisplayName);
			report.ColumnHeadingManager.CurrentConfiguration.Worksheets.CopyToWorkSheets(result.WorkSheets);
			return result;
		}

		internal static bool HasProcessingErrors(this Report report, ReportRunningError runningError = null)
		{
			if (report.ErrorManager is IHaveReportProcessingErrorsForGUI processingErrorManager)
			{
				var reportErrors = processingErrorManager.GetErrors().Where(e => e.Severity <= ReportProcessingErrorSeverity.ErrorWithoutErrorReport);
				if (reportErrors.Any())
				{
					runningError?.AddRunningError(ReportServiceErrorType.ValidationError, reportErrors.Select(e => e.Message).Aggregate((m, n) => m + System.Environment.NewLine + n));
					return true;
				}
			}
			return false;
		}

		internal static SelectedValueReportData GetSelectedValueReportData(this Report report)
		{
			var result = new SelectedValueReportData
			{
				Id = report.MenuItem.PK.ToGuid(),
				GroupBy = new SelectedValueGroupByData
				{
					BreakPage = report.GroupByCollection.BreakPageOverride,
					GroupBy = report.GroupByCollection.ToList().OfType<GroupBy>().FirstOrDefault(item => item.Selected)?.DisplayName,
				},
				SortOrder = report.SortOrderCollection.ToList().OfType<RuntimeOptions.SortOrder>().FirstOrDefault(item => item.Selected)?.DisplayName,
			};
			result.OptionalTemplates.AddRange(report.OptionalTemplateSheetCollection.ToList().OfType<OptionalTemplateSheet>().Where(item => item.Selected).Select(item => item.DisplayName));

			if (!report.ContainsAnyCustomisation)
			{
				result.PrintLanguage = report.Parent.DeliveryInstructions.Language;
			}

			if (report.Analyser.Config.PageStyle == PageStyles.Continuous)
			{
				result.Orientation = report.Orientation;
			}
			report.FilterCollection.OfType<FilterField>().ForEach(d => d.FillFilterData(result.FilterData));
			report.ColumnHeadingManager.CurrentConfiguration.Worksheets.CopyToSelectedValueWorkSheets(result.WorkSheets);
			return result;
		}

		static void CopyToWorkSheets(this WorksheetCollection originalWorkSheets, List<WorkSheetData> workSheets)
		{
			originalWorkSheets.OfType<Worksheet>().ForEach(s =>
			{
				var newWorkSheet = new WorkSheetData
				{
					Name = s.Name,
					NameLocalized = s.NameLocalized,
					Title = s.Title
				};

				s.ColumnHeadings.OfType<ColumnHeading>().ForEach(c =>
				{
					newWorkSheet.ColumnHeadings.Add(new ColumnHeadingData
					{
						Description = c.Description,
						DisplayLabel = c.DisplayLabel,
						OriginalColumnNumber = c.OriginalColumnNumber,
						HeadingText = c.HeadingText,
						TagName = c.TagName,
						Hidden = c.Hidden,
						WidthInPixels = c.WidthInPixels,
						CurrentPosition = c.CurrentPosition,
						HideIfDescriptionEmpty = c.HideIfDescriptionEmpty,
						BindTextInGui = c.ToString(),
						HeadingTextXMLFormat = c.HeadingText.KeepAlphanumericCharactersXMLFormatting(),
						TagNameXMLFormat = c.TagName.KeepAlphanumericCharactersXMLFormatting(),
						ShowPerformanceWarning = c.ShowPerformanceWarning,
					});
				});

				workSheets.Add(newWorkSheet);
			});
		}

		static void CopyToWorkSheets(this List<SelectedValueWorkSheetData> originalWorkSheets, WorksheetCollection workSheets)
		{
			originalWorkSheets.ForEach(s =>
			{
				var workSheet = workSheets[s.Name];
				workSheet.Title = s.Title;

				s.ColumnHeadings.ForEach(c =>
				{
					var columnHeading = workSheet.ColumnHeadings[c.DisplayLabel];
					columnHeading.HeadingText = c.HeadingText;
					columnHeading.TagName = c.TagName;
					columnHeading.Hidden = c.Hidden;
					columnHeading.WidthInPixels = c.WidthInPixels;
					columnHeading.CurrentPosition = c.CurrentPosition;
				});
			});
		}

		static void CopyToSelectedValueWorkSheets(this WorksheetCollection originalWorkSheets, List<SelectedValueWorkSheetData> workSheets)
		{
			originalWorkSheets.OfType<Worksheet>().ForEach(s =>
			{
				var newWorkSheet = new SelectedValueWorkSheetData
				{
					Name = s.Name,
					Title = s.Title,
				};

				s.ColumnHeadings.OfType<ColumnHeading>().ForEach(c => {
					newWorkSheet.ColumnHeadings.Add(new SelectedValueColumnHeadingData
					{
						DisplayLabel = c.DisplayLabel,
						HeadingText = c.HeadingText,
						TagName = c.TagName,
						Hidden = c.Hidden,
						WidthInPixels = c.WidthInPixels,
						CurrentPosition = c.CurrentPosition,
					});
				});
			});
		}

		public static string GetDependencyValueForLookupFilter(this ReportCommand reportCommand, string filterName, string selectedValue)
		{
			using (var report = reportCommand.GetReport())
			{
				var filter = report.FilterCollection[filterName];

				if (filter is LookupFilterFieldBase lookupFilter)
				{
					lookupFilter.DependencyValue = selectedValue;
					if (lookupFilter.ModuleID != null)
					{
						return lookupFilter.ModuleID.ToString();
					}
				}

				return ModuleIDs.NotAssigned.ToString();
			}
		}

		public static List<CodeDescription> GetDependencyValueForCodeListMultipleChoiceFilter(this ReportCommand reportCommand, string filterName, string selectedValue)
		{
			var result = new List<CodeDescription>();

			using (var report = reportCommand.GetReport())
			{
				var filter = report.FilterCollection[filterName];

				if (filter is CodeListMultipleChoice codeListMultipleChoice)
				{
					codeListMultipleChoice.DependencyValue = selectedValue;
					codeListMultipleChoice.List.CopyToCodeDescriptionList(result);
				}

				return result;
			}
		}

		public static void CopyToCodeDescriptionList(this ReadOnlyCodeDescriptionPairList pairList, List<CodeDescription> codeDescriptionList, Func<ICodeDescription, string> codeSelector = null, Func<ICodeDescription, string> descriptionSelector = null)
		{
			codeSelector ??= i => i.Code;
			descriptionSelector ??= i => i.Description; 
			pairList?.OfType<ICodeDescription>().Select(i => new CodeDescription { Code = codeSelector(i), Description = descriptionSelector(i), Pk = i.PK != null && Guid.TryParse(i.PK.ToString(), out var pk) ? pk : Guid.Empty }).ForEach(codeDescriptionList.Add);
		}

		public static void SyncToDeliveryInstructions(this DeliveryData deliveryData, DeliveryInstructions instructions, OrgContact currentContact)
		{
			instructions.PrinterDelivery.PrintQueuePK = deliveryData.PrintQueuePk;
			instructions.PrinterDelivery.NumberOfCopies = deliveryData.Copies;
			instructions.IsDraft = deliveryData.IsDraft;
			instructions.IncludeCoverNote = deliveryData.IncludeCoverNote;
			instructions.CoverNote = deliveryData.CoverNote;

			instructions.Recipients.RemoveAll();
			deliveryData.Contacts.ForEach(c =>
			{
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethodDescription = c.DeliveryMethod;
				if (currentContact != null && c.OrgHeaderPK == Guid.Empty)
				{
					contact.OrgHeaderPK = currentContact.OC_OH;
					contact.Name = currentContact.OC_ContactName;
				}
				else
				{
					if (!contact.OrgAddressPKInfo.ReadOnly)
					{
						contact.OrgHeaderPK = c.OrgHeaderPK;
					}
					if (!contact.NameInfo.ReadOnly)
					{
						contact.Name = c.ContactName;
					}
				}
				if (!contact.AttachmentTypeInfo.ReadOnly)
				{
					contact.AttachmentType = c.AttachmentType;
				}
				if (!contact.DeliveryAddressInfo.ReadOnly)
				{
					contact.DeliveryAddress = c.EmailOrFax;
				}
				if (!contact.EmailCarbonCopyRecipientsAsStringInfo.ReadOnly)
				{
					contact.EmailCarbonCopyRecipientsAsString = c.EmailCC;
				}
				if (!contact.EmailBlindCarbonCopyRecipientsAsStringInfo.ReadOnly)
				{
					contact.EmailBlindCarbonCopyRecipientsAsString = c.EmailBCC;
				}
				if (!contact.EmailFromAddressWithTypeInfo.ReadOnly)
				{
					contact.EmailFromAddressWithType = c.SendFrom;
				}
				if (!contact.EmailSubjectMacroInfo.ReadOnly)
				{
					contact.EmailSubjectMacro = c.EmailSubject;
				}
				if (!contact.SalutationInfo.ReadOnly)
				{
					contact.Salutation = c.Salutation;
				}
			});
		}

		public static bool HasValidationErrors(this BusinessObject businessObject, ReportRunningError runningError)
		{
			businessObject.RunPreSaveValidation();
			if (businessObject.HasErrors)
			{
				var errors = new ReportNotificationCollector(businessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetFatalNotifications().GetUniqueMessageList();
				runningError.ErrorType = ReportServiceErrorType.ValidationError;
				runningError.Errors.AddRange(errors);
				return true;
			}

			return false;
		}

		public static List<ReportSummaryData> ConvertToReportSummaryDataCollection<T>(this BusinessObjectCollection<T> reportCommandCollection) where T : StmMenuItem
		{
			var result = new List<ReportSummaryData>();
			foreach (var reportCommand in reportCommandCollection.OfType<ReportCommand>())
			{
				result.Add(new ReportSummaryData
				{
					Id = reportCommand.PK.ToGuid(),
					IsClientSpecific = reportCommand.SU_IsClientSpecific,
					IsPublished = reportCommand.SU_IsPublished,
					IsSystemDefined = reportCommand.SU_IsSystemDefined,
					ReportDescription = reportCommand.SU_Hint,
					ReportName = reportCommand.SU_MenuName
				});
			}

			return result;
		}

		public static List<ZString> ToZStringList(this List<string> stringList)
		{
			return stringList.Select(s => new ZString(s)).ToList();
		}

		public static void AddRunningError(this ReportRunningError runningError, ReportServiceErrorType errorType, string errorMessage)
		{
			runningError.ErrorType = errorType;
			runningError.Errors.Add(errorMessage);
		}
	}
}
