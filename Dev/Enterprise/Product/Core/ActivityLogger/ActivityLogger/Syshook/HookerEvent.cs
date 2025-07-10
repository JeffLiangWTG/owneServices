using System;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ActivityLogger
{
	internal class HookerEvent
	{
		[ThreadSafe]
		public static readonly HookerEvent Timeout = new HookerEvent { EventType = SyshookInterop.HookerEventType.TimeOut };
		[ThreadSafe]
		public static readonly HookerEvent Unknown = new HookerEvent { EventType = SyshookInterop.HookerEventType.Unknown };

		public HookerEvent()
		{ }

		public HookerEvent(HookMessage hMsg)
		{
			hookMessage = hMsg;

			switch (hookMessage.hookId)
			{
				case SyshookInterop.WH_MOUSE:
					EventType = SyshookInterop.HookerEventType.Mouse;
					break;
				case SyshookInterop.WH_KEYBOARD:
					EventType = SyshookInterop.HookerEventType.Keyboard;
					break;
				case SyshookInterop.WH_CBT:
					EventType = SyshookInterop.HookerEventType.Cbt;
					break;
				default:
					EventType = SyshookInterop.HookerEventType.Unknown;
					break;
			}
		}

		internal HookMessage hookMessage;

		public Int32 ProcessId => hookMessage.dwProcessId;
		public SyshookInterop.HookerEventType EventType { get; internal set; } = SyshookInterop.HookerEventType.Unknown;
		public Int32 NCode => hookMessage.nCode;
		public IntPtr WParam => hookMessage.wParam;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public IntPtr LParam => hookMessage.lParam;
	}
}
