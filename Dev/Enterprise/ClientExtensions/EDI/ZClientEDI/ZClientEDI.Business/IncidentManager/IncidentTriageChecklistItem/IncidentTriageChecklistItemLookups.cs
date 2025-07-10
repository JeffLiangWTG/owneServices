//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageChecklistItemLookups
//
//    This class should be used for overriding collections in AutoIncidentTriageChecklistItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageChecklistItemLookups : AutoIncidentTriageChecklistItemLookups
	{
		public IncidentTriageChecklistItemLookups(AutoIncidentTriageChecklistItem parent) : base(parent)
		{
		}

		public IncidentTriageChecklistItemLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		public new IncidentTriageChecklistItem Parent
		{
			get { return (IncidentTriageChecklistItem)base.Parent; }
		}

		public CodeDescriptionPairList Categorys => new IncidentTriageChecklistItemCategorys();

		public CodeDescriptionPairList ResponseTypes => new CodeDescriptionPairList();
	}
}
