using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7AddressWrapper : IH7Address
{
	public H7AddressWrapper(OrgAddress orgAddress)
	{
		this.orgAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
	}

	readonly OrgAddress orgAddress;

	public string Name => orgAddress.CompanyName;

	public string StreetAndNumber => orgAddress.OA_Address1 + " " + orgAddress.OA_Address2;

	public string Country => orgAddress.OA_RN_NKCountryCode;

	public string ZipCode => orgAddress.Postcode;

	public string City => orgAddress.City;

	public IContact Contact => CachedValueHelper.GetValue(ref contact, () =>
	{
		var orgContact = orgAddress.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
		return orgContact != null ? new ContactWrapper(orgContact) : null;
	});
	CachedValue<IContact> contact;
}
