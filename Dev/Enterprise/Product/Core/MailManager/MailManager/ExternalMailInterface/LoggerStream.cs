using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class LoggerStream : Stream
	{
		public LoggerStream(ILogger logger)
		{
			logAction = log => logger?.Log(LogType.Debug, log.TrimEnd('\0'));
		}

		public LoggerStream(LogMessageHandler handler)
		{
			logAction = log => handler?.Invoke(TraceEventType.Verbose, log.TrimEnd('\0'));
		}

		readonly Action<string> logAction;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => 0;

		public override long Position { get => 0; set => throw new NotImplementedException(); }

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			var msg = Encoding.UTF8.GetString(buffer);
			logAction?.Invoke(msg);
		}
	}
}
