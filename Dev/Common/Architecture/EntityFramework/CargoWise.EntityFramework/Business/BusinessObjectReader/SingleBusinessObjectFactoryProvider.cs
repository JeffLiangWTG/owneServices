namespace CargoWise.EntityFramework
{
	public class SingleBusinessObjectFactoryProvider : BusinessObjectFactoryProvider
	{
		public SingleBusinessObjectFactoryProvider(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObjectFactory CreateNew(bool reclaimMemory)
		{
			return Current;
		}
	}
}
