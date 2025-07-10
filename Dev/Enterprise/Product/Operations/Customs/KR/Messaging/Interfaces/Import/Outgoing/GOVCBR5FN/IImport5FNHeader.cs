using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5FNHeader
	{
		ZString ImportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		ZString TypeOfBusiness { get; }
		IOrganization Payer { get; }
		IOrganization CustomsBroker { get; }
	}
}
