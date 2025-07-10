using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	public class QuantityTypeWrapper : IQuantityType
	{
		QuantityTypeWrapper(ZDecimal quantity, ZString unitQty)
		{
			this.quantity = quantity;
			this.unitQty = unitQty;
		}

		internal static IQuantityType NewOrNull(ZDecimal quantity, ZString unitQty) => new QuantityTypeWrapper(quantity, unitQty);

		MeasurementUnitCommonCodeContentType? IQuantityType.UnitCode => Cache.ContainsKey(unitQty) ? Cache[unitQty] : null;

		decimal IQuantityType.Value => quantity;

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

		readonly ZDecimal quantity;
		readonly ZString unitQty;
	}
}
