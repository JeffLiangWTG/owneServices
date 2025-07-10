using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ShowHiddenDependencyAction : DynamicNetworkAction
	{
		public ShowHiddenDependencyAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("a33f1e22-add9-4eb3-9750-46fd273b4102", "Dependency");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("44fc758f-8d0e-4b6f-a116-c2641104376b", "Show an existing dependency link on this diagram");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Link";

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			return GetHiddenDependencyActions(GetNetworkViewModel(), GetNode(activeEntity));
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).HiddenDependencies.Any(),
				entity,
				() => Res.GetString("11AB92A3-CF72-4F8F-B435-3918E2D20B75", "There should be at least one hidden dependency."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion

		#region Implementation

		static IEnumerable<INetworkAction> GetHiddenDependencyActions(NetworkViewModel networkViewModel, NodeViewModel node)
		{
			if (node.HiddenDependencies.Any())
			{
				yield return new StaticNetworkAction(() =>
				{
					foreach (var hiddenDependency in node.HiddenDependencies.ToList())
					{
						networkViewModel.ShowConnection(hiddenDependency);
					}
				}, name: ResString.GetMultilingualString("BA09A926-17D5-4F9F-BEF1-37F4298D4CB8", "All"), group: 0);
			}

			foreach (var hiddenDependency in node.HiddenDependencies)
			{
				yield return new StaticNetworkAction(() => networkViewModel.ShowConnection(hiddenDependency),
					name: ResString.GetMultilingualString("535A6219-1072-4C2D-A6D1-2020ED748319", "{0}", hiddenDependency.GetDisplayName()), group: 1
					);
			}
		}

		#endregion
	}
}
