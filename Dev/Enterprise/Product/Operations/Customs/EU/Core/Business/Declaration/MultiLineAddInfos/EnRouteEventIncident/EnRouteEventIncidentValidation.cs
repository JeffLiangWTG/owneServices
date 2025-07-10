//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEnRouteEventIncidentValidation
//
//    This class should be used for overriding validation in AutoEnRouteEventIncidentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class EnRouteEventIncidentValidation : AutoEnRouteEventIncidentValidation
	{
		public EnRouteEventIncidentValidation(AutoEnRouteEventIncident parent) : base(parent)
		{
		}
	}
}
