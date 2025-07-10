using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eHub
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ScavengingSetting : RegistryBusinessObjectTemplate
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string GlobalizedUtcDateTimeFormat = "u";
		internal const int PopulateQueueIntervalMonths = 3;

		protected abstract class Schema
		{
			public const string TaskName = "TaskName";
			public const string PeriodStart = "PeriodStart";
			public const string PeriodEnd = "PeriodEnd";
			public const string DateTimeFormat = "DateTimeFomat";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ScavengingSetting(fallbackLevel, factory, null);
		}

		public ScavengingSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory, ScavengingSettingCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			SetParentCollection(parentCollection);
		}

		public ScavengingSetting()
			: base()
		{
		}

		[BusinessObjectTestExclude]
		public ScavengingSettingCollection ParentCollection
		{
			get { return parentCollection; }
		}
		ScavengingSettingCollection parentCollection;

		public void SetParentCollection(ScavengingSettingCollection parentCollection)
		{
			this.parentCollection = parentCollection;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTaskName();
			ValidatePeriodStart();
			ValidatePeriodEnd();
		}

		#region TaskName

		[CargoWise.ComponentModel.MaxLength(160)]
		public ZString TaskName
		{
			get { return taskName; }
			set
			{
				CheckMaximumLength(TaskNameInfo, value);
				SetNonPersistentPropertyValue<ZString>(TaskNameInfo, ref taskName, value);

				if (!IsValidationSuspended)
				{
					ValidateTaskName();
				}
			}
		}
		ZString taskName;

		public ZPropertyInfo TaskNameInfo
		{
			get { return GetZPropertyInfo(Schema.TaskName); }
		}

		public void ValidateTaskName()
		{
			TaskNameInfo.ClearAllNotifications();

			if (TaskName.IsEmpty)
			{
				TaskNameInfo.AddError(ErrorMustHaveTaskName);
				return;
			}

			if (ParentCollection != null && ParentCollection.HasMultipleTaskWithSameName(this.TaskName))
			{
				TaskNameInfo.AddError(ErrorTaskNameAlreadyExists);
			}
		}

		public static string ErrorMustHaveTaskName
		{
			get { return Res.GetString("7CFACAFE-5BF8-40B8-B851-36637B04C97E", "Should have Task Name."); }
		}

		public static string ErrorTaskNameAlreadyExists
		{
			get { return Res.GetString("20E86EBD-98DF-4D73-A17F-43D42D67D9D2", "Task Name already exists."); }
		}

		#endregion

		#region PeriodStart

		public ZDateTime PeriodStart
		{
			get { return periodStart; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(PeriodStartInfo, ref periodStart, value);
				if (!IsValidationSuspended)
				{
					ValidatePeriodStart();
				}
			}
		}
		ZDateTime periodStart;

		public ZPropertyInfo PeriodStartInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodStart); }
		}

		public void ValidatePeriodStart()
		{
			PeriodStartInfo.ClearAllNotifications();
		}

		#endregion

		#region PeriodEnd

		public ZDateTime PeriodEnd
		{
			get { return periodEnd; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(PeriodEndInfo, ref periodEnd, value);
				if (!IsValidationSuspended)
				{
					ValidatePeriodEnd();
				}
			}
		}
		ZDateTime periodEnd;

		public ZPropertyInfo PeriodEndInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodEnd); }
		}

		public void ValidatePeriodEnd()
		{
			PeriodEndInfo.ClearAllNotifications();
		}

		#endregion

		#region XML Reading and Writing

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TaskName, TaskName);
			writer.WriteElementString(Schema.PeriodStart, ToUtcDateTimeString(PeriodStart));
			writer.WriteElementString(Schema.PeriodEnd, ToUtcDateTimeString(PeriodEnd));
			writer.WriteElementString(Schema.DateTimeFormat, GlobalizedUtcDateTimeFormat);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			TaskName = reader.ReadElementString(Schema.TaskName);

			var periodStartString = reader.ReadElementString(Schema.PeriodStart);
			var periodEndString = reader.ReadElementString(Schema.PeriodEnd);
			var dateTimeFormatString = reader.ReadElementString(Schema.DateTimeFormat);

			PeriodStart = ParseDateTimeString(periodStartString, dateTimeFormatString);
			PeriodEnd = ParseDateTimeString(periodEndString, dateTimeFormatString);

			if (string.IsNullOrEmpty(dateTimeFormatString) &&
				(!string.IsNullOrEmpty(periodStartString) && PeriodStart.IsEmpty || !string.IsNullOrEmpty(periodEndString) && PeriodEnd.IsEmpty))
			{
				PeriodEnd = ZDateTime.UtcNow.AddMonths(-PopulateQueueIntervalMonths).AddSeconds(-1);
			}
		}

#if DEBUG
		internal virtual
#endif
		string ToUtcDateTimeString(ZDateTime zDateTime)
		{
			var utcDateTimeString = string.Empty;
			if (zDateTime.IsValid)
			{
				utcDateTimeString = zDateTime.ToString(GlobalizedUtcDateTimeFormat, CultureInfo.InvariantCulture);
			}

			return utcDateTimeString;
		}

#if DEBUG
		internal
#endif
		ZDateTime ParseDateTimeString(string dateTimeString, string dateTimeFormat)
		{
			var resultZDateTime = ZDateTime.Empty;
			if (!string.IsNullOrEmpty(dateTimeString))
			{
				if (!string.IsNullOrEmpty(dateTimeFormat) && TryParseUtcDateTimeString(dateTimeString, dateTimeFormat, out var zDateTime))
				{
					resultZDateTime = zDateTime;
				}
				else if (TryParseDateTimeStringWithinQueueIntervalMonths(dateTimeString, out zDateTime))
				{
					resultZDateTime = zDateTime;
				}
			}

			return resultZDateTime;
		}

#if DEBUG
		internal
#endif
		bool TryParseUtcDateTimeString(string dateTimeString, string dateTimeFormat, out ZDateTime zDateTime)
		{
			zDateTime = ZDateTime.Empty;
			var result = false;

			if (!string.IsNullOrEmpty(dateTimeString) &&
				DateTime.TryParseExact(
					dateTimeString,
					dateTimeFormat,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AdjustToUniversal,
					out var dateTime))
			{
				zDateTime = new ZDateTime(dateTime, DateTimeKind.Utc);
				result = true;
			}

			return result;
		}

#if DEBUG
		internal
#endif
		bool TryParseDateTimeStringWithinQueueIntervalMonths(string dateTimeString, out ZDateTime zDateTime)
		{
			zDateTime = ZDateTime.Empty;
			var result = false;
			var utcNow = ZDateTime.UtcNow;

			if (!string.IsNullOrEmpty(dateTimeString)
				&& DateTime.TryParse(dateTimeString, out var dateTime)
				&& dateTime <= utcNow
				&& dateTime > utcNow.AddMonths(-PopulateQueueIntervalMonths))
			{
				zDateTime = new ZDateTime(dateTime, DateTimeKind.Utc);
				result = true;
			}

			return result;
		}

		#endregion
	}
}
