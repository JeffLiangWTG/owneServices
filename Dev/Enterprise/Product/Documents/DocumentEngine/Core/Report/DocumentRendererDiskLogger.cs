namespace Enterprise.DocumentEngine
{
	using System.Diagnostics;
	using Enterprise.DocumentEngineCore.Registry;

	class DocumentRendererDiskLogger : IDocumentRendererLogger
	{
		public DocumentRendererDiskLogger()
		{
			var registry = DocumentsDataRegistry.Instance.LogDocumentRenderer.Value;
			var path = registry.LogFilePath.Trim();
			if (!path.IsEmpty)
			{
				listener = new TextWriterTraceListener(path);
				listener.TraceOutputOptions = TraceOptions.ProcessId | TraceOptions.ThreadId | TraceOptions.DateTime;

				if (registry.GenerateCallStacks)
				{
					listener.TraceOutputOptions |= TraceOptions.Callstack;
				}

				Trace.Listeners.Add(listener);
			}
		}

		readonly TraceListener listener;

		public void Log(string message)
		{
			if (listener != null)
			{
				Trace.TraceInformation(message);
			}
		}

		public void Dispose()
		{
			if (listener != null)
			{
				Trace.Listeners.Remove(listener);
				listener.Dispose();
			}
		}
	}
}
