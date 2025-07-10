using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumLookups
		{
			ICodeDescriptionPairList AdditionalReferenceNumberTypes { get; }
			IBusinessObjectCollection Countries { get; }
			ICusEntryNumber Parent { get; }
			ICodeDescriptionPairList CountriesAsCodeDescriptionList { get; }
		}
	}
}
