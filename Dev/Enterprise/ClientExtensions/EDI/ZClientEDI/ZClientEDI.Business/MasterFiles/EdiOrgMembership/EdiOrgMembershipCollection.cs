using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembershipCollection : ActiveBusinessObjectCollection<EdiOrgMembership>
	{
		public EdiOrgMembershipCollection(OrgHeader master)
			: base(master.Factory, master, new ZQuery(), EdiOrgMembershipSchema.EOR_OH)
		{
			Master = master;
		}

		readonly OrgHeader Master;

		protected EdiOrgMembershipCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static EdiOrgMembershipCollection CreateAdhocCollection(BusinessObjectFactory factory)
		{
			return new EdiOrgMembershipCollection(factory, new AdhocCollectionRelationship(typeof(EdiOrgMembership)));
		}

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(EdiOrgMembership newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (Master != null)
			{
				newElement.EOR_OH = Master.PK;
			}
		}

		#endregion
	}
}

