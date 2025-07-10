using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextLabelNoEncodeTest : TestCase
	{
		public void TestShouldNotEncode()
		{
			var label = new ZTextLabelNoEncode();
			label.Text = "<br>hello<br>";
			AssertEquals("<br>hello<br>", label.Text);
			AssertEquals("<br>hello<br>", new ZTextLabelNoEncode("<br>hello<br>").Text);
		}
	}
}
