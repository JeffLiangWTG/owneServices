using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class SingleEntityExecutionStrategy : CommonNetworkActionExecutionStrategy
	{
		protected override INetworkActionAccessibility IsActionApplicableCore(IDynamicNetworkAction action)
		{
			return new NetworkActionAccessibility(action.NetworkViewModel.SelectedEntities.Count() <= 1,
					action.NetworkViewModel.Network.DiagramEntity,
					() => Res.GetString("177E9B38-CA8C-4EF4-87A1-39B59EA16301", "This action is applicable to a single entity only."))
				.UnionIfAllowed(() => base.IsActionApplicableCore(action));
		}
	}
}
