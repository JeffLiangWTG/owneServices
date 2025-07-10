namespace Enterprise.BufferManagement.Business
{
	public readonly struct TaskLogDetails
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public TaskLogDetails(string taskID, decimal standardEstimateHours, string requiredCapabilityName = null)
		{
			TaskID = taskID;
			StandardEstimateHours = standardEstimateHours;
			RequiredCapabilityName = requiredCapabilityName;
		}

		public string TaskID { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal StandardEstimateHours { get; }
		public string RequiredCapabilityName { get; }

		#region Equality

		public override bool Equals(object obj)
		{
			return obj is TaskLogDetails other && this == other;
		}

		public override int GetHashCode()
		{
			return TaskID.GetHashCode() ^ StandardEstimateHours.GetHashCode() ^ RequiredCapabilityName?.GetHashCode() ?? 0;
		}

		public static bool operator ==(TaskLogDetails lhs, TaskLogDetails rhs)
		{
			return lhs.TaskID == rhs.TaskID
				&& lhs.StandardEstimateHours == rhs.StandardEstimateHours
				&& lhs.RequiredCapabilityName == rhs.RequiredCapabilityName;
		}

		public static bool operator !=(TaskLogDetails lhs, TaskLogDetails rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}
}
