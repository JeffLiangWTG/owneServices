using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObjectWithOwnImageGenerationLogic : IArchiveableBusinessObject
	{
		IEnumerable<ArchiveImageDescriptor> GenerateArchiveImages();
	}
}
