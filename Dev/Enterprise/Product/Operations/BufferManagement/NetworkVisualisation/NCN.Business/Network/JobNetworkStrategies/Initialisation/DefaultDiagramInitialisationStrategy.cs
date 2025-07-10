namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DefaultDiagramInitialisationStrategy : InitialisationStrategyBase
	{
		internal DefaultDiagramInitialisationStrategy(BMNetworkViewModel viewModel)
		{
			this.viewModel = viewModel;
		}

		readonly BMNetworkViewModel viewModel;

		internal override void InitialiseDiagram()
		{
			var defaultDiagram = (BMNCNShapeDefaultDiagram)viewModel.DiagramShape;
			defaultDiagram.SetupDefaultShapeNetworkForJobHeader();
		}
	}
}
