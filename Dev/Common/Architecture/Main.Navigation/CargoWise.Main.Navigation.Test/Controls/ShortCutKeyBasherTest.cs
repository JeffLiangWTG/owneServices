using System.Windows.Controls;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.WPF.Test
{
	[TestedType(typeof(ShortcutKeyControl))]
	[SuppressDataContextCheck]
	sealed class ShortCutKeyBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new ShortcutKeyControl();
		}
	}
}
