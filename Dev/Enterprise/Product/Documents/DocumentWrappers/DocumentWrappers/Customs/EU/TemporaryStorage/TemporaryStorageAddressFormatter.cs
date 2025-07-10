using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

sealed class TemporaryStorageAddressFormatter
{
	public TemporaryStorageAddressFormatter(IAddressDetails addressDetails)
	{
		this.addressDetails = Argument.NotNull(addressDetails, nameof(addressDetails));
	}

	public ZString AsString() => new ZStringBuilder()
		.AppendIfNotEmpty(addressDetails.CompanyName)
		.AppendIfNotEmpty(addressDetails.AddressLine1)
		.AppendIfNotEmpty(addressDetails.AddressLine2)
		.AppendIfNotEmpty(addressDetails.PostCode)
		.AppendIfNotEmpty(addressDetails.City)
		.AppendIfNotEmpty(addressDetails.Country)
		.ToStringWithNewLineBetweenAppends();

	readonly IAddressDetails addressDetails;
}
