using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	public sealed class AmountTypeWrapper : IAmountType
	{
		AmountTypeWrapper(ZDecimal invoiceAmount, ZString invoiceCurrency)
		{
			this.amount = invoiceAmount;
			this.currency = invoiceCurrency;
		}

		internal static AmountTypeWrapper NewOrNull(ZDecimal amount, ZString currency)
			=> amount.IsEmpty || currency.IsEmpty
			? null
			: new AmountTypeWrapper(amount, currency);

		#region IAmountType

		Iso3AlphaCurrencyCodeContentType? IAmountType.CurrencyID => Cache.ContainsKey(currency) ? Cache[currency] : null;

		decimal IAmountType.Value => amount;

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

		readonly ZDecimal amount;
		readonly ZString currency;
	}
}
