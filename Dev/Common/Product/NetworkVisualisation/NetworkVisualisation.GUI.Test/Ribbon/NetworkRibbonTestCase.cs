using System.Linq;
using CargoWise.Application;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public abstract class NetworkRibbonTestCase : TransactionedTestCase
	{
		public void TestDeletingShapeShouldNotCauseExceptions()
		{
			var network = GetNetworkWithEntities();
			AssertNotNull(network.DiagramEntity);
			AssertNotNull(network.Refresher);

			var control = BuildNetworkUserControlWithRibbon(network);
			var networkViewModel = control.NetworkViewModel;

			var nodes = networkViewModel.Nodes.Except(new NodeViewModel[] { networkViewModel.DiagramNodeViewModel }).ToArray();
			Assert("The network should contain at least one shape", nodes.Length >= 1);
			var node = nodes[0];

			networkViewModel.SelectEntities(new INetworkEntity[] { node.Entity });

			AssertNoExceptionThrown(() => networkViewModel.RemoveFromDiagram(node));
		}

		public void TestDeletingMultipleShapesShouldNotCauseExceptions()
		{
			var network = GetNetworkWithEntities();
			AssertNotNull(network.DiagramEntity);
			AssertNotNull(network.Refresher);

			var control = BuildNetworkUserControlWithRibbon(network);
			var networkViewModel = control.NetworkViewModel;

			var nodes = networkViewModel.Nodes.Except(new NodeViewModel[] { networkViewModel.DiagramNodeViewModel }).ToArray();
			Assert("The network should contain at least two shapes", nodes.Length >= 2);
			var node1 = nodes[0];
			var node2 = nodes[1];

			networkViewModel.SelectEntities(new INetworkEntity[] { node1.Entity, node2.Entity });

			AssertNoExceptionThrown(() => new RemoveFromDiagramAction(networkViewModel).Execute());
		}

#pragma warning disable CS0618
		NetworkUserControl BuildNetworkUserControlWithRibbon(INetwork network)
		{
			var ribbonDataProvider = GetRibbonDataProvider();
			using (var control = new NetworkUserControl(network.DiagramEntity, network.Refresher, ribbonDataProvider: ribbonDataProvider))
			{
				control.SetDataContext(network, false);
				var ribbonControl = control.RibbonControlExposed_ForTesting;
				AssertNotNull(ribbonControl);
				var ribbonViewModel = ribbonControl.Model;
				AssertNotNull(ribbonViewModel);
				return control;
			}
		}
#pragma warning restore CS0618

		protected abstract INetwork GetNetworkWithEntities();

		protected abstract IRibbonDataProvider GetRibbonDataProvider();

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.Get<IBMSRegistry>().NCNRibbonEnabled = true;
		}
	}
}
