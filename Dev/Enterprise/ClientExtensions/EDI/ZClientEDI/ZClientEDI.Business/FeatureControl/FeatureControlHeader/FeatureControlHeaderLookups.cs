//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlHeaderLookups
//
//    This class should be used for overriding collections in AutoFeatureControlHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlHeaderLookups : AutoFeatureControlHeaderLookups
	{
		public FeatureControlHeaderLookups(AutoFeatureControlHeader parent) : base(parent)
		{
		}

		public NewWorkItemCollection WorkItems => new NewWorkItemCollection(Factory);
	}
}
