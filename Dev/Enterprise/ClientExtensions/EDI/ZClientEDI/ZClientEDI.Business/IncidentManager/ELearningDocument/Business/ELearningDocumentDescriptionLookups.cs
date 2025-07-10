//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoELearningDocumentDescriptionLookups
//
//    This class should be used for overriding collections in AutoELearningDocumentDescriptionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public class ELearningDocumentDescriptionLookups : AutoELearningDocumentDescriptionLookups
	{
		public ELearningDocumentDescriptionLookups(AutoELearningDocumentDescription parent) : base(parent)
		{
		}
	}
}
