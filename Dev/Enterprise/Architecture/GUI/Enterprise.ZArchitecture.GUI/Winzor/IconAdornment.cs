using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using Extensions;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public partial class IconAdornment : InteractiveAdornment
	{
		Rectangle drawingArea;

		[SuppressMessage("CargoWiseOne", "CW1017:Non DPI-aware code has been detected")]
		void RefreshNotificationIcon()
		{
			var container = Control.Parent;
			if (container == null)
			{
				return;
			}

			var icon = (NotificationIcon)container.WinzorSpecificControls.FirstOrDefault(c => c is NotificationIcon icon && icon.NotificationAnchorControl == Control);
			// Workaround for `PaintExtension.Skip = true;` in ZTextBox since we don't have Paint event in Winzor
			var skipPaintIcon = Control is ZTextBox && Control.Focused;
			if (State == null && icon != null || skipPaintIcon)
			{
				hotSpot = null;
				container.WinzorSpecificControls.Remove(icon);
			}
			else if (State != null)
			{
				hotSpot = new GraphicsPath();
				var iconImage = NotificationIconScheme.Instance.GetMiniImage(State);
				if (Control is ZDropButton)
				{
					drawingArea = IconLayout.Compute(((ZDropButton)Control).GetActualRectangleForNotificationIcon(), iconImage.Size);
				}
				else if (Control is ZCheckBox)
				{
					var iconOffset = IconLayout.Compute(ControlDpiScalingHelper.NewScaledRectangle(0, 0, Control.Width, Control.Height, false), iconImage.Size);
					drawingArea = new Rectangle(Control.ClientAreaBounds.Left + iconOffset.Left, Control.ClientAreaBounds.Top + iconOffset.Top, iconOffset.Size.Width, iconOffset.Size.Height);
				}
				else
				{
					drawingArea = IconLayout.Compute(Control.ClientAreaBounds, iconImage.Size);
				}
				hotSpot.AddRectangle(drawingArea);

				if (icon == null)
				{
					icon = new NotificationIcon();
					icon.OnMouseOver += (sender, args) => MouseMove(new MouseEventArgs(MouseButtons.Left, 1, drawingArea.Left, drawingArea.Top, 0));
					icon.OnMouseOut += (sender, args) => MouseLeave();
					container.WinzorSpecificControls.Add(icon);
				}

				icon.IconDimensions = drawingArea.Size;
				icon.NotificationIconBase64DataUrl = iconImage.ToBase64DataUrl();
				icon.Top = drawingArea.Top;
				icon.Left = drawingArea.Left;
				icon.NotificationType = State.EnumValueName.ToLowerHyphen();
				icon.NotificationAnchorControl = Control;
				icon.ZIndex = Control.ZIndex;
				icon.TabStop = false;
			}

			var isIconAnchoredToEmptyLabelText = Control is ZLabel && string.IsNullOrEmpty(Control.Text);
			if (isIconAnchoredToEmptyLabelText)
			{
				container.WinzorSpecificControls.Remove(icon);
			}
			//If multiple NotificationIcons are on a single control, remove the excess icons.
			var icons = container.WinzorSpecificControls.OfType<NotificationIcon>().Where(c => c.NotificationAnchorControl == Control);
			if (icons.Count() > 1)
			{
				foreach (var currentIcon in icons.Skip(1))
				{
					container.WinzorSpecificControls.Remove(currentIcon);
				}
			}
		}
	}
}
