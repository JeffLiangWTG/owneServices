namespace CargoWise.EntityFramework
{
	public interface IDynamicBusinessObjectCollectionFactory
	{
		DynamicBusinessObjectCollection Create(BusinessObjectFactory bizoFactory);
	}
}
