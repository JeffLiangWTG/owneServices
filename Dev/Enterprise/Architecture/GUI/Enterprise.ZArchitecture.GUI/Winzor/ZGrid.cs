using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Extensions;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture
{
	public partial class ZGrid : DataGrid
	{
		protected override Color DragMaskColor => SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;

		protected override bool GridLayoutConfigurable => IsGridLayoutConfigurable;

		public override bool WholeRowSelectedOnClick => IsWholeRowSelectedOnClick;

		protected override bool SelectableByTabKey => false;

		internal new void InvalidateRow(int rowIndex) => base.InvalidateRow(rowIndex);

		void UpdateGridNotificationIcon(INotificationType notificationType)
		{
			UpdateIcon(NotificationIcon, notificationType, () => OnGridNotificationMouseOver());
			NotifyRenderRequired();
		}

		protected override void UpdateRowNotification(int rowIndex, NotificationIcon icon)
		{
			var bo = ListForNotifications != null && rowIndex < ListForNotifications.Count ? ListForNotifications?[rowIndex] as BusinessObject : null;

			UpdateIcon(icon, bo?.GetHighestSeverityNotificationType(), () => OnRowNotificationIconMouseOver(bo, icon));
		}

		void UpdateIcon(NotificationIcon icon, INotificationType notificationType, Action mouseOver)
		{
			if (icon != null)
			{
				icon.NotificationType = notificationType?.EnumValueName?.ToLowerHyphen();
				icon.OnMouseOver = (s, e) => mouseOver();
				icon.OnMouseOut = OnRowNotificationIconMouseLeave;
			}
		}

		void OnGridNotificationMouseOver()
		{
			NotificationRectForBalloon = Rectangle.Empty;
#if DEBUG
			ZArchitecture.GUI.Balloons.Balloon.Instance.IsShownDuringTesting = true;
#endif
			ShowGridCornerBalloon();
		}

		void OnRowNotificationIconMouseOver(BusinessObject bo, Control anchor)
		{
			NotificationRectForBalloon = Rectangle.Empty;
#if DEBUG
			ZArchitecture.GUI.Balloons.Balloon.Instance.IsShownDuringTesting = true;
#endif
			ShowGridColumnBalloon(bo, Rectangle.Empty, anchor);
		}

		void OnRowNotificationIconMouseLeave(object sender, MouseEventArgs e)
		{
			Balloon.Instance.Hide();
		}

		protected override async Task OnMouseDownAsync(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
		{
			await base.OnMouseDownAsync(e, hitTest, elementReference);
			switch (hitTest.Type)
			{
				case HitTestType.ColumnHeader:
					await HitColumnHeaderHandler(e, hitTest, elementReference);
					break;
				case HitTestType.RowHeader:
					await HitColumnRowHeaderHandler(e);
					break;
				default:
					break;
			}
		}

		async Task HitColumnHeaderHandler(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
		{
			if (e.GetMouseButtons() == MouseButtons.Left)
			{
				if (elementReference is ElementReference columnElementReference)
				{
					await (GetJSInterop<IGridJSInterop>()?.ReorderColumnAsync(e, columnElementReference, DragMaskColor) ?? Task.CompletedTask);
				}
				if (hitTest.Column == -1 && hitTest.Row == -1 && DataSource != null)
				{
					await InvokeWinzorDispatcherAsync(() => CustomiseColumns());
				}
			}
		}

		async Task HitColumnRowHeaderHandler(WebMouseEventArgs e)
		{
			if (e.GetMouseButtons() == MouseButtons.Left)
			{
				await InvokeWinzorDispatcherAsync(() => DoDragDrop());
			}
		}

		internal void MenuCopy()
		{
			if (CopySelectedRowsAllowed)
			{
				CopySelectedRows();
			}
			else
			{
				ShowCannotCopyMessage();
			}
		}

		void CheckMouseDownOnExteriorWhileDropEditOpen(HitTestInfo hitInfo, MouseEventArgs e)
		{
			var dropEditControlVisible = LastFocusedColumn?.EditControl is not null && LastFocusedColumn.EditControl is ZDropEdit && LastFocusedColumn.EditControl.Visible;
			if (hitInfo.Type != HitTestType.Cell && e.Clicks == 1 && dropEditControlVisible)
			{
				mouseDownOnExteriorWhileDropEditOpen = true;
			}
		}
	}
}
