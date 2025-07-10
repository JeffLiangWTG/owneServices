using System;

namespace Enterprise.ZArchitecture.GUI
{
	#region class LoggedEventArgs

	public sealed class LoggedEventArgs : EventArgs
	{
		public LoggedEventArgs(bool succeeded)
		{
			this.succeeded = succeeded;
		}

		public bool Succeeded
		{
			get { return succeeded; }
		}

		readonly bool succeeded;
	}

	#endregion
}