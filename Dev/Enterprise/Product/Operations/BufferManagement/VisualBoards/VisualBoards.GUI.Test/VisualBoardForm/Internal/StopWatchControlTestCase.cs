using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	public class StopWatchControlTestCase : VisualBoardsTestCase
	{
		[TestDate(2010, 12, 16, 13, 31, 0)]
		public void TestActions()
		{
			using (var form = new Form())
			using (var control = new StopWatchControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(TimeSpan.Zero, control.GetElapsedTime());
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)control.ButtonBackgroundImage_ForTest);

				control.StartOrResume();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 13, 33, 30);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)control.ButtonBackgroundImage_ForTest);

				TestDateAttribute.Date = new DateTime(2010, 12, 16, 14, 0, 0);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals(TimeSpan.FromMinutes(29), control.GetElapsedTime());

				control.Pause();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 15, 0, 0);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Paused, so no movement in time", TimeSpan.FromMinutes(29), control.GetElapsedTime());

				control.StartOrResume();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 17, 0, 0);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Resumed, so time count resumes", TimeSpan.FromMinutes(149), control.GetElapsedTime());

				control.Reset();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 17, 35, 0);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Reset, so time count resets to zero", TimeSpan.Zero, control.GetElapsedTime());

				control.StartOrResume();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 17, 38, 30);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Started, time count starts from zero after reset", TimeSpan.FromMinutes(3.5), control.GetElapsedTime());

				control.Pause();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 17, 48, 30);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Paused, so no movement in time", TimeSpan.FromMinutes(3.5), control.GetElapsedTime());

				control.StartOrResume();
				TestDateAttribute.Date = new DateTime(2010, 12, 16, 17, 49, 0);
				control.Timer_Tick(null, EventArgs.Empty);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)control.ButtonBackgroundImage_ForTest);
				AssertEquals("Resumed, so time count resumes", TimeSpan.FromMinutes(4), control.GetElapsedTime());
			}
		}
	}
}
