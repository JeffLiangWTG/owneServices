//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMREstablishmentCodesValidation
//
//    This class should be used for overriding validation in AutoCMREstablishmentCodesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREstablishmentCodesValidation : AutoCMREstablishmentCodesValidation
	{
		public CMREstablishmentCodesValidation(AutoCMREstablishmentCodes parent)
			: base(parent)
		{
		}
	}
}
