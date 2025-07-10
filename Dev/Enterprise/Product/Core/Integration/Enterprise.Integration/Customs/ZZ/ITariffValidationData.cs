using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ITariffValidationData
		{
			bool IsValid { get; }
			ZString Description { get; }
			ZString DataGroupingName { get; }
			ZDateTime EndDate { get; }
			ZString NearestNomenclature { get; }
		}

		public interface ITariffValidationDataProvider
		{
			ITariffValidationData Validate(ZString country, ZString tariffOrNomenclature, bool findNearest);
		}
	}
}
