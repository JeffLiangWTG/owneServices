using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IDeclaration
	{
		ZString DeclarationTypeCode { get; }
		ZString SpecificCircumstancesCodeCode { get; }
		ZInt GoodsItemQuantity { get; }
		IGoodsShipment GoodsShipment { get; }
		ZString FunctionalReferenceID { get; }
		IEnumerable<IDecAdditionalDocument> DecAdditionalDocuments { get; }
		IOrganisation Exporter { get; }
		IOrganisation ExporterNameAndAddress { get; }
		IOrganisation Declarant { get; }
		IAgent Agent { get; }
		IEnumerable<IAuthorisationHolder> AuthorisationHolders { get; }
		IEnumerable<ICurrencyExchange> CurrencyExchanges { get; }
		ZString PresentationOffice { get; }
		ZString SupervisingOffice { get; }
		ZDecimal TotalPackageQuantity { get; }
		ITransportMeans BorderTransportMeans { get; }
		IEnumerable<IObligationGuarantee> ObligationGuarantees { get; }
		ZDateTime AcceptanceDateTime { get; }
		ZDecimal TotalGrossMassMeasure { get; }
		IConsignment Consignment { get; }
		ZString ExitOfficeID { get; }
		IAmountAndCurrency InvoiceAmount { get; }

		IAmountAndCurrency FreightChargeAmount { get; }

		IEnumerable<IStatement> AdditionalInformations { get; }
	}
}
