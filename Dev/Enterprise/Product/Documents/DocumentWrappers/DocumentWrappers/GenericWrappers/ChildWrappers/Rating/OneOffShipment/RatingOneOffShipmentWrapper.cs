using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingOneOffShipmentWrapper : GenericWrapper
	{
		public RatingOneOffShipmentWrapper(Quote quote, BusinessObjectFactory factory)
			: base(quote, factory)
		{
			this.quote = quote;
			this.oneoff = quote.CurrentOneOffQuote ?? factory.GetNull<RateOneOffShipment>();
		}

		public ZInt NumberOfEntries
		{
			get { return oneoff.TT_NumberOfEntries; }
		}

		public ZInt NumberOfEntryLines
		{
			get { return oneoff.TT_NumberOfEntryLines; }
		}

		public CodeAndDescriptionWrapper Mode
		{
			get
			{
				return mode ?? (mode = new CodeAndDescriptionWrapper(oneoff.Mode, RatingFreightModeLists.RateModeList(), Factory));
			}
		}
		CodeAndDescriptionWrapper mode;

		public RatingDirectionWrapper Direction
		{
			get
			{
				if (direction == null)
				{
					ILocation local = quote.Company.Country;
					direction = new RatingDirectionWrapper(local.CompletelyCovers(oneoff.ReceivalLocation), local.CompletelyCovers(oneoff.DeliveryLocation), Factory);
				}
				return direction;
			}
		}
		RatingDirectionWrapper direction;

		public RatingFrequencyWrapper Frequency
		{
			get { return frequency ?? (frequency = new RatingFrequencyWrapper(oneoff.TT_Frequency, oneoff.TT_FrequencyUnit, Factory)); }
		}
		RatingFrequencyWrapper frequency;

		public RatingTransitTimeWrapper TransitTime
		{
			get { return transitTime ?? (transitTime = new RatingTransitTimeWrapper(oneoff.TT_TransitTime, Factory)); }
		}
		RatingTransitTimeWrapper transitTime;

		public CommodityWrapper Commodity
		{
			get { return commodity ?? (commodity = new CommodityWrapper(oneoff.Commodity, Factory)); }
		}
		CommodityWrapper commodity;

		public MoneyWrapper InsuranceValue
		{
			get { return insuranceWrapper ?? (insuranceWrapper = new MoneyWrapper(new Money(oneoff.TT_InsureVal, oneoff.InsureValCurr), Factory)); }
		}
		MoneyWrapper insuranceWrapper;

		readonly Quote quote;
		readonly RateOneOffShipment oneoff;
	}
}
