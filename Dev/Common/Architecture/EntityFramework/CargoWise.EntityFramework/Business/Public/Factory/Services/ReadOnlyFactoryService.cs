namespace CargoWise.EntityFramework
{
	class ReadOnlyFactoryService : IService
	{
		public ReadOnlyFactoryService()
		{
			ResetReadOnlyFactory();
		}

		public ReadOnlyBusinessObjectFactory ReadOnlyFactory { get; private set; }

		public void ResetReadOnlyFactory()
		{
			ReadOnlyFactory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = "MasterFiles ReadOnlyFactory" };
		}
	}
}
