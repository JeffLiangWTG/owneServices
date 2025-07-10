using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Common.MemoryManagement;
using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Common.Testing
{
	public static class DisposableLeakListenerWrapper
	{
		public static void RegisterDisposable(IDisposable disposable, bool forceStackTrace = false)
		{
			DisposableLeakListener.Instance.RegisterDisposable(disposable, forceStackTrace);
		}

		public static void UnRegisterDisposable(IDisposable disposable)
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(disposable);
		}
	}

	/// <summary>
	/// Tracks IDisposable uncollectedObjects to make sure they are disposed before the end of a unit test.
	/// If you write a class that implements IDisposable, consider using this class to detect resource leaks.
	/// ❗❕❗❕❗❕❗❕❗❕ NOTE::❗❕❗❕❗❕❗❕❗❕❗❕
	/// History: This class started as an NUnit v1 test listener.
	/// It evolved into a more general leak tracker diagnostic tool that could run outside of NUnit.
	/// It's then been refactored to move all NUnit references into a separate .Test.csproj.
	/// It could be cleaned up further but that was beyond the scope of work to move NUnit references.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public class DisposableLeakListener :
		IDisposableLeakListener
	{
		protected internal DisposableLeakListener()
		{
		}

		/// <summary>
		/// Get the singleton instance of this class.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly DisposableLeakListener Instance = new DisposableLeakListener();

		internal Dictionary<IDisposable, StackTrace> InternalActiveDisposables
		{
			get
			{
				return ActiveDisposables;
			}
		}

		public bool LeakTrackingEnabled { get; set; }

		public void Clear()
		{
			lock (mutex)
			{
				ActiveDisposables.Clear();
				weakReferences.Clear();
				AdditionalInformation = string.Empty;
			}
		}

		void WaitForProgressFormDisposal()
		{
			var stopwatch = Stopwatch.StartNew();
			while (progressFormDisposables.Count > 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
		}

		public string GetFailureMessageAndCleanup(Func<string> captureDumpAction, Func<string, string> formatStackTrace)
		{
			// TODO Is this where we set DisposableLeakListener.Instance.LeakTrackingEnabled = wasLeakTrackingEnabledBeforeTestStarted? Or is there some other place to override?
			try
			{
				WaitForProgressFormDisposal();
				IDisposable[] copyOfUndisposedObjects = null;
				var errors = new StringBuilder();
				lock (mutex)
				{
					if (ActiveDisposables.Count > 0)
					{
						if (!string.IsNullOrEmpty(AdditionalInformation))
						{
							errors.AppendLine(AdditionalInformation);
						}
						errors.AppendLine("There are " + ActiveDisposables.Keys.Count + " undisposed object(s) with the following types: ");

						var typesNotDisposed = ActiveDisposables.Keys.Select(undisposedObject => undisposedObject.GetType().FullName).Distinct();
						errors.AppendLine(String.Join(", ", typesNotDisposed));
						errors.AppendLine();
						errors.AppendLine("The following uncollectedObjects were not disposed:");

						foreach (var undisposedObject in ActiveDisposables.Keys.Take(MaxFailuresPerTest))
						{
							errors.Append(GetLeakReport(undisposedObject, formatStackTrace));
						}

						if (ActiveDisposables.Keys.Count > MaxFailuresPerTest)
						{
							errors.Append("<br><br>More than " + MaxFailuresPerTest + " leaks were detected. Not all non disposed uncollectedObjects have been displayed");
						}

						copyOfUndisposedObjects = ActiveDisposables.Keys.ToArray();
					}
				}

				if (copyOfUndisposedObjects != null)
				{
					foreach (var key in copyOfUndisposedObjects)
					{
						try
						{
							key.Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// Ignore the exception - it's more useful to report the object that wasn't disposed.
							// Usually the exception is because TearDown is outside a transaction, or because
							// some other object was disposed in the wrong order.
						}
					}

					return errors.ToString();
				}

				var builder = new StringBuilder();
				Verify(builder, captureDumpAction);
				if (builder.Length > 0)
				{
					return builder.ToString();
				}
			}
			finally
			{
				lock (mutex)
				{
					ActiveDisposables.Clear();
					AdditionalInformation = string.Empty;
				}

				CaptureMemoryDump = false;
				StackTraceEnabled = false;
			}
			return null;
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposable")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void RegisterDisposable(IDisposable disposable, bool forceStackTrace = false)
		{
			if (LeakTrackingEnabled && disposable != null)
			{
				lock (mutex)
				{
					if (!ActiveDisposables.ContainsKey(disposable))
					{
						if (Thread.CurrentThread.Name == "ProcessStatusFormManager")
						{
							progressFormDisposables.Add(disposable);
						}

						string className = disposable.GetType().FullName;
						if (!className.Contains("Suspender") && !className.Contains("BusinessObjectCollectionEnumerator"))
						{
							var reference = new WeakReference(disposable);
							weakReferences.Add(reference);

							if (lastCheck.Elapsed > timeBetweenPurges) // No reference to CargoWise.Types from CargoWise.Common
							{
								lastCheck.Restart();
								PurgeDeadWeakReferences();
							}

							if (disposable is IRequiresWindowsMessagePumpToCollect)
							{
								referencesRequiringWindowsMessagePumpToCollect.Add(reference);
							}
						}
						ActiveDisposables[disposable] = (forceStackTrace || StackTraceEnabled) ? new StackTrace(true) : null;
					}
				}
			}
		}
		readonly Stopwatch lastCheck = Stopwatch.StartNew();
		readonly TimeSpan timeBetweenPurges = new TimeSpan(0, 0, 30);

		void PurgeDeadWeakReferences()
		{
			weakReferences = weakReferences.FindAll(weakReference => weakReference.IsAlive);
		}

		/// <summary>
		/// Unregister an IDisposable object for tracking. This should be called in the Dispose method of the IDisposable
		/// object, but NOT if Dispose is called by the finalizer.
		/// </summary>
		/// <param name="disposable">IDisposable object being disposed</param>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposable")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Un")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnRegister")]
		public void UnRegisterDisposable(IDisposable disposable)
		{
			if (LeakTrackingEnabled && disposable != null)
			{
				lock (mutex)
				{
					if (ActiveDisposables.ContainsKey(disposable))
					{
						ActiveDisposables.Remove(disposable);

						if (Thread.CurrentThread.Name == "ProcessStatusFormManager")
						{
							progressFormDisposables.Remove(disposable);
						}
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly object mutex = new object();

		public void IgnoreObject(object objectToBeIgnored)
		{
			if (LeakTrackingEnabled && objectToBeIgnored != null)
			{
				lock (mutex)
				{
					var disposable = objectToBeIgnored as IDisposable;
					if (disposable != null && ActiveDisposables.ContainsKey(disposable))
					{
						ActiveDisposables.Remove(disposable);
					}

					var weakRef = weakReferences.FirstOrDefault(w => w.Target == objectToBeIgnored);
					if (weakRef != null)
					{
						weakReferences.Remove(weakRef);
					}
				}
			}
		}

		ICollection<DisposedNotCollectedType> GetUncollected_ThatAreNotActive_And_NotForAppDomainWorker()
		{
			lock (mutex)
			{
				return weakReferences
					.Select(weakRef => (IDisposable)weakRef.Target)
					.Where(target => target != null && !ActiveDisposables.ContainsKey(target))
					.GroupBy(target => target.GetType().FullName)
					.Where(group => !group.Key.Contains("AppDomainWorker"))
					.Select(group => new DisposedNotCollectedType(group.Key, group))
					.OrderBy(disposedNotCollected => disposedNotCollected.Quantity)
					.ToList();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:Using Application.DoEvents", Justification = "For testing")]
		[SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "This is the leak listener, a full GC.Collect is required here to chase down memory leaks")]
		[SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizers", Justification = "This is the leak listener, a full GC.Collect is required here to chase down memory leaks")]
		public ICollection<DisposedNotCollectedType> GetDisposedNotCollectedTypes()
		{
			ICollection<DisposedNotCollectedType> result = null;

			if (WeakReferenceCount > 0)
			{
				if (HasGuiElements())
				{
					guiElementManaager.Value.DoEvents();
				}

				PurgeDeadWeakReferences();

				if (WeakReferenceCount > 0)
				{
					var maxGen = GetMaxGeneration();
					GC.Collect(maxGen);

					result = GetUncollected_ThatAreNotActive_And_NotForAppDomainWorker();

					if (result.Any())
					{
						MemoryManager.Flush(FlushAction.Full, 1);
						GC.Collect();

						result = GetUncollected_ThatAreNotActive_And_NotForAppDomainWorker();
						if (result.Any())
						{
							GC.WaitForPendingFinalizers();
							GC.Collect();
							result = GetUncollected_ThatAreNotActive_And_NotForAppDomainWorker();
						}
					}
				}
			}

			return result ?? new List<DisposedNotCollectedType>(0);
		}

		int WeakReferenceCount
		{
			get
			{
				lock (mutex)
				{
					return weakReferences.Count;
				}
			}
		}

		bool HasGuiElements()
		{
			lock (mutex)
			{
				return weakReferences.Any(wr => guiElementManaager.Value.IsGuiElement(wr.Target) || referencesRequiringWindowsMessagePumpToCollect.Contains(wr));
			}
		}

		int GetMaxGeneration()
		{
			lock (mutex)
			{
				//ArgumentNullException is thrown if wr.Target is null. Happened randomly once.
				return weakReferences.Max(wr =>
				{
					var target = wr.Target;
					if (target != null)
					{
						return GC.GetGeneration(target);
					}
					return 0;
				});
			}
		}

		List<WeakReference> weakReferences = new List<WeakReference>();
		readonly Dictionary<IDisposable, StackTrace> ActiveDisposables = new Dictionary<IDisposable, StackTrace>();
		readonly HashSet<IDisposable> progressFormDisposables = new HashSet<IDisposable>();
		readonly HashSet<WeakReference> referencesRequiringWindowsMessagePumpToCollect = new HashSet<WeakReference>();

		readonly Lazy<IDisposableLeakListenerGuiElementManager> guiElementManaager = new Lazy<IDisposableLeakListenerGuiElementManager>(() => GlobalServiceProvider.Instance.GetRequiredService<IDisposableLeakListenerGuiElementManager>());

		public bool StackTraceEnabled { get; set; }

		internal bool CaptureMemoryDump { get; set; }

		public string AdditionalInformation { get; set; }

		internal const int MaxFailuresPerTest = 10;

		public bool IsRegistered(IDisposable disposable)
		{
			Argument.NotNull(disposable, nameof(disposable));
			return ActiveDisposables.ContainsKey(disposable);
		}

		protected virtual string GetLeakReport(IDisposable leakedObject, Func<string, string> formatStackTrace)
		{
			string objectDescription = GetObjectDescription(leakedObject);
			StackTrace trace;
			try
			{
				trace = ActiveDisposables[leakedObject];
			}
			catch (KeyNotFoundException)
			{
				return string.Empty;
			}
			string stackTrace = trace != null ? trace.ToString() : "Stack Trace disabled for performance - set ZModules.dll!DisposableLeakListener.StackTraceEnabled to true to see object creation call stack";
			string htmlStackTrace = formatStackTrace(stackTrace);
			return string.Format("Type : {0}<br>ToString() or .Name : <b>{1}</b><br>Construction stack trace:<br>{2}<br><br>", leakedObject.GetType().Name, objectDescription, htmlStackTrace);
		}

		string GetObjectDescription(object obj)
		{
			Argument.NotNull(obj, nameof(obj));
			PropertyInfo nameProperty = obj.GetType().GetProperty("Name");
			return nameProperty == null ? obj.ToString() : nameProperty.GetValue(obj, null) as string;
		}

		[SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizers", Justification = "Testing code")]
		public void Verify(StringBuilder errorCollector, Func<string> captureDumpAction = null)
		{
			Argument.NotNull(errorCollector, nameof(errorCollector));
			ICollection<DisposedNotCollectedType> list = null;
			int lastCount;
			DateTime lastCountChanged = DateTime.MinValue;
			do
			{
				lastCount = list != null ? list.Count : 0;
				list = GetDisposedNotCollectedTypes();

				if (lastCount != list.Count)
				{
					if (list.Count > 0)
					{
						GC.WaitForPendingFinalizers();
					}
					lastCountChanged = DateTime.UtcNow;
				}
			} while (list.Count > 0 && (lastCount != list.Count || DateTime.UtcNow - lastCountChanged < TimeSpan.FromSeconds(5) || (HasDispatcherTimers())));

			if (list.Count > 0)
			{
				if (!string.IsNullOrEmpty(AdditionalInformation))
				{
					errorCollector.AppendLine(AdditionalInformation);
				}
				errorCollector.AppendLine("<b>This test has leaked one or more disposed objects that have not been collected by the GC.</b><br/>");
				errorCollector.AppendLine("<p>When you dispose of an object you should also remove all strong references to it to avoid any ObjectDisposedExceptions<p>");
				errorCollector.AppendLine("<p>Each type that has leaked an instance is shown (with the number of leaked objects beside it).<br/>");
				errorCollector.AppendLine("Use Scitech Memory Profiler or another memory profiling tool to find the root references that are holding these uncollectedObjects up.<br/>");

				if (CaptureMemoryDump && captureDumpAction != null)
				{
					var dumpFileUrl = captureDumpAction();

					errorCollector.AppendLine($"Memory dump: <a href=\"{dumpFileUrl}\">{dumpFileUrl}</a><br/>");
				}
				else
				{
					errorCollector.AppendLine($"To automatically capture a memory dump when this fails, add the [CaptureMemoryDumpForDisposableLeak] attribute to your test.<br/>");
				}

				errorCollector.AppendLine("If you cannot identify the root because of an object[][] reference, remove all [ThreadStatic] implementations and run the profiler again.</p>");
				errorCollector.Append("<p>");

				foreach (DisposedNotCollectedType type in list)
				{
					errorCollector.Append(type.ToString() + "<br/>");
				}

				Type userIdleWorkerType = new DefaultAssemblyLoader().LoadAssembly(new AssemblyName("Enterprise.ZArchitecture.GUI")).GetType("Enterprise.ZArchitecture.GUI.UserIdleWorker");
				MethodInfo getLastCallStacksInfo = userIdleWorkerType.GetMethod("get_LastCallStacks", BindingFlags.Public | BindingFlags.Static);
				List<StackTrace> lastCallStacks = (List<StackTrace>)getLastCallStacksInfo.Invoke(null, null);

				if (lastCallStacks.Count > 0)
				{
					int maximumCallStackToReport = Math.Min(10, lastCallStacks.Count);
					errorCollector.Append("<p>");
					errorCollector.Append("The following workitems were added to the Queue after it was flushed which could caused the memory leack:");
					errorCollector.Append("<br>");
					for (int i = 0; i < maximumCallStackToReport; i++)
					{
						errorCollector.Append(string.Format("Workitem {0}<br>", i + 1));
						errorCollector.Append(lastCallStacks[i].ToString().Replace("\r\n", "<br>").Replace("\n", "<br>"));
						errorCollector.Append("<br>");
					}
				}
			}
		}

		bool HasDispatcherTimers()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var windowsBaseAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == "WindowsBase");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			if (windowsBaseAssembly == null)
			{
				return false;
			}
			var dispatcherType = windowsBaseAssembly.GetType("System.Windows.Threading.Dispatcher");
			var currentDispatcher = dispatcherType.GetProperty("CurrentDispatcher").GetValue(null);
			var timersField = dispatcherType.GetField("_timers", BindingFlags.Instance | BindingFlags.NonPublic);
			var timers = (IList)timersField.GetValue(currentDispatcher);
			return timers.Count > 0;
		}
	}

	public class DisposedNotCollectedType
	{
		public DisposedNotCollectedType(string typeName, IEnumerable uncollectedObjects)
		{
			Argument.NotNull(uncollectedObjects, nameof(uncollectedObjects));
			TypeName = typeName;

			foreach (var uncollectedObject in uncollectedObjects)
			{
				Quantity++;

				try
				{
					ToStrings.Add(uncollectedObject.ToString());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is OutOfMemoryException || ex is ThreadAbortException || ex is AppDomainUnloadedException)
					{
						throw;
					}

					//The objects toString may refer to disposed objects
					//Its ok to not get its toString
				}
			}
		}

		public string TypeName { get; internal set; }
		int quantity;
		public int Quantity
		{
			get
			{
				return quantity;
			}
			internal set
			{
				if (value < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(value));
				}

				quantity = value;
			}
		}

		List<string> _toStrings;
		public ICollection<string> ToStrings { get { return _toStrings ?? (_toStrings = new List<string>(Quantity)); } }

		public override string ToString()
		{
			var result = new StringBuilder();
			result.AppendFormat("{0} x {1}", Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), TypeName);

			if (ToStrings.Count != 0)
			{
				result.AppendFormat(" with ToString() values {{'{0}'}}", String.Join("', '", ToStrings));

				//When an error was thrown in a ToString
				if (Quantity > ToStrings.Count)
				{
					result.AppendFormat(" and {0} others", Quantity - ToStrings.Count);
				}
			}

			return result.ToString();
		}
	}
}
