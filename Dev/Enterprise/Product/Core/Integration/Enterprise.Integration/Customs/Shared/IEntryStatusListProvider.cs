using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IEntryStatusListProvider
			{
				ICodeDescriptionPairList EntryStatusList(BusinessObjectFactory factory, ZString countryCode, ZString messageType);

				ICodeDescriptionPairList EntryStatusListForShipments(BusinessObjectFactory factory, ZString countryCode);
			}
		}
	}
}
