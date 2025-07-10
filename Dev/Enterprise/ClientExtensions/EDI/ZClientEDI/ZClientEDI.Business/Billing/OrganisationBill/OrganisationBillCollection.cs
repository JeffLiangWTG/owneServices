using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class OrganisationBillCollection : NonPersistentBusinessObjectCollection<OrganisationBill>
	{
		public OrganisationBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrganisationBill(Factory, Env.CurrentBranch.PK, Factory.New<OrgHeader>().PK, "", ZDateTime.Now);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

