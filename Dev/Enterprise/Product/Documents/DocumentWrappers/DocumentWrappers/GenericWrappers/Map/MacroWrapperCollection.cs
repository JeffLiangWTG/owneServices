using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class MacroWrapperCollection : GenericWrapperCollection<MacroWrapper>
	{
		public MacroWrapperCollection(MacroValueProviderMapCollection valueProviderMaps, BusinessObjectFactory factory)
			: base(factory)
		{
			if (valueProviderMaps != null)
			{
				foreach (MacroValueProviderMap valueProviderMap in valueProviderMaps)
				{
					Add(new MacroWrapper(valueProviderMap, Factory));
				}
				this.Sort("Useage");
			}
		}
	}
}
