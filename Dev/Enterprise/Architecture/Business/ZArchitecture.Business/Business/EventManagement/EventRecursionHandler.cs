using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Hello people in the future.
	/// This class is meant for wrapping around atomic operations involving events.
	/// For example, deleting or adding an event will likely cause other events to be added and deleted.
	/// 
	/// By wrapping the delete/add operation in this class you are ensuring triggers will not fire multiple times, 
	/// even if there is a recursive propagation/cascade relationship that would otherwise cause that.
	/// 
	/// This is not the ideal solution to managing this issue, but it is the solution that doesn't require rewriting a lot of code.
	/// </summary>
	public sealed class EventRecursionHandler
	{
		#region Static Members

		public static IDisposable WithEventRecursionDetection(EventRecursionHandler handler = null)
		{
			if (threadStaticInstance == null)
			{
				threadStaticInstance = handler ?? new EventRecursionHandler();
				return new DisposableAction(() => threadStaticInstance = null);
			}
			else
			{
				return null;
			}
		}

		public static EventRecursionHandler Instance
		{
			get
			{
				var instance = threadStaticInstance;
				if (instance == null)
				{
					ErrorReporter.ReportOnce("aa87b5af-291e-4c83-87b6-3ac6693516f7", "Raising events without recursion handling.");
					return new EventRecursionHandler();
				}
				else
				{
					return instance;
				}
			}
		}

		[ThreadStatic]
		static EventRecursionHandler threadStaticInstance;

		#endregion

		#region API

		internal bool IsBlockingDuplicateFire(ZGuid identifier, ZDateTimeOffset value) => !firedTriggers.Add((identifier, value.Ticks));

		internal bool IsDuplicateCascade(ZGuid identifier, string eventCode) => !cascadedEvents.Add((identifier, eventCode));

		internal bool IsDuplicatePropagation(ZGuid identifier, string eventCode) => !propagatedEvents.Add((identifier, eventCode));

		readonly HashSet<(ZGuid, long)> firedTriggers = new HashSet<(ZGuid, long)>();
		readonly HashSet<(ZGuid, string)> cascadedEvents = new HashSet<(ZGuid, string)>();
		readonly HashSet<(ZGuid, string)> propagatedEvents = new HashSet<(ZGuid, string)>();

		#endregion
	}
}
