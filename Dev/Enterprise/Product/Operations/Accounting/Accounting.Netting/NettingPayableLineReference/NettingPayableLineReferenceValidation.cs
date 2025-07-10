//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingPayableLineReferenceValidation
//
//    This class should be used for overriding validation in AutoNettingPayableLineReferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableLineReferenceValidation : AutoNettingPayableLineReferenceValidation
	{
		public NettingPayableLineReferenceValidation(AutoNettingPayableLineReference parent) : base(parent)
		{
		}
	}
}
