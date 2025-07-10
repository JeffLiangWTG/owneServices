using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using NUnit.Framework;

namespace CargoWise.GUI.TileBar.Testing
{
	[TestedType(typeof(RecentItemsControl))]
	[SuppressDataContextCheck]
	class RecentItemsControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new RecentItemsControl();
		}
	}
}
