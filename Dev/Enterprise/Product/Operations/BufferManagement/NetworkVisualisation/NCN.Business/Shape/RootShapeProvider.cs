using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class RootShapeProvider
	{
		public static BMNCNShape GetRoot(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<BMNCNShape>(rootKey, () => null, CacheStalenessPolicy.NeverStale);
		}
		public static BMNCNShape SetRoot(BMNCNShape shape)
		{
			shape.Factory.ClearCachedValue<BMNCNShape>(rootKey);
			return shape.Factory.GetCachedValue(rootKey, () => shape, CacheStalenessPolicy.NeverStale);
		}

		const string rootKey = "e5b292e3-d208-4068-a0cd-7058c6d18146";
	}
}
