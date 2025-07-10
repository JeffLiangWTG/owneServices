using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlBindingsCollectionExtensionsTest : TestCase
	{
		public void TestRemoveBinding()
		{
			TextBox textBox = new TextBox();
			textBox.DataBindings.Add(new Binding("Text", new Exception(), "Message"));
			textBox.DataBindings.RemoveBinding("Text");
			AssertNull("Binding removed after ZBinding.RemoveBinding() called", textBox.DataBindings["Text"]);
		}
	}
}
