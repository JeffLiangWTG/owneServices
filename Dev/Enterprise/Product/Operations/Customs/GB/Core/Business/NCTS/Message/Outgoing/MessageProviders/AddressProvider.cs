using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class AddressProvider : IAddress
	{
		readonly JobDocAddress orgAddress;
		readonly bool isInPhase5TransitionPeriod;

		public AddressProvider(JobDocAddress orgAddress, bool isInPhase5TransitionPeriod)
		{
			this.orgAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}

		public string StreetAndNumber => string.IsNullOrEmpty(orgAddress.E2_Address1.Trim()) && string.IsNullOrEmpty(orgAddress.E2_Address2.Trim()) ? null : (orgAddress.E2_Address1.Trim() + " " + orgAddress.E2_Address2.Trim()).Trim();

		public string Postcode => string.IsNullOrEmpty(orgAddress.E2_Postcode.Trim()) ? null : orgAddress.E2_Postcode.Trim();

		public string City => string.IsNullOrEmpty(orgAddress.E2_City.Trim()) ? null : orgAddress.E2_City.Trim();

		public string Country => string.IsNullOrEmpty(orgAddress.E2_RN_NKCountryCode) ? null : orgAddress.E2_RN_NKCountryCode;

		public int AddressStreetAndNumberMaxLength => isInPhase5TransitionPeriod ? MessageSchemaInTransitionPeriod.AddressStreetAndNumberMaxLength : MessageSchema.AddressStreetAndNumberMaxLength;
	}
}
