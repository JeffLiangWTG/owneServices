using System;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.ComponentModel.Testing
{
	sealed class ControlPropertyDescriptorBaseTest : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestShouldThrowExceptionIfCantFindPropertyWithTheGivenName()
		{
			new ControlPropertyDescriptor<TextBox, string>("It's Christmas Time", "");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestShouldThrowExceptionIfCantFindEventWithTheGivenName()
		{
			new ControlPropertyDescriptor<TextBox, string>("Text", "", true, "It's Christmas Time");
		}

		public void TestCanAccessValueFromComponentBothDirections()
		{
			var box = CreateTextBox();

			descriptor.SetValue(box, newValue);

			AssertEquals(newValue, box.Text);
			AssertEquals(newValue, descriptor.GetValue(box));
		}

		public void TestProperlyHandlesEventHandlerAttachment()
		{
			var wasCalled = false;
			EventHandler handler = delegate
			{ wasCalled = true; };

			var box = CreateTextBox();

			descriptor.AddValueChanged(box, handler);
			box.Text = newValue;
			Assert(wasCalled);

			wasCalled = false;

			descriptor.RemoveValueChanged(box, handler);
			box.Text = oldValue;
			Assert(!wasCalled);
		}

		public void TestAttributes()
		{
			AssertNotNull("Attributes can be found", descriptor.Attributes[typeof(LocalizableAttribute)]);
		}

		#region Implementation

		const string oldValue = "Old";
		const string newValue = "New";

		readonly ControlPropertyDescriptor<TextBox, string> descriptor = new ControlPropertyDescriptor<TextBox, string>("Text", "TextChanged");

		static TextBox CreateTextBox()
		{
			var box = new TextBox();
			box.Text = oldValue;
			return box;
		}

		#endregion
	}
}
