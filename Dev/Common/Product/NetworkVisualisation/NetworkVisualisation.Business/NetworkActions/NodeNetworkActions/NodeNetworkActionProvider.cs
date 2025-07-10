using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class NodeNetworkActionProvider
	{
		public static IEnumerable<INetworkAction> GetNetworkActions(NetworkViewModel networkViewModel)
		{
			var baseActions = GetBaseActions(networkViewModel);
			var actionsToIntersperse = networkViewModel.Network.GetCustomNetworkActions(networkViewModel);
			return baseActions.Union(actionsToIntersperse);
		}

		static IEnumerable<INetworkAction> GetBaseActions(NetworkViewModel networkViewModel)
		{
			var group = 10;
			yield return new CreateNewAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new ShowAction(networkViewModel, group);
			yield return new ImportAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group = 20;
			yield return new AffinitiesAction(networkViewModel, group, 10);
			yield return new EditPropertiesAction(networkViewModel, group, 20, shouldUpdateOnNetworkEvents: false);

			group = 30;
			yield return new RemoveAndDeleteAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new RemoveFromDiagramAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group = 40;
			yield return new BringToFrontAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new SendToBackAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
		}
	}
}
