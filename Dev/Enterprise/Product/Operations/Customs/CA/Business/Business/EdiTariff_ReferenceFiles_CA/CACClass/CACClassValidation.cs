//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACClassValidation
//
//    This class should be used for overriding validation in AutoCACClassValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACClassValidation : AutoCACClassValidation
	{
		public CACClassValidation(AutoCACClass parent) : base(parent)
		{
		}
	}
}
