namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveImageGenerationAction : IArchivePreparationAction
	{
		void Setup(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveableBusinessObjectProviderCache providerDictionary, IArchiveSystemCache systemCache);
	}
}
