using System;
using System.ComponentModel;
using Enterprise.Integration;

namespace Enterprise.Billing.StlCollector.Retriever
{
	#region StlRetrieverLogger

	public delegate void StlRetrieverEvent(string message);

	public interface IUserAttendedStlRetrieverLogger : ILogger
	{
		void InitProgress(string count);
		void TaskProgress(string message);
		void TaskInfo(string message);
		void TaskFailed(string message);
	}

	#endregion

	class UserAttendedStlRetrieverLogger : IUserAttendedStlRetrieverLogger
	{
		public event StlRetrieverEvent OnInitProgress;
		public event StlRetrieverEvent OnTaskProgress;
		public event StlRetrieverEvent OnTaskInfo;
		public event StlRetrieverEvent OnTaskFailed;

		public ISynchronizeInvoke SyncInvoke
		{
			get { return syncInvoke; }
			set { syncInvoke = value; }
		}
		ISynchronizeInvoke syncInvoke;

		#region IUserAttendedStlRetrieverLogger members

		void IUserAttendedStlRetrieverLogger.InitProgress(string count)
		{
			RaiseEvent(OnInitProgress, count);
		}

		void IUserAttendedStlRetrieverLogger.TaskProgress(string message)
		{
			RaiseEvent(OnTaskProgress, message);
		}

		void IUserAttendedStlRetrieverLogger.TaskInfo(string message)
		{
			RaiseEvent(OnTaskInfo, message);
		}

		void IUserAttendedStlRetrieverLogger.TaskFailed(string message)
		{
			RaiseEvent(OnTaskFailed, message);
		}

		#endregion

		#region ILogger members

		void ILogger.Log(LogType type, string message)
		{
			if (type == LogType.Error)
			{
				(this as IUserAttendedStlRetrieverLogger).TaskFailed(message);
			}
			else
			{
				(this as IUserAttendedStlRetrieverLogger).TaskInfo(message);
			}
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			string errorStack = (ex == null) ? "" : ("\r\n" + ex.ToString());
			(this as IUserAttendedStlRetrieverLogger).TaskFailed(message + errorStack);
		}

		#endregion

		void RaiseEvent(StlRetrieverEvent eventToRaise, string message = null)
		{
			if (eventToRaise != null)
			{
				if (syncInvoke == null)
				{
					eventToRaise(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new StlRetrieverEvent(eventToRaise), new object[] { message });
				}
			}
		}
	}
}
