using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class MinimumIntervalSubLedgerTakeUp : RegistryBusinessObjectTemplate
	{
		#region Construction

		public MinimumIntervalSubLedgerTakeUp()
		{
		}

		public MinimumIntervalSubLedgerTakeUp(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public MinimumIntervalSubLedgerTakeUp(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string Interval = "Interval";
			public const string IntervalType = "IntervalType";
		}

		#endregion

		#region Validation

		public void ValidateIntervalType()
		{
			IntervalTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(IntervalTypeInfo);
			ListValidation.ErrorIfInvalidCode(IntervalTypeInfo, IntervalTypeList);
		}

		public void ValidateInterval()
		{
			IntervalInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(IntervalInfo);
			MandatoryValidation.CheckNotNegative(IntervalInfo);
		}

		#endregion

		#region IntervalTypes

		public static class IntervalTypes
		{
			public const string Months = "MONTHS";
			public const string Days = "DAYS";
			public const string Hours = "HOURS";
			public const string Minutes = "MINUTES";
		}

		#endregion

		#region IntervalTypeList

		public CodeDescriptionPairList IntervalTypeList
		{
			get
			{
				if (intervalTypeList == null)
				{
					intervalTypeList = new CodeDescriptionPairList();
					intervalTypeList.AddPair(IntervalTypes.Minutes, ResString.GetMultilingualString("a8449aa5-b005-417b-8739-0872f84df0de", "MINUTES"));
					intervalTypeList.AddPair(IntervalTypes.Hours, ResString.GetMultilingualString("550994ee-6b4f-4724-91b4-814c1ff89268", "HOURS"));
					intervalTypeList.AddPair(IntervalTypes.Days, ResString.GetMultilingualString("824b002d-f3bd-48b9-a125-0d2c23b3886d", "DAYS"));
					intervalTypeList.AddPair(IntervalTypes.Months, ResString.GetMultilingualString("d63c8961-a5e2-4328-a37e-d16aa5d35c2a", "MONTHS"));
				}

				return intervalTypeList;
			}
		}
		CodeDescriptionPairList intervalTypeList;

		#endregion

		#region IntervalType

		[List("IntervalTypeList")]
		public ZString IntervalType
		{
			get { return intervalType; }
			set
			{
				SetNonPersistentPropertyValue(IntervalTypeInfo, ref intervalType, value);
				if (!IsValidationSuspended)
				{
					ValidateIntervalType();
				}
			}
		}
		ZString intervalType = IntervalTypes.Minutes;

		public ZPropertyInfo IntervalTypeInfo
		{
			get { return GetZPropertyInfo(Schema.IntervalType); }
		}

		#endregion

		#region Interval

		public ZInt Interval
		{
			get { return interval; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(IntervalInfo, ref interval, value);
				if (!IsValidationSuspended)
				{
					ValidateInterval();
				}
			}
		}
		ZInt interval = 15;

		public ZPropertyInfo IntervalInfo
		{
			get { return GetZPropertyInfo(Schema.Interval); }
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MinimumIntervalSubLedgerTakeUp(fallbackLevel);
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateIntervalType();
			ValidateInterval();
		}

		#region XML Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IntervalType = reader.ReadElementString(Schema.IntervalType);
			Interval = reader.ReadElementStringAsZInt(Schema.Interval);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IntervalType, IntervalType);
			writer.WriteElementString(Schema.Interval, Interval.ToString());
		}

		#endregion

		#endregion
	}
}
