using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface IDLMOrganisation
	{
		ZString AuthorizationId { get; }
		ZString BusinessNumberForImportExport { get; }
		ZString CompanyName { get; }
		ZString Street { get; }
		ZString City { get; }
		ZString ProvinceState { get; }
		ZString Country { get; }
		ZString PostalZipCode { get; }
		ZString Telephone { get; }
		ZString TelephoneExtension { get; }
		ZString Fax { get; }
		string ToString();
	}
}
