namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class DataTransferProcessor
	{
		public abstract void Import();
		public abstract void Rollback();

		public delegate bool ProgressChangedEventHandler(int percentComplete, string status);
		public event ProgressChangedEventHandler ProgressChanged;

		public bool OnProgressChanged(int percentage, string status)
		{
			if (ProgressChanged != null)
			{
				return ProgressChanged(percentage, status);
			}

			return true;
		}

		public delegate void SavingEventHandler(int percentage, string status);
		public event SavingEventHandler Saving;
		protected void OnSaving(int percentage, string status)
		{
			if (Saving != null)
			{
				Saving(percentage, status);
			}
		}

		public delegate void SavingCompleteEventHandler(int percentage, string status);
		public event SavingCompleteEventHandler SavingComplete;
		protected void OnSavingComplete(int percentage, string status)
		{
			if (SavingComplete != null)
			{
				SavingComplete(percentage, status);
			}
		}

		public bool IsCanceled { get; set; }
	}
}
