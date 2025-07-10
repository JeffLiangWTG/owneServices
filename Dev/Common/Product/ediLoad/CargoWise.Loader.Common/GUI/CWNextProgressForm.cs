using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.IO;

namespace CargoWise.Loader.Common
{
	public partial class CWNextProgressForm : Form, IProcessStatus
	{
		float rotationAngle;

		public static CWNextProgressForm New()
		{
#if DEBUG
			if (fFormToReturn != null)
			{
				return fFormToReturn;
			}
			else
#endif
			{
				return new CWNextProgressForm();
			}
		}

#if DEBUG

		public static IDisposable OverrideFactoryForTest(CWNextProgressForm formToReturn)
		{
			return new OverrideResetter(formToReturn);
		}

		static CWNextProgressForm fFormToReturn;

		class OverrideResetter : IDisposable
		{
			public OverrideResetter(CWNextProgressForm formToReturn)
			{
				fFormToReturn = formToReturn;
			}

			public void Dispose()
			{
				fFormToReturn = null;
			}
		}

#endif

		public CWNextProgressForm()
		{
			InitializeComponent();
			FormBorderStyle = FormBorderStyle.None;
			UpdateBranding();
			AnimationTimer.Interval = 10;
			AnimationTimer.Tick += AnimationTimer_Tick;
			AnimationTimer.Start();
		}

		void AnimationTimer_Tick(object sender, EventArgs e)
		{
			rotationAngle = (rotationAngle + 4f) % 360;
			RotateInProgressImage(rotationAngle);
		}

		void RotateInProgressImage(float angle)
		{
			Image originalImage = Properties.Resources.progress;

			var inProgressBitmap = new Bitmap(originalImage);
			inProgressBitmap.SetResolution(DeviceDpi, DeviceDpi);

			var diagonalLength = (int)Math.Ceiling(Math.Sqrt(Math.Pow(inProgressBitmap.Width, 2) + Math.Pow(inProgressBitmap.Height, 2)));

			var temporaryRotatedBitmap = new Bitmap(diagonalLength, diagonalLength);
			temporaryRotatedBitmap.SetResolution(DeviceDpi, DeviceDpi);

			using (var g = Graphics.FromImage(temporaryRotatedBitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.Clear(Color.Transparent);

				g.TranslateTransform(diagonalLength / 2f, diagonalLength / 2f);
				g.RotateTransform(angle);
				g.DrawImage(inProgressBitmap, -inProgressBitmap.Width / 2f, -inProgressBitmap.Height / 2f);
			}

			var finalRotatedBitmap = new Bitmap(inProgressBitmap.Width, inProgressBitmap.Height);
			finalRotatedBitmap.SetResolution(DeviceDpi, DeviceDpi);

			using (var g = Graphics.FromImage(finalRotatedBitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var offset = (diagonalLength - inProgressBitmap.Width) / 2;
				g.DrawImage(temporaryRotatedBitmap, new Rectangle(0, 0, inProgressBitmap.Width, inProgressBitmap.Height),
					new Rectangle(offset, offset, inProgressBitmap.Width, inProgressBitmap.Height), GraphicsUnit.Pixel);
			}

			InProgressImage.Image?.Dispose();
			InProgressImage.Image = finalRotatedBitmap;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			AnimationTimer.Stop();
		}

		public void UpdateBranding()
		{
			Icon = BrandManager.CargoWise.CargoWiseResources.WTG_Icon;
			CWNextLogoBox.Image = BrandManager.CargoWise.CargoWiseResources.WTG_Logo;
		}

		public void UpdateStatus(string status, int progressValue)
		{
			StatusLabel.Text = status;
			ProgressBar.Value = progressValue;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var radius = 32;

			var path = new GraphicsPath();
			path.AddArc(0, 0, radius, radius, 180, 90); // Top-left corner
			path.AddArc(Width - radius, 0, radius, radius, 270, 90); // Top-right corner
			path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90); // Bottom-right corner
			path.AddArc(0, Height - radius, radius, radius, 90, 90); // Bottom-left corner
			path.CloseFigure();

			Region = new Region(path);
		}
	}
}
