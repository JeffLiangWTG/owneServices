#if !WINZOR
using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.Main.Startup.Login;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.Login
{
	[TestedType(typeof(NextLoginUserControl))]
	public class NextLoginUserControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var viewModel = new NextLoginViewModel(new LoginService());
			var control = new NextLoginUserControl(viewModel);
			return control;
		}
	}
}
#endif
