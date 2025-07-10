using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class VerticalLabelTest : TestCase
	{
		public void TestVerticalLabelCanInstantiate()
		{
			using (KForm form = new KForm())
			{
				VerticalLabelForTesting testLabel1 = new VerticalLabelForTesting();
				testLabel1.Name = "testLabel1";
				testLabel1.Text = "BottomUp - 1";
				testLabel1.TextDrawMode = VerticalLabelDrawMode.BottomUp;
				form.Controls.Add(testLabel1);

				VerticalLabelForTesting testLabel2 = new VerticalLabelForTesting();
				testLabel2.Name = "testLabel2";
				testLabel2.Text = "TopBottom - 2";
				testLabel2.TextDrawMode = VerticalLabelDrawMode.TopBottom;
				form.Controls.Add(testLabel2);

				AssertEquals("Precondition: testLabel1.PaintCalled", 0, testLabel1.PaintCalled);
				AssertEquals("Precondition: testLabel2.PaintCalled", 0, testLabel2.PaintCalled);
				//form.Show();
				//Application.DoEvents();
				ForcePaintEvenWhenComputerLocked(testLabel1);
				ForcePaintEvenWhenComputerLocked(testLabel2);
				AssertEquals("Precondition: testLabel1.PaintCalled", 1, testLabel1.PaintCalled);
				AssertEquals("Precondition: testLabel2.PaintCalled", 1, testLabel2.PaintCalled);
			}
		}

		static void ForcePaintEvenWhenComputerLocked(Control control)
		{
#if !WINZOR
			control.DrawToBitmap(new Bitmap(control.Width, control.Height), control.Bounds);
#endif
		}

		class VerticalLabelForTesting : VerticalLabel
		{
			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);
				PaintCalled++;
			}
			public int PaintCalled;
		}
	}
}
