using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class PrimaryOrgSelectorCollection : NonPersistentBusinessObjectCollection<PrimaryOrgSelector>
	{
		public PrimaryOrgSelectorCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PrimaryOrgSelector FindByOrgCode(ZString code)
		{
			foreach (PrimaryOrgSelector primaryOrg in this)
			{
				if (primaryOrg.OrganisationCode == code)
				{
					return primaryOrg;
				}
			}
			return null;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PrimaryOrgSelector(Factory, null);
		}

		#endregion
	}
}
