
namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	public interface ISyncStrategy
	{
		EntitySynchronisedEventArgs Synchronise();
	}
}
