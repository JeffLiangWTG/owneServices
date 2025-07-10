//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCADepotContainerValidation
//
//    This class should be used for overriding validation in AutoCusSCADepotContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotContainerValidation : AutoCusSCADepotContainerValidation
	{
		public CusSCADepotContainerValidation(AutoCusSCADepotContainer parent)
			: base(parent)
		{
		}
	}
}
