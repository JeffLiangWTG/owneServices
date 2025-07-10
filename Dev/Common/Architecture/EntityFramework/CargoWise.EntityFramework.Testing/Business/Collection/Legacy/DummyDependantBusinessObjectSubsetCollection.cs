namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyDependantBusinessObjectSubsetCollection : SubsetBusinessObjectCollection<DummyDependantBusinessObject>
	{
		public DummyDependantBusinessObjectSubsetCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}
	}
}
