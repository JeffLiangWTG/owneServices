using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	public interface IGridColumnStyle : IResCaptionedControl
	{
		string MappingName { get; }
		bool HeaderTextWasDefaulted { get; }
		DataGrid Grid { get; }
	}

	public abstract class GridColumnNotificationProvider
	{
		public GridColumnNotificationProvider(IGridColumnStyle columnStyle)
		{
			ColumnStyle = columnStyle;
		}
#if DEBUG
		internal
#endif
		protected readonly IGridColumnStyle ColumnStyle;

		public abstract Rectangle PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(Graphics graphics, CurrencyManager source, int paintingRowNum, Rectangle cellBounds, Brush backBrush);

		public abstract bool DoesCellHaveAnyNotifications(CurrencyManager source, int rowNum);

		[return: DpiState(DpiState.ScaledVariant)]
		public abstract Rectangle AdjustEditControlBoundsForNotificationIconIfRequired(Rectangle originalBounds, CurrencyManager source, int rowNum);

#if !WINZOR
		public abstract Brush DecideBackgroundColorBrush(CurrencyManager source, int paintingRowNum, Brush defaultBrush);
#endif

		public virtual void OnMouseHover(CurrencyManager source, int rowNum, Point mouseLocationInCell, Rectangle cellBounds, Control anchor = null)
		{
		}

		public virtual void OnMouseLeave(CurrencyManager source, int rowNum)
		{
		}

		public virtual void OnEnterEditControl(CurrencyManager source, int rowNum, Rectangle editControlRect, Control editControl = null)
		{
		}

		public virtual void OnLeaveEditControl()
		{
		}

		public virtual void ShowHelpBalloon(CurrencyManager source, int rowNum, Rectangle anchorRect)
		{
		}

		protected internal int NotificationIconWidth
		{
			get { return NotificationIconScheme.Instance.GetMiniImage(NotificationType.Error).Width; }
		}

		protected internal int NotificationIconHeight
		{
			get { return NotificationIconWidth; }
		}

		[return: DpiState(DpiState.ScaledVariant)]
		public Rectangle ShrinkBoundsForNotificationIcon(Rectangle bounds)
		{
			return ControlDpiScalingHelper.NewScaledRectangle(bounds.X + NotificationIconWidth, bounds.Y, bounds.Width - NotificationIconWidth, bounds.Height, false);
		}

		protected void PaintNotificationIcon(Graphics graphics, Rectangle iconBounds, Image notificationIcon, Brush backgroundBrush)
		{
			graphics.FillRectangle(backgroundBrush, iconBounds);

			var alignedIcon = VAlignRect(iconBounds, notificationIcon.Size, 1, 1);
			graphics.DrawImageUnscaled(notificationIcon, alignedIcon);
		}

		public static Rectangle VAlignRect(Rectangle area, Size clip, int xOffset, int yOffset)
		{
			return ControlDpiScalingHelper.NewScaledRectangle(area.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(xOffset),
				area.Y + (area.Height / 2) - (clip.Height / 2) + ControlDpiScalingHelper.ScaleToCurrentDpiY(yOffset),
				clip.Width, area.Height, false);
		}
	}
}
