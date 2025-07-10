using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AddressProvider : IAddress
	{
		readonly JobDocAddress orgAddress;

		public AddressProvider(JobDocAddress orgAddress, bool isTransitionPeriodAES30 = false)
		{
			this.orgAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
			isTransitionPeriod = isTransitionPeriodAES30;
		}
		readonly bool isTransitionPeriod;

		public string StreetAndNumber => (orgAddress.E2_Address1 + " " + orgAddress.E2_Address2).Trim();

		public string Postcode => orgAddress.E2_Postcode;

		public string City => orgAddress.E2_City;

		public string Country => orgAddress.E2_RN_NKCountryCode;

		public int StreetAndNumberMaxLength => isTransitionPeriod ? MessageSchema.AddressStreetAndNumberMaxLengthInTransitionPeriod : MessageSchema.AddressStreetAndNumberMaxLength;
	}
}
