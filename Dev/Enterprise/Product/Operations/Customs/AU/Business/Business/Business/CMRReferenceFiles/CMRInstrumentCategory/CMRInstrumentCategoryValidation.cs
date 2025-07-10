//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryValidation : AutoCMRInstrumentCategoryValidation
	{
		public CMRInstrumentCategoryValidation(AutoCMRInstrumentCategory parent)
			: base(parent)
		{
		}
	}
}
