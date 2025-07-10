using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	public class MeasureTypeWrapper : IMeasureType
	{
		readonly ZDecimal measure;
		readonly ZString codeType;

		MeasureTypeWrapper(ZDecimal measure, ZString codeType)
		{
			this.measure = measure;
			this.codeType = codeType;
		}

		public static MeasureTypeWrapper NewOrNull(ZDecimal measure, ZString codeType)
			=> !measure.IsEmpty ? new MeasureTypeWrapper(measure, codeType) : null;

		public MeasurementUnitCommonCodeContentType? UnitCode => Cache.ContainsKey(codeType) ? Cache[codeType] : null;

		public decimal Value => measure;

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
	}
}
