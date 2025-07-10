using System;

using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	static class ControlExtensions
	{
		public static IDisposable SuspendUserInterface(this Control controlToSuspend)
		{
			return new UserInterfaceSuspender(controlToSuspend);
		}

		class UserInterfaceSuspender : IDisposable
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
				if (controlToSuspend == null)
				{
					throw new InvalidOperationException("controlToSuspend should not be null");
				}
				controlToSuspend.Enabled = previousEnabledState;
			}
		}
	}
}
