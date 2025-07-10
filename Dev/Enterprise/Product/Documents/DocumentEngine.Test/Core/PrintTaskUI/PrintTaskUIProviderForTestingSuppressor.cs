using System;

namespace Enterprise.DocumentEngine.Testing
{
	public class PrintTaskUIProviderForTestingSuppressor : IDisposable
	{
		internal PrintTaskUIProviderForTestingSuppressor()
		{
			if (Enabled)
			{
				throw new InvalidOperationException("Only one suppressor can be enabled at any one time.");
			}

			Enabled = true;
		}

		public static bool Enabled { get; private set; }

		void IDisposable.Dispose()
		{
			Enabled = false;
		}
	}
}
