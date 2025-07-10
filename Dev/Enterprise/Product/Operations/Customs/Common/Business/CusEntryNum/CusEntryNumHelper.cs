using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumHelper : Integration.Customs.ICusEntryNumHelper
	{
		public ZString AdditionalReferenceNumberCategory
		{
			get { return CusEntryNumber.Categories.AdditionalReferenceNumber; }
		}

		public ICodeDescriptionPairList GetAdditionalReferenceNumberTypes(string countryCode)
		{
			return CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode);
		}
	}
}
