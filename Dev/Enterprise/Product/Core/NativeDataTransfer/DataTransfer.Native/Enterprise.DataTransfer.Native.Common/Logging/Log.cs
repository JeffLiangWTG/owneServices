using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Common.Logging
{
	public class Log
	{
		public Log(string message, LogType type)
		{
			Message = message;
			Type = type;
		}

		public readonly string Message;
		public readonly LogType Type;

		public override string ToString()
		{
			return Type + " - " + Message;
		}
	}
}