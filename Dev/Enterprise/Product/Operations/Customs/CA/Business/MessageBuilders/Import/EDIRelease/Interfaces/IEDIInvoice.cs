using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IEDIInvoiceMin
	{
		// Minimum data
		ZString InvoiceNumber { get; }
		IDocAddress Vendor { get; }
		IDocAddress Purchaser { get; }
		IDocAddress Consignee { get; }
		IDocAddress Exporter { get; }
		ZString HeaderOrigin { get; }
		ZString CommonCountryOfOrigin { get; }
		ZString CommonCountryOfExport { get; }
		IEnumerable<IEDIInvoiceLineOGD> InvoiceLines { get; }
	}

	public interface IEDIInvoiceAQ : IEDIInvoiceMin
	{
		// AQ data
		ZDate InvoiceDate { get; }
		ZDecimal InvoiceAmount { get; }
		ZString InvoiceCurrency { get; }
		ZDecimal IncludedOFTAndONS { get; }
		ZDecimal IncludedConstruction { get; }
		ZDecimal IncludedPacking { get; }
		ZDecimal ExcludedOFTAndONS { get; }
		ZDecimal ExcludedCommission { get; }
		ZDecimal ExcludedPacking { get; }
		ZString OtherReference { get; }
		IDocAddress Shipper { get; }
		ZString DepartmentRuling { get; }
		ZString LastPortName { get; }
		ZDate LastPortDate { get; }
		ZString TranshipmentCountry { get; }
		ZString ConditionsOfSale { get; }
		ZString TermsOfPayment { get; }
		ZBool ServicesInd { get; }
		ZBool RoyaltyInd { get; }
	}

	public interface IEDIInvoiceOGD : IEDIInvoiceAQ
	{
		// OGD data
		IDocAddress Manufacturer { get; }
	}
}
