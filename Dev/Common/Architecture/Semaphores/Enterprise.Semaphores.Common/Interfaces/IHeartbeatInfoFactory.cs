namespace Enterprise.Semaphores.Common
{
	public interface IHeartbeatInfoFactory
	{
		/// <summary>
		/// Creates new Hertbeat Session Info
		/// </summary>
		/// <returns>Heartbeat Session Info</returns>
		IHeartbeatInfo New();
	}
}
