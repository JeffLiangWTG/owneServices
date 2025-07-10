using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public class FavoriteLayoutsHandler : IFavoriteLayoutsHandler
	{
		public FavoriteLayoutsHandler()
		{
		}

		public IStmLinkCollection<IStmLink> FavoriteFilters
		{
			get
			{
				if (favoriteFilters == null)
				{
					favoriteFilters = new StmLinkCollection(factory, GetFavoriteFiltersQuery(), StmLinkConstants.FavoriteLayoutFilters, MaxNoOfFavoriteFilters, StmLinkSchema.Constants.STL_LastUsedDateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
					favoriteFilters.CleanUp();
				}

				return favoriteFilters;
			}
		}
		StmLinkCollection favoriteFilters;

		int MaxNoOfFavoriteFilters => 10;

		ZQuery GetFavoriteFiltersQuery()
		{
			var filter = new ZQuery(StmLinkSchema.STL_LinkType, StmLinkConstants.FavoriteLayoutFilters);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_GS_NKUser, EnvProxy.Instance.CurrentUser.Initials);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_GC_LogonCompany, EnvProxy.Instance.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_ModuleID, module);

			return filter;
		}

		readonly string module;
		readonly BusinessObjectFactory factory;
		public FavoriteLayoutsHandler(BusinessObjectFactory factory, string module)
		{
			this.factory = factory;
			this.module = module;
		}

		bool AddToFavoriteFilters(LinkWrapper filter)
		{
			if (!IsInFavoriteFilters(filter))
			{
				(FavoriteFilters as StmLinkCollection).AddToCollection(filter);
				factory.Save();
				return true;
			}

			return false;
		}

		internal bool IsInFavoriteFilters(LinkWrapper filter)
			=> (FavoriteFilters as StmLinkCollection).OfType<StmLink>().Any(t => LinkWrapper.AreShortcutsEqual(filter, t));

		internal void RemoveAllFavoriteFilters()
		{
			(FavoriteFilters as StmLinkCollection).DeleteAll();
			factory.Save();
		}

		void RemoveFromFavoriteFilters(LinkWrapper filter)
		{
			(FavoriteFilters as StmLinkCollection).RemoveFromCollection(filter);

			factory.Save();
		}

		public void AddOrRemoveFavoriteFilter(ILinkWrapper filter)
		{
			var linkWrapperFilter = filter as LinkWrapper;
			if ((FavoriteFilters as StmLinkCollection).Count >= MaxNoOfFavoriteFilters && !IsInFavoriteFilters(linkWrapperFilter))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Limit of 10 favorites reached.{0}You first need to remove a favorite if you want to add '{1}'", System.Environment.NewLine, (filter as LinkWrapper).RecordDescription));
			}

			try
			{
				if (!AddToFavoriteFilters(linkWrapperFilter))
				{
					RemoveFromFavoriteFilters(linkWrapperFilter);
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
		}
	}
}
