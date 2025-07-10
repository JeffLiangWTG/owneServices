//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACFIAEndUseCodesValidation
//
//    This class should be used for overriding validation in AutoCACFIAEndUseCodesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACFIAEndUseCodesValidation : AutoCACFIAEndUseCodesValidation
	{
		public CACFIAEndUseCodesValidation(AutoCACFIAEndUseCodes parent) : base(parent)
		{
		}
	}
}
