using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocIndexEntry))]
	sealed class DocIndexEntryTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			RateEntry entry = Factory.New<Quote>().AddRateEntry("AIR", "LSE", "", "");

			PricingPage standardPage = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			PricingPage landscapePage = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			return new DocumentWrapper[]
			{
				DocIndexEntry.New(standardPage, Factory),
				DocIndexEntry.New(landscapePage, Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			RateEntry entry = Factory.New<Quote>().AddRateEntry("AIR", "LSE", "", "");
			PricingPage standardPage = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			return DocIndexEntry.New(standardPage, Factory);
		}

		#endregion
	}
}
