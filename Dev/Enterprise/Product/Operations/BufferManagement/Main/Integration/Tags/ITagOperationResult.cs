using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface ITagOperationResult
	{
		bool WasSuccessful { get; }
		string Message { get; }
		ITagLink Link { get; }
	}
}
