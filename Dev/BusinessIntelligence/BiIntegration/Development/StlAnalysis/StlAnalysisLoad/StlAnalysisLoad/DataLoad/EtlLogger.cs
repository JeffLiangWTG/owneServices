namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.ComponentModel;
	using System.Threading;

	#region DwLoadEvent

	public delegate void DwLoadEvent(string message);

	interface IEtlLogger
	{
		void StartTask(string task);
		void StartSubtask(string subtask);
		void ShowError(string error, Exception ex);
	}

	#endregion

	class EtlLogger : IEtlLogger
	{
		public event DwLoadEvent OnTaskStarted;
		public event DwLoadEvent OnSubtaskStarted;
		public event DwLoadEvent OnError;

		public ISynchronizeInvoke SyncInvoke
		{
			get { return syncInvoke; }
			set { syncInvoke = value; }
		}
		ISynchronizeInvoke syncInvoke;

		void IEtlLogger.StartTask(string task)
		{
			RaiseDwLoadEvent(OnTaskStarted, task);
		}

		void IEtlLogger.StartSubtask(string subtask)
		{
			RaiseDwLoadEvent(OnSubtaskStarted, subtask);
		}

		void IEtlLogger.ShowError(string error, Exception ex)
		{
			if (!(ex is ThreadAbortException))
			{
				RaiseDwLoadEvent(OnError, error);
			}
		}

		void RaiseDwLoadEvent(DwLoadEvent eventToRaise, string message)
		{
			if (eventToRaise != null)
			{
				if (syncInvoke == null)
				{
					eventToRaise(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new DwLoadEvent(eventToRaise), new object[] { message });
				}
			}
		}
	}
}
