using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemDatesPlacesProvider : IGoodsShipmentItemTypeDatesPlaces
	{
		public IM413AndIM415GoodsShipmentItemDatesPlacesProvider(JobComInvoiceLine randomInvoiceLine)
		{
			this.randomInvoiceLine = randomInvoiceLine;
		}
		readonly JobComInvoiceLine randomInvoiceLine;

		public string CountryDestination => randomInvoiceLine.ZG_CountryOfDestination;

		public string CountryDispatch => randomInvoiceLine.ZG_CountryOfDispatch;

		public string CountryOrigin => randomInvoiceLine.JI_CountryOfOrigin;

		public string CountryPreferentialOrigin => randomInvoiceLine.ZG_CountryOfSupply;
	}
}
