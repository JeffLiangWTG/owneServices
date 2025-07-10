//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentMetricsLookups
//
//    This class should be used for overriding collections in AutoIncidentMetricsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class IncidentMetricsLookups : AutoIncidentMetricsLookups
	{
		public IncidentMetricsLookups(AutoIncidentMetrics parent) : base(parent)
		{
		}
	}
}
