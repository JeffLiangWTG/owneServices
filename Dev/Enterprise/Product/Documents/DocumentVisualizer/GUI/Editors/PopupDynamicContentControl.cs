using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class PopupDynamicContentControl : ZUserControl, IHaveHelpBar
	{
		public PopupDynamicContentControl()
		{
			InitializeComponent();

			cornerCloseButton.FlatStyle = FlatStyle.Flat;

			EventHandler closeAction = (s, e) => Close();
			closeButton.Click += closeAction;
			cornerCloseButton.Click += closeAction;

			MakeDraggable();

			this.OnTabSelectNextControl += HandleTabSelectNextControl;
			this.ParentChanged += (s, e) => SetLocation();
		}

		#region Draggable

		bool isDraggable;
		Size offset;

		IEnumerable<Control> NonDraggableItems
		{
			get { yield return cornerCloseButton; }
		}

		void MakeDraggable()
		{
			MakeDraggable(topPanel);

			foreach (var control in topPanel.Controls.Cast<Control>().Except(NonDraggableItems))
			{
				MakeDraggable(control);
			}
		}

		void MakeDraggable(Control contol)
		{
			contol.MouseDown += OnMouseDown;
			contol.MouseUp += OnMouseUp;
			contol.MouseMove += OnMouseMove;
		}

		void OnMouseDown(object sender, MouseEventArgs args)
		{
			var unscaleLocationX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(args.Location.X);
			var unscalesLocationY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(args.Location.Y);

			offset = ControlDpiScalingHelper.NewScaledSize(unscaleLocationX, unscalesLocationY);
			isDraggable = true;
		}

		void OnMouseUp(object sender, MouseEventArgs args)
		{
			isDraggable = false;
		}

		void OnMouseMove(object sender, MouseEventArgs args)
		{
			if (isDraggable && Parent != null)
			{
				var newLocationOffset = args.Location - offset;

				var maxLeft = Parent.Width - Width;
				var maxTop = Parent.Height - Height;

				var left = Math.Max(Left + newLocationOffset.X, 0);
				var top = Math.Max(Top + newLocationOffset.Y, 0);

				ControlDpiScalingHelper.SetLeft(this, Math.Min(left, maxLeft), false);
				ControlDpiScalingHelper.SetTop(this, Math.Min(top, maxTop), false);
			}
		}

		#endregion

		#region Content Layout

		readonly SortedList<Point, Control> controlsInTabOrder = new SortedList<Point, Control>(new LocationComparer());

		class LocationComparer : IComparer<Point>
		{
			int IComparer<Point>.Compare(Point point1, Point point2)
			{
				var result = point1.Y.CompareTo(point2.Y);

				if (result != 0)
				{
					return result;
				}

				return point1.X.CompareTo(point2.X);
			}
		}

		IEnumerable<Control> ContentControls
		{
			get
			{
				return contentPanel
					.Controls
					.Cast<Control>()
					.Where(control => control.Visible);
			}
		}

		void SetSize()
		{
			var maxWidth = Parent?.Width ?? ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			var minWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			var bottomMargin = ControlDpiScalingHelper.ScaleToCurrentDpiY(6);

			var contentHeight = 0;
			var contentWidth = 0;

			controlsInTabOrder.Clear();

			foreach (var contentControl in ContentControls)
			{
				var controlHeight = contentControl.Location.Y + contentControl.Size.Height;
				contentHeight = Math.Max(contentHeight, controlHeight);

				var controlWidth = contentControl.Location.X + contentControl.Size.Width;
				contentWidth = Math.Max(contentWidth, controlWidth);

				if (contentControl.CanSelect)
				{
					controlsInTabOrder.Add(contentControl.Location, contentControl);
				}
			}

			var height = topPanel.Height + bottomPanel.Height + contentHeight + bottomMargin;
			var width = ZMath.Clamp(contentWidth, minWidth, maxWidth);

			Size = ControlDpiScalingHelper.NewScaledSize(width, height, false);

			SetLocation();
		}

		void SetLocation()
		{
			if (Parent != null)
			{
				var locX = ZMath.Clamp(Left, 0, Math.Max(Parent.Width - Width, 0));
				var locY = ZMath.Clamp(Top, 0, Math.Max(Parent.Height - Height, 0));

				Location = ControlDpiScalingHelper.NewScaledPoint(locX, locY, false);
			}
		}

		void HandleTabSelectNextControl(object sender, TabbedNavigationEventArgs eventArgs)
		{
			if (ActiveControl == closeButton)
			{
				var handler = OnTabOffControl;

				if (handler != null)
				{
					handler(this, eventArgs);
				}

				eventArgs.Handled = true;
				return;
			}

			var index = ActiveControl != null
				? controlsInTabOrder.IndexOfKey(ActiveControl.Location)
				: -1;

			if (index == controlsInTabOrder.Count - 1)
			{
				closeButton.Select();
				eventArgs.Handled = true;
			}
		}

		protected override void SelectCore(bool directed, bool forward)
		{
			if (controlsInTabOrder.Count == 0)
			{
				closeButton.Select();
			}
			else
			{
				base.SelectCore(directed, forward);
			}
		}

		protected override bool ProcessCmdKey(ref Message m, Keys key)
		{
			const ushort WM_KEYDOWN = 0x100;

			if (m.Msg == WM_KEYDOWN && key == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref m, key);
		}

		#endregion

		public event EventHandler OnClose;
		public event ZUserControl.TabbedNavigationEventHandler OnTabOffControl;

		public void AddContentControls(IEnumerable<Control> controls)
		{
			if (controls != null)
			{
				contentPanel.SuspendLayout();

				foreach (var control in controls)
				{
					if (control != null)
					{
						contentPanel.Controls.Add(control);
						nothingToEditLabel.Visible = false;
					}
				}

				contentPanel.ResumeLayout();

				SetSize();
			}
		}

		public void SetScale(float scale)
		{
			contentPanel.SuspendLayout();

			scale = Math.Min(scale, 2f);

			foreach (var control in ContentControls)
			{
				control.Font = new Font(control.Font.Name, control.Font.Size * scale);
				control.Scale(new SizeF(scale, scale));
			}
			var unscaleScaleWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)(Width * scale));
			var unscalesScaleHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((int)(Height * scale));

			Size = ControlDpiScalingHelper.NewScaledSize(unscaleScaleWidth, unscalesScaleHeight);

			contentPanel.ResumeLayout();

			SetSize();
		}

		#region IHaveHelpBar

		HelpBarUserControl IHaveHelpBar.HelpBar { get; set; }

		IEnumerable<KeyValuePair<string, string>> IHaveHelpBar.KeyboardHints
		{
			get
			{
				if (keyboardHints == null)
				{
					keyboardHints = new List<KeyValuePair<string, string>>();
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.Escape, KeyboardHints.Hints.Escape));
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.TabForward, KeyboardHints.Hints.TabForward));
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.TabBackward, KeyboardHints.Hints.TabBackward));

					if (ContainsMultilineTextBox(this))
					{
						keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.NewLine, KeyboardHints.Hints.NewLinePopup));
					}
				}

				return keyboardHints;
			}
		}

		List<KeyValuePair<string, string>> keyboardHints;

		bool ContainsMultilineTextBox(Control control)
		{
			var textBox = control as ZTextBox;
			return textBox != null && textBox.Multiline
				|| control.Controls.Cast<Control>().Any(ContainsMultilineTextBox);
		}

		#endregion

		#region Implementation

		void Close()
		{
			var handler = OnClose;

			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}