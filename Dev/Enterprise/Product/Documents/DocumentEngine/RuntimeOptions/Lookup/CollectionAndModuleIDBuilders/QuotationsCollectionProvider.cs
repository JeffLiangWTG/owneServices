using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class QuotationsCollectionProvider : CollectionProvider
	{
		public QuotationsCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IQuoteCollection>("IQuoteCollection", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Quotations;
	}
}
