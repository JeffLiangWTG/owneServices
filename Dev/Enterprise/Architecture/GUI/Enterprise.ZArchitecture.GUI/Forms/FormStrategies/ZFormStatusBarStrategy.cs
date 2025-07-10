using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IStatusBarProvider
	{
		ZStatusBar MainStatusBar { get; set; }
		ZStatusBarPanel MessageStatusBarPanel { get; set; }
		ZStatusBarPanel ErrorStatusBarPanel { get; set; }
		INotificationType StatusBarINotificationType { get; }
		bool DisplayErrorsInMessagePanel { get; set; }
		bool ShowStatusBar { get; }
	}

	static class ZFormStatusBarStrategy
	{
		public static Brush DescriptionBrush { get { return SystemBrushes.WindowText; } }
		public static Brush ErrorBrush { get { return Brushes.Red; } }
		public static Brush WarningBrush { get { return Brushes.Olive; } }
		public static Brush MessageErrorBrush { get { return Brushes.Blue; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		public static void AddAdornments(Form form)
		{
			var statusBarProvider = form as IStatusBarProvider;

			if (statusBarProvider.ShowStatusBar)
			{
				var mainStatusBar = new ZStatusBar();
				var messageStatusBarPanel = new ZStatusBarPanel();
				var errorStatusBarPanel = new ZStatusBarPanel();

				if (statusBarProvider != null)
				{
					statusBarProvider.MainStatusBar = mainStatusBar;
					statusBarProvider.MessageStatusBarPanel = messageStatusBarPanel;
					statusBarProvider.ErrorStatusBarPanel = errorStatusBarPanel;
				}

			((System.ComponentModel.ISupportInitialize)(messageStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(errorStatusBarPanel)).BeginInit();
				form.SuspendLayout();
				// 
				// MainStatusBar
				// 
				mainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114);
				mainStatusBar.Name = "MainStatusBar";
				mainStatusBar.Panels.AddRange(new StatusBarPanel[] { messageStatusBarPanel, errorStatusBarPanel });
				mainStatusBar.ShowPanels = true;
				mainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 24);
				mainStatusBar.TabIndex = 0;
				mainStatusBar.TabStop = false;

				mainStatusBar.DrawItem +=
					delegate(object sender, StatusBarDrawItemEventArgs sbdevent)
					{
					#if !WINZOR
						var textToDisplay = sbdevent.Panel.Text;
						var textBrush = GetStatusBarTextBrush(form);

						using (var strfmt = new StringFormat())
						{
							strfmt.Alignment = StringAlignment.Near;
							strfmt.LineAlignment = StringAlignment.Center;
							strfmt.Trimming = StringTrimming.EllipsisWord;

							var outlineBox = ControlDpiScalingHelper.NewScaledRectangle(
								sbdevent.Bounds.X, sbdevent.Bounds.Y, sbdevent.Bounds.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), sbdevent.Bounds.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);

							try
							{
								sbdevent.Graphics.DrawRectangle(SystemPens.ControlDark, outlineBox);
								var stringBoundsF = new RectangleF(outlineBox.X + 3, outlineBox.Y + 3, outlineBox.Width - 3, OFont.GetFont().Height);
								TextRendererHelper.DrawText(sbdevent.Graphics, textToDisplay, sbdevent.Font, stringBoundsF, textBrush, strfmt);
							}
							catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
							catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
						}
					#else
						var statusBar = (ZStatusBar)sender;
						int borderDivHeight = statusBar.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
						statusBar.StatusBarBorderStyleString = $"height: {borderDivHeight}px; overflow: hidden;";

						foreach (var panel in statusBar.Panels)
						{
							panel.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(Math.Max(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(borderDivHeight) - 4));
						}
					#endif
					};
				// 
				// MessageStatusBarPanel
				// 
				messageStatusBarPanel.BorderStyle = StatusBarPanelBorderStyle.None;
				messageStatusBarPanel.Style = StatusBarPanelStyle.OwnerDraw;
				ControlDpiScalingHelper.SetWidth(ref messageStatusBarPanel, 300, true);
				// 
				// ErrorStatusBarPanel
				// 
				errorStatusBarPanel.BorderStyle = StatusBarPanelBorderStyle.None;
				errorStatusBarPanel.Style = StatusBarPanelStyle.OwnerDraw;
				// 
				// OWinForm
				// 
				form.Controls.Add(mainStatusBar);
				form.Resize += delegate { if (!form.IsDesignMode()) { ResizeStatusBarPanels(form, mainStatusBar, messageStatusBarPanel, errorStatusBarPanel); } };
				((System.ComponentModel.ISupportInitialize)(messageStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(errorStatusBarPanel)).EndInit();
				form.ResumeLayout(false);
			}
		}

#if DEBUG
		internal
#endif
 static Brush GetStatusBarTextBrush(Form form)
		{
			var statusBarProvider = form as IStatusBarProvider;
			if (statusBarProvider != null)
			{
				if (statusBarProvider.StatusBarINotificationType == CargoWise.ComponentModel.NotificationType.Error)
				{
					return ErrorBrush;
				}
				if (statusBarProvider.StatusBarINotificationType == CargoWise.EntityFramework.NotificationType.MessageError)
				{
					return MessageErrorBrush;
				}
				if (statusBarProvider.StatusBarINotificationType == CargoWise.ComponentModel.NotificationType.Warning)
				{
					return WarningBrush;
				}
			}

			return DescriptionBrush;
		}

		internal static void ResizeStatusBarPanels(Form form, ZStatusBar mainStatusBar, ZStatusBarPanel messageStatusBarPanel, ZStatusBarPanel errorStatusBarPanel)
		{
			const int GripSize = 15;
			var totalUsableLength = mainStatusBar.Width;

			if (form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				totalUsableLength = mainStatusBar.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(GripSize);
				mainStatusBar.SizingGrip = true;
			}
			else
			{
				mainStatusBar.SizingGrip = false;
			}

			if (totalUsableLength < ControlDpiScalingHelper.ScaleToCurrentDpiX(40))
			{
				totalUsableLength = ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			}

			var statusBarProvider = form as IStatusBarProvider;
			if (statusBarProvider != null && statusBarProvider.DisplayErrorsInMessagePanel)
			{
				ControlDpiScalingHelper.SetWidth(ref messageStatusBarPanel, totalUsableLength, false);
				errorStatusBarPanel.MinWidth = 0;
				ControlDpiScalingHelper.SetWidth(ref errorStatusBarPanel, 0, true);
			}
			else
			{
				ControlDpiScalingHelper.SetWidth(ref messageStatusBarPanel, (int)(totalUsableLength * 0.5), false);
				ControlDpiScalingHelper.SetWidth(ref errorStatusBarPanel, totalUsableLength - messageStatusBarPanel.Width, false);
			}
		}
	}
}
