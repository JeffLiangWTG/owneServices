//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiERequestDocumentQueueValidation
//
//    This class should be used for overriding validation in AutoEdiERequestDocumentQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiERequestDocumentQueueValidation : AutoEdiERequestDocumentQueueValidation
	{
		public EdiERequestDocumentQueueValidation(AutoEdiERequestDocumentQueue parent) : base(parent)
		{
		}
	}
}

