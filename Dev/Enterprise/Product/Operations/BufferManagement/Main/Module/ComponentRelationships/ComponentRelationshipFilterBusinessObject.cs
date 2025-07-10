using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ComponentRelationshipFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var nameTextFilter = filters.AddTextFilter(FilterDescriptions.Name, BMComponentSchema.FC_Name);
			nameTextFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ComponentRelationshipFilter|Name", "Name");

			filters.AddFilter(new ComponentsFilter(FilterDescriptions.Components, new BMComponentCollection(Factory)));

			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.ComponentRelationship);
				return query;
			}
		}

		public static class FilterDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Name = "Name";
			public const string Components = "Components";

			#endregion
		}
	}
}
