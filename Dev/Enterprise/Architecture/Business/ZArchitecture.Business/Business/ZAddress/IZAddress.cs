using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IZAddress
	{
		BusinessObjectFactory Factory { get; }
		AddressType DefaultAddressType { get; }
		ZAddressList AddressList { get; }
		ZGuid OrgPK { get; }
		bool IsDeleted { get; }
	}
}
