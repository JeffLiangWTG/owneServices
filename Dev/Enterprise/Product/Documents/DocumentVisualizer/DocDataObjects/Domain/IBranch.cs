using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IBranch
	{
		ZString Name { get; }
		ZString City { get; }
		ZString Code { get; }
		ZGuid PK { get; }
		IUnloco HomePort { get; }
		ICountry Country { get; }
		IOrganization Organization { get; }
	}
}
