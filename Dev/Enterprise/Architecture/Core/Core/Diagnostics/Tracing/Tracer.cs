using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	internal class Tracer : ITracer, ITracerConfiguration
	{
		internal Tracer()
		{
			TraceSources = new Dictionary<ZString, TraceSource>();
			SyncRoot = new object();
		}

		#region ITracer

		public bool IsEnabled(ZString traceSourceCode)
		{
			lock (SyncRoot)
			{
				return TraceSources.TryGetValue(traceSourceCode, out var source)
					&& source.Listeners.Count > 0
					&& source.Switch.Level != SourceLevels.Off;
			}
		}

		public void TraceCriticalEvent(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Critical, buildMessage);

		public void TraceErrorEvent(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Error, buildMessage);

		public void TraceWarningEvent(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Warning, buildMessage);

		public void TraceInformation(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Information, buildMessage);

		public void TraceVerbose(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Verbose, buildMessage);

		public void TraceStartingOfActivity(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Start, buildMessage);

		public void TraceStoppingOfActivity(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Stop, buildMessage);

		public void TraceSuspensionOfActivity(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Suspend, buildMessage);

		public void TraceResumptionOfActivity(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Resume, buildMessage);

		public void TraceTransferEvent(ZString traceSourceCode, Func<string> buildMessage)
			=> Trace(traceSourceCode, TraceEventType.Transfer, buildMessage);

		void Trace(ZString traceSourceCode, TraceEventType eventType, Func<string> buildMessage)
		{
			lock (SyncRoot)
			{
				if (IsEnabled(traceSourceCode))
				{
					var traceSource = this[traceSourceCode];
					var msgNumber = (traceSource.Listeners[0] as MessageWriterTraceListener).Counter;
					traceSource.TraceEvent(eventType, msgNumber, buildMessage?.Invoke());
				}
			}
		}

		public string GetTraceFilter(ZString traceSourceName)
		{
			var traceSource = this[traceSourceName];
			return traceSource.Listeners.Count > 0 ? (traceSource.Listeners[0] as MessageWriterTraceListener).TraceFilter : "";
		}

		#endregion

		#region ITracerConfiguration

		public void InitializeTraceSources(TraceSourceConfiguration traceSourceConfiguration)
		{
			foreach (TraceSourceSettingsConfiguration settings in traceSourceConfiguration.TraceSourceSettings)
			{
				var traceLevel = settings.SourceLevel;

				var source = this[settings.TraceSourceName];
				source.Switch = new SourceSwitch(settings.TraceSourceName, settings.TraceSourceDescription);
				source.Switch.Level = traceLevel;

				source.Listeners.Clear();
				var listener = new MessageWriterTraceListener(traceSourceConfiguration.MessageWriter, settings.TraceFilter, traceSourceConfiguration.GetTracePrefix);
				listener.Filter = new EventTypeFilter(traceLevel);
				listener.TraceOutputOptions = TraceOptions.None;
				if (traceSourceConfiguration.LogCallStack)
				{
					listener.TraceOutputOptions |= TraceOptions.Callstack;
				}
				if (traceSourceConfiguration.LogDateTime)
				{
					listener.TraceOutputOptions |= TraceOptions.DateTime;
				}
				if (traceSourceConfiguration.LogThreadId)
				{
					listener.TraceOutputOptions |= TraceOptions.ThreadId;
				}
				if (traceSourceConfiguration.LogProcessId)
				{
					listener.TraceOutputOptions |= TraceOptions.ProcessId;
				}
				source.Listeners.Add(listener);
			}
		}

		public void RemoveAllTraceSources()
		{
			RemoveTraceSource(TraceSources.Keys.ToArray());
		}

		public void RemoveTraceSource(params ZString[] traceSourceNames)
		{
			lock (SyncRoot)
			{
				if (traceSourceNames != null)
				{
					foreach (var sourceName in traceSourceNames)
					{
						if (TraceSources.ContainsKey(sourceName))
						{
							var traceSource = TraceSources[sourceName];
							traceSource.Close();
							traceSource.Listeners
								.Cast<TraceListener>()
								.ToList()
								.ForEach(l => l.Dispose());

							TraceSources.Remove(sourceName);
						}
					}
				}
			}
		}

		public bool CheckWhetherTraceSourceExists(params ZString[] traceSourceName)
		{
			lock (SyncRoot)
			{
				return TraceSources.Any(traceSource => traceSourceName.Contains(traceSource.Key) && traceSource.Value.Listeners.Count > 0);
			}
		}

		#endregion

		internal TraceSource this[string traceSourceName] => GetTraceSource(traceSourceName);

		internal int CountTraceSources => TraceSources.Count;

		TraceSource GetTraceSource(string traceSourceName)
		{
			lock (SyncRoot)
			{
				var source = TraceSources.GetOrAdd(traceSourceName, () => CreateTraceSource());
				return source;
			}

			TraceSource CreateTraceSource()
			{
				var traceSource = new TraceSource(traceSourceName);
				traceSource.Listeners.Clear();
				return traceSource;
			}
		}

		Dictionary<ZString, TraceSource> TraceSources { get; }

		object SyncRoot { get; }
	}
}
