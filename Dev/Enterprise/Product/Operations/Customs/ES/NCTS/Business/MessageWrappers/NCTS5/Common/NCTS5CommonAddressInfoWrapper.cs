using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonAddressInfoWrapper : NCTS5CommonAddressWrapper, INCTSCommonAddressInfo
	{
		public static NCTS5CommonAddressInfoWrapper New(OrgAddress orgAddress, bool shouldNotTrimPostCode = false, bool isInPhase5TransitionPeriod = false) =>
			orgAddress == null ? null : new NCTS5CommonAddressInfoWrapper(orgAddress.OA_Address1, orgAddress.OA_City, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, shouldNotTrimPostCode, isInPhase5TransitionPeriod);

		public static NCTS5CommonAddressInfoWrapper New(JobDocAddress jobDocAddress, bool shouldNotTrimPostCode = false, bool isInPhase5TransitionPeriod = false) =>
		jobDocAddress?.Address == null ? null : jobDocAddress.E2_AddressOverride ? new NCTS5CommonAddressInfoWrapper(jobDocAddress.E2_Address1AndE2_Address2.SubstringSafe(0, OrgAddress.Schema.OA_Address1MaxLength), jobDocAddress.E2_City, jobDocAddress.E2_Postcode, jobDocAddress.E2_RN_NKCountryCode, shouldNotTrimPostCode, isInPhase5TransitionPeriod) : NCTS5CommonAddressInfoWrapper.New(jobDocAddress.Address, shouldNotTrimPostCode, isInPhase5TransitionPeriod);

		NCTS5CommonAddressInfoWrapper(ZString streetAndNumber, ZString city, ZString postCode, ZString country, bool shouldNotTrimPostCode, bool isInPhase5TransitionPeriod) : base(streetAndNumber, city, postCode, shouldNotTrimPostCode, isInPhase5TransitionPeriod)
		{
			Country = country;
		}

		public ZString Country { get; }

		protected override bool TrimAddressAndPostCode => true;
	}
}
