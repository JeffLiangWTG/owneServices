using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumHelper
		{
			ZString AdditionalReferenceNumberCategory { get; }
			ICodeDescriptionPairList GetAdditionalReferenceNumberTypes(string countryCode);
		}
	}
}