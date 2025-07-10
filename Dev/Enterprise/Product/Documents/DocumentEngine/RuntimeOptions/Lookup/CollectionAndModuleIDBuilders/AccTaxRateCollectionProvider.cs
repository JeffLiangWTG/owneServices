using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccTaxRateCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AccTaxRateCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccTaxRateCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccTaxRate;

		public override int MaxLength => AccTaxRateSchema.AT_Code.MaxLength;
	}
}
