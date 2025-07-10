using System;

namespace CargoWise.Loader.Common
{
	public sealed class TaskDescriptionChangedEventArgs : EventArgs
	{
		readonly string taskDescription;

		public TaskDescriptionChangedEventArgs(string taskDescription)
		{
			this.taskDescription = taskDescription;
		}

		public string TaskDescription
		{
			get { return taskDescription; }
		}
	}
}

