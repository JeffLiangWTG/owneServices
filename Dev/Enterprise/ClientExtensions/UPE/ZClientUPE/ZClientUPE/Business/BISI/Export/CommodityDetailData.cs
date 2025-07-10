
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class CommodityDetailData
	{
		public CommodityDetailData(ZString goodsDescription, ZString tariffNumber, ZString countryOfOrigin, ZDecimal itemPrice)
		{
			fGoodsDescription = goodsDescription;
			fTariffNumber = tariffNumber;
			fCountryOfOrigin = countryOfOrigin;
			fItemPrice = itemPrice;
		}

		public ZString GoodsDescription
		{
			get { return fGoodsDescription; }
		}

		public ZString TariffNumber
		{
			get { return fTariffNumber; }
		}

		public ZString CountryOfOrigin
		{
			get { return fCountryOfOrigin; }
		}

		public ZDecimal Weight
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal ItemPrice
		{
			get { return fItemPrice; }
		}

		public ZString CPC
		{
			get { return ZString.Empty; }
		}

		readonly ZString fGoodsDescription;
		readonly ZString fTariffNumber;
		readonly ZString fCountryOfOrigin;
		readonly ZDecimal fItemPrice;
	}
}
