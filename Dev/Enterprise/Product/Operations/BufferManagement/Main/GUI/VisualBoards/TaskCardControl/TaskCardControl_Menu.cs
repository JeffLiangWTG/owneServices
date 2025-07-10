using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	partial class TaskCardControl
	{
		public void SetupMenuItems(KContextMenuStrip menuStrip)
		{
#if DEBUG

			if (AutoGenerateTaskMenuItems.Value)
			{
				ContextMenuStrip = CreateMenuStrip(this).AddItems_ForTest(this);
			}
			else
#endif
			{
				ContextMenuStrip = menuStrip;
			}
		}

		public static LazyContextMenuStrip CreateMenuStrip()
		{
			return new LazyContextMenuStrip(AddItemsToStrip, clearOnClose: true);
		}

		public static LazyContextMenuStrip CreateMenuStrip(Control control)
		{
			var strip = CreateMenuStrip();

			void DisposedHandler(object s, EventArgs e)
			{
				if (!strip.IsDisposed)
				{
					strip.Dispose();
				}
				control.Disposed -= DisposedHandler;
			}

			control.Disposed += DisposedHandler;

			return strip;
		}

		static void AddItemsToStrip(KContextMenuStrip strip, Control control)
		{
			var taskCardControl = (TaskCardControl)control;
			CardLoadResult result;

			if (taskCardControl.TryLoadCardBizosDiscardingCardOnFailure(out result, new BusinessObjectFactory { NameForDebugging = "TaskCardControl.MenuItems" }))
			{
				TaskCardMenuActionProvider.AddMenuItems(taskCardControl, taskCardControl.CardContent, result, taskCardControl.viewModel, strip);
			}
		}

#if DEBUG

		public static readonly LazyOverridable<bool> AutoGenerateTaskMenuItems = new LazyOverridable<bool>(() => false);

		public void TaskCardContextMenuStrip_OnOpening_ForTest()
		{
			var lazyContextMenuStrip = ContextMenuStrip as LazyContextMenuStrip;
			if (lazyContextMenuStrip != null)
			{
				lazyContextMenuStrip.AddItems_ForTest(this);
			}
			else
			{
				throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Task card control initialised with unexpected menu item type [{0}]", lazyContextMenuStrip.GetType().Name));
			}
		}

		public bool ShouldPreloadSubMenus_ForTest { get; set; }

#endif
	}
}
