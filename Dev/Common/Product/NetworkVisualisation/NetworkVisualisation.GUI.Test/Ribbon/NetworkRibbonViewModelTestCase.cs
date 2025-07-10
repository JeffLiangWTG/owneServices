using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public abstract class NetworkRibbonViewModelTestCase : TransactionedTestCase
	{
#pragma warning disable CS0618
		public void TestRibbonAndContextMenuNetworkActionsShouldMatch()
		{
			ObjectFactory.Get<IBMSRegistry>().NCNRibbonEnabled = true;

			var network = GetNetwork();
			AssertNotNull(network.DiagramEntity);
			AssertNotNull(network.Refresher);

			var ribbonDataProvider = GetRibbonDataProvider();
			using (var control = new NetworkUserControl(network.DiagramEntity, network.Refresher, ribbonDataProvider: ribbonDataProvider))
			{
				control.SetDataContext(network, false);

				var ribbonControl = control.RibbonControlExposed_ForTesting;
				AssertNotNull(ribbonControl);
				var ribbonViewModel = ribbonControl.Model;
				AssertNotNull(ribbonViewModel);
				var ribbonButtons = ribbonViewModel.Tabs.SelectMany(t => t.Groups).SelectMany(g => g.Items);
				var ribbonActions = ribbonButtons.Select(b => b.Action).ToArray();

				var contextMenuActions = control.ViewModel.DiagramNetworkActions_ExposedForTest;
				var contextMenuActionsIncludingReasonableChildren = GetActionsIncludingReasonableChildren(contextMenuActions, GetSignaturesOfContextMenuActionsThatAreContainersForOtherActions());

				var ribbonActionNames = ribbonActions.Select(a => a.ToString()).ToArray();
				var contextMenuActionNames = contextMenuActionsIncludingReasonableChildren.Select(a => a.ToString()).ToArray();

				var actionsMissingInRibbon = contextMenuActionNames.Where(n => !ribbonActionNames.Contains(n) && !GetSignaturesOfActionsPurposelyNotIncludedIntoRibbon().Any(s => n.StartsWith(s))).ToArray();
				Assert($@"The ribbon should have buttons for all actions implemented in the context menu (with defined exceptions), however these actions are missing in the ribbon:
{string.Join(System.Environment.NewLine, actionsMissingInRibbon)}", !actionsMissingInRibbon.Any());

				var actionsMissingInContextMenu = ribbonActionNames.Where(n => !contextMenuActionNames.Contains(n) && !GetSignaturesOfActionsPurposelyNotIncludedIntoContextMenu().Any(s => n.StartsWith(s))).ToArray();
				Assert($@"The context menu should have menu items for all actions implemented in the ribbon (with defined exceptions), however these actions are missing in the context menu:
{string.Join(System.Environment.NewLine, actionsMissingInContextMenu)}", !actionsMissingInContextMenu.Any());
			}
		}
#pragma warning restore CS0618

		IEnumerable<INetworkAction> GetActionsIncludingReasonableChildren(IEnumerable<INetworkAction> parentActions, IEnumerable<string> containerActionSignatures)
		{
			return parentActions.SelectMany(a => containerActionSignatures.Any(s => a.ToString().StartsWith(s))
				? GetActionsIncludingReasonableChildren(a.GetChildActions(), containerActionSignatures)
				: new INetworkAction[] { a });
		}

		protected abstract INetwork GetNetwork();

		protected abstract IRibbonDataProvider GetRibbonDataProvider();

		protected virtual IEnumerable<string> GetSignaturesOfActionsPurposelyNotIncludedIntoRibbon()
		{
			yield return nameof(SearchAction);
			yield return "StaticNetworkAction: Text Color";
		}

		protected virtual IEnumerable<string> GetSignaturesOfActionsPurposelyNotIncludedIntoContextMenu()
		{
			yield break;
		}

		protected virtual IEnumerable<string> GetSignaturesOfContextMenuActionsThatAreContainersForOtherActions()
		{
			yield return nameof(AffinitiesAction);
			yield return nameof(ShowAction);
			yield return nameof(ImportAction);
			yield return nameof(CreateNewAction);
		}
	}
}
