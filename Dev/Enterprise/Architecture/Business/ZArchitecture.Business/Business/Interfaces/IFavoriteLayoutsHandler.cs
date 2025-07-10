namespace Enterprise.ZArchitecture.Business
{
	public interface IFavoriteLayoutsHandler
	{
		IStmLinkCollection<IStmLink> FavoriteFilters { get; }
		void AddOrRemoveFavoriteFilter(ILinkWrapper filter);
	}
}
