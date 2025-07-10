using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeCollectionProviderAllHaveTestCase]
	public class DummyCollectionProvider : CollectionProvider
	{
		public DummyCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override ModuleIdentifier ModuleID => DummyModuleIDs.Dummy;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn(BusinessObjectFactory);
		}
	}

	class DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn : BusinessObjectCollection<DummyBusinessObjectCodePropertyIsNotSchemaColumn>
	{
		IBusinessObjectCollectionFetchStrategy lastStrategy;

		public ZQuery LastLoadedAdditionalFilter_Exposed => base.LastLoadedAdditionalFilter;

		public IFindBoxListProvider FindBoxListProviderOverride { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider => FindBoxListProviderOverride ?? base.FindBoxListProvider;

		public DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyBusinessObjectCollectionCodePropertyIsNotSchemaColumn(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			if (lastStrategy == null)
			{
				lastStrategy = new BusinessObjectCollectionFetchStrategyForTest(this);
			}

			return lastStrategy;
		}
	}
}
