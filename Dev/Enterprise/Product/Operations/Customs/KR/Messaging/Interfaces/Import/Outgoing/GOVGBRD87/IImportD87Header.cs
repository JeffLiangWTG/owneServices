using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportD87Header : IMessageDataProvider
	{
		ZBool HouseBillSplitDeclarationIndicator { get; }
		ZString CarnetCertificateNumber { get; }
		ZString RepresentativeProductName { get; }
		IOrganization Supplier { get; }
		ZString BondedAreaCode { get; }
		IOrganization Importer { get; }
		ZString CarnetUseCode { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		ZDate EffectiveToDate { get; }
		ZDecimal TotalInvoiceAmount { get; }
		ZDecimal TotalQty { get; }
		ZString InvoiceCurrency { get; }
		ZDecimal TotalGrossWeight { get; }
		ZString TotalGrossWeighUnit { get; }
		ZDecimal TotalPackQty { get; }
		ZString PackType { get; }
		ZString CargoManagementNo { get; }
		ZString UnipassDeclarantID { get; }
	}
}
