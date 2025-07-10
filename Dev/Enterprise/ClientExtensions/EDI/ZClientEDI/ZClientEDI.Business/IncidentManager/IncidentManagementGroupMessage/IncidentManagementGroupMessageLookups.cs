//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentManagementGroupMessageLookups
//
//    This class should be used for overriding collections in AutoIncidentManagementGroupMessageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupMessageLookups : AutoIncidentManagementGroupMessageLookups
	{
		public IncidentManagementGroupMessageLookups(AutoIncidentManagementGroupMessage parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Types => new IncidentManagementGroupMessageTypePairList();
	}
}
