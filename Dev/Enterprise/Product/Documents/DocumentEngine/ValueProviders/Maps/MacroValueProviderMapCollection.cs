using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public class MacroValueProviderMapCollection : NonPersistentBusinessObjectCollection<MacroValueProviderMap>
	{
		public MacroValueProviderMapCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MacroValueProviderMap(ZString.Empty, ZString.Empty, ZString.Empty);
		}
	}
}
