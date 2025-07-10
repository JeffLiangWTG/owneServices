using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CountryCollectionProvider : CollectionProviderWithCodeSupport
	{
		public CountryCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefCountryCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefCountry)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefCountryCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefCountry;

		public override int MaxLength => RefCountrySchema.RN_Code.MaxLength;
	}
}
