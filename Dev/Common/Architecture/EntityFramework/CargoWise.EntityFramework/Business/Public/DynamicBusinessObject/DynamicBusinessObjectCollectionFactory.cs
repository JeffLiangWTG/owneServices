namespace CargoWise.EntityFramework
{
	public class DynamicBusinessObjectCollectionFactory : IDynamicBusinessObjectCollectionFactory
	{
		public DynamicBusinessObjectCollection Create(BusinessObjectFactory bizoFactory) => new DynamicBusinessObjectCollection(bizoFactory);
	}
}
