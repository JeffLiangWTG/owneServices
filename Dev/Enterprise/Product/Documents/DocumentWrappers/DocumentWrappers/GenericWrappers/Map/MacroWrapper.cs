using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("Useage")]
	public class MacroWrapper : GenericWrapper
	{
		public MacroWrapper(MacroValueProviderMap valueProviderMap, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.valueProviderMap = valueProviderMap ?? new MacroValueProviderMap(ZString.Empty, ZString.Empty);
		}
		readonly MacroValueProviderMap valueProviderMap;

		public ZString TypeName => valueProviderMap.TypeName;

		public ZString Useage => valueProviderMap.Usage;

		public ZString Explanation => valueProviderMap.Description;
	}
}
