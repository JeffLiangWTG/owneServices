using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class AmountTypeWrapper : IAmountType
	{
		AmountTypeWrapper(ZDecimal invoiceAmount, ZString invoiceCurrency)
		{
			this.invoiceAmount = invoiceAmount;
			this.invoiceCurrency = invoiceCurrency;
		}

		internal static AmountTypeWrapper NewOrNull(ZDecimal invoiceAmount, ZString invoiceCurrency)
			=> invoiceAmount.IsEmpty || invoiceCurrency.IsEmpty
			? null
			: new AmountTypeWrapper(invoiceAmount, invoiceCurrency);

		#region IAmountType

		Iso3AlphaCurrencyCodeContentType? IAmountType.CurrencyId => Cache.ContainsKey(invoiceCurrency) ? Cache[invoiceCurrency] : null;

		decimal IAmountType.Value => invoiceAmount;

		#endregion

		static Dictionary<string, Iso3AlphaCurrencyCodeContentType> Cache
		{
			get
			{
				if (fCache == null)
				{
					fCache = new Dictionary<string, Iso3AlphaCurrencyCodeContentType>();
					var enumType = typeof(Iso3AlphaCurrencyCodeContentType);
					foreach (var field in enumType.GetFields())
					{
						if (Enum.TryParse(field.Name, out Iso3AlphaCurrencyCodeContentType result))
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
		static Dictionary<string, Iso3AlphaCurrencyCodeContentType> fCache = null;

		readonly ZDecimal invoiceAmount;
		readonly ZString invoiceCurrency;
	}
}
