using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public enum TariffType
	{
		Export,
		Import
	}

	/// <summary>
	/// TariffPropertyInfo is used by BorderWise to show filtered data.
	/// </summary>
	[TypeConverter(typeof(TariffInfoConverter))]
	public class TariffPropertyInfo
	{
		#region Constructors

		public TariffPropertyInfo()
			: this(TariffType.Export, ZDateTime.Today, ZString.Empty)
		{
		}

		public TariffPropertyInfo(TariffType tariffType, ZDateTime dateForDutyRate, ZString tariffCode)
		{
			TariffType = tariffType;
			DateForDutyRate = dateForDutyRate;
			TariffCode = tariffCode;
		}

		#endregion

		[DefaultValue(TariffType.Export), DisplayName("Tariff Type")]
		public TariffType TariffType { get; set; }

		[DefaultValue(""), DisplayName("Tariff Code Start With")]
		public string TariffCode { get; set; }

		public ZDateTime DateForDutyRate { get; set; }

		public override string ToString()
		{
			return string.Format((NoResString)"Type: '{0}', Code: '{1}'", TariffType, TariffCode); // Used by VS designer
		}
	}

	#region TariffInfoConverter

	public class TariffInfoConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			var properties = TypeDescriptor.GetProperties(typeof(TariffPropertyInfo));
			return new PropertyDescriptorCollection(new[] { properties["TariffType"], properties["TariffCode"] });
		}
	}

	#endregion
}
