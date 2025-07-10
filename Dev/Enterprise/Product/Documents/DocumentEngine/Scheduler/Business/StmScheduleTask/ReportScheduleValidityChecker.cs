using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleValidityChecker
	{
		readonly Report deserializedReport;
		readonly Report currentReport;
		readonly ReportScheduleTask parent;
		bool currentReportHasBeenAnalyzed;
		readonly ValidationErrorManager errorManager;

		internal ReportScheduleValidityChecker(ReportScheduleTask parent, Report deserializedReport, Report currentReport)
		{
			this.parent = parent;
			this.deserializedReport = deserializedReport;
			this.currentReport = currentReport;
			currentReportHasBeenAnalyzed = false;
			errorManager = new ValidationErrorManager();
		}

		internal void ClearErrorManager()
		{
			errorManager.ClearErrors();
		}

		internal bool IsDeserializedReportFiltersMatchingWithTemplateFilters()
		{
			if (!currentReportHasBeenAnalyzed)
			{
				currentReport.AnalyzeResettingLoadedFlagsAfterwards();
				currentReportHasBeenAnalyzed = true;
			}

			if (!CheckDeserializedReportFiltersSettingIsCompatibleWithTemplateFilters())
			{
				EmailErrorMessageToLocalUserToDealWithTheIssue();
				return false;
			}
			else
			{
				return true;
			}
		}
		bool CheckDeserializedReportFiltersSettingIsCompatibleWithTemplateFilters()
		{
			bool retValue = true;
			retValue &= IsDeserializedFilterCollectionCompatibleWithTemplate();
			retValue &= IsDeserializedGroupByCollectionCompatibleWithTemplate();
			retValue &= IsDeserializedSortOrderCollectionCompatibleWithTemplate();
			retValue &= IsDeserializedOptionalTemplateSheetCollectionCompatibleWithTemplate();
			return retValue;
		}

		bool IsDeserializedFilterCollectionCompatibleWithTemplate()
		{
			CollectionOfIFilter deserializedFilterCollection = deserializedReport.FilterCollection;
			CollectionOfIFilter currentFilterCollection = currentReport.FilterCollection;
			foreach (FilterField deserializedFilterField in deserializedFilterCollection)
			{
				if (string.IsNullOrEmpty(deserializedFilterField.DisplayName))
				{
					throw new DocumentEngineException("Can not check Filter's compatibility because its display name is empty");
				}

				if (!currentFilterCollection.Where(e => e is FilterField).Cast<FilterField>().Any(
					currentFilterField => currentFilterField.IsCompatibleWith(deserializedFilterField)))
				{
					string validationError = "\r\n" + Res.GetString("26752306-1052-4175-a279-4c1a7b2b40a9",
@"Filter display name: [{0}]
Filter type: [{1}]
Filter field list: [{2}]
Saved filter value: [{3}]",
						deserializedFilterField.DisplayName, deserializedFilterField.GetType().Name, deserializedFilterField.FieldName, deserializedFilterField.ValueAsObject.ToString());
					errorManager.AddFilterFieldError(validationError);
				}
			}
			return !errorManager.HasFilterFieldError();
		}

		bool IsDeserializedGroupByCollectionCompatibleWithTemplate()
		{
			GroupByCollection deserializedGroupByCollection = deserializedReport.GroupByCollection;
			GroupByCollection currentdGroupByCollection = currentReport.GroupByCollection;
			if (deserializedGroupByCollection.Count == 1 && string.IsNullOrEmpty(deserializedGroupByCollection[0].Text))
			{
				return true;
			}
			foreach (GroupBy deserializedGroupBy in deserializedGroupByCollection)
			{
				if (string.IsNullOrEmpty(deserializedGroupBy.DisplayName))
				{
					throw new DocumentEngineException("Can not check Groupby's compatibility because its display name is empty");
				}

				if (!currentdGroupByCollection.Where(e => e is GroupBy).Cast<GroupBy>().Any(
					currentGroupBy =>
						deserializedGroupBy.DisplayName == currentGroupBy.DisplayName &&
						deserializedGroupBy.FieldList == currentGroupBy.FieldList))
				{
					string validationError = "\r\n" + Res.GetString("1d89bbea-394c-4247-8b3d-be4bdbbb831f",
@"Group By display name: [{0}]
Group By field list: [{1}]",
						deserializedGroupBy.DisplayName, deserializedGroupBy.FieldList);
					errorManager.AddGroubByError(validationError);
				}
			}
			return !errorManager.HasGroupByError();
		}

		bool IsDeserializedSortOrderCollectionCompatibleWithTemplate()
		{
			SortOrderCollection deserializedSortOrderCollection = deserializedReport.SortOrderCollection;
			SortOrderCollection currentSortOrderCollection = currentReport.SortOrderCollection;
			if (deserializedSortOrderCollection.Count == 1 && string.IsNullOrEmpty(deserializedSortOrderCollection[0].Text))
			{
				return true;
			}
			foreach (RuntimeOptions.SortOrder deserializedSortOrder in deserializedSortOrderCollection)
			{
				if (string.IsNullOrEmpty(deserializedSortOrder.DisplayName))
				{
					throw new DocumentEngineException("Can not check SortOrder's compatibility because its display name is empty");
				}

				if (!currentSortOrderCollection.Where(e => e is RuntimeOptions.SortOrder).Cast<RuntimeOptions.SortOrder>().Any(
					currentSortOrder =>
						deserializedSortOrder.DisplayName == currentSortOrder.DisplayName &&
						deserializedSortOrder.FieldList == currentSortOrder.FieldList))
				{
					string validationError = "\r\n" + Res.GetString("f9074bda-d876-4af1-8232-46d6c12d68f4",
@"Sort Order display name: [{0}]
Sort Order field list: [{1}]",
						deserializedSortOrder.DisplayName, deserializedSortOrder.FieldList);
					errorManager.AddSortOrderError(validationError);
				}
			}
			return !errorManager.HasSortOrderError();
		}

		bool IsDeserializedOptionalTemplateSheetCollectionCompatibleWithTemplate()
		{
			OptionalTemplateSheetCollection deserializedOptionalTemplateSheetCollection = deserializedReport.OptionalTemplateSheetCollection;
			OptionalTemplateSheetCollection currentOptionalTemplateSheetCollection = currentReport.OptionalTemplateSheetCollection;

			foreach (OptionalTemplateSheet deserializedOptionalTemplateSheet in deserializedOptionalTemplateSheetCollection)
			{
				if (currentOptionalTemplateSheetCollection[deserializedOptionalTemplateSheet.Name] == null)
				{
					string validationError = "\r\n" + Res.GetString("be30a355-dea5-4cd3-89c6-e3f743ef7b84", @"Optional template sheet name: [{0}]", deserializedOptionalTemplateSheet.Name);
					errorManager.AddOptionalTemplateSheetError(validationError);
				}
			}
			return !errorManager.HasOptionalTemplateSheetError();
		}

		internal void EmailErrorMessageToLocalUserToDealWithTheIssue()
		{
			try
			{
				// notify user that the template has been changed since report was scheduled
				var emailTitle = Res.GetString("b32730e6-2c88-40de-8339-3ad853dfd2ff", "Error encountered while running schedule report");
				var emailMessage = Res.GetString("84c211d9-0711-493e-80f6-b3d7eca49f34",
@"Report Name: [{0}]

{1}

Errors Found
---------------
Report template has been modified since report was scheduled, current schedule is canceled since continue running it might cause unexpected behavior.
Please verify the report will work with the updated template then remark the schedule task as Active.

Validation failure details: {2}",
					parent != null ? parent.S5_ScheduleDescription.ToString() : "",
					currentReport.ToString(),
					errorManager.ToString());

				var sender = parent?.ErrorNotificationSender ?? new ScheduleReportErrorNotificationSender(parent, null);
				sender.SendErrorNotification(emailTitle, emailMessage, () =>
				{
					var email = new EmailDef();
					email.Subject = emailTitle;
					email.Body = emailMessage;

					if (parent != null)
					{
						var emailAddressForReportingErrors = parent.EmailAddressForReportingErrors;

						if (!string.IsNullOrWhiteSpace(emailAddressForReportingErrors))
						{
							email.AddRecipientForSystemCommunication(emailAddressForReportingErrors, RecipientDef.RecipientTypes.CC);
						}
					}

					var currentUser = Env.CurrentUser;
					if (currentUser != null && currentUser.IsActive && !string.IsNullOrEmpty(currentUser.EmailAddress))
					{
						email.AddRecipientForSystemCommunication(currentUser.EmailAddress, RecipientDef.RecipientTypes.TO);
						Env.OutgoingMailManager.CreateAndSave(email);
					}
					else
					{
						Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
					}
					return email.Recipients?.OfType<RecipientDef>().Select(x => x.Email).ToArray();
				}, true);
			}
			catch (EmailHasNoFromAddressException)
			{
				throw;
			}
			catch (EmailHasNoRecipientsException exception)
			{
				ErrorReporter.ReportOnce("Error sending report processing failure email (no recipients)", exception);
			}
			catch (EmailNotCompleteException exception)
			{
				ErrorReporter.ReportOnce("Error sending report processing failure email (email not complete)", exception);
			}
		}

		class ValidationErrorManager
		{
			readonly List<string> FilterFieldErrorList;
			readonly List<string> GroupByErrorList;
			readonly List<string> SortOrderErrorList;
			readonly List<string> OptionalTemplateSheetErrorList;

			public ValidationErrorManager()
			{
				FilterFieldErrorList = new List<string>();
				GroupByErrorList = new List<string>();
				SortOrderErrorList = new List<string>();
				OptionalTemplateSheetErrorList = new List<string>();
			}

			public void ClearErrors()
			{
				FilterFieldErrorList.Clear();
				GroupByErrorList.Clear();
				SortOrderErrorList.Clear();
				OptionalTemplateSheetErrorList.Clear();
			}

			public void AddFilterFieldError(string errorText)
			{
				FilterFieldErrorList.Add(errorText);
			}

			public void AddGroubByError(string errorText)
			{
				GroupByErrorList.Add(errorText);
			}

			public void AddSortOrderError(string errorText)
			{
				SortOrderErrorList.Add(errorText);
			}

			public void AddOptionalTemplateSheetError(string errorText)
			{
				OptionalTemplateSheetErrorList.Add(errorText);
			}

			public bool HasFilterFieldError()
			{
				return FilterFieldErrorList.Count > 0;
			}

			public bool HasGroupByError()
			{
				return GroupByErrorList.Count > 0;
			}

			public bool HasSortOrderError()
			{
				return SortOrderErrorList.Count > 0;
			}

			public bool HasOptionalTemplateSheetError()
			{
				return OptionalTemplateSheetErrorList.Count > 0;
			}

			public override string ToString()
			{
				string finalString = "";
				if (FilterFieldErrorList.Count > 0)
				{
					FilterFieldErrorList.Sort();
					finalString += "\r\n\r\n";
					finalString += Res.GetString("487e460d-108c-497c-974b-5dea7adda294", ">> Failed to find a compatible Filter Field, please check the updated report template and make sure the following Filter Field exists, if the filter does exist, please make sure it's compatible with the saved filter field value:");
					finalString += "\r\n";
					finalString += string.Join("\r\n", FilterFieldErrorList.ToArray());
				}

				if (GroupByErrorList.Count > 0)
				{
					GroupByErrorList.Sort();
					finalString += "\r\n\r\n";
					finalString += Res.GetString("18616cb1-3529-49e1-8372-b9b3c3eb5200", @">> Failed to find a compatible Group By, please check the updated report template and make sure the following Group By exists:");
					finalString += "\r\n";
					finalString += string.Join("\r\n", GroupByErrorList.ToArray());
				}

				if (SortOrderErrorList.Count > 0)
				{
					SortOrderErrorList.Sort();
					finalString += "\r\n\r\n";
					finalString += Res.GetString("cbf6c92e-5934-470f-9a2c-f98b752b3483", @">> Failed to find a compatible Sort Order, please check the updated report template and make sure the following Sort Order exists:");
					finalString += "\r\n";
					finalString += string.Join("\r\n", SortOrderErrorList.ToArray());
				}

				if (OptionalTemplateSheetErrorList.Count > 0)
				{
					OptionalTemplateSheetErrorList.Sort();
					finalString += "\r\n\r\n";
					finalString += Res.GetString("8b3519ce-d56c-4cc2-92b7-b367a65a4fac", @">> Failed to find a compatible Optional Template Sheet, please check the updated report template and make sure the following Optional Template Sheet exists:");
					finalString += "\r\n";
					finalString += string.Join("\r\n", OptionalTemplateSheetErrorList.ToArray());
				}
				return finalString;
			}
		}
	}
}
