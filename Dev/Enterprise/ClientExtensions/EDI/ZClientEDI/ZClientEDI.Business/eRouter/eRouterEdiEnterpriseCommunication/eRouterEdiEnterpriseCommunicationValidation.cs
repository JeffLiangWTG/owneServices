//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoeRouterEdiEnterpriseCommunicationValidation
//
//    This class should be used for overriding validation in AutoeRouterEdiEnterpriseCommunicationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.eRouter.Business
{
	public class eRouterEdiEnterpriseCommunicationValidation : AutoeRouterEdiEnterpriseCommunicationValidation
	{
		public eRouterEdiEnterpriseCommunicationValidation(AutoeRouterEdiEnterpriseCommunication parent) : base(parent)
		{
		}
	}
}

