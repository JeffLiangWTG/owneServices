using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2JobDocAddressValidation : JobDocAddressValidation
	{
		public ICS2JobDocAddressValidation(ICS2JobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();

			if (!Parent.E2_Postcode.IsEmpty || !Parent.E2_RN_NKCountryCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_CityInfo);
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();

			if (!Parent.E2_City.IsEmpty || !Parent.E2_RN_NKCountryCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_PostcodeInfo);
			}
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();

			if (!Parent.E2_City.IsEmpty || !Parent.E2_Postcode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_RN_NKCountryCodeInfo);
			}
		}

		internal void CheckFacilityPlaceFieldsRequirement()
		{
			ValidateE2_City();
			ValidateE2_Postcode();
			ValidateE2_RN_NKCountryCode();
		}
	}
}
