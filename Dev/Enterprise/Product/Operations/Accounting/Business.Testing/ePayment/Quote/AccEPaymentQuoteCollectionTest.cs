using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccEPaymentQuoteCollection))]
	sealed class AccEPaymentQuoteCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccEPaymentQuoteCollection(Factory);
	}
}
