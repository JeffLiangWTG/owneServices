using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public partial class IconAdornment : InteractiveAdornment
	{
#if !WINZOR
		static ColorMatrix DefaultColorMatrix
		{
			get { return defaultColorMatrix ?? (defaultColorMatrix = CreateColorMatrix(1)); }
		}
		[ThreadStatic]
		static ColorMatrix defaultColorMatrix;

		static ColorMatrix SemiTransparentColorMatrix
		{
			get { return semiTransparentColorMatrix ?? (semiTransparentColorMatrix = CreateColorMatrix(0.5f)); }
		}
		[ThreadStatic]
		static ColorMatrix semiTransparentColorMatrix;
#endif

		GraphicsPath hotSpot;

		#region Mounting

		public override void Attach()
		{
#if WINZOR
			RefreshNotificationIcon();
#endif
			Control.Layout += OnLayout;
			Control.Paint += OnPaint;
			base.Attach();
		}

		void OnLayout(object sender, EventArgs e)
		{
#if WINZOR
			RefreshNotificationIcon();
#endif
		}

		public override void Detach()
		{
			Control.Paint -= OnPaint;
			base.Detach();
		}

		void OnPaint(object sender, PaintEventArgs e)
		{
			Paint(e);
		}

		#endregion

		/// <summary>
		/// Draws adornment for specified control using provided graphics surface and client rectangle area.
		/// </summary>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		protected internal virtual void Paint(PaintEventArgs e)
		{
#if !WINZOR
			hotSpot = null;

			if (State == null)
			{
				return;
			}

			var clientRectangle = Control.ClientRectangle;

			var customArgs = e as ZPaintEventArgs;
			if (customArgs != null)
			{
				clientRectangle = customArgs.ActualRectangle;
			}

			Draw(e.Graphics, clientRectangle, NotificationIconScheme.Instance.GetMiniImage(State));
#endif
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
#if !WINZOR
		void Draw(Graphics graphics, Rectangle rectangle, Image image)
		{
			try
			{
				var area = IconLayout.Compute(rectangle, image.Size);

				hotSpot = new GraphicsPath();
				hotSpot.AddRectangle(area);

				using (var attrs = new ImageAttributes())
				{
					attrs.SetColorMatrix(IconLayout.SemiTransparent ? SemiTransparentColorMatrix : DefaultColorMatrix);
					graphics.DrawImage(image, area, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}
#endif

		public IIconLayout IconLayout { get; set; }

		public override IEnumerable<INotification> Notifications
		{
			set
			{
				base.Notifications = value;
#if WINZOR
				RefreshNotificationIcon();
#endif
				IconLayout.ResizeControl(Notifications.GetHighestSeverityNotificationType() == null);
				Control.Invalidate();
			}
		}

		protected internal override GraphicsPath HotSpot
		{
			get { return hotSpot; }
		}

#if !WINZOR
		static ColorMatrix CreateColorMatrix(float opacity)
		{
			return new ColorMatrix(
				new[]
				{
						new float[] { 1, 0, 0, 0, 0 },
						new float[] { 0, 1, 0, 0, 0 },
						new float[] { 0, 0, 1, 0, 0 },
						new float[] { 0, 0, 0, opacity, 0 },
						new float[] { 0, 0, 0, 0, 1 }
					}
				);
		}
#endif
	}
}
