//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoELearningDocumentTfIdfValidation
//
//    This class should be used for overriding validation in AutoELearningDocumentTfIdfValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public class ELearningDocumentTfIdfValidation : AutoELearningDocumentTfIdfValidation
	{
		public ELearningDocumentTfIdfValidation(AutoELearningDocumentTfIdf parent) : base(parent)
		{
		}
	}
}
