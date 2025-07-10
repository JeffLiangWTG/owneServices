using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture.GUI
{
	internal enum BalloonNotificationType { None, Help, Notification, QuickViewCard }

	public class ZGridColumnNotificationProvider : GridColumnNotificationProvider
	{
		public ZGridColumnNotificationProvider(IGridColumnStyle columnStyle)
			: base(columnStyle)
		{
		}

		public override void OnEnterEditControl(CurrencyManager source, int rowNum, Rectangle editControlRect, Control editControl = null)
		{
			base.OnEnterEditControl(source, rowNum, editControlRect);

			if (StatusBar != null || EnvProxy.Instance.Registry.TraningModeEnabled)
			{
				var info = GetCurrentInfo(source, rowNum);
				var notifications = info == null ? null : info.Notifications;
				var highestSeverityNotificationType = notifications == null ? null : notifications.GetHighestSeverityNotificationType();
				var message = string.Empty;

				if (highestSeverityNotificationType == null)
				{
					var resString = GetResourceStringData();
					if (resString != null)
					{
						message = resString.FullDescription;
					}
				}
				else
				{
					message = notifications.GetHighestSeverityNotification().Message;
				}

				if (StatusBar != null)
				{
					StatusBar.UpdateStatusBar(message, highestSeverityNotificationType);
				}

				if (info != null && EnvProxy.Instance.Registry.TraningModeEnabled)
				{
#if WINZOR
					ShowHelpBalloon(info, editControlRect, editControl);
#else
					ShowHelpBalloon(info, editControlRect);
#endif
				}
			}
		}

		public override void OnLeaveEditControl()
		{
			base.OnLeaveEditControl();
			if (StatusBar != null)
			{
				StatusBar.UpdateStatusBar("", null);
			}
			HideBalloonIfShowing();
		}

		public override bool DoesCellHaveAnyNotifications(CurrencyManager source, int rowNum)
		{
			var info = GetCurrentInfo(source, rowNum);
			return info != null && info.HasNotifications();
		}

		#if !WINZOR
		public override Brush DecideBackgroundColorBrush(CurrencyManager source, int paintingRowNum, Brush defaultBrush)
		{
			var result = defaultBrush;
			if (!ColumnStyle.Grid.IsSelected(paintingRowNum))
			{
				var info = GetCurrentInfo(source, paintingRowNum);

				if (info != null)
				{
					var notificationType = info.GetHighestSeverityNotificationType();
					if (notificationType != null)
					{
						result = BrushProvider.FromColor(NotificationColorScheme.GetColor(notificationType));
					}
				}
			}
			return result;
		}
		#endif

		public override Rectangle PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(Graphics graphics, CurrencyManager source, int paintingRowNum, Rectangle cellBounds, Brush backgroundBrush)
		{
			var result = cellBounds;
			var info = GetCurrentInfo(source, paintingRowNum);

			if (info != null)
			{
				var notificationType = info.GetHighestSeverityNotificationType();
				if (notificationType != null)
				{
					var notificationIcon = NotificationIconScheme.Instance.GetMiniImage(notificationType);
					var iconBounds = ControlDpiScalingHelper.NewScaledRectangle(cellBounds.X, cellBounds.Y, notificationIcon.Width, cellBounds.Height, false);
					PaintNotificationIcon(graphics, iconBounds, notificationIcon, backgroundBrush);
					result = ShrinkBoundsForNotificationIcon(result);
				}
			}

			return result;
		}

		public override Rectangle AdjustEditControlBoundsForNotificationIconIfRequired(Rectangle originalBounds, CurrencyManager source, int rowNum)
		{
			var result = originalBounds;
			var info = GetCurrentInfo(source, rowNum);
			if (info != null && info.HasNotifications())
			{
				result = ShrinkBoundsForNotificationIcon(result);
			}
			return result;
		}

		ZPropertyInfo GetCurrentInfo(CurrencyManager source, int rowNum)
		{
			if (source != null && source.List != null && rowNum < source.List.Count)
			{
				var current = source.List[rowNum] as BusinessObject;
				if (current != null)
				{
					var dataGridColumnStyle = ColumnStyle as DataGridColumnStyle;
					return dataGridColumnStyle != null
						? ZPropertyInfoRetriever.GetZPropertyInfo(dataGridColumnStyle, current)
						: current.ZPropertyInfoHash.GetPropertySafe(ColumnStyle.MappingName);
				}
			}
			return null;
		}

		BalloonNotificationType CurrentlyDisplayedBalloon
		{
			get { return Balloon.Instance.IsVisible ? currentlyDisplayedBalloon : BalloonNotificationType.None; }
			set { currentlyDisplayedBalloon = value; }
		}
		BalloonNotificationType currentlyDisplayedBalloon = BalloonNotificationType.None;

		public override void OnMouseHover(CurrencyManager source, int rowNum, Point mouseLocationInCell, Rectangle cellBounds, Control anchor = null)
		{
			var info = GetCurrentInfo(source, rowNum);
			if (info != null && info.HasNotifications() && !IsShowingNotificationBalloon && (mouseLocationInCell.X <= NotificationIconWidth))
			{
				var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(cellBounds.X, cellBounds.Y, NotificationIconWidth, NotificationIconHeight, false);
				ShowBalloon(info, iconRectangle, true, BalloonNotificationType.Notification, anchor);
			}
			else if (IsShowingNotificationBalloon && mouseLocationInCell.X > NotificationIconWidth)
			{
				HideBalloonIfShowing();
			}
			else if (!IsCurrentlyShowingBalloon && rowNum < source?.List?.Count)
			{
				ShowQuickViewCardBalloon(source.List[rowNum] as BusinessObject, cellBounds);
			}
		}

		bool IsCurrentlyShowingBalloon => CurrentlyDisplayedBalloon != BalloonNotificationType.None;
		bool IsShowingNotificationBalloon => CurrentlyDisplayedBalloon == BalloonNotificationType.Notification;

		void ShowQuickViewCardBalloon(BusinessObject bizo, Rectangle cellBounds)
		{
			var quickViewCard = bizo?.QuickViewCard;
			if (!string.IsNullOrEmpty(quickViewCard))
			{
				var descriptor = new BalloonDescriptor(ColumnStyle.Grid, cellBounds, bizo.HumanReadableName, quickViewCard, Enumerable.Empty<INotification>());
				ShowBalloon(descriptor, BalloonNotificationType.QuickViewCard);
			}
		}

		public override void ShowHelpBalloon(CurrencyManager source, int rowNum, Rectangle anchorRect)
		{
			ShowHelpBalloon(GetCurrentInfo(source, rowNum), anchorRect);
		}

		void ShowHelpBalloon(ZPropertyInfo info, Rectangle anchorRect, Control anchor = null)
		{
			if (info != null)
			{
				ShowBalloon(info, anchorRect, false, BalloonNotificationType.Help, anchor);
			}
		}

		void ShowBalloon(ZPropertyInfo info, Rectangle anchorRect, bool hideOnMouseHover, BalloonNotificationType type, Control anchor = null)
		{
			var notifications = info.Notifications;

			var data = GetResourceStringData();
			var caption = HintExtension.MessageForNonDefinedResourceString;
			var description = HintExtension.MessageForNonDefinedResourceString;
			if (data != null)
			{
				caption = data.Caption;
				description = data.FullDescription;
			}
			if (info.HasHumanReadableName)
			{
				caption = info.HumanReadableName.ToString();
			}
			var descriptor = new BalloonDescriptor(anchor ?? ColumnStyle.Grid, anchorRect, caption, description, notifications.GetUniqueNotifications());
			descriptor.HideWhenMouseOverBalloon = hideOnMouseHover;

			ShowBalloon(descriptor, type);
		}

		void ShowBalloon(BalloonDescriptor descriptor, BalloonNotificationType type)
		{
			CurrentlyDisplayedBalloon = type;
#if DEBUG && WINZOR
			ZArchitecture.GUI.Balloons.Balloon.Instance.IsShownDuringTesting = true;
#endif
			Balloon.Instance.Show(descriptor);
		}

		public override void OnMouseLeave(CurrencyManager source, int rowNum)
		{
			base.OnMouseLeave(source, rowNum);
			if (IsShowingNotificationBalloon)
			{
				HideBalloonIfShowing();
			}
		}

		void HideBalloonIfShowing()
		{
			if (IsCurrentlyShowingBalloon)
			{
				CurrentlyDisplayedBalloon = BalloonNotificationType.None;
				Balloon.Instance.Hide();
			}
		}

		IUpdateStatusBar StatusBar
		{
			get
			{
				if (statusBar == null)
				{
					Control control = ColumnStyle.Grid;
					while (control != null && !(control is IUpdateStatusBar))
					{
						control = control.Parent;
					}
					statusBar = control as IUpdateStatusBar;
				}
				return statusBar;
			}
		}
		IUpdateStatusBar statusBar;

		#region Resource Strings

		internal ResourceStringData GetResourceStringData()
		{
			return new ResourceStringKeyCalculator((ZGrid)ColumnStyle.Grid, ColumnStyle.MappingName).DataString;
		}

		#endregion
	}
}
