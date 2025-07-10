namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchivePreparationAction
	{
		/// <summary>
		/// A preparation action should be saved independent of whether any other archive actions succeeds or not.
		/// It must also be re-runnable, ie internally check if it's already done, then dont do it again.
		/// </summary>
		void Execute();
	}
}
