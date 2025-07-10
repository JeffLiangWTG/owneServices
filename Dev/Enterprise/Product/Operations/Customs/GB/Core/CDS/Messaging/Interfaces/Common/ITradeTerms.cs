using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ITradeTerms
	{
		ZString ConditionCode { get; }
		ZString LocationID { get; }
		ZString LocationName { get; }
	}

	class TradeTermsWrapper : ITradeTerms
	{
		TradeTermsWrapper(ZString conditionCode, ZString locationId, ZString locationName)
		{
			this.conditionCode = conditionCode;
			this.locationId = locationId;
			this.locationName = locationName;
		}

		public static TradeTermsWrapper New(ZString conditionCode, ZString locationId, ZString locationName)
		{
			return new TradeTermsWrapper(conditionCode, locationId, locationName);
		}

		ZString ITradeTerms.ConditionCode => conditionCode;

		ZString ITradeTerms.LocationID => locationId;

		ZString ITradeTerms.LocationName => locationName;

		readonly ZString conditionCode;
		readonly ZString locationId;
		readonly ZString locationName;
	}
}
