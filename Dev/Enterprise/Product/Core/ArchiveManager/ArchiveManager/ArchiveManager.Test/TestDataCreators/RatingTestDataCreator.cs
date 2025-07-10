using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Rating.Business;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	class RatingTestDataCreator : IRatingTestDataCreator
	{
		public ZGuid CreateAttachedRatingData(string quoteNumber, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();
			var oldDate = ZDate.Today.AddYears(-1);

			var ratingHeader = factory.New<RatingHeader>();
			ratingHeader.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			ratingHeader.TH_QuoteDate = oldDate;
			ratingHeader.TH_QuoteNumber = quoteNumber;
			ratingHeader.TH_OneTimeQuote = true;
			if (isCancelled)
			{
				ratingHeader.IsCancelled = true;
			}

			var rateOneOffShipment = factory.New<RateOneOffShipment>();
			rateOneOffShipment.TT_TH = ratingHeader.PK;

			var rateOneOffContainer = factory.New<RateOneOffContainers>();
			rateOneOffContainer.TC_TT = rateOneOffShipment.PK;

			factory.Save();
			return ratingHeader.PK;
		}
	}
}
