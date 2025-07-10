//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityApplicationPermissionLookups
//
//    This class should be used for overriding collections in AutoEdiIdentityApplicationPermissionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business
{
	public class EdiIdentityApplicationPermissionLookups : AutoEdiIdentityApplicationPermissionLookups
	{
		public EdiIdentityApplicationPermissionLookups(AutoEdiIdentityApplicationPermission parent) : base(parent)
		{
		}

		new EdiIdentityApplicationPermission Parent => (EdiIdentityApplicationPermission)base.Parent;

		public CodeDescriptionPairList ApplicationPermissions
		{
			get
			{
				if (!Parent.Application.IsCustomerApplication)
				{
					var nonCustomerApplicationPermission = new CodeDescriptionPairList();
					nonCustomerApplicationPermission.AddPair(AllApplicationsScope);
					return nonCustomerApplicationPermission;
				}

				var permissions = new CustomerApplicationPermissionsList();
				if (Parent.Application.IDA_IDA_ParentApplication.IsEmpty)
				{
					permissions.RemoveCode(CustomerApplicationPermissionsList.Codes.AuditAPI);
				}
				return permissions;
			}
		}

		public const string AllApplicationsScope = "*";
	}
}
