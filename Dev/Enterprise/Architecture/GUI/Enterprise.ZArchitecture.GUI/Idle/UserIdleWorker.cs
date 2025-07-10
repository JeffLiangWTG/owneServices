using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
#if WINZOR

using WinzorFramework;

#endif
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A queue of work item delegates that run after the user is idle for 1 second.
	/// </summary>
#if !DEBUG
	sealed
#endif
	public class UserIdleWorker : IDisposable, IIdleWorker
	{
		public bool Disposed { get; private set; }

#if DEBUG
		protected
#endif
 UserIdleWorker(int startDelay)
		{
			this.startDelay = startDelay;
		}

		public static bool IsActive
		{
			get { return instances != null; }
		}

		public static IEnumerable<IUserIdleWorkItem> QueuedWorkItems
		{
			get
			{
				if (instances != null)
				{
					foreach (var worker in instances.Values)
					{
						foreach (IUserIdleWorkItem item in worker.queuedWorkItems)
						{
							yield return item;
						}
					}
				}
			}
		}

		public static int QueuedWorkItemCount
		{
			get { return new List<IUserIdleWorkItem>(QueuedWorkItems).Count; }
		}

		#region QueueWorkItem

		const int DefaultStartDelay = 1000;

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, "", DefaultStartDelay, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, UserIdleWorkItemOptions options, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, "", options, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, string description, UserIdleWorkItemOptions options, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, description, DefaultStartDelay, options, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, int startDelayInMilliseconds, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, "", startDelayInMilliseconds, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, string description, int startDelayInMilliseconds, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, description, startDelayInMilliseconds, UserIdleWorkItemOptions.None, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, int startDelayInMilliseconds, UserIdleWorkItemOptions options, Delegate method, params object[] args)
		{
			return QueueWorkItem(workItemOwner, "", startDelayInMilliseconds, options, method, args);
		}

		/// <summary>
		///		Queues the <paramref name="method"/> for execution. The method executes when a user is idle.
		/// </summary>
		/// <returns>
		///		An instance of <see cref="IUserIdleWorkItem"/> if <paramref name="method"/> was successfully scheduled for execution; <c>null</c>, if the
		///		<paramref name="method"/> was executed synchronously (when the UserIdleWorker is disabled, from the registry for example).
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static IUserIdleWorkItem QueueWorkItem(Control workItemOwner, string description, int startDelayInMilliseconds, UserIdleWorkItemOptions options, Delegate method, params object[] args)
		{
			return GetInstance(startDelayInMilliseconds).QueueWorkItemCore(workItemOwner, description, options, method, args);
		}

#if DEBUG
		protected
#endif
		IUserIdleWorkItem QueueWorkItemCore(Control workItemOwner, string description, UserIdleWorkItemOptions options, Delegate method, object[] args)
		{
			if (Disposed)
			{
				throw new ObjectDisposedException(nameof(UserIdleWorker));
			}

			IUserIdleWorkItem result;
			if (UserIdleWorkerEnabled)
			{
				result = QueueWorkItemSkipEnabledCheck(workItemOwner, description, options, method, args);
			}
			else
			{
				var workItem = new WorkItem(this, workItemOwner, description, startDelay, options, method, args);
				workItem.Run();
				result = null;
			}
			return result;
		}

		IUserIdleWorkItem QueueWorkItemSkipEnabledCheck(Control workItemOwner, string description, UserIdleWorkItemOptions options, Delegate method, object[] args)
		{
			IUserIdleWorkItem result = null;
			if (workItemOwner == null || !workItemOwner.IsDisposed)
			{
				var workItem = new WorkItem(this, workItemOwner, description, startDelay, options, method, args);
				OnQueued(new UserIdleWorkItemEventArgs(workItem));
				AddWorkItemToQueue(workItem);
				try
				{
					Enabled = true;
					if (queuedWorkItems.Count == 1)
					{
						Timer.Interval = startDelay == 0 ? 1 : startDelay;
					}
					workItemsWaitingOnEditOfControl = false;
					result = workItem;
				}
				catch (Win32Exception)
				{
					// occasionally the timer can't be created due to a CreateHandle() exception (issue 00063777)
					workItem.Run();
				}
			}
			DeactivateIfNoWorkItemsRemaining();
			return result;
		}

		void AddWorkItemToQueue(WorkItem workItem)
		{
			queuedWorkItems.Add(workItem);
#if DEBUG
			if (StackTraceIsEnabled)
			{
				LastCallStacks.Add(new StackTrace(true));
			}
#endif
		}

#if DEBUG

		public static bool StackTraceIsEnabled
		{
			get { return stackTraceIsEnabled; }
			set
			{
				stackTraceIsEnabled = value;
				if (!value)
				{
					LastCallStacks.Clear();
				}
			}
		}
		static bool stackTraceIsEnabled;

		public static List<StackTrace> LastCallStacks
		{
			get { return lastCallStacks ?? (lastCallStacks = new List<StackTrace>()); }
		}
		static List<StackTrace> lastCallStacks;
#endif

		#endregion

		#region UserIdleWorkerEnabled registry setting

		bool UserIdleWorkerEnabled
		{
			get
			{
				if (userIdleWorkerEnabled == null || ObjectFactory.Get<ISystemDataRegistry>() != lastSystemDataRegistry)
				{
					userIdleWorkerEnabled = ObjectFactory.Get<ISystemDataRegistry>().UserIdleWorkerEnabled;
					lastSystemDataRegistry = ObjectFactory.Get<ISystemDataRegistry>();
				}
				return (bool)userIdleWorkerEnabled;
			}
		}
		bool? userIdleWorkerEnabled;
		ISystemDataRegistry lastSystemDataRegistry;

		#endregion

		#region Suspend

		public static IDisposable Suspend()
		{
			suspendIndex++;
			var result = new DisposableAction[1];
			result[0] = new DisposableAction(delegate
			{
				suspendIndex--;
				DisposableLeakListener.Instance.UnRegisterDisposable(result[0]);
			});
			DisposableLeakListener.Instance.RegisterDisposable(result[0]);
			return result[0];
		}

		public static bool IsSuspended
		{
			get { return suspendIndex > 0; }
		}

		[ThreadStatic]
		static int suspendIndex;

		#endregion

		#region Queued / Completed / Cancelled

		/// <summary>
		/// When a work item has been queued.
		/// </summary>
		public static event EventHandler<UserIdleWorkItemEventArgs> Queued;

		static void OnQueued(UserIdleWorkItemEventArgs e)
		{
			if (Queued != null)
			{
				Queued(null, e);
			}
		}

		/// <summary>
		/// When a work item has completed.
		/// </summary>
		public static event EventHandler<UserIdleWorkItemEventArgs> Completed;

		static void OnCompleted(UserIdleWorkItemEventArgs e)
		{
			if (Completed != null)
			{
				Completed(null, e);
			}
		}

		/// <summary>
		/// When a work item has completed.
		/// </summary>
		public static event EventHandler<UserIdleWorkItemEventArgs> Cancelled;

		static void OnCancelled(UserIdleWorkItemEventArgs e)
		{
			if (Cancelled != null)
			{
				Cancelled(null, e);
			}
		}

		#endregion

		#region Flush

		public static void Flush()
		{
			while (instances != null)
			{
				foreach (var worker in new List<UserIdleWorker>(instances.Values))
				{
					worker.DequeueAndRun1WorkItem(false);
				}
			}
		}

		#endregion

		#region Instance

		public static UserIdleWorker DefaultInstance
		{
			get { return GetInstance(DefaultStartDelay); }
		}

		static UserIdleWorker GetInstance(int startDelay)
		{
			if (instances == null)
			{
				instances = new Dictionary<int, UserIdleWorker>();
			}

			UserIdleWorker result = null;
			instances.TryGetValue(startDelay, out result);
			if (result == null)
			{
				result = new UserIdleWorker(startDelay);
			}
			instances[startDelay] = result;
			return result;
		}

		[ThreadStatic]
		static Dictionary<int, UserIdleWorker> instances;

		#endregion

		#region ErrorLogger

		public class ErrorLogger
		{
			public ErrorLogger()
			{
				messages = new List<string>();
			}

			public void Log(string message)
			{
				messages.Add(message);
			}

			public void Clear()
			{
				messages.Clear();
			}

			public string GetLoggedMessages()
			{
				return string.Join(System.Environment.NewLine, messages);
			}

			readonly List<string> messages;
		}

		#endregion

		#region WorkItem

		[DebuggerDisplay("Description={Description}, Options={Options}, StartDelay={StartDelay}")]
		internal class WorkItem : IUserIdleWorkItem, IDisposable
		{
			public WorkItem(UserIdleWorker owner, Control workItemOwner, string description, int startDelay, UserIdleWorkItemOptions options, Delegate method, object[] args)
			{
				if (workItemOwner != null)
				{
					workItemOwner.Disposed += DisposeForMatchDoNotRemoveAsEventRemovalWillNotWork;
				}
				this.owner = owner;
				this.WorkItemOwner = workItemOwner;
				this.Description = description;
				this.StartDelay = startDelay;
				this.Options = options;
				this.method = method;
				this.args = args;
				var key = GetKey(method.Method);
			}

			public string Description { get; private set; }
			public Control WorkItemOwner { get; private set; }
			public int StartDelay { get; private set; }
			public UserIdleWorkItemOptions Options { get; private set; }

			public bool IsReadyToRun(Form form)
			{
				var activeForm = ActiveForm;
				var duringEditCondition =
					AllowDuringEdit ||
					!(form != null && owner.IsEditingAControl(form));
				var whenFormActiveCondition =
					form == null ||
					AllowWhenFormInactive ||
					(activeForm != null && form == activeForm);
				return duringEditCondition && whenFormActiveCondition;
			}

			public void Run()
			{
#if WINZOR
				var context = new ServerInitiatedCallbackContext();
				using (WinzorDispatcher.Current.WithContext(context))
				{
					try
					{
						using (PerformanceStatisticsCollector.StartMonitoring("UserIdleWorker", method.Method.Name))
						{
							method.DynamicInvoke(args);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce(ex.Message, ex);
					}
					OnCompleted(new UserIdleWorkItemEventArgs(this));
					Dispose();
				}
#endif
#if !WINZOR
				try
				{
					using (PerformanceStatisticsCollector.StartMonitoring("UserIdleWorker", method.Method.Name))
					{
						method.DynamicInvoke(args);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
				OnCompleted(new UserIdleWorkItemEventArgs(this));
				Dispose();
#endif
			}

			void DisposeForMatchDoNotRemoveAsEventRemovalWillNotWork(object sender, EventArgs args)
			{
				Dispose();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
			string GetKey(MethodInfo method)
			{
				var controlName = WorkItemOwner == null ? "<null>" : WorkItemOwner.Name;
				return method.Name + "_" + controlName + "_" + Description;
			}

			public void Dispose()
			{
				if (WorkItemOwner != null)
				{
					WorkItemOwner.Disposed -= DisposeForMatchDoNotRemoveAsEventRemovalWillNotWork;
				}
				owner.CancelWorkItem(this);
			}

			#region Implementation

			readonly UserIdleWorker owner;
			readonly Delegate method;
			readonly object[] args;

			bool AllowDuringEdit
			{
				get { return (Options & UserIdleWorkItemOptions.AllowDuringEdit) == UserIdleWorkItemOptions.AllowDuringEdit; }
			}

			bool AllowWhenFormInactive
			{
				get
				{
					return
#if DEBUG
 AllowWhenFormInactiveForTest ||
#endif
 (Options & UserIdleWorkItemOptions.AllowWhenFormInactive) == UserIdleWorkItemOptions.AllowWhenFormInactive;
				}
			}

#endregion
		}

#endregion

#region For Testing
#if DEBUG

		internal static void RunOne()
		{
			if (instances != null)
			{
				foreach (var worker in instances.Values)
				{
					worker.DequeueAndRun1WorkItem(false);
					break;
				}
			}
		}

		[SuppressThreadStaticFieldMessage]
		internal static bool AllowWhenFormInactiveForTest = true;

		internal static Form ActiveForm
		{
			private get { return activeFormSet ? activeForm : Form.ActiveForm; }
			set
			{
				activeForm = value;
				activeFormSet = value != Form.ActiveForm;
			}
		}
		[ThreadStatic]
		static bool activeFormSet;
		[ThreadStatic]
		static Form activeForm;
#else
		static Form ActiveForm
		{
			get { return Form.ActiveForm; }
		}
#endif

		#endregion

		#region IsEditingAControl

		readonly WeakReference lastFormEditingAControl = new WeakReference(null);
		bool lastFormWasEditingAControl;

		bool IsEditingAControl(Form form)
		{
			var result = form != null && lastFormEditingAControl.Target == form ? lastFormWasEditingAControl : IsEditingAControlNotCached(form);
			lastFormEditingAControl.Target = form;
			lastFormWasEditingAControl = result;
			return result;
		}

		bool IsEditingAControlNotCached(Form form)
		{
			var control = form.GetFrontMostActiveControl();
			return control != null && EditableControl.Get(control).IsEditing;
		}

#endregion

#region PreserveCurrentGridCell

		static IDisposable PreserveCurrentGridCell(Form form)
		{
			var listEditableControl = FindActiveListEditableControl(form);
			return listEditableControl != null ? listEditableControl.PreserveCurrentSelection() : new DisposableAction(delegate { });
		}

		static IListEditableControl FindActiveListEditableControl(Form form)
		{
			var current = GetActiveMostControl(form);
			while (current != null)
			{
				var listEditableControl = current as IListEditableControl;
				if (listEditableControl != null)
				{
					return listEditableControl;
				}
				current = current.Parent;
			}
			return null;
		}

		sealed class OptimizedFormCurrentGridCellPreserver : IDisposable
		{
			public void PreserveCurrentGridCell(Form form)
			{
				if (form != null && !forms.Contains(form))
				{
					preservers.Add(UserIdleWorker.PreserveCurrentGridCell(form));
					forms.Add(form);
				}
			}

			public void Dispose()
			{
				foreach (var preserver in preservers)
				{
					preserver.Dispose();
				}
			}

			readonly List<IDisposable> preservers = new List<IDisposable>();
			readonly List<Form> forms = new List<Form>();
		}

#endregion

#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}

			try
			{
				Enabled = false;
				if (timer != null)
				{
					timer.Dispose();
				}

				if (instances != null)
				{
					instances.Remove(startDelay);
					if (instances.Count == 0)
					{
						instances = null;
					}
				}
			}
			finally
			{
				Disposed = true;
			}
		}

		#endregion

		#region Implementation

		readonly List<WorkItem> queuedWorkItems = new List<WorkItem>();
		readonly int startDelay;
		bool inApplicationIdle;
		bool ignoreNextTimerTick;
		bool workItemsWaitingOnEditOfControl;

		bool Enabled
		{
			get { return enabled; }
			set
			{
				if (value != enabled)
				{
					if (enabled)
					{
						UserIdleDetecter.UserActivity -= new EventHandler(UserIdleDetecter_UserActivity);
					}
					enabled = value;
					if (enabled)
					{
						UserIdleDetecter.UserActivity += new EventHandler(UserIdleDetecter_UserActivity);
					}
					try
					{
						Timer.Enabled = enabled;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						enabled = false;
						UserIdleDetecter.UserActivity -= new EventHandler(UserIdleDetecter_UserActivity);
						throw;
					}
					ignoreNextTimerTick = false;
				}
			}
		}
		bool enabled;

		IWindowsTimer Timer
		{
			get
			{
				if (timer == null)
				{
					timer = CreateTimer();
					timer.Tick += new EventHandler(Timer_Tick);
				}
				return timer;
			}
		}
		IWindowsTimer timer;

#if DEBUG
		protected virtual
#endif
 IWindowsTimer CreateTimer()
		{
			return new ProxyWindowsTimer();
		}

		void UserIdleDetecter_UserActivity(object sender, EventArgs e)
		{
			// putting a breakpoint here may crash your computer
			ignoreNextTimerTick = true;

			workItemsWaitingOnEditOfControl = false;
			lastFormEditingAControl.Target = null;
		}

		void Timer_Tick(object sender, EventArgs e)
		{
			if (!ignoreNextTimerTick)
			{
#if DEBUG
				if (!RunOnApplicationIdle)
				{
					RunWorkItemsUntilNotIdle();
				}
				else
#endif
				{
					Application.Idle -= new EventHandler(Application_Idle);
					Application.Idle += new EventHandler(Application_Idle);
				}
			}
			ignoreNextTimerTick = false;
		}

#if DEBUG
		protected virtual bool RunOnApplicationIdle
		{
			// it isn't possible to run on Application.Idle in unit tests because the message loop is never idle
			get { return !Globals.IsTest; }
		}
#endif

		void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= new EventHandler(Application_Idle);
			RunWorkItemsUntilNotIdle();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		void RunWorkItemsUntilNotIdle()
		{
			if (!ExceptionReporter.Instance.IsReportingException &&
				!Globals.Message.IsShowingError &&
				!inApplicationIdle &&
				!workItemsWaitingOnEditOfControl &&
				!IsSuspended)
			{
				var allowUserToContinue = false;
				EventHandler userActivity = delegate { allowUserToContinue = true; }; // putting a breakpoint here may crash your computer

				inApplicationIdle = true;
				UserIdleDetecter.UserActivity += userActivity;
				try
				{
					while (!allowUserToContinue && DequeueAndRun1WorkItem(true))
					{
						Application.DoEvents();
					}
				}
				finally
				{
					UserIdleDetecter.UserActivity -= userActivity;
					inApplicationIdle = false;
				}
			}
		}

		bool DequeueAndRun1WorkItem(bool observeConditions)
		{
			using (var preserver = new OptimizedFormCurrentGridCellPreserver())
			{
				var workItemWasRun = false;
				for (var i = 0; i < queuedWorkItems.Count; i++)
				{
					var workItem = queuedWorkItems[i];
					var form = workItem.WorkItemOwner == null ? null : workItem.WorkItemOwner.FindForm();
					if (!observeConditions || workItem.IsReadyToRun(form))
					{
						queuedWorkItems.RemoveAt(i--);

						if (form == null || !form.IsDisposed)
						{
							preserver.PreserveCurrentGridCell(form);
							workItemWasRun = true;
							workItem.Run();
							break;
						}
					}
				}

				workItemsWaitingOnEditOfControl = (!workItemWasRun && queuedWorkItems.Count > 0);
				DeactivateIfNoWorkItemsRemaining();
				return workItemWasRun;
			}
		}

		void CancelWorkItem(WorkItem workItem)
		{
			queuedWorkItems.Remove(workItem);
			OnCancelled(new UserIdleWorkItemEventArgs(workItem));
			DeactivateIfNoWorkItemsRemaining();
		}

		void DeactivateIfNoWorkItemsRemaining()
		{
			if (queuedWorkItems.Count == 0)
			{
				Dispose();
			}
		}

		static Control GetActiveMostControl(Control control)
		{
			var result = control;
			var containerControl = control as ContainerControl;
			if (containerControl != null)
			{
				var activeControl = GetActiveMostControl(containerControl.ActiveControl);
				if (activeControl != null)
				{
					result = activeControl;
				}
			}
			return result;
		}

#endregion

#region IIdleWorker Members

		void IIdleWorker.Flush()
		{
			UserIdleWorker.Flush();
		}

		void IIdleWorker.QueueWorkItem(Delegate method, params object[] args)
		{
			UserIdleWorker.QueueWorkItem(null, method, args);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void IIdleWorker.QueueWorkItemWithOptions(int startDelayInMilliseconds, UserIdleWorkItemOptions options, Delegate method, params object[] args)
		{
			UserIdleWorker.QueueWorkItem(null, "", startDelayInMilliseconds, options, method, args);
		}

#endregion
	}
}
