//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiOrgMembershipLookups
//
//    This class should be used for overriding collections in AutoEdiOrgMembershipLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembershipLookups : AutoEdiOrgMembershipLookups
	{
		public EdiOrgMembershipLookups(AutoEdiOrgMembership parent) : base(parent) { }

		public CodeDescriptionBoolCollection MembershipTypes => GetMembershipTypes();

		public static CodeDescriptionBoolCollection GetMembershipTypes()
		{
			var codeDescriptionBoolCollection = EDIDataRegistry.Instance.OrgMembershipTypes.Value;
			codeDescriptionBoolCollection?.Sort((CodeDescriptionBool x1, CodeDescriptionBool x2) =>
				x1.Description.CompareTo(x2.Description)
			);
			return codeDescriptionBoolCollection;
		}
	}
}
