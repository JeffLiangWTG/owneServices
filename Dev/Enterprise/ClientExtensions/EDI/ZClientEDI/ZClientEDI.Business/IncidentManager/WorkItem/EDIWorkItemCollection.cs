using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIWorkItemCollection : WorkItemCollection
	{
		public EDIWorkItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EDIWorkItemCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public EDIWorkItemCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new EDIWorkItemCollectionFetchStrategy(this);
		}
	}
}

