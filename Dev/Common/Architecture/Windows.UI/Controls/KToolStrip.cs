using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI.Controls.Internal;
using Microsoft.Win32;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KToolStrip : ToolStrip
	{
		public KToolStrip() : base() { }

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (IsTrackingEnabled)
			{
				Tracker.TrackInstanceForLeak(this);
			}
		}

		protected override void Dispose(bool disposing)
		{
			disposingStarted = true;

			var toolStripOverflowButton = typeof(ToolStrip).GetField("toolStripOverflowButton", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(this) as ToolStripOverflowButton;
			var items = Items;

			//fixing memory leak of WI00228602 by unhooking ToolStripOverflow from SystemEvents.UserPreferenceChanged
			for (var i = 0; i < items.Count; ++i)
			{
				if (items[i] is ToolStripDropDownItem item)
				{
					var dropDown = typeof(ToolStripDropDownItem).GetField("dropDown", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(item);
					if (dropDown != null)
					{
						UnhookSystemEventUserPreferenceChangedEvent.UnhookObject(dropDown);
					}
				}
			}

			base.Dispose(disposing);

			//fixing memory leak of WI00228602 by severing link between ToolStripOverflowButton and KToolStrip
			if (toolStripOverflowButton != null)
			{
				typeof(ToolStripItem).GetField("parent", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(toolStripOverflowButton, null);
				typeof(ToolStripOverflowButton).GetField("parentToolStrip", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(toolStripOverflowButton, null);
			}

			if (disposing)
			{
				if (IsTrackingEnabled)
				{
					Tracker.StopTrackingInstanceForLeak(this);
				}
			}
		}

		bool disposingStarted;

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (disposingStarted)
			{
				return null;
			}
			return base.CreateAccessibilityInstance();
		}

		static ControlTracker Tracker
		{
			get { return tracker ?? (tracker = new ControlTracker()); }
		}
		[ThreadStatic]
		static ControlTracker tracker;

		public static bool IsTrackingEnabled { get => isTrackingEnabled; set => isTrackingEnabled = value; }

		[ThreadSafe]
		static bool isTrackingEnabled;

		// Inspired by http://hacking-code.blogspot.com/2010/08/unhooking-leaking-object-from.html
		public static class UnhookSystemEventUserPreferenceChangedEvent
		{
			[ThreadSafe] // Locked using the eventLockObject
			static IList userPreferenceChangedListCache;

			[ThreadSafe]
			static object eventLockObject;

			static object GetEventLock()
				=> typeof(SystemEvents).GetField("eventLockObject", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

			public static void UnhookObject(object pObjectToUnhook)
			{
				foreach (var itemToRemove in GetHandlersForTarget(pObjectToUnhook))
				{
					SystemEvents.UserPreferenceChanged -= itemToRemove;
				}
			}

			static ICollection<UserPreferenceChangedEventHandler> GetHandlersForTarget(object pObjectToUnhook)
			{
				// The userPreferencesChangedList is a static global variable living within SystemEvents
				// We need to share the same lock with it
				lock (LazyInitializer.EnsureInitialized(ref eventLockObject, GetEventLock))
				{
					var userPreferenceChangedList = LazyInitializer.EnsureInitialized(ref userPreferenceChangedListCache, GetUserPreferenceChangedList);

					return userPreferenceChangedList?.Cast<object>()
						.Select(GetDelegateFromSystemEventInvokeInfo)
						.OfType<UserPreferenceChangedEventHandler>()
						.Where(d => d.Target == pObjectToUnhook)
						.ToList() ?? new List<UserPreferenceChangedEventHandler>();
				}
			}

			static IList GetUserPreferenceChangedList()
			{
				// Using reflection, get the FieldInfo object for the internal collection of handlers (which has the type Dictionary<object, List<SystemEventInvokeInfo>>). We will use this collection to find the handler we want to unhook and remove it.
				var dictFieldInfoValue = (IDictionary)typeof(SystemEvents)
					.GetField("_handlers", BindingFlags.Static | BindingFlags.GetField | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
					.GetValue(null);

				return dictFieldInfoValue
					.Values
					.OfType<IList>()
					.FirstOrDefault(IsListenerListForUserPreferenceChangedEvent);
			}

			static bool IsListenerListForUserPreferenceChangedEvent(IList eventHandlers)
			{
				var firstHandler = eventHandlers.Cast<object>().FirstOrDefault();

				return firstHandler != null && GetDelegateFromSystemEventInvokeInfo(firstHandler) is UserPreferenceChangedEventHandler;
			}

			static Delegate GetDelegateFromSystemEventInvokeInfo(object pSystemEventInvokeInfo)
			{
				return pSystemEventInvokeInfo.GetType()
					.GetField("_delegate", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
					.GetValue(pSystemEventInvokeInfo) as Delegate;
			}
		}
	}
}
