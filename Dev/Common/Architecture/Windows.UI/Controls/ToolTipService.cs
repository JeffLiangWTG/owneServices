using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	public static class ToolTipService
	{
		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a ZButton")]
		public static void SetToolTip(Control control, string caption, int initialDelayMillis = 100)
		{
			if (!control.IsDisposed && control.IsCallingFromOwnedThread())
			{
				var tooltip = GetTooltip(control, initialDelayMillis);
				tooltip.SetToolTip(control, caption);
			}
		}

		public static void ShowToolTip(Control control, string caption, Point point, int duration = 0, int initialDelayMillis = 100)
		{
			if (!control.IsDisposed && control.IsCallingFromOwnedThread())
			{
				var tooltip = GetTooltip(control, initialDelayMillis);
				if (duration <= 0)
				{
					tooltip.Show(caption, control, point);
				}
				else
				{
					tooltip.Show(caption, control, point, duration);
				}
			}
		}

		public static void HideToolTip(Control control)
		{
			if (!control.IsDisposed && control.IsCallingFromOwnedThread())
			{
				if (Cache.TryGetValue(control.GetHashCode(), out ToolTipImpl toolTip))
				{
					toolTip.Hide(control);
				}
			}
		}

		public static void ClearTooltip(Control control)
		{
			if (control.IsCallingFromOwnedThread())
			{
				var key = control.GetHashCode();
				var tooltip = Cache.ContainsKey(key) ? Cache[key] : null;
				if (tooltip != null)
				{
					Dispose(control);
				}
			}
		}

		public static bool HasToolTip(Control c)
		{
			return !string.IsNullOrEmpty(GetToolTip(c));
		}

		public static string GetToolTip(Control control)
		{
			if (control.IsCallingFromOwnedThread())
			{
				if (Cache.TryGetValue(control.GetHashCode(), out ToolTipImpl toolTip))
				{
					return toolTip.GetToolTip(control);
				}
			}
			return string.Empty;
		}

		#region Implementation

		static ToolTipImpl GetTooltip(Control control, int initialDelayMillis)
		{
			ToolTipImpl tooltip;
			var key = control.GetHashCode();

			if (Cache.ContainsKey(key))
			{
				tooltip = Cache[key];
			}
			else
			{
				tooltip = new ToolTipImpl { InitialDelay = initialDelayMillis };
				control.Disposed += Control_Disposed;
				Cache[key] = tooltip;
			}

			return tooltip;
		}

		static void Control_Disposed(object sender, EventArgs e)
		{
			Dispose(sender);
		}

		static void Dispose(object control)
		{
			var key = control.GetHashCode();
			if (Cache.ContainsKey(key))
			{
				var tooltip = Cache[key];
				tooltip.Dispose();
				Cache.Remove(key);
			}
		}

		[ThreadSafe]
		static IDictionary<int, ToolTipImpl> cache;
		internal static IDictionary<int, ToolTipImpl> Cache
		{
			get
			{
				return cache ?? (cache = new ConcurrentDictionary<int, ToolTipImpl>());
			}
		}

		#endregion

		#region ToolTipImpl

		internal class ToolTipImpl : ToolTip
		{
			internal ToolTipImpl()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if (disposing)
				{
					IsDisposed = true;
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}

			internal bool IsDisposed { get; private set; }
		}

		#endregion
	}
}
