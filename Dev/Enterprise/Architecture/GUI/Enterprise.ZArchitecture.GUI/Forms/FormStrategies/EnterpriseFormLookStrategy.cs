using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IPositionSaveProvider
	{
		Rectangle FormPositionRectangle { get; set; }
		bool RememberFormPosition { get; set; }
		bool RememberFormSize { get; set; }
	}

	public static class EnterpriseFormLookStrategy
	{
		public static void AddAdornments(Form form)
		{
			if (form != null && !form.IsDesignMode())
			{
				form.Font = OFont.GetFont();
				form.Icon = BrandingFactory.Instance.ProductIcon;
				form.KeyPreview = true;

#if DEBUG
				form.ShowInTaskbar = !Globals.IsTest;
#endif
			}
		}

		public static Color SelectedControlColor
		{
			get { return SystemColors.Info; }
		}

		#region Graphics Scaling

		static SizeF ScaleBaseSize
		{
			get { return new SizeF(6F, 13F); } // Based on Tahoma 8pt. If OFont.GetFont() changes, this must be changed.
		}

		static SizeF ScaleFactor
		{
			get
			{
				if (scaleFactor.IsEmpty)
				{
					using (var tempForm = new ZForm())
					{
						tempForm.SuspendLayout();
						tempForm.Font = OFont.GetFont();
						tempForm.AutoScaleDimensions = ScaleBaseSize;
						tempForm.AutoScaleMode = AutoScaleMode.Font;
						scaleFactor = tempForm.AutoScaleFactorForInternalUse;
					}
				}
				return scaleFactor;
			}
		}
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static SizeF scaleFactor;

		internal static float HorizontalScaleFactor
		{
			get { return ScaleFactor.Width; }
		}

		internal static float VerticalScaleFactor
		{
			get { return ScaleFactor.Height; }
		}

		internal static SizeF MaxCharacterWidth
		{
			get
			{
				if (fMaximumCharacterWidth == SizeF.Empty)
				{
					using (var textBox = new ZTextBox())
					using (var g = textBox.CreateGraphics())
					{
						fMaximumCharacterWidth = g.MeasureString("W", OFont.GetFont());
					}
				}
				return fMaximumCharacterWidth;
			}
		}
		static SizeF fMaximumCharacterWidth = SizeF.Empty;

		internal static float DpiX
		{
			get
			{
				if (fDpiX == 0.0f)
				{
					using (var textBox = new ZTextBox())
					using (var g = textBox.CreateGraphics())
					{
						fDpiX = g.DpiX;
					}
				}
				return fDpiX;
			}
		}
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static float fDpiX;

		#endregion

		#region Restore Size and Position

		public static bool FormIsSaved(Form form)
		{
			return ((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.ContainsForm(form.Name);
		}

		public static void RestorePositionAndSize(Form form)
		{
			RestoreJustPositionAndSize(form);

			if (form is IStatusBarProvider statusBarProvider && statusBarProvider.ShowStatusBar)
			{
				ZFormStatusBarStrategy.ResizeStatusBarPanels(form, statusBarProvider.MainStatusBar, statusBarProvider.MessageStatusBarPanel, statusBarProvider.ErrorStatusBarPanel);
			}
		}

		public static void RestoreJustPositionAndSize(Form form, bool justPosition = false)
		{
			if (!form.IsDesignMode()
				&& !Db.DatabaseUpgradedExceptionHasBeenThrownInConnection
				&& (!Globals.IsTest
#if DEBUG
			|| (form is ZForm zForm && zForm.ForceRememberPositionAndSize)
#endif
			))
			{
				var formRect = ((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.GetFormLocationAndSize(form.Name);

#if !WINZOR

				if (formRect != Rectangle.Empty && !CachedScreenInfo.Instance.Contains(formRect))
				{
					// cache is invalid
					Enterprise.RemoteDesktopServices.Server.TrackingInfo.TrackingInfoLogger.Instance.NewLog(() =>
					{
						var sb = new StringBuilder();
						sb.AppendLine($"Invalid form area: {formRect}");
						sb.AppendLine($"Current screen work areas:");
						foreach (var screen in CachedScreenInfo.Instance.ScreenInfos)
						{
							sb.AppendLine($"    {screen}");
						}
						return sb.ToString();
					});
					formRect = Rectangle.Empty;
				}

#endif

				SetPositionAndSize(form, formRect.X, formRect.Y,
					(int)(formRect.Width * HorizontalScaleFactor),
					(int)(formRect.Height * VerticalScaleFactor), justPosition: justPosition);
			}
		}

		public static void SavePositionAndSize(Form form)
		{
			if (form is IPositionSaveProvider positionSaveProvider
				&& positionSaveProvider.FormPositionRectangle != form.Bounds
				&& !form.IsDesignMode()
				&& !Db.DatabaseUpgradedExceptionHasBeenThrownInConnection
				&& (!Globals.IsTest
#if DEBUG
			|| (form is ZForm zForm && zForm.ForceRememberPositionAndSize)
#endif
			))
			{
				var formRect = Rectangle.Empty;

				if (positionSaveProvider.RememberFormPosition)
				{
					ControlDpiScalingHelper.SetX(ref formRect, form.Left, false);
					ControlDpiScalingHelper.SetY(ref formRect, form.Top, false);
				}

				if (positionSaveProvider.RememberFormSize &&
					(form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
				{
					ControlDpiScalingHelper.SetHeight(ref formRect, (int)(form.Height / VerticalScaleFactor), false);
					ControlDpiScalingHelper.SetWidth(ref formRect, (int)(form.Width / HorizontalScaleFactor), false);
				}

				if (formRect != Rectangle.Empty)
				{
					((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.SetFormLocationAndSize(form.Name, formRect);
				}

				positionSaveProvider.FormPositionRectangle = ControlDpiScalingHelper.NewScaledRectangle(form.Left, form.Top, form.Width, form.Height, false);
			}
		}
#if !WINZOR
		const int ScreenEdgeMargin = 50;
#endif

		internal static void SetPositionAndSize(Form form, int newLeft, int newTop, int newWidth, int newHeight, bool justPosition = false)
		{
			// Force first-time shown form to be centered
			if (!IsLayoutSaved(newWidth, newHeight) && newLeft == 0 && newTop == 0)
			{
				newLeft = -1;
				newTop = -1;

#if WINZOR
				// In Winzor, the server does not control the location of the client window.                                                                                              
				// However, the StartPosition variable determines the position as a window is first being displayed.
				// Using CenterScreen will replicate the existing functionality.
				form.StartPosition = FormStartPosition.CenterScreen;
#endif
			}

			RestoreFormLayout(form, newLeft, newTop, newWidth, newHeight, justPosition);

#if !WINZOR
			if (form.WindowState != FormWindowState.Maximized)
			{
				var currentScreen = CachedScreenInfo.Instance.FromControl(form);
				CheckHorizontalBounds(form, currentScreen);
				CheckVerticalBounds(form, currentScreen);
			}
#endif

			var positionSaveProvider = form as IPositionSaveProvider;
			if (positionSaveProvider != null)
			{
				positionSaveProvider.FormPositionRectangle = ControlDpiScalingHelper.NewScaledRectangle(form.Left, form.Top, form.Width, form.Height, false);
			}
		}

		static void RestoreFormLayout(Form form, int newLeft, int newTop, int newWidth, int newHeight, bool justPosition = false)
		{
			if (form.WindowState == FormWindowState.Maximized)
			{ return; }

			var positionSaveProvider = form as IPositionSaveProvider;
			if (positionSaveProvider != null)
			{
#if !WINZOR
				if (positionSaveProvider.RememberFormPosition && positionSaveProvider.RememberFormSize)
				{
					foreach (var screenBounds in CachedScreenInfo.Instance.ScreenInfos)
					{
						if (newLeft <= screenBounds.Left && newTop <= screenBounds.Top &&
							newLeft + newWidth >= screenBounds.Right && newTop + newHeight >= screenBounds.Bottom)
						{
							ControlDpiScalingHelper.SetLeft(ref form, screenBounds.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
							ControlDpiScalingHelper.SetTop(ref form, screenBounds.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
							CheckHorizontalBounds(form, screenBounds);
							CheckVerticalBounds(form, screenBounds);
							form.WindowState = FormWindowState.Maximized;
							return;
						}
					}
				}
#endif
				if (!justPosition && positionSaveProvider.RememberFormSize && IsLayoutSaved(newWidth, newHeight) &&
					(form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
				{
					if (form.Width != newWidth)
					{
						ControlDpiScalingHelper.SetWidth(ref form, newWidth, false);
					}
					if (form.Height != newHeight)
					{
						ControlDpiScalingHelper.SetHeight(ref form, newHeight, false);
					}
				}

				if (positionSaveProvider.RememberFormPosition)
				{
					ControlDpiScalingHelper.SetLeft(ref form, newLeft, false);
					ControlDpiScalingHelper.SetTop(ref form, newTop, false);
				}
			}
		}

		static bool IsLayoutSaved(int newWidth, int newHeight)
		{
			return (newWidth > 0 && newHeight > 0);
		}
#if !WINZOR
		static void CheckHorizontalBounds(Form form, Rectangle screenBounds)
		{
			if (form.Width > screenBounds.Width)
			{
				ControlDpiScalingHelper.SetWidth(ref form, screenBounds.Width, false);
			}

			if ((form.Left < screenBounds.Left) || (form.Left >= screenBounds.Right - ControlDpiScalingHelper.ScaleToCurrentDpiX(ScreenEdgeMargin)) || (form.Right > screenBounds.Right))
			{
				ControlDpiScalingHelper.SetLeft(ref form, screenBounds.Left + (screenBounds.Width - form.Width) / 2, false);
			}
		}

		static void CheckVerticalBounds(Form form, Rectangle screenBounds)
		{
			if (form.Height > screenBounds.Height)
			{
				ControlDpiScalingHelper.SetHeight(ref form, screenBounds.Height, false);
			}

			if ((form.Top < screenBounds.Top) || (form.Top >= screenBounds.Bottom - ControlDpiScalingHelper.ScaleToCurrentDpiY(ScreenEdgeMargin)) || (form.Bottom > screenBounds.Bottom))
			{
				ControlDpiScalingHelper.SetTop(ref form, screenBounds.Top + (screenBounds.Height - form.Height) / 2, false);
			}
		}
#endif
#endregion
	}
}
