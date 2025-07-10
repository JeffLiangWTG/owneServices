using System;
using System.Collections;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DummyLogger : ILogger, IEnumerable<string>
	{
		readonly List<string> logs = new List<string>();

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			logs.Add(type.ToString() + " - " + message + " :- Exception [" + ex.GetType().FullName + "] - " + ex.Message);
		}

		void ILogger.Log(LogType type, string message)
		{
			logs.Add(type.ToString() + " - " + message);
		}

		#region IEnumerable<string> Members

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			return logs.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return logs.GetEnumerator();
		}

		#endregion
	}
}
