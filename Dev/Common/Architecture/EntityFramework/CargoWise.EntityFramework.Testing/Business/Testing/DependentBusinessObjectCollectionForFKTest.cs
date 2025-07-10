namespace CargoWise.EntityFramework.Testing
{
	sealed class DependentBusinessObjectCollectionForFKTest : DependentBusinessObjectCollection<DummyDependantBusinessObject, DummyWithDependentsBusinessObject>
	{
		public DependentBusinessObjectCollectionForFKTest(DummyWithDependentsBusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}
	}
}
