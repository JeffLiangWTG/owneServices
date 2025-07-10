using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class TruncatedImportPartyIdAddressProvider : IImportPartyIdAddress
	{
		public static IImportPartyIdAddress NewOrNull(IImportPartyIdAddress inner) => inner == null ? null : new TruncatedImportPartyIdAddressProvider(inner);

		TruncatedImportPartyIdAddressProvider(IImportPartyIdAddress inner)
		{
			this.inner = inner;
		}

		readonly IImportPartyIdAddress inner;

		public string Name => inner.Name.LeftOrNull(MessageSchema.ATLASMessageSchema.PartyNameMaxLength);

		public string District => inner.District.LeftOrNull(MessageSchema.ATLASMessageSchema.AddressDistrictMaxLength);

		public string Address => inner.Address.LeftOrNull(MessageSchema.ATLASMessageSchema.AddressLineMaxLength);

		public string City => inner.City.LeftOrNull(MessageSchema.ATLASMessageSchema.AddressCityMaxLength);

		public string Postcode => inner.Postcode.LeftOrNull(MessageSchema.ATLASMessageSchema.AddressPostcodeMaxLength);

		public string Country => inner.Country;
	}
}
