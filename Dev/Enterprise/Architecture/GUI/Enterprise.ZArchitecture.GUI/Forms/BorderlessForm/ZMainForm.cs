using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Drawing.Colors;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Drawing;
using Enterprise.ZArchitecture.GUI.Forms.BorderlessForm;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	// Builds on a borderless window by providing a title bar, colour theming, and a workspace panel
	public partial class ZMainForm : ZBorderlessForm, IToggleMaximiseForm
	{
		internal readonly IMessageFilter messageFilter;
		public ZMainForm()
		{
			InitializeComponent();
			InitialiseWindowTheme();
			ResizeMainWorkPanel();
			ResizeTitleBar();
			UpdateAppButtonTooltips();
#if !WINZOR
			messageFilter = new AppTitleDoubleClickMessageFilter(this);
			Application.AddMessageFilter(messageFilter);
#endif

#if WINZOR
			AppIcon.Visible = false;
			AppMinimise.Visible = false;
			AppMaximise.Visible = false;
			AppClose.Visible = false;
#endif
		}
#if !WINZOR
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			Application.RemoveMessageFilter(messageFilter);
			base.OnFormClosed(e);
		}
#endif

		#region Events

		internal void AppMinimise_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				WindowState = FormWindowState.Minimized;
			}
		}

		internal void AppMaximise_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ToggleMaximise();
			}
		}

		internal void AppClose_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Close();
			}
		}

		DateTime systemMenuCloseTime = DateTime.MinValue;
		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "app-specific time")]
		internal void AppIcon_MouseUp(object sender, MouseEventArgs e)
		{
			if ((DateTime.Now - systemMenuCloseTime).TotalMilliseconds > 200) // app-specific time
			{
				ShowSystemMenu();
				systemMenuCloseTime = DateTime.Now; // app-specific time
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "app-specific time")]
		internal void AppTitleText_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				FormTitleMouseDown(e);
			}
		}

		internal void AppTitleText_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && AppTitleText.ClientRectangle.Contains(e.Location))
			{
				ShowSystemMenu();
			}
		}

		internal void ZMainForm_Activated(object sender, EventArgs e)
		{
			RecalculateCurrentTheme(true);
		}

		internal void ZMainForm_Deactivate(object sender, EventArgs e)
		{
			RecalculateCurrentTheme(false);
		}

		internal void ZMainForm_SizeChanged(object sender, EventArgs e)
		{
			var maximized = WindowState == FormWindowState.Maximized;
			AppMaximise.Text = maximized ? "2" : "1";

			var panels = new[] { TopLeftCornerPanel, TopRightCornerPanel, BottomLeftCornerPanel, BottomRightCornerPanel,
				TopBorderPanel, LeftBorderPanel, RightBorderPanel, BottomBorderPanel };

			foreach (var panel in panels)
			{
				panel.Visible = !maximized;
			}

			ResizeMainWorkPanel();
			ResizeTitleBar();
		}

		internal void ZMainForm_BackColorChanged(object sender, EventArgs e)
		{
			Main.BackColor = BackColor;
			Workspace.BackColor = BackColor;
		}

		internal void ZMainForm_TextChanged(object sender, EventArgs e)
		{
			AppTitleText.Text = TextIncludingSuffix;
		}

		// https://docs.microsoft.com/en-us/windows/desktop/winmsg/wm-seticon
		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "CA rule is incorrect")]
		void ZMainForm_IconChanged(object sender, Native.IconType iconType)
		{
			if (iconType == Native.IconType.SMALL)
			{
				AppIcon.DisplayedImage = Icon.ToBitmap();
			}
		}

		#endregion

		#region Themes

		protected internal void InitialiseWindowTheme()
		{
			Main.BackColor = BackColor;
			Workspace.BackColor = BackColor;
			WindowTheme = DefaultWindowTheme;
		}

		protected internal virtual void SetTextColor(Color color)
		{
			AppTitleText.ForeColor = color;
		}

		protected internal virtual void SetTitleBarColor(Color color)
		{
			AppTitleText.BackColor = color;
			AppIcon.BackColor = color;
			TitleBar.BackColor = color;
		}

		protected internal virtual void ApplyWindowTheme(WindowTheme theme)
		{
			SetBorderColour(theme.Border);
			SetTextColor(theme.Text);
			SetTitleBarColor(theme.TitleBar);
		}

		protected internal virtual void RecalculateCurrentTheme(bool isActive)
		{
			if (isActive)
			{
				ApplyWindowTheme(WindowTheme);
				AppMinimise.ApplyTheme(StandardButtonTheme);
				AppMaximise.ApplyTheme(StandardButtonTheme);
				AppClose.ApplyTheme(CloseButtonTheme);
			}
			else
			{
				ApplyWindowTheme(WindowTheme.AsInactive());
				AppMinimise.ApplyTheme(StandardButtonTheme.AsInactive());
				AppMaximise.ApplyTheme(StandardButtonTheme.AsInactive());
				AppClose.ApplyTheme(CloseButtonTheme.AsInactive());
			}
		}

		static readonly Color DefaultBorderColour = Color.FromArgb(102, 203, 234);
		static readonly Color DefaultCloseButtonColor = Color.FromArgb(234, 102, 102);
		static readonly Color DefaultTextColour = Color.Black;
		static readonly Color DefaultTitleBarColour = Color.FromArgb(102, 203, 234);

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		public static readonly WindowTheme DefaultWindowTheme = new WindowTheme(DefaultBackColor, DefaultBorderColour, DefaultCloseButtonColor, DefaultTextColour, DefaultTitleBarColour);

		// setting the theme will force a recalculate of all colours and will be applied immediately.
		WindowTheme windowTheme;
		public WindowTheme WindowTheme
		{
			get => windowTheme;
			set
			{
				windowTheme = value;
				StandardButtonTheme = new ButtonTheme(WindowTheme.Text, WindowTheme.TitleBar);
				CloseButtonTheme = new ButtonTheme(WindowTheme.Text, WindowTheme.Close);
				RecalculateCurrentTheme(true);
			}
		}

		protected internal ButtonTheme StandardButtonTheme { get; set; }
		protected internal ButtonTheme CloseButtonTheme { get; set; }

		#endregion

		#region Size

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Already scaled")]
		protected internal virtual void ResizeTitleBar()
		{
			// make all these icons squares, locking their width to their height, which is the height of the title bar
			AppClose.Width = AppClose.Height;
			AppMaximise.Width = AppMaximise.Height;
			AppMinimise.Width = AppMinimise.Height;

			AppIcon.Width = AppIcon.Height;
			if (WindowState == FormWindowState.Maximized)
			{
				AppIcon.Padding = new Padding(BorderSize / 2);
			}
			else
			{
				AppIcon.Padding = new Padding(0, 0, 0, BorderSize); // account for form border top
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Already scaled")]
		protected internal virtual void ResizeMainWorkPanel()
		{
			if (WindowState == FormWindowState.Maximized)
			{
				Main.Dock = DockStyle.Fill; // this is kind of a hack since the sizing doesn't work properly because windows returns the wrong window size (it adds borders)
			}
			else
			{
				Main.Dock = DockStyle.None;
				var mainWorkPanelWidth = Math.Min(Width - LeftBorderPanel.Width - RightBorderPanel.Width, short.MaxValue);
				var mainWorkPanelHeight = Math.Min(Height - TopBorderPanel.Height - BottomBorderPanel.Height, short.MaxValue);

				Main.Size = new Size(mainWorkPanelWidth, mainWorkPanelHeight);
				Main.Location = new Point(LeftBorderPanel.Right, TopBorderPanel.Bottom);
			}
		}

		#endregion

		#region WndProc

#if !WINZOR

		[SuppressMessage("CargoWiseOne", "CW1040", Justification = "Report and escape when ZMainForm reaches an extreme size")]
		protected override void WndProc(ref Message m)
		{
			if (!this.IsDesignMode())
			{
				var canChangeSize = true;
				switch (m.Msg)
				{
					case WindowsMessage.WM_SIZE:
						canChangeSize = Check_WM_SIZE(ref m);
						break;
					case WindowsMessage.WM_WINDOWPOSCHANGING:
					case WindowsMessage.WM_WINDOWPOSCHANGED:
						canChangeSize = Check_WM_WINDOWPOSCHANGING(ref m);
						break;
				}
				if (!canChangeSize)
				{
					return;
				}
			}

			base.WndProc(ref m);
		}

		bool Check_WM_SIZE(ref Message m)
		{
			var width = NativeMethods.Util.SignedLOWORD(m.LParam);
			var height = NativeMethods.Util.SignedHIWORD(m.LParam);
			return CheckSize(width, height);
		}

		bool Check_WM_WINDOWPOSCHANGING(ref Message m)
		{
			var mwp = (WindowPos)Marshal.PtrToStructure(m.LParam, typeof(WindowPos));
			var noSize = mwp.flags & 1;
			if (noSize == 0) // Size is not ignored
			{
				return CheckSize(mwp.cx, mwp.cy);
			}
			return true;
		}

		bool CheckSize(int width, int height)
		{
			if (height >= UInt16.MaxValue || height < 0 || width >= UInt16.MaxValue || width < 0)
			{
				return false;
			}
			return true;
		}

#endif

		#endregion

		#region Form Functionality

		void UpdateAppButtonTooltips()
		{
			AppClose.IsCaptionOverridden = false;
			AppMinimise.IsCaptionOverridden = false;
			AppMaximise.IsCaptionOverridden = false;
			AppClose.ToolTipCaption = CloseButtonText;
			AppMinimise.ToolTipCaption = MinimiseButtonText;
			AppMaximise.ToolTipCaption = MaximiseButtonText;
		}

		public void SetIconFromImage(Image image)
		{
			var b = (Bitmap)image;
			using (var i = Icon.FromHandle(b.GetHicon()))
			{
				Icon = i;
			}
		}

		public void ToggleMaximise()
		{
			WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
			UpdateAppButtonTooltips();
		}

		#endregion

		#region Properties

		protected MultilingualString CloseButtonText => ResString.GetMultilingualString("6d065869-0e4c-4ed0-bae3-c80316973231", "Close");
		protected MultilingualString MinimiseButtonText => ResString.GetMultilingualString("6d065869-0e4c-4ed0-bae3-c80316973232", "Minimize");
		protected MultilingualString MaximiseButtonText => WindowState == FormWindowState.Maximized
			? ResString.GetMultilingualString("6d065869-0e4c-4ed0-bae3-c80316973233", "Restore")
			: ResString.GetMultilingualString("6d065869-0e4c-4ed0-bae3-c80316973234", "Maximize");

		[System.ComponentModel.Description("The scale factor to apply to the image.")]
		public float IconZoom
		{
			get => AppIcon.Zoom;
			set => AppIcon.Zoom = value;
		}

		public IntPtr TitleHWnd => this.AppTitleText?.Handle ?? IntPtr.Zero;

		#endregion
	}

	#region Theme Classes

	public class WindowTheme
	{
		public WindowTheme(Color backColor, Color border, Color close, Color text, Color titleBar)
		{
			BackColor = backColor;
			Border = border;
			Close = close;
			Text = text;
			TitleBar = titleBar;
		}

		public Color BackColor { get; }
		public Color Border { get; }
		public Color Close { get; }
		public Color Text { get; }
		public Color TitleBar { get; }

		public WindowTheme AsInactive()
		{
			return new WindowTheme
			(
				ColourHelper.MakeGrayscale(BackColor),
				ColourHelper.MakeGrayscale(Border),
				ColourHelper.MakeGrayscale(Close),
				ColourHelper.MakeGrayscale(Text),
				ColourHelper.MakeGrayscale(TitleBar)
			);
		}
	}

	#endregion
}
