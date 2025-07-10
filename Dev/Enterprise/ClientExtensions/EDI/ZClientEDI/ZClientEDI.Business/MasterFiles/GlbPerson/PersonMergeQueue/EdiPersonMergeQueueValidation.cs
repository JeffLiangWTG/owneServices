//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPersonMergeQueueValidation
//
//    This class should be used for overriding validation in AutoEdiPersonMergeQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiPersonMergeQueueValidation : AutoEdiPersonMergeQueueValidation
	{
		public EdiPersonMergeQueueValidation(AutoEdiPersonMergeQueue parent) : base(parent)
		{
		}
	}
}