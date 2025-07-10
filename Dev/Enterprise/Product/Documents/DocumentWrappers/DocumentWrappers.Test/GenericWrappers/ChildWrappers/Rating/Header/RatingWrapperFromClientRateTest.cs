using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingWrapperFromClientRate))]
	sealed class RatingWrapperFromClientRateTest : RatingWrapperTest<ClientRate, RatingWrapperFromClientRate>
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			Env.Registry.Rating.GRIUpdateCoverPageText = "GRIUpdateCoverPageText";
			Env.Registry.Rating.GRIUpdateFooterPageText = "GRIUpdateFooterPageText";

			RatingWrapperFromClientRate wrapper = (RatingWrapperFromClientRate)GetNewDocumentWrapper();
			AssertEquals("wrapper.QuotationTitle", "Rate Update Notification", wrapper.QuotationTitle);
			AssertEquals("wrapper.CoverPageText", "GRIUpdateCoverPageText", wrapper.CoverPageText);
			AssertEquals("wrapper.QuotationAcceptText", ZString.Empty, wrapper.QuotationAcceptText);
			AssertEquals("wrapper.QuotationAcceptTooltip", ZString.Empty, wrapper.QuotationAcceptTooltip);
			AssertEquals("wrapper.CoverPageFooterText", "GRIUpdateFooterPageText", wrapper.CoverPageFooterText);
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
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "BLAT";
			client.OH_FullName = "Blaticus";

			Header.TH_OH = client.PK;

			RatingWrapperFromClientRate wrapper = (RatingWrapperFromClientRate)GetNewDocumentWrapper();

			AssertEquals("Blaticus Client Rate", wrapper.PrimarySource);
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
SecondSignatory :  is null
";
			}
		}

		protected override ClientRate GetNewRatingHeader()
		{
			return Factory.New<ClientRate>();
		}

		protected override RatingWrapper GetNewRatingWrapper()
		{
			return RatingWrapper.New(Header, Factory);
		}

		#endregion

	}
}
