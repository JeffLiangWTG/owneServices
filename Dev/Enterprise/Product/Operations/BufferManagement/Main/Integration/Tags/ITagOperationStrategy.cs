using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface ITagOperationStrategy
	{
		ITagOperationResult AddTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog = true);
		ITagOperationResult RemoveTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog = true);
	}
}
