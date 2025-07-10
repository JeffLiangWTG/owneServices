using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.Business;

public class Address : ITAddress
{
	public Address(IDocAddress address)
	{
		this.address = address;
	}

	readonly IDocAddress address;

	public ZString City
	{
		get
		{
			var oa = address as OrgAddress ?? (address as JobDocAddress).Address;
			return oa.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, oa.OA_RL_NKRelatedPortCode)).RL_PortName;
		}
	}
	public ZString Country
	{
		get
		{
			return ((address as JobDocAddress)?.E2_RN_NKCountryCode ?? (address as OrgAddress)?.OA_RN_NKCountryCode) ?? ZString.Empty;
		}
	}
	public ZString CountrySubEntity { get => address.E2_State; }
	public ZString PostalCode { get => address.E2_Postcode; }
	public ZString StreetAndNumber1 { get => address.E2_Address1; }
	public ZString StreetAndNumber2 { get => address.E2_Address2; }
}
