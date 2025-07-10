using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembershipFlattenedCollection : NonPersistentBusinessObjectCollection<EdiOrgMembershipFlattened>
	{
		public EdiOrgMembershipFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EdiOrgMembershipFlattened();
		}
	}
}

