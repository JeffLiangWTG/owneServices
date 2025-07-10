using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI
{
	sealed class FlexCelPreviewWithCultureTest : NUnit.Framework.TestCase
	{
		public void TestOnPaintHandlesFlexCelCreatingImageException()
		{
			using (FlexCelPreviewWithCulture preview = new FlexCelPreviewWithCulture())
			{
				preview.GenerateFlexCelImageException = true;
				preview.InternalOnPaint(new PaintEventArgs(preview.CreateGraphics(), new System.Drawing.Rectangle()));
				AssertEquals(@$"FlexCelCoreException caught when loading the Preview Control - 

Your system might be low on memory or system resources, please close all other tasks, exit {BrandingFactory.Instance.ProductName}, come back in and try again.

Error message is: Error in Flexcel when creating an image", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOnPaintHandlesFlexCelCreatingFontException()
		{
			using (FlexCelPreviewWithCulture preview = new FlexCelPreviewWithCulture())
			{
				preview.GenerateFlexCelFontException = true;
				preview.InternalOnPaint(new PaintEventArgs(preview.CreateGraphics(), new System.Drawing.Rectangle()));
				AssertEquals(@"FlexCelCoreException caught when loading the Preview Control - 

Your system may have some problems with a font needed for rendering the document, please fix or uninstall this font before trying to run this document again.

Error message is: Error in Flexcel when creating an font", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOnPaint()
		{
			using (FlexCelPreviewWithCulture preview = new FlexCelPreviewWithCulture())
			{
				Culture.CultureChanged += new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
				try
				{
					preview.InternalOnPaint(new PaintEventArgs(preview.CreateGraphics(), new System.Drawing.Rectangle()));
					AssertEquals(0, CultureChangedCount);

					preview.IsLocalDocument = true;
					preview.InternalOnPaint(new PaintEventArgs(preview.CreateGraphics(), new System.Drawing.Rectangle()));
					AssertEquals(2, CultureChangedCount);
				}
				finally
				{
					Culture.CultureChanged -= new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
				}
			}
		}

		void Culture_CultureChanged(object sender, Culture.CultureChangedEventArgs e)
		{
			CultureChangedCount++;
		}
		int CultureChangedCount;
	}
}
