using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms.Internal
{
	public class MenuItemClickTracker : Disposable
	{
		MenuItemClickTracker()
		{
			shouldProcessCommandInvoke = true;
		}

		public bool ShouldProcessCommandInvoke
		{
			get { return shouldProcessCommandInvoke; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (menuItemNeedsPopping)
				{
					MenuItemsCurrentlyExecuting.Remove(menuItemData);
				}
			}
		}

		object menuItemData;
		bool menuItemNeedsPopping;
		bool shouldProcessCommandInvoke;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public static MenuItemClickTracker TrackClick(Message m, Control parentControl)
		{
			MenuItemClickTracker tracker = null;

			if (m.Msg == WindowsMessage.WM_COMMAND && m.LParam == IntPtr.Zero)
			{
				try
				{
					var menuItemData = GetMenuItemData(m.WParam);
					if (menuItemData != null)
					{
						tracker = new MenuItemClickTracker();
						tracker.menuItemData = menuItemData;

						var caption = GetMenuCaption(menuItemData);
						if (MenuItemsCurrentlyExecuting.Contains(menuItemData))
						{
							tracker.shouldProcessCommandInvoke = false;
							UserEventTracker.Instance.AddUserEvent(parentControl, "MenuClick (re-entrant, ignored)", caption);
						}
						else
						{
							MenuItemsCurrentlyExecuting.Add(menuItemData);
							tracker.menuItemNeedsPopping = true;

							UserEventTracker.Instance.AddUserEvent(parentControl, "MenuClick", caption);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowDeveloperExceptionOnce("WndProcUsageAnalysis", "Exception in heavy reflection", ex);
				}
			}

			return tracker;
		}

		static List<object> MenuItemsCurrentlyExecuting
		{
			get { return menuItemsCurrentlyExecuting ?? (menuItemsCurrentlyExecuting = new List<object>()); }
		}
		[ThreadStatic]
		static List<object> menuItemsCurrentlyExecuting;

		static object GetMenuItemData(IntPtr wParam)
		{
			var weakReference = (WeakReference)MenuCaptionReflection.GetComandFromIDInfo.Invoke(null, new object[] { LOWORD(wParam) });
			return (weakReference != null) ? weakReference.Target : null;
		}

		static string GetMenuCaption(object menuItemData)
		{
			return (string)MenuCaptionReflection.CaptionInfo.GetValue(menuItemData);
		}

		#region Lifted from System.Windows.Forms.Form - blame Lutz

		static int LOWORD(IntPtr n)
		{
			return LOWORD((int)((long)n));
		}

		static int LOWORD(int n)
		{
			return (n & 0xffff);
		}

		#endregion

		static class MenuCaptionReflection
		{
			public static MethodInfo GetComandFromIDInfo
			{
				get
				{
					if (getComandFromIDInfo == null)
					{
						getComandFromIDInfo = CommandType.GetMethod("GetCommandFromID", BindingFlags.Static | BindingFlags.Public);
					}
					return getComandFromIDInfo;
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant field names")]
			public static FieldInfo CaptionInfo
			{
				get
				{
					if (captionInfo == null)
					{
#if WINZOR
						throw new NotImplementedException("Winzor does not seem to have an equivalent field.");
#elif NET
						const string fieldName = "_caption";
#else
						const string fieldName = "caption";
#endif
						captionInfo = MenuItemDataType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
					}
					return captionInfo;
				}
			}

			static Type CommandType
			{
				get
				{
					if (commandType == null)
					{
						commandType = typeof(Control).Assembly.GetType("System.Windows.Forms.Command");
					}
					return commandType;
				}
			}

			static Type MenuItemDataType
			{
				get
				{
					if (menuItemDataType == null)
					{
						menuItemDataType = typeof(MenuItem).GetNestedType("MenuItemData", BindingFlags.NonPublic);
					}
					return menuItemDataType;
				}
			}

			[SuppressThreadStaticFieldMessage]
			static FieldInfo captionInfo;
			[SuppressThreadStaticFieldMessage]
			static MethodInfo getComandFromIDInfo;
			[SuppressThreadStaticFieldMessage]
			static Type commandType;
			[SuppressThreadStaticFieldMessage]
			static Type menuItemDataType;
		}
	}
}
