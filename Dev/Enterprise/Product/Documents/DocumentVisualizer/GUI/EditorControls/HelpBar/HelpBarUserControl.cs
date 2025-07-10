using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class HelpBarUserControl : ZUserControl
	{
		internal HelpBarUserControl()
		{
			InitializeComponent();
		}

		internal static void AttachToParent(IHaveHelpBar helpBarParent)
		{
			var control = helpBarParent as Control ?? throw new NotSupportedException("Only Controls should implement IHaveHelpBar");

			if (helpBarParent != null && control.Parent != null)
			{
				var helpBar = new HelpBarUserControl();
				helpBar.Visible = false;
				helpBar.attachedControl = control;

				helpBarParent.HelpBar = helpBar;
				AddHints(helpBar, helpBarParent);

				control.Parent.Controls.Add(helpBar);

				helpBar.SetLocation();
				control.LocationChanged += helpBar.AttachedControlLocationChanged;
				control.Disposed += helpBar.AttachedControlDisposed;
			}
		}

		static void AddHints(HelpBarUserControl helpBar, IHaveHelpBar helpBarParent)
		{
			if (!helpBarParent.KeyboardHints.Any())
			{
				var noHelpLabel = new ZLabel();
				noHelpLabel.Size = helpBar.Size;
				noHelpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				noHelpLabel.Text = NoHelpAvailableMessage;
				helpBar.Controls.Add(noHelpLabel);

				helpBar.Size = noHelpLabel.Size;
			}
			else
			{
				const int shortCutMaxWidth = 75;
				const int padding = 4;
				int yLocation = 0;
				int widestPoint = 0;

				foreach (var hint in helpBarParent.KeyboardHints)
				{
					var shortcutLabel = new ZLabel();
					shortcutLabel.AutoSize = true;
					shortcutLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(shortCutMaxWidth, 0);
					shortcutLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, yLocation);
					shortcutLabel.Font = new Font(shortcutLabel.Font, FontStyle.Bold);
					shortcutLabel.Text = hint.Key;
					helpBar.Controls.Add(shortcutLabel);

					var hintLabel = new ZLabel();
					hintLabel.AutoSize = true;

					var unscaleLabelWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(helpBar.Width - shortcutLabel.Width);
					var unscaleshortcutLabel = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(shortcutLabel.Width);

					hintLabel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(unscaleLabelWidth, 0);
					hintLabel.Location = ControlDpiScalingHelper.NewScaledPoint(unscaleshortcutLabel, yLocation);
					hintLabel.Text = hint.Value;
					helpBar.Controls.Add(hintLabel);

					yLocation = yLocation + Math.Max(ControlDpiScalingHelper.UnscaleFromCurrentDpiY(shortcutLabel.Height), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(hintLabel.Height)) + padding;

					if (ControlDpiScalingHelper.UnscaleFromCurrentDpiX(hintLabel.Right) > widestPoint)
					{
						widestPoint = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(hintLabel.Right);
					}
				}

				helpBar.Size = ControlDpiScalingHelper.NewScaledSize(widestPoint + padding, yLocation);
			}
		}

		static string NoHelpAvailableMessage
		{
			get { return Res.GetString("db357221-9a3f-4ea4-9db6-be84196f3d7e", "No help available for this control."); }
		}

		Control attachedControl;

		void AttachedControlDisposed(object sender, EventArgs e)
		{
			if (Parent != null)
			{
				Parent.Controls.Remove(this);
			}

			Dispose();
		}

		void AttachedControlLocationChanged(object sender, EventArgs e)
		{
			SetLocation();
		}

		void SetLocation()
		{
			if (attachedControl != null)
			{
				var x = attachedControl.Location.X;

				if (x + Width > Parent.Width)
				{
					x = x - (Width - attachedControl.Width);
				}

				var y = attachedControl.Location.Y + attachedControl.Height;

				if (y + Height > Parent.Height)
				{
					y = attachedControl.Location.Y - Height;
				}

				var unscaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(x);
				var unscaleY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(y);

				this.Location = ControlDpiScalingHelper.NewScaledPoint(unscaleX, unscaleY);
			}
		}
	}
}
