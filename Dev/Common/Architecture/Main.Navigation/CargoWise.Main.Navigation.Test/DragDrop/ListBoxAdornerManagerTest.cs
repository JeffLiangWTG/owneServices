using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class ListBoxAdornerManagerTest : TestCase
{
	[RequiresSTA]
	public void TestUpdate()
	{
		var window = new Window();
		var element = new TextBox();
		window.Content = element;
		window.Show();
		var adornerManager = new ListBoxAdornerManager(AdornerLayer.GetAdornerLayer(element), new SolidColorBrush());
		AssertNull("Adorner is null", adornerManager.adorner);
		adornerManager.Update(element, true);
		AssertNotNull("Adorner is not null", adornerManager.adorner);
		window.Close();
	}
}
