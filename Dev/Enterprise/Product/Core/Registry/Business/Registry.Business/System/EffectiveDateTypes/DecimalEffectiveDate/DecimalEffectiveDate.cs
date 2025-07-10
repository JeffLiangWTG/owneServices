using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DecimalEffectiveDate : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string PreviousValue = "PreviousValue";
			public const string NewValue = "NewValue";
			public const string EffectiveDate = "EffectiveDate";
		}
		#endregion

		public DecimalEffectiveDate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public DecimalEffectiveDate()
			: base()
		{
		}

		#region Bound Properties

		#region PreviousValue

		public virtual ZDecimal PreviousValue
		{
			get { return previousValue; }
			set
			{
				if (previousValue != value)
				{
					SetNonPersistentPropertyValue<ZDecimal>(PreviousValueInfo, ref previousValue, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidatePreviousValue();
					}
				}
			}
		}
		ZDecimal previousValue;

		public ZPropertyInfo PreviousValueInfo
		{
			get { return GetZPropertyInfo(Schema.PreviousValue); }
		}

		#endregion

		#region NewValue

		public virtual ZDecimal NewValue
		{
			get { return newValue; }
			set
			{
				if (newValue != value)
				{
					SetNonPersistentPropertyValue<ZDecimal>(NewValueInfo, ref newValue, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateNewValue();
					}
				}
			}
		}
		ZDecimal newValue;

		public ZPropertyInfo NewValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewValue); }
		}

		#endregion

		#region EffectiveDate

		public virtual ZDateTime EffectiveDate
		{
			get { return effectiveDate; }
			set
			{
				if (effectiveDate != value)
				{
					SetNonPersistentPropertyValue<ZDateTime>(EffectiveDateInfo, ref effectiveDate, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateEffectiveDate();
					}
				}
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
			PreviousValue = ZDecimal.Zero;
			NewValue = ZDecimal.Zero;
			EffectiveDate = ZDateTime.Today;
		}

		const string DateFormat = "yyyyMMdd";
		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.PreviousValue, PreviousValue.ToString());
			writer.WriteElementString(Schema.NewValue, NewValue.ToString());
			writer.WriteElementString(Schema.EffectiveDate, EffectiveDate.ToString(DateFormat));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PreviousValue = reader.ReadElementStringAsZDecimal(Schema.PreviousValue);
			NewValue = reader.ReadElementStringAsZDecimal(Schema.NewValue);
			EffectiveDate = reader.ReadElementStringAsZDateTime(Schema.EffectiveDate, DateFormat);
		}

		#endregion

		public ZDecimal EffectiveValueForToday
		{
			get { return EffectiveValue(ZDate.Today); }
		}

		public ZDecimal EffectiveValue(ZDate dateToCheck)
		{
			return dateToCheck < EffectiveDate.Date ? PreviousValue : NewValue;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DecimalEffectiveDate();
		}

		#region Validation

		public DecimalEffectiveDateValidation Validation => validation ?? (validation = GetNewValidation());
		DecimalEffectiveDateValidation validation;

		protected virtual DecimalEffectiveDateValidation GetNewValidation() => new DecimalEffectiveDateValidation(this);

		#endregion
	}
}
