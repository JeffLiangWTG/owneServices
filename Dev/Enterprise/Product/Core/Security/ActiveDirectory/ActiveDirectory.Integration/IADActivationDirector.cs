using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADActivationDirector
	{
		void EnableIntegration(EntitiesToSync entitiesToSync);
		void DisableIntegration(bool disableGroupOnly = false);

		bool HasChanges { get; }
		void SaveChanges();
	}
}
