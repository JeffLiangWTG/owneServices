//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRInBondDetailsValidation
//
//    This class should be used for overriding validation in AutoJPAFRInBondDetailsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRInBondDetailsValidation : AutoJPAFRInBondDetailsValidation
	{
		public JPAFRInBondDetailsValidation(AutoJPAFRInBondDetails parent)
			: base(parent)
		{
		}
	}
}
