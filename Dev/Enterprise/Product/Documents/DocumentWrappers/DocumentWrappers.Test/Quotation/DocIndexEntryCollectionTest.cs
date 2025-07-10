using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocIndexEntryCollection))]
	sealed class DocIndexEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocIndexEntryCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			RateEntry quoteEntry = Factory.New<Quote>().AddRateEntry("AIR", "LSE", "", "");
			return DocIndexEntry.New(new PricingPage(quoteEntry, quoteEntry.Factory, PricingPageStyle.Standard), Factory);
		}

		protected override DocIndexEntryCollection GetCollectionToTest()
		{
			return new DocIndexEntryCollection(Factory);
		}

		#endregion
	}
}
