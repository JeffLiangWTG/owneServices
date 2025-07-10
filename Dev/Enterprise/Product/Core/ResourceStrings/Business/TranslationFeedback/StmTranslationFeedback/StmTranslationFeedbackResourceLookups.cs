//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmTranslationFeedbackResourceLookups
//
//    This class should be used for overriding collections in AutoStmTranslationFeedbackResourceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackResourceLookups : AutoStmTranslationFeedbackResourceLookups
	{
		public StmTranslationFeedbackResourceLookups(AutoStmTranslationFeedbackResource parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ResourceStringLevels
		{
			get { return ResourceStringDataLevels.List; }
		}

		public CodeDescriptionPairList MatchTypes
		{
			get { return TranslationFeedbackMatchTypes.List; }
		}
	}
}
