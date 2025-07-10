using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ConnectorItem))]
	[SuppressDataContextCheck]
	class ConnectorItemBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new ConnectorItem();
		}
	}
}
