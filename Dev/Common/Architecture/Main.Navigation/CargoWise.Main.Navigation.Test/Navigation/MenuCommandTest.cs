using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

class MenuCommandTest : TestCase
{
	public void TestCanExecute()
	{
		var canExecute = false;
		var uut = new MenuCommand(() => { }, () => canExecute);

		AssertEquals(expected: false, uut.CanExecute(null));

		canExecute = true;

		AssertEquals(expected: true, uut.CanExecute(null));
	}

	public void TestExecute_WhenCanExecuteIsNull()
	{
		var count = 0;
		var uut = new MenuCommand(() => count++);

		uut.Execute(null);

		AssertEquals(expected: 1, count);

		uut.Execute(null);

		AssertEquals(expected: 2, count);
	}

	public void TestExecute_WhenCanExecuteReturnsTrue()
	{
		var count = 0;
		var uut = new MenuCommand(() => count++, () => true);

		uut.Execute(null);

		AssertEquals(expected: 1, count);

		uut.Execute(null);

		AssertEquals(expected: 2, count);
	}

	public void TestExecute_WhenCanExecuteReturnsFalse()
	{
		var count = 0;
		var uut = new MenuCommand(() => count++, () => false);

		uut.Execute(null);

		AssertEquals(expected: 0, count);

		uut.Execute(null);

		AssertEquals(expected: 0, count);
	}
}
