//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingReceivableLineReferenceValidation
//
//    This class should be used for overriding validation in AutoNettingReceivableLineReferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableLineReferenceValidation : AutoNettingReceivableLineReferenceValidation
	{
		public NettingReceivableLineReferenceValidation(AutoNettingReceivableLineReference parent) : base(parent)
		{
		}
	}
}
