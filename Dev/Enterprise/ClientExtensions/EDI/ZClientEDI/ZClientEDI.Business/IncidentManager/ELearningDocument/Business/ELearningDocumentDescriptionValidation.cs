//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoELearningDocumentDescriptionValidation
//
//    This class should be used for overriding validation in AutoELearningDocumentDescriptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public class ELearningDocumentDescriptionValidation : AutoELearningDocumentDescriptionValidation
	{
		public ELearningDocumentDescriptionValidation(AutoELearningDocumentDescription parent) : base(parent)
		{
		}
	}
}
