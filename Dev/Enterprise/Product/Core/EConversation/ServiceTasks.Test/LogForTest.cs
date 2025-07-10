using System;
using System.Globalization;
using Enterprise.Integration;

namespace Enterprise.EConversation.Testing
{
	public class LogForTest
	{
		public LogForTest(LogType type, string message, Exception ex)
		{
			this.type = type;
			this.message = message;
			this.ex = ex;
		}

		public override bool Equals(object obj)
		{
			LogForTest rhs = obj as LogForTest;
			return rhs != null && type == rhs.type && message == rhs.message;
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() ^ message.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}: {1}", type, message);
		}

		public readonly LogType type;
		public readonly string message;
		public readonly Exception ex;
	}
}
