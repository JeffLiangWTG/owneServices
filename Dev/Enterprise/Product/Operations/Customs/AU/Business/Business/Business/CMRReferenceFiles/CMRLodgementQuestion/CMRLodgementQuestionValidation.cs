//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRLodgementQuestionValidation
//
//    This class should be used for overriding validation in AutoCMRLodgementQuestionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRLodgementQuestionValidation : AutoCMRLodgementQuestionValidation
	{
		public CMRLodgementQuestionValidation(AutoCMRLodgementQuestion parent)
			: base(parent)
		{
		}
	}
}
