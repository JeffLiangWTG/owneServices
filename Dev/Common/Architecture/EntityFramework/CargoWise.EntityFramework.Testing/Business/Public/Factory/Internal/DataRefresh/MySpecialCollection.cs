namespace CargoWise.EntityFramework.Testing
{
	sealed class MySpecialCollection : BusinessObjectCollection<DummyBusinessObject>
	{
		public MySpecialCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
