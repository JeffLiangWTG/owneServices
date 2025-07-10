using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Favorites
{
	public class StmLinkCollection : ActiveBusinessObjectCollection<StmLink>, IStmLinkCollection<IStmLink>
	{
		public StmLinkCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public StmLinkCollection(BusinessObjectFactory factory, ZQuery query, string linkType, int maximumNumberOfItems, string sortBy, ListSortDirection sortDirection)
			: this(factory, query)
		{
			this.linkType = linkType;
			this.maximumNumberOfItems = maximumNumberOfItems;
			ApplySort(sortBy, sortDirection);
		}

		readonly string linkType;
		readonly int maximumNumberOfItems;

		public void SetLinkDefaults(StmLink link)
		{
			link.STL_GC_LogonCompany = EnvProxy.Instance.CurrentCompany.PK;
			link.STL_GS_NKUser = EnvProxy.Instance.CurrentUser.Initials;
			link.STL_LastUsedDateTimeUtc = ZDateTime.UtcNow;
			link.STL_LinkType = linkType;
			link.STL_ShortcutIndex = (byte)Count;
		}

		public StmLink AddNew(LinkWrapper shortcut)
		{
			var link = Count >= maximumNumberOfItems && maximumNumberOfItems > 0 ? this.Last() : AddNew();
			link.InitializeLink(shortcut);
			SetLinkDefaults(link);
			return link;
		}

		public int CleanUp()
		{
			var cleanList = new List<StmLink>();
			var deleteList = new List<StmLink>();

			for (var i = 0; i < Count; i++)
			{
				var item = this[i];

				if (i > (maximumNumberOfItems - 1) || cleanList.Any(t => t.Equals(item)))
				{
					deleteList.Add(item);
				}
				else
				{
					cleanList.Add(item);
				}
			}

			deleteList.ForEach(t => Delete(t));
			return deleteList.Count;
		}
	}
}
