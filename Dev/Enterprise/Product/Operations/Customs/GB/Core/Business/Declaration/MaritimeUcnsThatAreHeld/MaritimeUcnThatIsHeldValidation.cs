//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMaritimeUcnThatIsHeldValidation
//
//    This class should be used for overriding validation in AutoMaritimeUcnThatIsHeldValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class MaritimeUcnThatIsHeldValidation : AutoMaritimeUcnThatIsHeldValidation
	{
		public MaritimeUcnThatIsHeldValidation(AutoMaritimeUcnThatIsHeld parent) : base(parent)
		{
		}
	}
}

