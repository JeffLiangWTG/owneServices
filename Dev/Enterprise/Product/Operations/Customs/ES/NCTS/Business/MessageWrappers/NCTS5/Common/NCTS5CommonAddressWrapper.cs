using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonAddressWrapper : INCTSCommonAddress
	{
		public static NCTS5CommonAddressWrapper New(OrgAddress orgAddress, bool isInPhase5TransitionPeriod) =>
			orgAddress == null ? null : new NCTS5CommonAddressWrapper(orgAddress.OA_Address1, orgAddress.OA_City, orgAddress.OA_PostCode, false, isInPhase5TransitionPeriod);

		public static NCTS5CommonAddressWrapper New(ZString address, ZString city, ZString postCode, bool isInPhase5TransitionPeriod) => new NCTS5CommonAddressWrapper(address, city, postCode, false, isInPhase5TransitionPeriod);

		protected NCTS5CommonAddressWrapper(ZString streetAndNumber, ZString city, ZString postCode, bool shouldNotTrimPostCode, bool isInPhase5TransitionPeriod)
		{
			StreetAndNumber = !TrimAddressAndPostCode ? streetAndNumber : streetAndNumber.SubstringSafe(0, isInPhase5TransitionPeriod ? StreetAndNumberMaxCharsProvisionalPeriod : StreetAndNumberMaxCharsFinalPeriod);
			City = city;
			PostCode = shouldNotTrimPostCode || !TrimAddressAndPostCode ? postCode : postCode.SubstringSafe(0, isInPhase5TransitionPeriod ? PostCodeMaxCharsProvisionalPeriod : PostCodeMaxCharsFinalPeriod);
		}

		public ZString StreetAndNumber { get; }

		public ZString City { get; }

		public ZString PostCode { get; }

		protected virtual bool TrimAddressAndPostCode => false;

		const int StreetAndNumberMaxCharsProvisionalPeriod = 35;

		const int StreetAndNumberMaxCharsFinalPeriod = 70;

		const int PostCodeMaxCharsProvisionalPeriod = 9;

		const int PostCodeMaxCharsFinalPeriod = 17;
	}
}
