//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDocumentDeliveryValidation
//
//    This class should be used for overriding validation in AutoStmDocumentDeliveryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Business
{
	public class StmDocumentDeliveryValidation : AutoStmDocumentDeliveryValidation
	{
		public StmDocumentDeliveryValidation(AutoStmDocumentDelivery parent) : base(parent)
		{
		}
	}
}
