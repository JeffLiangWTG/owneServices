using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocEntryQuotation))]
	sealed class DocEntryQuotationWrapperTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			Quote quote = Factory.New<Quote>();

			return new DocumentWrapper[]
			{
				DocEntryQuotation.New(new PricingPage(quote.AddRateEntry("AIR", "LSE", "", ""),Factory, PricingPageStyle.Standard), Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			Quote quote = Factory.New<Quote>();
			return DocEntryQuotation.New(new PricingPage(quote.AddRateEntry("AIR", "LSE", "", ""), Factory, PricingPageStyle.Standard), Factory);
		}

		#endregion
	}
}
