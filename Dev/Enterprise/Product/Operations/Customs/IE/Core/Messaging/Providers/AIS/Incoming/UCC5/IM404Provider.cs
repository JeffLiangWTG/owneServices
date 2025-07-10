using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.Business;
using GoodsShipmentTypeItem = CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404.GoodsShipmentTypeItem;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM404Provider
	{
		public class GoodsShipmentProvider
		{
			public class ItemProvider : IGoodsItemProvider
			{
				public class TaxBoxProvider : ITaxTypeProvider
				{
					public TaxBoxProvider(TaxBoxType messageObject)
					{
						TaxType = messageObject.BoxTaxType;
						Amount = messageObject.BoxAmount ?? ZDecimal.Zero;
						TaxRate = messageObject.BoxTaxRate ?? ZDecimal.Zero;
						TaxAmount = messageObject.BoxTaxPayableAmount ?? ZDecimal.Zero;
						MethodOfPayment = messageObject.BoxTaxPaymentMethod;
					}

					public ZString TaxType { get; }

					ZString ITaxTypeProvider.Unit => ZString.Empty;

					ZDecimal ITaxTypeProvider.Quantity => ZDecimal.Zero;

					public ZDecimal Amount { get; }

					public ZDecimal TaxRate { get; }

					public ZDecimal TaxAmount { get; }

					public ZString MethodOfPayment { get; }
				}

				public ItemProvider(GoodsShipmentTypeItem messageObject)
				{
					DeclarationGoodsItemNumber = messageObject.GoodsItemNumber16 ?? string.Empty;
					TaxTypes = messageObject.Taxes?.TaxBox43Bis?.Select(x => new TaxBoxProvider(x)).ToArray() ?? Array.Empty<ITaxTypeProvider>();
				}

				public ZString DeclarationGoodsItemNumber { get; }

				public IReadOnlyCollection<ITaxTypeProvider> TaxTypes { get; }
			}

			public GoodsShipmentProvider(GoodsShipmentType messageObject)
			{
				Items = messageObject.GoodsShipmentItem?.Select(x => new ItemProvider(x)).ToArray();
			}

			public IReadOnlyCollection<ItemProvider> Items { get; }
		}

		public IM404Provider(Im404 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im404 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZString PreferredPaymentMethod => xmlObject.Declaration?.PreferredPaymentMethod48;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public ZDate AmendmentAcceptanceDate
		{
			get
			{
				const string dateFormat = "yyyyMMdd";
				var outDate = ZDate.Invalid;
				if (DateTime.TryParseExact(xmlObject.Declaration?.AmendmentAcceptanceDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
				{
					outDate = parsedDate.ConvertToZDate();
				}
				return outDate;
			}
		}

		GoodsShipmentProvider goodsShipmentCache;
		public GoodsShipmentProvider GoodsShipment => goodsShipmentCache ??= new GoodsShipmentProvider(xmlObject.GoodsShipment);
	}
}
