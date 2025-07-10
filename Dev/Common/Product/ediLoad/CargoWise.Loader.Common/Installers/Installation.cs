using System;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public class Installation : InstallationItem
	{
		readonly Configuration configuration;

		public Installation(Configuration configuration)
			: base(null)
		{
			Argument.NotNull(configuration, nameof(configuration));
			this.configuration = configuration;
		}

		public Configuration Configuration
		{
			get
			{
				return configuration;
			}
		}

		public event EventHandler<TaskDescriptionChangedEventArgs> CurrentTaskDescriptionChanged;
		public event EventHandler Progress;

		protected sealed override bool NeedsToInstallCore()
		{
			return true;
		}

		public void OnCurrentTaskDescriptionChanged(string taskDescription)
		{
			EventHandler<TaskDescriptionChangedEventArgs> currentTaskDescriptionChanged = CurrentTaskDescriptionChanged;
			if (currentTaskDescriptionChanged != null)
			{
				currentTaskDescriptionChanged(this, new TaskDescriptionChangedEventArgs(taskDescription));
			}
		}

		public void OnProgress()
		{
			EventHandler progress = Progress;
			if (progress != null)
			{
				progress(this, EventArgs.Empty);
			}
		}
	}
}
