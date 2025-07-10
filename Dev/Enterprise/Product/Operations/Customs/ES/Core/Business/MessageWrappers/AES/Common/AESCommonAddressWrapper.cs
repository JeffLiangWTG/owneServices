using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonAddressWrapper : PartyAddressWrapper
	{
		public static AESCommonAddressWrapper New(OrgAddress orgAddress, bool isProvisionalPeriod = false) =>
			orgAddress == null ? null : new AESCommonAddressWrapper(orgAddress.OA_Address1, orgAddress.OA_City, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, isProvisionalPeriod);

		public static AESCommonAddressWrapper New(ZString address, ZString city, ZString postCode, ZString country, bool isProvisionalPeriod = false) => new AESCommonAddressWrapper(address, city, postCode, country, isProvisionalPeriod);

		AESCommonAddressWrapper(ZString address, ZString city, ZString postCode, ZString country, ZBool isProvisionalPeriod)
			: base(isProvisionalPeriod ? address.Left(AddressMaxLengthForProvisionalPeriod) : address,
				  city,
				  isProvisionalPeriod ? postCode.Left(PostCodeMaxLengthForProvisionalPeriod) : postCode,
				  country)
		{
		}

		const int AddressMaxLengthForProvisionalPeriod = 35;
		const int PostCodeMaxLengthForProvisionalPeriod = 9;
	}
}
