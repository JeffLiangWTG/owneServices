using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingOneOffShipmentWrapper))]
	sealed class RatingOneOffShipmentWrapperTest : GenericWrapperTest
	{
		public void TestNumberOfEntriesAndLines()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_NumberOfEntries = 4;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 15;

			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals(4, wrapper.NumberOfEntries);
			AssertEquals(15, wrapper.NumberOfEntryLines);
		}

		public void TestDirection()
		{
			ZString localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			ZQuery filter = new ZQuery();
			filter.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, localPort.Left(2));

			ZString osPort = Factory.LoadTop1<RefUNLOCO>(filter).RL_Code;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = osPort;
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = localPort;

			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals("IMP", wrapper.Direction.Code);

			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = localPort;
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = osPort;

			wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals("EXP", wrapper.Direction.Code);
		}

		public void TestFrequency()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_Frequency = 5;
			quote.CurrentOneOffQuote.TT_FrequencyUnit = FrequencyList.Codes.Days;

			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals("Every 5 Days", wrapper.Frequency.ToString());
		}

		public void TestTransitTime()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_TransitTime = "3";

			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals("3 Days", wrapper.TransitTime.ToString());
		}

		public void TestCommodity()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "MTHZ";

			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);
			AssertEquals("MTHZ", wrapper.Commodity.Code);
		}

		public override void TestWrapperMappingsEmpty()
		{
			Quote quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory).Quote;
			RatingOneOffShipmentWrapper wrapper = new RatingOneOffShipmentWrapper(quote, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.NumberOfEntries", 1, wrapper.NumberOfEntries);
				AssertEquals("wrapper.NumberOfEntryLines", 1, wrapper.NumberOfEntryLines);
				AssertEquals("wrapper.Mode", "", wrapper.Mode.Code);
				AssertEquals("wrapper.Direction.Code", "CXT", wrapper.Direction.Code);
				AssertEquals("wrapper.Frequency", "", wrapper.Frequency.ToString());
				AssertEquals("wrapper.TransitionTime", "", wrapper.TransitTime.ToString());
				AssertEquals("wrapper.Commodity", " ()", wrapper.Commodity.ToString());
				AssertEquals("wrapper.InsuranceValue", "", wrapper.InsuranceValue.ToString());
			});
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : GEN (General)
Direction : Cross Trade
Frequency : Every 5 Days
InsuranceValue : 500.00 USD
Mode : 
Registry : (No Default Field Value Available on Registry)
TransitTime : 30 Days
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Quote quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory).Quote;
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "GEN";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "NZAKL";
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "NLAMS";
			quote.CurrentOneOffQuote.TT_RL_NKViaLocation = "SGSIN";
			quote.CurrentOneOffQuote.TT_IncoTerm = "FOB";
			quote.CurrentOneOffQuote.TT_InsureVal = 500;
			quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr = "USD";
			quote.CurrentOneOffQuote.TT_Frequency = 5;
			quote.CurrentOneOffQuote.TT_FrequencyUnit = FrequencyList.Codes.Days;
			quote.CurrentOneOffQuote.TT_TransitTime = "30";

			return new RatingOneOffShipmentWrapper(quote, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RatingOneOffShipment
======================================================================
Name                                    Type
----------------------------------------------------------------------
Mode                                    CodeAndDescription
Commodity                               Commodity
InsuranceValue                          Money
Direction                               Rating Direction
Frequency                               Rating Frequency
TransitTime                             Rating Transit Time
NumberOfEntries                         Int
NumberOfEntryLines                      Int
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			QuotedBooking booking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			return new RatingOneOffShipmentWrapper(booking.Quote, Factory);
		}

		#endregion
	}
}
