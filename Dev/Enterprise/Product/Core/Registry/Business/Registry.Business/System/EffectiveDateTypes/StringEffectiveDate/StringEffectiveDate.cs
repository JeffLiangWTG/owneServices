using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StringEffectiveDate : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string PreviousValue = "PreviousValue";
			public const int PreviousValueMaxLength = 80;
			public const string NewValue = "NewValue";
			public const int NewValueMaxLength = 80;
			public const string EffectiveDate = "EffectiveDate";
		}
		#endregion

		public StringEffectiveDate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public StringEffectiveDate()
			: base()
		{
		}

		#region Bound Properties
		#region PreviousValue
		[CargoWise.ComponentModel.MaxLength(Schema.PreviousValueMaxLength)]
		public ZString PreviousValue
		{
			get { return previousValue; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(PreviousValueInfo, ref previousValue, value);
			}
		}
		ZString previousValue;

		public ZPropertyInfo PreviousValueInfo
		{
			get { return GetZPropertyInfo(Schema.PreviousValue); }
		}
		#endregion

		#region NewValue
		[CargoWise.ComponentModel.MaxLength(Schema.NewValueMaxLength)]
		public ZString NewValue
		{
			get { return newValue; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(NewValueInfo, ref newValue, value);
			}
		}
		ZString newValue;

		public ZPropertyInfo NewValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewValue); }
		}
		#endregion

		#region EffectiveDate
		public ZDateTime EffectiveDate
		{
			get { return effectiveDate; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(EffectiveDateInfo, ref effectiveDate, value);
			}
		}
		ZDateTime effectiveDate;

		public ZPropertyInfo EffectiveDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveDate); }
		}
		#endregion
		#endregion

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			PreviousValue = ZString.Empty;
			NewValue = ZString.Empty;
			EffectiveDate = ZDateTime.Today;
		}

		const string DateFormat = "yyyyMMdd";
		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.PreviousValue, PreviousValue);
			writer.WriteElementString(Schema.NewValue, NewValue);
			writer.WriteElementString(Schema.EffectiveDate, EffectiveDate.ToString(DateFormat));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PreviousValue = reader.ReadElementString(Schema.PreviousValue);
			NewValue = reader.ReadElementString(Schema.NewValue);
			EffectiveDate = reader.ReadElementStringAsZDateTime(Schema.EffectiveDate, DateFormat);
		}

		#endregion

		public ZString EffectiveValueForToday
		{
			get { return EffectiveValue(ZDate.Today); }
		}

		public ZString EffectiveValue(ZDate dateToCheck)
		{
			return dateToCheck < EffectiveDate.Date ? PreviousValue : NewValue;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StringEffectiveDate();
		}
	}
}
