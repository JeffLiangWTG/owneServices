using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Tools
{
	public sealed class SqlCommandsManager : NonPersistentBusinessObject, IDisposable
	{
		public SqlCommandsManager()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public SqlCommandsManager(TaskScheduler uiTaskScheduler)
			: this()
		{
			this.UITaskScheduler = uiTaskScheduler;
			State = SqlProfilerState.Stopped;
		}

		#region Implement

		public DbCommandWrapperCollection CommandList
		{
			get { return commandList ?? (commandList = new DbCommandWrapperCollection()); }
		}
		DbCommandWrapperCollection commandList;

		public void Clear()
		{
			lock (CommandList)
			{
				CommandList.RemoveAll();
			}
		}

		public void Start()
		{
			if (State == SqlProfilerState.Stopped)
			{
				Clear();
				HookEvent();
			}

			State = SqlProfilerState.Running;
		}

		public void Pause()
		{
			State = SqlProfilerState.Suspended;
		}

		public void Stop()
		{
			State = SqlProfilerState.Stopped;
			UnHookEvent();
		}

		public SqlProfilerState State
		{
			get;
			private set;
		}

		public void Dispose()
		{
			Stop();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		bool CanAcceptCommand
		{
			get { return State == SqlProfilerState.Running; }
		}

		readonly TaskScheduler UITaskScheduler;
		const int MaxLength = 1000;

		void AddSqlCommandsEvent(SqlCommandExecutedEventArgs args)
		{
			Task.Factory.StartNew(
				() => AddSqlCommandsEventCore(args), CancellationToken.None, TaskCreationOptions.None
				, UITaskScheduler
				);
		}

#if DEBUG
		internal
#endif
		void AddSqlCommandsEventCore(SqlCommandExecutedEventArgs args)
		{
			if (CanAcceptCommand)
			{
				var command = new DbCommandWrapper(args);
				CommandList.Add(command);

				if (CommandList.Count > MaxLength)
				{
					((IList)CommandList).RemoveAt(0);
				}
			}
		}

		void HookEvent()
		{
			SqlEventTracker.Instance.SqlCommandExecutedEvent += AddSqlCommandsEvent;
		}

		void UnHookEvent()
		{
			SqlEventTracker.Instance.SqlCommandExecutedEvent -= AddSqlCommandsEvent;
		}
	}
}
