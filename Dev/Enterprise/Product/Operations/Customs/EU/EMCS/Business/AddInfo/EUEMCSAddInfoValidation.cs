//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUEMCSAddInfoValidation
//
//    This class should be used for overriding validation in AutoEUEMCSAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EUEMCSAddInfoValidation : AutoEUEMCSAddInfoValidation
	{
		public EUEMCSAddInfoValidation(AutoEUEMCSAddInfo parent)
			: base(parent)
		{
		}

		protected new AddInfo Parent
		{
			get { return (AddInfo)base.Parent; }
		}
	}
}
