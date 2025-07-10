using System;

namespace Enterprise.ClientSharedComponents
{
	public class SharedProgressEventArgs : EventArgs
	{
		public SharedProgressEventArgs(int currentProgress, int total)
		{
			percentComplete = -1;
			CurrentProgress = currentProgress;
			Total = total;
		}

		public SharedProgressEventArgs(int currentProgress, int total, string message)
		{
			percentComplete = -1;
			CurrentProgress = currentProgress;
			Total = total;
			Message = message;
		}

		public SharedProgressEventArgs(int percentComplete, string message)
		{
			this.percentComplete = percentComplete;
			CurrentProgress = 0;
			Total = 0;
			Message = message;
		}

		public int PercentComplete
		{
			get
			{
				int result;
				if (percentComplete == -1)
				{
					result = CalculatePercentComplete();
				}
				else
				{
					result = percentComplete;
				}
				return result < 100 ? result : 100;
			}
		}

		int CalculatePercentComplete()
		{
			return (int)Enterprise.ZArchitecture.Core.Utilities.Round((CurrentProgress / (decimal)Total) * 100, 0);
		}

		readonly int percentComplete;
		public readonly string Message;
		public readonly int CurrentProgress;
		public readonly int Total;
	}

	public delegate void SharedProgressEventHandler(object sender, SharedProgressEventArgs e);
}
