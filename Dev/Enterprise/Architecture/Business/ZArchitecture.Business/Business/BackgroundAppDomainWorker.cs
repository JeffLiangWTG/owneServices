using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture
{
	public class BackgroundAppDomainWorker : MarshalByRefObject
	{
		protected BackgroundAppDomainWorker()
		{
		}

		#region Error event

		public static event UnhandledExceptionEventHandler Error
		{
			add { Instance.UnhandledError += value; }
			remove { Instance.UnhandledError -= value; }
		}

		internal event UnhandledExceptionEventHandler UnhandledError;

		void OnUnhandledError(Exception ex, int number)
		{
			var errorInfo = $"{DateTime.Now}: TraceNumber:{number}, CurrentAppDomain:{GetAppDomainInfo(AppDomain.CurrentDomain)}.\r\n " +
				$"CreatedDomians:{string.Join(",", createdAppDomains)}, UnloadDomains:{string.Join(",", unloadedAppDomains)}\r\n";  // debugging purposes
			unhandledErrors.Add((errorInfo, ex));
			if (UnhandledError != null)
			{
				UnhandledError(null, new UnhandledExceptionEventArgs(ex, false));
			}
		}

		#endregion

		#region QueueWorkItem

#if NETFRAMEWORK
		public static IAsyncResult QueueWorkItem(string workItemDescription, CrossAppDomainDelegate method)
		{
			return QueueWorkItem(workItemDescription, method, Array.Empty<object>());
		}
#else
		public static IAsyncResult QueueWorkItem(string workItemDescription, Action method)
		{
			return QueueWorkItem(workItemDescription, method, Array.Empty<object>());
		}
#endif

		/// <summary>
		/// Run a unit of work on a separate thread in a separate application domain asynchronously.
		/// An anonymous delegate that references local or member variables cannot be used.
		/// The delegate arguments, and the delegate instance (if the method is not static) must be serializable.
		/// </summary>
		/// <param name="workItemDescription">
		/// The human-readable description of the work item. Used as the key in ReportOnce,
		/// and shown to the user when attempting to close Enterprise with pending background jobs.
		/// </param>
		/// <param name="method">The delegate to run. If the delegate points to an instance method, the instance must be serializable.</param>
		/// <param name="args">The arguments to the delegate. Each argument must be serializable.</param>
		/// <returns>The return value of the delegate, which must be serializable.</returns>
		public static IAsyncResult QueueWorkItem(string workItemDescription, Delegate method, params object[] args)
		{
			return Instance.QueueWorkItemCore(workItemDescription, method, args).AsyncResult;
		}

		public static IBackgroundAppDomainWorkItem QueueAndReturnWorkItem(string workItemDescription, Delegate method, params object[] args)
		{
			return Instance.QueueWorkItemCore(workItemDescription, method, args);
		}

		IBackgroundAppDomainWorkItem QueueWorkItemCore(string workItemDescription, Delegate method, params object[] args)
		{
			return QueueWorkItemCore(new WorkItem(workItemDescription, method, args));
		}

		protected virtual IBackgroundAppDomainWorkItem QueueWorkItemCore(WorkItem workItem)
		{
			IBackgroundAppDomainWorkItem result = null;
			if (appDomainWorkerInMainDomain != null)
			{
				result = appDomainWorkerInMainDomain.QueueWorkItemCore(workItem.Description, workItem.Method, workItem.Args);
			}
			else
			{
				lock (mutex)
				{
					EnqueueWorkItem(workItem);
					EnsureWorkerThreadRunning();
				}
				result = workItem;
			}
			return result;
		}

#endregion

		#region WorkItemsInProgress

		public static IBackgroundAppDomainWorkItem[] WorkItemsInProgress
		{
			get { return Instance.WorkItemsInProgressCore; }
		}

		protected IBackgroundAppDomainWorkItem[] WorkItemsInProgressCore
		{
			get
			{
				lock (mutex)
				{
					return queuedWorkItems.ToArray();
				}
			}
		}

		#endregion

		public static IBackgroundAppDomainWorkItem RecentlyCompletedWorkItem => Instance.recentlyCompletedWorkItem;

		#region WorkItemsInProgressChanged

		public static event EventHandler WorkItemsInProgressChanged;

		void OnWorkItemsInProgressChanged(EventArgs e)
		{
			if (WorkItemsInProgressChanged != null)
			{
				WorkItemsInProgressChanged(this, e);
			}
		}

		#endregion

		#region Helper Classes

		[Serializable]
		[DebuggerDisplay("Description={Description}, Status={Status}, SubmittedTime={SubmittedTime}")]
		protected class WorkItem : IBackgroundAppDomainWorkItem
		{
			public WorkItem(string description, Delegate method, object[] args)
			{
				this.description = description;
				this.Method = method;
				this.Args = args;
				this.AsyncResult = new AsyncResult();

				if (method.Target is MarshalByRefObject)
				{
					throw new ArgumentException("Only delegates to static methods or methods on [Serializable] classes are allowed.", nameof(method));
				}
			}

			public readonly Delegate Method;
			public readonly object[] Args;
			public readonly AsyncResult AsyncResult;

			public string Description
			{
				get { return description; }
			}
			readonly string description;

			public BackgroundAppDomainWorkItemStatus Status
			{
				get { return status; }
				set { status = value; }
			}
			BackgroundAppDomainWorkItemStatus status = BackgroundAppDomainWorkItemStatus.Queued;

			public ZDateTime SubmittedTime
			{
				get { return submittedTime; }
			}
			readonly DateTime submittedTime = ZDateTime.Now.ToDateTime();

			IAsyncResult IBackgroundAppDomainWorkItem.AsyncResult
			{
				get { return this.AsyncResult; }
			}
		}

		protected sealed class AppDomainWorker : MarshalByRefObject
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
			public void InitialiseIfRequired(BackgroundAppDomainWorker workerInMainDomain, string loginName, Guid branchPK, Guid departmentPK, string databaseName, string serverName, bool isUserInteractive)
			{
				if (!initializeCalledInCurrentAppDomain)
				{
					try
					{
						EnterpriseApplicationConfiguration.ConfigureObjectFactory();
						ObjectFactory.GetType<Enterprise.Integration.Initialisation.IInitialiser>().InvokeMember("InitialiseBatchProcessor", BindingFlags.InvokeMethod, null, null, null);

						Db.InitializeDatabaseDetails(
							serverName,
							databaseName,
							isUserInteractive ? ApplicationType.Default : ApplicationType.Web);
						Db.Connection.EnsureIsOpen();

						Globals.IsUserInteractive = isUserInteractive;
						IEnvironment env = EnvProxy.Instance;
						env.SetUserContext(env.NewUserContext(loginName, branchPK, departmentPK));
						if (env.CurrentCompany == null)
						{
							throw new InvalidOperationException("EnvProxy.Instance.Setup failed, EnvProxy.Instance.CurrentCompany is null");
						}

						if (env.CurrentBranch == null)
						{
							throw new InvalidOperationException("EnvProxy.Instance.Setup failed, EnvProxy.Instance.CurrentBranch is null");
						}

						if (env.CurrentDepartment == null)
						{
							throw new InvalidOperationException("EnvProxy.Instance.Setup failed, EnvProxy.Instance.CurrentDepartment is null");
						}

						if (env.CurrentUser == null)
						{
							new InvalidOperationException("EnvProxy.Instance.Setup failed, EnvProxy.Instance.CurrentUser is null");
						}

						appDomainWorkerInMainDomain = workerInMainDomain;
						initializeCalledInCurrentAppDomain = true;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (!(ex is ThreadAbortException))
						{
							ErrorReporter.ReportOnce("AppDomainWorkerInitialiseIfRequiredFailed", ex.Message, ex);
						}
						throw;
					}
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Permanently available for debugging purposes")]
			public object Invoke(string workItemDescription, Delegate method, object[] args)
			{
				Debug.WriteLine("Start: " + workItemDescription);
				try
				{
					return method.DynamicInvoke(args);
				}
				catch (TargetInvocationException ex)
				{
					if (!(ex.InnerException is ThreadAbortException))
					{
						ErrorReporter.ReportOnce(workItemDescription + " - " + ex.InnerException.Message, ex.InnerException);
						throw;
					}
					return null;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!(ex is ThreadAbortException))
					{
						ErrorReporter.ReportOnce(workItemDescription + " - " + ex.Message, ex);
						throw;
					}
					return null;
				}
				finally
				{
					Debug.WriteLine("Finish: " + workItemDescription);
					if (!initializeCalledInCurrentAppDomain)
					{
						ErrorReporter.ReportOnce(workItemDescription + " - Initialize() must be called before a work item is invoked");
					}
				}
			}

#if NETFRAMEWORK
			public override object InitializeLifetimeService()
			{
				return null;
			}
#endif

			[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
			static bool initializeCalledInCurrentAppDomain;
		}

		protected class AsyncResult : MarshalByRefObject, IAsyncResult
		{
			public bool IsCompleted;
			bool IAsyncResult.IsCompleted
			{
				get { return IsCompleted; }
			}

			public object AsyncState;
			object IAsyncResult.AsyncState
			{
				get { return AsyncState; }
			}

			public bool CompletedSynchronously;
			bool IAsyncResult.CompletedSynchronously
			{
				get { return CompletedSynchronously; }
			}

			WaitHandle IAsyncResult.AsyncWaitHandle
			{
				get { throw new NotImplementedException(); }
			}

#if NETFRAMEWORK
			public override object InitializeLifetimeService()
			{
				return null;
			}
#endif
		}

		#endregion

		#region Implementation

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		protected static BackgroundAppDomainWorker Instance = new BackgroundAppDomainWorker();
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static BackgroundAppDomainWorker appDomainWorkerInMainDomain;

		readonly Queue<WorkItem> queuedWorkItems = new Queue<WorkItem>();
		protected WorkItem recentlyCompletedWorkItem;
		Thread workerThread;
		readonly object mutex = new object();

#if NETFRAMEWORK
		public override object InitializeLifetimeService()
		{
			return null;
		}
#endif

		void EnsureWorkerThreadRunning()
		{
			if (workerThread == null)
			{
				workerThread = new Thread(WorkerThreadMethod);
				workerThread.SetApartmentState(ApartmentState.STA);
				workerThread.Start();
			}
		}

		void WorkerThreadMethod()
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var envInitializationWorker = CreateNewWorkerInAppDomain();
					bool unloaded = false;
					while (!unloaded && QueuedWorkItemCount > 0)
					{
						IEnvironment env = EnvProxy.Instance;
						envInitializationWorker.InitialiseIfRequired(this, env.CurrentUser.LoginName, env.CurrentBranch.PK, env.CurrentDepartment.PK,
							Db.DatabaseName, Db.ServerName, Globals.IsUserInteractive);

						WorkItem workItem = PeekNextWorkItem();
						try
						{
							ProcessWorkItem(workItem);
						}
						catch (Exception ex) // this catch is specifically for handling critical exceptions
						{
							if (!(ex is ThreadAbortException))
							{
								// the child AppDomain will also report the error silently
								OnUnhandledError(ex, 1);
							}
						}
						finally
						{
							DequeueWorkItem();
							workItem.AsyncResult.IsCompleted = true;

							lock (mutex)
							{
								if (QueuedWorkItemCount == 0)
								{
									UnloadWorkerAppDomain();
									workerThread = null;
									unloaded = true;
								}
							}
						}
					}
				}
				catch (Exception ex) // this catch is specifically for handling critical exceptions
				{
					if (!(ex is ThreadAbortException))
					{
						// the child AppDomain will also report the error silently
						OnUnhandledError(ex, 2);
					}
				}
			}
		}

		void ProcessWorkItem(WorkItem workItem)
		{
			AppDomainWorker worker = CreateNewWorkerInAppDomain();
			workItem.Status = BackgroundAppDomainWorkItemStatus.Running;
			workItem.AsyncResult.AsyncState = worker.Invoke(workItem.Description, workItem.Method, workItem.Args);
		}

		int QueuedWorkItemCount
		{
			get
			{
				lock (mutex)
				{
					return queuedWorkItems.Count;
				}
			}
		}

		protected void EnqueueWorkItem(WorkItem workItem)
		{
			queuedWorkItems.Enqueue(workItem);
			OnWorkItemsInProgressChanged(EventArgs.Empty);
		}

		protected void DequeueWorkItem()
		{
			lock (mutex)
			{
				recentlyCompletedWorkItem = queuedWorkItems.Dequeue();
				OnWorkItemsInProgressChanged(EventArgs.Empty);
			}
		}

		WorkItem PeekNextWorkItem()
		{
			lock (mutex)
			{
				return queuedWorkItems.Peek();
			}
		}

		protected AppDomain WorkerAppDomain
		{
			get
			{
				lock (mutex)
				{
					if (workerAppDomain != null && IsAppDomainUnloadedOrUnloading(workerAppDomain))
					{
						try
						{
							workerAppDomain.UnhandledException -= HandleWorkerAppDomainUnhandledException;
						}
						catch (AppDomainUnloadedException ex)
						{
							OnUnhandledError(ex, 3);
						}
						workerAppDomain = null;
					}

					if (workerAppDomain == null)
					{
#pragma warning disable CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
						workerAppDomain = AppDomain.CreateDomain(AppDomainFriendlyName);
#pragma warning restore CW1157, SYSLIB0024 // WI00669071 - Do not use System.AppDomain.
						workerAppDomain.UnhandledException += HandleWorkerAppDomainUnhandledException;
						workerAppDomain.DomainUnload += WorkerAppDomain_DomainUnload;
						OnAppDomainCreated(workerAppDomain);
					}

					return workerAppDomain;
				}
			}
		}

		void WorkerAppDomain_DomainUnload(object sender, EventArgs e)
		{
			if (sender is AppDomain appDomain)
			{
				unloadedAppDomains.Add("UnloadTime:" + DateTime.Now + GetAppDomainInfo(appDomain));
			}
		}

		readonly List<string> createdAppDomains = new List<string>();
		readonly List<string> unloadedAppDomains = new List<string>();

		AppDomain workerAppDomain;

		void HandleWorkerAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				OnUnhandledError(ex, 4);

				ErrorReporter.ReportOnce($"{nameof(BackgroundAppDomainWorker)}_{ex.GetType().Name}", ex.Message, ex);
			}
		}

		// Be careful not to cache the remote AppDomainWorker as it cannot be reused when the worker domain is unloaded.
		protected virtual AppDomainWorker CreateNewWorkerInAppDomain()
		{
			AppDomainWorker result;
			if (appDomainWorkerInMainDomain != null)
			{
				result = new AppDomainWorker();
			}
			else
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				result = (AppDomainWorker)WorkerAppDomain.CreateInstanceAndUnwrap(typeof(AppDomainWorker).Assembly.GetName().Name, typeof(AppDomainWorker).FullName);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}

			return result;
		}

		protected void UnloadWorkerAppDomain()
		{
			AppDomain appDomainToUnload = null;

			if (workerAppDomain != null)
			{
				appDomainToUnload = workerAppDomain;
				try
				{
					workerAppDomain.UnhandledException -= HandleWorkerAppDomainUnhandledException;
				}
				catch (Exception ex)
				{
					OnUnhandledError(ex, 7);
				}
				workerAppDomain = null;
				UnloadAppDomainSafe(appDomainToUnload);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For error report, non translatable.")]
		string GetAppDomainInfo(AppDomain appDomainToGet)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			string FriendlyNameSafe(AppDomain appDomain)
			{
				var result = "";
				try
				{
					result = appDomain == null ? "<null>" : $"{appDomain.Id}:{appDomain.FriendlyName}:{appDomain.GetHashCode()}";
				}
				catch (AppDomainUnloadedException)
				{
					result = "<AppDomainUnloadedException>";
				}
				return result;
			}

			string IsDefaultAppDomainSafe(AppDomain appDomain)
			{
				var result = "";
				try
				{
					result = appDomain == null ? "<null>" : (appDomain.IsDefaultAppDomain() ? "true" : "false");
				}
				catch (AppDomainUnloadedException)
				{
					result = "<AppDomainUnloadedException>";
				}
				return result;
			}

			var appDomainInfo = "<canNotGetInfo>";
			try
			{
				appDomainInfo = $"{FriendlyNameSafe(appDomainToGet)}|{IsDefaultAppDomainSafe(appDomainToGet)}|{appDomainToGet.IsFinalizingForUnload()}";
			}
			catch (Exception ex)
			{
				appDomainInfo = ex.Message;
			}
			return appDomainInfo;
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		readonly List<(string, Exception)> unhandledErrors = new List<(string, Exception)>();

		void UnloadAppDomainSafe(AppDomain appDomain)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			//the AppDomain unloading shouldn't be blocking - so just do it in its own thread.
			//Hopefully it finishes - but if not, there's nothing we can do
			//(except identifying the logic error that makes that specific work item freeze up from the issue and fixing it, hence the report)
			var thread = new Thread(() =>
			{
				Exception exception = null;
				var max_tries = 10;
				for (int i = 0; i < max_tries; ++i)
				{
					try
					{
#pragma warning disable SYSLIB0024
						AppDomain.Unload(appDomain);
#pragma warning disable SYSLIB0024
						break;
					}
					// AppDomain.Unload() does not throw AppDomainUnloadedException
					// https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.unload?view=netframework-4.8
					// https://docs.microsoft.com/en-us/dotnet/api/system.cannotunloadappdomainexception?view=netframework-4.8
					catch (AppDomainUnloadedException)
					{
						break;
					}
					catch (CannotUnloadAppDomainException ex)
					{
						if (IsAppDomainUnloadedOrUnloading(appDomain))
						{
							break;
						}

						if (i >= (max_tries - 1))
						{
							exception = ex;
							break;
						}

						Thread.Sleep(i * 1000);
					}
				}

				if (exception != null)
				{
					ReportFailureToUnloadAppDomain(appDomain, exception);
				}
				else
				{
					OnAppDomainUnloaded();
				}
			});
			thread.Start();
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		void ReportFailureToUnloadAppDomain(AppDomain appDomainToUnload, Exception ex)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			OnUnhandledError(ex, 5);

			ErrorReporter.ReportOnce(nameof(BackgroundAppDomainWorker) + "_UnloadingAppDomain", $@"{ex.Message}
AppDomain: {GetAppDomainInfo(appDomainToUnload)}
.net Version: {System.Environment.Version}
UnhandledErrors:{String.Join("", unhandledErrors.Select(e => $"\r\n{e.Item1}.\r\n {e.Item2.Message}\r\n{e.Item2.StackTrace}"))}
workerThread State: {workerThread?.ThreadState}"
, ex);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		static bool IsAppDomainUnloadedOrUnloading(AppDomain appDomain)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			if (appDomain == null)
			{
				return true;
			}

			try
			{
				if (appDomain.IsFinalizingForUnload())
				{
					return true;
				}

				// Accessing AppDomain.FriendlyName will throw AppDomainUnloadedException on unloaded appdomain
				// https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.friendlyname?view=netframework-4.8
				var appDomainFriendlyName = appDomain.FriendlyName;

				// Following code is here to ensure line above is not optimized out by JIT compiler.
				if (!string.IsNullOrEmpty(appDomainFriendlyName))
				{
					return false;
				}
			}
			catch (AppDomainUnloadedException)
			{
				return true;
			}

			return false;
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		protected virtual string AppDomainFriendlyName
		{
			get { return "WorkerAppDomain"; }
		}

		protected virtual void OnAppDomainCreated(AppDomain appDomain)
		{
			createdAppDomains.Add("CreateTime:" + DateTime.Now + GetAppDomainInfo(appDomain));
		}

		protected virtual void OnAppDomainUnloaded()
		{
		}

		#endregion
	}
}
