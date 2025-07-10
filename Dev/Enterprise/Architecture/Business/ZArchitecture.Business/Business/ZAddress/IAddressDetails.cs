using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IAddressDetails
	{
		ZString CompanyName { get; }
		ZString ContactName { get; }
		ZString Phone { get; }
		ZString Fax { get; }
		ZString Email { get; }
		ZString AddressLine1 { get; }
		ZString AddressLine2 { get; }
		ZString City { get; }
		ZString State { get; }
		ZString PostCode { get; }
		ZString Country { get; }
	}
}
