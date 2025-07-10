using System;

namespace Enterprise.ZArchitecture
{
	public class ProgressEventArgs : EventArgs
	{
		public ProgressEventArgs(int currentProgress, int total)
		{
			this.CurrentProgress = currentProgress;
			this.Total = total;
		}

		#region PercentComplete

		public int PercentComplete
		{
			get { return (int)Enterprise.ZArchitecture.Core.Utilities.Round((CurrentProgress / (decimal)Total) * 100, 0); }
		}

		#endregion

		public readonly int CurrentProgress;
		public readonly int Total;
	}
}
