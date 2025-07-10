using System;

namespace Enterprise.Integration.Rating
{
	public interface IRating
	{
		Type RatingHeaderType { get; }
		Type CompanyTariffRatingHeaderType { get; }
		Type QuoteType { get; }
		Type RateEntryType { get; }
		Type QuotationProcessTaskType { get; }
		Type RateLineType { get; }
		Type RateLineItemType { get; }
	}
}
