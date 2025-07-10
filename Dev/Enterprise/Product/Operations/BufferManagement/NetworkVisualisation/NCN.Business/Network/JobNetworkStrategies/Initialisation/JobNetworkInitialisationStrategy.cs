using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class JobNetworkInitialisationStrategy : InitialisationStrategyBase
	{
		internal JobNetworkInitialisationStrategy(BMNetworkViewModel viewModel)
		{
			this.viewModel = viewModel;
		}

		readonly BMNetworkViewModel viewModel;

		internal override void InitialiseDiagram()
		{
			var processHeader = viewModel.DiagramShape.ProcessHeader;
			var parent = processHeader != null ? processHeader.Parent as BusinessObject : null;

			if (parent != null)
			{
				viewModel.DiagramShape.RegisterEditableChildObject(parent);
			}
		}
	}
}
