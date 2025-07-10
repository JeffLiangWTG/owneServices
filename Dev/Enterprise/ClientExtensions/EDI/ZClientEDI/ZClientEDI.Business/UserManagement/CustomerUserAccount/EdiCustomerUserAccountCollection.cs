using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiCustomerUserAccountCollection : BusinessObjectCollection<EdiCustomerUserAccount>
	{
		public EdiCustomerUserAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiCustomerUserAccountCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery();
			query.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			return query;
		}
	}
}

