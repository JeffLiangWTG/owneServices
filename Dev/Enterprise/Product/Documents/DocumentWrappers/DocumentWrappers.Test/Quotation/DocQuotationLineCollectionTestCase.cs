using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocQuotationLineCollection))]
	sealed class DocQuotationLineCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<DocQuotationLineCollection>
	{
		#region Implementation

		protected override DocQuotationLineCollection GetCollectionToTest()
		{
			return new DocQuotationLineCollection(null, null, new RateEntry[] { Factory.New<RateEntry>() }, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var rateLine = rateEntry.RateLines.AddNew();
			return DocRateLineItem.New(QuotationLine.Spacing(rateLine), Factory);
		}

		#endregion
	}
}
