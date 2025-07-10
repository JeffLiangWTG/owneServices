namespace ServiceManager.Logging.Abstractions
{
	public interface IServiceTaskErrorTracker
	{
		void TrackServiceTaskError(string taskCode);
	}
}
