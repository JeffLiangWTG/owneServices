//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCADepotHouseValidation
//
//    This class should be used for overriding validation in AutoCusSCADepotHouseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotHouseValidation : AutoCusSCADepotHouseValidation
	{
		public CusSCADepotHouseValidation(AutoCusSCADepotHouse parent)
			: base(parent)
		{
		}
	}
}
