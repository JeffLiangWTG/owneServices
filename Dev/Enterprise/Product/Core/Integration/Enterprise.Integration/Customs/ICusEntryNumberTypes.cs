using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumberTypes
		{
			ICodeDescriptionPairList CountrySpecificCustomsEntryNumberTypeList(BusinessObjectFactory factory, ZString countryCode, bool isImport);
		}
	}
}
