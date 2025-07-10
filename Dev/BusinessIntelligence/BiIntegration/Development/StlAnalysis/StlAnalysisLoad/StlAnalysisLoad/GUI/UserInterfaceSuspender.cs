namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Windows.Forms;

	public class UserInterfaceSuspender : IDisposable
	{
		public UserInterfaceSuspender(Control controlToSuspend)
		{
			this.controlToSuspend = controlToSuspend;
			previousEnabledState = controlToSuspend.Enabled;
			controlToSuspend.Enabled = false;
		}

		readonly bool previousEnabledState;
		readonly Control controlToSuspend;

		public void Dispose()
		{
			controlToSuspend.Enabled = previousEnabledState;
		}
	}
}
