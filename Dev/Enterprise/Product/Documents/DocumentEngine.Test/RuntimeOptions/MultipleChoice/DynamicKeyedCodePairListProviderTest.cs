using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	public abstract class DynamicKeyedCodePairListProviderTest<K, L> : StaticKeyedCodePairListProviderTest<K, L> where L : DynamicKeyedCodePairListProvider<K>
	{
		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected override void SetUp()
		{
			base.SetUp();
			SetupDataForFirstKey(Factory);
			SetupDataForSecondKey(Factory);
			Factory.Save();
		}

		protected abstract void SetupDataForFirstKey(BusinessObjectFactory factory);
		protected abstract void SetupDataForSecondKey(BusinessObjectFactory factory);
	}
}
