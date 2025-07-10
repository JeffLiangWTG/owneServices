namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveImageDeletionAction : IArchivePreparationAction
	{
		void Setup(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache systemCache, IArchiveConfiguration config);
	}
}
