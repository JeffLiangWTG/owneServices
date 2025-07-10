using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class RuntimeOptionUserControlTest : TestCase
	{
		public void TestDesiredCaptionWidthException()
		{
			using (var control = new RuntimeOptionUserControl())
			{
				control.Controls.Add(new TextBox());
				control.Controls.Add(new TextBox());
				control.Controls.Add(new TextBox());

				AssertExceptionThrown(typeof(FormatException), () => { var ex = control.DesiredCaptionWidth; });
			}
		}
	}
}
