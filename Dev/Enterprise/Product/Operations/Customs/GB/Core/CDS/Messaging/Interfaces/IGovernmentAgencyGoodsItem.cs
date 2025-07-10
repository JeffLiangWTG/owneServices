using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IGovernmentAgencyGoodsItem
	{
		IEnumerable<IPreviousDocument> PreviousDocuments { get; }
		IEnumerable<IStatement> AdditionalInformations { get; }
		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }
		IEnumerable<IGovernmentProcedure> GovernmentProcedures { get; }
		IOrganisation Consignor { get; }
		IOrganisation Seller { get; }
		IOrganisation Buyer { get; }
		IEnumerable<IParty> AEOMutualRecognitionParties { get; }
		IEnumerable<IParty> DomesticDutyTaxParties { get; }
		ZString ValuationAdjustmentAdditionCode { get; }
		ICustomsValuation CustomsValuation { get; }
		ZString DestinationCountryCode { get; }
		IEnumerable<IPackaging> Packagings { get; }
		ZString TransactionNatureCode { get; }
		IAmountAndCurrency StatisticalValue { get; }
		ICommodity Commodity { get; }
		IEnumerable<ICountry> Origins { get; }
		ZString TransportChargesMethodOfPayment { get; }
		ZString ExportCountryCode { get; }
	}
}
