using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ChargePaymentBasisCollection))]
	public class ChargePaymentBasisCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			return new ChargePaymentBasisCollection(charge);
		}
	}
}
