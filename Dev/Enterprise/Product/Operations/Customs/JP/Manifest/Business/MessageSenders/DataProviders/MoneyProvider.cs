using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class MoneyProvider : IMoney
	{
		public MoneyProvider(AsycudaBill bill, string type)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.type = Argument.NotNull(type, nameof(type));
		}

		readonly AsycudaBill bill;
		readonly string type;

		public decimal? Amount
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.FreightValue):
						return bill.ABL_TransportValue;
					case nameof(BillProvider.GoodsValue):
						return bill.ABL_FreightValue;
					default:
						return 0;
				}
			}
		}

		public string CurrencyCode
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.FreightValue):
						return bill.ABL_RX_NKTransportValueCurrency;
					case nameof(BillProvider.GoodsValue):
						return bill.ABL_RX_NKFreightValueCurrency;
					default:
						return string.Empty;
				}
			}
		}
	}
}
