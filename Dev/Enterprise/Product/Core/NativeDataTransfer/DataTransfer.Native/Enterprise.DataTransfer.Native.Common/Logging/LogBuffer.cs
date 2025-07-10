using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Common.Logging
{
	public class LogBuffer : ILogBuffer
	{
		public LogBuffer(IEnumerable<Log> buffer)
		{
			this.buffer = buffer;
		}
		readonly IEnumerable<Log> buffer;

		#region ILogBuffer Members

		public bool HasError()
		{
			return buffer.Any(item => item.Type == LogType.Error);
		}

		public bool HasWarning()
		{
			return buffer.Any(item => item.Type == LogType.Warning);
		}

		public bool HasInfo()
		{
			return buffer.Any(item => item.Type == LogType.Information);
		}

		public IEnumerable<string> Errors
		{
			get
			{
				return from item in buffer
					   where item.Type == LogType.Error
					   select item.Message;
			}
		}

		public IEnumerable<string> Warnings
		{
			get
			{
				return from item in buffer
					   where item.Type == LogType.Warning
					   select item.Message;
			}
		}

		public IEnumerable<string> Infos
		{
			get
			{
				return from item in buffer
					   where item.Type == LogType.Information
					   select item.Message;
			}
		}

		public IEnumerable<Log> Logs()
		{
			return buffer;
		}

		#endregion
	}
}