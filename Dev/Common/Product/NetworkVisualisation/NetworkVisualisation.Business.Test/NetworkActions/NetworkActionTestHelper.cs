using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public static class NetworkActionTestHelper
	{
		public static void MakeActionsRefreshOnSelectionChanged(INetworkViewModel viewModel, params INetworkAction[] actions)
		{
			viewModel.SelectionChanged += (sender, args) =>
			{
				foreach (var action in actions)
				{
					action.Refresh(args);
				}
			};
		}
	}
}
