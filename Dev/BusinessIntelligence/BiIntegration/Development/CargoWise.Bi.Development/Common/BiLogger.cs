using System;
using CargoWise.Bi.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Development.Common
{
	public delegate void BiLoggerEvent(string message, DateTime time);

	public class BiLogger : IBiLogger
	{
		#region Instance and Events

		public static BiLogger Instance
		{
			get { return logger ?? (logger = new BiLogger()); }
		}
		[ThreadSafe]
		static BiLogger logger;

		public event BiLoggerEvent OnCompleted;
		public event BiLoggerEvent OnFailed;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event BiLoggerEvent OnStartTask;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event BiLoggerEvent OnStartSubtask;

		#region Static Instance Shortcuts

		public static void Complete(string message = null)
		{
			(Instance as IBiLogger).Complete(message);
		}

		public static void Fail(string message)
		{
			(Instance as IBiLogger).Fail(message);
		}

		public static void Fail(Exception ex)
		{
			(Instance as IBiLogger).Fail(ex);
		}

		public static void Fail(string message, Exception ex)
		{
			(Instance as IBiLogger).Fail(message, ex);
		}

		public static void StartTask(string message)
		{
			(Instance as IBiLogger).StartTask(message);
		}

		public static void StartSubtask(string message)
		{
			(Instance as IBiLogger).StartSubtask(message);
		}

		#endregion

		#endregion

		#region IBiLogger members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.Complete(string message)
		{
			Instance.CompleteMessage(message, DateTime.Now); // development tool only
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.Fail(Exception ex)
		{
			Instance.FailMessage(ex.Message, DateTime.Now); // development tool only
			Instance.FailMessage(ex.StackTrace, DateTime.Now); // development tool only

			if (ex.InnerException != null)
			{
				Fail(ex.InnerException);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.Fail(string message)
		{
			Instance.FailMessage(message, DateTime.Now); // development tool only
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.Fail(string message, Exception ex)
		{
			Instance.FailMessage(message, DateTime.Now); // development tool only
			Fail(ex);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.StartTask(string message)
		{
			Instance.StartTask(message, DateTime.Now); // development tool only
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "development tool only")]
		void IBiLogger.StartSubtask(string message)
		{
			Instance.StartSubtask(message, DateTime.Now); // development tool only
		}

		#endregion

		#region Implementation

		void CompleteMessage(string message, DateTime time)
		{
			RaiseEvent(OnCompleted, message, time);
		}

		void FailMessage(string message, DateTime time)
		{
			RaiseEvent(OnFailed, message, time);
		}

		void StartTask(string message, DateTime time)
		{
			RaiseEvent(OnStartTask, message, time);
		}

		void StartSubtask(string message, DateTime time)
		{
			RaiseEvent(OnStartSubtask, message, time);
		}

		void RaiseEvent(BiLoggerEvent eventToRaise, string message, DateTime time)
		{
			if (eventToRaise != null)
			{
				eventToRaise(message, time);
			}
		}

		#endregion
	}
}
