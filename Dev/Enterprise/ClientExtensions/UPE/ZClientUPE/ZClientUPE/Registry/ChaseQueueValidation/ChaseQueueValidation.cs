using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class ChaseQueueValidation : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string DayOfTheWeek = "DayOfTheWeek";
			public const string TimeFrom = "TimeFrom";
			public const string TimeTo = "TimeTo";
		}

		#endregion

		#region Bound Properties

		#region TimeFrom

		public ZDateTime TimeFrom
		{
			get { return timeFrom; }
			set
			{
				if (timeFrom != value)
				{
					SetNonPersistentPropertyValue(TimeFromInfo, ref timeFrom, value);
				}

				if (!IsValidationSuspended)
				{
					ValidateTimeFrom();
				}
			}
		}
		ZDateTime timeFrom;

		public ZPropertyInfo TimeFromInfo
		{
			get { return GetZPropertyInfo(Schema.TimeFrom); }
		}

		void ValidateTimeFrom()
		{
			TimeFromInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TimeFromInfo);
			if (!TimeFrom.IsValid)
			{
				TimeFromInfo.AddMessageError("Please specify valid value.");
			}
		}

		#endregion

		#region TimeTo

		public ZDateTime TimeTo
		{
			get { return timeTo; }
			set
			{
				if (timeTo != value)
				{
					SetNonPersistentPropertyValue(TimeToInfo, ref timeTo, value);
				}
			}
		}
		ZDateTime timeTo;

		public ZPropertyInfo TimeToInfo
		{
			get { return GetZPropertyInfo(Schema.TimeTo); }
		}

		#endregion

		#region DayOfTheWeek

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString DayOfTheWeek
		{
			get { return dayOfTheWeek; }
			set
			{
				if (dayOfTheWeek != value)
				{
					SetNonPersistentPropertyValue(DayOfTheWeekInfo, ref dayOfTheWeek, value);
				}

				if (!IsValidationSuspended)
				{
					ValidateDayOfTheWeek();
				}

				DayOfTheWeekInfo.RefreshBinding();
			}
		}
		ZString dayOfTheWeek;

		public ZPropertyInfo DayOfTheWeekInfo
		{
			get { return GetZPropertyInfo(Schema.DayOfTheWeek); }
		}

		void ValidateDayOfTheWeek()
		{
			DayOfTheWeekInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(DayOfTheWeekInfo);
			ListValidation.ErrorIfInvalidCode(DayOfTheWeekInfo, DayOfTheWeekCollection);
		}

		#endregion

		#region DayOfTheWeekCollection

		public CodeDescriptionPairList DayOfTheWeekCollection
		{
			get
			{
				if (dayOfTheWeekCollection == null)
				{
					dayOfTheWeekCollection = new CodeDescriptionPairList();
					string[] days = Enum.GetNames(typeof(DayOfWeek));
					foreach (string day in days)
					{
						dayOfTheWeekCollection.AddPair(day, day);
					}
				}
				return dayOfTheWeekCollection;
			}
		}
		CodeDescriptionPairList dayOfTheWeekCollection;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.DayOfTheWeek, DayOfTheWeek);
			writer.WriteElementString(Schema.TimeFrom, TimeFrom.ToLongTimeString());
			writer.WriteElementString(Schema.TimeTo, TimeTo.ToLongTimeString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DayOfTheWeek = reader.ReadElementString(Schema.DayOfTheWeek);
			ZDateTime.TryParseExact(reader.ReadElementString(Schema.TimeFrom), out timeFrom, ZDateTime.LongTimeFormat);
			ZDateTime.TryParseExact(reader.ReadElementString(Schema.TimeTo), out timeTo, ZDateTime.LongTimeFormat);
			RefreshBinding();
		}

		#endregion

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDayOfTheWeek();
			ValidateTimeFrom();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ChaseQueueValidation result = new ChaseQueueValidation();
			if (dayOfTheWeekCollection != null)
			{
				result.dayOfTheWeekCollection = dayOfTheWeekCollection;
			}
			return result;
		}

		#endregion
	}
}
