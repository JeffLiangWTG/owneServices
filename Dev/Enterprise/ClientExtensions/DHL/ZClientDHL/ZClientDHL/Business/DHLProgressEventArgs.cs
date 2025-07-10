using System;

namespace Enterprise.Client.DHL
{
	public class DHLProgressEventArgs : EventArgs
	{
		public DHLProgressEventArgs(int current, int total, string message)
			: this((current * 100 / total), message)
		{
		}

		public DHLProgressEventArgs(int percentComplete, string message)
		{
			this.PercentComplete = percentComplete < 100 ? percentComplete : 100;
			this.Message = message;
		}

		public int PercentComplete;
		public string Message;
	}

	public delegate void DHLProgressEventHandler(object sender, DHLProgressEventArgs e);
}
