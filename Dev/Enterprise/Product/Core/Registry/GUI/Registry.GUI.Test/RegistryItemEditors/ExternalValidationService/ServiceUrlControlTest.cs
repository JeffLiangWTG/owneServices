using System;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ServiceUrlControlTest : NUnit.Framework.TestCase
	{
		public void TestValue()
		{
			using (ServiceUrlControl control = new ServiceUrlControl())
			{
				AssertEquals("Default empty text value", String.Empty, control.textBoxUrl.Text);
				control.Value = "http://www.google.com.au/";
				AssertEquals("Set value", "http://www.google.com.au/", control.textBoxUrl.Text);
				AssertEquals("Get value", "http://www.google.com.au/", control.Value);
			}
		}
	}
}
