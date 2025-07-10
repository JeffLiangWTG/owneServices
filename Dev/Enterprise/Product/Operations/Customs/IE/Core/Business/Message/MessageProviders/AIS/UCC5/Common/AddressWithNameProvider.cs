using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class AddressWithNameProvider : AddressProvider, IAddressWithName
	{
		public static new AddressWithNameProvider New(IDocAddress address) => address == null ? null : new AddressWithNameProvider(address);

		AddressWithNameProvider(IDocAddress address) : base(address)
		{
		}

		public string Name => address.E2_CompanyName;

		const string DefaultEmptyPostCode = "000";

		public override string Postcode => Country.Equals(Core.Constants.CountryCodes.HongKong) && base.Postcode.IsEmpty() ? DefaultEmptyPostCode : base.Postcode;
	}
}
