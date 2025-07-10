using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class DynamicKeyedCodePairListProvider<K> : StaticKeyedCodePairListProvider<K>
	{
		public DynamicKeyedCodePairListProvider(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		readonly BusinessObjectFactory Factory;

		protected override CodeDescriptionPairList GetCombinedList(K key)
		{
			CodeDescriptionPairList list = base.GetCombinedList(key);
			list.AddRangeOverwriteIfExists(GetDynamicCodeDescriptionPairListCore(key, Factory));
			return list;
		}
		protected abstract CodeDescriptionPairList GetDynamicCodeDescriptionPairListCore(K key, BusinessObjectFactory factory);
	}
}
