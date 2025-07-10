//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientBranchValidation
//
//    This class should be used for overriding validation in AutoClientBranchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientBranchValidation : AutoClientBranchValidation
	{
		public ClientBranchValidation(AutoClientBranch parent) : base(parent)
		{
		}
	}
}
