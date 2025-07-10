using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NetworkRibbonControl))]
	public class BMRibbonBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			AssertNotNull(((ShapeNetworkEntity)networkViewModel.ActiveEntity).AsShape());
			var ribbonViewModel = new BMRibbonViewModel(networkViewModel, control: null);
			return new NetworkRibbonControl(ribbonViewModel);
		}
	}
}
