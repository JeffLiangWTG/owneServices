//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACClassHeaderValidation
//
//    This class should be used for overriding validation in AutoCACClassHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACClassHeaderValidation : AutoCACClassHeaderValidation
	{
		public CACClassHeaderValidation(AutoCACClassHeader parent) : base(parent)
		{
		}
	}
}
