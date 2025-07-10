using CargoWise.Types;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingWrapperFromTariff))]
	sealed class RatingWrapperFromTariffTest : RatingWrapperTest<CompanyTariff, RatingWrapperFromTariff>
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			RatingWrapperFromTariff wrapper = (RatingWrapperFromTariff)GetNewDocumentWrapper();
			AssertEquals("wrapper.QuotationTitle", "", wrapper.QuotationTitle);
			AssertEquals("wrapper.CoverPageText", "", wrapper.CoverPageText);
			AssertEquals("wrapper.QuotationAcceptText", ZString.Empty, wrapper.QuotationAcceptText);
			AssertEquals("wrapper.QuotationAcceptTooltip", ZString.Empty, wrapper.QuotationAcceptTooltip);
			AssertEquals("wrapper.CoverPageFooterText", "", wrapper.CoverPageFooterText);
			AssertEquals("wrapper.IsReprint", false, wrapper.IsReprint);
			AssertEquals("wrapper.ValidFrom", ZDateTime.Empty, wrapper.ValidFrom);
			AssertEquals("wrapper.ValidUntil", ZDateTime.Empty, wrapper.ValidUntil);
			AssertEquals("wrapper.OneOffShipment", null, wrapper.OneOffShipment);
			AssertEquals("wrapper.TrailingPages.Count", 0, wrapper.TrailingPages.Count);
			AssertEquals("wrapper.PublishedAirFreightAgents.Count", 0, wrapper.PublishedAirFreightAgents.Count);
			AssertEquals("wrapper.PublishedSeaFreightAgents.Count", 0, wrapper.PublishedSeaFreightAgents.Count);
		}

		#endregion

		#region TestPrimarySource

		public void TestPrimarySource()
		{
			Header.TH_GlobalRateLevel = 4;
			Header.TH_GlobalRateDescription = "Blaticus";

			RatingWrapperFromTariff wrapper = (RatingWrapperFromTariff)GetNewDocumentWrapper();

			AssertEquals("Level 4 Company Tariff - Blaticus", wrapper.PrimarySource);
		}

		#endregion

		#region Implementation

		protected override CompanyTariff GetNewRatingHeader()
		{
			return Factory.New<CompanyTariff>();
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
OneOffShipment :  is null
Registry : (No Default Field Value Available on Registry)
SecondSignatory :  is null
";
			}
		}

		protected override RatingWrapper GetNewRatingWrapper()
		{
			return RatingWrapper.New(Header, Factory);
		}

		#endregion

	}
}
