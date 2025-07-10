using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture
{
	class SchemeCollection : BusinessObjectCollection<GridColourScheme>
	{
		protected override bool FetchOnlyFromLocalCache
		{
			get { return true; }
		}

		internal SchemeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GridColourScheme GetScheme(ZGuid pK)
		{
			return FindByPK(pK) as GridColourScheme;
		}

		public GridColourScheme GetScheme(ZQuery query)
		{
			var all = GetSchemes(query);
			if (all.Length > 0)
			{
				return all[0];
			}
			else
			{
				return null;
			}
		}

		public GridColourScheme[] GetSchemes(ZQuery query)
		{
			var result = new List<GridColourScheme>();
			foreach (var obj in this)
			{
				if (obj.MatchesFilter(query))
				{
					result.Add(obj as GridColourScheme);
				}
			}
			return result.ToArray();
		}
	}
}
