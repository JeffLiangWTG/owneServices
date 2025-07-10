using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IOrganisation
	{
		ZString ShortCode { get; }
		ZString EoriCode { get; }
		ZString Name { get; }
		ZString Street { get; }
		ZString City { get; }
		ZString PostCode { get; }
		ZString CountryCode { get; }
		ZBool IsNotMissing { get; }
	}
}
