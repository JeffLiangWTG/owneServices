using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingWrapperFromJobHeader))]
	sealed class RatingWrapperFromJobHeaderTest : RatingWrapperTest<JobHeader, RatingWrapperFromJobHeader>
	{
		#region TestValidFromAndValidUntil

		public void TestValidFromAndValidUntil()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var openDate = new ZDateTime(2013, 10, 4);
			var closeDate = new ZDateTime(2013, 11, 4);
			jobHeader.JH_A_JOP = openDate;
			jobHeader.JH_A_JCL = closeDate;

			var wrapper = new RatingWrapperFromJobHeader(jobHeader, Factory);
			AssertEquals(openDate, wrapper.ValidFrom);
			AssertEquals(closeDate, wrapper.ValidUntil);

			jobHeader.JH_A_JOP = openDate;
			jobHeader.JH_A_JCL = ZDateTime.Empty;
			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 2);
			wrapper = new RatingWrapperFromJobHeader(jobHeader, Factory);
			AssertEquals(openDate, wrapper.ValidFrom);
			AssertEquals(new ZDateTime(2013, 12, 4), wrapper.ValidUntil);

			jobHeader.JH_A_JOP = openDate;
			jobHeader.JH_A_JCL = ZDateTime.Empty;
			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 0);
			wrapper = new RatingWrapperFromJobHeader(jobHeader, Factory);
			AssertEquals(openDate, wrapper.ValidFrom);
			AssertEquals(ZDateTime.Empty, wrapper.ValidUntil);

			jobHeader.JH_A_JOP = openDate;
			jobHeader.JH_A_JCL = ZDateTime.Empty;
			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, -1);
			wrapper = new RatingWrapperFromJobHeader(jobHeader, Factory);
			AssertEquals(openDate, wrapper.ValidFrom);
			AssertEquals(new ZDateTime(2013, 11, 30), wrapper.ValidUntil);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (RatingWrapperFromJobHeader)GetNewDocumentWrapper();

			AssertEquals("wrapper.PrimarySource", ZString.Empty, wrapper.PrimarySource);
			AssertEquals("wrapper.QuotationTitle", ZString.Empty, wrapper.QuotationTitle);
			AssertEquals("wrapper.CoverPageText", ZString.Empty, wrapper.CoverPageText);
			AssertEquals("wrapper.QuotationAcceptText", ZString.Empty, wrapper.QuotationAcceptText);
			AssertEquals("wrapper.QuotationAcceptTooltip", ZString.Empty, wrapper.QuotationAcceptTooltip);
			AssertEquals("wrapper.CoverPageFooterText", ZString.Empty, wrapper.CoverPageFooterText);
			AssertEquals("wrapper.IsReprint", false, wrapper.IsReprint);
			AssertNull("wrapper.SecondSignatory", wrapper.SecondSignatory);
			AssertNull("wrapper.OneOffShipment", wrapper.OneOffShipment);
			AssertNull("wrapper.TrailingPages", wrapper.TrailingPages);
			AssertNull("wrapper.PublishedAirFreightAgents", wrapper.PublishedAirFreightAgents);
			AssertNull("wrapper.PublishedSeaFreightAgents", wrapper.PublishedSeaFreightAgents);
		}

		#endregion

		#region ExpectedDefaultFormatting

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

		#endregion

		#region Implementaion

		// These Test are not applicable for JobHeader as it is based on RatingHeader types
		protected override void AssertInvoiceTermsText(string expectedText, ZArchitecture.Core.CodeDescriptionPair term)
		{
			Assert(true);
		}

		// These Test are not applicable for JobHeader as it is based on RatingHeader types
		protected override void AssertMultipleInvoiceTermsText(string expectedText)
		{
			Assert(true);
		}

		protected override JobHeader GetNewRatingHeader()
		{
			return Factory.NewJobForTesting<JobHeader>();
		}

		protected override RatingWrapper GetNewRatingWrapper()
		{
			return RatingWrapper.New(Header, Factory);
		}

		#endregion
	}
}
