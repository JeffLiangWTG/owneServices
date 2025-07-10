using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Quotation.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCostsComparerRateLine))]
	sealed class DocCostsComparerRateLineTest : DocRateLineItemTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new[] { CreateDocumentWrapperFromStaticNewMethod() };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			const QuotationLineType Type = QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription;
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return DocCostsComparerRateLine.New(QuotationLine.Header(rateLine, Type), DocCostsComparerEntry.New(new CostsComparerEntry(new CostsComparer(), rateEntry, new List<RateLine>(new RateLine[] { rateLine })), Factory), Factory);
		}
	}
}
