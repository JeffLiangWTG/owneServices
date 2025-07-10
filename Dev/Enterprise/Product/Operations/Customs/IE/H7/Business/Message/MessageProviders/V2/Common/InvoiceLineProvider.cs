using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class InvoiceLineProvider : IMoney
	{
		public InvoiceLineProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public static InvoiceLineProvider NewOrNull(AsycudaPackedItem packItem)
		{
			return packItem == null ? null : new InvoiceLineProvider(packItem);
		}

		public Decimal Amount => packedItem.API_GoodsValue;

		public string Currency => packedItem.API_RX_NKGoodsValueCurrency;
	}
}
