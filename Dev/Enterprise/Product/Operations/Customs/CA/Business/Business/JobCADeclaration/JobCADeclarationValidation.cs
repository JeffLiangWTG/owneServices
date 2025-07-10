//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCADeclarationValidation
//
//    This class should be used for overriding validation in AutoJobCADeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class JobCADeclarationValidation : AutoJobCADeclarationValidation
	{
		public JobCADeclarationValidation(AutoJobCADeclaration parent)
			: base(parent)
		{
		}
	}
}
