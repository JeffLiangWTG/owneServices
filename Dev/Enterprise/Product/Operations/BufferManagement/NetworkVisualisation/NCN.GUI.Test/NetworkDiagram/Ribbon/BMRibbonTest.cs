using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class BMRibbonTest : NetworkRibbonTestCase
	{
		protected override INetwork GetNetworkWithEntities()
		{
			var diagram = NetworkTestCase.CreateDiagram(new BusinessObjectFactory());
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			networkViewModel.CreateNewShape(diagram);
			networkViewModel.CreateNewShape(diagram);
			return networkViewModel.Network;
		}

		protected override IRibbonDataProvider GetRibbonDataProvider()
		{
			return new BMRibbonViewModelProvider();
		}
	}
}
