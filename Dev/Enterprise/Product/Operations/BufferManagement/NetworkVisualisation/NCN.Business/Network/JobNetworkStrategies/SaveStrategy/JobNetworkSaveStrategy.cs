namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class JobNetworkSaveStrategy
	{
		public JobNetworkSaveStrategy(JobNetwork network)
		{
			Network = network;
		}

		protected JobNetwork Network { get; }

		public void OnBeforeSave() => OnBeforeSaveCore();

		protected virtual void OnBeforeSaveCore()
		{
		}
	}
}
