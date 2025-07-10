using System.ComponentModel;

namespace Enterprise.DocumentScanning.Business
{
	public sealed class AmsiSession : IAmsiSession
	{
		readonly AmsiContextSafeHandle _context;
		readonly AmsiSessionSafeHandle _session;

		internal AmsiSession(AmsiContextSafeHandle context, AmsiSessionSafeHandle session)
		{
			_context = context;
			_session = session;
		}

		public bool IsMalware(string payload, string contentName)
		{
			var returnValue = Amsi.AmsiScanString(_context, payload, contentName, _session, out var result);
			if (returnValue != 0)
			{
				throw new Win32Exception(returnValue);
			}

			return Amsi.AmsiResultIsMalware(result);
		}

		public bool IsMalware(byte[] payload, string contentName)
		{
			var returnValue = Amsi.AmsiScanBuffer(_context, payload, (uint)payload.Length, contentName, _session, out var result);
			if (returnValue != 0)
			{
				throw new Win32Exception(returnValue);
			}

			return Amsi.AmsiResultIsMalware(result);
		}

		public void Dispose()
		{
			_session.Dispose();
		}
	}
}
