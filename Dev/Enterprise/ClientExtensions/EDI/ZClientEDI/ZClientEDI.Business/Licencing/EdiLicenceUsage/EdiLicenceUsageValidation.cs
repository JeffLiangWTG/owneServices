//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiLicenceUsageValidation
//
//    This class should be used for overriding validation in AutoEdiLicenceUsageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class EdiLicenceUsageValidation : AutoEdiLicenceUsageValidation
	{
		public EdiLicenceUsageValidation(AutoEdiLicenceUsage parent) : base(parent)
		{
		}
	}
}

