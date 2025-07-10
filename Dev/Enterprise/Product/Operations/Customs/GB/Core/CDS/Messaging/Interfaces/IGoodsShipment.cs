using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IGoodsShipment
	{
		IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems { get; }
		ZString UCRTraderAssignedReferenceID { get; }
		IWareHouse Warehouse { get; }
		IOrganisation Importer { get; }
		IOrganisation Seller { get; }
		IOrganisation Buyer { get; }
		IEnumerable<IParty> AEOMutualRecognitionParties { get; }
		IEnumerable<IParty> DomesticDutyTaxParties { get; }
		ITradeTerms TradeTerms { get; }
		ICustomsValuation CustomsValuation { get; }
		ZString ExportCountryID { get; }
		ZString DestinationCountryCode { get; }
		ZString TransactionNatureCode { get; }
		IConsignment Consignment { get; }
		IEnumerable<IPreviousDocument> PreviousDocuments { get; }
	}
}
