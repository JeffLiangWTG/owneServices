
namespace Enterprise.Client.EDI.MasterFiles.Progress
{
	public interface IEdiProgress
	{
		void SetStatusAndPercentComplete(string status, int percentComplete);

		void SetExpectedCount(int count);
		void UpdateCurrentCount(string status);

		bool IsCancelled { get; }
	}
}
