//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCollectionOrderLineValidation
//
//    This class should be used for overriding validation in AutoAccCollectionOrderLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderLineValidation : AutoAccCollectionOrderLineValidation
	{
		public AccCollectionOrderLineValidation(AutoAccCollectionOrderLine parent) : base(parent)
		{
		}
	}
}

