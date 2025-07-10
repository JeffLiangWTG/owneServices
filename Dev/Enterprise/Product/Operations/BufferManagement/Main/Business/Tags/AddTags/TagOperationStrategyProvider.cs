using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	class TagOperationStrategyProvider : ITagOperationStrategy
	{
		ITagOperationResult ITagOperationStrategy.AddTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			return GetStrategy(magnitude).AddTag(tagable, magnitude, showSecurityDialog);
		}

		ITagOperationResult ITagOperationStrategy.RemoveTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			return GetStrategy(magnitude).RemoveTag(tagable, magnitude, showSecurityDialog);
		}

		static ITagOperationStrategy GetStrategy(ITagMagnitude magnitude)
		{
			return magnitude is WorkQueue ? new WorkQueueTagOperationStrategy() : new DefaultTagOperationStrategy();
		}
	}
}
