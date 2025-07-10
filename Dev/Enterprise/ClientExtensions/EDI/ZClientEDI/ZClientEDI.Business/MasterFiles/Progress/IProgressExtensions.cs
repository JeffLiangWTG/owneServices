namespace Enterprise.Client.EDI.MasterFiles.Progress
{
	public static class IProgressExtensions
	{
		public static void SafeSetStatusAndPercentComplete(this IEdiProgress progress, string status, int percentComplete)
		{
			if (progress != null)
			{
				progress.SetStatusAndPercentComplete(status, percentComplete);
			}
		}

		public static void SafeSetExpectedCount(this IEdiProgress progress, int count)
		{
			if (progress != null)
			{
				progress.SetExpectedCount(count);
			}
		}

		public static void SafeUpdateCurrentCount(this IEdiProgress progress, string status)
		{
			if (progress != null)
			{
				progress.UpdateCurrentCount(status);
			}
		}

		public static bool SafeIsCancelled(this IEdiProgress progress)
		{
			if (progress != null)
			{
				return progress.IsCancelled;
			}

			return false;
		}
	}
}


