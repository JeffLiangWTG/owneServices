//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLicenseAgreementValidation
//
//    This class should be used for overriding validation in AutoLicenseAgreementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Licensing
{
	public class LicenseAgreementValidation : AutoLicenseAgreementValidation
	{
		public LicenseAgreementValidation(AutoLicenseAgreement parent) : base(parent)
		{
		}
	}
}
