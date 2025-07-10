using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocTableQuotation))]
	sealed class DocTableQuotationWrapperTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocTableQuotation.New(GetFormatTable(), Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocTableQuotation.New(GetFormatTable(), Factory);
		}

		PricingPage GetFormatTable()
		{
			RateEntry entry = Factory.New<Quote>().AddRateEntry("AIR", "LSE", "", "");
			PricingPage formatTable = new PricingPage(entry, Factory, PricingPageStyle.Landscape);
			return formatTable;
		}

		#endregion
	}
}
