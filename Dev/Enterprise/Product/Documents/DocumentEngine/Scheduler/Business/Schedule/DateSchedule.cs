using System;
using System.ComponentModel;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class DateSchedule : Schedule
	{
		public DateSchedule()
		{
		}

		public DateSchedule(ReportScheduleTask reportScheduleTask)
		{
			ScheduleTask = reportScheduleTask;
			if (reportScheduleTask != null && reportScheduleTask.Recurrence != null)
			{
				var taskPeriod = reportScheduleTask.Recurrence.TaskPeriod;
				if (taskPeriod == ScheduleRecurrenceType.AccountingPeriod)
				{
					taskPeriod = ScheduleRecurrenceType.Monthly;
				}

				Period = taskPeriod;
			}
		}

		protected override ZDateTime BaseDateTime
		{
			get
			{
				var baseDateTime = base.BaseDateTime;
				if (ByHourAndMinute && ScheduleTask != null && baseDateTime.IsValid)
				{
					var scheduleRunTime = ScheduleTask.Recurrence.RecurringStartTimeLocal;
					return new ZDateTime(baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, scheduleRunTime.Hour, scheduleRunTime.Minute, scheduleRunTime.Second);
				}
				return baseDateTime;
			}
		}

		public ZDateTime GetScheduleDate()
		{
			return GetScheduleDate(BaseDateTime);
		}

		public ZDateTime GetScheduleDate(ZDateTime baseDateTime)
		{
			var result = ZDateTime.Empty;

			if ((ScheduleTask != null) && IsValid)
			{
				if (baseDateTime.IsValid)
				{
					if (ByWeek)
					{
						result = Calculator.CalculateDayOfWeek(baseDateTime, PeriodsToAdd, DayNameAsDayNumber);
					}
					else if (ByMonth)
					{
						result = Calculator.CalculateDayOfMonth(baseDateTime, PeriodsToAdd, LastDay ? 0 : DayNumber);
					}
					else if (ByYear)
					{
						result = Calculator.CalculateDayOfYear(baseDateTime, PeriodsToAdd, LastDay ? 0 : DayNumber);
					}
					else if (ByDay)
					{
						if (PeriodScope == PeriodScopeList.Codes.This)
						{
							result = ZDateTime.Today;
						}
						else
						{
							result = ZDateTime.Today.AddDays(PeriodsToAdd);
						}
					}
					else if (ByHourAndMinute)
					{
						if (PeriodScope == PeriodScopeList.Codes.Next)
						{
							result = baseDateTime.AddHours(Hour).AddMinutes(MinuteOfHour);
						}
						else if (PeriodScope == PeriodScopeList.Codes.Previous)
						{
							result = baseDateTime.AddHours(-Hour).AddMinutes(-MinuteOfHour);
						}
					}
				}
			}

			return result;
		}

		public void CopyChangesFrom(DateSchedule sourceSchedule)
		{
			PeriodScope = sourceSchedule.PeriodScope;
			PeriodCount = sourceSchedule.PeriodCount;
			Period = sourceSchedule.Period;
			LastDay = sourceSchedule.LastDay;
			DayNumber = sourceSchedule.DayNumber;
			DayName = sourceSchedule.DayName;
			Hour = sourceSchedule.Hour;
			MinuteOfHour = sourceSchedule.MinuteOfHour;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			ByDay = true;
			dayNumber = 0;
			dayName = null;
			LastDay = false;
			Hour = 0;
			MinuteOfHour = 0;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ByDay = true;
			DayNumber = 1;
			DayName = WeekDayList.Codes.Sunday;
			PeriodScope = PeriodScopeList.Codes.This;
			Hour = 0;
			MinuteOfHour = 0;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ByWeek = true;
			DayName = WeekDayList.Codes.Sunday;
			PeriodScope = PeriodScopeList.Codes.This;
		}
#endif

		#region Storage Value

		public static bool TryParse(DateTime storageValue, out DateSchedule result)
		{
			bool canParse = false;
			result = null;

			if (ValidRange(storageValue))
			{
				string period;

				if (storageValue.Day == 1)
				{
					period = ScheduleRecurrenceType.Weekly;
				}
				else if (storageValue.Day == 2)
				{
					period = ScheduleRecurrenceType.Monthly;
				}
				else if (storageValue.Day == 3)
				{
					period = ScheduleRecurrenceType.Yearly;
				}
				else if (storageValue.Day == 4)
				{
					period = ScheduleRecurrenceType.Daily;
				}
				else if (storageValue.Day == 5)
				{
					period = ScheduleRecurrenceType.HourAndMinute;
				}
				else
				{
					period = null;
				}

				if (period != null)
				{
					result = new DateSchedule();
					result.Period = period;

					if (storageValue.Hour == LastDayStorageHour)
					{
						result.LastDay = true;
					}
					else
					{
						result.DayNumber = (ZShort)((storageValue.Hour * 60) + storageValue.Minute);
					}

					result.Hour = (ZShort)storageValue.Millisecond;
					result.MinuteOfHour = (ZShort)storageValue.Second;

					result.PopulatePeriodScopeAndNumber(storageValue);
					canParse = true;
				}
			}

			return canParse;
		}

		protected override DateTime ToStorageValueCore(DateTime periodAdjustedValue)
		{
			DateTime result;
			if (ByDay)
			{
				result = periodAdjustedValue.AddDays(3);
			}
			else
			{
				result = new DateTime(periodAdjustedValue.Year, periodAdjustedValue.Month, StorageValueDay);

				if (ByWeek)
				{
					result = result.AddMinutes(DayNameAsDayNumber);
				}
				else if (ByHourAndMinute)
				{
					result = result.AddSeconds(MinuteOfHour);
					result = result.AddMilliseconds(Hour);
				}
				else
				{
					if (LastDay)
					{
						result = result.AddHours(LastDayStorageHour);
					}
					else
					{
						result = result.AddMinutes(DayNumber);
					}
				}
			}

			return result;
		}

		int StorageValueDay
		{
			get
			{
				switch (Period)
				{
					case ScheduleRecurrenceType.Weekly:
						return 1;
					case ScheduleRecurrenceType.Monthly:
						return 2;
					case ScheduleRecurrenceType.Yearly:
						return 3;
					case ScheduleRecurrenceType.Daily:
						return 4;
					case ScheduleRecurrenceType.HourAndMinute:
						return 5;
					default:
						throw new InvalidOperationException("StorageValueDay cannot be obtained because the Period is invalid.");
				}
			}
		}

		const int LastDayStorageHour = 23;

		#endregion

		#region Period

		[MaxLength(3)]
		public ZString Period
		{
			get { return period; }
			set
			{
				if (value != period)
				{
					CheckMaximumLength(PeriodInfo, value);
					period = value;
					PeriodInfo.RefreshBinding();

					OnValueChanged();
				}
			}
		}
		ZString period;

		public ZPropertyInfo PeriodInfo
		{
			get { return GetZPropertyInfo(nameof(Period)); }
		}

		#endregion

		#region Description Overrides

		protected override ZString GetDescriptionCore()
		{
			var result = base.GetDescriptionCore();

			if (result.IsEmpty)
			{
				result = Res.GetString("0d02aa5c-5002-4ee9-86fd-035ef1484b5f", "(no date selected)");
			}
			else if (ByHourAndMinute && MinuteOfHour == 0 && Hour == 0)
			{
				var builder = new StringBuilder();
				builder.Append(Res.GetString("489ec805-4db3-4a69-b194-4c489f81d4a9", "The Scheduled Run Time of the report, on the day of its Next Run Time."));
				AppendDescriptionEnd(builder);
				return builder.ToString();
			}

			return result;
		}

		protected override void AppendDescriptionStart(StringBuilder builder)
		{
			base.AppendDescriptionStart(builder);
			if (ByDay)
			{
				var dayString = PeriodCount > 1 ?
					Res.GetString("e8972296-3820-4e8f-928a-6fbf573c3ebe", "days") :
					Res.GetString("f3db4c4b-9f54-4dad-87ee-1d9cf4c4b6c6", "day");

				if (PeriodScope == PeriodScopeList.Codes.Next)
				{
					builder.Append(Res.GetString("d49b2558-3a36-42d6-98f6-c17894d9c58f", "{0} {1} after", PeriodCount, dayString));
				}
				else if (PeriodScope == PeriodScopeList.Codes.Previous)
				{
					builder.Append(Res.GetString("03467071-cb1c-4179-a63f-dda44e05bcfb", "{0} {1} prior to", PeriodCount, dayString));
				}
				else
				{
					builder.Append(Res.GetString("0ec4219a-0a93-446d-871e-393a2b1426bc", "The day"));
				}
			}
			else if (ByHourAndMinute)
			{
				var hourString = string.Empty;

				if (Hour > 1)
				{
					hourString = Hour.ToString() + " " + Res.GetString("7110ac64-1186-4a36-aceb-7733878ab339", "hours");
				}
				else if (Hour == 1)
				{
					hourString = Hour.ToString() + " " + Res.GetString("74ddccbe-3b1d-48ce-8b13-67b460291d08", "hour");
				}

				builder.Append(hourString);
				var minuteString = string.Empty;

				if (MinuteOfHour > 1)
				{
					minuteString = MinuteOfHour.ToString() + " " + Res.GetString("c483ecc9-4d6f-4365-bd85-5b8b555faf95", "minutes");
				}
				else if (MinuteOfHour == 1)
				{
					minuteString = MinuteOfHour.ToString() + " " + Res.GetString("e2ba1dfb-4aac-4a0c-863a-c84afda7b9a5", "minute");
				}

				var andString = (MinuteOfHour > 0 && Hour > 0) ? " " + Res.GetString("23a9507c-f270-496e-b7b1-9bf9ff14534b", "and") + " " : "";
				builder.Append(andString);
				builder.Append(minuteString);
			}
			else if (ByWeek)
			{
				builder.Append(Res.GetString("0a3abdef-20b6-4a46-9e7f-cdcb4f9514d3", "The {0} of", Calculator.ConvertToDayOfWeek(DayNameAsDayNumber).ToString()));
			}
			else
			{
				builder.Append(Res.GetString("0b7a04c9-3341-41a1-875f-1873c2fb808c", "The") + " ");
				if (LastDay)
				{
					builder.Append(Res.GetString("8cff3eb9-8baa-41b6-aacf-f74315fc20bf", "last day"));
				}
				else
				{
					builder.Append(ZArchitecture.Core.NumberFormatter.ToOrdinalString(DayNumber) + " " + Res.GetString("f3db4c4b-9f54-4dad-87ee-1d9cf4c4b6c6", "day"));
				}

				builder.Append(" " + Res.GetString("bf8c7d68-91d7-40ab-bee7-00c44e053def", "of"));
			}
		}

		protected override void AppendDescriptionMiddle(StringBuilder builder)
		{
			if (!ByHourAndMinute)
			{
				base.AppendDescriptionMiddle(builder);
			}
			else
			{
				builder.Append(" " + Res.GetString("d18b7514-b0fc-4817-ac6d-a713fcbe76e5", "the report's Scheduled Run Time, on the day of its Next Run Time."));
			}
		}

		protected override void AppendDescriptionEnd(StringBuilder builder)
		{
			base.AppendDescriptionEnd(builder);
			if (!ByHourAndMinute)
			{
				var today = ZDateTime.Today;
				var scheduleDate = GetScheduleDate(today);
				if (!scheduleDate.IsEmpty)
				{
					builder.Append(" ");
					builder.Append(
						Res.GetString("f32e444a-4b1b-40e3-a8cb-77eec23ac882", "If the Report ran today ({0}), the date used would be {1}.",
						today.ToShortDateString(),
						scheduleDate.ToShortDateString()));
				}
			}
			else
			{
				var scheduleDate = GetScheduleDate(BaseDateTime);
				if (!scheduleDate.IsEmpty)
				{
					builder.Append(" ");
					builder.Append(
						Res.GetString("93a1bb12-c89b-4ebe-89b3-2f178f9d9619", "If the Report ran now ({0}), the date/time used would be {1}.",
						ZDateTime.Now.ToLongTimeString(),
						scheduleDate.ToLongTimeString()));
				}
				else
				{
					builder.Clear();
					builder.Append(Res.GetString("2fd26ef0-51a2-47ad-9228-d75c356778d8", "Failed to calculate due to no date being specified for \"Next Run Time\", \"Start Date\" and \"Scheduled Run Time\"."));
				}
			}
		}

		internal override string PeriodDescription
		{
			get
			{
				if (ByWeek)
				{
					return Res.GetString("bedb97d3-4d57-4ee8-9f2a-fec0d9f2303b", "week");
				}
				else if (ByMonth)
				{
					return Res.GetString("e7c618fa-cb67-4e9f-9974-5d642222899f", "month");
				}
				else if (ByYear)
				{
					return Res.GetString("c1cf359d-6585-46c9-9e68-b7d38d7a01d5", "year");
				}
				else if (ByDay)
				{
					return Res.GetString("9d79d1dc-5156-4994-ab4c-f7c1f790a6a2", "today");
				}
				else if (ByHourAndMinute)
				{
					return Res.GetString("91e27f20-fca5-4327-9b1a-6412864519bc", "hour and minute");
				}
				else
				{
					return "";
				}
			}
		}

		#endregion

		#region Bound Properties

		#region Day Number
		ZShort dayNumber;

		[ReadOnlyMember(nameof(LastDay))]
		public ZShort DayNumber
		{
			get { return dayNumber; }
			set
			{
				if (!LastDay)
				{
					if (value < 1)
					{
						value = 1;
					}
					else
					{
						if (ByMonth)
						{
							if (value > 31)
							{
								value = 31;
							}
						}
						else if (ByYear)
						{
							if (value > 366)
							{
								value = 366;
							}
						}
					}
				}

				SetNonPersistentPropertyValue<ZShort>(DayNumberInfo, ref dayNumber, value);

				if (ByWeek)
				{
					DayNameAsDayNumber = value;
				}

				OnValueChanged();
			}
		}

		public ZPropertyInfo DayNumberInfo
		{
			get { return GetZPropertyInfo(nameof(DayNumber)); }
		}

		#endregion

		#region Day Name

		[MaxLength(3)]
		public ZString DayName
		{
			get { return dayName; }
			set
			{
				CheckMaximumLength(DayNameInfo, value);
				dayName = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDayName();
				}

				DayNameInfo.RefreshBinding();
				OnValueChanged();
			}
		}

		ZShort DayNameAsDayNumber
		{
			get
			{
				ZShort result = 0;

				if (!DayNameInfo.HasErrors())
				{
					ZShort.TryParse(Lookups.WeekDays.GetDescriptionFromCode(DayName), out result);
				}

				return result;
			}
			set
			{
				var dayName = Lookups.WeekDays.GetCodeFromDescription(value.ToString());
				if (!string.IsNullOrEmpty(dayName))
				{
					this.DayName = dayName;
				}
			}
		}

		public ZPropertyInfo DayNameInfo
		{
			get { return GetZPropertyInfo(nameof(DayName)); }
		}

		ZString dayName;

		#endregion

		#region Last Day

		public ZBool LastDay
		{
			get { return lastDay; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(LastDayInfo, ref lastDay, value);
				LastDayInfo.RefreshBinding();
				OnValueChanged();
			}
		}

		public ZPropertyInfo LastDayInfo
		{
			get { return GetZPropertyInfo(nameof(LastDay)); }
		}

		ZBool lastDay;

		#endregion

		#region Periods

		public ZBool ByDay
		{
			get { return Period == ScheduleRecurrenceType.Daily; }
			set
			{
				if (value)
				{
					Period = ScheduleRecurrenceType.Daily;
				}
			}
		}

		public ZBool ByWeek
		{
			get { return Period == ScheduleRecurrenceType.Weekly; }
			set
			{
				if (value)
				{
					Period = ScheduleRecurrenceType.Weekly;
				}
			}
		}

		public ZBool ByMonth
		{
			get { return Period == ScheduleRecurrenceType.Monthly; }
			set
			{
				if (value)
				{
					Period = ScheduleRecurrenceType.Monthly;
				}
			}
		}

		public ZBool ByYear
		{
			get { return Period == ScheduleRecurrenceType.Yearly; }
			set
			{
				if (value)
				{
					Period = ScheduleRecurrenceType.Yearly;
				}
			}
		}

		public ZBool ByHourAndMinute
		{
			get { return Period == ScheduleRecurrenceType.HourAndMinute; }
			set
			{
				if (value)
				{
					Period = ScheduleRecurrenceType.HourAndMinute;
					PeriodCount = 1;
				}
			}
		}

		#endregion

		#endregion

		#region Hours/Minutes

		ZShort hour;

		public ZShort Hour
		{
			get { return hour; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				SetNonPersistentPropertyValue(HourInfo, ref hour, value);
				HourInfo.RefreshBinding();
				OnValueChanged();
			}
		}

		public ZPropertyInfo HourInfo
		{
			get { return GetZPropertyInfo(nameof(Hour)); }
		}

		ZShort minuteOfHour;

		public ZShort MinuteOfHour
		{
			get { return minuteOfHour; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= 60)
				{
					value = 59;
				}

				SetNonPersistentPropertyValue(MinuteOfHourInfo, ref minuteOfHour, value);
				MinuteOfHourInfo.RefreshBinding();
				OnValueChanged();
			}
		}

		public ZPropertyInfo MinuteOfHourInfo
		{
			get { return GetZPropertyInfo(nameof(MinuteOfHour)); }
		}

		#endregion

		#region Lookups

		public new DateScheduleLookups Lookups
		{
			get { return (DateScheduleLookups)base.Lookups; }
		}

		protected override ScheduleLookups GetNewLookups()
		{
			return new DateScheduleLookups(this);
		}

		#endregion

		#region Validation

		public new DateScheduleValidation Validation
		{
			get { return (DateScheduleValidation)base.Validation; }
		}

		protected override ScheduleValidation GetNewValidation()
		{
			return new DateScheduleValidation(this);
		}

		#endregion

		#region FillData

		internal void FillData(DateScheduleData data)
		{
			Period = data.RecurrenceType;
			PeriodCount = data.PeriodCount;
			PeriodScope = data.PeriodScope;
			LastDay = data.IsLastDay;
			DayNumber = data.DayNumber;
			DayNameAsDayNumber = (ZShort)(data.DayNameAsDayNumber + 1);//Lookups.WeekDays is start from SUN - 1 while we want to start from SUN - 0 in the front-end
			Hour = data.Hour;
			MinuteOfHour = data.MinuteOfHour;
		}

		internal DateScheduleData ExtractData()
		{
			if (!IsValid)
			{
				return null;
			}
			var data = new DateScheduleData();
			data.RecurrenceType = Period;
			data.PeriodScope = PeriodScope;
			data.DayNameAsDayNumber = DayNameAsDayNumber - 1;//Lookups.WeekDays is start from SUN - 1 while we want to start from SUN - 0 in the front-end
			data.DayNumber = DayNumber;
			data.PeriodCount = PeriodCount;
			data.IsLastDay = LastDay;
			data.Hour = Hour;
			data.MinuteOfHour = MinuteOfHour;

			data.CalculatedResult = Description;
			data.StorageValue = ToStorageValue();
			data.ScheduleDate = GetScheduleDate().ToDateTime();
			return data;
		}

		#endregion
	}
}
