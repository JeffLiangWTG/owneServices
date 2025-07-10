using System;

namespace Enterprise.Client.TNT
{
	public class TNTProgressEventArgs : EventArgs
	{
		public TNTProgressEventArgs(int current, int total, string message) : this((current * 100) / total, message)
		{
		}

		public TNTProgressEventArgs(int percentComplete, string message)
		{
			this.PercentComplete = percentComplete;
			this.Message = message;
		}

		public int PercentComplete;
		public string Message;
	}

	public delegate void TNTProgressEventHandler(object sender, TNTProgressEventArgs e);
}
