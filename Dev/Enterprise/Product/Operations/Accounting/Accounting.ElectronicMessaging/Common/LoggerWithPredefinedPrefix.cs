using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	#region SuppressResourceStringsCheckRegion

	public class LoggerWithPredefinedPrefix : ILogger
	{
		public LoggerWithPredefinedPrefix(ILogger serviceTasklogger)
		{
			Prefixes = new Stack<string>();
			ServiceTasklogger = serviceTasklogger;
		}
		Stack<string> Prefixes { get; }

		ILogger ServiceTasklogger { get; }

		public DisposableAction SetPrefix(string prefix)
		{
			return new DisposableAction(
				() => Prefixes.Push(prefix),
				() =>
				{
					if (Prefixes.Count > 0)
					{
						Prefixes.Pop();
					}
				});
		}

		public void Log(LogType type, string message)
		{
			ServiceTasklogger.Log(type, FormattableString.Invariant($"{GetPrefixAsString()} : {message}"));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			ServiceTasklogger.Log(type, FormattableString.Invariant($"{GetPrefixAsString()} : {message}"));
		}

		string GetPrefixAsString() => Prefixes.Count > 0 ? FormattableString.Invariant($"[{string.Join("][", Prefixes.Reverse().ToArray())}]") : string.Empty;
	}

	#endregion
}