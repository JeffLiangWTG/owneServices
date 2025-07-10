//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmTranslationFeedbackLookups
//
//    This class should be used for overriding collections in AutoStmTranslationFeedbackLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackLookups : AutoStmTranslationFeedbackLookups
	{
		public StmTranslationFeedbackLookups(AutoStmTranslationFeedback parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		public CodeDescriptionPairList ResourceStringLevels
		{
			get { return ResourceStringDataLevels.List; }
		}

		public CodeDescriptionPairList MatchTypes
		{
			get { return TranslationFeedbackMatchTypes.List; }
		}

		public CodeDescriptionPairList Statuses
		{
			get { return new TranslationFeedbackStatusList(); }
		}
	}
}
