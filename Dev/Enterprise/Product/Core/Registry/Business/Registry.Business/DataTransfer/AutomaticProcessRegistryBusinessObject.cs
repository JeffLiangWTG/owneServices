using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AutomaticProcessRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public AutomaticProcessRegistryBusinessObject()
		{
			InitializeValidationDelegates();
		}

		public AutomaticProcessRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			InitializeValidationDelegates();
		}

		void InitializeValidationDelegates()
		{
			validateIntervalTypeAction = new Action(() =>
			{
				IntervalTypeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(IntervalTypeInfo);
				ListValidation.ErrorIfInvalidCode(IntervalTypeInfo, IntervalTypeList);
				CheckIntervalNotLessThanMinimumValue(IntervalTypeInfo);
			});
			validateIntervalAction = new Action(() =>
			{
				IntervalInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(IntervalInfo);
				MandatoryValidation.CheckNotNegative(IntervalInfo);
				CheckIntervalNotLessThanMinimumValue(IntervalInfo);
			});
			validateNextRunDateAction = new Action(() =>
			{
				NextRunDateTimeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(NextRunDateTimeInfo);
				if (!NextRunDateTimeInfo.Value.IsEmpty && !NextRunDateTimeInfo.Value.IsValid)
				{
					NextRunDateTimeInfo.AddError(Res.GetString("ee6f74f1-4408-4e91-83ab-0985ae7ef6b4", "Please enter a valid Next Run"));
				}
			});
		}

		void CheckIntervalNotLessThanMinimumValue(ZPropertyInfo propertyInfo)
		{
			if (ShouldCheckIntervalNotLessThanMinimumValue)
			{
				var minimumDuration = GetIntervalDuration(MinimumIntervalType, MinimumInterval);
				if (minimumDuration == TimeSpan.MaxValue)
				{
					var minimumValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), CurrentFallbackLevel.BranchPK, CurrentFallbackLevel.DepartmentPK);
					minimumDuration = GetIntervalDuration(minimumValue.IntervalType, minimumValue.Interval);
				}
				var intervalDuration = GetIntervalDuration(IntervalType, Interval);

				if (minimumDuration > intervalDuration)
				{
					propertyInfo.AddError(Res.GetString("03a9f826-f3eb-4e43-8173-606603064fef", "The specified interval cannot be less than {0}.", minimumDuration));
				}
			}
		}

		public bool ShouldCheckIntervalNotLessThanMinimumValue { get; set; }
		public string MinimumIntervalType { get; set; }
		public int MinimumInterval { get; set; }

		TimeSpan GetIntervalDuration(ZString intervalType, ZInt interval)
		{
			switch (intervalType)
			{
				case IntervalTypes.Months:
					return TimeSpan.FromDays(interval * 28);
				case IntervalTypes.Days:
					return TimeSpan.FromDays(interval);
				case IntervalTypes.Hours:
					return TimeSpan.FromHours(interval);
				case IntervalTypes.Minutes:
					return TimeSpan.FromMinutes(interval);
				default:
					return TimeSpan.MaxValue;
			}
		}

		#region Schema & Constants

		public abstract class Schema
		{
			public const string NextRunDateTime = "NextRunDateTime";
			public const string LastRunDateTimeAsString = "LastRunDateTimeAsString";
			public const string LastRunDateTime = "LastRunDateTime";
			public const string Interval = "Interval";
			public const string IntervalType = "IntervalType";
			public const string IntervalTypeList = "IntervalTypeList";
		}

		public static class IntervalTypes
		{
			public const string Months = "MONTHS";
			public const string Days = "DAYS";
			public const string Hours = "HOURS";
			public const string Minutes = "MINUTES";
		}

		#endregion

		#region UpdateRuns

		public void UpdateRuns(ZDateTime lastRun)
		{
			if (!lastRun.IsEmpty && lastRun.IsValid)
			{
				lastRunDateTime = lastRun;
				UpdateNextRun();
			}
		}

		void UpdateNextRun()
		{
			switch (IntervalType)
			{
				case IntervalTypes.Months:
					while (nextRunDateTime <= lastRunDateTime)
					{
						nextRunDateTime = nextRunDateTime.AddMonths(Interval);
					}

					break;
				case IntervalTypes.Days:
					while (nextRunDateTime <= lastRunDateTime)
					{
						nextRunDateTime = nextRunDateTime.AddDays(Interval);
					}

					break;
				case IntervalTypes.Hours:
					while (nextRunDateTime <= lastRunDateTime)
					{
						nextRunDateTime = nextRunDateTime.AddHours(Interval);
					}

					break;
				case IntervalTypes.Minutes:
					while (nextRunDateTime <= lastRunDateTime)
					{
						nextRunDateTime = nextRunDateTime.AddMinutes(Interval);
					}

					break;
			}
		}

		#endregion

		#region Interval

		public ZInt Interval
		{
			get { return interval; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(IntervalInfo, ref interval, value);
				Validate(ValidateInterval);
			}
		}

		ZInt interval = 1;

		public ZPropertyInfo IntervalInfo
		{
			get { return GetZPropertyInfo(Schema.Interval); }
		}

		public void ValidateInterval()
		{
			if (ValidateIntervalAction != null)
			{
				ValidateIntervalAction();
			}
		}

		public Action ValidateIntervalAction
		{
			get { return validateIntervalAction; }
			set { validateIntervalAction = value; }
		}
		Action validateIntervalAction;

		#endregion

		#region IntervalType

		[MaxLength(10)]
		public ZString IntervalType
		{
			get { return intervalType; }
			set
			{
				CheckMaximumLength(IntervalTypeInfo, value);
				SetNonPersistentPropertyValue<ZString>(IntervalTypeInfo, ref intervalType, value);
				Validate(ValidateIntervalType);
			}
		}

		ZString intervalType = IntervalTypes.Days;

		public ZPropertyInfo IntervalTypeInfo
		{
			get { return GetZPropertyInfo(Schema.IntervalType); }
		}

		public void ValidateIntervalType()
		{
			if (ValidateIntervalTypeAction != null)
			{
				ValidateIntervalTypeAction();
			}
		}

		public Action ValidateIntervalTypeAction
		{
			get { return validateIntervalTypeAction; }
			set { validateIntervalTypeAction = value; }
		}
		Action validateIntervalTypeAction;

		public CodeDescriptionPairList IntervalTypeList
		{
			get
			{
				if (intervalTypeList == null)
				{
					intervalTypeList = new CodeDescriptionPairList();
					intervalTypeList.AddPair(IntervalTypes.Months, Res.GetString("e0021045-a8c4-4a17-8c83-4cd4866267a2", "MONTHS"));
					intervalTypeList.AddPair(IntervalTypes.Days, Res.GetString("6a05d99d-4aee-4b8a-a506-90ab3fcfc972", "DAYS"));
					intervalTypeList.AddPair(IntervalTypes.Hours, Res.GetString("fa5cffde-831e-4680-b59f-6bd2ed5ebfd4", "HOURS"));
					intervalTypeList.AddPair(IntervalTypes.Minutes, Res.GetString("bc8d1697-12ed-4bf7-b91a-4d4624d48a59", "MINUTES"));
				}
				return intervalTypeList;
			}
		}
		CodeDescriptionPairList intervalTypeList;

		#endregion

		#region NextRunDateTime

		public ZDateTime NextRunDateTime
		{
			get { return nextRunDateTime; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(NextRunDateTimeInfo, ref nextRunDateTime, value);
				Validate(ValidateNextRunDateTime);
			}
		}

		ZDateTime nextRunDateTime;

		public ZPropertyInfo NextRunDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.NextRunDateTime); }
		}

		public void ValidateNextRunDateTime()
		{
			if (ValidateNextRunDateAction != null)
			{
				ValidateNextRunDateAction();
			}
		}

		public Action ValidateNextRunDateAction
		{
			get { return validateNextRunDateAction; }
			set { validateNextRunDateAction = value; }
		}
		Action validateNextRunDateAction;

		#endregion

		#region LastRunDateTime

		public virtual ZDateTime LastRunDateTime
		{
			get { return lastRunDateTime; }
			set { SetNonPersistentPropertyValue<ZDateTime>(LastRunDateTimeInfo, ref lastRunDateTime, value); }
		}

		protected ZDateTime lastRunDateTime;

		public ZPropertyInfo LastRunDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.LastRunDateTime); }
		}
		#endregion

		#region LastRunDateTimeAsString

		public ZString LastRunDateTimeAsString
		{
			get
			{
				var result = ZString.Empty;
				if (!LastRunDateTime.IsEmpty)
				{
					result = LastRunDateTime.ToLongTimeString();
				}
				return result;
			}
		}
		public ZPropertyInfo LastRunDateTimeAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.LastRunDateTimeAsString); }
		}

		#endregion

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var valuesToClone = clone as AutomaticProcessRegistryBusinessObject;
			valuesToClone.ShouldCheckIntervalNotLessThanMinimumValue = ShouldCheckIntervalNotLessThanMinimumValue;
			valuesToClone.MinimumInterval = MinimumInterval;
			valuesToClone.MinimumIntervalType = MinimumIntervalType;
		}

		protected void Validate(Action validateAction)
		{
			if (!IsValidationSuspended && validateAction != null)
			{
				validateAction();
			}
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			ZString nextRun = NextRunDateTime.IsEmpty ? "" : (string)NextRunDateTime.SqlFormat;
			ZString lastRun = LastRunDateTime.IsEmpty ? "" : (string)LastRunDateTime.SqlFormat;
			writer.WriteElementString(Schema.NextRunDateTime, nextRun);
			writer.WriteElementString(Schema.LastRunDateTime, lastRun);
			writer.WriteElementString(Schema.Interval, Interval.ToString());
			writer.WriteElementString(Schema.IntervalType, IntervalType.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZString nextRun = reader.ReadElementString(Schema.NextRunDateTime);
			ZString lastRun = reader.ReadElementString(Schema.LastRunDateTime);
			nextRunDateTime = nextRun.IsEmpty ? ZDateTime.Empty : ZDateTime.FromSqlFormat(nextRun);
			lastRunDateTime = lastRun.IsEmpty ? ZDateTime.Empty : ZDateTime.FromSqlFormat(lastRun);
			interval = ZInt.Parse(reader.ReadElementString(Schema.Interval));
			intervalType = new ZString(reader.ReadElementString(Schema.IntervalType));
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutomaticProcessRegistryBusinessObject();
		}
	}
}
