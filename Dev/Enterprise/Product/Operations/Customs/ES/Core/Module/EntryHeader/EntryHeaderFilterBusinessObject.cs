using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Module
{
	public class EntryHeaderFilterBusinessObject : EU.Module.EntryHeaderFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddParallelFilter(filters);

			return filters;
		}

		#region Parallel

		void AddParallelFilter(ModuleFilterCollection filters)
		{
			var addInfoFilter = new AddInfoModuleBooleanFilter(DeclarationFilterConstants.Parallel, GetParallelQuery, Lookups.ParallelList);
			addInfoFilter.Category = FilterCategories.StatusAndFlags;
			addInfoFilter.MultilingualDescription = ResString.GetMultilingualString("ECCEC6A1-6D6A-4C56-93E9-0488C5644B27", DeclarationFilterConstants.Parallel);

			filters.AddFilter(addInfoFilter);
		}

		ZQuery GetParallelQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(Business.Declaration.CusEntryHeader));

			comparisonOperator = SQLComparisonOperator.Contains;
			var parallelColumn = EUAddInfoSchema.ZG_Parallel.Name.Substring(3);
			var queryValue = parallelColumn + "=" + value;

			if (value == YesNoList.Codes.No)
			{
				comparisonOperator = SQLComparisonOperator.NotContains;
				queryValue = parallelColumn + "=";
			}

			entryHeaderQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, comparisonOperator, queryValue);
			return entryHeaderQuery;
		}

		#endregion

		public new EntryHeaderFilterLookups Lookups => (EntryHeaderFilterLookups)base.Lookups;

		protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);
	}
}
