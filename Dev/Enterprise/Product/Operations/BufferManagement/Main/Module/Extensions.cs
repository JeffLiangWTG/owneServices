using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public static class Extensions
	{
		public static ZBoolDescriptionPairList ConvertToBoolDescriptionPairList(this IEnumerable<string> descriptions)
		{
			var boolList = new ZBoolDescriptionPairList();

			foreach (var description in descriptions)
			{
				boolList.AddNew(description, false);
			}

			return boolList;
		}

		public static ZBoolDescriptionPairList SynchroniseInto(this IEnumerable<string> list, ZBoolDescriptionPairList oldList)
		{
			var freshBoolList = list.ConvertToBoolDescriptionPairList();
			var selectedItems = new HashSet<ZString>(oldList.Where(p => p.Value).Select(p => p.Description));

			foreach (var codeDescriptionPair in freshBoolList)
			{
				codeDescriptionPair.Value = selectedItems.Contains(codeDescriptionPair.Description);
			}

			return freshBoolList;
		}

		public static ModuleGuidAppliedToSubCollectionFilter AddAppliedToSubCollectionFilter(this ModuleFilterCollection filterCollection, ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotIn queryDelegate, IBusinessObjectCollection collection)
		{
			var filter = new ModuleGuidAppliedToSubCollectionFilter(description, category, moduleID, queryDelegate, collection);
			filterCollection.AddFilter(filter);
			return filter;
		}

		public static ModuleGuidAppliedToSubCollectionFilter AddAppliedToSubCollectionFilterWithInherited(this ModuleFilterCollection filterCollection, ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInAndInherited queryDelegate, bool shouldReevaluateQuery, IBusinessObjectCollection collection, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
		{
			var filter = new ModuleGuidAppliedToSubCollectionFilter(description, category, moduleID, queryDelegate, shouldReevaluateQuery, collection, parameters, factory);
			filterCollection.AddFilter(filter);
			return filter;
		}

		public static TagWithJobOrWorkflowFilter AddToTagWithJobOrWorkflowFilter(this ModuleFilterCollection filterCollection, ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInandDropdownOption queryDelegate, IBusinessObjectCollection collection)
		{
			var filter = new TagWithJobOrWorkflowFilter(description, category, moduleID, queryDelegate, collection);
			filterCollection.AddFilter(filter);
			return filter;
		}
	}
}
