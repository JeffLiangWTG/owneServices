using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class CollectionProviderWithCodeSupport : CollectionProvider
	{
		protected CollectionProviderWithCodeSupport(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		public abstract int MaxLength { get; }
	}
}
