using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLabelCaptionCacheTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCaptions_WhenControlInHierarchyDisposed()
		{
#pragma warning disable CW1105 // Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class
			var containerControl = new UserControl();
#pragma warning restore CW1105 // Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class
			var textBox = new TextBox();

			containerControl.Controls.Add(textBox);
			containerControl.Dispose();
			LabelCaptionCache.GetCaptions(textBox);
		}

		public void TestControlCaptionResourceString()
		{
			using (var form1 = new Form1())
			{
				form1.Control1.CaptionResourceString = Res.GetData("E242CF56-B701-422C-9BBE-2D713EE9D570", "Explicit Caption");
				AssertEquals("Explicit Caption", LabelCaptionCache.GetCaptions(form1.Control1)[0]);
			}
		}

		#region Test Classes

		class TestLabelCaptionCache : ZLabelCaptionCache
		{
		}

		#endregion

		#region Implementation

		TestLabelCaptionCache LabelCaptionCache
		{
			get { return labelCaptionCache ?? (labelCaptionCache = new TestLabelCaptionCache()); }
		}
		TestLabelCaptionCache labelCaptionCache;

		class BaseForm : ZForm
		{
			public BaseForm()
			{
				Name = "Form1";
			}
		}

		class Form1 : BaseForm
		{
			public Form1()
			{
				Text = "Form 1";

				Control1 = new ZLabel();
				Control1.Name = "Control1";
				Control1.Text = "Caption 1";
				Controls.Add(Control1);
			}

			public ZLabel Control1;
		}

		#endregion
	}
}
