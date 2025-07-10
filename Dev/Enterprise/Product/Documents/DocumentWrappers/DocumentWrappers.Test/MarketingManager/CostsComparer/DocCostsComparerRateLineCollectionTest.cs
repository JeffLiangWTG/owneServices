using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCostsComparerRateLineCollection))]
	sealed class DocCostsComparerRateLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCostsComparerRateLineCollection>
	{
		protected override DocCostsComparerRateLineCollection GetCollectionToTest()
		{
			return new DocCostsComparerRateLineCollection(new CostsComparer());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			const QuotationLineType Type = QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription;
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return DocCostsComparerRateLine.New(QuotationLine.Header(rateLine, Type), DocCostsComparerEntry.New(new CostsComparerEntry(new CostsComparer(), rateEntry, new List<RateLine>(new RateLine[] { rateLine })), Factory), Factory);
		}
	}
}
