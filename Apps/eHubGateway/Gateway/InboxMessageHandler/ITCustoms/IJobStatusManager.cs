namespace CargoWise.eHub.Gateway.ITCustoms
{
	public interface IJobStatusManager
	{
		void TriggerJobStatus(string clientSystemID, Files files);
	}
}
