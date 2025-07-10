using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ISanctionData
		{
			bool IsConditionActive { get; }
			ZString ConditionType { get; }
			ZString ConditionDescription { get; }
		}

		public interface ISanctionDataProvider
		{
			ISanctionData ConditionState(ZString issuingCountry, ZString destinationCountry, ZDateTime effectiveDate);
		}
	}
}
