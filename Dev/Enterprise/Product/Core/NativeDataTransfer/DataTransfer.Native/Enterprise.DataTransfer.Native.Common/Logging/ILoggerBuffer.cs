using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.Common.Logging
{
	public interface ILogBuffer
	{
		bool HasError();
		bool HasWarning();
		bool HasInfo();
		IEnumerable<Log> Logs();
		IEnumerable<string> Errors { get; }
		IEnumerable<string> Warnings { get; }
		IEnumerable<string> Infos { get; }
	}
}