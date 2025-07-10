//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentValidation : AutoCMRInstrumentValidation
	{
		public CMRInstrumentValidation(AutoCMRInstrument parent)
			: base(parent)
		{
		}
	}
}
