using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CargoWise.Common
{
	class DeferredError
	{
		public string Key { get; }
		public string Message { get; }
		public Exception Exception { get; }
		public Action<string, string, Exception> ReportStrategy { get; }

		DeferredError(string key, string message, Exception exception, Action<string, string, Exception> reportStrategy)
		{
			Key = key;
			Message = message;
			ReportStrategy = reportStrategy;
			Exception = exception;
		}

		public static DeferredError New(string key, string message, Exception exception, Action<string, string, Exception> reportStrategy)
		{
			string finalKey = key;
			if (string.IsNullOrEmpty(key) && exception?.StackTrace is null)
			{
				finalKey = GenerateKeyForDeferredErrorWithNoStackTrace();
			}
			return new DeferredError(finalKey, message, exception, reportStrategy);
		}

		static string GenerateKeyForDeferredErrorWithNoStackTrace()
		{
			var stackTrace = new StackTrace();
			var frames = stackTrace.GetFrames();
			var entryToErrorReporter = frames.Last(x => x.GetMethod().ReflectedType == typeof(ErrorReporter));
			var cutOffIndex = frames.IndexOf(f => f == entryToErrorReporter);
			var topFrames = frames.Skip(cutOffIndex).Take(10).ToArray();
			var lastFrame = topFrames.LastOrDefault();

			var builder = new StringBuilder("----- Deferred Error With No Stack Trace -----").AppendLine();

			foreach (var frame in topFrames)
			{
				var method = frame.GetMethod();
				builder.Append(" at ");
				builder.Append(method.ReflectedType.FullName);
				builder.Append('.');
				builder.Append(method.Name);
				builder.Append('(');
				var firstParam = true;
				foreach (var param in method.GetParameters())
				{
					if (!firstParam)
					{
						builder.Append(", ");
					}
					builder.Append(param.ParameterType);
					builder.Append(' ');
					builder.Append(param.Name);
					firstParam = false;
				}
				builder.Append(')');
				builder.AppendLine();
			}

			return builder.ToString();
		}
	}
}
