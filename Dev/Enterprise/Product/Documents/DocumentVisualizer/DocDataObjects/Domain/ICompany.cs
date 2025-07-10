using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ICompany
	{
		ZString Code { get; }
		ZString Name { get; }
		ZGuid PK { get; }
		ZString LicenceCode { get; }
		ICountry Country { get; }
		IOrganization Organization { get; }
	}
}
