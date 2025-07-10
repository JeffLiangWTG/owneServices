using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class DummyFilterStripsHelper : IFilterStripsHelper
	{
		public string GetAutomaticFilterTestCaseName_ForObjectFactory() => null;

		public Type BusinessObjectType { get; private set; }

		public void Initialise(Type businessObjectType, BusinessObjectFactory factory)
		{
			BusinessObjectType = businessObjectType;
		}

		public void AddFilterStrips(IModuleFilterCollection filters)
		{
			var collection = (ModuleFilterCollection)filters;
			collection.AddTextFilter("Awesomeness Factor", DummyBizoSchema.Z0_NVarChar);
		}

		public bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return BusinessObjectType != null && typeof(IDummyInterface).IsAssignableFrom(BusinessObjectType) || MockIsApplicableToBizOTypeIsAssignableFrom;
		}

		internal bool MockIsApplicableToBizOTypeIsAssignableFrom;

		public bool CanAddFilters()
		{
			return true;
		}

		public void AssertHelperFiltersWork(string methodName, BusinessObject filterStripBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
		}

		public void SetUpForHelperFiltersWorkTests()
		{
		}

		public void AddFilterStripsForIndexSearch(IModuleFilterCollection filters, SearchField[]  defaultHiddenIndexSearchFields)
		{
			var searchField = defaultHiddenIndexSearchFields?.FirstOrDefault(x => x.FieldName == IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter");

			if (searchField == null)
			{
				return;
			}

			var collection = (ModuleFilterCollection)filters;
			var filter = new IndexSearchModuleTextFilter(searchField);
			filter.Category = TestFilter;
			collection.AddFilter(filter);
		}

		internal static FilterCategory TestFilter => new FilterCategory((NoResString)"Test Filter");
	}
}
