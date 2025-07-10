//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUCChapterValidation
//
//    This class should be used for overriding validation in AutoAUCChapterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCChapterValidation : AutoAUCChapterValidation
	{
		public AUCChapterValidation(AutoAUCChapter parent)
			: base(parent)
		{
		}
	}
}
