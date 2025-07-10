using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinzorFramework;
using WinzorFramework.Extensions;
using ThreadTimer = System.Threading.Timer;

// This code was copied and modified from the Shared.40 repository
// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared?path=%2FShared.40%2FShared.40%2FProcessStatusFormManager.cs&_a=contents&version=GBmaster

namespace Enterprise.ZArchitecture.GUI
{
	public static class ProcessStatusFormManager
	{
		public const string ThreadName = "ProcessStatusFormManager";

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		public static WeakReference<Thread> mostRecentThread_DebugOnly = null;
	}

	public class ProcessStatusFormManager<TForm> : IDisposable where TForm : Form, CargoWise.IO.IProcessStatus, new()
	{
		public ProcessStatusFormManager(ThreadExceptionEventHandler threadExceptionHandler)
		{
			this.threadExceptionHandler = threadExceptionHandler;
		}

		record StartThreadArgs(IFormOpener parentFormOpener, IFormInstanceRegister parentFormInstanceRegister);

		public void Start()
		{
			parentWinzorDispatcherContext = WinzorDispatcher.Current.CurrentContext;
			var args = new StartThreadArgs(WinzorDispatcher.Current.FormOpener, WinzorDispatcher.Current.FormInstanceRegister);

			if (InitialDelay == TimeSpan.Zero)
			{
				StartThread(args);
			}
			else
			{
				startTimer = new ThreadTimer(StartThread, args, InitialDelay, TimeSpan.FromMilliseconds(Timeout.Infinite));
			}
		}

		void StartThread(object state)
		{
			startedEvent.Reset();
			if (!IsDisposed)
			{
				var args = (StartThreadArgs)state;
				var formOpener = args.parentFormOpener;
				var formRegister = args.parentFormInstanceRegister;
				winzorDispatcher = new WinzorDispatcher(formOpener, formRegister, ProcessStatusFormManager.ThreadName, isBackgroundThread: true, setSynchronizationContext: false);
				dispatcherTask = winzorDispatcher.InvokeAsync(Run);
			}
		}

		protected IDisposable disposableActionForDbConnection;

		[SuppressMessage("CargoWiseOne", "CW1067:Application ThreadException Rule", Justification = "<Pending>")]
		void Run()
		{
			using var winzorDispatcherWithContext = winzorDispatcher.WithContext(new NewThreadWinzorDispatcherContext(parentWinzorDispatcherContext));
			Application.ThreadException += Application_ThreadException;
			try
			{
				while (!IsDisposed)
				{
					showFormEvent.WaitOne();
					if (!IsDisposed)
					{
						form = CreateForm();
#if DEBUG
						ProcessStatusFormManager.mostRecentThread_DebugOnly = new WeakReference<Thread>(Thread.CurrentThread);
#endif
						try
						{
							formMonitorCancellationTokenSource = new CancellationTokenSource();
							formMonitorTask = FormMonitorAsync(formMonitorCancellationTokenSource.Token);
							startedEvent.Set();
							form.FormClosed += Form_FormClosed;
							form.ShowDialog();
						}
						finally
						{
							formMonitorCancellationTokenSource?.Cancel();
						}
					}

					if (!IsDisposed)
					{
						startedEvent.Reset();
					}
				}
			}
			catch (Exception ex)
			{
				Application_ThreadException(null, new ThreadExceptionEventArgs(ex));
			}
			finally
			{
				startedEvent.Set();

				if (disposableActionForDbConnection != null)
				{
					disposableActionForDbConnection.Dispose();
				}

				Application.ThreadException -= Application_ThreadException;
			}
		}

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			if (e.Exception is ObjectDisposedException && IsDisposed)
			{
				return;
			}

			threadExceptionHandler?.Invoke(sender, e);
		}

		void Form_FormClosed(object sender, EventArgs e)
		{
			if (form != null)
			{
				form.FormClosed -= Form_FormClosed;
			}

			DoDisposeForm();
		}

		async Task FormMonitorAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
				{
					await Task.Delay(TimeSpan.FromMilliseconds(formTimerIntervalInMs), cancellationToken);

					await form.InvokeWinzorDispatcherAsync(() =>
					{
						try
						{
							if (IsDisposed)
							{
								DoDisposeForm();
							}
							else if (!showFormEvent.WaitOne(0))
							{
								DoDisposeForm();
							}
							else
							{
								var nextState = progressState;
								if (!nextState.Equals(lastUpdatedProgressState))
								{
									lastUpdatedProgressState = nextState;
									form.UpdateStatus(nextState.Status, nextState.ProgressValue);
								}

								while (actionsToExecuteOnForm.TryDequeue(out var action))
								{
									action(form);
								}
							}
						}
						catch
						{
							if (form != null && !form.IsDisposed)
							{
								DoDisposeForm();
							}

							throw;
						}
					});
				});
			}
		}

		void DoDisposeForm()
		{
			if (form != null)
			{
				if (form.InvokeRequired)
				{
					form.BeginInvoke(new MethodInvoker(Dispose));
				}
				else
				{
					form.Dispose();
				}
			}

			lastUpdatedProgressState = new ProgressState(string.Empty, 0);
		}

		public void UpdateStatus(string status, int progressValue)
		{
			progressState = new ProgressState(status, progressValue);
		}

		public void InvokeOnForm(Action<TForm> action)
		{
			actionsToExecuteOnForm.Enqueue(action);
		}

		public void HideForm()
		{
			showFormEvent.Reset();
		}

		public void ShowForm()
		{
			if (!IsDisposed)
			{
				showFormEvent.Set();
			}
		}

		protected virtual TForm CreateForm()
		{
			return new TForm();
		}

		public TimeSpan InitialDelay
		{
			get;
			set;
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		public string Status
		{
			get { return progressState.Status; }
		}

		public int ProgressValue
		{
			get { return progressState.ProgressValue; }
		}

		public void Dispose()
		{
			if (!IsDisposed)
			{
				IsDisposed = true;
				startTimer?.Dispose();
				showFormEvent.Set();
				startedEvent.WaitOne();
				DisposeCore();

				var tasks = Task.WhenAll(dispatcherTask ?? Task.CompletedTask, formMonitorTask ?? Task.CompletedTask);
				if (tasks.Wait(FormDisposeTimerIntervalInMs))
				{
					winzorDispatcher?.Dispose();
				}
				else
				{
					tasks.ContinueWith(t => winzorDispatcher?.Dispose());
				}
			}
		}

		protected virtual void DisposeCore()
		{
		}

		readonly ThreadExceptionEventHandler threadExceptionHandler;
		IWinzorDispatcherContext parentWinzorDispatcherContext;
		WinzorDispatcher winzorDispatcher;
		CancellationTokenSource formMonitorCancellationTokenSource;
		protected Task dispatcherTask;
		Task formMonitorTask;
		ThreadTimer startTimer;
		TForm form;
		protected int formTimerIntervalInMs = 250;
		protected virtual int FormDisposeTimerIntervalInMs => formTimerIntervalInMs;
		ProgressState progressState = new ProgressState(string.Empty, 0);
		ProgressState lastUpdatedProgressState = new ProgressState(string.Empty, 0);
		readonly EventWaitHandle showFormEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
		readonly EventWaitHandle startedEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
		readonly ConcurrentQueue<Action<TForm>> actionsToExecuteOnForm = new ConcurrentQueue<Action<TForm>>();

		class ProgressState
		{
			public ProgressState(string status, int progressValue)
			{
				Status = status;
				ProgressValue = progressValue;
			}

			public string Status { get; }
			public int ProgressValue { get; }

			public override bool Equals(object obj)
			{
				return obj is ProgressState state && state.Status == Status && state.ProgressValue == ProgressValue;
			}

			public override int GetHashCode()
			{
				return Status.GetHashCode() ^ ProgressValue.GetHashCode();
			}
		}

		class NewThreadWinzorDispatcherContext : IWinzorDispatcherContext
		{
			public NewThreadWinzorDispatcherContext(IWinzorDispatcherContext context)
			{
				inner = context;
			}

			public OpenFormAction OpenForm(Form form) => inner.OpenForm(form);

			public Form Form => inner.Form;

			public void InvokeRenderDispatcher(Func<Task> workItem) => inner.InvokeRenderDispatcher(workItem);

			public void NotifyRenderRequired(Control control)
			{
				if (control.HasRendered)
				{
					control.ReadyToRender();
					_ = control.InvokeStateHasChangedAsync();
				}
			}

			public void OnEnterMessageLoop()
			{
			}

			public void RegisterRenderTask(Task task)
			{
			}

			readonly IWinzorDispatcherContext inner;
		}
	}
}
