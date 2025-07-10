//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientLicenceUsageValidation
//
//    This class should be used for overriding validation in AutoClientLicenceUsageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientLicenceUsageValidation : AutoClientLicenceUsageValidation
	{
		public ClientLicenceUsageValidation(AutoClientLicenceUsage parent) : base(parent)
		{
		}
	}
}

