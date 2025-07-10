using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class OrganizationSubBalanceCollection : NonPersistentBusinessObjectCollection<OrganizationSubBalance>	{
		public OrganizationSubBalanceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrganizationSubBalance();
		}

		/// <summary>
		/// Returns the SubBalance object in this collection for the given organization.
		/// If no SubBalance object exists for this org, returns null
		/// </summary>
		public OrganizationSubBalance GetOrganizationSubBalance(ZGuid organization)
		{
			foreach (OrganizationSubBalance subBal in this)
			{
				if (subBal.Organization == organization)
				{
					return subBal;
				}
			}
			return null;
		}
	}
}
