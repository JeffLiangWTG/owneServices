using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM413AndIM415GoodsShipmentItemProvider : IIM413AndIM415GoodsShipmentItem
	{
		public IM413AndIM415GoodsShipmentItemProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public string GoodsItemNumber => packedItem.API_LineNo.ToString();

		public IReadOnlyCollection<string> AdditionalProcedure => additionalProcedure ??= MessageProviderHelper.GetAdditionalProcedures(packedItem);
		IReadOnlyCollection<string> additionalProcedure;

		public IDocumentsAuth DocumentsAuthorisations => CachedValueHelper.GetValue(ref documentsAuthorisationsCached, () => new DocumentAuthorisationsProvider(packedItem));
		CachedValue<IDocumentsAuth> documentsAuthorisationsCached;

		public IParties03 Parties => CachedValueHelper.GetValue(ref partiesCached, () => new Parties03Provider(packedItem));
		CachedValue<IParties03> partiesCached;

		public IReadOnlyCollection<IPayment> Taxes => Array.Empty<IPayment>();

		public IValuationInformation02 ItemAmountInvoicedIntrinsicValue => CachedValueHelper.GetValue(ref itemAmountInvoicedIntrinsicValueCached, () => new ItemAmountInvoicedIntrinsicValueProvider(packedItem));
		CachedValue<IValuationInformation02> itemAmountInvoicedIntrinsicValueCached;

		public IGoodsInformation GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new GoodsInformationProvider(packedItem));
		CachedValue<IGoodsInformation> goodsInformationCached;
	}
}
