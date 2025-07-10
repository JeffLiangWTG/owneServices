using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Newtonsoft.Json;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Poland
{
	public class PolandEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch accBatch, GlbBranch branch, UniversalTransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var uniqueIdHelper = (ITransactionInfoHelper)new TransactionInfoHelper();

			if (universalTransactionBatch.TransactionCollection == null)
			{
				return null;
			}

			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			foreach (var t in universalTransactionBatch.TransactionCollection.Where(x => x.ShipmentCollection != null))
			{
				// Note that ShipmentCollection is removed after this code runs, but before the XUT is assigned to the GEI message.
				var goodsDescriptions = t.ShipmentCollection
						.Select(x => x.GoodsDescription ?? "")
						.Where(x => !x.IsEmpty)
						.Distinct();
				var orderNumbers = t.ShipmentCollection
						.Where(x => x.LocalProcessing != null)
						.SelectMany(x => (x.LocalProcessing.OrderNumberCollection ?? Enumerable.Empty<OrderNumber>())
											.OrderBy(y => y.Sequence)
											.Select(y => y.OrderReference ?? "")
						)
						.Where(y => !y.IsEmpty)
						.Distinct();
				var shipmentDetails = new
				{
					GoodsDescriptions = goodsDescriptions,
					OrderNumbers = orderNumbers,
				};

				if (shipmentDetails.GoodsDescriptions.Any()
					|| shipmentDetails.OrderNumbers.Any())
				{
					var shipmentDetailsAsJson = JsonConvert.SerializeObject(shipmentDetails, Formatting.None);

					var transactionUniqueId = uniqueIdHelper.GetUniqueIdentifierWithinBatch(t);
					result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
					{
						Key = $"{transactionUniqueId}║ShipmentDetails",
						Value = shipmentDetailsAsJson,
					});
				}
			}

			return result.Count == 0 ? null : result;
		}
	}
}
