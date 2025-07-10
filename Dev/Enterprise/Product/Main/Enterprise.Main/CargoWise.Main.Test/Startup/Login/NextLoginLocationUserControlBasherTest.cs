#if !WINZOR
using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.Main.Startup.Login;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.Login
{
	[TestedType(typeof(NextLoginLocationUserControl))]
	public class NextLoginLocationUserControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var viewModel = new NextLoginLocationViewModel(new LoginService());
			var control = new NextLoginLocationUserControl(viewModel);
			return control;
		}
	}
}
#endif
