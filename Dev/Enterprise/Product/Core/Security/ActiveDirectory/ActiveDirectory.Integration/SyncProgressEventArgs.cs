using System;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public class SyncProgressEventArgs : EventArgs
	{
		public SyncProgressEventArgs()
		{
			LogType = LogType.Information;
		}

		public string TaskName { get; set; }
		public int? OverallPercentComplete { get; set; }
		public string TaskDetails { get; set; }
		public LogType LogType { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as SyncProgressEventArgs;
			return other != null
				&& other.TaskName == TaskName
				&& other.TaskDetails == TaskDetails
				&& other.OverallPercentComplete == OverallPercentComplete
				&& other.LogType == LogType;
		}

		public override int GetHashCode()
		{
			return (TaskName + TaskDetails + LogType.ToString() + OverallPercentComplete).GetHashCode();
		}
	}
}
