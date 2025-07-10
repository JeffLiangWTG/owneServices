
using System;
using Enterprise.RemoteDesktopServices.Testing;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	public class LogService : ILogService
	{
		public void Log(string message)
		{
			Console.Out.WriteLine(message + RemoteDesktopServicesTest.ControlChars);
		}

		public void Log(string message, Exception ex)
		{
			Console.Error.WriteLine($"{message} {Environment.NewLine} {ex}");
		}
	}
}
