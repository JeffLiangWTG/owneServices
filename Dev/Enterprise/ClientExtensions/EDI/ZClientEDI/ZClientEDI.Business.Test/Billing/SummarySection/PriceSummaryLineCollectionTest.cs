using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceSummaryLineCollection))]
	internal class PriceSummaryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceSummaryLineCollection>
	{
		protected override PriceSummaryLineCollection GetCollectionToTest()
			=> new PriceSummaryLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new PriceSummaryLine(Factory);
	}
}
