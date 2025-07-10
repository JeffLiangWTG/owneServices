using Enterprise.ZArchitecture.Favorites;

namespace Enterprise.Core.Modules
{
	public interface IFavoriteProvider
	{
		bool AddToFavorites(LinkWrapper shortcut);
		void DeleteFromFavorites(LinkWrapper shortcut);
		bool IsInFavorites(LinkWrapper shortcut);
		void AddToRecentItems(LinkWrapper shortcut);
		void RemoveFromRecentItems(LinkWrapper shortcut);
	}
}
