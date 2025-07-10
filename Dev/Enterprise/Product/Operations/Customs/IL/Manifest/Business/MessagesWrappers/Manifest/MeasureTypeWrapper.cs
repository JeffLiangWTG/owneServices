using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class MeasureTypeWrapper : IMeasureType
	{
		MeasureTypeWrapper(ZDecimal value, ZString unit)
		{
			this.value = value;
			this.unit = unit;
		}

		public static MeasureTypeWrapper NewOrNull(ZDecimal value, ZString unit) => new MeasureTypeWrapper(value, unit);

		public MeasurementUnitCommonCodeContentType? UnitCode => Cache.ContainsKey(unit) ? Cache[unit] : null;

		public decimal Value => value;

		static Dictionary<string, MeasurementUnitCommonCodeContentType> Cache
		{
			get
			{
				if (fCache == null)
				{
					fCache = new Dictionary<string, MeasurementUnitCommonCodeContentType>();
					var enumType = typeof(MeasurementUnitCommonCodeContentType);
					foreach (var field in enumType.GetFields())
					{
						if (Enum.TryParse(field.Name, out MeasurementUnitCommonCodeContentType result))
						{
							var attribute = Attribute.GetCustomAttribute(field, typeof(XmlEnumAttribute)) as XmlEnumAttribute;
							if (attribute?.Name != null)
							{
								fCache.Add(attribute.Name, result);
							}
							else
							{
								fCache.Add(field.Name, result);
							}
						}
					}
				}
				return fCache;
			}
		}
		[ThreadSafe]
		static Dictionary<string, MeasurementUnitCommonCodeContentType> fCache = null;

		readonly ZDecimal value;
		readonly ZString unit;
	}
}
