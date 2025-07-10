using System.ComponentModel;

namespace Enterprise.DocumentScanning.Business
{
	public sealed class AmsiContext : IAmsiContext
	{
		readonly AmsiContextSafeHandle _context;

		public AmsiContext(string applicationName)
		{
			var result = Amsi.AmsiInitialize(applicationName, out var context);
			if (result != 0)
			{
				throw new Win32Exception(result);
			}

			_context = context;
		}

		public IAmsiSession CreateSession()
		{
			var result = Amsi.AmsiOpenSession(_context, out var session);
			session.Context = _context;
			if (result != 0)
			{
				throw new Win32Exception(result);
			}

			return new AmsiSession(_context, session);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
