namespace Enterprise.ArchiveManager.Integration
{
	// TODO - This will be moved into IArchiveStageDescriptor in WI00885920 because all system
	// and stage descriptors should be completely self contained, but to extract the logic
	// and add all the required properties in each system descriptor is too much for this WI
	public interface ISelfContainedArchiveSystemDescriptor : IArchiveSystemDescriptor
	{
		int OnOrBeforeMinimumValue { get; }

		int BatchSizeControlValue { get; }
	}
}
