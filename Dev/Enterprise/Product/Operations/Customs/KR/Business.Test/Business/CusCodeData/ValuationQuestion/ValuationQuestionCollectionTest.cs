using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ValuationQuestionCollection))]
	sealed class ValuationQuestionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			return new ValuationQuestionCollection(invoice);
		}
	}
}
