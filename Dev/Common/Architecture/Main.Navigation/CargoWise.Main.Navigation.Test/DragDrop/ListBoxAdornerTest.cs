using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class ListBoxAdornerTest : TestCase
{
	[RequiresSTA]
	public void TestVisibility()
	{
		var window = new Window();
		var element = new TextBox();
		window.Content = element;
		window.Show();
		var adorner = new ListBoxAdorner(element, AdornerLayer.GetAdornerLayer(element), new SolidColorBrush());
		adorner.Remove();
		AssertEquals("Adorner is collapsed", Visibility.Collapsed, adorner.Visibility);
		adorner.Update();
		AssertEquals("Adorner is visible", Visibility.Visible, adorner.Visibility);
		window.Close();
	}
}
