using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Unlike the .NET picture box, this control is thread safe when there are message loops on
	/// two different threads in the one application.
	/// </summary>
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ZAnimationBox : UserControl // On different thread, cannot use Z or O
	{
		public ZAnimationBox()
		{
			DoubleBuffered = true;
			InitializeComponent();

			timer.Interval = 100;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

#if DEBUG
		internal bool DoubleBufferedExposed => DoubleBuffered;
#endif

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				timer.Start();
			}
			else
			{
				timer.Stop();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		void timer_Tick(object sender, EventArgs e)
		{
			currentFrame++;
			if (currentFrame >= frameCount)
			{
				currentFrame = 0;
			}
			try
			{
				image.SelectActiveFrame(dimension, currentFrame);
				Invalidate();
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}
#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (image != null && currentFrame < frameCount)
			{
				e.Graphics.DrawImage(Image, 0, 0);
			}
		}

#endif
		public Image Image
		{
			get { return image; }
			set
			{
				image = value;
				if (image != null)
				{
					dimension = new FrameDimension(image.FrameDimensionsList[0]);
					frameCount = image.GetFrameCount(dimension);
#if WINZOR
					BackgroundImage = value;
#endif
				}
				Invalidate();
			}
		}

		Image image;
		internal int currentFrame;
		int frameCount;
		FrameDimension dimension;
	}
}
