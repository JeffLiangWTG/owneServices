using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocAirLCLRateEntryCollection))]
	sealed class DocAirLCLRateEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocAirLCLRateEntryCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			RateEntry rateEntry = Factory.New<RateEntry>();
			return DocRateEntry.New(rateEntry, Factory);
		}

		protected override DocAirLCLRateEntryCollection GetCollectionToTest()
		{
			return new DocAirLCLRateEntryCollection(Factory);
		}

		#endregion
	}
}
