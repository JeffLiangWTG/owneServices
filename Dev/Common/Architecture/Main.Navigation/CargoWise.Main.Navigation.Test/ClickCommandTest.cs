using NUnit.Framework;

namespace CargoWise.Main.Navigation.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class ClickCommandTest : TestCase
{
	public void TestClickCommand()
	{
		var clicked = false;
		var command = new ClickCommand(() => clicked = true);
		Assert("Not clicked", !clicked);
		Assert("Can execute", command.CanExecute(null));
		command.Execute(null);
		Assert("Clicked", clicked);
		command.Clear();
		Assert("Cannot execute", !command.CanExecute(null));
	}
}
