namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class ScaledDiagramUpdateEntitiesStrategy : JobNetworkUpdateEntitiesStrategy
	{
		internal ScaledDiagramUpdateEntitiesStrategy(JobNetwork jobNetwork)
			: base(jobNetwork)
		{
		}

		internal override void OnEntityEdited()
		{
			jobNetwork.RefreshSchedules();
		}
	}
}
