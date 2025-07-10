using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebServiceLevelCollection : ActiveServiceLevelCollection
	{
		public WebServiceLevelCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public WebServiceLevelCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, CombineWithPublishedFilter(filter, JoinCondition.And))
		{
		}

		public WebServiceLevelCollection(BusinessObjectFactory factory, ZQuery filter, JoinCondition condition)
			: base(factory, CombineWithPublishedFilter(filter, condition))
		{
		}

		internal static ZQuery CombineWithPublishedFilter(ZQuery filter, JoinCondition condition)
		{
			ZQuery publishedFilter = new ZQuery();

			if (Globals.IsWeb)
			{
				bool found = false;
				var loggedInOrganisation =
#if DEBUG
				LoggedInOrgHeaderForTest ??
#endif
#if NETFRAMEWORK
				((OrgContactWebUser)(WebEnv.AppInstance?.SiteUser))?.LoggedInOrganisation;
#elif NET
				((OrgContactWebUser)(WebEnv.SiteUser))?.LoggedInOrganisation;
#endif

				if (loggedInOrganisation != null)
				{
					var levels = loggedInOrganisation.OrgServiceLevels
						.Where(level => level.PM_IsPublished)
						.Select(level => level.PM_RS_NKSrvLvl)
						.ToArray();
					found = levels.Length > 0;

					publishedFilter.AddToFilter(JoinCondition.Or, RefServiceLevelSchema.RS_Code, levels);
				}

				if (!found)
				{
					publishedFilter = ZQuery.NoResultQuery;
				}
			}

			filter.AddToFilter(publishedFilter, condition);
			return filter;
		}

#if DEBUG
		internal static OrgHeader LoggedInOrgHeaderForTest { get; set; }
#endif
	}
}
