using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class RefCurrencyCollectionProvider : CollectionProviderWithCodeSupport
	{
		public RefCurrencyCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefCurrencyCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefCurrency)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefCurrencyCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefCurrency;

		public override int MaxLength => RefCurrencySchema.RX_Code.MaxLength;
	}
}
