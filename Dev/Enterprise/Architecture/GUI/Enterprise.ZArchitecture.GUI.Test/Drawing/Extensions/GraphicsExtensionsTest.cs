using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Drawing.Testing
{
	sealed class GraphicsExtensionsTest : TestCase
	{
		public void TestGetDrawableStringByTrimmingEnd()
		{
			using (var textbox = new TextBox())
			using (var graphics = textbox.CreateGraphics())
			using (var font = new Font(FontFamily.GenericSerif, 12f))
			{
				var text1 = new string('a', 40000);
				var expectedText1 = new string('a', 32000);
				AssertMultilineASCIIEquals("Should be trimmed", expectedText1, graphics.GetDrawableStringByTrimmingEnd(text1));
				AssertNoExceptionThrown(() => graphics.DrawString(expectedText1, font, Brushes.Black, 0, 0));

				var text2 = new string('b', 30000) + '\r' + new string('a', 30000);
				var expectedText2 = new string('b', 30000) + '\r' + new string('a', 1999);
				AssertMultilineASCIIEquals("Should be trimmed", expectedText2, graphics.GetDrawableStringByTrimmingEnd(text2));
				AssertNoExceptionThrown(() => graphics.DrawString(expectedText2, font, Brushes.Black, 0, 0));
			}
		}
	}
}
