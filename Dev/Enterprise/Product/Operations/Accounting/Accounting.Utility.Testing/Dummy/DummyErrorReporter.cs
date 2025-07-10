using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Accounting.Utility.Testing
{
	public class DummyErrorReporter : IErrorReporter
	{
		public void Clear()
		{
			Reports.Clear();
		}

		public void Report(string key, string message, Exception exception)
		{
			Reports.Add(Tuple.Create(key, message, exception));
		}

		public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception exception)
		{
			Reports.Add(Tuple.Create(key, message, exception));
		}

		public List<Tuple<string, string, Exception>> Reports { get; } = new List<Tuple<string, string, Exception>>();
	}
}
