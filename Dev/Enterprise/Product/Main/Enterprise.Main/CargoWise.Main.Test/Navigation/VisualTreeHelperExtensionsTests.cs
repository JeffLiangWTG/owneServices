#if !WINZOR
using System.Linq;
using System.Windows.Controls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation.Tests;

sealed class VisualTreeHelperExtensionsTests : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestFindVisualChildren_FindsAllChildrenOfType()
	{
		// Arrange
		var parent = new Grid();
		var child1 = new Button();
		var child2 = new TextBox();
		var child3 = new Button();

		parent.Children.Add(child1);
		parent.Children.Add(child2);
		parent.Children.Add(child3);

		// Act
		var buttons = parent.FindVisualChildren<Button>().ToList();

		// Assert
		AssertEquals(2, buttons.Count);
	}

	[RequiresSTA]
	public void TestFindVisualChildren_ReturnsEmpty_WhenNoChildrenMatchType()
	{
		// Arrange
		var parent = new Grid();
		var child1 = new TextBox();
		var child2 = new TextBox();

		parent.Children.Add(child1);
		parent.Children.Add(child2);

		// Act
		var buttons = parent.FindVisualChildren<Button>().ToList();

		// Assert
		AssertEquals(0, buttons.Count);
	}

	[RequiresSTA]
	public void TestFindLogicalChild_FindsFirstChildOfType()
	{
		// Arrange
		var parent = new StackPanel();
		var child1 = new Button();
		var child2 = new TextBox();

		parent.Children.Add(child1);
		parent.Children.Add(child2);

		// Act
		var result = parent.FindLogicalChild<Button>();

		// Assert
		AssertSame(child1, result);
	}

	[RequiresSTA]
	public void TestFindLogicalChild_ReturnsNull_WhenNoChildMatchesType()
	{
		// Arrange
		var parent = new StackPanel();
		var child1 = new TextBox();
		var child2 = new TextBox();

		parent.Children.Add(child1);
		parent.Children.Add(child2);

		// Act
		var result = parent.FindLogicalChild<Button>();

		// Assert
		AssertNull(result);
	}

	[RequiresSTA]
	public void TestFindLogicalChild_FindsChildInNestedStructure()
	{
		// Arrange
		var parent = new StackPanel();
		var childContainer = new StackPanel();
		var targetChild = new Button();

		childContainer.Children.Add(targetChild);
		parent.Children.Add(childContainer);

		// Act
		var result = parent.FindLogicalChild<Button>();

		// Assert
		AssertSame(targetChild, result);
	}
}
#endif
