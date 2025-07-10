using System.Globalization;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public class SimpleLog : ISimpleLog
	{
		public string Message { get; private set; }
		public LogType Type { get; private set; }
		ZDateTime DateTime { get; set; }

		public SimpleLog(LogType type, string message)
		{
			Type = type;
			Message = message;
		}

		public SimpleLog(LogType type, ZDateTime dateTime, string message)
			: this(type, message)
		{
			DateTime = dateTime;
		}

		public override string ToString()
		{
			var stringBuilder = new ZStringBuilder();

			if (!DateTime.IsEmpty)
			{
				stringBuilder
					.Append(DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture))
					.Append("| ");
			}

			if (Type != LogType.Information)
			{
				stringBuilder
					.Append(Type.ToString())
					.Append(" - ");
			}

			return stringBuilder.Append(Message).ToString();
		}
	}
}
