
namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class MasterDetailRelation
	{
		public MasterDetailRelation(LookupFilterFieldBase detailFilter, string masterFilterFieldName, string relationType)
		{
			this.MasterFilterFieldName = masterFilterFieldName;
			this.DetailFilter = detailFilter;
			this.RelationType = relationType;
		}
		public void SetMasterFilter(LookupFilterFieldBase master)
		{
			MasterFilter = master;
		}

		public void RefreshBinding()
		{
			DetailFilter.CollectionProvider.SetFilterCollection(FilterProvider.Filter);
			DetailFilter.RefreshBinding();
		}

		SQLFilterProvider FilterProvider
		{
			get
			{
				if (fFilterProvider == null)
				{
					fFilterProvider = SQLFilterProviderFactory.CreateSQLFilterProvider(MasterFilter, DetailFilter, RelationType);
				}
				return fFilterProvider;
			}
		}

		public readonly string MasterFilterFieldName;
		public readonly LookupFilterFieldBase DetailFilter;
		LookupFilterFieldBase MasterFilter;
		SQLFilterProvider fFilterProvider;
		readonly string RelationType;
	}
}

