using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingWrapperFromQuotation))]
	sealed class RatingWrapperQuotationTest : RatingWrapperTest<Quote, RatingWrapperFromQuotation>
	{
		#region TestCoverPageText

		public void TestCoverPageText()
		{
			Env.Registry.Rating.QuoteCoverPageTextNew = "Cover";
			Env.Registry.Rating.QuoteCoverPageFooterText = "Footer";

			Quote quote = Factory.New<Quote>();
			RatingWrapper wrapper = new RatingWrapperFromQuotation(quote, Factory);
			AssertEquals("Cover", wrapper.CoverPageText);
			AssertEquals("Footer", wrapper.CoverPageFooterText);

			quote.Notes.AddNew(false, PredefinedNoteTypes.Instance.QuoteCoverPageText.Description, "Super Mega Delux!!!");
			AssertEquals("Super Mega Delux!!!", wrapper.CoverPageText);
			AssertEquals("Footer", wrapper.CoverPageFooterText);
		}

		#endregion

		#region TestTrailingPages

		public void TestTrailingPages()
		{
			Quote quote = Factory.New<Quote>();
			quote.TrailingPageImages.Clear();

			using (Image image1 = new Bitmap(20, 20))
			using (Image image2 = new Bitmap(20, 20))
			using (Image image3 = new Bitmap(20, 20))
			{
				quote.TrailingPageImages.Add(image1);
				quote.TrailingPageImages.Add(image2);
				quote.TrailingPageImages.Add(image3);

				RatingWrapper wrapper = new RatingWrapperFromQuotation(quote, Factory);

				AssertEquals(3, wrapper.TrailingPages.Count);
				AssertSame(image1, wrapper.TrailingPages[0].Image);
				AssertSame(image2, wrapper.TrailingPages[1].Image);
				AssertSame(image3, wrapper.TrailingPages[2].Image);
			}
		}

		#endregion

		#region TestPublishedFreightAgents

		[SetOrgAllowMixedCase(true)]
		public void TestPublishedFreightAgents()
		{
			TestCaseHelper.ClearTable(OrgAppointedAgentPortsSchema.Constants.TableName);

			OrgHeader sydAirAgent = Factory.NewWithValidTestData<OrgHeader>();
			sydAirAgent.OH_FullName = "SYD Air Agent";
			sydAirAgent.OH_RL_NKClosestPort = "AUSYD";
			sydAirAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts sydAirApp = sydAirAgent.AppointedAgentPorts.AddNew();
			sydAirApp.O5_PortOrCountry = "AUSYD";
			sydAirApp.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			sydAirApp.O5_OA_AgentOfficeAddress = sydAirAgent.MainAddress.PK;

			OrgHeader amsAirAgent = Factory.NewWithValidTestData<OrgHeader>();
			amsAirAgent.OH_FullName = "AMS Air Agent";
			amsAirAgent.OH_RL_NKClosestPort = "NLAMS";
			amsAirAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts amsAirApp = amsAirAgent.AppointedAgentPorts.AddNew();
			amsAirApp.O5_PortOrCountry = "NLAMS";
			amsAirApp.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			amsAirApp.O5_OA_AgentOfficeAddress = amsAirAgent.MainAddress.PK;

			OrgHeader sydSeaAgent = Factory.NewWithValidTestData<OrgHeader>();
			sydSeaAgent.OH_FullName = "SYD Sea Agent";
			sydSeaAgent.OH_RL_NKClosestPort = "AUSYD";
			sydSeaAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts sydSeaApp = sydSeaAgent.AppointedAgentPorts.AddNew();
			sydSeaApp.O5_PortOrCountry = "AUSYD";
			sydSeaApp.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			sydSeaApp.O5_OA_AgentOfficeAddress = sydSeaAgent.MainAddress.PK;

			OrgHeader amsSeaAgent = Factory.NewWithValidTestData<OrgHeader>();
			amsSeaAgent.OH_FullName = "AMS Sea Agent";
			amsSeaAgent.OH_RL_NKClosestPort = "NLAMS";
			amsSeaAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts amsSeaApp = amsSeaAgent.AppointedAgentPorts.AddNew();
			amsSeaApp.O5_PortOrCountry = "NLAMS";
			amsSeaApp.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			amsSeaApp.O5_OA_AgentOfficeAddress = amsSeaAgent.MainAddress.PK;

			Quote quote = Factory.New<Quote>();
			quote.AddRateEntry(RatingConstants.RateCategory.AIR, "ULD", "AUSYD", "NLAMS");
			quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NLAMS");

			Factory.Save();

			RatingWrapperFromQuotation wrapper = new RatingWrapperFromQuotation(quote, Factory);

			AssertContainsExactElementsInAnyOrder(
				new string[] { "SYD Air Agent", "AMS Air Agent" },
				Array.ConvertAll(wrapper.PublishedAirFreightAgents.ToArray<OrganisationWrapper>(), (w) => w.CompanyName.ToString())
			);

			AssertContainsExactElementsInAnyOrder(
				new string[] { "SYD Sea Agent", "AMS Sea Agent" },
				Array.ConvertAll(wrapper.PublishedSeaFreightAgents.ToArray<OrganisationWrapper>(), (w) => w.CompanyName.ToString())
			);
		}

		#endregion

		#region TestReprint

		public void TestReprint()
		{
			Quote quote = Factory.New<Quote>();

			quote.DocumentPrintMode = QuotationDocumentMode.Final;
			AssertEquals("Final", false, new RatingWrapperFromQuotation(quote, Factory).IsReprint);

			quote.DocumentPrintMode = QuotationDocumentMode.Draft;
			AssertEquals("Draft", false, new RatingWrapperFromQuotation(quote, Factory).IsReprint);

			quote.DocumentPrintMode = QuotationDocumentMode.Reprint;
			AssertEquals("Reprint", true, new RatingWrapperFromQuotation(quote, Factory).IsReprint);

			quote.DocumentPrintMode = QuotationDocumentMode.Unknown;
			AssertEquals("Unknown", false, new RatingWrapperFromQuotation(quote, Factory).IsReprint);
		}

		#endregion

		#region TestPrimarySource

		public void TestPrimarySource()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "BLAT";
			client.OH_FullName = "Blaticus";

			Header.TH_OH = client.PK;

			RatingWrapperFromQuotation wrapper = (RatingWrapperFromQuotation)GetNewDocumentWrapper();

			AssertEquals("Quotation for Blaticus", wrapper.PrimarySource);
		}

		#endregion

		#region TestOneOffShipment

		public void TestOneOffShipment()
		{
			Quote quote = Factory.New<Quote>();
			Quote oneoff = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory).Quote;

			Factory.Save();

			RatingWrapperFromQuotation quoteWrapper = new RatingWrapperFromQuotation(quote, Factory);
			RatingWrapperFromQuotation oneoffWrapper = new RatingWrapperFromQuotation(oneoff, Factory);

			AssertNull("quoteWrapper", quoteWrapper.OneOffShipment);
			AssertNotNull("oneoffWrapper", oneoffWrapper.OneOffShipment);
		}

		#endregion

		#region TestValidity

		public void TestValidity()
		{
			var now = ZDate.Today;

			Header.TH_QuoteDate = now.AddDays(-1);
			Header.TH_QuoteEndDate = now.AddDays(40);

			RatingWrapperFromQuotation wrapper = (RatingWrapperFromQuotation)GetNewDocumentWrapper();
			AssertEquals("ValidFrom", now.AddDays(-1), wrapper.ValidFrom);
			AssertEquals("ValidUntil", now.AddDays(40), wrapper.ValidUntil);
		}

		#endregion

		#region TestQuotationAcceptText

		public void TestQuotationAcceptText()
		{
			var oneOff = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory).Quote;
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotation = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory).Quote;
			var quotedBooking = QuotedBooking.New(quotation.PK, shipment.PK, Factory).Quote;

			Factory.Save();

			AssertEquals("One Off Quote Accept Text", "Accept One Off Quote", new RatingWrapperFromQuotation(oneOff, Factory).QuotationAcceptText);
			AssertEquals("Quotation Accept Text", "Accept Quotation", new RatingWrapperFromQuotation(quotedBooking, Factory).QuotationAcceptText);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			Header.TH_QuoteDate = ZDate.Empty;
			Header.TH_QuoteEndDate = ZDate.Empty;

			RatingWrapperFromQuotation wrapper = (RatingWrapperFromQuotation)GetNewDocumentWrapper();
			AssertEquals("wrapper.QuotationTitle", "Quotation", wrapper.QuotationTitle);
			AssertEquals("wrapper.CoverPageText", "", wrapper.CoverPageText);
			AssertEquals("wrapper.SecondSignatory", null, wrapper.SecondSignatory);
			AssertEquals("wrapper.QuotationAcceptText", "Accept Quotation", wrapper.QuotationAcceptText);
			AssertEquals("wrapper.QuotationAcceptTooltip", "Accept Quotation", wrapper.QuotationAcceptTooltip);
			AssertEquals("wrapper.CoverPageFooterText", "This quotation is subject to our Standard Terms and Conditions which are available on request.", wrapper.CoverPageFooterText);
			AssertEquals("wrapper.IsReprint", false, wrapper.IsReprint);
			AssertEquals("wrapper.ValidFrom", ZDateTime.Empty, wrapper.ValidFrom);
			AssertEquals("wrapper.ValidUntil", ZDateTime.Empty, wrapper.ValidUntil);
			AssertEquals("wrapper.OneOffShipment", null, wrapper.OneOffShipment);
			AssertEquals("wrapper.TrailingPages.Count", 0, wrapper.TrailingPages.Count);
			AssertEquals("wrapper.PublishedAirFreightAgents.Count", 0, wrapper.PublishedAirFreightAgents.Count);
			AssertEquals("wrapper.PublishedSeaFreightAgents.Count", 0, wrapper.PublishedSeaFreightAgents.Count);
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
OneOffShipment :  is null
Registry : (No Default Field Value Available on Registry)
SecondSignatory : Fread
";
			}
		}

		protected override Quote GetNewRatingHeader()
		{
			return Factory.New<Quote>();
		}

		protected override RatingWrapper GetNewRatingWrapper()
		{
			return RatingWrapper.New(Header, Factory);
		}

		#endregion

	}
}
