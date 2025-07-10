using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZAddressExposed : ZAddress
	{
		public ZAddressExposed(ZPropertyInfo addressInfo)
			: base(addressInfo)
		{
		}

		public ZPropertyInfo OuterAddressInfoExposed
		{
			get { return AddressFKInfo; }
		}
	}
}
