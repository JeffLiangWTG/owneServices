using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskScheduleValidation : StmScheduleTaskValidation
	{
		public ServiceTaskScheduleValidation(ServiceTaskSchedule parent)
			: base(parent)
		{
		}

		protected new ServiceTaskSchedule Parent
		{
			get { return (ServiceTaskSchedule)base.Parent; }
		}

		protected override void CheckS5_GB()
		{
			base.CheckS5_GB();

			var parent = Parent;
			if (!parent.S5_IsActive)
			{
				return;
			}

			var attributes = parent.StaticServiceAttributes;
			if (attributes == null)
			{
				return;
			}

			var requiredCountries = attributes.RequiresCompanyInCountry;

			if (!attributes.CanRunInAnyBranch)
			{
				CheckBranchIfRunInSpecificBranch(requiredCountries);
			}
			else if (!requiredCountries.IsNullOrEmpty())
			{
				CheckBranchIfRunInAnyBranchAndRequiresCompanyInCountry(requiredCountries);
			}
			else
			{
				return;
			}
		}

		void CheckBranchIfRunInSpecificBranch(string requiredCountries)
		{
			MandatoryValidation.CheckEntered(Parent.S5_GBInfo);

			var branch = Parent.Branch;
			if (branch == null)
			{
				return;
			}

			if (!branch.GB_IsActive)
			{
				Parent.S5_GBInfo.AddError(string.Format("The selected branch '{0}' is inactive.", branch.GB_Code));
				return;
			}

			if (requiredCountries.IsNullOrEmpty())
			{
				return;
			}

			var countryList = requiredCountries.Split(',');

			var branchCode = branch.GB_Code;
			var company = branch.Company;

			if (!countryList
				.Any(country => country == company.GC_RN_NKCountryCode || country == branch.HomePort?.RL_RN_NKCountryCode.ToString()))
			{
				Parent.S5_GBInfo.AddError(string.Format("The service task is unable to run as the branch selected '{0}' is invalid. The country code of the branch and the branch company must be {1}.",
					branchCode, BuildCountriesString(countryList)));
			}
		}

		string BuildCountriesString(string[] countryList)
		{
			var sb = new StringBuilder();

			var refCountries = Parent.Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryList));
			for (var i = 0; i < refCountries.Length; i++)
			{
				if (i > 0)
				{
					sb.Append(", or ");
				}
				sb.AppendFormat("'{0}' - {1}", refCountries[i].RN_Code, refCountries[i].RN_DescMultilingual.GetUnresolvedString());
			}

			return sb.ToString();
		}

		void CheckBranchIfRunInAnyBranchAndRequiresCompanyInCountry(string requiredCountries)
		{
			var activeCompanies = GlbCompany.GetActiveCompanies(countryCode: null, Parent.Factory)
				.OrderBy(c => c.GC_SystemCreateTimeUtc)
				.ThenByDescending(c => c.PK);

			var countryList = requiredCountries.Split(',');

			if (!activeCompanies.Any(c => countryList.Contains(c.GC_RN_NKCountryCode.ToString()))
				&& !activeCompanies
					.SelectMany(c => c.ActiveBranches
						.Where(b => countryList.Contains(b.HomePort?.RL_RN_NKCountryCode.ToString())))
					.Any())
			{
				Parent.S5_GBInfo.AddError(string.Format("No suitable branch found for the service task in the following required countries: {0}.", BuildCountriesString(countryList)));
			}
		}

		protected override void CheckS5_IsActive()
		{
			base.CheckS5_IsActive();

			CheckS5_IsActiveForMandatoryAndReadOnly();

			if (Parent.S5_IsActive)
			{
				var wasActive = (ZBool)Parent.S5_IsActiveInfo.OriginalValue;
				if (!wasActive && ParentHasInactiveBranch)
				{
					Parent.S5_IsActiveInfo.AddError(Res.GetString("51bc3973-33b5-44b6-a2d5-3ba63e9d9bc0", "You cannot activate a service task when the branch is inactive."));
				}
			}
			else
			{
				var attributes = Parent.StaticServiceAttributes;
				if (attributes != null && attributes.IsMandatory)
				{
					if (!string.IsNullOrEmpty(attributes.RequiresCompanyInCountry))
					{
						var countries = attributes.RequiresCompanyInCountry.Split(',').Select(c => c.Trim()).ToList();

						var companiesFilter = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, countries);
						companiesFilter.AddToFilter(GlbCompanySchema.GC_IsActive, true);
						var companies = Parent.Factory.Load<GlbCompany>(companiesFilter);

						var hasSuitableCompany = companies.Any(company => company.Branches.Any(branch => branch.GB_IsActive && countries.Contains(branch.GB_RN_NKCountryCode)));
						if (hasSuitableCompany)
						{
							Parent.S5_IsActiveInfo.AddError(Res.GetString("9c5e259a-d9d6-43b7-a8b9-81bde93941fe",
								"This service task is mandatory for active companies in the following country(s): {0}, and cannot be deactivated.", attributes.RequiresCompanyInCountry));
						}
					}
					else
					{
						Parent.S5_IsActiveInfo.AddError(Res.GetString("3A338A51-4382-4678-BDC3-6ED30AF9D46C", "This service task is mandatory and cannot be deactivated."));
					}
				}
			}
		}

		void CheckS5_IsActiveForMandatoryAndReadOnly()
		{
			var attributes = Parent.StaticServiceAttributes;

			if (attributes != null)
			{
				if (Parent.S5_IsActiveInfo.HasChanges)
				{
					if (!attributes.IsMandatory)
					{
						if ((attributes.IsScheduleReadOnly))
						{
							Parent.S5_IsActiveInfo.AddError(Res.GetString("0B0FFF71-FA40-4EDA-B49A-22BF8B37A879", "The configuration of this service task is read only, it cannot be modified."));
						}
						else if ((attributes.IsReadOnlyForWiseCloudClient && DataUtils.IsWiseTechGlobalDatabaseServer(Db.Connection)))
						{
							var loginName = EnvProxy.Instance?.CurrentUser?.LoginName;
							if (!string.IsNullOrEmpty(loginName) && !string.Equals(loginName, User.SupportUserName, StringComparison.OrdinalIgnoreCase))
							{
								Parent.S5_IsActiveInfo.AddError(Res.GetString("3D3458B5-0715-4910-B645-6B6757EA98D6", "The configuration of this service task can only be modified by CargoWise support."));
							}
						}
					}
				}
			}
		}

		protected override void CheckS5_TaskPeriodCount()
		{
			var maxValue = GetMaxPeriodCountValue(Parent.S5_TaskPeriod);

			if (Math.Abs(Parent.S5_TaskPeriodCount) > maxValue)
			{
				Parent.S5_TaskPeriodCountInfo.AddError($"Period Count is out of bounds for associated TimeSpan. Bounds are +/- {maxPeriodCountValues[Parent.S5_TaskPeriod]}.");
			}
			else
			{
				ValidateFrequency();
			}

			long GetMaxPeriodCountValue(string period) => maxPeriodCountValues.ContainsKey(period) ? maxPeriodCountValues[period] : (long)TimeSpan.MaxValue.TotalSeconds;
		}

		static readonly Dictionary<string, long> maxPeriodCountValues = new Dictionary<string, long>() {
				{ string.Empty, (long)TimeSpan.MaxValue.TotalSeconds },
				{ ScheduleRecurrenceType.Second, (long)TimeSpan.MaxValue.TotalSeconds },
				{ ScheduleRecurrenceType.Minute, (long)TimeSpan.MaxValue.TotalMinutes },
				{ ScheduleRecurrenceType.Hourly, (long)TimeSpan.MaxValue.TotalHours },
				{ ScheduleRecurrenceType.Daily, (long)TimeSpan.MaxValue.TotalDays },
				{ ScheduleRecurrenceType.Weekly, (long)TimeSpan.MaxValue.TotalDays / 7 },
				{ ScheduleRecurrenceType.Monthly, (long)TimeSpan.MaxValue.TotalDays / 28 },
				{ ScheduleRecurrenceType.Yearly, (long)TimeSpan.MaxValue.TotalSeconds }
			};

		bool ParentHasInactiveBranch => Parent.S5_GB.IsValid && !(Parent.Branch?.GB_IsActive ?? true);

		protected override void CheckS5_NextScheduledPrintRunTimeUtc()
		{
			var message = "";

			if (Parent.S5_NextScheduledPrintRunTimeUtc.IsValid && Parent.S5_NextScheduledPrintRunTimeUtcInfo.HasChanges)
			{
				var attributes = Parent.StaticServiceAttributes;
				if (attributes != null && attributes.IsMandatory && Parent.S5_IsActive)
				{
					if (Parent.S5_NextScheduledPrintRunTimeUtc > Parent.NextRunTimeOriginalValue && Globals.IsUserInteractive)
					{
						Parent.S5_NextScheduledPrintRunTimeUtcInfo.AddError(Res.GetString("CA75CC03-3E78-4C33-B366-FBC088272707", "Next runtime for mandatory service tasks cannot be postponed."));
					}
				}

				if (
					(Parent.S5_TaskPeriod == ScheduleRecurrenceType.Daily
				|| Parent.S5_TaskPeriod == ScheduleRecurrenceType.Weekly
				|| Parent.S5_TaskPeriod == ScheduleRecurrenceType.Monthly
				|| Parent.S5_TaskPeriod == ScheduleRecurrenceType.Yearly) &&
					Parent.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow &&
					!Parent.IsValidTimeOfDayToRun(out message))
				{
					//not 100% true, since some times in the past will allow the service task to run (anything within +/- 1 hour of the current date/time). But this is not only confusing to convey, but unnecessary.
					Parent.S5_NextScheduledPrintRunTimeUtcInfo.AddError(Res.GetString("f25180e6-e95c-4f12-a8e8-0a7921501e51",
						"The Next Run Time is in the past; please enter a valid Next Run Time (the present or anything in the future will work)."));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateSecondaryProcessesMaxCount();
			ValidateSecondaryProcessesMaxCountDescription();
			Parent.ClearRowNotifications();
			ValidateTaskPeriod();
			ValidateBranchInDatabase();
		}

		#region ValidateTaskPeriod

		public void ValidateTaskPeriod()
		{
			if (Parent.S5_TaskPeriod.IsEmpty)
			{
				Parent.AddRowError("The Service Task does not have a valid Recurrence Pattern. Edit the Service Task Schedule and enter a valid Recurrence Pattern.");
			}
		}

		#endregion

		#region ValidateFrequency

#if DEBUG
		public bool FrequencyValidationWasCalledForTest;
#endif

		public void ValidateFrequency()
		{
			if (!Parent.IsNudgeable)
			{
#if DEBUG
				FrequencyValidationWasCalledForTest = true;
#endif
				var attributes = Parent.StaticServiceAttributes;
				if (attributes != null)
				{
					var minimumPeriod = GetPeriodDuration(attributes.MinimumPeriod, TimeSpan.MinValue, isRandomPeriod: false);
					var maximumPeriod = GetPeriodDuration(attributes.MaximumPeriod, TimeSpan.MaxValue, isRandomPeriod: false);

					if (minimumPeriod == maximumPeriod)
					{
						if (Parent.SchedulePeriodDuration > maximumPeriod)
						{
							Parent.S5_TaskPeriodCountInfo.AddError(GetExactFrequencyMessage(attributes.MaximumPeriod));
						}
						else if (Parent.SchedulePeriodDuration < minimumPeriod)
						{
							Parent.S5_TaskPeriodCountInfo.AddError(GetExactFrequencyMessage(attributes.MinimumPeriod));
						}
					}
					else if (Parent.SchedulePeriodDuration < minimumPeriod)
					{
						Parent.S5_TaskPeriodCountInfo.AddError(GetMaximumFrequencyMessage(attributes.MinimumPeriod));
					}
					else if (Parent.SchedulePeriodDuration > maximumPeriod)
					{
						Parent.S5_TaskPeriodCountInfo.AddError(GetMinimumFrequencyMessage(attributes.MaximumPeriod));
					}
				}
			}
		}

		public static TimeSpan GetPeriodDuration(string s, TimeSpan defaultValue, bool isRandomPeriod)
		{
			int value;
			string recurrence;
			if (ParseFrequency(s, out value, out recurrence))
			{
				if (isRandomPeriod)
				{
					value = new Random().Next(value);
				}

				switch (recurrence)
				{
					case ScheduleRecurrenceType.Second:
						return TimeSpan.FromSeconds(value);

					case ScheduleRecurrenceType.Minute:
						return TimeSpan.FromMinutes(value);

					case ScheduleRecurrenceType.Hourly:
						return TimeSpan.FromHours(value);

					case ScheduleRecurrenceType.Daily:
						return TimeSpan.FromDays(value);

					case ScheduleRecurrenceType.Weekly:
						return TimeSpan.FromDays(value * 7);

					case ScheduleRecurrenceType.Monthly:
						return TimeSpan.FromDays(value * 28);  // use the shortest possible month

					case ScheduleRecurrenceType.Yearly:
						return TimeSpan.FromDays(value * 365);
				}
			}

			return defaultValue;
		}

		static string GetExactFrequencyMessage(string period)
		{
			return "The task should be run " + ToFrequencyString(period);
		}

		static string GetMaximumFrequencyMessage(string period)
		{
			return "The task schedule shouldn't be more frequent than " + ToFrequencyString(period);
		}

		static string GetMinimumFrequencyMessage(string period)
		{
			return "The task should be run at least " + ToFrequencyString(period);
		}

		static string ToFrequencyString(string s)
		{
			int value;
			string recurrence;
			if (ParseFrequency(s, out value, out recurrence))
			{
				switch (recurrence)
				{
					case ScheduleRecurrenceType.Second:
						return string.Format("every {0} second{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Minute:
						return string.Format("every {0} minute{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Hourly:
						return string.Format("every {0} hour{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Daily:
						return string.Format("every {0} day{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Weekly:
						return string.Format("every {0} week{1}", value, value > 1 ? "s" : "");

					case ScheduleRecurrenceType.Monthly:
						return string.Format("every {0} month{1}", value, value > 1 ? "s" : "");
				}
			}

			return "";
		}

		public static bool ParseFrequency(string s, out int value, out string recurrence)
		{
			if (string.IsNullOrEmpty(s))
			{
				value = 0;
				recurrence = null;
				return false;
			}

			s = s.Trim();
			var numLength = 0;
			for (; numLength < s.Length; numLength++)
			{
				if (!char.IsDigit(s, numLength))
				{
					break;
				}
			}

			var hasValue = int.TryParse(s.Substring(0, numLength), out value);
			switch (s.Substring(numLength).TrimStart().ToLowerInvariant())
			{
				case "s":
				case "second":
				case "seconds":
					recurrence = ScheduleRecurrenceType.Second;
					return hasValue;

				case "m":
				case "minute":
				case "minutes":
					recurrence = ScheduleRecurrenceType.Minute;
					return hasValue;

				case "h":
				case "hour":
				case "hours":
					recurrence = ScheduleRecurrenceType.Hourly;
					return hasValue;

				case "d":
				case "day":
				case "days":
					recurrence = ScheduleRecurrenceType.Daily;
					return hasValue;

				case "w":
				case "week":
				case "weeks":
					recurrence = ScheduleRecurrenceType.Weekly;
					return hasValue;

				case "n":
				case "month":
				case "months":
					recurrence = ScheduleRecurrenceType.Monthly;
					return hasValue;

				case "y":
				case "year":
				case "years":
					recurrence = ScheduleRecurrenceType.Yearly;
					return hasValue;

				default:
					recurrence = null;
					return false;
			}
		}

		#endregion

		#region ValidateSecondaryProcessesMaxCount

		public void ValidateSecondaryProcessesMaxCount()
		{
			ValidateCalculatedProperty(Parent.SecondaryProcessesMaxCountInfo);
		}

		protected void CheckSecondaryProcessesMaxCount()
		{
			Parent.ExtendedConfigProcessesMaxCountWarningMessage = string.Empty;
			Parent.ExtendedConfigProcessesMaxCountWarningLink = string.Empty;

			var result = GetTaskSpecificSecondaryProcessValidation()?.Validate();

			if (result == null)
			{
				return;
			}

			foreach (var error in result.Errors)
			{
				Parent.SecondaryProcessesMaxCountInfo.AddError(error);
			}

			foreach (var warning in result.PropertySpecificWarnings)
			{
				var propertyName = warning.Key;
				var value = warning.Value;
				var propertyInfo = Parent.GetType().GetProperty(propertyName);
				if (propertyInfo != null)
				{
					if (propertyInfo.PropertyType == typeof(ZString))
					{
						propertyInfo.SetValue(Parent, new ZString(value));
					}
					else
					{
						propertyInfo.PropertyType
							.GetMethod("AddWarning")
							?.Invoke(propertyInfo.GetValue(Parent), new object[] { value });
					}
				}
			}

			IServiceTaskSpecificValidation GetTaskSpecificSecondaryProcessValidation()
			{
				var typeName = Parent.StaticServiceAttributes?.TaskSpecificValidationTypeName;
				var typeAssemblyName = Parent.StaticServiceAttributes?.TaskSpecificValidationTypeAssemblyName;

				if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(typeAssemblyName))
				{
					return null;
				}

				return (IServiceTaskSpecificValidation)Activator.CreateInstance(
					Type.GetType($"{typeName}, {typeAssemblyName}", throwOnError: true),
					(int)Parent.SecondaryProcessesMaxCount);
			}
		}

		public void ValidateSecondaryProcessesMaxCountDescription()
		{
			ValidateCalculatedProperty(Parent.SecondaryProcessesMaxCountDescriptionInfo);
		}

		protected virtual void CheckSecondaryProcessesMaxCountDescription()
		{
			if (Parent.SecondaryProcessesMaxCountsList.GetCodeFromDescription(Parent.SecondaryProcessesMaxCountDescription) == null)
			{
				Parent.SecondaryProcessesMaxCountDescriptionInfo.AddError(Res.GetString("4f5773bb-b379-4f10-967d-fde3a4d7cf20", "Please enter a valid maximum count for secondary processes."));
			}
		}

		#endregion

		#region S5_ParentID

		protected override void CheckS5_ParentIDIsValidZGuid()
		{
		}

		protected void CheckS5_ParentIDIsNotEmpty()
		{
		}

		protected override void CheckS5_ParentID()
		{
			MandatoryValidation.CheckNotEntered(Parent.S5_ParentIDInfo);
		}

		#endregion

		void ValidateBranchInDatabase()
		{
			if (Parent.S5_IsActive)
			{
				var wasActive = (ZBool)Parent.S5_IsActiveInfo.OriginalValue;
				if (!wasActive && Parent.S5_GB.IsValid && !(BranchInDatabase?.GB_IsActive ?? true))
				{
					Parent.AddRowError(Res.GetString("51bc3973-33b5-44b6-a2d5-3ba63e9d9bc0", "You cannot activate a service task when the branch is inactive."));
				}
			}
		}

		GlbBranch BranchInDatabase
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(GlbBranch));
				query.AddToFilter(GlbBranchSchema.PK, Parent.S5_GB);
				query.ReLoadExistingRows = true;
				return Parent.Factory.LoadTop1<GlbBranch>(query);
			}
		}
	}
}


