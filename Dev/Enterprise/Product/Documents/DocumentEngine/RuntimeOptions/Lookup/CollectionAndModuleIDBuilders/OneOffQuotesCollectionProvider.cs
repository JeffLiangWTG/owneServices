using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class OneOffQuotesCollectionProvider : CollectionProvider
	{
		public OneOffQuotesCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)ObjectFactory.Get<IViewOneOffQuoteCollection>("IViewOneOffQuoteCollection", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.OneOffQuotes;
	}
}
