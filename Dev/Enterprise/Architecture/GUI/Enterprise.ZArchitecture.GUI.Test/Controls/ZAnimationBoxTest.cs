using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZAnimationBoxTest : NUnit.Framework.TestCase
	{
		public void TestOperation()
		{
			var box = new ZAnimationBox();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveProgressForm));
			box.Image = ((System.Drawing.Image)(resources.GetObject("Animation.Image")));
			using (var form = new Form())
			{
				form.Controls.Add(box);
				form.Show();
				AssertEquals("Timer started", true, box.timer.Enabled);
				AssertEquals("Double buffered", true, box.DoubleBufferedExposed);
				var frame = box.currentFrame;
				System.Threading.Thread.Sleep(200);
				Application.DoEvents();
				Assert("Frame has changed", frame != box.currentFrame);

				form.Hide();
				System.Threading.Thread.Sleep(200);
				Application.DoEvents();
				frame = box.currentFrame;
				System.Threading.Thread.Sleep(200);
				Application.DoEvents();
				Assert("Frame has not changed", frame != box.currentFrame);
			}
		}
	}
}
