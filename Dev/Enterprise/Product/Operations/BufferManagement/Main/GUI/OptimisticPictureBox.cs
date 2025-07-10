using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.BufferManagement.GUI
{
	public class OptimisticPictureBox : KPictureBox
	{
		public OptimisticPictureBox(bool shouldDisposeImageOnControlDispose)
		{
			this.shouldDisposeImageOnControlDispose = shouldDisposeImageOnControlDispose;
		}

		readonly bool shouldDisposeImageOnControlDispose;

		protected override void OnPaint(PaintEventArgs pe)
		{
			try
			{
				var keepPainting = 2;
				while (keepPainting > 0)
				{
					try
					{
						base.OnPaint(pe);
						keepPainting = 0;
					}
					catch (ArgumentException)
					{
						keepPainting--;
						Thread.Sleep(50);
					}
				}
			}
			catch (InvalidOperationException)
			{
				// The picture will not render, but the application will not crash.
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (shouldDisposeImageOnControlDispose)
			{
				Image?.Dispose();
				BackgroundImage?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
