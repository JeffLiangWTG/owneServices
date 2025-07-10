using System;
using System.Linq;
using System.Windows.Forms;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	public interface IBalloonWindow
	{
		BalloonDescriptor Descriptor { get; }
	}

	public partial class BalloonWindow : Control, IBalloonWindow
	{
		public BalloonWindow()
		{
			ZIndex = 95;
		}

		public BalloonDescriptor Descriptor
		{
			get { return descriptor; }
			set
			{
				if (value != descriptor)
				{
					if (descriptor != null)
					{
						UnhookAnchorControlAndParentFormEvents();
					}

					descriptor = value;
					if (value == null)
					{
						RemoveBalloonFromAnchorControl();
					}
					else
					{
						HideQuickly();
					}

					if (descriptor != null)
					{
						HookAnchorControlAndParentFormEvents();
					}
				}
			}
		}

		void HideQuickly()
		{
			if (Visible)
			{
				Visible = false;
				base.Hide();
			}
		}

		internal bool hasHookedFormDispose;

		void HookAnchorControlAndParentFormEvents()
		{
			if (Descriptor.AnchorControl != null)
			{
				HookTabPageEvents();
			}

			if (Descriptor.ParentForm != null)
			{
				if (!hasHookedFormDispose)
				{
					Descriptor.ParentForm.Disposed += DisposeBalloonOnFormOrControlDispose;
					hasHookedFormDispose = true;
				}

				Descriptor.ParentForm.LocationChanged += DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.VisibleChanged += DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.Deactivate += DisableBalloonOnFormDeactive;
			}
		}

		void UnhookAnchorControlAndParentFormEvents()
		{
			if (Descriptor.AnchorControl != null)
			{
				UnhookTabPageEvents();
			}

			if (Descriptor.ParentForm != null)
			{
				Descriptor.ParentForm.LocationChanged -= DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.VisibleChanged -= DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.Deactivate -= DisableBalloonOnFormDeactive;
			}
		}

		void HookTabPageEvents()
		{
			var tab = FindHostTab(Descriptor.AnchorControl);

			if (tab != null)
			{
				tab.Leave += DisableBalloonOnFormOrControlChange;
			}
		}

		void UnhookTabPageEvents()
		{
			var tab = FindHostTab(Descriptor.AnchorControl);

			if (tab != null)
			{
				tab.Leave -= DisableBalloonOnFormOrControlChange;
			}
		}

		static ZTabPage FindHostTab(Control control)
		{
			while (control.Parent != null)
			{
				if (control.Parent is ZTabPage)
				{
					return (ZTabPage)control.Parent;
				}

				control = control.Parent;
			}

			return null;
		}

		internal void DisableBalloonOnFormOrControlChange(object sender, EventArgs e)
		{
			Descriptor = null;
			HideQuickly();
		}

		internal void DisableBalloonOnFormDeactive(object sender, EventArgs e)
		{
			if (Descriptor?.ParentForm != null && !Descriptor.ParentForm.Disposing)
			{
				Descriptor = null;
				HideQuickly();
			}
		}

		internal void DisposeBalloonOnFormOrControlDispose(object sender, EventArgs e)
		{
			Descriptor = null;
			Dispose();
		}

		BalloonDescriptor descriptor;
		Control container;

		public bool ForceTopOfScreen { get; internal set; }

		public bool TopMost { get; set; }

		public override bool RenderInPortal => true;

		Control AnchorControl { get; set; }

		Control VisibleAnchorControl
		{
			get => visibleAnchorControl;
			set
			{
				visibleAnchorControl = value.Visible ? value : value.FindForm();
			}
		}
		Control visibleAnchorControl;

		Popup PopupRef { get; set; }

		public new void Show()
		{
			var descriptor = Descriptor;
			if (descriptor == null)
			{
				return;
			}

			container = descriptor.AnchorControl.Parent ?? descriptor.AnchorControl;
			if (container is DataGrid)
			{
				container = container.Parent;
			}
			var notificationIcon = container.WinzorSpecificControls.FirstOrDefault(c => c is NotificationIcon icon && icon.NotificationAnchorControl == descriptor.AnchorControl);
			var anchor = notificationIcon ?? descriptor.AnchorControl;
			if (anchor == null)
			{
				return;
			}

			Left = anchor.Left;
			Top = anchor.Top;
			AnchorControl = anchor;
			VisibleAnchorControl = anchor;
			VisibleAnchorControl.WinzorSpecificControls.Add(this);
			VisibleAnchorControl.WinzorSpecificControls.AfterRemove += NotificationIconHidden;
			Visible = true;
		}

		void NotificationIconHidden(Control notificationIcon)
		{
			if (notificationIcon is NotificationIcon)
			{
				RemoveBalloonFromAnchorControl();
				AnchorControl = default;
				if (PopupRef != null)
				{
					InvokeRenderDispatcher(async () =>
					{
						await ExceptionHandlerExtension.HandleJSExceptionAsync(async () => await PopupRef.HideAsync());
					});
				}
			}
		}

		// BalloonWindow inherits Form has no parent in CW1 (it does here for winzor render purpose), override Visible to omit parent as work-around
		public override bool Visible
		{
			get => ControlVisible;
			set => base.Visible = value;
		}

		// BalloonWindow has no width set so we must override to allow it to render.
		protected override bool ShouldRender => Visible;

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			RemoveBalloonFromAnchorControl();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			RemoveBalloonFromAnchorControl();
		}

		void RemoveBalloonFromAnchorControl()
		{
			if (AnchorControl != null)
			{
				VisibleAnchorControl.WinzorSpecificControls.Remove(this);
				VisibleAnchorControl.WinzorSpecificControls.AfterRemove -= NotificationIconHidden;
			}
		}

		#region Test
#if DEBUG
		public Control GetAnchorControlForTest() { return AnchorControl; }

		public Control GetVisibleAnchorControlForTest() { return VisibleAnchorControl; }

#endif
		#endregion
	}
}
