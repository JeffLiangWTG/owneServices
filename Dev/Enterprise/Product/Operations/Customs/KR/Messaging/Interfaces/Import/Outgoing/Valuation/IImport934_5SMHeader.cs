using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport934_5SMHeader : IMessageDataProvider
	{
		ZString ValuationMethod { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Payer { get; }
		IOrganization Supplier { get; }
		IOrganization Importer { get; }
		ZString PurchaseOrderNo { get; }
		ZDate PurchaseOrderDate { get; }
		IValueDeclarationPerson Author { get; }
		IValueDeclarationPerson ResponsiblePerson { get; }
	}
}
