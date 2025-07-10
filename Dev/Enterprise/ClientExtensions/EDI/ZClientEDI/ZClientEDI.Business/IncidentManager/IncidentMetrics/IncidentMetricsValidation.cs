//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentMetricsValidation
//
//    This class should be used for overriding validation in AutoIncidentMetricsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class IncidentMetricsValidation : AutoIncidentMetricsValidation
	{
		public IncidentMetricsValidation(AutoIncidentMetrics parent) : base(parent)
		{
		}
	}
}
